# Gun Examples And Version Contracts

Read only the relevant section. All repository paths below are relative to the toolkit root; Unity `Assets/...` paths refer to `ModProj`. These observations are static source/prefab evidence, not claims of successful gameplay.

## Source Map

| Question | Smallest useful source |
|---|---|
| Scaffold/category support | `ModProj/Assets/Toolkit/TemplateWizard/Scripts/TemplateWizardAsset.cs` |
| Default gun/shotgun/projectile serialization | `ModProj/Assets/Toolkit/TemplateWizard/Dummy/{Gun,Shotgun,Bullet}.prefab` |
| Current mod firing/spawn authority | `ModProj/Assets/Build/Common/Script/WeaponFlyObjBaseScript.txt` |
| Different Wizard fallback firing logic | `ModProj/Assets/Toolkit/TemplateWizard/Dummy/WeaponFlyObjBaseScript.txt` |
| Projectile collision script | `ModProj/Assets/Build/Common/Script/BulletScript.txt` |
| AK47 gun, injection, bullet, metadata | `ModProj/Assets/Build/Gun_AK47/{Weapon,Script,FlyObj,Config}` |
| UMP burst, injection, bullet, metadata | `ModProj/Assets/Build/Gun_UMP/{Weapon,Script,FlyObj,Config}` |
| Trigger fields/callback names | `ModProj/Assets/Toolkit/Scripts/InteractTrigger.cs`, `InteractTriggerX.cs` |
| Projectile fields/callback names | `ModProj/Assets/Toolkit/Scripts/FlyObject.cs`, `FlyObjectX.cs` |
| Injection storage/types | `ModProj/Assets/Toolkit/Scripts/LuaBehaviour.cs` |
| Grip and fitting helper | `ModProj/Assets/Toolkit/Prefabs/HandAttach.prefab`, `ModProj/Assets/Toolkit/Scripts/AttachPoint.cs`, `ModProj/Assets/Toolkit/HandPoseHelper/support/HandPoseHelper.cs` |
| Damage/material vocabulary | Nearby prefab, `Readme.md`, and verified hit/material config or generated API docs |
| Prefix/address generation and validation | `ModProj/Assets/Toolkit/Scripts/Addressable/{AddressableConfig,AddressableHelper}.cs` |
| Network prefab declaration/registration | `ModProj/Assets/Toolkit/Scripts/ItemInfoConfig.cs` |
| Delivery destination | `ModProj/Assets/Editor/AddressabesBuilder.cs`, `ModProj/ADBTools/InstallModToQuest.bat` |

For game APIs not implemented in the toolkit, inspect only the needed generated page, commonly `docs/class_cross_link_1_1_interact_trigger_x.html`, `class_cross_link_1_1_fly_object_x.html`, or `class_cross_link_1_1_weapon_fly_obj_base.html`.

Use the published community tutorial linked from `SKILL.md` for workflow background; no local documentation checkout is required. Current toolkit source overrides older tutorial screenshots or steps. The tutorial does not authorize source cleanup.

## AK47 And UMP

| Contract | AK47 | UMP |
|---|---|---|
| Weapon file | `Weapon/Gun_AK47.prefab` | `Weapon/Gun_UMP.prefab` |
| Trigger | `InteractTriggerX` | `InteractTriggerX` |
| Script ID | `WMD_GunAKScript` | `WMD_GunUMPScript` |
| Projectile ID | `WMD_BulletAK` | `WMD_UMPBullet` |
| Projectile behavior | `FlyObjectX`, `WMD_BulletScript` | `FlyObjectX`, `WMD_BulletScript` |
| Immediate/charge baseline | `instantSkill = false`, charge end `-1` | `instantSkill = true`, charge end `1` |
| Projectile speed/lifetime | `force = 25`, `maxFlyTime = 8` | `force = 200`, `maxFlyTime = 3` |
| Sound/effect | Impact `WMD_akimpact`, `WMD_BasicSpark` | Shoot `WMD_shoot`; impact `WMD_bulletImpact`, `WMD_BasicSpark` |
| Networking metadata | `supportedMultiplayer = true`, `networkPrefabRegister` includes `WMD_BulletAK` | Older sparse config; do not infer network support |

Adapt these values, do not prescribe them as realistic ballistics or balance defaults.

AK47's root GameObject is `GunAK47`, while its asset is `Gun_AK47.prefab`. Its copied Gaze name is `WMD_GunAK47`, while StoreItemInfo uses `WMD_Gun_AK47`. Address generation follows the **asset path/name**, not the root name. Keep new assets consistent and repair copied Gaze IDs through Unity.

UMP's Lua table defaults `releaseBulletNumber = 5`; its prefab injects `6`. Inspect the injection bridge/runtime before deciding the effective value. Do not document either number as an unconditional constant.

UMP's root `SoundEffectPlayer` still names `WMD_akshot`, despite its own Audio folder containing `shoot.wav` and `bulletImpact.wav`. The projectile has its own shoot sound. Audit dependency closure rather than shipping accidental AK47 sound dependencies.

### UMP Burst Safety

`GunUMPScript:OnChargeRelease()` replaces a scheduler and repeatedly calls `PlayShootAction()`. It then dereferences the result and sets `hitMlp = 101` and `damageMlp = 1.5`.

- The override has no non-server authority guard, unlike the Common implementation.
- Common `SpawnBullet` intentionally produces no projectile on an online non-server client. Failed resource loads can also yield no usable projectile.
- Guard the result before accessing hit information. Preserve the authority gate when overriding shooting callbacks.
- Validate duration and a positive integral count before scheduling. Do not divide by zero.
- Track/cancel burst and recoil schedules at the appropriate verified trigger lifecycle boundaries. Use callbacks actually dispatched by `InteractTriggerX`; do not assume arbitrary Unity callbacks reach this Lua host.
- Confirm the observed burst count/timing in game. Scheduler endpoint semantics and callback overrides can change the count.

`releaseBulletTime`/`releaseBulletNumber` control UMP's custom burst only. They do not rate-limit ordinary Common held fire.

## Current Common Versus Wizard Dummy

| Behavior | Existing `Build/Common` script | `TemplateWizard/Dummy` fallback script |
|---|---|---|
| `SpawnBullet` signature | `(flyObjName, position, rotation, vel)` | `(interact)` |
| Networking | Server-authoritative `NetworkCreateFlyObj` plus RPC effect path | Local `CreateFlyObj` |
| Fire-rate control | No `shootInterval` read | Reads `shootInterval` |
| Pellets/spread | No `bulletsPerShot`/`bulletSpread` read | Reads both fields |
| Root shootSound injection | Not played | Calls `shootSound:PlaySound()` |
| Recoil | `OnShoot` -> `AddRecoil` | Inline `PlayShootAction` |

The Wizard copies the fallback **only when Common is missing**, so the same Gun/Shotgun prefab does not guarantee the same behavior across checkouts. Never replace the existing shared script blindly or call one version's `SpawnBullet` with another version's arguments.

The fallback also calls `TryAerialStay(ph)` inside `SpawnBullet` without declaring `ph` there, and its collision-ignore branch dereferences the projectile without a preceding null guard. If adapting that route, use a mod-specific script, verify the hand reference, add nil/authority guards, and test it. Do not present the fallback as a prevalidated shotgun implementation.

### Numeric Flags

Common uses `if self.shootOnCharge or self.shootOnCharge == 1`, likewise for `shootOnActivateEnd` and `autoSetupBulletIgnore`. In Lua, numeric `0` still passes the first expression. By contrast, its immediate-shot expression explicitly handles `dontShootOnInstant == 0`. Inspect each expression independently.

When a mod-specific script accepts booleans and Number List flags, use an explicit normalization such as `value == true or value == 1`. Do not edit Common to fix a single gun, and do not claim a numeric injection is a Lua boolean.

### Projectile Versions

Wizard `Bullet.prefab` uses plain `FlyObject`, no custom bullet script, gravity off, speed `10`, and a verified `Bullet` hit template. It is a starting asset, not a finished firearm projectile.

AK47/UMP use `FlyObjectX` and Common `BulletScript`. That script handles collisions with other FlyObjects via `OnCollision`/`RpcOnCollision`; it contains no constant-speed update. A projectile's name or initial launch speed does not prove its runtime ballistics.

## Firing And Projectile Semantics

Use the following behavior distinctions when adapting the shipped toolkit examples. They are design decisions to verify in the selected implementation, not requirements to obtain additional game-source assets.

| Behavior | Meaning for a mod |
|---|---|
| `flyObjName` | Use the exact bare prefixed ID of the mod's own registered projectile |
| `shootVel = -1` | Use the projectile's `force`; a nonnegative override selects launch speed, not a promise of constant speed |
| Immediate shot plus charged release | Two distinct trigger paths; define whether each path fires and test their combined count |
| Burst duration/count | Timed repeated shots, not simultaneous shotgun pellets or a magazine system |
| `HandAttach` / `MountAttach` / `shootPosition` | Separate grip, storage, and muzzle roles; fit each to the current model |
| Recoil vector/curve and force mode | Tune together with Rigidbody mass, damping, and single-/two-hand handling |

UMP's `OnChargeRelease` schedules its burst without first calling the inherited charged-shot callback. If a new override invokes a base shot and also schedules a burst, account for the additional shot explicitly. The example assigns `damageMlp = 1.5`; multiplying instead would preserve an existing multiplier but can accumulate changes on reused projectiles unless state resets. Decide damage and durability behavior intentionally rather than treating callback inheritance as neutral.

Current Common Lua fires along the muzzle's forward axis and does not automatically play a shoot Animator or apply aim-target correction. Merely injecting `shootAnim` or `aimWidget` cannot enable functionality the selected script never reads. Implement optional animation/target correction in a mod-specific script only after verifying the necessary game APIs in toolkit docs and examples.

Gravity, drag, collision handling, and any projectile-specific runtime behavior can affect velocity after launch. Common `BulletScript` alone does not maintain speed. If constant-speed behavior is required, verify the supported projectile APIs and callbacks before implementing it; a Unity FixedUpdate method is not automatically dispatched to a `FlyObjectX` Lua script. Toolkit serialization stubs and editor previews do not prove game execution.

## Focused Troubleshooting

| Symptom | First inexpensive check |
|---|---|
| Gun appears but will not fire | Primary grip `allowTrigger`, trigger `interact`, mana/durability, exact script ID and injections |
| No projectile or a nil error after firing | Exact `FlyObj/` entry, `flyObjName`, selected FlyObject component and server/client authority |
| Shotgun behaves like a single gun | Whether the actual Common script reads pellet/spread fields |
| Fire cadence changes with frame rate | Whether `UpdateSkill` has an explicit interval guard |
| Gun is silent | Which implementation owns playback; Audio/Sound entry and stale AK47 injection |
| Immediate self-impact | Muzzle inside body collider, ignore flag truthiness, collider ignore coverage |
| UI damage changes but hits do not | `skillDamage` is display-only; inspect projectile hit config/multipliers |
| Wrong store/gaze text | Asset-name-based Weapon address versus root name and stale nonempty Gaze identifiers |
| Duplicate rounds/errors on clients | Missing override authority guard, unsafe nil dereference, or network prefab/state setup |
| Install succeeds but mod is missing | Actual target game's install path/package, Product Name, platform bundle and current output |
