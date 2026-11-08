using UnityEngine;
using Moba.CameraControl;
using Moba.Core;
using Moba.Navigation;
using Moba.Units;

namespace Moba.Abilities
{
    /// <summary>
    /// Три способности героя, мана и перезарядки. Игрок кастует Q/W/E,
    /// боты используют тот же набор через TryCast.
    /// </summary>
    public class AbilitySystem : MonoBehaviour
    {
        private Hero hero;
        private NavMover mover;
        private Ability[] abilities;

        public bool PlayerControlled { get; private set; }

        public void Initialize(Hero owner, bool playerControlled)
        {
            hero = owner;
            mover = GetComponent<NavMover>();
            PlayerControlled = playerControlled;

            abilities = new Ability[]
            {
                new ProjectileAbility(),
                new DashAbility(),
                new HealAbility()
            };
        }

        public Ability GetAbility(int index)
        {
            if (abilities == null || index < 0 || index >= abilities.Length) return null;
            return abilities[index];
        }

        public float GetCooldown(int index)
        {
            Ability ability = GetAbility(index);
            return ability != null ? ability.CooldownRemaining : 0f;
        }

        public bool IsReady(int index)
        {
            Ability ability = GetAbility(index);
            return ability != null && ability.IsReady;
        }

        public bool TryCast(int index, Vector3 aimPoint)
        {
            if (hero == null || !hero.IsAlive) return false;

            Ability ability = GetAbility(index);
            if (ability == null) return false;

            return ability.TryCast(new AbilityContext(hero, aimPoint, mover));
        }

        private void Update()
        {
            if (abilities == null) return;

            for (int i = 0; i < abilities.Length; i++)
            {
                abilities[i].Tick(Time.deltaTime);
            }

            if (!PlayerControlled || hero == null || !hero.IsAlive) return;
            if (MatchManager.Instance != null && !MatchManager.Instance.IsPlaying) return;

            if (Input.GetKeyDown(KeyCode.Q)) CastAtCursor(0);
            if (Input.GetKeyDown(KeyCode.W)) CastAtCursor(1);
            if (Input.GetKeyDown(KeyCode.E)) CastAtCursor(2);
        }

        private void CastAtCursor(int index)
        {
            Vector3 aimPoint = hero.transform.position + hero.transform.forward * 5f;

            if (RTSCamera.Instance != null && RTSCamera.Instance.TryGetGroundPoint(out Vector3 groundPoint))
            {
                aimPoint = groundPoint;
            }

            TryCast(index, aimPoint);
        }
    }
}
