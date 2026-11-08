using UnityEngine;
using Moba.Combat;
using Moba.Core;

namespace Moba.AI
{
    /// <summary>
    /// Поведение башни: сначала стреляет по миньонам, если их нет — по героям.
    /// </summary>
    public class TurretAI : MonoBehaviour
    {
        private const float RescanInterval = 0.2f;

        private Unit self;
        private Attacker attacker;
        private Unit target;
        private float nextScanTime;

        public void Initialize(Unit owner, Attacker unitAttacker)
        {
            self = owner;
            attacker = unitAttacker;
        }

        private void Update()
        {
            if (self == null || !self.IsAlive) return;

            if (Time.time >= nextScanTime)
            {
                nextScanTime = Time.time + RescanInterval;
                target = UnitRegistry.FindNearestEnemy(
                    self.Team, transform.position, attacker.Range, UnitKind.Minion);
            }

            if (target != null && attacker.CanReach(target)) attacker.TryAttack(target);
        }
    }
}
