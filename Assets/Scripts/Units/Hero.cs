using System;
using UnityEngine;
using Moba.Core;

namespace Moba.Units
{
    /// <summary>
    /// Герой: здоровье из Unit, плюс мана для способностей.
    /// </summary>
    public class Hero : Unit
    {
        public float MaxMana { get; private set; }
        public float Mana { get; private set; }

        public event Action<Hero> ManaChanged;

        public void Initialize(Team team, float maxHealth, float maxMana)
        {
            base.Initialize(team, maxHealth);
            MaxMana = maxMana;
            Mana = maxMana;
            ManaChanged?.Invoke(this);
        }

        public bool TrySpendMana(float amount)
        {
            if (Mana < amount) return false;

            Mana -= amount;
            ManaChanged?.Invoke(this);
            return true;
        }

        public void RestoreMana(float amount)
        {
            Mana = Mathf.Min(MaxMana, Mana + amount);
            ManaChanged?.Invoke(this);
        }
    }
}
