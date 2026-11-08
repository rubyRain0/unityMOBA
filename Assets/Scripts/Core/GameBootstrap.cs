using UnityEngine;
using Moba.CameraControl;
using Moba.Map;
using Moba.Navigation;
using Moba.Spawn;
using Moba.UI;
using Moba.Units;

namespace Moba.Core
{
    /// <summary>
    /// Точка входа матча: собирает карту, печёт NavMesh, создаёт камеру,
    /// шесть героев (игрок + 5 ботов), волны миньонов и HUD.
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

            // Индексы линий: 0 — mid, 1 — top, 2 — bottom.
            Hero player = HeroSpawner.Spawn(layout, Team.Blue, 1, 1, true, cameraRig);
            HeroSpawner.Spawn(layout, Team.Blue, 1, 0, false, cameraRig);
            HeroSpawner.Spawn(layout, Team.Blue, 2, 2, false, cameraRig);
            HeroSpawner.Spawn(layout, Team.Red, 0, 0, false, cameraRig);
            HeroSpawner.Spawn(layout, Team.Red, 1, 1, false, cameraRig);
            HeroSpawner.Spawn(layout, Team.Red, 2, 2, false, cameraRig);

            cameraRig.Follow(player.transform);

            var wavesGo = new GameObject("Waves");
            wavesGo.transform.SetParent(transform, false);
            var waves = wavesGo.AddComponent<WaveManager>();
            waves.Initialize(layout);

            Hud.Create(player);
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
    }
}
