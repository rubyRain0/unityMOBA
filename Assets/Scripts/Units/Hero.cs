using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Moba.Core;
using Moba.Navigation;

namespace Moba.Units
{
    /// <summary>
    /// Герой: здоровье из Unit, плюс мана и смерть с респауном на базе.
    /// </summary>
    public class Hero : Unit
    {
        public float MaxMana { get; private set; }
        public float Mana { get; private set; }
        public Vector3 SpawnPoint { get; private set; }
        public float RespawnEndTime { get; private set; }
        public UnitKind LastKillerKind { get; private set; } = UnitKind.None;
        public bool LastKillerWasNexus { get; private set; }
        public float DamageMultiplier { get; private set; } = 1f;

        public event Action<Hero> ManaChanged;

        private GameObject visual;
        private Collider bodyCollider;
        private NavMeshAgent agent;
        private NavMover mover;
        private Coroutine respawnRoutine;

        private void Awake()
        {
            if (transform.childCount > 0) visual = transform.GetChild(0).gameObject;
            if (visual != null) bodyCollider = visual.GetComponent<Collider>();
            agent = GetComponent<NavMeshAgent>();
            mover = GetComponent<NavMover>();
        }

        private void Update()
        {
            if (!IsAlive || Mana >= MaxMana) return;
            Mana = Mathf.Min(MaxMana, Mana + GameConfig.HeroManaRegenPerSecond * Time.deltaTime);
        }

        public void Initialize(Team team, float maxHealth, float maxMana)
        {
            base.Initialize(team, maxHealth, UnitKind.Hero);
            MaxMana = maxMana;
            Mana = maxMana;
            ManaChanged?.Invoke(this);
        }

        public void SetSpawnPoint(Vector3 point)
        {
            SpawnPoint = point;
        }

        public void SetDamageMultiplier(float value)
        {
            DamageMultiplier = Mathf.Max(0.01f, value);
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

        protected override void OnDeath(Unit killer)
        {
            LastKillerKind = killer != null ? killer.Kind : UnitKind.None;
            LastKillerWasNexus = killer is Structure structure && structure.IsNexus;
            RespawnEndTime = Time.time + GameConfig.HeroRespawnSeconds;

            base.OnDeath(killer);
            if (mover != null) mover.Stop();
            if (respawnRoutine == null) respawnRoutine = StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            if (visual != null) visual.SetActive(false);
            if (bodyCollider != null) bodyCollider.enabled = false;
            if (agent != null) agent.enabled = false;

            yield return new WaitForSeconds(GameConfig.HeroRespawnSeconds);

            transform.position = SpawnPoint;
            if (agent != null) agent.enabled = true;
            if (mover != null)
            {
                mover.Stop();
                mover.SnapToNavMesh();
            }
            if (visual != null) visual.SetActive(true);
            if (bodyCollider != null) bodyCollider.enabled = true;

            Revive();
            Mana = MaxMana;
            ManaChanged?.Invoke(this);

            respawnRoutine = null;
        }
    }
}
