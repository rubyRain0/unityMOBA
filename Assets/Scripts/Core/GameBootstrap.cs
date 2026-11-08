using UnityEngine;
using UnityEngine.AI;
using Moba.CameraControl;
using Moba.Combat;
using Moba.Control;
using Moba.Map;
using Moba.Navigation;
using Moba.UI;
using Moba.Units;
using Moba.Utils;

namespace Moba.Core
{
    /// <summary>
    /// Точка входа матча: собирает карту, печёт NavMesh, создаёт камеру,
    /// героя игрока, манекены и HUD. Сцена содержит только этот объект.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private bool spawnTrainingDummies = true;

        private void Start()
        {
            UnitRegistry.Clear();
            EnsureLighting();

            MapLayout layout = MapBuilder.Build(transform);
            NavMeshRuntimeBaker.Bake(layout.Root);

            RTSCamera cameraRig = RTSCamera.Create();
            Hero hero = SpawnHero(layout, cameraRig);
            cameraRig.Follow(hero.transform);

            if (spawnTrainingDummies) SpawnTrainingDummies(layout);

            Hud.Create(hero);
        }

        private static void EnsureLighting()
        {
            if (FindFirstObjectByType<Light>() == null)
            {
                var lightGo = new GameObject("Directional Light");
                Light light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.3f;
                light.color = new Color(1f, 0.97f, 0.9f);
                lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.35f, 0.38f, 0.45f);
        }

        private static Hero SpawnHero(MapLayout layout, RTSCamera cameraRig)
        {
            var go = new GameObject("PlayerHero");
            go.transform.position = layout.HeroSpawn(Team.Blue);

            SceneFactory.Primitive(
                PrimitiveType.Capsule, "Visual", go.transform,
                new Vector3(0f, 1f, 0f), Vector3.one, TeamColors.Player);

            go.AddComponent<NavMeshAgent>();
            var mover = go.AddComponent<NavMover>();
            mover.Configure(GameConfig.HeroMoveSpeed);

            var hero = go.AddComponent<Hero>();
            hero.Initialize(Team.Blue, GameConfig.HeroMaxHealth, GameConfig.HeroMaxMana);

            var attacker = go.AddComponent<Attacker>();
            attacker.Initialize(
                hero,
                GameConfig.HeroAttackRange,
                GameConfig.HeroAttackDamage,
                GameConfig.HeroAttackInterval,
                GameConfig.HeroProjectileSpeed);

            var controller = go.AddComponent<PlayerController>();
            controller.Setup(hero, mover, attacker, cameraRig);

            WorldHealthBar.Attach(hero, TeamColors.Blue);
            mover.SnapToNavMesh();

            return hero;
        }

        private static void SpawnTrainingDummies(MapLayout layout)
        {
            Vector3[] offsets =
            {
                new(14f, 0f, 4f),
                new(18f, 0f, 9f),
                new(9f, 0f, 12f)
            };

            foreach (Vector3 offset in offsets)
            {
                var go = new GameObject("TrainingDummy");
                go.transform.position = layout.BlueBase + offset;

                SceneFactory.Primitive(
                    PrimitiveType.Capsule, "Visual", go.transform,
                    new Vector3(0f, 1f, 0f), Vector3.one, TeamColors.Red);

                var dummy = go.AddComponent<TrainingDummy>();
                dummy.Initialize(Team.Red, GameConfig.DummyMaxHealth);

                WorldHealthBar.Attach(dummy, TeamColors.Red);
            }
        }
    }
}
