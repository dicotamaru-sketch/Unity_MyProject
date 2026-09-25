using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FlowGame
{
    public class FlowBootstrap : MonoBehaviour
    {
        private const int TrailLayer = 9;

        [SerializeField] private int seed = 19027;
        [SerializeField] private int particleCount = 160;
        [SerializeField] private Color backgroundColor = new Color(0.035f, 0.04f, 0.06f, 1f);

        [Header("ポストプロセス")]
        [SerializeField] private float bloomThreshold = 0.9f;
        [SerializeField] private float bloomIntensity = 1.2f;
        [SerializeField] private float bloomScatter = 0.75f;

        private void Awake()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = backgroundColor;
                SetupPostProcessing(cam);
            }

            var simulation = gameObject.AddComponent<FlowSimulation>();
            simulation.Seed = seed;
            simulation.ParticleCount = particleCount;
            simulation.Setup();

            GameObject trailGO = new GameObject("FlowTrails");
            trailGO.layer = TrailLayer;
            trailGO.transform.SetParent(transform, false);
            var flowRenderer = trailGO.AddComponent<FlowRenderer>();
            flowRenderer.Setup(simulation);

            var input = gameObject.AddComponent<FlowInputController>();
            input.Setup(simulation, cam);

            if (cam != null)
            {
                var feedback = gameObject.AddComponent<FlowFeedback>();
                feedback.Setup(simulation, input, cam, TrailLayer);
            }

            GameObject uiGO = new GameObject("FlowUI");
            uiGO.transform.SetParent(transform, false);
            var ui = uiGO.AddComponent<FlowUIController>();
            ui.Setup(simulation);
        }

        private void SetupPostProcessing(Camera cam)
        {
            cam.allowHDR = true;
            var data = cam.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;

            GameObject volGO = new GameObject("FlowVolume");
            volGO.transform.SetParent(transform, false);
            var volume = volGO.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(bloomThreshold);
            bloom.intensity.Override(bloomIntensity);
            bloom.scatter.Override(bloomScatter);

            var tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.Override(TonemappingMode.ACES);

            volume.profile = profile;
        }
    }
}
