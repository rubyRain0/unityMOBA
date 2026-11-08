using UnityEngine;
using Moba.Core;
using Moba.Utils;

namespace Moba.Map
{
    /// <summary>
    /// Лечение союзников около своей базы. Работает по реестру юнитов,
    /// тикает раз в полсекунды, чтобы не считать дистанции каждый кадр.
    /// </summary>
    public class HealZone : MonoBehaviour
    {
        private const float TickInterval = 0.5f;

        private Team team;
        private float radius;
        private float healPerSecond;
        private float timer;

        public void Initialize(Team zoneTeam, float zoneRadius, float healAmountPerSecond)
        {
            team = zoneTeam;
            radius = zoneRadius;
            healPerSecond = healAmountPerSecond;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer < TickInterval) return;

            float amount = healPerSecond * TickInterval;
            timer = 0f;

            foreach (Unit unit in UnitRegistry.All)
            {
                if (unit == null || !unit.IsAlive || unit.Team != team) continue;
                if (unit.Kind == UnitKind.Structure) continue;
                if (MathUtil.FlatDistance(unit.transform.position, transform.position) > radius) continue;

                unit.Heal(amount);
            }
        }
    }
}
