using System.Collections.Generic;
using UnityEngine;
using Moba.Utils;

namespace Moba.Core
{
    /// <summary>
    /// Реестр всех живых юнитов. Нужен ИИ, лечению у базы и поиску цели,
    /// чтобы не сканировать сцену через FindObjects.
    /// </summary>
    public static class UnitRegistry
    {
        private static readonly HashSet<Unit> Units = new();

        public static IReadOnlyCollection<Unit> All => Units;

        public static void Clear()
        {
            Units.Clear();
        }

        public static void Register(Unit unit)
        {
            if (unit != null) Units.Add(unit);
        }

        public static void Unregister(Unit unit)
        {
            if (unit != null) Units.Remove(unit);
        }

        public static Unit FindNearestEnemy(Team team, Vector3 position, float range, bool aliveOnly = true)
        {
            Unit best = null;
            float bestSqr = range * range;

            foreach (Unit unit in Units)
            {
                if (unit == null || (aliveOnly && !unit.IsAlive)) continue;
                if (unit.Team == Team.None || unit.Team == team) continue;

                float sqr = MathUtil.FlatSqrDistance(position, unit.transform.position);
                if (sqr >= bestSqr) continue;

                bestSqr = sqr;
                best = unit;
            }

            return best;
        }
    }
}
