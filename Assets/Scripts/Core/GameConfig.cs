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

        // Манекены для проверки боя
        public const float DummyMaxHealth = 300f;
        public const float DummyRespawnSeconds = 3f;

        // Камера
        public const float CameraHeight = 20f;
        public const float CameraBack = 15f;
        public const float CameraFov = 55f;

        public static readonly Vector3 BlueBasePosition = new(-BaseInset, 0f, -BaseInset);
        public static readonly Vector3 RedBasePosition = new(BaseInset, 0f, BaseInset);
    }
}
