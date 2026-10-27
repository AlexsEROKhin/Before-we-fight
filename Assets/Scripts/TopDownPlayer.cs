using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public sealed class TopDownPlayer : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0)] private float moveSpeed = 3f;
    [SerializeField] private string menuScene = "Title";
    [Header("Editable animation clips")]
    [SerializeField] private AnimationClip idle;
    [SerializeField] private AnimationClip walk;
    [SerializeField] private AnimationClip jump;
    [SerializeField] private AnimationClip land;
    [SerializeField] private AnimationClip dash;
    [SerializeField] private AnimationClip hurt1;
    [SerializeField] private AnimationClip hurt2;
    [SerializeField] private AnimationClip death;
    [Header("Up and down facing")]
    [SerializeField] private AnimationClip idleUp;
    [SerializeField] private AnimationClip walkUp;
    [SerializeField] private AnimationClip idleDown;
    [SerializeField] private AnimationClip walkDown;
    [Header("Diagonal facing (right sprites are mirrored for left)")]
    [SerializeField] private AnimationClip walkUpRight;
    [SerializeField] private AnimationClip walkDownRight;
    [Header("Sword attack toward mouse cursor")]
    [SerializeField, Min(0.1f)] private float attackReach = 0.65f;
    [SerializeField, Min(0.1f)] private float attackRadius = 0.3f;
    [SerializeField, Range(30f, 180f)] private float swordArcDegrees = 115f;
    [SerializeField, Min(0.05f)] private float swordSwingSeconds = 0.14f;
    [SerializeField, Min(0f), Tooltip("Recovery after the swing. Total attack interval = swing duration + recovery.")]
    private float swordCooldown = 0.4f;
    [SerializeField, Min(0f)] private float swordGripOffset = 0.12f;
    [SerializeField, Min(0f)] private float swordTrailFadeSeconds = 0.06f;
    [SerializeField, Range(0f, 1f)] private float swordTrailOpacity = 0.65f;
    [Header("Chest abilities")]
    [SerializeField, Range(0f, 1f)] private float criticalChancePerUpgrade = 0.2f;
    [SerializeField] private Color criticalSwordTint = new Color(1f, 0.83f, 0.32f, 1f);
    [SerializeField, Min(0.1f)] private float killExplosionRadius = 1.5f;
    [SerializeField, Min(0f)] private float killExplosionCooldown = 3f;
    [SerializeField] private Sprite killExplosionSprite;
    [SerializeField, Min(0.05f)] private float killExplosionVisualSeconds = 0.2f;
    [SerializeField, Range(1f, 30f)] private float wideSwingArcPerUpgrade = 15f;
    [SerializeField, Range(0.01f, 0.2f)] private float wideSwingRadiusPerUpgrade = 0.05f;
    [SerializeField, Range(0.1f, 1f)] private float dashGuardSecondsPerUpgrade = 0.45f;
    [SerializeField, Min(1)] private int bloodSwordHitsPerHeal = 5;
    [SerializeField, Min(1)] private int bloodSwordHealPerUpgrade = 8;
    [SerializeField, Range(0.01f, 0.2f)] private float steelRhythmSpeedPerUpgrade = 0.1f;
    [SerializeField, Range(0.01f, 0.2f)] private float ironSkinReductionPerUpgrade = 0.1f;
    [SerializeField, Min(0.1f)] private float momentumDuration = 3f;
    [SerializeField, Range(0.01f, 0.3f)] private float momentumSpeedPerUpgrade = 0.15f;
    [SerializeField, Min(0.01f)] private float spinRadiusPerUpgrade = 0.2f;
    [SerializeField, Min(0.1f)] private float spinCooldownReductionPerUpgrade = 1f;
    [SerializeField] private UpgradeBarUI upgradeBar;
    [Header("Q close sword spin and short blade wave")]
    [SerializeField] private KeyCode circularAttackKey = KeyCode.Q;
    [SerializeField, Min(0.1f)] private float circularAttackRadius = 1.2f;
    [SerializeField, Min(0f)] private float circularAttackWindup = 0.04f;
    [SerializeField, Min(0.05f)] private float circularAttackVisualSeconds = 0.24f;
    [SerializeField, Min(0.1f)] private float circularAttackCooldown = 7f;
    [SerializeField] private SpriteRenderer circularAttackEffect;
    [SerializeField] private ActiveSkillBarUI activeSkillBar;
    [Header("Health: 62 artwork segments, initially 10 HP each")]
    [SerializeField, Min(1)] private int healthSegments = 62;
    [SerializeField, Min(1)] private int healthPerSegment = 10;
    [SerializeField, Min(0)] private int healthPerSegmentPerLevel = 1;
    [SerializeField, Min(0)] private int healthOnEnemyKill = 20;
    [SerializeField, Min(0)] private float damageImmunitySeconds = 0.8f;
    [SerializeField, Min(0f)] private float hurtFlashSeconds = 0.22f;
    [SerializeField] private Color hurtFlashColor = new Color(1f, 0.55f, 0.55f, 1f);
    [SerializeField] private Image healthFill;
    [SerializeField] private Text healthLabel;
    [SerializeField] private Image healthSpriteImage;
    [SerializeField] private SpriteFrameSet healthFrames;
    [Header("Experience")]
    [SerializeField, Min(1)] private int experienceToLevel = 8;
    [SerializeField] private Image experienceSpriteImage;
    [SerializeField] private SpriteFrameSet experienceFrames;
    [Header("Defeat")]
    [SerializeField] private GameObject gameOverPanel;
    [Header("Dash")]
    [SerializeField, Min(0.1f)] private float dashSpeed = 8f;
    [SerializeField, Min(0.01f)] private float dashCooldown = 3f;
    [SerializeField, Min(0.01f)] private float dashSpendSeconds = 0.15f;
    [SerializeField] private Image dashSpriteImage;
    [SerializeField] private SpriteFrameSet dashFrames;
    [SerializeField] private Color twoDashChargesTint = new Color(0.42f, 0.94f, 1f, 1f);
    [Header("Controls: WASD / arrows, J / left click (sword), Space, Left Shift")]
    [SerializeField] private KeyCode hitKey = KeyCode.J;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;
    private Rigidbody2D body;
    private SpriteRenderer sprite;
    private Animator animationPlayer;
    private Vector2 input;
    private AnimationClip current;
    private float actionEndsAt;
    private float nextDamageAt;
    private Vector2 facing = Vector2.right;
    private int health;
    private float externalControlUntil;
    private float slowUntil;
    private float slowMultiplier = 1f;
    private readonly Dictionary<MonoBehaviour, float> areaSlowSources = new();
    private readonly List<TimedSlow> lingeringSlows = new();
    private struct TimedSlow
    {
        public float Multiplier;
        public float Until;
    }
    private int experience;
    private int level = 1;
    private Transform animatedVisual;
    private Coroutine visualJump;
    private bool facingVertically;
    private bool facingDiagonal;
    private bool facingUp;
    private float dashEndsAt;
    private float dashStartedAt;
    private float nextDashAt;
    private Vector2 dashDirection;
    private SpriteRenderer swordVisual;
    private SpriteRenderer swordTrail;
    private Coroutine swordSwing;
    private float nextSwordAt;
    private Vector3 visualRestPosition;
    private Color normalSpriteColor;
    private Coroutine hurtFlash;
    private float nextFootstepAt;
    private bool alternateFootstep;
    private bool swordHitSoundPlayed;
    private float swordDamageBonus;
    private float swordBonusHitProgress;
    private float movementSpeedBonus;
    private float maximumHealthBonus;
    private float criticalChance;
    private bool criticalSwing;
    private Color swordOriginalColor;
    private Color trailOriginalColor;
    private int maximumDashCharges = 1;
    private int dashCharges = 1;
    private int killExplosionLevel;
    private float nextKillExplosionAt;
    private int wideSwingLevel;
    private int dashGuardLevel;
    private int bloodSwordLevel;
    private int bloodSwordHitProgress;
    private int steelRhythmLevel;
    private int ironSkinLevel;
    private int momentumLevel;
    private int spinMasteryLevel;
    private float momentumEndsAt;
    private float nextCircularAttackAt;
    private Coroutine circularAttackRoutine;

    public bool IsAlive => health > 0;
    public int CurrentHealth => health;
    public int MaximumHealth => MaxHealth;
    private int MaxHealth => Mathf.RoundToInt(healthSegments * healthPerSegment * (1f + maximumHealthBonus));
    public float SwordDamageMultiplier => 1f + swordDamageBonus;
    public float MovementSpeedMultiplier => 1f + movementSpeedBonus;
    private float EffectiveSwordSwingSeconds => swordSwingSeconds * Mathf.Max(0.6f, 1f - steelRhythmLevel * steelRhythmSpeedPerUpgrade);
    private float EffectiveSpinCooldown => Mathf.Max(3f, circularAttackCooldown - spinMasteryLevel * spinCooldownReductionPerUpgrade);
    private float EffectiveSpinRadius => circularAttackRadius + spinMasteryLevel * spinRadiusPerUpgrade;

    public void ApplyChestUpgrade(ChestUpgradeKind upgrade)
    {
        switch (upgrade)
        {
            case ChestUpgradeKind.Sword:
                swordDamageBonus += 0.05f;
                break;
            case ChestUpgradeKind.Boots:
                movementSpeedBonus += 0.05f;
                break;
            case ChestUpgradeKind.Health:
                int previousMaximum = MaxHealth;
                maximumHealthBonus += 0.05f;
                health += MaxHealth - previousMaximum;
                UpdateHealthDisplay();
                break;
            case ChestUpgradeKind.CriticalSword:
                criticalChance = Mathf.Min(0.6f, criticalChance + criticalChancePerUpgrade);
                break;
            case ChestUpgradeKind.SecondDash:
                maximumDashCharges = 2;
                dashCharges = 2;
                UpdateDashDisplay();
                break;
            case ChestUpgradeKind.KillExplosion:
                killExplosionLevel++;
                break;
            case ChestUpgradeKind.WideSwing:
                wideSwingLevel = Mathf.Min(3, wideSwingLevel + 1);
                break;
            case ChestUpgradeKind.DashGuard:
                dashGuardLevel = Mathf.Min(2, dashGuardLevel + 1);
                break;
            case ChestUpgradeKind.BloodSword:
                bloodSwordLevel = Mathf.Min(3, bloodSwordLevel + 1);
                break;
            case ChestUpgradeKind.SteelRhythm:
                steelRhythmLevel = Mathf.Min(3, steelRhythmLevel + 1);
                break;
            case ChestUpgradeKind.IronSkin:
                ironSkinLevel = Mathf.Min(3, ironSkinLevel + 1);
                break;
            case ChestUpgradeKind.Momentum:
                momentumLevel = Mathf.Min(3, momentumLevel + 1);
                break;
            case ChestUpgradeKind.SpinMastery:
                spinMasteryLevel = Mathf.Min(3, spinMasteryLevel + 1);
                break;
        }
        if (upgradeBar == null) upgradeBar = FindFirstObjectByType<UpgradeBarUI>();
        if (upgradeBar != null) upgradeBar.RecordUpgrade(upgrade);
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        Transform playerVisual = transform.Find("PlayerVisual");
        if (playerVisual != null) sprite = playerVisual.GetComponent<SpriteRenderer>();
        if (sprite == null)
        {
            foreach (SpriteRenderer candidate in GetComponentsInChildren<SpriteRenderer>(true))
                if (candidate.enabled && candidate.gameObject.name != "Shadow") { sprite = candidate; break; }
        }
        if (sprite == null) sprite = GetComponent<SpriteRenderer>();
        normalSpriteColor = sprite.color;
        animatedVisual = playerVisual != null ? playerVisual : (sprite != null ? sprite.transform : transform);
        visualRestPosition = animatedVisual.localPosition;
        Transform sword = transform.Find("SwordVisual");
        if (sword != null)
        {
            swordVisual = sword.GetComponent<SpriteRenderer>();
            if (swordVisual != null) { swordOriginalColor = swordVisual.color; swordVisual.enabled = false; }
        }
        Transform trail = transform.Find("SwordTrail");
        if (trail != null)
        {
            swordTrail = trail.GetComponent<SpriteRenderer>();
            if (swordTrail != null) { trailOriginalColor = swordTrail.color; swordTrail.enabled = false; }
        }
        animationPlayer = GetComponent<Animator>();
        if (walkUpRight == null) walkUpRight = FindControllerClip("NewHero_Walk_UpRight");
        if (walkDownRight == null) walkDownRight = FindControllerClip("NewHero_Walk_DownRight");
        health = MaxHealth;
        UpdateHealthDisplay();
        UpdateExperienceDisplay();
        UpdateDashDisplay();
        if (circularAttackEffect != null) circularAttackEffect.enabled = false;
        if (activeSkillBar == null) activeSkillBar = FindFirstObjectByType<ActiveSkillBarUI>();
        if (activeSkillBar != null) activeSkillBar.SetQCooldown(0f, circularAttackCooldown);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        Animation oldPreview = GetComponent<Animation>();
        if (oldPreview != null) { oldPreview.Stop(); oldPreview.enabled = false; }
        Play(idle);
    }

    private void Update()
    {
        if (ChestRewardUI.IsOpen)
        {
            input = Vector2.zero;
            body.linearVelocity = Vector2.zero;
            return;
        }
        input = IsAlive
            ? Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f)
            : Vector2.zero;
        // Keep the visual facing locked during a dash or a visual jump.
        if (visualJump == null && Time.time >= actionEndsAt && input.sqrMagnitude > 0.001f)
        {
            facing = input.normalized;
            if (Mathf.Abs(input.x) > 0.01f && Mathf.Abs(input.y) > 0.01f)
            {
                facingDiagonal = true;
                facingVertically = false;
                facingUp = input.y > 0f;
                sprite.flipX = input.x < 0f;
            }
            else if (Mathf.Abs(input.y) > 0.01f)
            {
                facingDiagonal = false;
                facingVertically = true;
                facingUp = input.y > 0f;
                sprite.flipX = false;
            }
            else if (Mathf.Abs(input.x) > 0.01f)
            {
                facingDiagonal = false;
                facingVertically = false;
                sprite.flipX = input.x < 0f;
            }
        }
        bool moving = input.sqrMagnitude > 0.001f;
        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (IsAlive && Input.GetKeyDown(circularAttackKey) && circularAttackRoutine == null &&
            swordSwing == null &&
            Time.time >= nextCircularAttackAt && Time.time >= actionEndsAt &&
            visualJump == null && Time.time >= dashEndsAt)
            circularAttackRoutine = StartCoroutine(CircularAttackRoutine());
        bool attackPressed = Input.GetKeyDown(hitKey) || (!overUI && Input.GetMouseButtonDown(0));
        if (IsAlive && attackPressed && swordVisual != null && swordSwing == null &&
            circularAttackRoutine == null && Time.time >= nextSwordAt &&
            (Time.time >= actionEndsAt || visualJump != null))
            BeginSwordSwing();
        if (IsAlive && visualJump == null && Time.time >= actionEndsAt)
        {
            if (Input.GetKeyDown(dashKey) && dash != null && dashCharges > 0)
                StartDash();
            else if (Input.GetKeyDown(jumpKey) && jump != null)
                StartJump();
            else if (moving)
                Play((facingDiagonal
                    ? (facingUp ? (walkUpRight ?? walkUp) : (walkDownRight ?? walkDown))
                    : facingVertically ? (facingUp ? walkUp : walkDown) : walk) ?? walk);
            else
                Play(((facingDiagonal || facingVertically) ? (facingUp ? idleUp : idleDown) : idle) ?? idle);
        }
        if (IsAlive && moving && visualJump == null && Time.time >= actionEndsAt &&
            Time.time >= dashEndsAt && body.linearVelocity.sqrMagnitude > 0.25f &&
            Time.time >= nextFootstepAt)
        {
            GameSfx.PlayHero(alternateFootstep ? SfxCue.StepB : SfxCue.StepA);
            alternateFootstep = !alternateFootstep;
            nextFootstepAt = Time.time + 0.3f;
        }
        RechargeDash();
        UpdateDashDisplay();
        if (activeSkillBar != null)
            activeSkillBar.SetQCooldown(Mathf.Max(0f, nextCircularAttackAt - Time.time), EffectiveSpinCooldown);
        if (Input.GetKeyDown(KeyCode.Escape) && Application.CanStreamedLevelBeLoaded(menuScene))
            SceneManager.LoadScene(menuScene);
    }

    private void FixedUpdate()
    {
        if (Time.time < dashEndsAt)
        {
            body.linearVelocity = dashDirection * dashSpeed;
            return;
        }
        if (Time.time < externalControlUntil) return;
        if (Time.time >= slowUntil) slowMultiplier = 1f;
        float areaSlow = 1f;
        foreach (KeyValuePair<MonoBehaviour, float> source in areaSlowSources)
            if (source.Key != null && source.Key.isActiveAndEnabled)
                areaSlow = Mathf.Min(areaSlow, source.Value);
        for (int i = lingeringSlows.Count - 1; i >= 0; i--)
        {
            TimedSlow effect = lingeringSlows[i];
            if (Time.time >= effect.Until) lingeringSlows.RemoveAt(i);
            else areaSlow = Mathf.Min(areaSlow, effect.Multiplier);
        }
        float momentum = Time.time < momentumEndsAt ? 1f + momentumLevel * momentumSpeedPerUpgrade : 1f;
        body.linearVelocity = input * moveSpeed * MovementSpeedMultiplier * momentum * Mathf.Min(slowMultiplier, areaSlow);
    }

    public void ApplySlow(float multiplier, float duration)
    {
        slowMultiplier = Mathf.Clamp(multiplier, 0.05f, 1f);
        slowUntil = Mathf.Max(slowUntil, Time.time + duration);
    }

    // Area slows are source-owned, do not stack, and never affect the dash branch above.
    public void SetAreaSlow(MonoBehaviour source, float multiplier)
    {
        if (source != null) areaSlowSources[source] = Mathf.Clamp(multiplier, 0.05f, 1f);
    }

    public void RemoveAreaSlow(MonoBehaviour source)
    {
        if (source != null) areaSlowSources.Remove(source);
    }

    // Starts when leaving a puddle; multiple traps use the strongest slow, not multiplied speed loss.
    public void ApplyLingeringSlow(float multiplier, float duration)
    {
        if (duration <= 0f || !IsAlive) return;
        lingeringSlows.Add(new TimedSlow
        {
            Multiplier = Mathf.Clamp(multiplier, 0.05f, 1f),
            Until = Time.time + duration
        });
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0 || !IsAlive) return;
        experience += amount;
        while (experience >= experienceToLevel)
        {
            experience -= experienceToLevel;
            level++;
            experienceToLevel = Mathf.CeilToInt(experienceToLevel * 1.25f);
            int oldMaxHealth = MaxHealth;
            healthPerSegment += healthPerSegmentPerLevel;
            health += MaxHealth - oldMaxHealth;
        }
        UpdateHealthDisplay();
        UpdateExperienceDisplay();
    }

    public void RewardEnemyDefeat(int experience)
    {
        if (!IsAlive) return;
        if (momentumLevel > 0) momentumEndsAt = Time.time + momentumDuration;
        AddExperience(experience);
        if (healthOnEnemyKill > 0 && health < MaxHealth)
        {
            health = Mathf.Min(MaxHealth, health + healthOnEnemyKill);
            UpdateHealthDisplay();
        }
        if (killExplosionLevel > 0 && Time.time >= nextKillExplosionAt)
        {
            nextKillExplosionAt = Time.time + killExplosionCooldown;
            ExplodeOnKill();
        }
    }

    private void UpdateExperienceDisplay()
    {
        if (experienceSpriteImage == null || experienceFrames == null || experienceFrames.Count == 0) return;
        float progress = experienceToLevel > 0 ? (float)experience / experienceToLevel : 0f;
        int frame = Mathf.RoundToInt(progress * (experienceFrames.Count - 1));
        experienceSpriteImage.sprite = experienceFrames.GetFrame(frame);
    }

    public void PullToward(Vector2 destination, float duration)
    {
        if (!IsAlive || duration <= 0f) return;
        StartCoroutine(PullRoutine(destination, duration));
    }

    private System.Collections.IEnumerator PullRoutine(Vector2 destination, float duration)
    {
        externalControlUntil = Time.time + duration;
        actionEndsAt = Mathf.Max(actionEndsAt, externalControlUntil);
        input = Vector2.zero;
        body.linearVelocity = Vector2.zero;
        Vector2 start = body.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            body.MovePosition(Vector2.Lerp(start, destination, Mathf.Clamp01(elapsed / duration)));
            yield return new WaitForFixedUpdate();
        }
    }

    private void StartAction(AnimationClip clip)
    {
        current = null;
        Play(clip);
        actionEndsAt = Time.time + clip.length;
    }

    private void BeginSwordSwing()
    {
        Camera gameCamera = Camera.main;
        if (gameCamera == null) return;
        Vector3 cursor = gameCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aim = (Vector2)(cursor - transform.position);
        if (aim.sqrMagnitude < 0.0001f) aim = facing.sqrMagnitude > 0f ? facing : Vector2.right;
        aim.Normalize();
        facing = aim;
        facingVertically = Mathf.Abs(aim.y) > Mathf.Abs(aim.x);
        facingDiagonal = false;
        facingUp = aim.y > 0f;
        sprite.flipX = aim.x < -0.01f;
        if (visualJump == null)
        {
            current = null;
            Play(facingVertically ? (facingUp ? idleUp : idleDown) ?? idle : idle);
        }
        float swingSeconds = EffectiveSwordSwingSeconds;
        actionEndsAt = Mathf.Max(actionEndsAt, Time.time + swingSeconds);
        nextSwordAt = Time.time + swingSeconds + swordCooldown * (swingSeconds / swordSwingSeconds);
        criticalSwing = criticalChance > 0f && Random.value < criticalChance;
        swordVisual.color = criticalSwing ? criticalSwordTint : swordOriginalColor;
        if (swordTrail != null) swordTrail.color = criticalSwing ? criticalSwordTint : trailOriginalColor;
        swordHitSoundPlayed = false;
        GameSfx.PlayHero(SfxCue.SwordWhoosh);
        swordSwing = StartCoroutine(SwordSwingRoutine(aim));
    }

    private IEnumerator SwordSwingRoutine(Vector2 aim)
    {
        swordVisual.enabled = true;
        bool swingBehindPlayer = aim.y > 0.35f;
        swordVisual.sortingOrder = sprite.sortingOrder + (swingBehindPlayer ? -1 : 2);
        if (swordTrail != null)
        {
            swordTrail.enabled = false;
            swordTrail.sortingOrder = sprite.sortingOrder + (swingBehindPlayer ? -2 : 1);
        }
        float aimAngle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg - 90f;
        float elapsed = 0f;
        var hitTargets = new HashSet<Collider2D>();
        float swingSeconds = EffectiveSwordSwingSeconds;
        while (elapsed < swingSeconds)
        {
            float progress = Mathf.Clamp01(elapsed / swingSeconds);
            float effectiveArc = Mathf.Min(175f, swordArcDegrees + wideSwingLevel * wideSwingArcPerUpgrade);
            float swingAngle = Mathf.Lerp(-effectiveArc * 0.5f, effectiveArc * 0.5f, progress);
            float worldAngle = aimAngle + swingAngle;
            Vector2 bladeDirection = new Vector2(
                Mathf.Cos((worldAngle + 90f) * Mathf.Deg2Rad),
                Mathf.Sin((worldAngle + 90f) * Mathf.Deg2Rad));
            Vector3 swingOrigin = (Vector3)(aim * swordGripOffset)
                + Vector3.up * (animatedVisual.localPosition.y - visualRestPosition.y);
            swordVisual.transform.localPosition = swingOrigin;
            swordVisual.transform.localRotation = Quaternion.Euler(0f, 0f, worldAngle);
            if (swordTrail != null && progress >= 0.7f)
            {
                swordTrail.enabled = true;
                swordTrail.transform.localPosition = swingOrigin;
                swordTrail.transform.localRotation = Quaternion.Euler(0f, 0f, aimAngle);
                SetSwordTrailAlpha(swordTrailOpacity * Mathf.Clamp01((progress - 0.7f) / 0.25f));
            }
            if (progress >= 0.2f && progress <= 0.85f)
                Strike(bladeDirection, hitTargets);
            elapsed += Time.deltaTime;
            yield return null;
        }
        swordVisual.enabled = false;
        swordVisual.color = swordOriginalColor;
        if (swordTrail != null && swordTrail.enabled)
        {
            float fadeElapsed = 0f;
            while (fadeElapsed < swordTrailFadeSeconds)
            {
                SetSwordTrailAlpha(swordTrailOpacity * (1f - fadeElapsed / swordTrailFadeSeconds));
                fadeElapsed += Time.deltaTime;
                yield return null;
            }
            swordTrail.enabled = false;
            swordTrail.color = trailOriginalColor;
        }
        swordSwing = null;
    }

    private void SetSwordTrailAlpha(float alpha)
    {
        Color color = swordTrail.color;
        color.a = alpha;
        swordTrail.color = color;
    }

    private IEnumerator CircularAttackRoutine()
    {
        nextCircularAttackAt = Time.time + EffectiveSpinCooldown;
        actionEndsAt = Mathf.Max(actionEndsAt, Time.time + circularAttackWindup + circularAttackVisualSeconds);
        body.linearVelocity = Vector2.zero;
        swordHitSoundPlayed = false;
        criticalSwing = criticalChance > 0f && Random.value < criticalChance;
        GameSfx.PlayHero(SfxCue.SwordWhoosh);
        if (circularAttackWindup > 0f) yield return new WaitForSeconds(circularAttackWindup);
        if (!IsAlive)
        {
            circularAttackRoutine = null;
            yield break;
        }
        Camera gameCamera = Camera.main;
        Vector2 aim = facing.sqrMagnitude > 0.001f ? facing.normalized : Vector2.right;
        if (gameCamera != null)
        {
            Vector2 toMouse = gameCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            if (toMouse.sqrMagnitude > 0.001f) aim = toMouse.normalized;
        }
        float startAngle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg - 100f;
        float elapsed = 0f;
        int nextHitSample = 0;
        var hitTargets = new HashSet<Collider2D>();
        Color waveColor = circularAttackEffect != null ? circularAttackEffect.color : Color.white;
        Vector3 waveScale = circularAttackEffect != null ? circularAttackEffect.transform.localScale : Vector3.one;
        if (swordVisual != null)
        {
            swordVisual.color = criticalSwing ? criticalSwordTint : swordOriginalColor;
            swordVisual.enabled = true;
        }
        if (circularAttackEffect != null)
        {
            circularAttackEffect.transform.localScale = waveScale * (EffectiveSpinRadius / circularAttackRadius);
            circularAttackEffect.enabled = true;
        }
        while (elapsed < circularAttackVisualSeconds && IsAlive)
        {
            float progress = Mathf.Clamp01(elapsed / circularAttackVisualSeconds);
            float angle = startAngle + progress * 360f;
            Vector2 bladeDirection = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            // Sweep the damage together with the blade. Fixed angular samples
            // prevent gaps when the game runs at a lower frame rate.
            int reachedSample = Mathf.FloorToInt(progress * 24f);
            while (nextHitSample <= reachedSample)
            {
                float sampleAngle = (startAngle + nextHitSample * 15f) * Mathf.Deg2Rad;
                Vector2 sampleDirection = new Vector2(Mathf.Cos(sampleAngle), Mathf.Sin(sampleAngle));
                Strike(sampleDirection, hitTargets, EffectiveSpinRadius * 0.5f,
                    EffectiveSpinRadius * 0.5f);
                nextHitSample++;
            }
            int order = sprite.sortingOrder + (bladeDirection.y > 0.3f ? -1 : 2);
            if (swordVisual != null)
            {
                swordVisual.sortingOrder = order;
                swordVisual.transform.localPosition = bladeDirection * swordGripOffset;
                swordVisual.transform.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
            }
            if (circularAttackEffect != null)
            {
                circularAttackEffect.sortingOrder = order + 1;
                circularAttackEffect.transform.localPosition = bladeDirection * (0.3f + 0.1f * spinMasteryLevel);
                circularAttackEffect.transform.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
                float alpha = Mathf.Min(1f, progress * 8f) * Mathf.Min(1f, (1f - progress) * 6f);
                circularAttackEffect.color = new Color(waveColor.r, waveColor.g, waveColor.b,
                    waveColor.a * alpha);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (IsAlive)
        {
            while (nextHitSample <= 24)
            {
                float sampleAngle = (startAngle + nextHitSample * 15f) * Mathf.Deg2Rad;
                Vector2 sampleDirection = new Vector2(Mathf.Cos(sampleAngle), Mathf.Sin(sampleAngle));
                Strike(sampleDirection, hitTargets, EffectiveSpinRadius * 0.5f,
                    EffectiveSpinRadius * 0.5f);
                nextHitSample++;
            }
        }
        if (swordVisual != null)
        {
            swordVisual.enabled = false;
            swordVisual.color = swordOriginalColor;
        }
        if (circularAttackEffect != null)
        {
            circularAttackEffect.enabled = false;
            circularAttackEffect.color = waveColor;
            circularAttackEffect.transform.localScale = waveScale;
        }
        circularAttackRoutine = null;
    }

    private void StartJump()
    {
        // The drawn jump is a side view. Reuse the saved directional poses for
        // the other directions, just as the dash does, while keeping one jump timer.
        AnimationClip directionalJump = facingDiagonal
            ? (facingUp ? walkUpRight ?? walkUp : walkDownRight ?? walkDown) ?? jump
            : facingVertically ? (facingUp ? idleUp : idleDown) ?? jump : jump;
        if (facingDiagonal) sprite.flipX = facing.x < 0f;
        else if (facingVertically) sprite.flipX = false;
        current = null;
        Play(directionalJump);
        GameSfx.PlayHero(SfxCue.Jump);
        actionEndsAt = Time.time + jump.length;
        if (animatedVisual != transform)
        {
            if (visualJump != null) StopCoroutine(visualJump);
            visualJump = StartCoroutine(VisualJumpRoutine(jump.length));
        }
    }

    private void StartDash()
    {
        bool wasFull = dashCharges == maximumDashCharges;
        dashCharges--;
        dashDirection = input.sqrMagnitude > 0.001f ? input.normalized : facing;
        if (dashDirection.sqrMagnitude < 0.001f) dashDirection = Vector2.right;
        bool hasVerticalDirection = Mathf.Abs(dashDirection.y) > 0.01f;
        bool diagonalDash = hasVerticalDirection && Mathf.Abs(dashDirection.x) > 0.01f;
        AnimationClip directionalDash = diagonalDash
            ? (dashDirection.y > 0f ? walkUpRight ?? walkUp : walkDownRight ?? walkDown) ?? dash
            : hasVerticalDirection ? (dashDirection.y > 0f ? idleUp : idleDown) ?? dash : dash;
        if (diagonalDash) sprite.flipX = dashDirection.x < 0f;
        else if (hasVerticalDirection) sprite.flipX = false;
        current = null;
        Play(directionalDash);
        GameSfx.PlayHero(SfxCue.Dash);
        dashStartedAt = Time.time;
        dashEndsAt = Time.time + dash.length;
        actionEndsAt = dashEndsAt;
        if (wasFull) nextDashAt = dashEndsAt + dashCooldown;
        nextDamageAt = Mathf.Max(nextDamageAt, dashEndsAt + dashGuardLevel * dashGuardSecondsPerUpgrade);
        UpdateDashDisplay();
    }

    private System.Collections.IEnumerator VisualJumpRoutine(float duration)
    {
        Vector3 restingPosition = animatedVisual.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float height = Mathf.Sin(progress * Mathf.PI) * 0.22f;
            animatedVisual.localPosition = restingPosition + Vector3.up * height;
            yield return null;
        }
        animatedVisual.localPosition = restingPosition;
        visualJump = null;
        // The current land asset is a front-facing placeholder. Let Update
        // return directly to the correctly facing idle/walk clip instead.
    }

    private void Strike(Vector2 direction, HashSet<Collider2D> hitTargets,
        float reachOverride = -1f, float radiusOverride = -1f)
    {
        float reach = reachOverride >= 0f ? reachOverride : attackReach;
        Vector2 center = body.position + direction * reach;
        float effectiveRadius = radiusOverride >= 0f
            ? radiusOverride : attackRadius + wideSwingLevel * wideSwingRadiusPerUpgrade;
        foreach (Collider2D target in Physics2D.OverlapCircleAll(center, effectiveRadius))
        {
            TrapperEnemy trapper = target.GetComponent<TrapperEnemy>();
            if (trapper != null && trapper.IsInvisible) continue;
            if (!hitTargets.Add(target)) continue;
            int bonusHits = 0;
            bool hitsEnemy = target.GetComponent<SimpleEnemy>() != null ||
                target.GetComponent<MageCreep>() != null || target.GetComponent<RingCreep>() != null ||
                target.GetComponent<HookwardenEnemy>() != null || target.GetComponent<WhirlraiderEnemy>() != null ||
                target.GetComponent<StonecleaverEnemy>() != null || target.GetComponent<BlinkshadeEnemy>() != null ||
                target.GetComponent<FrostbinderEnemy>() != null || target.GetComponent<ShadowrazerEnemy>() != null ||
                target.GetComponent<DreadmarksmanEnemy>() != null || target.GetComponent<WarcryBruteEnemy>() != null ||
                target.GetComponent<DeepTideColossusEnemy>() != null || target.GetComponent<IoEnemy>() != null || trapper != null;
            if (hitsEnemy && swordDamageBonus > 0f)
            {
                swordBonusHitProgress += swordDamageBonus;
                if (swordBonusHitProgress >= 1f)
                {
                    bonusHits++;
                    swordBonusHitProgress -= 1f;
                }
            }
            if (hitsEnemy && criticalSwing) bonusHits++;
            EnemyHitFeedback feedback = target.GetComponent<EnemyHitFeedback>();
            if (feedback != null) feedback.React(direction);
            SimpleEnemy enemy = target.GetComponent<SimpleEnemy>();
            if (enemy != null) enemy.TakeHit();
            MageCreep mage = target.GetComponent<MageCreep>();
            if (mage != null) mage.TakeHit();
            RingCreep ringCreep = target.GetComponent<RingCreep>();
            if (ringCreep != null) ringCreep.TakeHit();
            HookwardenEnemy hookwarden = target.GetComponent<HookwardenEnemy>();
            if (hookwarden != null) hookwarden.TakeHit();
            WhirlraiderEnemy whirlraider = target.GetComponent<WhirlraiderEnemy>();
            if (whirlraider != null) whirlraider.TakeHit();
            StonecleaverEnemy stonecleaver = target.GetComponent<StonecleaverEnemy>();
            if (stonecleaver != null) stonecleaver.TakeHit();
            BlinkshadeEnemy blinkshade = target.GetComponent<BlinkshadeEnemy>();
            if (blinkshade != null) blinkshade.TakeHit();
            FrostbinderEnemy frostbinder = target.GetComponent<FrostbinderEnemy>();
            if (frostbinder != null) frostbinder.TakeHit();
            ShadowrazerEnemy shadowrazer = target.GetComponent<ShadowrazerEnemy>();
            if (shadowrazer != null) shadowrazer.TakeHit();
            DreadmarksmanEnemy dreadmarksman = target.GetComponent<DreadmarksmanEnemy>();
            if (dreadmarksman != null) dreadmarksman.TakeHit();
            WarcryBruteEnemy warcryBrute = target.GetComponent<WarcryBruteEnemy>();
            if (warcryBrute != null) warcryBrute.TakeHit();
            DeepTideColossusEnemy deepTide = target.GetComponent<DeepTideColossusEnemy>();
            if (deepTide != null) deepTide.TakeHit();
            IoEnemy io = target.GetComponent<IoEnemy>();
            if (io != null) io.TakeHit();
            if (trapper != null) trapper.TakeHit();
            // Hit-count enemies use a discrete health system. Accumulating 5% grants one
            // additional hit every 20 landed sword strikes, without random criticals.
            for (int extraHit = 0; extraHit < bonusHits; extraHit++)
            {
                if (enemy != null && enemy.CurrentHits > 0) enemy.TakeHit();
                if (mage != null && mage.CurrentHits > 0) mage.TakeHit();
                if (ringCreep != null && ringCreep.CurrentHits > 0) ringCreep.TakeHit();
                if (hookwarden != null && hookwarden.CurrentHits > 0) hookwarden.TakeHit();
                if (whirlraider != null && whirlraider.CurrentHits > 0) whirlraider.TakeHit();
                if (stonecleaver != null && stonecleaver.CurrentHits > 0) stonecleaver.TakeHit();
                if (blinkshade != null && blinkshade.CurrentHits > 0) blinkshade.TakeHit();
                if (frostbinder != null && frostbinder.CurrentHits > 0) frostbinder.TakeHit();
                if (shadowrazer != null && shadowrazer.CurrentHits > 0) shadowrazer.TakeHit();
                if (dreadmarksman != null && dreadmarksman.CurrentHits > 0) dreadmarksman.TakeHit();
                if (warcryBrute != null && warcryBrute.CurrentHits > 0) warcryBrute.TakeHit();
                if (deepTide != null && deepTide.CurrentHits > 0) deepTide.TakeHit();
                if (io != null && io.IsAlive) io.TakeHit();
                if (trapper != null && trapper.CurrentHits > 0) trapper.TakeHit();
            }
            if (hitsEnemy && bloodSwordLevel > 0)
            {
                bloodSwordHitProgress++;
                if (bloodSwordHitProgress >= bloodSwordHitsPerHeal)
                {
                    bloodSwordHitProgress = 0;
                    TryHeal(bloodSwordHealPerUpgrade * bloodSwordLevel);
                }
            }
            HittableTree tree = target.GetComponent<HittableTree>();
            if (tree != null) tree.TakeHit(direction);
            if (!swordHitSoundPlayed && (enemy != null || mage != null || ringCreep != null ||
                hookwarden != null || whirlraider != null || stonecleaver != null ||
                blinkshade != null || frostbinder != null || shadowrazer != null ||
                dreadmarksman != null || warcryBrute != null || deepTide != null ||
                io != null || trapper != null || tree != null))
            {
                swordHitSoundPlayed = true;
                GameSfx.PlayHero(SfxCue.SwordHit);
            }
        }
    }

    public void ReceiveDamage(int amount)
    {
        if (!IsAlive || amount <= 0 || Time.time < nextDamageAt) return;
        amount = Mathf.Max(1, Mathf.RoundToInt(amount * (1f - ironSkinLevel * ironSkinReductionPerUpgrade)));
        health = Mathf.Max(0, health - amount);
        GameSfx.PlayHero(SfxCue.HeroHurt);
        nextDamageAt = Time.time + damageImmunitySeconds;
        UpdateHealthDisplay();
        if (hurtFlash != null) StopCoroutine(hurtFlash);
        hurtFlash = StartCoroutine(FlashOnDamage());
        if (!IsAlive)
        {
            CancelSwordSwing();
            input = Vector2.zero;
            body.linearVelocity = Vector2.zero;
            if (visualJump != null) { StopCoroutine(visualJump); visualJump = null; }
            if (animatedVisual != null) animatedVisual.localPosition = Vector3.zero;
            if (death != null) { current = null; Play(death); actionEndsAt = float.PositiveInfinity; }
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }
        else
        {
            CancelSwordSwing();
            AnimationClip hurt = Random.value < 0.5f ? hurt1 : hurt2;
            if (hurt != null) StartAction(hurt);
        }
    }

    public bool TryHeal(int amount)
    {
        if (!IsAlive || amount <= 0 || health >= MaxHealth) return false;
        health = Mathf.Min(MaxHealth, health + amount);
        GameSfx.PlayHero(SfxCue.Heal);
        UpdateHealthDisplay();
        return true;
    }

    private IEnumerator FlashOnDamage()
    {
        if (hurtFlashSeconds <= 0f)
        {
            sprite.color = normalSpriteColor;
            hurtFlash = null;
            yield break;
        }

        Color flashColor = new Color(
            normalSpriteColor.r * hurtFlashColor.r,
            normalSpriteColor.g * hurtFlashColor.g,
            normalSpriteColor.b * hurtFlashColor.b,
            normalSpriteColor.a);
        float elapsed = 0f;
        while (elapsed < hurtFlashSeconds)
        {
            sprite.color = Color.Lerp(flashColor, normalSpriteColor, elapsed / hurtFlashSeconds);
            elapsed += Time.deltaTime;
            yield return null;
        }
        sprite.color = normalSpriteColor;
        hurtFlash = null;
    }

    private void UpdateHealthDisplay()
    {
        if (healthFill != null && healthFill != healthSpriteImage)
            healthFill.fillAmount = (float)health / MaxHealth;
        if (healthLabel != null) healthLabel.text = $"{health} / {MaxHealth}";
        if (healthSpriteImage != null && healthFrames != null)
        {
            int filledSegments = Mathf.CeilToInt((float)health / MaxHealth * healthSegments);
            healthSpriteImage.sprite = healthFrames.GetFrame(healthSegments - filledSegments);
        }
    }

    private void UpdateDashDisplay()
    {
        if (dashSpriteImage == null || dashFrames == null || dashFrames.Count < 5) return;
        dashSpriteImage.color = maximumDashCharges > 1 && dashCharges == 2 ? twoDashChargesTint : Color.white;
        int frame;
        if (dashCharges > 0 && Time.time >= dashEndsAt)
            frame = 0; // Full bar: Shift is available again.
        else if (Time.time < dashEndsAt)
            frame = Mathf.Clamp(1 + Mathf.FloorToInt((Time.time - dashStartedAt) / dashSpendSeconds * 4f), 1, 4);
        else
        {
            float refill = Mathf.Clamp01((Time.time - dashEndsAt) / dashCooldown);
            frame = Mathf.Clamp(5 + Mathf.FloorToInt(refill * (dashFrames.Count - 5)), 5, dashFrames.Count - 1);
        }
        dashSpriteImage.sprite = dashFrames.GetFrame(frame);
    }

    private void RechargeDash()
    {
        if (dashCharges >= maximumDashCharges || Time.time < nextDashAt) return;
        dashCharges++;
        if (dashCharges < maximumDashCharges) nextDashAt = Time.time + dashCooldown;
    }

    private void ExplodeOnKill()
    {
        float radius = killExplosionRadius + 0.2f * (killExplosionLevel - 1);
        var hitEnemies = new HashSet<GameObject>();
        foreach (Collider2D target in Physics2D.OverlapCircleAll(body.position, radius))
        {
            GameObject enemyObject = target.attachedRigidbody != null
                ? target.attachedRigidbody.gameObject : target.gameObject;
            if (!hitEnemies.Add(enemyObject)) continue;
            TrapperEnemy trapper = enemyObject.GetComponent<TrapperEnemy>();
            if (trapper != null && trapper.IsInvisible) continue;
            SimpleEnemy simple = enemyObject.GetComponent<SimpleEnemy>();
            if (simple != null) simple.TakeHit();
            MageCreep mage = enemyObject.GetComponent<MageCreep>();
            if (mage != null) mage.TakeHit();
            RingCreep ring = enemyObject.GetComponent<RingCreep>();
            if (ring != null) ring.TakeHit();
            HookwardenEnemy hookwarden = enemyObject.GetComponent<HookwardenEnemy>();
            if (hookwarden != null) hookwarden.TakeHit();
            WhirlraiderEnemy whirlraider = enemyObject.GetComponent<WhirlraiderEnemy>();
            if (whirlraider != null) whirlraider.TakeHit();
            StonecleaverEnemy stonecleaver = enemyObject.GetComponent<StonecleaverEnemy>();
            if (stonecleaver != null) stonecleaver.TakeHit();
            BlinkshadeEnemy blinkshade = enemyObject.GetComponent<BlinkshadeEnemy>();
            if (blinkshade != null) blinkshade.TakeHit();
            FrostbinderEnemy frostbinder = enemyObject.GetComponent<FrostbinderEnemy>();
            if (frostbinder != null) frostbinder.TakeHit();
            ShadowrazerEnemy shadowrazer = enemyObject.GetComponent<ShadowrazerEnemy>();
            if (shadowrazer != null) shadowrazer.TakeHit();
            DreadmarksmanEnemy marksman = enemyObject.GetComponent<DreadmarksmanEnemy>();
            if (marksman != null) marksman.TakeHit();
            WarcryBruteEnemy brute = enemyObject.GetComponent<WarcryBruteEnemy>();
            if (brute != null) brute.TakeHit();
            DeepTideColossusEnemy colossus = enemyObject.GetComponent<DeepTideColossusEnemy>();
            if (colossus != null) colossus.TakeHit();
            IoEnemy io = enemyObject.GetComponent<IoEnemy>();
            if (io != null) io.TakeHit();
            if (trapper != null) trapper.TakeHit();
        }
        if (killExplosionSprite != null) StartCoroutine(ShowKillExplosion(radius));
    }

    private IEnumerator ShowKillExplosion(float radius)
    {
        GameObject effect = new GameObject("Kill explosion (temporary)");
        effect.transform.position = body.position;
        SpriteRenderer renderer = effect.AddComponent<SpriteRenderer>();
        renderer.sprite = killExplosionSprite;
        renderer.sortingOrder = sprite.sortingOrder + 3;
        float diameter = killExplosionSprite.bounds.size.x;
        effect.transform.localScale = Vector3.one * (radius * 2f / Mathf.Max(diameter, 0.01f));
        float elapsed = 0f;
        while (elapsed < killExplosionVisualSeconds)
        {
            renderer.color = new Color(1f, 1f, 1f, 1f - elapsed / killExplosionVisualSeconds);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(effect);
    }

    private void Play(AnimationClip clip)
    {
        if (clip == null || clip == current) return;
        current = clip;
        animationPlayer.Play(clip.name, 0, 0f);
    }

    private AnimationClip FindControllerClip(string clipName)
    {
        if (animationPlayer == null || animationPlayer.runtimeAnimatorController == null) return null;
        foreach (AnimationClip clip in animationPlayer.runtimeAnimatorController.animationClips)
            if (clip != null && clip.name == clipName) return clip;
        return null;
    }

    private void OnDisable()
    {
        if (hurtFlash != null) StopCoroutine(hurtFlash);
        hurtFlash = null;
        if (sprite != null) sprite.color = normalSpriteColor;
        CancelSwordSwing();
        input = Vector2.zero;
        if (body != null) body.linearVelocity = Vector2.zero;
        if (animatedVisual != null) animatedVisual.localPosition = Vector3.zero;
    }

    private void CancelSwordSwing()
    {
        if (swordSwing != null) StopCoroutine(swordSwing);
        swordSwing = null;
        if (swordVisual != null) swordVisual.enabled = false;
        if (swordTrail != null) swordTrail.enabled = false;
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused)
        {
            input = Vector2.zero;
            if (body != null) body.linearVelocity = Vector2.zero;
        }
    }

#if UNITY_EDITOR
    // Inspector defaults when the component is added by hand; creates no objects or scenes.
    private void Reset()
    {
        const string folder = "Assets/Generated/Animations/Player_";
        idle = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>(folder + "Idle.anim");
        walk = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>(folder + "Walk.anim");
        jump = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>(folder + "Jump.anim");
    }
#endif
}
