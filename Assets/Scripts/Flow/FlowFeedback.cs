using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FlowGame
{
    public class FlowFeedback : MonoBehaviour
    {
        [Header("減衰（1秒後に残る割合）")]
        public float DecayPlaying = 0.35f;
        public float DecaySettling = 0.6f;
        public float DecayFailed = 0.02f;

        [Header("流れ場")]
        public float AdvectScale = 1f;
        public float RingWidth = 0.9f;
        public float DragRadius = 1.2f;
        public float DragStrength = 0.8f;
        public float Spread = 0.6f;

        [Header("表示")]
        public float DisplayIntensity = 1.0f;
        public float InputWeight = 1.0f;

        private FlowSimulation simulation;
        private FlowInputController input;
        private Camera mainCam;
        private Camera trailCam;
        private int trailLayer;

        private RenderTexture accumA;
        private RenderTexture accumB;
        private RenderTexture trailRT;
        private Material feedbackMat;
        private Material displayMat;
        private Transform displayQuad;

        private FlowSimulation.FlowState lastState;

        public void Setup(FlowSimulation sim, FlowInputController inputController, Camera camera, int layer)
        {
            simulation = sim;
            input = inputController;
            mainCam = camera;
            trailLayer = layer;

            feedbackMat = new Material(Shader.Find("Hidden/Flow/Feedback"));
            displayMat = new Material(Shader.Find("Flow/Additive"));

            BuildTrailCamera();
            BuildDisplayQuad();
            EnsureRenderTextures();
            lastState = simulation.State;
        }

        private void BuildTrailCamera()
        {
            GameObject go = new GameObject("FlowTrailCamera");
            go.transform.SetParent(mainCam.transform, false);
            trailCam = go.AddComponent<Camera>();
            trailCam.orthographic = true;
            trailCam.clearFlags = CameraClearFlags.SolidColor;
            trailCam.backgroundColor = new Color(0f, 0f, 0f, 0f);
            trailCam.cullingMask = 1 << trailLayer;
            trailCam.depth = mainCam.depth - 1;
            trailCam.allowHDR = true;
            trailCam.nearClipPlane = mainCam.nearClipPlane;
            trailCam.farClipPlane = mainCam.farClipPlane;

            var data = trailCam.GetUniversalAdditionalCameraData();
            data.renderType = CameraRenderType.Base;
            data.renderPostProcessing = false;
        }

        private void BuildDisplayQuad()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "FlowAccumDisplay";
            Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(mainCam.transform, false);
            go.transform.localPosition = new Vector3(0f, 0f, 5f);
            go.transform.localRotation = Quaternion.identity;

            var mr = go.GetComponent<MeshRenderer>();
            mr.material = displayMat;
            mr.sortingOrder = 0;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            displayQuad = go.transform;
        }

        private void EnsureRenderTextures()
        {
            int w = Mathf.Max(8, Screen.width);
            int h = Mathf.Max(8, Screen.height);
            if (accumA != null && accumA.width == w && accumA.height == h) return;

            ReleaseRenderTextures();
            accumA = CreateRT(w, h);
            accumB = CreateRT(w, h);
            trailRT = CreateRT(w, h);
            ClearAccum();

            trailCam.targetTexture = trailRT;
            feedbackMat.SetTexture("_TrailTex", trailRT);
            displayMat.SetTexture("_MainTex", accumA);
        }

        private static RenderTexture CreateRT(int w, int h)
        {
            var rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGBHalf)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            rt.Create();
            return rt;
        }

        private void ReleaseRenderTextures()
        {
            if (accumA != null) accumA.Release();
            if (accumB != null) accumB.Release();
            if (trailRT != null) trailRT.Release();
            accumA = accumB = trailRT = null;
        }

        private void ClearAccum()
        {
            var prev = RenderTexture.active;
            RenderTexture.active = accumA;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = accumB;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = prev;
        }

        private void OnDestroy()
        {
            ReleaseRenderTextures();
        }

        private void LateUpdate()
        {
            if (simulation == null || mainCam == null) return;

            EnsureRenderTextures();
            SyncTrailCamera();

            var state = simulation.State;
            bool restarted = state == FlowSimulation.FlowState.Playing && lastState != FlowSimulation.FlowState.Playing
                             && lastState != FlowSimulation.FlowState.Settling;
            if (restarted) ClearAccum();
            lastState = state;

            float decayPerSec;
            float inputWeight;
            float advect;
            switch (state)
            {
                case FlowSimulation.FlowState.Settling:
                    decayPerSec = DecaySettling; inputWeight = InputWeight; advect = AdvectScale * 0.5f;
                    break;
                case FlowSimulation.FlowState.Cleared:
                    decayPerSec = 1f; inputWeight = 0f; advect = 0f;
                    break;
                case FlowSimulation.FlowState.Failed:
                    decayPerSec = DecayFailed; inputWeight = InputWeight * 0.3f; advect = AdvectScale * 1.5f;
                    break;
                default:
                    decayPerSec = DecayPlaying; inputWeight = InputWeight; advect = AdvectScale;
                    break;
            }

            float dt = Mathf.Min(Time.deltaTime, 1f / 20f);
            float halfH = mainCam.orthographicSize;
            float halfW = halfH * mainCam.aspect;

            feedbackMat.SetFloat("_Decay", Mathf.Pow(decayPerSec, dt));
            feedbackMat.SetFloat("_InputWeight", inputWeight);
            feedbackMat.SetFloat("_Dt", dt);
            feedbackMat.SetFloat("_AdvectScale", advect);
            feedbackMat.SetVector("_CamCenter", mainCam.transform.position);
            feedbackMat.SetVector("_CamHalfSize", new Vector4(halfW, halfH, 0f, 0f));
            feedbackMat.SetVector("_Ring", new Vector4(
                simulation.RingBaseRadius, simulation.RingWobbleAmplitude,
                simulation.RingWobbleFrequency, simulation.BaseAngularSpeed));
            feedbackMat.SetFloat("_RingWidth", RingWidth);
            feedbackMat.SetFloat("_Spread", Spread);
            feedbackMat.SetFloat("_DragRadius", DragRadius);
            feedbackMat.SetFloat("_DragStrength", input != null && input.IsDragging ? DragStrength : 0f);
            Vector2 dp = input != null ? input.DragWorldPos : Vector2.zero;
            Vector2 dv = input != null ? input.DragWorldVelocity : Vector2.zero;
            feedbackMat.SetVector("_Drag", new Vector4(dp.x, dp.y, dv.x, dv.y));

            Graphics.Blit(accumA, accumB, feedbackMat);
            var tmp = accumA;
            accumA = accumB;
            accumB = tmp;

            displayMat.SetTexture("_MainTex", accumA);
            displayMat.SetFloat("_Intensity", DisplayIntensity);
            displayQuad.localScale = new Vector3(halfW * 2f, halfH * 2f, 1f);
        }

        private void SyncTrailCamera()
        {
            trailCam.orthographicSize = mainCam.orthographicSize;
            trailCam.transform.localPosition = Vector3.zero;
            trailCam.transform.localRotation = Quaternion.identity;
        }
    }
}
