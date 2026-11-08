using UnityEngine;
using Moba.CameraControl;
using Moba.Combat;
using Moba.Core;
using Moba.Navigation;
using Moba.Units;

namespace Moba.Control
{
    /// <summary>
    /// Управление героем игрока: ПКМ по земле — идти, ПКМ по врагу — преследовать
    /// и атаковать. Держит текущую цель и обновляет маршрут.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        private const float RepathInterval = 0.2f;

        private Hero hero;
        private NavMover mover;
        private Attacker attacker;
        private RTSCamera cameraRig;
        private Unit target;
        private float nextRepathTime;

        public Unit CurrentTarget => target;

        public void Setup(Hero controlledHero, NavMover navMover, Attacker unitAttacker, RTSCamera rtsCamera)
        {
            hero = controlledHero;
            mover = navMover;
            attacker = unitAttacker;
            cameraRig = rtsCamera;
        }

        private void Update()
        {
            if (hero == null || !hero.IsAlive) return;

            if (Input.GetMouseButtonDown(1)) IssueCommand();
            UpdateCombat();
        }

        private void IssueCommand()
        {
            if (cameraRig.TryPickEnemyUnit(hero.Team, out Unit enemy))
            {
                target = enemy;
                return;
            }

            target = null;
            if (cameraRig.TryGetGroundPoint(out Vector3 point)) mover.MoveTo(point);
        }

        private void UpdateCombat()
        {
            if (target == null || !target.IsAlive)
            {
                target = null;
                return;
            }

            if (attacker.CanReach(target))
            {
                mover.Stop();
                attacker.TryAttack(target);
                return;
            }

            if (Time.time >= nextRepathTime)
            {
                nextRepathTime = Time.time + RepathInterval;
                mover.MoveTo(target.transform.position);
            }
        }
    }
}
