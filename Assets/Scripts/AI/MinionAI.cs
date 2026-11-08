using UnityEngine;
using Moba.Combat;
using Moba.Core;
using Moba.Map;
using Moba.Navigation;

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
        private LaneRoute laneRoute;
        private Unit target;
        private float nextScanTime;
        private float nextRepathTime;

        public void Initialize(Unit owner, NavMover navMover, Attacker unitAttacker, Vector3[] routePath)
        {
            self = owner;
            mover = navMover;
            attacker = unitAttacker;
            laneRoute = new LaneRoute(routePath);
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

            if (target != null && target.CanBeDamaged)
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
            if (laneRoute == null) return;

            Vector3 destination = laneRoute.Next(transform.position, WaypointReached);

            if (Time.time >= nextRepathTime)
            {
                nextRepathTime = Time.time + RepathInterval;
                mover.MoveTo(destination);
            }
        }
    }
}
