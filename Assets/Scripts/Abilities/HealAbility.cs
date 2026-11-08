using UnityEngine;
using Moba.Core;
using Moba.Effects;

namespace Moba.Abilities
{
    /// <summary>E — мгновенное самолечение.</summary>
    public class HealAbility : Ability
    {
        public HealAbility()
            : base("Лечение", KeyCode.E, 45f, 15f, new Color(0.4f, 1f, 0.45f))
        {
        }

        protected override void Execute(AbilityContext context)
        {
            context.Caster.Heal(context.Caster.MaxHealth * GameConfig.HealAbilityPercent);

            TimedEffect.SpawnAttached(
                context.Caster.transform,
                Vector3.up,
                Color,
                1.2f,
                2.6f,
                0.5f);
        }
    }
}
