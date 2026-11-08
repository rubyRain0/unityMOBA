using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moba.Core
{
    public enum MatchState
    {
        Countdown,
        Playing,
        Ended
    }

    public class HeroStats
    {
        public Unit Hero;
        public Team Team;
        public int Kills;
        public int Deaths;
        public float DamageToHeroes;
    }

    public class MatchResult
    {
        public Team Winner;
        public int BlueKills;
        public int RedKills;
        public int BlueTowers;
        public int RedTowers;
        public float Duration;
        public int PlayerKills;
        public int PlayerDeaths;
        public float PlayerDamage;
    }

    /// <summary>
    /// Состояние матча: отсчёт, игра, завершение. Считает убийства, башни,
    /// урон по героям и определяет победителя по нексусу или таймеру.
    /// </summary>
    public class MatchManager : MonoBehaviour
    {
        public static MatchManager Instance { get; private set; }

        public MatchState State { get; private set; } = MatchState.Countdown;
        public bool IsPlaying => State == MatchState.Playing;
        public bool IsCountdown => State == MatchState.Countdown;

        public float CountdownRemaining { get; private set; }
        public float ElapsedTime { get; private set; }
        public int BlueKills { get; private set; }
        public int RedKills { get; private set; }
        public int BlueTowers { get; private set; }
        public int RedTowers { get; private set; }

        public event Action<MatchResult> MatchEnded;

        private readonly Dictionary<Unit, HeroStats> heroStats = new();
        private Unit playerHero;
        private float matchStartTime;
        private bool initialized;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            UnitRegistry.UnitDied -= OnUnitDied;
            if (Instance == this) Instance = null;
        }

        public void Initialize(Unit player)
        {
            if (initialized) return;
            initialized = true;
            playerHero = player;

            foreach (Unit unit in UnitRegistry.All)
            {
                if (unit != null && unit.Kind == UnitKind.Hero)
                {
                    heroStats[unit] = new HeroStats
                    {
                        Hero = unit,
                        Team = unit.Team
                    };
                }
            }

            UnitRegistry.UnitDied += OnUnitDied;

            CountdownRemaining = GameConfig.MatchCountdownSeconds;
            State = MatchState.Countdown;
            Time.timeScale = 0f;
        }

        private void Update()
        {
            if (State == MatchState.Countdown)
            {
                // Ограничиваем шаг: первый кадр после генерации карты и запекания
                // NavMesh может длиться несколько секунд и «съесть» весь отсчёт.
                CountdownRemaining -= Mathf.Min(Time.unscaledDeltaTime, 0.1f);
                if (CountdownRemaining <= 0f)
                {
                    CountdownRemaining = 0f;
                    State = MatchState.Playing;
                    Time.timeScale = 1f;
                    matchStartTime = Time.time;
                }
                return;
            }

            if (State != MatchState.Playing) return;

            ElapsedTime = Time.time - matchStartTime;

            if (ElapsedTime >= GameConfig.MatchDurationSeconds)
            {
                EndMatchByTime();
            }
        }

        public void RecordDamage(Unit source, float amount)
        {
            if (source == null || amount <= 0f) return;
            if (!heroStats.TryGetValue(source, out HeroStats stats)) return;

            stats.DamageToHeroes += amount;
        }

        private void OnUnitDied(Unit victim, Unit killer)
        {
            if (victim == null) return;

            if (victim.Kind == UnitKind.Hero && heroStats.TryGetValue(victim, out HeroStats victimStats))
            {
                victimStats.Deaths++;

                Team killerTeam = killer != null && killer.Team != Team.None
                    ? killer.Team
                    : victim.Team.Opponent();

                if (killer != null && killer.Kind == UnitKind.Hero &&
                    heroStats.TryGetValue(killer, out HeroStats killerStats))
                {
                    killerStats.Kills++;
                }

                if (killerTeam == Team.Blue) BlueKills++;
                else if (killerTeam == Team.Red) RedKills++;
            }

            if (victim.Kind != UnitKind.Structure) return;

            if (victim.IsPrimaryObjective)
            {
                EndMatch(victim.Team.Opponent());
                return;
            }

            Team credit = killer != null && killer.Team != Team.None
                ? killer.Team
                : victim.Team.Opponent();

            if (credit == Team.Blue) BlueTowers++;
            else if (credit == Team.Red) RedTowers++;
        }

        private void EndMatchByTime()
        {
            Team winner;

            if (BlueTowers != RedTowers)
            {
                winner = BlueTowers > RedTowers ? Team.Blue : Team.Red;
            }
            else if (BlueKills != RedKills)
            {
                winner = BlueKills > RedKills ? Team.Blue : Team.Red;
            }
            else
            {
                float blueDamage = 0f;
                float redDamage = 0f;

                foreach (HeroStats stats in heroStats.Values)
                {
                    if (stats.Team == Team.Blue) blueDamage += stats.DamageToHeroes;
                    else redDamage += stats.DamageToHeroes;
                }

                if (Mathf.Approximately(blueDamage, redDamage)) winner = Team.None;
                else winner = blueDamage > redDamage ? Team.Blue : Team.Red;
            }

            EndMatch(winner);
        }

        private void EndMatch(Team winner)
        {
            if (State == MatchState.Ended) return;

            if (State == MatchState.Playing)
            {
                ElapsedTime = Time.time - matchStartTime;
            }

            State = MatchState.Ended;
            Time.timeScale = 0f;

            var result = new MatchResult
            {
                Winner = winner,
                BlueKills = BlueKills,
                RedKills = RedKills,
                BlueTowers = BlueTowers,
                RedTowers = RedTowers,
                Duration = ElapsedTime
            };

            if (playerHero != null && heroStats.TryGetValue(playerHero, out HeroStats playerStats))
            {
                result.PlayerKills = playerStats.Kills;
                result.PlayerDeaths = playerStats.Deaths;
                result.PlayerDamage = playerStats.DamageToHeroes;
            }

            MatchEnded?.Invoke(result);
        }
    }
}
