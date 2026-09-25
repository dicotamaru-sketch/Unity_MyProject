using UnityEngine;

namespace FlowGame
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class FlowRenderer : MonoBehaviour
    {
        public Color CoolColor = new Color(0.35f, 0.55f, 0.95f);
        public Color WarmColor = new Color(1f, 0.75f, 0.35f);
        public float BaseAlpha = 0.85f;
        public float NodeDiameter = 0.62f;

        private FlowSimulation simulation;
        private Mesh mesh;

        private Vector3[] vertices;
        private Color32[] colors;
        private int[] indices;

        private SpriteRenderer[] nodeRings;
        private SpriteRenderer[] nodeArrows;
        private Transform[] nodeArrowPivots;

        public void Setup(FlowSimulation sim)
        {
            simulation = sim;

            mesh = new Mesh { name = "FlowTrails" };
            mesh.MarkDynamic();
            GetComponent<MeshFilter>().mesh = mesh;

            var mr = GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Sprites/Default"));
            mr.material = mat;
            mr.sortingOrder = 1;

            BuildNodeMarkers();
        }

        private void BuildNodeMarkers()
        {
            nodeRings = new SpriteRenderer[2];
            nodeArrows = new SpriteRenderer[2];
            nodeArrowPivots = new Transform[2];

            Sprite ringSprite = CreateRingSprite();
            Sprite arrowSprite = CreateArrowSprite();

            for (int n = 0; n < 2; n++)
            {
                GameObject ringGO = new GameObject("Node" + n + "Ring");
                ringGO.transform.SetParent(transform, false);
                var ringSr = ringGO.AddComponent<SpriteRenderer>();
                ringSr.sprite = ringSprite;
                ringSr.sortingOrder = 2;
                ringGO.transform.localScale = new Vector3(NodeDiameter, NodeDiameter, 1f);
                nodeRings[n] = ringSr;

                GameObject pivotGO = new GameObject("Node" + n + "ArrowPivot");
                pivotGO.transform.SetParent(transform, false);
                nodeArrowPivots[n] = pivotGO.transform;

                GameObject arrowGO = new GameObject("Node" + n + "Arrow");
                arrowGO.transform.SetParent(pivotGO.transform, false);
                var arrowSr = arrowGO.AddComponent<SpriteRenderer>();
                arrowSr.sprite = arrowSprite;
                arrowSr.sortingOrder = 3;
                arrowGO.transform.localPosition = new Vector3(NodeDiameter * 0.55f, 0f, 0f);
                arrowGO.transform.localScale = new Vector3(0.35f, 0.35f, 1f);
                nodeArrows[n] = arrowSr;
            }
        }

        private void LateUpdate()
        {
            if (simulation == null) return;

            UpdateTrailMesh();
            UpdateNodeMarkers();
        }

        private void UpdateTrailMesh()
        {
            int particleCount = simulation.ActiveParticleCount;
            int trailLength = simulation.ActiveTrailLength;
            int segsPerParticle = trailLength - 1;
            int vertCount = particleCount * segsPerParticle * 2;

            if (vertices == null || vertices.Length != vertCount)
            {
                vertices = new Vector3[vertCount];
                colors = new Color32[vertCount];
                indices = new int[vertCount];
                for (int k = 0; k < vertCount; k++) indices[k] = k;
            }

            int vi = 0;
            for (int p = 0; p < particleCount; p++)
            {
                float glow = simulation.GetParticleGlow(p);
                for (int s = 0; s < segsPerParticle; s++)
                {
                    Vector2 a = simulation.GetTrailPoint(p, s);
                    Vector2 b = simulation.GetTrailPoint(p, s + 1);
                    float ageA = (float)s / segsPerParticle;
                    float ageB = (float)(s + 1) / segsPerParticle;

                    vertices[vi] = a;
                    colors[vi] = TrailColor(ageA, glow);
                    vi++;
                    vertices[vi] = b;
                    colors[vi] = TrailColor(ageB, glow);
                    vi++;
                }
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.colors32 = colors;
            mesh.SetIndices(indices, MeshTopology.Lines, 0);
            mesh.RecalculateBounds();
        }

        private Color32 TrailColor(float ageT, float glow)
        {
            float alpha = Mathf.Lerp(1f, 0f, ageT) * BaseAlpha;
            Color c = Color.Lerp(CoolColor, WarmColor, glow);
            c.a = alpha;
            return c;
        }

        private void UpdateNodeMarkers()
        {
            for (int n = 0; n < 2; n++)
            {
                Vector2 pos = simulation.NodeWorldPos(n);
                Vector2 tangent = simulation.NodeTangent(n);
                float charge01 = Mathf.Clamp01(simulation.NodeCharge[n] / simulation.ChargeThreshold);

                nodeRings[n].transform.localPosition = pos;
                Color ringColor = Color.Lerp(new Color(0.25f, 0.25f, 0.32f, 0.7f), new Color(1f, 0.85f, 0.4f, 0.95f), charge01);
                nodeRings[n].color = ringColor;
                float pulse = 1f + 0.08f * Mathf.Sin(Time.time * 3f) * charge01;
                nodeRings[n].transform.localScale = new Vector3(NodeDiameter * pulse, NodeDiameter * pulse, 1f);

                nodeArrowPivots[n].localPosition = pos;
                float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
                nodeArrowPivots[n].localRotation = Quaternion.Euler(0f, 0f, angle);
                nodeArrows[n].color = new Color(1f, 1f, 1f, 0.55f + 0.25f * charge01);
            }
        }

        private static Sprite CreateRingSprite()
        {
            int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] px = new Color[size * size];
            Vector2 c = new Vector2(size / 2f, size / 2f);
            float outerR = size * 0.46f;
            float innerR = size * 0.34f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), c);
                    float a = 0f;
                    if (d <= outerR && d >= innerR)
                    {
                        float edge = Mathf.Min(outerR - d, d - innerR);
                        a = Mathf.Clamp01(edge / 2f);
                    }
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Sprite CreateArrowSprite()
        {
            int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] px = new Color[size * size];
            for (int i = 0; i < px.Length; i++) px[i] = Color.clear;

            for (int y = 0; y < size; y++)
            {
                float t = (float)y / size;
                int xStart = (int)(size * 0.1f);
                int xEnd = (int)(size * (0.1f + 0.8f * (1f - Mathf.Abs(t - 0.5f) * 2f)));
                for (int x = xStart; x < xEnd; x++)
                    px[y * size + x] = Color.white;
            }
            tex.SetPixels(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0f, 0.5f), size);
        }
    }
}
