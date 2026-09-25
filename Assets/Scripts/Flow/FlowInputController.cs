using UnityEngine;
using UnityEngine.InputSystem;

namespace FlowGame
{
    public class FlowInputController : MonoBehaviour
    {
        public float MinStrokeDistance = 0.4f;

        private FlowSimulation simulation;
        private Camera cam;

        private bool isDragging;
        private float dragPathLength;
        private Vector2 lastWorldPos;
        private float lastSampleTime;

        public bool IsDragging => isDragging;
        public Vector2 DragWorldPos => lastWorldPos;
        public Vector2 DragWorldVelocity { get; private set; }

        public void Setup(FlowSimulation sim, Camera camera)
        {
            simulation = sim;
            cam = camera;
        }

        private void Update()
        {
            if (simulation == null || cam == null) return;

            Vector2? screenPos = null;
            bool pressedThisFrame = false;
            bool releasedThisFrame = false;

            var touch = Touchscreen.current;
            var mouse = Mouse.current;

            if (touch != null && (touch.primaryTouch.press.isPressed || touch.primaryTouch.press.wasReleasedThisFrame))
            {
                screenPos = touch.primaryTouch.position.ReadValue();
                pressedThisFrame = touch.primaryTouch.press.wasPressedThisFrame;
                releasedThisFrame = touch.primaryTouch.press.wasReleasedThisFrame;
            }
            else if (mouse != null && (mouse.leftButton.isPressed || mouse.leftButton.wasReleasedThisFrame))
            {
                screenPos = mouse.position.ReadValue();
                pressedThisFrame = mouse.leftButton.wasPressedThisFrame;
                releasedThisFrame = mouse.leftButton.wasReleasedThisFrame;
            }

            if (!screenPos.HasValue) return;

            Vector2 worldPos = ScreenToWorld(screenPos.Value);

            if (pressedThisFrame && !isDragging)
            {
                isDragging = true;
                dragPathLength = 0f;
                lastWorldPos = worldPos;
                lastSampleTime = Time.unscaledTime;
                simulation.BeginDrag();
            }
            else if (isDragging)
            {
                float dt = Mathf.Max(Time.unscaledTime - lastSampleTime, 0.0001f);
                Vector2 delta = worldPos - lastWorldPos;
                dragPathLength += delta.magnitude;
                Vector2 velocity = delta / dt;

                simulation.ApplyDragSample(worldPos, velocity, Time.deltaTime);

                DragWorldVelocity = Vector2.Lerp(DragWorldVelocity, velocity, 0.5f);
                lastWorldPos = worldPos;
                lastSampleTime = Time.unscaledTime;
            }

            if (releasedThisFrame && isDragging)
            {
                isDragging = false;
                DragWorldVelocity = Vector2.zero;
                bool validStroke = dragPathLength >= MinStrokeDistance;
                simulation.EndDrag(validStroke);
            }
        }

        private Vector3 ScreenToWorld(Vector2 screenPos)
        {
            float distance = -cam.transform.position.z;
            Vector3 sp = new Vector3(screenPos.x, screenPos.y, distance);
            return cam.ScreenToWorldPoint(sp);
        }
    }
}
