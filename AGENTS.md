# BattleTalent Mod Toolkit Agent Guide

## Scope And Authority

This file applies to the entire repository. The Unity project is under `ModProj/` and targets Unity 2020.3.48f1, Windows PC VR, and Meta Quest Android.

- Treat `ModProj/Assets/Toolkit`, `ModProj/Assets/Editor`, and the nearest working example in `ModProj/Assets/Build` as implementation authority.
- Use `Readme.md` for the scripting API overview, common components, resource path concepts, and debugging entry points.
- Use `docs/index.html` as the generated API index. Open only the class pages needed for the current task.
- Treat `ModProj/PROJECT_PROMPT.md` as orientation, not an API contract.
- Verify package versions in `ModProj/Packages/manifest.json` before using package-specific APIs. Important pinned versions include Addressables 1.19.18 and URP 10.10.1.

## Available Skills

Before exploring or editing for a task, compare the request with the triggers below. When a trigger matches, read the linked `SKILL.md` and follow it. Do not load all skills by default; load multiple skills only when the task crosses their boundaries.

- [`battle-talent-mod-authoring`](.agents/skills/battle-talent-mod-authoring/SKILL.md): Load when creating or modifying a mod, prefab, scene, `ItemInfoConfig`, component setup, toolkit C#, or editor C#.
- [`battle-talent-melee-weapon-authoring`](.agents/skills/battle-talent-melee-weapon-authoring/SKILL.md): Load when creating or modifying a melee weapon Mod such as a knife, sword, axe, hammer, mace, club, or staff, especially when collider, hit, grip, mount, icon, metadata, or `StabObject` setup is involved.
- [`battle-talent-gun-mod-authoring`](.agents/skills/battle-talent-gun-mod-authoring/SKILL.md): Load when creating or modifying a firearm Mod such as a pistol, rifle, SMG, or shotgun, including its gun prefab, grips, trigger scripts, projectile, recoil, sounds, effects, and metadata. Use toolkit templates and AK47/UMP as references; do not use for bows, spells, or standalone projectiles.
- [`battle-talent-lua`](.agents/skills/battle-talent-lua/SKILL.md): Load when creating, changing, or debugging XLua `.txt` behavior, callbacks, injections, event lifetimes, `LuaBehaviour`, or `Require(...)` references.
- [`battle-talent-build`](.agents/skills/battle-talent-build/SKILL.md): Load when refreshing or validating Addressables, migrating prefixes, checking shaders, building, installing, reading packaged/runtime output, or diagnosing `Player.log`.
- [`battle-talent-song-mod-authoring`](.agents/skills/battle-talent-song-mod-authoring/SKILL.md): Load when creating or modifying a standalone SongMod from licensed audio and artwork; not for generic sound effects or 3D model generation.
- [`projsao-avatar-mod-authoring`](.agents/skills/projsao-avatar-mod-authoring/SKILL.md): Load when creating or modifying a ProjSAO-targeted AvatarMod, including Humanoid setup, `RuntimeAvatarBuilder`, avatar hand poses, slots, or optional clothing physics.
- [`projsao-role-mod-authoring`](.agents/skills/projsao-role-mod-authoring/SKILL.md): Load when creating or modifying a ProjSAO-targeted NPC RoleMod, including Humanoid role prefabs, `RoleModInfo`, weapon slots, armor, custom AI actions, dismemberment, or body grab points.
- [`projsao-skin-mod-authoring`](.agents/skills/projsao-skin-mod-authoring/SKILL.md): Load when creating or modifying a legacy ProjSAO player SkinMod, including stock-rig mesh binding, `SkinInfo`, mesh-root layout, first-person head hiding, or Skin Addressables.
- [`tripo-battletalent-workflow`](.agents/skills/tripo-battletalent-workflow/SKILL.md): Load when determining an asset's intended use, turning player model prompts into reviewed concept images and approved Tripo models, refining Tripo assets, or handing them off to category-specific BattleTalent authoring.

For a task that spans authoring, Lua, and delivery, load only the involved skills. For a general repository question that does not match these workflows, do not load a BattleTalent skill.

## Tripo Workflow

- Establish model-only versus existing/new mod work, category, and platform before concept generation. Do not ask for a game version; use the current toolkit contract and report runtime compatibility only when observed. Do not default to Weapon; clarify ambiguous NPC Role, player Avatar, and legacy Skin requests.
- Read the matching authoring skill for source requirements, then generate and show a concept image. Wait for explicit approval of that image before Tripo image-to-3D. Do not use text-to-3D unless the player explicitly chooses to skip concept review.
- Generate concept images with Tripo-native text-to-image by default, using existing Tripo credentials and credit approval, even if built-in image tools are available. Use another provider only when explicitly selected; a supplied image approved for use as-is skips generation. Do not require an OpenAI API key for the default route.
- After model approval, hand off to the matching existing authoring skill for Unity configuration and validation. For model-only requests, deliver outputs and stop without creating a mod or changing Addressables.
- For automatically generated Avatar/NPC models, reuse suitable binding or obtain approval for rig-check and rigging and apply focused source checks. Player-provided models explicitly declared rigged use the direct-authoring exception below. Do not auto-rig weapons, static props, or legacy stock-rig skins, or add paid animations without a request.
- On automated Avatar/NPC rig-check, rigging, or source-suitability failure, stop and offer manual correction in Mixamo or Tripo Studio. When the player supplies a corrected rig and asks to continue, accept it and proceed to mod creation; do not regenerate, audit, re-rig, or stop at a handoff report.
- Player-provided models explicitly declared rigged go directly to Avatar/NPC creation without separate rig, weight, joint, or deformation audits. This overrides source-preflight instructions; keep normal import/builder error handling and investigate only actual failures. Final save/reference/identifier checks remain, not a repeated visual acceptance pipeline.
- Preserve all models' existing imported size and unit/scale settings unless the player explicitly requests resizing. Do not invent a target height, normalize against a template, bake corrective scale into bones/meshes, or add compensating scale parents. Fit slots/grips/colliders to the unchanged model.
- Route NPCs to `projsao-role-mod-authoring`, Avatars to `projsao-avatar-mod-authoring`, firearms to `battle-talent-gun-mod-authoring`, and melee weapons to `battle-talent-melee-weapon-authoring`. Use safe authoring defaults for unspecified routine options and report them; ask only for actual blockers or explicit approval requirements.
- If Tripo or Node.js is unavailable, direct the user to run `Setup-TripoWorkflow.cmd`.
- Treat `tripo-out/` as disposable local staging. Move only approved assets into the intended mod's source layout when Unity integration is requested.
- Never request, display, or commit Tripo API keys, OAuth data, or user-level CLI configuration.
- Confirm before any generation, retry, batch, rigging, animation, or conversion that may spend Tripo credits.
- Resolve deployment from active code and the invoked installer, not disabled `#if false` snippets. Missing deployment evidence must not block concept/model preparation or trigger game-version questions.

## Working Method

- Start from the smallest concrete owner: a failing behavior, source file, prefab/component, config asset, menu command, or nearby example.
- Before editing, state one falsifiable local hypothesis and the cheapest check that could disprove it.
- Prefer a small change following a nearby implementation over a new abstraction or broad cleanup.
- After the first substantive edit, run the focused check before widening the change.
- Do not infer game-side `CL.*` APIs that are absent from this repository. Check `Readme.md`, the exact generated page under `docs/`, and a working Lua example.

## Always-On Repository Rules

- Use a live Unity Editor as the authority for imported assets, serialized references, compilation, and Addressables state.
- Do not manually rewrite serialized `.prefab`, `.unity`, `.asset`, or `.meta` YAML. Use the Unity Editor so object references and GUIDs remain valid.
- Preserve `.meta` files when moving or renaming assets; never regenerate a referenced asset GUID.
- Never edit generated `.csproj`/`.sln`, `ModProj/Library`, `ModProj/Temp`, `ModProj/obj`, package cache, or generated bundle output as source.
- Keep runtime and editor C# dependencies separated. Runtime code must not import `UnityEditor` outside an editor-only assembly or `#if UNITY_EDITOR` boundary.
- After changing C#, refresh/import in Unity, wait for compilation and domain reload, and require a clean Console before using the changed type.
- Change third-party code under Mirror, Plugins, SecondParty, or TextMesh Pro only when the task explicitly owns it.
- Do not report Unity import, Lua execution, Addressables build, game runtime, or headset installation as successful unless it was observed.
- Require explicit confirmation before any operation that removes source assets. Generated output cleanup may run only when required by the requested build workflow.

## Completion Report

Report changed source assets, checks actually run, Addressables/build/install impact when applicable, and checks that still require BattleTalent or a physical headset. Distinguish build success, file installation, mod discovery, and gameplay verification.
