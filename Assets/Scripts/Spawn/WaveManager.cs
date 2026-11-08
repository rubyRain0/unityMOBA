using UnityEngine;
using Moba.Core;
using Moba.Map;

namespace Moba.Spawn
{
    /// <summary>
    /// Спавнит волны миньонов на всех линиях для обеих команд.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        private MapLayout layout;
        private float nextWaveTime;
        private int waveNumber;

        public int WaveNumber => waveNumber;

        public void Initialize(MapLayout mapLayout)
        {
            layout = mapLayout;
            nextWaveTime = Time.time + GameConfig.FirstWaveDelay;
        }

        private void Update()
        {
            if (layout == null || Time.time < nextWaveTime) return;

            nextWaveTime = Time.time + GameConfig.WaveInterval;
            waveNumber++;

            for (int lane = 0; lane < layout.Lanes.Length; lane++)
            {
                for (int i = 0; i < GameConfig.MinionsPerWave; i++)
                {
                    MinionSpawner.Spawn(layout, lane, Team.Blue, i, transform);
                    MinionSpawner.Spawn(layout, lane, Team.Red, i, transform);
                }
            }
        }
    }
}
