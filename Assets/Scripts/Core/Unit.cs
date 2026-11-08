using System;
using UnityEngine;

namespace Moba.Core
{
    /// <summary>
    /// Базовый боевой юнит: команда, здоровье, урон и события.
    /// Герои, миньоны, башни и нексусы наследуются от него.
    /// </summary>
    public class Unit : MonoBehaviour
    {
        public Team Team { get; private set; } = Team.None;
        public float MaxHealth { get; private set; }
        public float Health { get; private set; }
        public bool IsAlive => Health > 0f;

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

        public virtual void Initialize(Team team, float maxHealth)
        {
            Team = team;
            MaxHealth = Mathf.Max(1f, maxHealth);
            Health = MaxHealth;
            HealthChanged?.Invoke(this);
        }

        public virtual void TakeDamage(float amount, Unit source = null)
        {
            if (!IsAlive || amount <= 0f) return;

            Health = Mathf.Max(0f, Health - amount);
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

        public void Revive()
        {
            Health = MaxHealth;
            HealthChanged?.Invoke(this);
        }

        protected virtual void OnDeath(Unit killer)
        {
            Died?.Invoke(this, killer);
        }
    }
}
