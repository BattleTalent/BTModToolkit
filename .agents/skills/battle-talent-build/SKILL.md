---
name: battle-talent-build
description: 'Manage BattleTalent Addressables and delivery. Use for addressablePaths, prefixes, validation, shader checks, Windows PC VR builds, Meta Quest Android builds, ADB installation, generated output, Player.log, and packaged runtime diagnosis.'
---

# BattleTalent Addressables, Build, And Runtime Validation

Load this skill only when a task affects asset registration, identifiers, packaging, installation, or post-package behavior. Load the authoring or Lua skill as well only when those sources must change.

## Existing Documentation And Authority

- Read the resource-path and debugging sections of `../../../Readme.md` when background is needed.
- The tracked `../../../docs/addressable_1.png` through `addressable_4.png` illustrate the Addressables setup.
- Current behavior is controlled by `ModProj/Assets/Toolkit/Scripts/Addressable/AddressableConfig.cs`, `AddressableHelper.cs`, and `ModProj/Assets/Editor/AddressabesBuilder.cs`; source overrides older screenshots or generated docs.

## Addressables Workflow

1. Confirm every shippable mod root appears in `ModProj/Assets/Resources/AddressableConfig.asset` under `addressablePaths`.
2. Run Unity menu `Tools > Create And Refresh Addressables`, or select `Tools > Select Addressables Config` and use `CreateAndRefreshAddressableName` in the Inspector.
3. Inspect relevant entries in the default Addressables group.
4. Run the AddressableConfig `ValidateAddressables` action before building.

Treat generated addresses as contracts. When renaming content, update ItemInfoConfig identifiers, serialized prefab Lua IDs, `Require(...)` references, and Addressables entries together.

Addressables validation checks selected ItemInfoConfig identifiers and prefab Lua IDs against final address segments. It does not validate Lua syntax, callback behavior, Avatar identifiers, or Lua components on inactive child GameObjects. A refresh may retain stale entries for removed content, so inspect the group after renames or deletions.

Do not use `ClearAddressables` as routine maintenance: the current implementation removes the default Addressables group itself, not just its entries.

## Prefix Migration Hazards

Do not run `ModifyPrefixInPathsPrefabsAndScripts` blindly. The current implementation has gaps that require inspection before and after migration:

- Scene migration can replace Lua string injection values with the migrated script ID.
- Role migration omits `leftWeapon` and `rightWeapon`.
- Hand-pose migration omits `leftHandPreset` and `rightHandPreset`.
- Prefab, scene, and validation scans omit inactive child Lua components.

If migration is required, preserve a reviewable backup, use Unity to inspect and repair affected serialized fields, include inactive descendants, then save the config and run `CreateAndRefreshAddressableName` again. Inspect regenerated entries and run `ValidateAddressables`. Do not hand-edit serialized YAML.

## Validation Ladder

Run the cheapest applicable checks in order and repair the same slice when one fails:

1. Static structure: path/name conventions, Lua `return Class(...)`, `Require(...)`, prefab script ID shape, injections, event cleanup, and config prefixes.
2. Unity import and C# compilation: refresh, wait for domain reload, and require no new Console errors.
3. Asset integrity: reopen/save touched prefabs or scenes and verify no missing scripts or references, including inactive children.
4. Config and Addressables: run ItemInfoConfig `Check`, refresh Addressables, inspect entries, and run `ValidateAddressables` while accounting for its omissions.
5. Shader compatibility: when shaders change, run `BuildTools > CheckShaders` and test visuals on the target VR platform.
6. Build the requested target with its platform-specific command.
7. Install and run in BattleTalent or the supported simulator; inspect `Player.log` for runtime and Lua failures.

There is no repository-wide automated test suite or standalone Lua parser. Focused EditMode or PlayMode tests may be used when a relevant test assembly exists, but IDE compilation does not replace Unity compilation.

## Windows PC VR

- Command: `BuildTools > FastBuildAndInstallForWindows`.
- Output: `ModProj/Assets/Mods/<ProductName>/StandaloneWindows`.
- Install: `%USERPROFILE%/AppData/LocalLow/CyDream/BattleTalent/Mods/<ProductName>`.
- `Tools > Open PC Mod Folder` reveals the PC mod/log area.

## Meta Quest Android

- Require a connected, developer-enabled, USB-debug-authorized headset visible to `ModProj/ADBTools/adb.exe`.
- Command: `BuildTools > FastBuildAndInstallForAndroid`.
- Output: `ModProj/Assets/Mods/<ProductName>/Android`.
- Install: `/sdcard/Android/data/com.CyDream/BattleTalent/files/Mods/<ProductName>`.
- Observe both Unity Console and ADB output before reporting installation success.

The output and install folder use `Application.productName`; the repository default is `ModProj`.

`BuildTools > BuildAllBundles` builds Android and Windows without explicit install steps. Its current Android ASTC initialization condition is suspicious; prefer platform-specific fast commands when correctness matters, or explicitly verify Android texture settings and output.

## Destructive Operations

- `BuildTools > ClearOldFiles` recursively deletes generated output under `ModProj/Assets/Mods`.
- AddressableConfig `ClearAddressables` removes the default Addressables group.
- `Tools > Destructive > Cleanup All Content` replaces `ModProj/Assets/Build` and `ModProj/Assets/Resources` while preserving only a small allowlist.

Require explicit confirmation before `Cleanup All Content` or any operation that removes source assets.

## Completion Evidence

Report separately whether bundles built, files installed, BattleTalent discovered the mod, and gameplay was verified. Include output/install paths and any remaining checks requiring BattleTalent or a physical headset.