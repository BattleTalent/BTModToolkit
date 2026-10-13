---
name: battle-talent-melee-weapon-authoring
description: Create and validate Battle Talent melee weapon mods for knives, swords, axes, hammers, maces, clubs, and similar held weapons. Use when importing a melee model, configuring its prefab, damage, collision, grip, mount point, icon, ItemInfoConfig, or deciding whether the weapon can stab; do not use for guns, bows, spells, or standalone projectiles.
---

# Battle Talent Melee Weapon Authoring

Create the weapon in `ModProj` with the Unity Editor. Use the same shared workflow for edged and blunt weapons; the category-specific branch in this skill is whether the finished prefab has a configured `StabObject`.

Also follow `../battle-talent-mod-authoring/SKILL.md` for general Unity asset rules. Load `../battle-talent-build/SKILL.md` when refreshing Addressables, building, installing, or reading runtime logs.

## Authority And Safety

- Use Unity 2020.3.48f1 and the current repository implementation. Treat `ModProj/Assets/Toolkit/TemplateWizard`, the closest example under `ModProj/Assets/Build`, and a live Unity Inspector as authoritative.
- Use the community tutorial as workflow background: <https://battletalent.github.io/community-docs/docs/support-mod-types/Weapon/Tutorials/Basic%20-%20Create%20a%20weapon%20mod>.
- **Never run `Tools > Destructive > Cleanup All Content`.** The tutorial's “Remove unused content” step is obsolete and wrong for this repository. Do not delete demo content as preparation.
- Create and edit prefabs, ScriptableObjects, materials, component references, and Addressables through Unity. Never hand-edit `.prefab`, `.asset`, `.meta`, or Addressables YAML.
- Do not claim import, compilation, build, installation, mod discovery, or gameplay success unless it was observed.

## Resolve The Requested Weapon

Before authoring, determine or reasonably infer:

- mod folder/prefab name, display name, description, and unique prefix;
- source model and textures/materials;
- one-handed or two-handed grip;
- target platform: Windows PC VR, Meta Quest, or both;
- whether the weapon is intended to penetrate targets.

Classify the prefab branch by behavior, not by the model's label:

| Intended behavior | Required `StabObject` state |
|---|---|
| Knife, dagger, sword, spear, or another weapon with a usable penetrating point | Present and configured; `StabArea` covers only the penetrating blade/tip |
| Hammer, mace, club, staff, or another non-penetrating blunt weapon | No active/configured `StabObject`; remove an inherited one in Unity if the chosen template contains it |

An axe or other hybrid weapon may use the penetrating branch only when its design truly includes a stabbing point. Do not add `StabObject` merely because the weapon is sharp.

## Batch-First Execution

Prefer Unity batchmode plus editor C# APIs over mouse-driven Unity UI for melee weapon creation. Batch creation must still let Unity own every imported asset and serialized write; it is not permission to create or rewrite `.prefab`, `.asset`, `.meta`, or Addressables YAML outside Unity.

- First inspect reusable weapon creator entry points under `ModProj/Assets/Editor`. `FireHammerWeaponCreator.cs` is the closest current blunt/two-handed example, and `ZangetsuWeaponCreator.cs` is a blade example. Reuse their verified patterns selectively; check every copied behavior against this skill because an older creator may retain legacy naming or component choices.
- When no existing creator covers the requested weapon, add a narrowly scoped editor-only entry point such as `CrossLink.<ModName>WeaponCreator.Create`, plus a focused validation method such as `ValidateExisting`. Make it parameterized only when repeated creation is actually expected.
- Prefer an explicit public static entry point over relying on `[InitializeOnLoadMethod]` side effects. A `MenuItem` may be provided as a graphical fallback, but batch creation remains the primary path.
- Invoke the entry point with the installed Unity 2020.3.48f1 editor:

  ```text
  <UnityEditor> -batchmode -nographics -quit -projectPath <ModProj> -executeMethod CrossLink.<ModName>WeaponCreator.Create -logFile <log>
  ```

- Do not start a batch writer while another Unity process owns `ModProj`. Use the active Unity instance safely or ask the user to close it before batch execution.
- The creator must refuse to overwrite an existing `Assets/Build/<ModName>` unless the user explicitly requested an update and the existing assets and update path were inspected.
- The creator should automate deterministic work through `AssetDatabase`, `PrefabUtility`, importer APIs, `SerializedObject`, and repository APIs: Template Wizard generation, model/material setup, component creation, prefab references, ItemInfoConfig, icon generation, Addressables refresh, saving, reloading, and focused validation.
- The creator must enforce the invariants in this skill, including the `StabObject` branch, root `RagdollHitInfoRef` plus refreshed `refs` when multiple hit objects exist, and linked `Assets/Toolkit/Prefabs/AttachLine.prefab` instances named `AttachLine`.
- Write a result file or an unambiguous log summary that separately reports created assets, validation counts, Addressables state, and `BUILD`, `INSTALL`, and `GAMEPLAY` status. Read both the Unity process result and log before claiming success.
- Use the graphical Editor for visual judgment after the batch pass: confirm model axis/scale, collider coverage, stab geometry, grip and mount placement, material appearance, and icon framing. Make any correction through Unity, then rerun focused batch validation when practical.

## Choose A Starting Point

From the batch creator, obtain `TemplateWizardAsset.GetWizard()`, select `ModType.Weapon`, assign the requested name and `StoreItemInfo`, choose `Sword` or `Axe` as the closest starting prefab, and call `GenerateTemplate()`. Use `Tools > Template Wizard` only as the UI fallback for this same operation. The name must be unique, contain no spaces, and start with an uppercase letter. The template subtype is only a starting layout and does not replace the `StabObject` decision above.

The wizard should create:

```text
Assets/Build/<ModName>/
|-- Config/<ModName>.asset
|-- ICon/<generated icon name>.png
`-- Weapon/<ModName>.prefab
```

It also registers the mod root in `AddressableConfig.addressablePaths` and refreshes Addressables. Confirm those results instead of recreating them manually.

For comparison, inspect only the closest working example:

- minimal melee structure: `ModProj/Assets/Build/Stick`;
- hammer or mace behavior: `ModProj/Assets/Build/LedoHammer` or `Mace_Strong`;
- blade behavior: `ModProj/Assets/Build/Dagger_Bleed`, `Rapier_Light`, or a nearby `Sword_*` example.

## Configure The Prefab In Unity

1. Open `Weapon/<ModName>.prefab` in Prefab Mode. Put the imported model under the prefab root, preserve the root's toolkit components, and align the weapon so local `+Z` is forward. Scale it against the template example, then hide the example mesh.
2. Assign every visible `MeshRenderer` or `SkinnedMeshRenderer` required by the template's color-change field so damage/color feedback can reach the rendered weapon.
3. Add collision geometry to the damaging head/blade and handle as needed. For a Rigidbody weapon, use convex `MeshCollider` shapes or primitive compound colliders and set the collision objects to the `Interact` layer. Prefer multiple simple convex colliders when one convex hull is inaccurate.
4. Configure `RagdollHitInfoObj` and `CollisionEffect` for each meaningful hit region. The damaging head/blade and handle may use separate objects so they do not deal the same hit. Copy field choices from the nearest comparable example; do not invent undocumented hit template names.
   - When the weapon has more than one `RagdollHitInfoObj`, add one `RagdollHitInfoRef` to the weapon root. It groups the hit objects so the same weapon does not register them as independent repeated hits.
   - Invoke `Reset` on `RagdollHitInfoRef` after all hit objects are in place so it automatically populates `refs`. The current Inspector's `RefreshRefs` button calls the same `Reset()` method and is the preferred visible action when available.
   - Invoke `Reset`/`RefreshRefs` again whenever a `RagdollHitInfoObj` is added, removed, or reparented. Verify that `refs` contains every intended hit object exactly once; the underlying `GetComponentsInChildren<RagdollHitInfoObj>()` scan does not include inactive descendants by default.
5. Position the existing `MountAttach` where the item should sit in a belt or backpack slot.
6. Use the existing `Assets/Toolkit/Prefabs/AttachLine.prefab` for the primary grip. If the weapon template already contains an instance of that prefab, reuse it; otherwise drag that prefab under the weapon root. Keep the instance linked to the source prefab and keep its GameObject name exactly `AttachLine`. Do not create a replacement GameObject, add the component manually, or rename it. Adjust its start/end transforms to cover the usable grip and reference it from `InteractWeapon`.
7. For a two-handed weapon, add a second instance of the same `Assets/Toolkit/Prefabs/AttachLine.prefab` and keep its name `AttachLine` as well. Set its `DependAttachObj` to the primary grip, choose the appropriate `Index Limit`, register both instances on `InteractWeapon`, and enable `Allow Second Hand`. Do not enable this for a strictly one-handed design.
8. Apply the `StabObject` branch below, tune handling using the closest example, save the prefab, exit Prefab Mode, reopen it, and check for missing scripts or references including inactive children.

### Penetrating Blade Branch

- Add or keep `StabObject` on the weapon root.
- Configure its `StabArea`/geometry to surround the blade or point that may enter a target, not the handle, guard, or blunt faces.
- Align the stab direction with the model's penetrating tip and local forward direction. Use the Inspector gizmo and verify the volume after reopening the prefab.
- Confirm in Battle Talent that the point can enter and leave a target without the handle becoming a stab surface.

#### Straight Or Slightly Curved Blades

- Prefer **one continuous rectangular side-cut Geo per cutting edge**, not one Geo per blade segment, when the blade is straight or only slightly curved. Keep a separate tip-directed Geo for a penetrating point: a single-edged katana normally uses two entries, one for the tip and one for the whole cutting edge. A box enclosing the blade does not configure every attack direction; `geoForward` determines which direction it accepts. Keep compound colliders when useful; collider count and Geos count are independent.
- Fit a rectangular volume using `geoFwdDis`, `geoWidth`, `thickness`, and `geoPoint`. `geoPoint` is the center of the **rear face**, not the box center: with normalized directions, the box center is `geoPoint + geoForward * geoFwdDis / 2`. Length runs forward from that rear face; width extends by `geoWidth / 2` on each side along `Cross(geoThick, geoForward)`; thickness extends by `thickness / 2` along `geoThick`. Keep the forward and thickness directions normalized and perpendicular.
- For the tip Geo, aim `geoForward` toward the actual tip and use `sideGeo = -1`. For the cutting-edge Geo, aim `geoForward` outward through the sharpened edge, not toward the tip or spine; set `sliceDir` along the blade and use `sideGeo = 0`, as in the nearby examples. The current toolkit treats a nonnegative `sideGeo` as side-cut geometry; do not assume it is a reference to another Geo without verifying the runtime implementation. Keep `isBlade = true`. For the side-cut box, `geoFwdDis` spans across the blade and `geoWidth` spans along it. Choose the edge sign from the actual mesh, not a universal `+X` or `-X` assumption.
- Project the blade vertices into each Geo's frame to determine its bounds, then place `geoPoint` and add only a small clearance. Both directions need blade-only coverage, excluding guard and grip. A tip box may remain localized to the point; the single side-cut box should cover the whole usable edge. Do not copy dimensions or offsets from another model, or add extra blade segments merely for a slight curve.
- Set each Geo's `cols` to the intended blade colliders only, excluding guard and handle colliders. If a request for "one Geo" could mean one full-edge region rather than one entry total, preserve both thrust and cutting behavior; clarify the tradeoff before intentionally removing either direction.
- After saving and reopening, inspect the rectangular Gizmos and their direction arrows, not just containment. Verify tip and edge directions separately, tip/side-cut classification and `sliceDir`, intended blade coverage, guard/handle exclusion, and blade-only collider references. Use the same fitting helper in creator/update paths and validate both roles, so a later repair neither restores unnecessary segmentation nor drops the edge-directed Geo.
- The local `KatanaSlashWeaponCreator.cs` demonstrates one tip Geo plus one fitted full-edge Geo. When ProjSAO is available, compare `Assets/Resources/Weapon/Katana_Slash.prefab` and `Ninjato_Black.prefab` for separate tip/side-cut roles and rectangular-area placement; adapt their geometry to the current model instead of copying numerical values blindly.

### Blunt Branch

- Ensure the finished prefab has no active/configured `StabObject` and no `StabArea` geometry.
- If the selected template inherited `StabObject`, remove the component in Prefab Mode; do not leave an empty or accidental stab volume.
- Validate impact behavior through `RagdollHitInfoObj`, `CollisionEffect`, colliders, mass, and handling. Absence of stabbing does not replace those shared configurations.

## Configure Mod Metadata And Icon

- Give the Unity Player `Product Name` a unique mod-package name because build and install folders use `Application.productName`.
- Set a collision-resistant prefix in `Assets/Resources/AddressableConfig.asset`. Keep the prefix consistent with generated identifiers.
- In `Config/<ModName>.asset`, confirm the `StoreItemInfo` entry:
  - `addStoreItemName` resolves to `<Prefix><PrefabName>`;
  - `itemType` is `Weapon`;
  - player-facing `name` and `desc` are filled;
  - `supportedMultiplayer` is enabled only after multiplayer behavior is verified.
- Run the ItemInfoConfig Inspector `Check` action and resolve all errors.
- Use `Tools > Icon Generator`, render the weapon prefab, overwrite the wizard-created icon without renaming it, set texture Max Size to `256`, and apply the import settings.

## Addressables, Build, And Test

When delivery is requested, follow `../battle-talent-build/SKILL.md` and use the current commands rather than the tutorial's old menu names:

1. Confirm `Assets/Build/<ModName>` is present in `AddressableConfig.addressablePaths`.
2. Run `Tools > Create And Refresh Addressables`; do not use `ClearAddressables` as routine maintenance.
3. Inspect the generated Weapon, Config, and ICon entries, then run `ValidateAddressables`.
4. Require a clean Unity compilation and Console before building.
5. Prefer `BuildTools > FastBuildAndInstallForWindows` for PC VR or `BuildTools > FastBuildAndInstallForAndroid` for Quest. Use `BuildAllBundles` only when both outputs are actually required and verify each output separately.
6. In Battle Talent, verify mod discovery, spawn/store metadata, grip pose, mount orientation, collision/damage, and the category-specific stab behavior. On Quest, also verify the headset connection and ADB installation output.

## Completion Report

Report:

- the created or changed mod assets and whether the prefab is penetrating or blunt;
- the batch creator entry point, Unity batch command/log/result, and any graphical follow-up that was required;
- `StabObject` state and how its area was verified;
- when multiple hit objects exist, the root `RagdollHitInfoRef` state and the verified contents of its `refs` array;
- confirmation that every grip uses the linked `Assets/Toolkit/Prefabs/AttachLine.prefab` instance and retains the name `AttachLine`;
- Unity import/compile and prefab-reference results;
- ItemInfoConfig and Addressables validation results;
- build target/output, installation, mod discovery, and gameplay as separate statuses;
- any remaining check that requires Battle Talent, PC VR, or a physical Quest headset.
