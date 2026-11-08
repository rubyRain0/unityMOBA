using UnityEngine;
using UnityEngine.UI;
using Moba.Abilities;
using Moba.Core;
using Moba.Units;

namespace Moba.UI
{
    /// <summary>
    /// Минимальный HUD: здоровье и мана героя, подсказка по управлению.
    /// Собирается кодом, чтобы не зависеть от префабов и Canvas в сцене.
    /// </summary>
    public class Hud : MonoBehaviour
    {
        private Hero hero;
        private Font font;
        private RectTransform hpFill;
        private RectTransform manaFill;
        private Text hpText;
        private Text manaText;
        private GameObject deathPanel;
        private Text deathKillerText;
        private Text deathTimerText;
        private AbilitySystem abilitySystem;
        private Image[] abilityImages;
        private Text[] abilityCooldownTexts;

        public static Hud Create(Hero hero)
        {
            var go = new GameObject("HUD");

            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();

            var hud = go.AddComponent<Hud>();
            hud.hero = hero;
            hud.Build();
            return hud;
        }

        private void Build()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            RectTransform panel = CreatePanel(
                transform, "StatusPanel",
                new Vector2(0.5f, 0f), new Vector2(440f, 138f), new Vector2(0f, 110f),
                new Color(0.04f, 0.05f, 0.07f, 0.85f));

            hpFill = CreateBar(panel, "HP", new Vector2(16f, -16f), new Vector2(408f, 32f), new Color(0.8f, 0.18f, 0.18f), out hpText);
            manaFill = CreateBar(panel, "Mana", new Vector2(16f, -56f), new Vector2(408f, 24f), new Color(0.2f, 0.45f, 1f), out manaText);

            Text hint = CreateText(
                transform, "Hint",
                "ПКМ — движение / атака      Q/W/E — способности      Колесо мыши — зум",
                20, TextAnchor.UpperLeft);
            RectTransform hintRect = hint.rectTransform;
            hintRect.SetParent(transform, false);
            hintRect.anchorMin = new Vector2(0f, 1f);
            hintRect.anchorMax = new Vector2(0f, 1f);
            hintRect.pivot = new Vector2(0f, 1f);
            hintRect.anchoredPosition = new Vector2(24f, -24f);
            hintRect.sizeDelta = new Vector2(900f, 40f);

            abilitySystem = hero != null ? hero.GetComponent<AbilitySystem>() : null;
            BuildDeathPanel();
            BuildAbilityBar();
            UpdateBars();
        }

        private void Update()
        {
            UpdateBars();
            UpdateDeathPanel();
            UpdateAbilityBar();
        }

        private void BuildDeathPanel()
        {
            RectTransform panel = CreatePanel(
                transform, "DeathPanel",
                new Vector2(0.5f, 0.5f), new Vector2(620f, 176f), Vector2.zero,
                new Color(0.08f, 0.02f, 0.02f, 0.88f));
            deathPanel = panel.gameObject;

            Text title = CreateText(panel, "Title", "Вы погибли", 38, TextAnchor.MiddleCenter);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -18f);
            titleRect.sizeDelta = new Vector2(0f, 46f);
            title.fontStyle = FontStyle.Bold;
            title.color = new Color(1f, 0.45f, 0.4f);

            deathKillerText = CreateText(panel, "Killer", string.Empty, 22, TextAnchor.MiddleCenter);
            RectTransform killerRect = deathKillerText.rectTransform;
            killerRect.anchorMin = new Vector2(0f, 1f);
            killerRect.anchorMax = new Vector2(1f, 1f);
            killerRect.pivot = new Vector2(0.5f, 1f);
            killerRect.anchoredPosition = new Vector2(0f, -70f);
            killerRect.sizeDelta = new Vector2(0f, 30f);

            deathTimerText = CreateText(panel, "Timer", string.Empty, 26, TextAnchor.MiddleCenter);
            RectTransform timerRect = deathTimerText.rectTransform;
            timerRect.anchorMin = new Vector2(0f, 0f);
            timerRect.anchorMax = new Vector2(1f, 0f);
            timerRect.pivot = new Vector2(0.5f, 0f);
            timerRect.anchoredPosition = new Vector2(0f, 18f);
            timerRect.sizeDelta = new Vector2(0f, 34f);
            deathTimerText.color = new Color(1f, 0.9f, 0.7f);

            deathPanel.SetActive(false);
        }

        private void UpdateDeathPanel()
        {
            if (deathPanel == null || hero == null) return;

            bool dead = !hero.IsAlive;
            if (deathPanel.activeSelf != dead) deathPanel.SetActive(dead);
            if (!dead) return;

            float remaining = Mathf.Max(0f, hero.RespawnEndTime - Time.time);
            deathTimerText.text = $"Возрождение через {remaining:0.0} с";
            deathKillerText.text = KillerLabel(hero.LastKillerKind, hero.LastKillerWasNexus);
        }

        private static string KillerLabel(UnitKind kind, bool nexus)
        {
            return kind switch
            {
                UnitKind.Structure => nexus ? "Убийца: нексус противника" : "Убийца: башня противника",
                UnitKind.Minion => "Убийца: миньон противника",
                UnitKind.Hero => "Убийца: герой противника",
                _ => "Причина смерти неизвестна"
            };
        }

        private void BuildAbilityBar()
        {
            if (abilitySystem == null) return;

            abilityImages = new Image[3];
            abilityCooldownTexts = new Text[3];

            for (int i = 0; i < 3; i++)
            {
                Ability ability = abilitySystem.GetAbility(i);
                if (ability == null) continue;

                RectTransform box = CreatePanel(
                    transform, "Ability_" + ability.Key,
                    new Vector2(0.5f, 0f), new Vector2(72f, 72f), new Vector2((i - 1) * 84f, 24f),
                    new Color(ability.Color.r, ability.Color.g, ability.Color.b, 0.85f));

                abilityImages[i] = box.GetComponent<Image>();

                Text keyText = CreateText(box, "Key", ability.Key.ToString(), 28, TextAnchor.MiddleCenter);
                RectTransform keyRect = keyText.rectTransform;
                keyRect.anchorMin = Vector2.zero;
                keyRect.anchorMax = Vector2.one;
                keyRect.offsetMin = Vector2.zero;
                keyRect.offsetMax = Vector2.zero;

                Text cooldownText = CreateText(box, "CD", string.Empty, 18, TextAnchor.LowerCenter);
                RectTransform cooldownRect = cooldownText.rectTransform;
                cooldownRect.anchorMin = new Vector2(0f, 0f);
                cooldownRect.anchorMax = new Vector2(1f, 0f);
                cooldownRect.pivot = new Vector2(0.5f, 0f);
                cooldownRect.anchoredPosition = new Vector2(0f, 4f);
                cooldownRect.sizeDelta = new Vector2(0f, 20f);
                abilityCooldownTexts[i] = cooldownText;
            }
        }

        private void UpdateAbilityBar()
        {
            if (abilitySystem == null || abilityImages == null) return;

            for (int i = 0; i < abilityImages.Length; i++)
            {
                if (abilityImages[i] == null) continue;

                Ability ability = abilitySystem.GetAbility(i);
                float cooldown = abilitySystem.GetCooldown(i);
                bool onCooldown = cooldown > 0.05f;

                abilityCooldownTexts[i].text = onCooldown ? cooldown.ToString("0.0") : string.Empty;

                Color color = ability.Color;
                abilityImages[i].color = new Color(
                    color.r, color.g, color.b,
                    onCooldown ? 0.25f : 0.85f);
            }
        }

        private void UpdateBars()
        {
            if (hero == null || hpFill == null) return;

            SetFill(hpFill, hero.MaxHealth > 0f ? hero.Health / hero.MaxHealth : 0f);
            hpText.text = $"HP  {Mathf.CeilToInt(hero.Health)} / {Mathf.CeilToInt(hero.MaxHealth)}";

            SetFill(manaFill, hero.MaxMana > 0f ? hero.Mana / hero.MaxMana : 0f);
            manaText.text = $"Мана  {Mathf.CeilToInt(hero.Mana)} / {Mathf.CeilToInt(hero.MaxMana)}";
        }

        private static void SetFill(RectTransform fill, float ratio)
        {
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(Mathf.Clamp01(ratio), 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
        }

        private RectTransform CreatePanel(
            Transform parent,
            string name,
            Vector2 anchor,
            Vector2 size,
            Vector2 anchoredPosition,
            Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        private RectTransform CreateBar(
            RectTransform parent,
            string name,
            Vector2 topLeft,
            Vector2 size,
            Color color,
            out Text label)
        {
            var bgGo = new GameObject(name + "_BG", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var bg = (RectTransform)bgGo.transform;
            bg.SetParent(parent, false);
            bg.anchorMin = new Vector2(0f, 1f);
            bg.anchorMax = new Vector2(0f, 1f);
            bg.pivot = new Vector2(0f, 1f);
            bg.anchoredPosition = topLeft;
            bg.sizeDelta = size;

            Image bgImage = bgGo.GetComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.6f);
            bgImage.raycastTarget = false;

            var fillGo = new GameObject(name + "_Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var fill = (RectTransform)fillGo.transform;
            fill.SetParent(bg, false);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;

            Image fillImage = fillGo.GetComponent<Image>();
            fillImage.color = color;
            fillImage.raycastTarget = false;

            label = CreateText(bg, name + "_Text", string.Empty, 18, TextAnchor.MiddleCenter);
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            return fill;
        }

        private Text CreateText(Transform parent, string name, string content, int size, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);

            Text text = go.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = anchor;
            text.raycastTarget = false;
            text.text = content;
            return text;
        }
    }
}
