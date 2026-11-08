using UnityEngine;
using Moba.CameraControl;
using Moba.Control;
using Moba.Core;
using Moba.Utils;

namespace Moba.UI
{
    /// <summary>
    /// Белая обводка на земле вокруг юнита под курсором.
    /// Если курсор ни на кого не наведён — подсвечивается текущая цель игрока.
    /// </summary>
    public class FocusHighlight : MonoBehaviour
    {
        private const int Segments = 48;
        private const float Height = 0.14f;

        private LineRenderer line;
        private PlayerController playerController;
        private Unit current;

        public static FocusHighlight Create(Unit playerHero)
        {
            var go = new GameObject("FocusHighlight");
            var highlight = go.AddComponent<FocusHighlight>();
            highlight.playerController =
                playerHero != null ? playerHero.GetComponent<PlayerController>() : null;
            highlight.Build();
            return highlight;
        }

        private void Build()
        {
            line = gameObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = Segments;
            line.startWidth = 0.09f;
            line.endWidth = 0.09f;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.material = SceneFactory.Unlit(Color.white);
            line.enabled = false;
        }

        private void Update()
        {
            Unit focus = FindFocus();

            if (focus == null || !focus.IsAlive)
            {
                current = null;
                line.enabled = false;
                return;
            }

            if (focus != current)
            {
                current = focus;
                BuildCircle(Radius(focus));
            }

            Vector3 position = focus.transform.position;
            transform.position = new Vector3(position.x, Height, position.z);
            line.enabled = true;
        }

        private Unit FindFocus()
        {
            RTSCamera cameraRig = RTSCamera.Instance;
            if (cameraRig != null && cameraRig.TryGetUnitUnderCursor(out Unit hovered))
            {
                return hovered;
            }

            return playerController != null ? playerController.CurrentTarget : null;
        }

        private void BuildCircle(float radius)
        {
            for (int i = 0; i < Segments; i++)
            {
                float angle = i / (float)Segments * Mathf.PI * 2f;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
        }

        private static float Radius(Unit unit)
        {
            if (unit.Kind == UnitKind.Structure)
            {
                return unit.IsPrimaryObjective ? 3.3f : 2.1f;
            }

            if (unit.Kind == UnitKind.Hero) return 0.9f;
            return 0.55f;
        }
    }
}
