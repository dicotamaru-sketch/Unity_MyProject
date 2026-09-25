using UnityEngine;

namespace FlowGame
{
    public class FlowBootstrap : MonoBehaviour
    {
        [SerializeField] private int seed = 19027;
        [SerializeField] private int particleCount = 160;
        [SerializeField] private Color backgroundColor = new Color(0.035f, 0.04f, 0.06f, 1f);

        private void Awake()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = backgroundColor;
            }

            var simulation = gameObject.AddComponent<FlowSimulation>();
            simulation.Seed = seed;
            simulation.ParticleCount = particleCount;
            simulation.Setup();

            GameObject trailGO = new GameObject("FlowTrails");
            trailGO.transform.SetParent(transform, false);
            var flowRenderer = trailGO.AddComponent<FlowRenderer>();
            flowRenderer.Setup(simulation);

            var input = gameObject.AddComponent<FlowInputController>();
            input.Setup(simulation, cam);

            GameObject uiGO = new GameObject("FlowUI");
            uiGO.transform.SetParent(transform, false);
            var ui = uiGO.AddComponent<FlowUIController>();
            ui.Setup(simulation);
        }
    }
}
