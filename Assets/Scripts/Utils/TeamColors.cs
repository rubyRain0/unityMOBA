using UnityEngine;
using Moba.Core;

namespace Moba.Utils
{
    public static class TeamColors
    {
        public static readonly Color Blue = new(0.22f, 0.55f, 1f);
        public static readonly Color Red = new(1f, 0.28f, 0.22f);
        public static readonly Color Player = new(0.1f, 0.95f, 0.9f);
        public static readonly Color Neutral = new(0.6f, 0.6f, 0.6f);

        public static Color Of(Team team)
        {
            return team switch
            {
                Team.Blue => Blue,
                Team.Red => Red,
                _ => Neutral
            };
        }

        public static Color Darken(Color color, float amount)
        {
            return new Color(color.r * (1f - amount), color.g * (1f - amount), color.b * (1f - amount), color.a);
        }
    }
}
