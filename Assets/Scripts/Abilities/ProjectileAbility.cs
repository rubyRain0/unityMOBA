using UnityEngine;
using Moba.Core;
using Moba.Utils;

namespace Moba.Abilities
{
    /// <summary>Q — линейный снаряд по направлению курсора.</summary>
    public class ProjectileAbility : Ability
    {
        public ProjectileAbility()
            : base("Снаряд", KeyCode.Q, 35f, 5f, new Color(0.3f, 0.9f, 1f))
        {
        }

        protected override void Execute(AbilityContext context)
        {
            Vector3 origin = context.Caster.transform.position + Vector3.up * 1.2f;
            Vector3 direction = MathUtil.FlatDirection(context.Caster.transform.position, context.AimPoint);
            if (direction == Vector3.zero)
            {
                direction = MathUtil.Flat(context.Caster.transform.forward).normalized;
            }

            Skillshot.Spawn(
                context.Caster,
                origin,
                direction,
                GameConfig.SkillshotDamage * context.Caster.DamageMultiplier,
                GameConfig.SkillshotSpeed,
                GameConfig.SkillshotRange,
                Color);
        }
    }
}
