using System;
using UnityEngine;

namespace Moba.Core
{
    public enum UnitKind
    {
        None = 0,
        Hero = 1,
        Minion = 2,
        Structure = 3
    }

    /// <summary>
    /// Базовый боевой юнит: команда, здоровье, урон и события.
    /// Герои, миньоны, башни и нексусы наследуются от него.
    /// </summary>
    public class Unit : MonoBehaviour
    {
        public Team Team { get; private set; } = Team.None;
        public UnitKind Kind { get; private set; } = UnitKind.None;
        public float MaxHealth { get; private set; }
        public float Health { get; private set; }
        public bool IsAlive => Health > 0f;

        /// <summary>Можно ли сейчас наносить урон. Башни и нексус переопределяют.</summary>
        public virtual bool CanBeDamaged => IsAlive;

        /// <summary>Главная цель (нексус). Отличает её от обычных строений.</summary>
        public virtual bool IsPrimaryObjective => false;

        public event Action<Unit> HealthChanged;
        public event Action<Unit, Unit> Died;

        protected virtual void OnEnable()
        {
            UnitRegistry.Register(this);
        }

        protected virtual void OnDisable()
        {
            UnitRegistry.Unregister(this);
        }

        public virtual void Initialize(Team team, float maxHealth, UnitKind kind = UnitKind.None)
        {
            Team = team;
            Kind = kind;
            MaxHealth = Mathf.Max(1f, maxHealth);
            Health = MaxHealth;
            HealthChanged?.Invoke(this);
        }

        public virtual void TakeDamage(float amount, Unit source = null)
        {
            if (!IsAlive || !CanBeDamaged || amount <= 0f) return;

            float before = Health;
            Health = Mathf.Max(0f, Health - amount);
            float applied = before - Health;

            if (source != null && source.Kind == UnitKind.Hero && Kind == UnitKind.Hero)
            {
                MatchManager.Instance?.RecordDamage(source, applied);
            }

            HealthChanged?.Invoke(this);

            if (Health <= 0f)
            {
                OnDeath(source);
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return;

            Health = Mathf.Min(MaxHealth, Health + amount);
            HealthChanged?.Invoke(this);
        }

        public virtual void Revive()
        {
            Health = MaxHealth;
            HealthChanged?.Invoke(this);
        }

        protected virtual void OnDeath(Unit killer)
        {
            Died?.Invoke(this, killer);
            UnitRegistry.NotifyDied(this, killer);
        }
    }
}
