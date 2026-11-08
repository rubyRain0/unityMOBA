using System.Collections;
using UnityEngine;
using Moba.Core;

namespace Moba.Units
{
    /// <summary>
    /// Манекен для проверки боя: после смерти исчезает и через пару секунд
    /// возвращается с полным запасом здоровья.
    /// </summary>
    public class TrainingDummy : Unit
    {
        private GameObject visual;
        private Collider bodyCollider;
        private Coroutine respawnRoutine;

        private void Awake()
        {
            if (transform.childCount > 0) visual = transform.GetChild(0).gameObject;
            if (visual != null) bodyCollider = visual.GetComponent<Collider>();
        }

        protected override void OnDeath(Unit killer)
        {
            base.OnDeath(killer);
            if (respawnRoutine == null) respawnRoutine = StartCoroutine(RespawnAfterDelay());
        }

        private IEnumerator RespawnAfterDelay()
        {
            if (visual != null) visual.SetActive(false);
            if (bodyCollider != null) bodyCollider.enabled = false;

            yield return new WaitForSeconds(GameConfig.DummyRespawnSeconds);

            if (bodyCollider != null) bodyCollider.enabled = true;
            if (visual != null) visual.SetActive(true);

            Revive();
            respawnRoutine = null;
        }
    }
}
