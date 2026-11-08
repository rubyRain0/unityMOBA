using UnityEngine;
using Moba.Core;

namespace Moba.Units
{
    /// <summary>
    /// Башня или нексус. После разрушения исчезает визуал и отключается
    /// коллайдер, но объект остаётся в реестре для подсчёта итогов.
    /// Вторая башня линии и нексус защищены до разрушения предыдущих целей.
    /// </summary>
    public class Structure : Unit
    {
        private GameObject visual;
        private Collider bodyCollider;

        public bool IsNexus { get; private set; }
        public int Lane { get; private set; } = -1;
        public int Tier { get; private set; } = -1;

        public override bool CanBeDamaged => base.CanBeDamaged && IsObjectiveUnlocked();
        public override bool IsPrimaryObjective => IsNexus;

        private void Awake()
        {
            if (transform.childCount > 0) visual = transform.GetChild(0).gameObject;
            if (visual != null) bodyCollider = visual.GetComponent<Collider>();
        }

        public void ConfigureObjective(int lane, int tier, bool isNexus)
        {
            Lane = lane;
            Tier = tier;
            IsNexus = isNexus;
        }

        protected override void OnDeath(Unit killer)
        {
            base.OnDeath(killer);
            if (bodyCollider != null) bodyCollider.enabled = false;
            if (visual != null) visual.SetActive(false);
        }

        /// <summary>Строение — одноразовая цель: возрождение запрещено.</summary>
        public override void Revive()
        {
        }

        private bool IsObjectiveUnlocked()
        {
            if (IsNexus) return HasDestroyedInnerTower(Team);
            if (Tier <= 0) return true;
            return IsTowerDestroyed(Team, Lane, 0);
        }

        private static bool IsTowerDestroyed(Team team, int lane, int tier)
        {
            foreach (Unit unit in UnitRegistry.All)
            {
                if (unit is not Structure tower || tower.IsNexus) continue;
                if (tower.Team != team || tower.Lane != lane || tower.Tier != tier) continue;

                return !tower.IsAlive;
            }

            return false;
        }

        private static bool HasDestroyedInnerTower(Team team)
        {
            foreach (Unit unit in UnitRegistry.All)
            {
                if (unit is not Structure tower || tower.IsNexus) continue;
                if (tower.Team == team && tower.Tier == 1 && !tower.IsAlive) return true;
            }

            return false;
        }
    }
}
