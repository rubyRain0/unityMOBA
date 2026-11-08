using UnityEngine;
using Moba.Core;
using Moba.Utils;

namespace Moba.Map
{
    /// <summary>
    /// Процедурная сборка карты: земля, стены, три линии, базы, фонтаны,
    /// нексусы и башни. Возвращает MapLayout для игровых систем.
    /// </summary>
    public static class MapBuilder
    {
        private static readonly Color GroundColor = new(0.16f, 0.22f, 0.18f);
        private static readonly Color WallColor = new(0.12f, 0.13f, 0.15f);
        private static readonly Color LaneColor = new(0.42f, 0.40f, 0.30f);

        public static MapLayout Build(Transform parent)
        {
            var layout = new MapLayout
            {
                BlueBase = GameConfig.BlueBasePosition,
                RedBase = GameConfig.RedBasePosition,
                FountainRadius = GameConfig.FountainRadius
            };

            Transform map = new GameObject("Map").transform;
            map.SetParent(parent, false);
            layout.Root = map.gameObject;

            BuildGround(map);
            BuildWalls(map);
            layout.Lanes = BuildLanes(map);
            BuildBase(map, layout, Team.Blue);
            BuildBase(map, layout, Team.Red);
            BuildTowers(map, layout);

            return layout;
        }

        private static void BuildGround(Transform map)
        {
            float size = GameConfig.MapHalfSize * 2f;
            SceneFactory.Primitive(
                PrimitiveType.Cube, "Ground", map,
                new Vector3(0f, -0.25f, 0f),
                new Vector3(size, 0.5f, size),
                GroundColor);
        }

        private static void BuildWalls(Transform map)
        {
            float half = GameConfig.MapHalfSize;
            float inset = GameConfig.WallInset;
            const float thickness = 2f;
            const float height = 3f;
            float midY = height * 0.5f;

            SceneFactory.Primitive(PrimitiveType.Cube, "Wall_North", map, new Vector3(0f, midY, inset), new Vector3(half * 2f, height, thickness), WallColor);
            SceneFactory.Primitive(PrimitiveType.Cube, "Wall_South", map, new Vector3(0f, midY, -inset), new Vector3(half * 2f, height, thickness), WallColor);
            SceneFactory.Primitive(PrimitiveType.Cube, "Wall_East", map, new Vector3(inset, midY, 0f), new Vector3(thickness, height, half * 2f), WallColor);
            SceneFactory.Primitive(PrimitiveType.Cube, "Wall_West", map, new Vector3(-inset, midY, 0f), new Vector3(thickness, height, half * 2f), WallColor);
        }

        private static Vector3[][] BuildLanes(Transform map)
        {
            Vector3 blue = GameConfig.BlueBasePosition;
            Vector3 red = GameConfig.RedBasePosition;

            Vector3[][] lanes =
            {
                // Mid — прямая диагональ.
                new[] { blue, red },
                // Top — вдоль левого и верхнего края.
                new[]
                {
                    blue,
                    new Vector3(-38f, 0f, blue.z),
                    new Vector3(-38f, 0f, 38f),
                    new Vector3(red.x, 0f, 38f),
                    red
                },
                // Bottom — вдоль нижнего и правого края.
                new[]
                {
                    blue,
                    new Vector3(blue.x, 0f, -38f),
                    new Vector3(38f, 0f, -38f),
                    new Vector3(38f, 0f, red.z),
                    red
                }
            };

            foreach (Vector3[] lane in lanes)
            {
                for (int i = 1; i < lane.Length; i++)
                {
                    BuildLaneSegment(map, lane[i - 1], lane[i]);
                }
            }

            return lanes;
        }

        private static void BuildLaneSegment(Transform map, Vector3 a, Vector3 b)
        {
            Vector3 delta = b - a;
            float length = delta.magnitude;
            if (length <= 0.01f) return;

            Vector3 mid = (a + b) * 0.5f;
            mid.y = 0.02f;

            GameObject strip = SceneFactory.Primitive(
                PrimitiveType.Cube, "Lane", map, mid,
                new Vector3(5f, 0.04f, length), LaneColor, keepCollider: false);

            strip.transform.rotation = Quaternion.Euler(0f, Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg, 0f);
        }

        private static void BuildBase(Transform map, MapLayout layout, Team team)
        {
            Vector3 basePosition = layout.BaseFor(team);
            Color teamColor = TeamColors.Of(team);

            SceneFactory.Primitive(
                PrimitiveType.Cylinder, $"Base_{team}", map,
                new Vector3(basePosition.x, 0.06f, basePosition.z),
                new Vector3(18f, 0.06f, 18f),
                TeamColors.Darken(teamColor, 0.55f), keepCollider: false);

            SceneFactory.Primitive(
                PrimitiveType.Cylinder, $"Fountain_{team}", map,
                new Vector3(basePosition.x, 0.03f, basePosition.z),
                new Vector3(GameConfig.FountainRadius * 2f, 0.03f, GameConfig.FountainRadius * 2f),
                TeamColors.Darken(teamColor, 0.35f), keepCollider: false);

            SceneFactory.Primitive(
                PrimitiveType.Cylinder, $"Nexus_{team}", map,
                new Vector3(basePosition.x, 3f, basePosition.z),
                new Vector3(GameConfig.NexusRadius * 2f, 3f, GameConfig.NexusRadius * 2f),
                teamColor);

            var healZoneGo = new GameObject($"HealZone_{team}");
            healZoneGo.transform.SetParent(map, false);
            healZoneGo.transform.position = basePosition;
            HealZone healZone = healZoneGo.AddComponent<HealZone>();
            healZone.Initialize(team, GameConfig.FountainRadius, GameConfig.FountainHealPerSecond);
        }

        private static void BuildTowers(Transform map, MapLayout layout)
        {
            // Доли длины линии от синей базы до красной.
            // 0.30/0.44 для синих, 0.56/0.70 для красных — зеркально.
            float[] blueFractions = { 0.30f, 0.44f };
            float[] redFractions = { 0.56f, 0.70f };

            for (int laneIndex = 0; laneIndex < layout.Lanes.Length; laneIndex++)
            {
                Vector3[] path = layout.Lanes[laneIndex];
                float length = MapPath.Length(path);

                for (int tier = 0; tier < 2; tier++)
                {
                    BuildTower(map, layout, Team.Blue, laneIndex, tier, path, length * blueFractions[tier]);
                    BuildTower(map, layout, Team.Red, laneIndex, tier, path, length * redFractions[tier]);
                }
            }
        }

        private static void BuildTower(Transform map, MapLayout layout, Team team, int laneIndex, int tier, Vector3[] path, float distance)
        {
            Vector3 position = MapPath.PointAt(path, distance);
            Vector3 direction = MapPath.DirectionAt(path, distance);
            Vector3 side = MapPath.Perpendicular(direction);

            // Ставим башню сбоку от линии, со стороны центра карты.
            if (Vector3.Dot(side, -position) < 0f) side = -side;
            position += side * 3.2f;
            position.y = 0f;

            Color color = TeamColors.Darken(TeamColors.Of(team), 0.2f);
            SceneFactory.Primitive(
                PrimitiveType.Cylinder, $"Tower_{team}_{laneIndex}_{tier}", map,
                new Vector3(position.x, 2.5f, position.z),
                new Vector3(3f, 2.5f, 3f), color);

            layout.Towers.Add(new TowerSpot
            {
                Team = team,
                Lane = laneIndex,
                Tier = tier,
                Position = position
            });
        }
    }
}
