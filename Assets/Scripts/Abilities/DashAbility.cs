using UnityEngine;
using UnityEngine.AI;
using Moba.Core;
using Moba.Effects;
using Moba.Utils;

namespace Moba.Abilities
{
    /// <summary>W — рывок к курсору с проверкой навигационной сетки.</summary>
    public class DashAbility : Ability
    {
        public DashAbility()
            : base("Рывок", KeyCode.W, 30f, 8f, new Color(1f, 0.75f, 0.25f))
        {
        }

        protected override void Execute(AbilityContext context)
        {
            Vector3 start = context.Caster.transform.position;
            Vector3 direction = MathUtil.FlatDirection(start, context.AimPoint);
            if (direction == Vector3.zero)
            {
                direction = MathUtil.Flat(context.Caster.transform.forward).normalized;
            }

            Vector3 target = start + direction * GameConfig.DashDistance;
            if (NavMesh.SamplePosition(target, out NavMeshHit hit, 6f, NavMesh.AllAreas))
            {
                target = hit.position;
            }

            if (context.Mover != null && context.Mover.Agent != null)
            {
                context.Mover.Agent.Warp(target);
                context.Mover.Stop();
            }

            TimedEffect.Spawn(start + Vector3.up, Color, 1.4f, 0.2f, 0.35f);
        }
    }
}
