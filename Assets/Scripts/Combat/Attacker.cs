using UnityEngine;
using Moba.Core;
using Moba.Utils;

namespace Moba.Combat
{
    /// <summary>
    /// Базовая атака: радиус, урон, интервал и спавн снаряда.
    /// Используется героями, миньонами и башнями.
    /// </summary>
    public class Attacker : MonoBehaviour
    {
        private Unit owner;
        private float range;
        private float damage;
        private float interval;
        private float projectileSpeed;
        private float nextAttackTime;

        public float Range => range;
        public float Damage => damage;

        public void Initialize(
            Unit attacker,
            float attackRange,
            float attackDamage,
            float attackInterval,
            float speed = 25f)
        {
            owner = attacker;
            range = attackRange;
            damage = attackDamage;
            interval = attackInterval;
            projectileSpeed = speed;
        }

        public bool CanReach(Unit target)
        {
            if (target == null || owner == null || !owner.IsAlive) return false;
            if (!target.IsAlive || !target.CanBeDamaged) return false;
            if (target.Team == owner.Team || target.Team == Team.None) return false;

            return MathUtil.FlatDistance(owner.transform.position, target.transform.position) <= range;
        }

        public bool TryAttack(Unit target)
        {
            if (Time.time < nextAttackTime || !CanReach(target)) return false;

            nextAttackTime = Time.time + interval;
            Projectile.Spawn(owner, target, damage, projectileSpeed, TeamColors.Of(owner.Team));
            return true;
        }
    }
}
