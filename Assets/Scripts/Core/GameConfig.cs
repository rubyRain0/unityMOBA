using UnityEngine;

namespace Moba.Core
{
    /// <summary>
    /// Единая точка настройки прототипа. Все игровые числа живут здесь,
    /// чтобы баланс можно было править без поиска по коду.
    /// </summary>
    public static class GameConfig
    {
        // Карта
        public const int LaneCount = 3;
        public const float MapHalfSize = 50f;
        public const float WallInset = 47f;
        public const float BaseInset = 34f;
        public const float FountainRadius = 12f;
        public const float FountainHealPerSecond = 45f;

        // База
        public const float NexusRadius = 2.5f;

        // Герой
        public const float HeroMaxHealth = 800f;
        public const float HeroMaxMana = 300f;
        public const float HeroMoveSpeed = 7f;
        public const float HeroAttackRange = 7f;
        public const float HeroAttackDamage = 60f;
        public const float HeroAttackInterval = 0.8f;
        public const float HeroProjectileSpeed = 28f;
        public const float HeroManaRegenPerSecond = 8f;

        /// <summary>Множитель урона только для героя игрока.</summary>
        public const float PlayerDamageMultiplier = 10f;

        // Способности
        public const float SkillshotDamage = 130f;
        public const float SkillshotSpeed = 26f;
        public const float SkillshotRange = 14f;
        public const float SkillshotRadius = 0.8f;
        public const float DashDistance = 6f;
        public const float HealAbilityPercent = 0.25f;

        // ИИ героев
        public const float BotAggroRange = 12f;
        public const float BotRetreatHealthRatio = 0.3f;
        public const float BotRecoverHealthRatio = 0.7f;
        public const float BotRepathInterval = 0.3f;

        public const float HeroRespawnSeconds = 8f;

        // Миньоны и волны
        public const float MinionMaxHealth = 150f;
        public const float MinionMoveSpeed = 4.5f;
        public const float MinionAttackRange = 4.5f;
        public const float MinionAttackDamage = 16f;
        public const float MinionAttackInterval = 0.9f;
        public const float MinionProjectileSpeed = 22f;
        public const float MinionAggroRange = 9f;
        public const int MinionsPerWave = 3;
        public const float WaveInterval = 22f;
        public const float FirstWaveDelay = 3f;
        public const float MinionSpawnDistance = 8f;
        public const float MinionSpawnSpacing = 1.8f;

        // Башни и нексус
        public const float TowerMaxHealth = 1500f;
        public const float TowerAttackRange = 9f;
        public const float TowerAttackDamage = 40f;
        public const float TowerAttackInterval = 1.2f;
        public const float TowerProjectileSpeed = 25f;
        public const float TowerHeight = 5f;
        public const float NexusMaxHealth = 3200f;
        public const float NexusAttackRange = 8f;
        public const float NexusAttackDamage = 30f;
        public const float NexusAttackInterval = 1.5f;
        public const float NexusProjectileSpeed = 25f;

        // Камера
        public const float CameraHeight = 20f;
        public const float CameraBack = 15f;
        public const float CameraFov = 55f;

        public static readonly Vector3 BlueBasePosition = new(-BaseInset, 0f, -BaseInset);
        public static readonly Vector3 RedBasePosition = new(BaseInset, 0f, BaseInset);
    }
}
