using UnityEngine;
using Moba.Utils;

namespace Moba.Map
{
    /// <summary>
    /// Следование по полилинии линии: хранит текущий waypoint и продвигает его
    /// при достижении. Общий для миньонов и ботов-героев.
    /// </summary>
    public class LaneRoute
    {
        private readonly Vector3[] path;
        private int index;

        public LaneRoute(Vector3[] path)
        {
            this.path = path;
            index = path != null && path.Length > 1 ? 1 : 0;
        }

        public Vector3 Next(Vector3 position, float threshold)
        {
            if (path == null || path.Length == 0) return position;

            int last = path.Length - 1;
            index = Mathf.Clamp(index, 0, last);

            if (MathUtil.FlatDistance(position, path[index]) <= threshold)
            {
                index = Mathf.Min(index + 1, last);
            }

            return path[index];
        }
    }
}
