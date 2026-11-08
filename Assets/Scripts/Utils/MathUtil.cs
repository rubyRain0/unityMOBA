using UnityEngine;

namespace Moba.Utils
{
    public static class MathUtil
    {
        public static Vector3 Flat(Vector3 value)
        {
            value.y = 0f;
            return value;
        }

        public static float FlatDistance(Vector3 a, Vector3 b)
        {
            return Mathf.Sqrt(FlatSqrDistance(a, b));
        }

        public static float FlatSqrDistance(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return dx * dx + dz * dz;
        }

        public static Vector3 FlatDirection(Vector3 from, Vector3 to)
        {
            Vector3 delta = Flat(to - from);
            float magnitude = delta.magnitude;
            return magnitude > 0.0001f ? delta / magnitude : Vector3.zero;
        }
    }
}
