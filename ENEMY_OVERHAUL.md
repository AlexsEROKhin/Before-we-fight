# Enemy overhaul (Before We Fight)

Open `Assets/Scenes/Arena.unity`. All existing enemy art and player movement are preserved.

## Enemies found

| Enemy | Role | Improvement |
| --- | --- | --- |
| RingCreep | close-range pressure | Wind-up before the damaging ring, more stable mixed-enemy separation, squad pacing. |
| MageCreep / InvokerMage | sky-strike caster | Nearby strikes no longer stack over the same player position; obstacle-aware retreat and squad pacing. |
| Io | flying support | Prioritizes endangered, injured and stronger allies; avoids solid obstacles; linked ally gets a small visible buff ring; bombs have trails, a warning, an impact ring and coordinated attack timing; death fades. |
| Hookwarden | pull / follow-up fighter | Repositions for an unobstructed hook and marks the target before throwing. |
| Whirlraider | mobile spin attacker | Marks the start of a spin, subtly steers during it and avoids walls. |
| Stonecleaver | advancing fissure | Locks and marks the threatened point, coordinates with other ground attacks, cleans up the fissure on death. |
| Blinkshade | teleport assassin | Locks and marks its landing spot before vanishing instead of teleporting to the player's last-moment position. |
| Frostbinder | slowing caster | Reserves its marked impact zone so allied casters do not stack unavoidable effects. |
| Shadowrazer | area caster | Coordinates its existing telegraphed ground strike with other casters. |
| Dreadmarksman | long-range marksman | Strafes to find a clear firing lane; coordinates aimed shots. |
| WarcryBrute | enraged heavy fighter | Heavy strike targets a locked position with a visible ground ring; hit interruption cancels it. |
| DeepTideColossus | heavy shockwave | Shockwave radius is visible during the charge; hit interruption cancels it. |
| SimpleEnemy | legacy melee fallback (no prefab) | Attack has a dodgeable wind-up; movement uses a kinematic body. |

`EnemySeparation`, already present on the enemy prefabs, now provides stable same-frame separation, obstacle steering, and the Io support adapter at runtime. The adapter restores the enemies' **existing hit counters**; it does not introduce a second HP pool. Enemies using `EnemyDefeat.PlayDeath` fade after their existing death frames; Io has a separate magical fade. `EnemyHitFeedback` no longer moves the physics object or leaves its sprite tinted when hits arrive close together.

The ten named squads in Arena each have an editable `EnemySquadStage` component and one editable Io prefab alongside two RingCreeps and two Mages. Stage 1–10 changes engagement distance, movement and attack cadence for the placed RingCreep/MageCreep groups. In the Inspector, edit each squad's stage and its early/late scales. Per-enemy wind-up, cooldown, warning radius, avoidance and support parameters remain editable on their existing components. The `EnemySeparation` component also exposes obstacle steering settings.

Each Io has 80 HP (up from 40). The eight cyan health blocks above it are visible when the player approaches; each block represents 10 HP and darkens when lost. The Io prefab exposes maximum health and health-bar segment size, spacing, colours and visibility range. The extra Io in Stonecleaver Camp is unchanged, so Arena currently has 11 Io in total. The player's sword swing takes 0.14 seconds followed by 0.4 seconds of recovery, allowing a new attack every 0.54 seconds. Both timings remain editable on the Arena player component.

All other enemy prefabs now have `EnemyHealthBar` too. Its red blocks read each enemy script's existing `CurrentHits` / `MaximumHits`: one block equals one sword hit. Io healing restores these same blocks. Width, spacing, colour and visibility range are editable on each prefab. This includes the two Arena creeps and the specialist prefabs not yet placed in Arena; no enemy damage or durability was changed by adding these displays.

## Verification and limits

All gameplay scripts compile against the installed Unity 6.2 assemblies. The Arena scene has ten stage components and ten matching references. A Unity Play Mode test still needs to be run locally: the batch Editor currently exits during licensing, before scene import. Open Arena in Unity and test a first and late squad, an Io tether/bomb, and each reserve-enabled specialist before relying on balance numbers.

There is no parry code in the current `TopDownPlayer` controller, so no enemy was falsely labelled parryable. This update preserves dash invulnerability through `ReceiveDamage`; it does not add or alter player controls. No new character sprite sheets were created. Several specialist enemy prefabs are in the project but are not placed in Arena, so their changes are ready when you place them, not represented as new arena spawns.
