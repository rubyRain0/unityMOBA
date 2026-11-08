using System.Collections.Generic;
using UnityEngine;

namespace Moba.Map
{
    /// <summary>
    /// Утилиты для полилиний линий (лейнов). Используются при построении карты,
    /// расстановке башен и движении миньонов по маршруту.
    /// </summary>
    public static class MapPath
    {
        public static float Length(Vector3[] path)
        {
            float total = 0f;
            for (int i = 1; i < path.Length; i++)
            {
                total += Vector3.Distance(path[i - 1], path[i]);
            }
            return total;
        }

        public static Vector3 PointAt(Vector3[] path, float distance)
        {
            if (path.Length == 0) return Vector3.zero;
            if (path.Length == 1 || distance <= 0f) return path[0];

            float remaining = distance;
            for (int i = 1; i < path.Length; i++)
            {
                Vector3 a = path[i - 1];
                Vector3 b = path[i];
                float segment = Vector3.Distance(a, b);
                if (segment <= 0.0001f) continue;

                if (remaining <= segment)
                {
                    return Vector3.Lerp(a, b, remaining / segment);
                }

                remaining -= segment;
            }

            return path[path.Length - 1];
        }

        public static Vector3 DirectionAt(Vector3[] path, float distance)
        {
            if (path.Length < 2) return Vector3.forward;

            float remaining = Mathf.Max(0f, distance);
            for (int i = 1; i < path.Length; i++)
            {
                Vector3 a = path[i - 1];
                Vector3 b = path[i];
                Vector3 delta = b - a;
                float segment = delta.magnitude;
                if (segment <= 0.0001f) continue;

                if (remaining <= segment)
                {
                    return delta / segment;
                }

                remaining -= segment;
            }

            Vector3 lastA = path[path.Length - 2];
            Vector3 lastB = path[path.Length - 1];
            return (lastB - lastA).normalized;
        }

        public static Vector3 Perpendicular(Vector3 direction)
        {
            return new Vector3(-direction.z, 0f, direction.x);
        }

        public static Vector3[] Reverse(Vector3[] path)
        {
            var reversed = new Vector3[path.Length];
            for (int i = 0; i < path.Length; i++)
            {
                reversed[i] = path[path.Length - 1 - i];
            }
            return reversed;
        }

        /// <summary>
        /// Отрезает начало линии до указанной дистанции: первая точка —
        /// позиция спавна, дальше идут только точки впереди.
        /// </summary>
        public static Vector3[] Trim(Vector3[] path, float distance)
        {
            var result = new List<Vector3> { PointAt(path, distance) };
            float remaining = Mathf.Max(0f, distance);

            for (int i = 1; i < path.Length; i++)
            {
                float segment = Vector3.Distance(path[i - 1], path[i]);
                if (segment <= 0.0001f) continue;

                if (remaining < segment)
                {
                    for (int j = i; j < path.Length; j++)
                    {
                        result.Add(path[j]);
                    }
                    break;
                }

                remaining -= segment;
            }

            return result.ToArray();
        }
    }
}
