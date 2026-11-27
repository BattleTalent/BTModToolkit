---
name: battle-talent-song-mod-authoring
description: Create, validate, build, and prepare a Battle Talent SongMod in ModProj from licensed audio and artwork. Use for Song templates, Music ItemInfoConfig setup, Audio and ICon assets, SongMod Addressables, installation, testing, or publishing preparation; do not use for general sound effects or scripted music playback inside another mod.
---

# Battle Talent SongMod Authoring

Use this skill for a standalone song that should appear in Battle Talent's music selection. For generic mod structure, also follow `../battle-talent-mod-authoring/SKILL.md`. When refreshing Addressables, building, installing, or reading runtime logs, follow `../battle-talent-build/SKILL.md` as well.

The community tutorial is useful background: <https://battletalent.github.io/community-docs/docs/support-mod-types/Song/Tutorials/create-a-song-mod>. Current repository code and a live Unity Editor override old tutorial text.

## Hard Safety Boundary

- Never run `Tools > Destructive > Cleanup All Content`. The tutorial's “Remove unused content / Cleanup All Content” step is obsolete and wrong; the menu command has been removed, and its implementation can replace source trees under `Assets/Build` and `Assets/Resources`.
- Do not run `Clear Addressables` as routine setup. The current implementation removes the default Addressables group. Use `Tools > Create And Refresh Addressables` instead.
- Do not manually edit `.asset`, `.meta`, prefab, or scene YAML. Import, rename, configure, and save assets through Unity so GUIDs and references remain valid.
- Do not remove existing source assets without explicit confirmation. Preserve every `.meta` file when moving or renaming an imported asset.

## Establish The Mod Identity

Before authoring, determine:

- a unique product/mod name such as `ABC_MySongPack`;
- a globally unique Addressables prefix ending in `_`, such as `ABC_MySongPack_`;
- a PascalCase mod folder and asset base name without spaces, such as `MySong`;
- the song display name and description;
- the target platform: Windows PC VR, Meta Quest, or both;
- proof that the audio and icon may legally be redistributed, plus the credit/license text required for publishing.

Use a fresh toolkit checkout when changing global project identity. `Application.productName` controls the built and installed mod folder, while `Assets/Resources/AddressableConfig.asset` supplies the prefix for every generated address in that Unity project. In a populated shared project, inspect the existing values and ask before changing either globally.

## Create The Song Template

Perform asset creation in the live Unity 2020.3.48f1 Editor.

1. Set `Edit > Project Settings > Player > Product Name` to the unique product/mod name when this project is dedicated to that mod.
2. Select `Assets/Resources/AddressableConfig.asset` and set `Prefix` to the unique prefix, including its trailing underscore, only when the project identity is meant to change.
3. Open `Tools > Template Wizard` and keep its asset visible in the Inspector.
4. Enter the unique mod folder name. It must be nonempty, contain no spaces, start with an uppercase character, and not already exist under `Assets/Build`.
5. Set `Mod Type` to `Song` and fill the `StoreItemInfo` fields.
6. Confirm `addStoreItemName` is `<Prefix><AssetBaseName>`, `itemType` is `Music`, `name` is the player-facing title, and `desc` is meaningful. Leave dependencies empty unless the SongMod genuinely requires another mod. Keep `loadOnSpawn` enabled unless there is a tested reason to load during installation.
7. Click `Generate Template` once and wait for Unity import and Addressables refresh to finish.

The expected result is:

```text
Assets/Build/<ModFolderName>/
|-- Audio/
|-- Config/<ModFolderName>.asset
`-- ICon/<ModFolderName>.png
```

The wizard creates the config, a placeholder icon, registers the mod root in `AddressableConfig.addressablePaths`, and refreshes Addressables. Compare the result with `Assets/Build/SimpleBackgroundSong` when anything differs.

## Import The Song And Icon

Import assets through Unity and wait for each import to complete.

- Put one distributable `.mp3` in `Audio` and name it `<AssetBaseName>.mp3`. Its generated address must become `Audio/Sound/<Prefix><AssetBaseName>` and therefore match `StoreItemInfo.addStoreItemName` after the `Audio/Sound/` segment is removed.
- Replace the generated placeholder in `ICon` with a square `.png` named `<AssetBaseName>.png`; do not leave both placeholder and final icons with competing base names.
- In the icon importer, use `Sprite (2D and UI)`, enable `Read/Write`, set Max Size to `256` or smaller, and click `Apply`. The tutorial recommends 256 px; the shipped example uses 128 px, which is also valid.
- Reopen the generated `ItemInfoConfig` and verify `Music`, the identifier, display name, description, dependencies, and `loadOnSpawn` after the imports.

Avoid changing audio import settings without a demonstrated playback, memory, or platform need. Treat the shipped `SimpleBackgroundSong.mp3` importer as the nearest baseline.

## Validate Before Building

Use the cheapest failing check first:

1. Confirm the folder is exactly `Audio`, `Config`, and `ICon` (`ICon` capitalization is intentional), with one intended song and icon.
2. Confirm the audio and icon base names match the suffix of `addStoreItemName`, and the config uses `ItemType.Music`.
3. Confirm the mod root is present exactly once in `AddressableConfig.addressablePaths`.
4. Run the config Inspector's `Check` action if available.
5. Run `Tools > Create And Refresh Addressables`, inspect the default group, and run `ValidateAddressables` from `AddressableConfig`.
6. Require the expected addresses for config, audio, and icon and no new Unity Console errors. Do not continue after a failed import or validation check.

Do not report Unity import or Addressables validation as successful unless it was observed in the live Editor.

## Build, Install, And Test

Use the platform-specific workflow from `battle-talent-build`:

- Windows PC VR: `BuildTools > FastBuildAndInstallForWindows`.
- Meta Quest: require a connected, developer-enabled, USB-debug-authorized headset, then use `BuildTools > FastBuildAndInstallForAndroid`.
- Use `BuildTools > BuildAllBundles` only when both bundles are actually requested and explicitly verify Android texture settings and output.

Record build and installation separately. After launching Battle Talent, verify in order:

1. the mod is discovered;
2. the song entry and icon appear;
3. selecting the entry plays the intended full track;
4. pause, next/previous track, scene changes, and restart do not produce new errors;
5. the target platform log contains no missing Addressable, decode, or mod-loading errors.

Quest installation is not gameplay verification. A successful bundle build is not proof that files were installed or that the song plays.

## Publishing Preparation

Prepare a zip only from the generated mod output for the intended platform, not from the Unity source project. Include accurate title, description, target platform, dependencies, credits, and license notices. Do not upload or publish to mod.io unless the user explicitly asks for that external action; account authentication and the final submission remain separate from creating the SongMod.

## Completion Report

Report:

- created or changed source assets under `Assets/Build/<ModFolderName>`;
- product name, Addressables prefix, and final generated addresses;
- checks actually observed in Unity;
- per-platform build output;
- whether files were installed, Battle Talent discovered the mod, and playback was verified;
- any remaining PC VR, Battle Talent, licensing, mod.io, or physical-headset checks.
