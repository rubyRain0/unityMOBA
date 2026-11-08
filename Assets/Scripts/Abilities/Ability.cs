using UnityEngine;
using Moba.Navigation;
using Moba.Units;

namespace Moba.Abilities
{
    public readonly struct AbilityContext
    {
        public readonly Hero Caster;
        public readonly Vector3 AimPoint;
        public readonly NavMover Mover;

        public AbilityContext(Hero caster, Vector3 aimPoint, NavMover mover)
        {
            Caster = caster;
            AimPoint = aimPoint;
            Mover = mover;
        }
    }

    /// <summary>
    /// Базовая активная способность: мана, перезарядка и вызов эффекта.
    /// </summary>
    public abstract class Ability
    {
        public string Name { get; }
        public KeyCode Key { get; }
        public float ManaCost { get; }
        public float Cooldown { get; }
        public Color Color { get; }
        public float CooldownRemaining { get; private set; }

        public bool IsReady => CooldownRemaining <= 0f;

        protected Ability(string name, KeyCode key, float manaCost, float cooldown, Color color)
        {
            Name = name;
            Key = key;
            ManaCost = manaCost;
            Cooldown = cooldown;
            Color = color;
        }

        public bool TryCast(AbilityContext context)
        {
            if (!IsReady) return false;
            if (context.Caster == null || !context.Caster.IsAlive) return false;
            if (!context.Caster.TrySpendMana(ManaCost)) return false;

            CooldownRemaining = Cooldown;
            Execute(context);
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (CooldownRemaining <= 0f) return;
            CooldownRemaining = Mathf.Max(0f, CooldownRemaining - deltaTime);
        }

        protected abstract void Execute(AbilityContext context);
    }
}
