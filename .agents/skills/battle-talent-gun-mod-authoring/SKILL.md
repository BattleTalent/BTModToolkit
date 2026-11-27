---
name: battle-talent-gun-mod-authoring
description: Create, modify, and validate Battle Talent firearm mods in ModProj, including pistols, rifles, SMGs, shotguns, gun prefabs, grips, trigger scripts, bullets, recoil, sounds, effects, and ItemInfoConfig. Use the toolkit templates and Gun_AK47/Gun_UMP examples; not for bows, ordinary melee weapons, spells, or standalone projectiles.
---

# Battle Talent Gun Mod Authoring

Create the mod in `ModProj`, using Unity to import assets and serialize prefabs, components, configs, and Addressables. A functioning gun is a connected contract between a weapon prefab, its trigger behavior, a projectile prefab, and resolvable resource identifiers, not just a gun model with colliders.

Also follow `../battle-talent-mod-authoring/SKILL.md`. Load `../battle-talent-lua/SKILL.md` when creating or changing scripts, and `../battle-talent-build/SKILL.md` when registering, packaging, installing, or diagnosing runtime output.

## Authority And Safety

- Use the community gun tutorial for workflow background: <https://battletalent.github.io/community-docs/docs/support-mod-types/Weapon/Tutorials/advanced-create-a-gun-mod>.
- **Never run `Tools > Destructive > Cleanup All Content`.** The tutorial's `Remove unused content` / `Cleanup All Content` step is obsolete, incorrect, and excluded from this workflow. Do not delete shipped examples or Resources as preparation.
- Use Unity 2020.3.48f1; verify `ModProj/Packages/manifest.json` before using package APIs. This checkout pins Addressables 1.19.18 and URP 10.10.1.
- Current toolkit source, the selected example, and the live Unity Inspector override older screenshots. Do not manually edit `.prefab`, `.asset`, `.unity`, `.mat`, or `.meta` YAML.
- Use `Readme.md` and only the relevant generated pages under `docs/` to verify game APIs. Toolkit classes may be serialization stubs, not runnable copies of game behavior.
- Require only the mod toolkit and the documented Unity environment. Do not require a separate game-source checkout or copy internal game components, GUIDs, or unverified built-in resource names into the mod.
- Do not change the shared Common scripts to satisfy one gun. Put gun-specific behavior under that mod's `Script/` folder. Preserve other mods and existing prefix contracts.

## Establish The Baseline

Determine or reasonably infer the mod/prefab name, display text, model/material/audio sources, one- or two-handed grip, firing mode, projectile behavior, and Windows/Quest delivery requirements. Ammo, magazines, manual reload, networking, and animations are optional features, not implicit requirements.

Before editing, state a falsifiable local hypothesis: **the selected trigger implementation can create the intended projectile using the proposed script ID, injections, and current prefix.** The cheapest check is to inspect that exact script/component and compare its required fields with the selected gun and projectile prefabs. Resolve discrepancies before adding features.

Choose one starting route:

| Need | Starting point |
|---|---|
| Basic firearm | Template Wizard `Weapon` + `Gun`; compare `Assets/Build/Gun_AK47` |
| Charged release burst | `Assets/Build/Gun_UMP`, including `GunUMPScript.txt` and `UMPBullet.prefab` |
| Shotgun | Wizard `Weapon` + `Shotgun`, then verify or implement pellet count, spread, and fire interval in the actual selected script |

Read [references/examples-and-contracts.md](references/examples-and-contracts.md) when selecting scripts, using burst/shotgun/networking behavior, tuning firing/projectile semantics, or diagnosing copied examples. It records known mismatches that a prefab-only check will miss.

## Execute Through Unity

Prefer deterministic editor APIs when practical: `AssetDatabase`, `PrefabUtility`, importer APIs, `SerializedObject`, and the existing Template Wizard. Use the graphical Editor for axis/scale, hand pose, collider coverage, muzzle placement, and visual judgment.

If automation is needed, first inspect existing editor entry points. Add only a narrowly scoped editor-only creator/validator when no suitable entry point exists. A creator must refuse accidental overwrite and use Unity APIs for every serialized write. Call the real Wizard rather than reproducing it with filesystem/YAML copies.

For batch execution, use the installed Unity 2020.3.48f1:

```text
<UnityEditor> -batchmode -nographics -quit -projectPath <ModProj> -executeMethod CrossLink.<Creator>.Create -logFile <log>
```

Do not start a second writer while another Unity instance owns the project. After changing C#, refresh/import, wait for compilation/domain reload, and require a clean Console before using the changed type. Batch success requires the process result and log, not merely launched execution.

## Scaffold The Mod

1. Select `Tools > Template Wizard`, choose `ModType.Weapon` and `WeaponType.Gun` or `Shotgun`, and supply `newModFolderName` and `storeItemInfo`. An editor creator may use `TemplateWizardAsset.GetWizard()` and `GenerateTemplate()` for the same operation.
2. The name must be nonempty, unique under `Assets/Build`, contain no spaces, and begin with an uppercase character. Explicitly set `storeItemInfo.addStoreItemName = <Prefix><WeaponPrefabName>` and `itemType = Weapon` in an automated creator; do not rely on Inspector `OnValidate` running.
3. Confirm the generated structure:

   ```text
   Assets/Build/<ModName>/
     Config/<ModName>.asset
     ICon/<ModName>.png
     Weapon/<ModName>.prefab
     FlyObj/Bullet.prefab
     Script/                  # only for custom gun/projectile scripts
     Audio/                   # only for custom sounds
     Effect/                  # only for custom effects
   ```

4. The Wizard also ensures `Assets/Build/Common/Script/WeaponFlyObjBaseScript.txt` exists, registers Common and the mod root in `AddressableConfig.addressablePaths`, and refreshes Addressables. It does **not** overwrite an existing Common script, customize hardcoded Dummy IDs to the active prefix, or ensure `BulletScript.txt` for copied `FlyObjectX` bullets.
5. Read the active prefix from `Assets/Resources/AddressableConfig.asset`; `WMD_` is an example, not a required prefix. Reuse an established prefix in a populated project unless migration was requested. Use distinct projectile/script/audio/effect filenames: separate mod folders do not namespace their generated addresses.

## Configure The Weapon Prefab

1. Insert the imported model under the generated weapon root, keeping `Rigidbody`, `InteractWeapon`, `GazeObj`, and the selected trigger. Align the model and muzzle with local `+Z` forward and `+Y` up. Preserve imported size and existing unit/scale settings; new wrappers use unit scale. Resize only when explicitly requested, not to match the dummy. Disable the example-weapon gizmo and fit grip/collision/muzzle positions to the unchanged model.
2. Add primitive compound colliders or convex MeshColliders on the physical gun parts. Use the named `Interact` layer, not a hardcoded layer number. Keep gun-body collision and damage separate from projectile collision and damage.
3. Keep/configure `RagdollHitInfoObj` and `CollisionEffect` for striking with the gun body. The current Gun Dummy uses `Metal`; older AK47/UMP roots use `Sword`. Do not copy that blade damage classification automatically. If different body regions genuinely need different hit objects, add one root `RagdollHitInfoRef`, run `Reset`/`RefreshRefs`, and inspect its coverage.
4. An ordinary gun does not need `StabObject`. Configure stabbing only for an intentional bayonet/gunblade and follow the melee skill for that part.
5. Reuse the template's linked `Assets/Toolkit/Prefabs/HandAttach.prefab`. The object is named `HandAttach`, but its grip component is `AttachPoint`, not a C# class named HandAttach. Verify `interact`, `selfRB`, `InteractWeapon.attachList`, and `allowTrigger`.
6. For visual grip fitting, place `Assets/Toolkit/HandPoseHelper/HandPoseHelper.prefab` in a temporary scene context, assign its `handAttach`, run `AddDrawTool`, and adjust grip/hand transforms against the model. Keep `Bip002 L Hand` and `Bip002 R Hand`; use `RemoveDrawTool` afterwards and do not save preview hands into the shippable gun. Both nodes must exist before using the current helper.
7. For a two-handed gun, add a suitable support grip through Unity, set its `dependAttachObj` to the primary grip, register both grips, and enable `allowSecondHand`. Keep the firing grip's trigger enabled; use the selected attach implementation's side/index restrictions. Do not apply the melee-only AttachLine rule to the primary pistol grip.
8. Position `MountAttach` and verify `mountAttachs` references. Use `InteractWeapon > Swing Like Pistol` as a handling starting point, then tune mass, force/damping, hand speeds, and optional `RigidbodyInit` for this model.
9. Assign visible renderers to `InteractTriggerX.skillChangeColors`; check the selected material's color property and charge-color setting.
10. Save and reopen the prefab. Verify missing scripts, inactive descendants, references, collider coverage, hand pose, and mount orientation before widening the change.

## Connect Trigger, Muzzle, And Script

Use the Wizard/AK47/UMP `InteractTriggerX` route for scripted mods. Keep one intended trigger implementation; do not add a second trigger beside the existing one or attach unsupported game-internal components.

| Field | Required contract |
|---|---|
| `interact` | The gun's `InteractWeapon` |
| `script.luaScript` | Bare prefixed script ID, e.g. `<Prefix>MyGunScript` or `<Prefix>WeaponFlyObjBaseScript` |
| Object injection `shootPosition` | A **Transform** at/outside the muzzle, with its forward axis pointing along the barrel |
| String injection `flyObjName` | Bare prefixed projectile ID, e.g. `<Prefix>MyGunBullet`; exact capitalization |
| Optional number injection `shootVel` | `-1` uses projectile `force`; nonnegative values override launch speed, subject to the selected projectile's behavior |
| Optional object injection `aimWidget` | The `AimWidget` component, not its GameObject |
| Optional object injection `shootSound` | `SoundEffectPlayer`, **only if the chosen Lua implementation actually calls it** |

Custom scripts are `.txt` TextAssets under `Script/`, ending with `return Class(...)`. A derived gun can inherit the current shared script through `Require("LuaScript/<Prefix>WeaponFlyObjBaseScript")`; the prefab stores the bare derived ID, not the full Require address.

Choose firing behavior intentionally:

- Single press: use `instantSkill = true` and the selected implementation's immediate shot path.
- Held fire: use the continuous skill route and an explicit script-side shot interval. The current Common `UpdateSkill` has **no interval limiter**; adding `shootInterval` to Number List alone does not create one.
- Charged release burst: inspect UMP's `OnChargeRelease`, configure a positive charge duration, burst duration/count, authority gating, nil handling, and scheduler cleanup. Mana cost is not a magazine system.
- Shotgun: simultaneous pellets and directional spread are distinct from UMP's timed burst. The current Common script does not read `bulletsPerShot` or `bulletSpread`, even though the Shotgun Dummy injects them.

Treat `skillDamage` as UI display, not authoritative projectile damage. `manaCost = -1` means no skill in the trigger contract, not unlimited free fire. Do not toggle `dontNeedUpdate` as a fire-rate control.

Lua numeric injections are not booleans: `0` is truthy. Check each script's comparisons before using 0/1 flags; normalize flags in a mod-specific implementation if needed. Do not assume Number List `shootOnCharge = 0` disables a truthiness-tested Common branch.

Optional aiming: use the existing `Assets/Toolkit/Prefabs/AimWidget.prefab`, position it consistently with the muzzle, and inject its component. AK47/UMP `OnGrab` enable the laser only when `Func_Laser_Aim` is unlocked/enabled. The current Common Lua fires along `shootPosition.forward`; the widget alone does not add target correction.

## Configure The Projectile And Feedback

1. Keep the projectile in this mod's `FlyObj/` folder. Prefer a distinct name such as `<ModName>Bullet` over another globally colliding `Bullet`. Rename/move through Unity with the `.meta` preserved, and update the gun ID and registration together.
2. Preserve one intended projectile behavior, its Rigidbody, collider reference, and `RagdollHitInfoObj` with the verified `Bullet` hit template. The Wizard's projectile uses `FlyObject`; AK47/UMP use `FlyObjectX` plus Common `BulletScript.txt`. A custom bullet Lua script is not required for a basic Wizard gun.
3. Configure `col`, `force`, `maxFlyTime`, `flyAtSpawn`, `forwardAlwayFollowVelocity`, `dealDmgAsArrow`, `collisionCount`, `collisionFlyCount`, and optional delay deliberately. These are projectile settings, not gun-body settings. Set gravity/drag and continuous collision detection for the desired trajectory; do not assume high-speed rounds cannot tunnel.
4. For an X bullet, verify its bare prefixed script ID and the script's packaged address. The copied Common `BulletScript` handles projectile collision callbacks, not constant-speed updates. Verify the actual projectile's runtime trajectory instead of assuming its speed remains constant.
5. Put custom AudioClips in `Audio/` and impact/muzzle/tail prefabs in `Effect/`. Copy only needed effects, such as an example `BasicSpark`, through Unity. Preserve lifecycle helpers; persistent trails/particles must reset when projectiles are reused.
6. Fill `FlyObject.shootSound`, `impactSound`, `shootEffect`, `impactEffect`, `tailEffect`, and optional decals with verified resource IDs. Custom sound/effect fields normally use the bare prefixed name, not an `Assets/...` path.
7. Prefer projectile `shootSound` for the current Common route. The tutorial's disabled root `SoundEffectPlayer` plus `shootSound` injection works only with a script that calls `PlaySound()`. Avoid duplicate playback if both routes are used.
8. Keep muzzle spawn outside the gun collider and configure shooter-collision ignore deliberately. Ignoring the gun's colliders is not automatically equivalent to ignoring every player-hand/body collider or implementing safe friendly-fire behavior.
9. Save/reopen both prefabs; first test a basic shot in the target game before adding reload, pooling-sensitive state, advanced effects, or networking.

## Metadata, Addresses, And Delivery

Configure `Config/<ModName>.asset`: `StoreItemInfo.addStoreItemName = <Prefix><WeaponPrefabName>`, `itemType = Weapon`, and player-facing `name`/`desc`. Set `loadOnSpawn` deliberately. Inspect copied `GazeObj.showName`/`showInfo`; stale nonempty example IDs are not repaired by a refresh.

Generate the icon with `Tools > Icon Generator`, replacing the Wizard icon in place without changing its GUID. Import at Max Size `256` and inspect framing.

Confirm the mod root and required Common root are registered, refresh via `Tools > Create And Refresh Addressables`, and check actual entries:

| Asset category | Generated logical address |
|---|---|
| Weapon | `Weapon/<Prefix><WeaponPrefabName>` |
| Projectile | `FlyObj/<Prefix><ProjectilePrefabName>` |
| Gun/bullet script | `LuaScript/<Prefix><ScriptFileBaseName>` |
| Sound | `Audio/Sound/<Prefix><ClipFileBaseName>` |
| Effect | `Effect/<Prefix><EffectPrefabName>` |
| Config | `Config/<Prefix><ConfigFileBaseName>` |
| Icon | `ICon/<Prefix><IconFileBaseName>` |

Run ItemInfoConfig `Check` and `ValidateAddressables`. Supplement them with exact, category-aware checks for the gun's `flyObjName`, Require targets, every sound/effect ID, duplicate addresses, inactive child scripts, and optional network registration. Current validation does not prove those contracts or Lua execution. Never use `ClearAddressables` as routine maintenance.

Multiplayer is opt-in: use the current network-capable spawn/authority pattern, inspect `networkPrefabRegister` (bare prefixed projectile IDs), and verify projectile/network-state compatibility. `AutoRegisterNetworkPrefab` collects names, not proof of support. UMP is not a verified drop-in multiplayer implementation. Enable `supportedMultiplayer` only after host/client testing.

Build/install only when requested. Follow the build skill and current `AddressabesBuilder.cs`/ADB scripts, not old tutorial menus or guessed package names. Product Name determines package output/install folders; inspect it before replacing an installed package. Preserve source assets. To package only this mod, inspect the selected roots/group entries and Common dependencies through Unity rather than deleting examples.

## Validation And Completion

After the first substantive change, run the focused Unity reload/reference check before expanding the work. For delivery, observe import/Console, metadata checks, address checks, target build, and installation separately.

Gameplay checks in Battle Talent:

- Mod discovery, store/spawn text/icon, primary/support grip, handedness, mount orientation, and gun-body collisions.
- Single/held/charged fire counts and cadence, muzzle direction, recoil/haptics, shot/impact audio, aiming unlock behavior, damage, and durability/mana.
- Projectile impacts on NPCs, environment, thin surfaces and moving targets; timeout/reuse behavior, shooter collisions, and absence of orphan effects or nil/load errors.
- When implemented: reload/ammo state and cancellation/drop/despawn cleanup; multiplayer authority, one spawn per intended shot, and host/client effects.
- Quest-specific materials, stereoscopic rendering, performance under sustained fire, and physical grip scale.

Report changed source assets and any creator entry point/log; selected trigger/projectile script contracts; checks actually run; and **build, file installation, mod discovery, gameplay, and multiplayer as separate statuses**. Mark every target-game/headset check not observed as unverified.
