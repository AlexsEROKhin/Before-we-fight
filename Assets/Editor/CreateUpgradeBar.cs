using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CreateUpgradeBar
{
    [MenuItem("Tools/Before We Fight/Add Q Skill to Arena")]
    public static void AddQSkill()
    {
        if (EditorApplication.isPlaying || SceneManager.GetActiveScene().name != "Arena")
        {
            Debug.LogError("Stop Play mode and open the Arena scene first.");
            return;
        }
        Scene scene = SceneManager.GetActiveScene();
        UpgradeBarUI bar = Object.FindFirstObjectByType<UpgradeBarUI>();
        TopDownPlayer player = Object.FindFirstObjectByType<TopDownPlayer>();
        if (bar == null || player == null)
        {
            Debug.LogError("The existing Upgrade Bar and player must be present in Arena.");
            return;
        }
        Transform root = bar.transform;
        Font font = null;
        foreach (Text existing in Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (existing.font != null) { font = existing.font; break; }
        if (font == null) { Debug.LogError("A UI font was not found."); return; }

        RectTransform frame = (RectTransform)root;
        frame.sizeDelta = new Vector2(450, 58);
        RectTransform wine = (RectTransform)root.Find("Wine border");
        RectTransform inset = (RectTransform)root.Find("Dark inset");
        if (wine != null) wine.sizeDelta = new Vector2(444, 52);
        if (inset != null) inset.sizeDelta = new Vector2(440, 48);
        if (root.Find("Skill separator") == null)
            ImageObject("Skill separator", root, new Vector2(330, 10),
                new Vector2(3, 38), new Color(0.62f, 0.39f, 0.38f));

        ActiveSkillBarUI skillBar = root.GetComponent<ActiveSkillBarUI>();
        if (skillBar == null) skillBar = root.gameObject.AddComponent<ActiveSkillBarUI>();
        PrepareSkillSprite("Assets/Art/Abilities/sword_spin_icon.png", 32f);
        PrepareSkillSprite("Assets/Art/Abilities/sword_wave_short.png", 96f);
        Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Abilities/sword_spin_icon.png");
        Image shade;
        Text countdown;
        CreateSkillSlot(root, "Q", 342, font, icon, out shade, out countdown);
        CreateSkillSlot(root, "R", 395, font, null, out _, out _);
        skillBar.Configure(shade, countdown);
        Transform qSlot = root.Find("Active Skill Q");
        Image qHitArea = qSlot.GetComponent<Image>();
        SerializedObject qGraphic = new SerializedObject(qHitArea);
        qGraphic.FindProperty("m_RaycastTarget").boolValue = true;
        qGraphic.ApplyModifiedPropertiesWithoutUndo();
        UpgradeSlotHover qHover = qSlot.GetComponent<UpgradeSlotHover>();
        if (qHover == null) qHover = qSlot.gameObject.AddComponent<UpgradeSlotHover>();
        qHover.ConfigureSkill(bar, "SWORD SPIN  Q",
            "Quick sword sweep around you. Cooldown: 7 seconds.");

        Sprite effectSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Abilities/sword_wave_short.png");
        Transform effectTransform = player.transform.Find("SwordWaveVisual") ??
            player.transform.Find("CircularSwordEffect");
        if (effectTransform == null)
        {
            GameObject effect = new GameObject("SwordWaveVisual", typeof(SpriteRenderer));
            effect.transform.SetParent(player.transform, false);
            effectTransform = effect.transform;
        }
        effectTransform.name = "SwordWaveVisual";
        SpriteRenderer effectRenderer = effectTransform.GetComponent<SpriteRenderer>();
        effectTransform.localPosition = Vector3.zero;
        effectTransform.localScale = Vector3.one;
        effectRenderer.sprite = effectSprite;
        effectRenderer.color = Color.white;
        effectRenderer.enabled = false;

        SerializedObject serialized = new SerializedObject(player);
        serialized.FindProperty("activeSkillBar").objectReferenceValue = skillBar;
        serialized.FindProperty("circularAttackEffect").objectReferenceValue = effectRenderer;
        serialized.FindProperty("circularAttackRadius").floatValue = 1.2f;
        serialized.FindProperty("circularAttackWindup").floatValue = 0.04f;
        serialized.FindProperty("circularAttackVisualSeconds").floatValue = 0.24f;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(player);
        EditorUtility.SetDirty(skillBar);
        EditorUtility.SetDirty(qHitArea);
        EditorUtility.SetDirty(qHover);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Updated the editable Q sword spin and short blade wave in Arena.");
    }

    [MenuItem("Tools/Before We Fight/Layout Reward Descriptions")]
    public static void LayoutRewardDescriptions()
    {
        if (EditorApplication.isPlaying || SceneManager.GetActiveScene().name != "Arena")
        {
            Debug.LogError("Stop Play mode and open the Arena scene first.");
            return;
        }
        ChestRewardUI rewardPanel = Object.FindFirstObjectByType<ChestRewardUI>(FindObjectsInactive.Include);
        if (rewardPanel == null) { Debug.LogError("Chest Rewards panel was not found."); return; }
        string[] names = { "Critical Sword Label (editable)", "Second Dash Label (editable)",
            "Kill Explosion Label (editable)" };
        foreach (string name in names)
        {
            Transform child = rewardPanel.transform.Find(name);
            if (child == null) { Debug.LogError("Missing reward label: " + name); return; }
            RectTransform rect = (RectTransform)child;
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -240f);
            rect.sizeDelta = new Vector2(390f, 72f);
            Text label = child.GetComponent<Text>();
            label.fontSize = 24;
            label.supportRichText = true;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            EditorUtility.SetDirty(rect);
            EditorUtility.SetDirty(label);
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Reward card descriptions now fit below their existing illustrations.");
    }

    private static void PrepareSkillSprite(string path, float pixelsPerUnit)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;
        if (importer.textureType == TextureImporterType.Sprite &&
            importer.spritePixelsPerUnit == pixelsPerUnit &&
            importer.filterMode == FilterMode.Point && !importer.mipmapEnabled) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
    }

    private static void CreateSkillSlot(Transform root, string key, int x, Font font,
        Sprite iconSprite, out Image shade, out Text countdown)
    {
        Transform existing = root.Find("Active Skill " + key);
        if (existing != null)
        {
            Text existingKey = existing.Find("Key " + key)?.GetComponent<Text>();
            if (existingKey != null) existingKey.text = key;
            Image existingIcon = existing.Find("Skill icon (editable)")?.GetComponent<Image>();
            if (existingIcon != null)
            {
                existingIcon.sprite = iconSprite;
                existingIcon.enabled = iconSprite != null;
            }
            shade = existing.Find("Cooldown shade")?.GetComponent<Image>();
            countdown = existing.Find("Cooldown seconds")?.GetComponent<Text>();
            return;
        }
        Image border = ImageObject("Active Skill " + key, root, new Vector2(x, 10),
            new Vector2(47, 38), new Color(0.72f, 0.57f, 0.33f));
        ImageObject("Skill recess", border.transform, new Vector2(2, 2),
            new Vector2(43, 34), new Color(0.055f, 0.045f, 0.065f));
        Image icon = ImageObject("Skill icon (editable)", border.transform,
            new Vector2(11, 4), new Vector2(31, 29), Color.white);
        icon.sprite = iconSprite;
        icon.preserveAspect = true;
        icon.enabled = iconSprite != null;
        shade = ImageObject("Cooldown shade", border.transform, new Vector2(2, 2),
            new Vector2(43, 34), new Color(0.02f, 0.02f, 0.04f, 0.74f));
        shade.type = Image.Type.Filled;
        shade.fillMethod = Image.FillMethod.Vertical;
        shade.fillOrigin = (int)Image.OriginVertical.Top;
        shade.fillAmount = 0f;
        shade.enabled = false;
        TextObject("Key " + key, border.transform, new Vector2(2, 1),
            new Vector2(12, 17), font, 13, TextAnchor.LowerLeft,
            new Color(1f, 0.84f, 0.52f)).text = key;
        countdown = TextObject("Cooldown seconds", border.transform,
            new Vector2(16, 8), new Vector2(27, 22), font, 15,
            TextAnchor.MiddleCenter, Color.white);
        countdown.enabled = false;
    }

    [MenuItem("Tools/Before We Fight/Repair Upgrade Bar Hover")]
    public static void RepairHover()
    {
        Canvas canvas = null;
        foreach (Canvas candidate in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (candidate.name == "Arena HUD Canvas") canvas = candidate;
        Transform barTransform = canvas != null ? canvas.transform.Find("Upgrade Bar (editable)") : null;
        if (barTransform == null) return;
        UpgradeBarUI bar = barTransform.GetComponent<UpgradeBarUI>();
        for (int i = 0; i < 6; i++)
        {
            Transform slot = barTransform.Find("Upgrade Slot " + (i + 1));
            if (slot == null) continue;
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(slot.gameObject);
            foreach (UpgradeSlotHover oldHover in slot.GetComponents<UpgradeSlotHover>())
                Object.DestroyImmediate(oldHover);
            UpgradeSlotHover hover = slot.gameObject.AddComponent<UpgradeSlotHover>();
            hover.Configure(bar, i);
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Repaired all six Upgrade Bar hover components.");
    }

    [MenuItem("Tools/Before We Fight/Create Upgrade Bar in Arena")]
    public static void Build()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Stop Play mode before editing the Arena HUD.");
            return;
        }
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "Arena")
        {
            Debug.LogError("Open the Arena scene first.");
            return;
        }
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        foreach (Canvas candidate in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (candidate.name == "Arena HUD Canvas") canvas = candidate;
        if (canvas == null || canvas.name != "Arena HUD Canvas")
        {
            Debug.LogError("Arena HUD Canvas was not found.");
            return;
        }
        if (canvas.transform.Find("Upgrade Bar (editable)") != null)
        {
            Debug.Log("Upgrade Bar already exists; it was not replaced.");
            return;
        }

        Font font = null;
        foreach (Text existing in Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (existing.font != null) { font = existing.font; break; }

        Image frame = ImageObject("Upgrade Bar (editable)", canvas.transform,
            new Vector2(22, 24), new Vector2(332, 58), new Color(0.08f, 0.055f, 0.075f));
        ImageObject("Wine border", frame.transform, new Vector2(3, 3),
            new Vector2(326, 52), new Color(0.42f, 0.19f, 0.25f));
        ImageObject("Dark inset", frame.transform, new Vector2(5, 5),
            new Vector2(322, 48), new Color(0.12f, 0.08f, 0.12f));
        UpgradeBarUI bar = frame.gameObject.AddComponent<UpgradeBarUI>();
        var slots = new UpgradeBarUI.Slot[6];
        for (int i = 0; i < slots.Length; i++)
        {
            Image border = ImageObject("Upgrade Slot " + (i + 1), frame.transform,
                new Vector2(9 + i * 53, 10), new Vector2(47, 38), new Color(0.55f, 0.34f, 0.35f));
            ImageObject("Slot recess", border.transform, new Vector2(2, 2),
                new Vector2(43, 34), new Color(0.055f, 0.045f, 0.065f));
            Image icon = ImageObject("Icon (editable)", border.transform, new Vector2(6, 4),
                new Vector2(35, 29), Color.white);
            icon.preserveAspect = true;
            icon.enabled = false;
            Text level = TextObject("Level", border.transform, new Vector2(27, 1),
                new Vector2(18, 16), font, 13, TextAnchor.LowerRight, Color.white);
            level.gameObject.SetActive(false);
            slots[i] = new UpgradeBarUI.Slot { icon = icon, level = level };
            border.gameObject.AddComponent<UpgradeSlotHover>().Configure(bar, i);
        }

        Image tooltipFrame = ImageObject("Upgrade tooltip (editable)", frame.transform,
            new Vector2(0, 66), new Vector2(294, 57), new Color(0.08f, 0.055f, 0.075f));
        ImageObject("Tooltip border", tooltipFrame.transform, new Vector2(2, 2),
            new Vector2(290, 53), new Color(0.42f, 0.19f, 0.25f));
        ImageObject("Tooltip inset", tooltipFrame.transform, new Vector2(4, 4),
            new Vector2(286, 49), new Color(0.055f, 0.045f, 0.065f));
        Text tooltipText = TextObject("Upgrade name and effect", tooltipFrame.transform,
            new Vector2(9, 7), new Vector2(275, 43), font, 13,
            TextAnchor.MiddleLeft, new Color(1f, 0.86f, 0.63f));

        bar.Configure(slots, new[]
        {
            Art(ChestUpgradeKind.Sword, "Assets/Art/ChestLoot/reward_sword.png", "SWORD", "+5% damage"),
            Art(ChestUpgradeKind.Boots, "Assets/Art/ChestLoot/reward_boots.png", "BOOTS", "+5% speed"),
            Art(ChestUpgradeKind.Health, "Assets/Art/ChestLoot/reward_health.png", "HEART", "+5% maximum HP"),
            Art(ChestUpgradeKind.CriticalSword, "Assets/Art/Abilities/critical_sword.png", "CRITICAL SWORD", "+20% chance of an extra hit"),
            Art(ChestUpgradeKind.SecondDash, "Assets/Art/Abilities/second_dash.png", "SECOND DASH", "Two dash charges"),
            Art(ChestUpgradeKind.KillExplosion, "Assets/Art/Abilities/kill_explosion_card.png", "KILL EXPLOSION", "Explosion after a kill"),
            Art(ChestUpgradeKind.WideSwing, "Assets/Art/Abilities/wide_swing.png", "WIDE SWING", "Wider sword arc and reach"),
            Art(ChestUpgradeKind.DashGuard, "Assets/Art/Abilities/dash_guard.png", "DASH GUARD", "Protection briefly after dashing"),
            Art(ChestUpgradeKind.BloodSword, "Assets/Art/Abilities/blood_sword.png", "BLOOD SWORD", "Heal every five sword hits"),
            Art(ChestUpgradeKind.SteelRhythm, "Assets/Art/Abilities/steel_rhythm.png", "STEEL RHYTHM", "Sword attacks 10% faster per level"),
            Art(ChestUpgradeKind.IronSkin, "Assets/Art/Abilities/iron_skin.png", "IRON SKIN", "Take 10% less damage per level"),
            Art(ChestUpgradeKind.Momentum, "Assets/Art/Abilities/momentum.png", "MOMENTUM", "Move 15% faster for 3s after a kill"),
            Art(ChestUpgradeKind.SpinMastery, "Assets/Art/Abilities/spin_mastery.png", "SPIN MASTERY", "Q gains reach and 1s less cooldown")
        }, tooltipFrame.gameObject, tooltipText);
        tooltipFrame.gameObject.SetActive(false);

        foreach (TopDownPlayer player in Object.FindObjectsByType<TopDownPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            SerializedObject serialized = new SerializedObject(player);
            serialized.FindProperty("upgradeBar").objectReferenceValue = bar;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(player);
        }
        EditorUtility.SetDirty(bar);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Created editable Upgrade Bar in Arena HUD Canvas.");
    }

    private static UpgradeBarUI.UpgradeArt Art(ChestUpgradeKind kind, string path,
        string title, string description) => new UpgradeBarUI.UpgradeArt
    {
        kind = kind,
        icon = AssetDatabase.LoadAssetAtPath<Sprite>(path),
        title = title,
        description = description
    };

    private static Image ImageObject(string name, Transform parent, Vector2 position,
        Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = name.StartsWith("Upgrade Slot");
        return image;
    }

    private static Text TextObject(string name, Transform parent, Vector2 position,
        Vector2 size, Font font, int fontSize, TextAnchor alignment, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Text text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }
}
