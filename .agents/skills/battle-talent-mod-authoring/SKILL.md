---
name: battle-talent-mod-authoring
description: 'Create and modify BattleTalent mods and Unity content in ModProj. Use for Template Wizard scaffolding, weapons, spells, songs, scenes, roles, skins, avatars, prefabs, ItemInfoConfig assets, component setup, and toolkit or editor C#.'
---

# BattleTalent Mod Authoring

Load this skill for source and asset authoring. If the task changes XLua behavior, also load `../battle-talent-lua/SKILL.md`. If it changes Addressables or reaches build/install validation, also load `../battle-talent-build/SKILL.md`.

## Read Only What Is Needed

- Read `../../../Readme.md` for common component semantics and links to the community documentation.
- Use `../../../docs/index.html` as an API index; open only the exact class page needed.
- Treat current code in `../../../ModProj/Assets/Toolkit`, `../../../ModProj/Assets/Editor`, and the closest shipped example as authoritative when generated docs are older or broader.

## Ownership Boundaries

| Path | Ownership |
|---|---|
| `ModProj/Assets/Build` | Mod source, shared mod scripts, and shipped examples |
| `ModProj/Assets/Toolkit` | Reusable framework, templates, inspectors, and runtime components |
| `ModProj/Assets/Editor` | Project-wide editor and build tooling |
| `ModProj/Assets/Resources` | Toolkit configuration assets |
| `ModProj/Assets/AddressableAssetsData` | Unity-generated Addressables state; manage through Unity |
| `ModProj/Assets/Mods` | Disposable generated bundle output |
| `ModProj/ADBTools` | Bundled Quest deployment tooling |

For one mod's behavior, prefer content under `ModProj/Assets/Build/<ModName>`. Change `Assets/Toolkit` only for genuinely reusable behavior.

## Scaffold A Mod

Keep mod content under `ModProj/Assets/Build/<ModName>`. Prefer Unity menu `Tools > Template Wizard`; it selects `Assets/Resources/TemplateWizard.asset`, whose Inspector exposes `Generate Template`.

Names accepted by the wizard are nonempty, contain no spaces, begin with an uppercase character, and are unique under `Assets/Build`.

| Category | Generated folders under `Assets/Build/<ModName>` | Config member |
|---|---|---|
| Weapon | `ICon`, `Config`, `Weapon` | `storeItemInfo` |
| Gun or Shotgun | Weapon folders plus `FlyObj`; ensures the Common weapon script | `storeItemInfo` |
| Song | `ICon`, `Config`, `Audio` | `storeItemInfo` |
| Scene | `ICon`, `Config`, `Scene`; ensures the CommonScene init script | `sceneModInfo` |
| Role | `ICon`, `Config`, `Audio`, `Role` | `roleModInfo` |
| Skin | `ICon`, `Config`, `Skin` | `skinInfo` |
| Avatar | `ICon`, `Config`, `Avatar` | `avatarInfo` |

Weapon subtypes are Sword, Gun, Axe, and Shotgun. `ICon` is intentional; preserve its spelling.

The wizard creates the initial `ItemInfoConfig`, prefixes category identifiers, registers the mod root in `AddressableConfig.addressablePaths`, and refreshes Addressables. If the wizard cannot express the requested category, copy only the necessary structure from the closest example. Check the folder-to-address mapping near the top of `ModProj/Assets/Toolkit/Scripts/Addressable/AddressableHelper.cs` before inventing a folder name.

Common optional folders include `Script`, `Effect`, `FlyObj`, `SceneObj`, `HandPose`, `Wave`, `UI`, `BrokenArmor`, `ArmorProfile`, and `Action`.

## Choose A Nearby Example

| Need | Start from |
|---|---|
| Minimal melee weapon | `ModProj/Assets/Build/Stick` |
| Gun and projectile | `ModProj/Assets/Build/Gun_UMP` or `Gun_AK47` |
| Bow | `ModProj/Assets/Build/Bow_Simple` |
| Networked bow | `ModProj/Assets/Build/Bow_Simple_Network` |
| Weapon hit event | `ModProj/Assets/Build/Dagger_Bleed` |
| Throwable/explosive weapon | `ModProj/Assets/Build/Dagger_Explode` |
| Wand/projectile effect | `ModProj/Assets/Build/Wand_FireBall` |
| Spell inheritance | `ModProj/Assets/Build/Spell_FireBall` |
| Scene initialization | `ModProj/Assets/Build/CommonScene` and `SimpleScene` |
| Larger game mode | `ModProj/Assets/Build/SurvivorMode` |
| Song, role, skin, or avatar | The matching `ModProj/Assets/Build/Simple*` example |

Inspect one close example rather than copying a whole complex mod. Prefer the current shared script in `Assets/Build/Common` or `CommonScene` over a differently evolved dummy in `Assets/Toolkit/TemplateWizard/Dummy`.

## C# Rules

- Follow the existing `CrossLink` namespace and local Unity component patterns.
- Use APIs available in Unity 2020.3 and the versions pinned in `Packages/manifest.json`.
- Keep runtime and editor dependencies separated.
- After each C# change, refresh Unity, wait for domain reload, and inspect the Console before attaching or using the type.
- Do not refactor examples or third-party code while implementing an unrelated mod feature.

## Prefabs, Scenes, And Configs

- Create and modify GameObjects, components, prefabs, scenes, materials, and ScriptableObjects in Unity. Do not hand-edit their YAML.
- Inspect the component that owns the behavior and change only required serialized fields.
- Save prefab stages and scenes explicitly, reopen them, and verify references in the Inspector.
- Every shippable category uses an `ItemInfoConfig` under its `Config` folder. Configure it in the Inspector.
- `loadOnSpawn = true` defers loading until spawn; `false` loads during mod installation.
- Run the ItemInfoConfig Inspector `Check` action, but note that current checks omit Avatar identifiers and do not prove runtime behavior.
- The canonical scene template supplies its VR camera through runtime scene initialization. Do not add a standalone Camera unless the mod design and a nearby scene pattern require one. Preserve the template's main lighting unless intentionally replacing it.

## Authoring Check

1. Verify folder/name conventions and compare the owning prefab/config against one nearby example.
2. Import and compile in Unity; require no new Console errors.
3. Reopen touched prefabs/scenes and check missing scripts and object references, including inactive children.
4. For config or shippable asset changes, continue with the Addressables and build skill.