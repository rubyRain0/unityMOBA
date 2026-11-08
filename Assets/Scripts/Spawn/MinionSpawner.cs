using UnityEngine;
using UnityEngine.AI;
using Moba.AI;
using Moba.Combat;
using Moba.Core;
using Moba.Map;
using Moba.Navigation;
using Moba.UI;
using Moba.Units;
using Moba.Utils;

namespace Moba.Spawn
{
    /// <summary>
    /// Создаёт одного миньона с навигацией, атакой и маршрутом по линии.
    /// </summary>
    public static class MinionSpawner
    {
        public static Unit Spawn(MapLayout layout, int lane, Team team, int index, Transform parent)
        {
            Vector3[] fullPath = layout.LaneFor(lane, team);
            float distance = Mathf.Max(
                1.5f,
                GameConfig.MinionSpawnDistance - index * GameConfig.MinionSpawnSpacing);
            Vector3 position = MapPath.PointAt(fullPath, distance);
            Vector3[] route = MapPath.Trim(fullPath, distance);

            var go = new GameObject($"Minion_{team}_{lane}");
            go.transform.SetParent(parent, false);
            go.transform.position = position;

            SceneFactory.Primitive(
                PrimitiveType.Capsule, "Visual", go.transform,
                new Vector3(0f, 0.6f, 0f),
                new Vector3(0.6f, 0.6f, 0.6f),
                TeamColors.Of(team));

            go.AddComponent<NavMeshAgent>();
            var mover = go.AddComponent<NavMover>();
            mover.Configure(GameConfig.MinionMoveSpeed, 0.3f, 1.2f);

            var minion = go.AddComponent<Minion>();
            minion.Initialize(team, GameConfig.MinionMaxHealth, UnitKind.Minion);

            var attacker = go.AddComponent<Attacker>();
            attacker.Initialize(
                minion,
                GameConfig.MinionAttackRange,
                GameConfig.MinionAttackDamage,
                GameConfig.MinionAttackInterval,
                GameConfig.MinionProjectileSpeed);

            var ai = go.AddComponent<MinionAI>();
            ai.Initialize(minion, mover, attacker, route);

            WorldHealthBar.Attach(minion, TeamColors.Of(team), 1.8f, 0.8f);
            mover.SnapToNavMesh();

            return minion;
        }
    }
}
