using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Batch-only integration check: Unity -batchmode -executeMethod IoSmokeTest.Run.
[InitializeOnLoad]
public static class IoSmokeTest
{
    private const string ActiveKey = "IoSmokeTest.Active";
    private static double enteredAt;
    private static bool checkedInitial;
    private static bool checkedHealing;
    private static bool sawAttack;
    private static bool checkedExplosion;
    private static bool killedIo;
    private static TopDownPlayer player;
    private static IoEnemy io;
    private static EnemySupportable ally;

    static IoSmokeTest()
    {
        EditorApplication.update += Tick;
    }

    public static void Run()
    {
        SessionState.SetBool(ActiveKey, true);
        EditorSceneManager.OpenScene("Assets/Scenes/Arena.unity");
        // A headless graphics device cannot render the minimap RenderTexture.
        // Keep gameplay systems intact while checking Io in batch Play Mode.
        foreach (ArenaMinimap minimap in Object.FindObjectsByType<ArenaMinimap>(FindObjectsSortMode.None))
            minimap.enabled = false;
        EditorApplication.isPlaying = true;
    }

    private static void Tick()
    {
        if (!SessionState.GetBool(ActiveKey, false) || !EditorApplication.isPlaying) return;
        if (enteredAt <= 0) enteredAt = EditorApplication.timeSinceStartup;
        double elapsed = EditorApplication.timeSinceStartup - enteredAt;
        if (!checkedInitial && elapsed >= 1.5)
        {
            checkedInitial = true;
            player = Object.FindFirstObjectByType<TopDownPlayer>();
            var claimedAllies = new HashSet<EnemySupportable>();
            foreach (IoEnemy candidate in Object.FindObjectsByType<IoEnemy>(FindObjectsSortMode.None))
            {
                EnemySupportable candidateTarget = candidate.GetComponent<IoTether>().Target;
                if (candidateTarget == null) continue;
                if (!claimedAllies.Add(candidateTarget))
                {
                    Debug.LogError("IO_SMOKE_FAIL support: two Io claimed one ally");
                    Finish(1);
                    return;
                }
                if (io == null || (ally != null && ally.GetComponent<RingCreep>() == null &&
                    candidateTarget.GetComponent<RingCreep>() != null))
                {
                    io = candidate;
                    ally = candidateTarget;
                }
            }
            int bombs = CountBombs(io);
            IoTether tether = io != null ? io.GetComponent<IoTether>() : null;
            IoHealthBar healthBar = io != null ? io.GetComponent<IoHealthBar>() : null;
            SpriteRenderer ioVisual = io != null ? io.GetComponentInChildren<SpriteRenderer>() : null;
            if (io == null || player == null || bombs != 3 || ally == null ||
                healthBar == null || io.MaximumHealth != 80 ||
                ioVisual == null || ioVisual.sprite == null ||
                Mathf.Abs(ally.SpeedMultiplier - 1.25f) > 0.001f)
            {
                Debug.LogError($"IO_SMOKE_FAIL initial: io={io != null} player={player != null} " +
                    $"bombs={bombs} ally={ally != null} " +
                    $"hpBar={healthBar != null} maxHp={(io != null ? io.MaximumHealth : 0)} " +
                    $"sprite={ioVisual != null && ioVisual.sprite != null} " +
                    $"speed={(ally != null ? ally.SpeedMultiplier : 0f)}");
                Finish(1);
                return;
            }
            Debug.Log("IO_SMOKE_OK initial: 3 orbit bombs, ally tether, 25% speed bonus");
            // Keep the second attack out of this short test so the first
            // bomb's full five-second respawn can be checked unambiguously.
            typeof(IoEnemy).GetField("attackCooldown",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(io, 100f);
            RingCreep ring = ally.GetComponent<RingCreep>();
            MageCreep mage = ally.GetComponent<MageCreep>();
            if (ring != null) ring.TakeHit();
            else if (mage != null) mage.TakeHit();
            player.transform.position = io.transform.position + Vector3.right * 4f;
        }
        if (checkedInitial && !checkedHealing && elapsed >= 4.2)
        {
            checkedHealing = true;
            if (ally == null || ally.HealthFraction < 0.999f)
            {
                Debug.LogError("IO_SMOKE_FAIL healing: linked ally did not recover a hit");
                Finish(1);
                return;
            }
            Debug.Log("IO_SMOKE_OK healing: existing enemy hit counter restored");
        }
        if (checkedInitial && io != null)
        {
            foreach (IoBomb bomb in Object.FindObjectsByType<IoBomb>(FindObjectsSortMode.None))
                if (bomb != null && bomb.Owner == io && !bomb.IsOrbiting) sawAttack = true;
        }
        if (!checkedExplosion && elapsed >= 7.0)
        {
            int remaining = CountBombs(io);
            if (!sawAttack || remaining != 2)
            {
                Debug.LogError($"IO_SMOKE_FAIL explosion: attack={sawAttack} remaining={remaining}");
                Finish(1);
                return;
            }
            Debug.Log("IO_SMOKE_OK explosion: attack bomb vanished, two still orbit");
            checkedExplosion = true;
        }
        if (checkedExplosion && !killedIo && elapsed >= 10.8)
        {
            int count = CountBombs(io);
            if (count != 3)
            {
                Debug.LogError($"IO_SMOKE_FAIL respawn: bombs={count}");
                Finish(1);
                return;
            }
            Debug.Log("IO_SMOKE_OK respawn: third bomb returned after explosion");
            killedIo = true;
            for (int i = 0; i < 32 && io.IsAlive; i++) io.TakeHit();
        }
        if (killedIo && elapsed >= 11.1)
        {
            int bombs = CountBombs(io);
            float speed = ally != null ? ally.SpeedMultiplier : 1f;
            if (bombs != 0 || Mathf.Abs(speed - 1f) > 0.001f)
            {
                Debug.LogError($"IO_SMOKE_FAIL cleanup: bombs={bombs} speed={speed}");
                Finish(1);
                return;
            }
            Debug.Log("IO_SMOKE_OK cleanup: bombs removed, speed restored");
            Finish(0);
        }
    }

    private static void Finish(int code)
    {
        SessionState.SetBool(ActiveKey, false);
        EditorApplication.Exit(code);
    }

    private static int CountBombs(IoEnemy owner)
    {
        if (owner == null) return 0;
        int count = 0;
        foreach (IoBomb bomb in Object.FindObjectsByType<IoBomb>(FindObjectsSortMode.None))
            if (bomb != null && bomb.Owner == owner) count++;
        return count;
    }
}
