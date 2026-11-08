using UnityEngine;
using Moba.Combat;
using Moba.Core;
using Moba.Navigation;
using Moba.Utils;

namespace Moba.AI
{
    /// <summary>
    /// Поведение миньона: идёт по маршруту линии, при обнаружении врага
    /// атакует его, а после победы возвращается на маршрут.
    /// </summary>
    public class MinionAI : MonoBehaviour
    {
        private const float RescanInterval = 0.2f;
        private const float RepathInterval = 0.5f;
        private const float ChaseRepathInterval = 0.25f;
        private const float WaypointReached = 1.5f;

        private Unit self;
        private NavMover mover;
        private Attacker attacker;
        private Vector3[] path;
        private int waypointIndex;
        private Unit target;
        private float nextScanTime;
        private float nextRepathTime;

        public void Initialize(Unit owner, NavMover navMover, Attacker unitAttacker, Vector3[] route)
        {
            self = owner;
            mover = navMover;
            attacker = unitAttacker;
            path = route;
            waypointIndex = path != null && path.Length > 1 ? 1 : 0;
        }

        private void Update()
        {
            if (self == null || !self.IsAlive) return;

            if (Time.time >= nextScanTime)
            {
                nextScanTime = Time.time + RescanInterval;
                target = UnitRegistry.FindNearestEnemy(
                    self.Team, transform.position, GameConfig.MinionAggroRange);
            }

            if (target != null && target.IsAlive)
            {
                if (attacker.CanReach(target))
                {
                    mover.Stop();
                    attacker.TryAttack(target);
                    return;
                }

                if (Time.time >= nextRepathTime)
                {
                    nextRepathTime = Time.time + ChaseRepathInterval;
                    mover.MoveTo(target.transform.position);
                }
                return;
            }

            target = null;
            AdvanceRoute();
        }

        private void AdvanceRoute()
        {
            if (path == null || path.Length == 0) return;

            int last = path.Length - 1;
            if (waypointIndex > last) waypointIndex = last;

            if (MathUtil.FlatDistance(transform.position, path[waypointIndex]) <= WaypointReached)
            {
                waypointIndex = Mathf.Min(waypointIndex + 1, last);
            }

            if (Time.time >= nextRepathTime)
            {
                nextRepathTime = Time.time + RepathInterval;
                mover.MoveTo(path[waypointIndex]);
            }
        }
    }
}
