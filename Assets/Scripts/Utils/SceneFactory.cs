using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Moba.Utils
{
    /// <summary>
    /// Фабрика примитивов и материалов. Карта, юниты, снаряды и полоски HP
    /// собираются через неё.
    /// </summary>
    public static class SceneFactory
    {
        private static readonly Dictionary<Color, Material> LitCache = new();

        public static GameObject Primitive(
            PrimitiveType type,
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Color color,
            bool keepCollider = true)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;

            Transform t = go.transform;
            t.SetParent(parent, false);
            t.localPosition = localPosition;
            t.localScale = localScale;

            if (!keepCollider)
            {
                Collider collider = go.GetComponent<Collider>();
                if (collider != null) collider.enabled = false;
            }

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = Lit(color);
            }

            return go;
        }

        public static GameObject UnlitQuad(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;

            Transform t = go.transform;
            t.SetParent(parent, false);
            t.localPosition = localPosition;
            t.localScale = localScale;

            Collider collider = go.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = Unlit(color);
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            return go;
        }

        public static Material Lit(Color color)
        {
            if (LitCache.TryGetValue(color, out Material cached)) return cached;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            Material material = new(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.15f);

            LitCache[color] = material;
            return material;
        }

        public static Material Unlit(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");

            Material material = new(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Cull")) material.SetFloat("_Cull", 0f); // двусторонний

            return material;
        }
    }
}
