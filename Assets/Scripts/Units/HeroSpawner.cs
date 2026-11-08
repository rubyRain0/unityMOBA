using UnityEngine;
using UnityEngine.AI;
using Moba.Abilities;
using Moba.AI;
using Moba.CameraControl;
using Moba.Combat;
using Moba.Control;
using Moba.Core;
using Moba.Map;
using Moba.Navigation;
using Moba.UI;
using Moba.Utils;

namespace Moba.Units
{
    /// <summary>
    /// Общая сборка героя — игрока или бота.
    /// </summary>
    public static class HeroSpawner
    {
        public static Hero Spawn(
            MapLayout layout,
            Team team,
            int lane,
            int spawnIndex,
            bool isPlayer,
            RTSCamera cameraRig)
        {
            Vector3 spawnPoint;
            if (isPlayer)
            {
                spawnPoint = layout.HeroSpawn(team, spawnIndex);
            }
            else
            {
                // Бот стартует у входа на свою линию, чтобы не возвращаться к базе.
                Vector3[] lanePath = layout.LaneFor(lane, team);
                Vector3 lanePoint = MapPath.PointAt(lanePath, GameConfig.MinionSpawnDistance);
                Vector3 side = MapPath.Perpendicular(
                    MapPath.DirectionAt(lanePath, GameConfig.MinionSpawnDistance)) * 1.5f;
                spawnPoint = lanePoint + side;
            }

            Color displayColor = isPlayer ? TeamColors.Player : TeamColors.Of(team);

            var go = new GameObject(isPlayer ? "PlayerHero" : $"HeroBot_{team}_{lane}");
            go.transform.position = spawnPoint;

            SceneFactory.Primitive(
                PrimitiveType.Capsule, "Visual", go.transform,
                new Vector3(0f, 1f, 0f), Vector3.one, displayColor);

            go.AddComponent<NavMeshAgent>();
            var mover = go.AddComponent<NavMover>();
            mover.Configure(GameConfig.HeroMoveSpeed);

            var hero = go.AddComponent<Hero>();
            hero.Initialize(team, GameConfig.HeroMaxHealth, GameConfig.HeroMaxMana);
            hero.SetSpawnPoint(spawnPoint);

            float damageMultiplier;
            if (isPlayer) damageMultiplier = GameConfig.PlayerDamageMultiplier;
            else if (team == Team.Red) damageMultiplier = GameConfig.EnemyDamageMultiplier;
            else damageMultiplier = 1f;

            hero.SetDamageMultiplier(damageMultiplier);

            var attacker = go.AddComponent<Attacker>();
            attacker.Initialize(
                hero,
                GameConfig.HeroAttackRange,
                GameConfig.HeroAttackDamage * damageMultiplier,
                GameConfig.HeroAttackInterval,
                GameConfig.HeroProjectileSpeed);

            var abilities = go.AddComponent<AbilitySystem>();
            abilities.Initialize(hero, isPlayer);

            WorldHealthBar.Attach(hero, displayColor);
            mover.SnapToNavMesh();

            if (isPlayer)
            {
                var controller = go.AddComponent<PlayerController>();
                controller.Setup(hero, mover, attacker, cameraRig);
            }
            else
            {
                var bot = go.AddComponent<HeroBotAI>();
                bot.Initialize(hero, mover, attacker, abilities, layout, lane);
            }

            return hero;
        }
    }
}
