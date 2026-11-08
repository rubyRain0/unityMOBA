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

        /// <summary>
        /// Ближайший враг в радиусе. Если задан preferredKind, сначала ищем
        /// юнита этого типа (башни бьют миньонов первыми), потом любого.
        /// </summary>
        public static Unit FindNearestEnemy(
            Team team,
            Vector3 position,
            float range,
            UnitKind? preferredKind = null)
        {
            if (preferredKind.HasValue)
            {
                Unit preferred = FindNearest(team, position, range, preferredKind.Value);
                if (preferred != null) return preferred;
            }

            return FindNearest(team, position, range, null);
        }

        private static Unit FindNearest(Team team, Vector3 position, float range, UnitKind? kind)
        {
            Unit best = null;
            float bestSqr = range * range;

            foreach (Unit unit in Units)
            {
                if (unit == null || !unit.CanBeDamaged) continue;
                if (unit.Team == Team.None || unit.Team == team) continue;
                if (kind.HasValue && unit.Kind != kind.Value) continue;

                float sqr = MathUtil.FlatSqrDistance(position, unit.transform.position);
                if (sqr >= bestSqr) continue;

                bestSqr = sqr;
                best = unit;
            }

            return best;
        }
    }
}
