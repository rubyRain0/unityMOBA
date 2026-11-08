using UnityEngine;
using Moba.Core;
using Moba.Utils;

namespace Moba.Combat
{
    /// <summary>
    /// Самонаводящийся снаряд базовой атаки. Урон применяется при попадании.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private const float Lifetime = 5f;

        private Unit owner;
        private Unit target;
        private float damage;
        private float speed;
        private float remainingLife;

        public static Projectile Spawn(Unit owner, Unit target, float damage, float speed, Color color)
        {
            var go = new GameObject("Projectile");
            go.transform.position = owner.transform.position + Vector3.up * SpawnHeight(owner.Kind);

            SceneFactory.Primitive(
                PrimitiveType.Sphere, "Visual", go.transform,
                Vector3.zero, Vector3.one * 0.35f, color, keepCollider: false);

            var projectile = go.AddComponent<Projectile>();
            projectile.owner = owner;
            projectile.target = target;
            projectile.damage = damage;
            projectile.speed = speed;
            projectile.remainingLife = Lifetime;
            return projectile;
        }

        private void Update()
        {
            remainingLife -= Time.deltaTime;

            if (remainingLife <= 0f || owner == null || target == null || !target.IsAlive)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 aimPoint = target.transform.position + Vector3.up * AimHeight(target.Kind);
            transform.position = Vector3.MoveTowards(transform.position, aimPoint, speed * Time.deltaTime);

            if ((transform.position - aimPoint).sqrMagnitude <= 0.04f)
            {
                target.TakeDamage(damage, owner);
                Destroy(gameObject);
            }
        }

        private static float SpawnHeight(UnitKind kind)
        {
            return kind switch
            {
                UnitKind.Structure => 4f,
                UnitKind.Minion => 0.8f,
                UnitKind.Hero => 1.2f,
                _ => 1f
            };
        }

        private static float AimHeight(UnitKind kind)
        {
            return kind switch
            {
                UnitKind.Structure => 2.5f,
                UnitKind.Minion => 0.7f,
                UnitKind.Hero => 1.1f,
                _ => 0.8f
            };
        }
    }
}
