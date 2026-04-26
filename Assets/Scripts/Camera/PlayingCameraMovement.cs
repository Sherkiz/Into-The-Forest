using UnityEngine;
using UnityEngine.InputSystem;

namespace ITF.CameraControl
{
    public class CameraControllerSkillTree : MonoBehaviour
    {
        [SerializeField] private float mouseDraggingSpeed = 1f;
        [SerializeField] private float mouseZoomSpeed = 1f;
        [SerializeField] private Vector2 cameraXBounds;
        [SerializeField] private Vector2 cameraYBounds;
        [SerializeField] private float minSize;
        [SerializeField] private float maxSize;
        private Camera cam;
        private readonly float initialZoom = 5f;
        private float RelativeZoomLevel { get => initialZoom / cam.orthographicSize; }
        private Vector3 initialPosition;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            initialPosition = new Vector3(cameraXBounds.x, cameraYBounds.x, -10);
        }
        private void Start()
        {
            ResetPosition();
        }
        private void LateUpdate()
        {
            HandleMouseInput();
            HandleOtherInputs();
        }
        public void ResetPosition()
        {
            transform.position = initialPosition;
            cam.orthographicSize = initialZoom;
        }
        /// <summary>
        /// Returns true if the given position is inside bounds defined for CameraController and false otherwise.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private bool IsPositionInBounds(Vector3 position)
        {
            if (position.x < cameraXBounds.x || position.x > cameraXBounds.y) { return false; }
            if (position.y < cameraYBounds.x || position.y > cameraYBounds.y) { return false; }
            return true;
        }
        private Vector3 ClampPositionToBounds(Vector3 position)
        {
            position.x = Mathf.Clamp(position.x, cameraXBounds.x, cameraXBounds.y);
            position.y = Mathf.Clamp(position.y, cameraYBounds.x, cameraYBounds.y);
            return position;
        }
        private void HandleMouseInput()
        {
            if (Mouse.current.middleButton.isPressed)
            {
                Vector2 translation = -Mouse.current.delta.ReadValue();
                translation *= mouseDraggingSpeed * Time.fixedDeltaTime / RelativeZoomLevel;
                TranslateCamera(translation);
            }
            if (Mouse.current.scroll.y.value != 0)
            {
                float zoomInput = -Mouse.current.scroll.y.value * mouseZoomSpeed;
                cam.orthographicSize = Mathf.Clamp(zoomInput + cam.orthographicSize, minSize, maxSize);
            }
        }
        private void TeleportCameraToPosition(Vector2 position)
        {
            TeleportCameraToPosition(new Vector3(position.x, position.y, initialPosition.z));
        }
        private void TeleportCameraToPosition(Vector3 position)
        {
            transform.position = ClampPositionToBounds(position);
        }
        private void TranslateCamera(Vector2 translation) => TeleportCameraToPosition(transform.position + (Vector3)translation);
        private void HandleOtherInputs()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                ResetPosition();
            }
        }
    }
}
