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
- [`battle-talent-lua`](.agents/skills/battle-talent-lua/SKILL.md): Load when creating, changing, or debugging XLua `.txt` behavior, callbacks, injections, event lifetimes, `LuaBehaviour`, or `Require(...)` references.
- [`battle-talent-build`](.agents/skills/battle-talent-build/SKILL.md): Load when refreshing or validating Addressables, migrating prefixes, checking shaders, building, installing, reading packaged/runtime output, or diagnosing `Player.log`.

For a task that spans authoring, Lua, and delivery, load only the involved skills. For a general repository question that does not match these workflows, do not load a BattleTalent skill.

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