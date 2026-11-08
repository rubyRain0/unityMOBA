using UnityEngine;
using Moba.Utils;

namespace Moba.Effects
{
    /// <summary>
    /// Короткоживущий визуальный эффект: сфера, меняющая размер и исчезающая.
    /// </summary>
    public class TimedEffect : MonoBehaviour
    {
        private float lifetime;
        private float elapsed;
        private float startScale;
        private float endScale;

        public static TimedEffect Spawn(
            Vector3 position,
            Color color,
            float startScale,
            float endScale,
            float lifetime)
        {
            var go = new GameObject("TimedEffect");
            go.transform.position = position;
            return Create(go, color, startScale, endScale, lifetime);
        }

        /// <summary>Эффект, привязанный к объекту: двигается вместе с ним.</summary>
        public static TimedEffect SpawnAttached(
            Transform parent,
            Vector3 localPosition,
            Color color,
            float startScale,
            float endScale,
            float lifetime)
        {
            var go = new GameObject("TimedEffect");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            return Create(go, color, startScale, endScale, lifetime);
        }

        private static TimedEffect Create(
            GameObject go,
            Color color,
            float startScale,
            float endScale,
            float lifetime)
        {
            go.transform.localScale = Vector3.one * startScale;

            SceneFactory.Primitive(
                PrimitiveType.Sphere, "Visual", go.transform,
                Vector3.zero, Vector3.one, color, keepCollider: false);

            var effect = go.AddComponent<TimedEffect>();
            effect.startScale = startScale;
            effect.endScale = endScale;
            effect.lifetime = Mathf.Max(0.01f, lifetime);
            return effect;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / lifetime);
            transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);

            if (t >= 1f) Destroy(gameObject);
        }
    }
}
