using UnityEngine;
using Moba.Core;

namespace Moba.CameraControl
{
    /// <summary>
    /// Камера MOBA: свободно ездит по карте (край экрана, средняя кнопка мыши),
    /// по F1 привязывается к герою, приближается колесом мыши,
    /// отдаёт точку под курсором и юнита под курсором для клик-управления.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class RTSCamera : MonoBehaviour
    {
        private const float EdgeSize = 0.01f;
        private const float EdgeScrollSpeed = 26f;
        private const float DragSensitivity = 0.35f;
        private const float ZoomMin = 0.65f;
        private const float ZoomMax = 1.45f;

        public static RTSCamera Instance { get; private set; }

        public bool FollowEnabled { get; private set; }

        private Camera cam;
        private Transform followTarget;
        private Vector3 offset;
        private Vector3 focus;
        private float zoom = 1f;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            offset = new Vector3(0f, GameConfig.CameraHeight, -GameConfig.CameraBack);
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void LateUpdate()
        {
            float dt = Time.unscaledDeltaTime;

            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                zoom = Mathf.Clamp(zoom - scroll * 0.08f, ZoomMin, ZoomMax);
            }

            if (Input.GetKeyDown(KeyCode.F1) && followTarget != null)
            {
                FollowEnabled = !FollowEnabled;
            }

            if (FollowEnabled && followTarget != null) focus = followTarget.position;
            else MoveFreely(dt);

            ApplyTransform();
        }

        private void ApplyTransform()
        {
            transform.position = focus + offset * zoom;
            transform.rotation = Quaternion.LookRotation(Vector3.up * 1.2f - offset * zoom);
        }

        private void MoveFreely(float dt)
        {
            if (Input.GetMouseButton(2))
            {
                Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
                Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                focus += (right * -Input.GetAxis("Mouse X") + forward * -Input.GetAxis("Mouse Y"))
                         * (DragSensitivity * zoom);
            }
            else
            {
                focus += EdgeDirection() * (EdgeScrollSpeed * zoom * dt);
            }

            focus.x = Mathf.Clamp(focus.x, -GameConfig.MapHalfSize, GameConfig.MapHalfSize);
            focus.z = Mathf.Clamp(focus.z, -GameConfig.MapHalfSize, GameConfig.MapHalfSize);
            focus.y = 0f;
        }

        private static Vector3 EdgeDirection()
        {
            if (!Application.isFocused) return Vector3.zero;

            Vector3 mouse = Input.mousePosition;
            Vector3 dir = Vector3.zero;

            if (mouse.x <= Screen.width * EdgeSize) dir.x -= 1f;
            else if (mouse.x >= Screen.width * (1f - EdgeSize)) dir.x += 1f;

            if (mouse.y <= Screen.height * EdgeSize) dir.z -= 1f;
            else if (mouse.y >= Screen.height * (1f - EdgeSize)) dir.z += 1f;

            return dir;
        }

        public void SetTarget(Transform target)
        {
            followTarget = target;
            if (target != null) focus = target.position;
            ApplyTransform();
        }

        public bool TryGetGroundPoint(out Vector3 point)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            var groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float enter))
            {
                point = ray.GetPoint(enter);
                return true;
            }

            point = Vector3.zero;
            return false;
        }

        public bool TryGetUnitUnderCursor(out Unit unit)
        {
            unit = null;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 500f)) return false;

            unit = hit.collider.GetComponentInParent<Unit>();
            return unit != null && unit.IsAlive;
        }

        public bool TryPickEnemyUnit(Team myTeam, out Unit unit)
        {
            if (!TryGetUnitUnderCursor(out unit)) return false;

            if (unit.Team == myTeam || unit.Team == Team.None || !unit.CanBeDamaged)
            {
                unit = null;
                return false;
            }

            return true;
        }

        /// <summary>Берёт камеру из сцены или создаёт новую.</summary>
        public static RTSCamera Create()
        {
            Camera existing = Camera.main;
            GameObject go;

            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                go = new GameObject("Main Camera") { tag = "MainCamera" };
                go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }

            Camera cameraComponent = go.GetComponent<Camera>();
            cameraComponent.fieldOfView = GameConfig.CameraFov;
            cameraComponent.farClipPlane = 500f;

            RTSCamera rts = go.GetComponent<RTSCamera>();
            if (rts == null) rts = go.AddComponent<RTSCamera>();

            return rts;
        }
    }
}
