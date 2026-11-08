using UnityEngine;
using Moba.Core;

namespace Moba.CameraControl
{
    /// <summary>
    /// Камера MOBA: плавно следует за героем, приближается колесом мыши,
    /// отдаёт точку под курсором и юнита под курсором для клик-управления.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class RTSCamera : MonoBehaviour
    {
        public static RTSCamera Instance { get; private set; }

        private Camera cam;
        private Transform followTarget;
        private Vector3 offset;
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
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                zoom = Mathf.Clamp(zoom - scroll * 0.08f, 0.65f, 1.45f);
            }

            if (followTarget == null) return;

            Vector3 desired = followTarget.position + offset * zoom;
            float t = 1f - Mathf.Exp(-10f * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, t);

            Vector3 lookPoint = followTarget.position + Vector3.up * 1.2f;
            transform.rotation = Quaternion.LookRotation(lookPoint - transform.position);
        }

        public void Follow(Transform target)
        {
            followTarget = target;
            SnapToTarget();
        }

        private void SnapToTarget()
        {
            if (followTarget == null) return;

            transform.position = followTarget.position + offset * zoom;
            Vector3 lookPoint = followTarget.position + Vector3.up * 1.2f;
            transform.rotation = Quaternion.LookRotation(lookPoint - transform.position);
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

        public bool TryPickEnemyUnit(Team myTeam, out Unit unit)
        {
            unit = null;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 500f)) return false;

            Unit candidate = hit.collider.GetComponentInParent<Unit>();
            if (candidate == null || !candidate.CanBeDamaged) return false;
            if (candidate.Team == myTeam || candidate.Team == Team.None) return false;

            unit = candidate;
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
