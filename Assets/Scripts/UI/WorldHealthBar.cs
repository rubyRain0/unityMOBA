using UnityEngine;
using Moba.Core;
using Moba.Utils;

namespace Moba.UI
{
    /// <summary>
    /// Полоска здоровья над юнитом. Два квада, развёрнутых к камере.
    /// </summary>
    public class WorldHealthBar : MonoBehaviour
    {
        private const float FullWidth = 1.2f;
        private const float Height = 2.6f;

        private Unit unit;
        private Renderer backgroundRenderer;
        private Renderer fillRenderer;
        private Transform fill;

        public static WorldHealthBar Attach(Unit unit, Color color)
        {
            var go = new GameObject("HealthBar");
            go.transform.SetParent(unit.transform, false);
            go.transform.localPosition = new Vector3(0f, Height, 0f);

            var bar = go.AddComponent<WorldHealthBar>();
            bar.unit = unit;
            bar.Build(color);
            return bar;
        }

        private void Build(Color color)
        {
            GameObject background = SceneFactory.UnlitQuad(
                "BG", transform, Vector3.zero,
                new Vector3(1.34f, 0.18f, 1f),
                new Color(0.03f, 0.03f, 0.03f, 0.9f));

            GameObject fillGo = SceneFactory.UnlitQuad(
                "Fill", transform, new Vector3(0f, 0f, -0.01f),
                new Vector3(FullWidth, 0.11f, 1f),
                color);

            backgroundRenderer = background.GetComponent<Renderer>();
            fillRenderer = fillGo.GetComponent<Renderer>();
            fill = fillGo.transform;
        }

        private void LateUpdate()
        {
            if (unit == null || fill == null) return;

            Transform cam = Camera.main != null ? Camera.main.transform : null;
            if (cam != null) transform.rotation = cam.rotation;

            bool visible = unit.IsAlive;
            if (backgroundRenderer != null) backgroundRenderer.enabled = visible;
            if (fillRenderer != null) fillRenderer.enabled = visible;

            float ratio = unit.MaxHealth > 0f ? Mathf.Clamp01(unit.Health / unit.MaxHealth) : 0f;
            fill.localScale = new Vector3(FullWidth * ratio, 0.11f, 1f);
            fill.localPosition = new Vector3(-(FullWidth - FullWidth * ratio) * 0.5f, 0f, -0.01f);
        }
    }
}
