using System.Collections.Generic;
using UnityEngine;
using Moba.Core;

namespace Moba.Map
{
    public struct TowerSpot
    {
        public Team Team;
        public int Lane;
        public int Tier; // 0 — внешняя башня, 1 — внутренняя
        public Vector3 Position;
    }

    /// <summary>
    /// Данные карты, которые нужны другим системам: базы, маршруты линий,
    /// точки спавна и места под башни. Создаётся MapBuilder'ом.
    /// </summary>
    public class MapLayout
    {
        public GameObject Root;
        public Vector3 BlueBase;
        public Vector3 RedBase;
        public Vector3[][] Lanes;
        public float FountainRadius;
        public readonly List<TowerSpot> Towers = new();

        public Vector3 BaseFor(Team team)
        {
            return team == Team.Red ? RedBase : BlueBase;
        }

        public Vector3[] LaneFor(int laneIndex, Team team)
        {
            return team == Team.Red ? MapPath.Reverse(Lanes[laneIndex]) : Lanes[laneIndex];
        }

        /// <summary>Точка спавна миньонов чуть впереди своей базы.</summary>
        public Vector3 SpawnPoint(int laneIndex, Team team)
        {
            return MapPath.PointAt(LaneFor(laneIndex, team), 8f);
        }

        public Vector3 HeroSpawn(Team team, int index = 0)
        {
            float sign = team == Team.Red ? -1f : 1f;
            Vector3 offset = new Vector3(
                4f + index * 1.8f,
                0f,
                4f - index * 1.8f) * sign;
            return BaseFor(team) + offset;
        }

        public Vector3 TowerPosition(int laneIndex, Team team, int tier)
        {
            for (int i = 0; i < Towers.Count; i++)
            {
                TowerSpot spot = Towers[i];
                if (spot.Lane != laneIndex || spot.Team != team || spot.Tier != tier) continue;
                return spot.Position;
            }
            return SpawnPoint(laneIndex, team);
        }
    }
}
