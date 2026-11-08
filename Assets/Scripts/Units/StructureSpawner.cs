using UnityEngine;
using Moba.AI;
using Moba.Combat;
using Moba.Core;
using Moba.Map;
using Moba.UI;
using Moba.Utils;

namespace Moba.Units
{
    /// <summary>
    /// Создаёт башни и нексусы как боевые юниты. Вызывается до запекания
    /// NavMesh, чтобы строения стали препятствиями.
    /// </summary>
    public static class StructureSpawner
    {
        public static void SpawnAll(MapLayout layout, Transform parent)
        {
            foreach (TowerSpot spot in layout.Towers)
            {
                SpawnTower(spot, parent);
            }

            SpawnNexus(layout, Team.Blue, parent);
            SpawnNexus(layout, Team.Red, parent);
        }

        private static void SpawnTower(TowerSpot spot, Transform parent)
        {
            var go = new GameObject($"Tower_{spot.Team}_{spot.Lane}_{spot.Tier}");
            go.transform.SetParent(parent, false);
            go.transform.position = spot.Position;

            SceneFactory.Primitive(
                PrimitiveType.Cylinder, "Visual", go.transform,
                new Vector3(0f, GameConfig.TowerHeight * 0.5f, 0f),
                new Vector3(3f, GameConfig.TowerHeight * 0.5f, 3f),
                TeamColors.Darken(TeamColors.Of(spot.Team), 0.2f));

            var structure = go.AddComponent<Structure>();
            structure.Initialize(spot.Team, GameConfig.TowerMaxHealth, UnitKind.Structure);
            structure.ConfigureObjective(spot.Lane, spot.Tier, false);

            var attacker = go.AddComponent<Attacker>();
            attacker.Initialize(
                structure,
                GameConfig.TowerAttackRange,
                GameConfig.TowerAttackDamage,
                GameConfig.TowerAttackInterval,
                GameConfig.TowerProjectileSpeed);

            var turret = go.AddComponent<TurretAI>();
            turret.Initialize(structure, attacker);

            WorldHealthBar.Attach(
                structure,
                TeamColors.Of(spot.Team),
                GameConfig.TowerHeight + 1.2f,
                2f);
        }

        private static void SpawnNexus(MapLayout layout, Team team, Transform parent)
        {
            var go = new GameObject($"Nexus_{team}");
            go.transform.SetParent(parent, false);
            go.transform.position = layout.BaseFor(team);

            SceneFactory.Primitive(
                PrimitiveType.Cylinder, "Visual", go.transform,
                new Vector3(0f, 3f, 0f),
                new Vector3(GameConfig.NexusRadius * 2f, 3f, GameConfig.NexusRadius * 2f),
                TeamColors.Of(team));

            var structure = go.AddComponent<Structure>();
            structure.Initialize(team, GameConfig.NexusMaxHealth, UnitKind.Structure);
            structure.ConfigureObjective(-1, -1, true);

            var attacker = go.AddComponent<Attacker>();
            attacker.Initialize(
                structure,
                GameConfig.NexusAttackRange,
                GameConfig.NexusAttackDamage,
                GameConfig.NexusAttackInterval,
                GameConfig.NexusProjectileSpeed);

            var turret = go.AddComponent<TurretAI>();
            turret.Initialize(structure, attacker);

            WorldHealthBar.Attach(structure, TeamColors.Of(team), 7.5f, 2.6f);
        }
    }
}
