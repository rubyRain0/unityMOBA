using UnityEngine;
using UnityEngine.AI;
using Moba.CameraControl;
using Moba.Combat;
using Moba.Control;
using Moba.Map;
using Moba.Navigation;
using Moba.Spawn;
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
        private void Start()
        {
            UnitRegistry.Clear();
            EnsureLighting();

            MapLayout layout = MapBuilder.Build(transform);
            StructureSpawner.SpawnAll(layout, layout.Root.transform);
            NavMeshRuntimeBaker.Bake(layout.Root);

            RTSCamera cameraRig = RTSCamera.Create();
            Hero hero = SpawnHero(layout, cameraRig);
            cameraRig.Follow(hero.transform);

            var wavesGo = new GameObject("Waves");
            wavesGo.transform.SetParent(transform, false);
            var waves = wavesGo.AddComponent<WaveManager>();
            waves.Initialize(layout);

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
            hero.SetSpawnPoint(layout.HeroSpawn(Team.Blue));

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

    }
}
