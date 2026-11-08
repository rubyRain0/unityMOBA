using UnityEngine;
using Moba.Core;
using Moba.Effects;
using Moba.Utils;

namespace Moba.Abilities
{
    /// <summary>
    /// Линейный снаряд способности: летит прямо и наносит урон первому врагу.
    /// </summary>
    public class Skillshot : MonoBehaviour
    {
        private Unit owner;
        private Vector3 direction;
        private Color color;
        private float damage;
        private float speed;
        private float maxRange;
        private float traveled;

        public static Skillshot Spawn(
            Unit owner,
            Vector3 origin,
            Vector3 direction,
            float damage,
            float speed,
            float maxRange,
            Color color)
        {
            var go = new GameObject("Skillshot");
            go.transform.position = origin;

            SceneFactory.Primitive(
                PrimitiveType.Sphere, "Visual", go.transform,
                Vector3.zero, Vector3.one * 0.45f, color, keepCollider: false);

            var skillshot = go.AddComponent<Skillshot>();
            skillshot.owner = owner;
            skillshot.direction = direction;
            skillshot.damage = damage;
            skillshot.speed = speed;
            skillshot.maxRange = maxRange;
            skillshot.color = color;
            return skillshot;
        }

        private void Update()
        {
            if (owner == null)
            {
                Destroy(gameObject);
                return;
            }

            float step = speed * Time.deltaTime;
            transform.position += direction * step;
            traveled += step;

            Unit hit = FindHit();
            if (hit != null)
            {
                hit.TakeDamage(damage, owner);
                TimedEffect.Spawn(transform.position, color, 0.6f, 1.8f, 0.3f);
                Destroy(gameObject);
                return;
            }

            if (traveled >= maxRange) Destroy(gameObject);
        }

        private Unit FindHit()
        {
            Unit best = null;
            float bestDistance = float.MaxValue;

            foreach (Unit unit in UnitRegistry.All)
            {
                if (unit == null || unit == owner) continue;
                if (unit.Team == owner.Team || unit.Team == Team.None) continue;
                if (!unit.CanBeDamaged) continue;

                float distance = MathUtil.FlatDistance(transform.position, unit.transform.position);
                float radius = GameConfig.SkillshotRadius + HitRadius(unit);
                if (distance > radius || distance >= bestDistance) continue;

                bestDistance = distance;
                best = unit;
            }

            return best;
        }

        private static float HitRadius(Unit unit)
        {
            return unit.Kind switch
            {
                UnitKind.Structure => 2f,
                UnitKind.Hero => 0.6f,
                UnitKind.Minion => 0.4f,
                _ => 0.5f
            };
        }
    }
}
