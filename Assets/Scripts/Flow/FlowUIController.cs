using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace FlowGame
{
    public class FlowUIController : MonoBehaviour
    {
        private FlowSimulation simulation;
        private Text guidanceText;
        private Text strokesText;
        private Text stateText;

        public void Setup(FlowSimulation sim)
        {
            simulation = sim;
            BuildCanvas();
        }

        private void BuildCanvas()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<InputSystemUIInputModule>();
            }

            GameObject canvasGO = new GameObject("FlowCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960, 600);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            guidanceText = CreateText(canvasGO.transform, "Guidance",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -20f), new Vector2(-400f, 60f),
                24, TextAnchor.UpperCenter, new Color(1f, 1f, 1f, 0.85f));

            strokesText = CreateText(canvasGO.transform, "Strokes",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -20f), new Vector2(300f, 40f),
                22, TextAnchor.UpperLeft, new Color(1f, 1f, 1f, 0.85f));

            stateText = CreateText(canvasGO.transform, "StateMessage",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(700f, 60f),
                34, TextAnchor.MiddleCenter, Color.white);

            CreateRestartButton(canvasGO.transform);
        }

        private Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, int fontSize, TextAnchor align, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Text t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize;
            t.alignment = align;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            return t;
        }

        private void CreateRestartButton(Transform parent)
        {
            GameObject btnGO = new GameObject("RestartButton");
            btnGO.transform.SetParent(parent, false);
            RectTransform rt = btnGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-90f, -30f);
            rt.sizeDelta = new Vector2(140f, 48f);

            var img = btnGO.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.12f);

            var btn = btnGO.AddComponent<Button>();
            btn.onClick.AddListener(() => simulation.ResetSimulation());

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            RectTransform trt = textGO.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            Text btnText = textGO.AddComponent<Text>();
            btnText.text = "やり直す";
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.fontSize = 20;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;
        }

        private void Update()
        {
            if (simulation == null) return;

            strokesText.text = "のこり " + Mathf.Max(0, simulation.StrokesRemaining) + " 筆";

            switch (simulation.State)
            {
                case FlowSimulation.FlowState.Playing:
                    stateText.text = "";
                    guidanceText.text = "指で流れをなぞり、暗い節を粒子の進む向きへ結ぶ。";
                    break;
                case FlowSimulation.FlowState.Settling:
                    stateText.text = "安定させています…";
                    guidanceText.text = "";
                    break;
                case FlowSimulation.FlowState.Cleared:
                    stateText.text = "つながった。";
                    guidanceText.text = "";
                    break;
                case FlowSimulation.FlowState.Failed:
                    stateText.text = "つながらなかった。";
                    guidanceText.text = "右上の「やり直す」でもう一度。";
                    break;
            }
        }
    }
}
