using UnityEngine;
using Moba.Abilities;
using Moba.Combat;
using Moba.Core;
using Moba.Map;
using Moba.Navigation;
using Moba.Units;
using Moba.Utils;

namespace Moba.AI
{
    /// <summary>
    /// ИИ героя: держит линию, фокусит врагов, отступает лечиться на базу,
    /// использует снаряд по героям и лечение при низком HP.
    /// </summary>
    public class HeroBotAI : MonoBehaviour
    {
        private const float ScanInterval = 0.2f;
        private const float WaypointReached = 2f;

        private Hero self;
        private NavMover mover;
        private Attacker attacker;
        private AbilitySystem abilities;
        private LaneRoute laneRoute;
        private Vector3 basePosition;
        private Unit target;
        private float nextScanTime;
        private float nextRepathTime;
        private bool retreating;

        public void Initialize(
            Hero owner,
            NavMover navMover,
            Attacker unitAttacker,
            AbilitySystem abilitySystem,
            MapLayout layout,
            int lane)
        {
            self = owner;
            mover = navMover;
            attacker = unitAttacker;
            abilities = abilitySystem;
            basePosition = layout.BaseFor(owner.Team);
            laneRoute = new LaneRoute(
                MapPath.Trim(layout.LaneFor(lane, owner.Team), GameConfig.MinionSpawnDistance));
        }

        private void Update()
        {
            if (self == null || !self.IsAlive) return;

            float healthRatio = self.Health / self.MaxHealth;

            if (abilities != null && healthRatio < 0.55f && abilities.IsReady(2))
            {
                abilities.TryCast(2, self.transform.position);
            }

            if (!retreating && healthRatio < GameConfig.BotRetreatHealthRatio) retreating = true;
            else if (retreating && healthRatio > GameConfig.BotRecoverHealthRatio) retreating = false;

            if (retreating)
            {
                Retreat();
                return;
            }

            if (Time.time >= nextScanTime)
            {
                nextScanTime = Time.time + ScanInterval;
                target = SelectTarget();
            }

            if (target != null && target.CanBeDamaged)
            {
                TryCastQ();

                if (attacker.CanReach(target))
                {
                    mover.Stop();
                    attacker.TryAttack(target);
                    return;
                }

                MoveTo(target.transform.position);
                return;
            }

            target = null;
            if (laneRoute == null) return;
            MoveTo(laneRoute.Next(transform.position, WaypointReached));
        }

        private Unit SelectTarget()
        {
            Unit enemyHero = UnitRegistry.FindNearestEnemy(
                self.Team, transform.position, GameConfig.BotAggroRange, UnitKind.Hero);
            if (enemyHero != null) return enemyHero;

            return UnitRegistry.FindNearestEnemy(
                self.Team, transform.position, GameConfig.BotAggroRange);
        }

        private void TryCastQ()
        {
            if (abilities == null || !abilities.IsReady(0)) return;
            if (target == null || target.Kind != UnitKind.Hero) return;
            if (MathUtil.FlatDistance(transform.position, target.transform.position) > GameConfig.SkillshotRange) return;

            abilities.TryCast(0, target.transform.position);
        }

        private void Retreat()
        {
            if (abilities != null && abilities.IsReady(1) &&
                MathUtil.FlatDistance(transform.position, basePosition) > 10f)
            {
                abilities.TryCast(1, basePosition);
            }

            if (MathUtil.FlatDistance(transform.position, basePosition) <= GameConfig.FountainRadius * 0.7f)
            {
                mover.Stop();
                return;
            }

            MoveTo(basePosition);
        }

        private void MoveTo(Vector3 point)
        {
            if (Time.time < nextRepathTime) return;
            nextRepathTime = Time.time + GameConfig.BotRepathInterval;
            mover.MoveTo(point);
        }
    }
}
