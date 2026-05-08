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
        private float cameraMinX;
        private float cameraMaxX;
        private float cameraMinY;
        private float cameraMaxY;
        private float RelativeZoomLevel { get => initialZoom / cam.orthographicSize; }
        private Vector3 initialPosition;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            CalculateCameraBounds();
            initialPosition = new Vector3(cameraMinX, cameraMinY, -10);
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
            CalculateCameraBounds();
            ClampPositionToBounds();
        }
        void CalculateCameraBounds()
        {
            float ySize = cam.orthographicSize;
            float xSize = ySize * cam.aspect;

            cameraMinX = cameraXBounds.x + xSize;
            cameraMaxX = cameraXBounds.y - xSize;
            cameraMinY = cameraYBounds.x + ySize;
            cameraMaxY = cameraYBounds.y - ySize;
        }
        /// <summary>
        /// Returns true if the given position is inside bounds defined for CameraController and false otherwise.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private bool IsPositionInBounds(Vector3 position)
        {
            if (position.x < cameraMinX || position.x > cameraMaxX) { return false; }
            if (position.y < cameraMinY || position.y > cameraMaxY) { return false; }
            return true;
        }
        private Vector3 ClampPositionToBounds(Vector3 position)
        {
            position.x = Mathf.Clamp(position.x, cameraMinX, cameraMaxX);
            position.y = Mathf.Clamp(position.y, cameraMinY, cameraMaxY);
            return position;
        }
        private void ClampPositionToBounds()
        {
            transform.position = ClampPositionToBounds(transform.position);
        }
        private void HandleMouseInput()
        {
            HandleTranslationInput();
            HandleZoomInput();
        }
        private void HandleTranslationInput()
        {
            if (Mouse.current.middleButton.isPressed)
            {
                Vector2 translation = -Mouse.current.delta.ReadValue();
                TranslateCamera(translation);
            }
        }
        private void HandleZoomInput()
        {
            if (Mouse.current.scroll.y.value != 0)
            {
                float zoomInput = -Mouse.current.scroll.y.value * mouseZoomSpeed;
                cam.orthographicSize = Mathf.Clamp(zoomInput + cam.orthographicSize, minSize, maxSize);
                CalculateCameraBounds();
                ClampPositionToBounds();
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
        private void TranslateCamera(Vector2 translation)
        {
            translation *= mouseDraggingSpeed * Time.fixedDeltaTime / RelativeZoomLevel;
            TeleportCameraToPosition(transform.position + (Vector3)translation);
        }

        private void HandleOtherInputs()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                ResetPosition();
            }
        }
    }
}
