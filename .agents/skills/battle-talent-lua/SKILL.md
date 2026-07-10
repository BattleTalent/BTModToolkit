---
name: battle-talent-lua
description: 'Create, modify, and debug BattleTalent XLua behavior scripts. Use for Assets/Build Script .txt files, LuaBehaviour, InteractTriggerX, FlyObjectX, callbacks, injections, event cleanup, Require addresses, and Player.log Lua failures.'
---

# BattleTalent XLua Development

Load this skill only for Lua behavior or its C# bridge contract. Load `../battle-talent-mod-authoring/SKILL.md` when prefab/component authoring is also required, and `../battle-talent-build/SKILL.md` when scripts must be registered, packaged, installed, or runtime-tested.

## API Sources

- Start with `../../../Readme.md`, especially Lua Host Classes, common components, resource paths, debugging, and log locations.
- Search `../../../docs/index.html` and open only relevant generated pages, such as `docs/class_cross_link_1_1_lua_behaviour.html`, `class_cross_link_1_1_interact_trigger_x.html`, or `class_cross_link_1_1_fly_object_x.html`.
- For callback names and parameters, prefer the current bridge source in `ModProj/Assets/Toolkit/Scripts` and a working script in `ModProj/Assets/Build`.
- Game-side implementations of many `CL.*` APIs are absent from this repository. Do not invent members; corroborate docs with nearby usage.

## File And Address Contract

Mod behavior scripts are text assets, not `.lua` files:

```text
ModProj/Assets/Build/<ModName>/Script/<FileBaseName>.txt
```

Keep these identifiers distinct:

| Use | Shape with current default prefix |
|---|---|
| Addressables entry | `LuaScript/WMD_<FileBaseName>` |
| Usual full `Require(...)` target | `LuaScript/WMD_<FileBaseName>` |
| Serialized prefab `LuaBehaviour.script.luaScript` ID | `WMD_<FileBaseName>` |

Read the actual prefix from `ModProj/Assets/Resources/AddressableConfig.asset`; do not assume `WMD_` after customization. Older examples contain full and bare `Require(...)` strings. Preserve the owning mod's working convention unless the full logical address is verified.

## Class Pattern

```lua
local Example = {
    dontNeedUpdate = true,
}

function Example:Start()
    self.interact = self.host:GetComponent(typeof(CL.InteractBase))
end

function Example:OnDestroy()
end

return Class(nil, nil, Example)
```

Inheritance normally ends with:

```lua
return Class(Require("LuaScript/<Prefix><BaseFileName>"), nil, Example)
```

- `self.host` is the hosting GameObject.
- Object, number, and string injection names serialized by `LuaBehaviour` become `self.<name>` fields. Match names exactly and prefer injections over scene-wide lookup.
- Keep mutable instance state on `self` unless shared module state is intentional.
- Finish each script with the appropriate single `return Class(...)`.

## Runtime Contracts

Common globals in working examples include `UE`, `CL`, `typeof`, `IsNullOrDestroyed`, `CL.ResourceMgr`, `CL.Scheduler`, and `CL.PhysicsHelper`.

Plain `LuaBehaviour` bridges `Awake`, `Start`, `OnEnable`, `OnDisable`, `OnDestroy`, optional `Update`, collision callbacks, and trigger callbacks. Specialized callbacks are owned by bridge components such as `InteractTriggerX` and `FlyObjectX`. Unity supporting a callback does not prove Lua receives it; confirm dispatch in the bridge or generated API page.

Event APIs conventionally use `"+"` to subscribe and `"-"` to unsubscribe. Retain the same function reference. Clean up subscriptions, scheduler handles, spawned resources, and temporary state at the matching lifecycle boundary.

## Editing And Diagnosis

1. Confirm the `.txt` path and expected generated Addressables entry.
2. Inspect the prefab/component script ID and object/number/string injections.
3. Copy callback signatures from the owning bridge or nearest working example.
4. Check every `Require(...)`, handler lifetime, and final `return Class(...)`.
5. Refresh Addressables after creating, deleting, renaming, or moving scripts.
6. Build and install the current output before runtime diagnosis.
7. Inspect `Player.log` for load, syntax, nil-member, and callback-signature failures.

Unity C# compilation does not parse these `.txt` scripts. There is no standalone Lua parser or repository-wide Lua test runner, so do not report Lua execution as validated until it runs in BattleTalent or the supported simulator.