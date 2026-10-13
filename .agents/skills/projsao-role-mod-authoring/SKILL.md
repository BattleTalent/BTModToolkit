---
name: projsao-role-mod-authoring
description: Create or modify Battle Talent NPC Role mods for ProjSAO, including Humanoid role prefabs, RoleModInfo, weapon slots, armor, custom AI actions, dismemberment, and body grab points. Use for NPC RoleMod content; not for player AvatarMod or character Skin mods.
---

# ProjSAO RoleMod Authoring

Create the RoleMod in `ModProj` and keep the prefab name, active prefix, `RoleModInfo.roleName`, and generated Role address consistent. Use the Unity Editor for every imported or serialized asset; never hand-edit `.prefab`, `.asset`, `.controller`, `.unity`, or `.meta` YAML.

## Authority And Scope

- Use the current repository implementation as the mod contract: `ModProj/Assets/Toolkit/TemplateWizard`, `ModProj/Assets/Toolkit/Scripts/ItemInfoConfig.cs`, `ModProj/Assets/Toolkit/Scripts/Addressable/AddressableHelper.cs`, and the closest example under `ModProj/Assets/Build`.
- The source tutorials, when available, are under `D:/Community-docs/community-docs/docs/support-mod-types/4.Role/Tutorials`. Read only the tutorial for the feature being built. Current Inspector labels and repository tools override older screenshots or menu names.
- Use `ModProj/Assets/Build/SimpleRole` for a basic Role, `SimpleRole_Action` for actions/armor/dismemberment/grab points, and `SmallGoblinLancer` for a current custom-action example.
- A local ProjSAO checkout may provide useful runtime evidence. The checkout at `D:/ProjSAO/ProjSAO` currently has an older `ItemInfoConfig` without `RoleModInfo`; do not copy that obsolete schema. Its `CreatureGenerator` and `RagdollReactAnimation` still confirm runtime invariants such as the exact weapon-point names, armor fields, action arrays, `disableMotionFeature`, and `dizzyGrabPoses`.
- If the work adds or changes XLua behavior, also load `battle-talent-lua`. If it refreshes Addressables, builds, installs, or diagnoses runtime output, also load `battle-talent-build`.

## Batch-First Execution

Prefer Unity batchmode plus editor C# APIs over mouse-driven Unity UI for RoleMod creation. Batch execution must still let Unity own every import and serialized write; it is not permission to create or rewrite `.prefab`, `.asset`, `.controller`, `.unity`, or `.meta` files outside Unity.

- First look for a reusable RoleMod editor entry point under `ModProj/Assets/Editor`. Reuse it when its inputs and validation cover the requested Role. Otherwise add a narrowly scoped editor-only entry point such as `CrossLink.<ModName>RoleModCreator.Create`; make it parameterized when repeated use is clearly expected. Report every added creator source file and do not delete it without the required confirmation.
- Invoke the entry point with the project's Unity 2020.3.48f1 editor using `-batchmode -nographics -quit -projectPath <ModProj> -executeMethod <Namespace.Type.Method> -logFile <log>`. Read the log and process result before claiming creation or validation succeeded.
- The entry point must use `ModelImporter`, `AssetDatabase`, `PrefabUtility`, `SerializedObject` or the repository's public editor APIs. It must refuse to overwrite an existing `Assets/Build/<ModName>` unless the user explicitly requested an update and the update path has been inspected.
- Create the scaffold by calling the actual Template Wizard implementation: get `TemplateWizardAsset`, select `ModType.Role`, assign the name and `RoleModInfo`, and call `GenerateTemplate()`. Do not imitate the wizard with shell-created folders, copied YAML, or a parallel scaffolder.
- Automate deterministic work: Humanoid importer settings and bone validation, Wizard generation, model insertion, child/root Animator setup, `RoleSlots.GenerateSlots()`, basic `RoleModInfo`, the default `ItemInfoConfig.ReplaceAllCharacters()` call, Unity saves/reloads, Addressables refresh, and identifier validation.
- Do not bring Avatar-only work into this pipeline. RoleMod creation does not configure `RuntimeAvatarBuilder`, `HandPoseControl.numOfFinger`, `AvatarInfo.handposeOffset`, or avatar default grip poses.
- If another Unity process owns the project, do not start a second writer. Resolve the active instance safely or ask the user to close it before running batchmode.
- Use the graphical editor only for a step whose correctness requires visual judgment or when batch validation identifies a concrete problem: repairing Humanoid mapping/T-Pose, adjusting weapon slots, inspecting materials or scale, fitting armor/colliders, previewing actions, tuning dismemberment, or positioning body-grab lines. After a UI correction, resume batch reload and validation when practical.

## Establish The Baseline

Before editing, state and test this local hypothesis: **the imported model can be configured as a valid Humanoid and the generated Role root can drive it without losing the template's `Animator` and `RoleSlots`.**

Perform the cheap check in the batch entry point first: set the model's `ModelImporter.animationType` to `Human`, set `avatarSetup` to `CreateFromThisModel`, reimport it, instantiate it through Unity, and verify a valid Human Avatar with hips, spine/chest, head, both arms/hands, and both legs/feet. Stop before scaffolding if this fails. Open **Configure Avatar** only when Unity cannot produce the required mapping automatically; weapon slots, custom actions, grab points, and Humanoid bone helpers all depend on it.

## Scaffold And Configure The Role Prefab

1. From the batch entry point, obtain `TemplateWizardAsset.GetWizard()`, set `selectedModType` to `TemplateWizardAsset.ModType.Role`, assign `newModFolderName` and the intended `roleModInfo`, then call `GenerateTemplate()` to create `Assets/Build/<ModName>`. The name must be nonempty, unique under `Assets/Build`, contain no spaces, and begin with an uppercase character. Use `Tools > Template Wizard` only as the UI fallback for this same operation.
2. Keep the generated structure and add optional folders only when needed:

   ```text
   Assets/Build/<ModName>/
     Audio/
     Config/<ModName>.asset
     ICon/<ModName>.png
     Role/<ModName>.prefab
     Anim/                 # optional custom controller
     Action/               # optional ActionData
     ArmorProfile/         # optional armor definition
     BrokenArmor/          # optional detached armor prefabs
   ```

   The wizard registers the mod root in `AddressableConfig.addressablePaths` and refreshes Addressables. Preserve that registration.
3. Load `Role/<ModName>.prefab` with `PrefabUtility.LoadPrefabContents`, instantiate the imported model beneath the generated root, and keep the root's `Animator` and `RoleSlots` components.
4. Disable the `Animator` on the inserted model object. Assign the model's Humanoid Avatar to the generated root `Animator`; the root Animator is the runtime driver.
5. Call `RoleSlots.GenerateSlots()` and validate all four generated transforms. Use the graphical editor to adjust them when their placement or grip axes cannot be established mechanically:

   - `LWeapon Point` and `RWeapon Point` must align with the corresponding hands and weapon grip axes.
   - `LWeapon Spine` and `RWeapon Spine` must sit on the intended back/shoulder storage positions.

   ProjSAO's `CreatureGenerator` looks up these names exactly. Do not rename them.
6. Save with `PrefabUtility.SaveAsPrefabAsset`, unload the prefab contents, reload it from `AssetDatabase`, and verify the root Avatar, disabled child Animator, exact slot references, and missing-script state. Inspect materials, scale, and final slot placement visually when needed.

## Configure `RoleModInfo`

The template creates one `RoleModInfo` in `Config/<ModName>.asset`. Configure it through the batch creator and mark/save the config through Unity; use the Inspector only for review or values requiring user judgment:

- Set `roleName` to exactly `<activePrefix><PrefabName>`. The generated prefab address must become `Role/<activePrefix><PrefabName>`.
- Set `name` and `desc` for player-facing text.
- After the generated `ItemInfoConfig` has been loaded, call `ItemInfoConfig.ReplaceAllCharacters()` by default. It replaces `replaceRole` on every contained `RoleModInfo` with `ReplaceableCharacterConfig.characters` followed by `memeCharacters`; use a narrower explicit list only when the user requests restricted replacement behavior. This method does not assign `defaultReplacement`, so set that field deliberately and require its value to be present in `replaceRole`.
- `weapon` supplies both hands when `leftWeapon` and `rightWeapon` are empty. A nonempty side-specific field overrides that side. For a mod weapon, use its prefixed `StoreItemInfo.addStoreItemName`, not a prefab filename or `Weapon/` address.
- `attr` reads only its first element. Leave the array empty to use game defaults; otherwise set `hpMax`, `mpMax`, `atkMlp`, and `massMlp` intentionally.
- Set `disableMotionFeature` only for boss-like roles that should resist motion/knockback and use the boss presentation. ProjSAO applies it to the character motion controller.
- Configure only required `SoundEffectInfo` entries: bone break, hurt, death, ground, warning, attack, and taunt. Audio files under `Audio/` receive `Audio/Sound/<prefix>...` addresses after refresh.
- Keep `loadOnSpawn = true` when deferred role loading is desired; changing it affects when the config is loaded, not the Role address.

Replace the placeholder icon through Unity's asset pipeline. Keep its `.meta` when replacing the file in place.

## Optional Armor

Use this section only when the Role ships wearable or breakable armor.

1. Place each armor object under its actual target bone, fit it visually, set its layer to `Ragdoll`, add appropriate colliders, name it with the active prefix, disable it in the prefab, and save.
2. Create an `ArmorProfile` under the mod's `ArmorProfile/` folder. For each entry:

   - `tag` identifies the slot such as `Helm`, `Torso`, or a limb.
   - `resBonePath` is the exact prefixed armor object/resource name.
   - `bonePath` is the target bone name used by the character hierarchy.
   - `hpPercent` controls durability relative to character health; set `fullCover`, `beHitEffect`, and `stiffSec` only when the design needs them.

3. For breakable armor, create `BrokenArmor/<UnprefixedArmorName>.prefab`. The folder mapping supplies the prefix and the final address `Character/BrokenArmor/<prefix><UnprefixedArmorName>`. Add the appropriate Collider, Rigidbody, `CollisionEffect`, and `SceneObj`; add `AutoDestroy` with a short delay such as 3 seconds when the debris should expire. Match the shipped `SimpleRole` pattern rather than inventing component defaults.
4. In `RoleModInfo`, set `armorProfile` to the prefixed profile identifier and add fixed tags to `armorConfigs` or randomized tags to `armorConfigsRandom`. Do not configure armor tags without a matching profile entry.

## Optional Custom AI Actions

Use this section only when the Role needs behavior beyond the replaced character's normal controller/actions.

1. Put custom clips in a Resources-relative folder such as `Assets/Resources/<ModName>/Anim`. Copy `ModProj/Assets/Toolkit/CharacterAnimator/Goblin.controller` into the mod's `Anim/` folder and rename the copy; do not edit the shared controller.
2. Create `ActionData` assets under the mod's `Action/` folder. Configure timelines using the Inspector tooltips. For combined actions, set `combineList` and run **OutputCombine**, then inspect the generated timelines rather than editing serialized data.
3. Create an `AIPreset` in `Config/`:

   - Assign the copied controller.
   - Fill `aiActions`, plus `dodgeActions`, `wakeupActions`, and `cooldownSlots` only as needed.
   - Set `originalController` to `Goblin` when the controller was copied from `Goblin.controller`.
   - Run **AutoAddState** with a path relative to `Resources`, not an `Assets/...` path. Every `ActionAnimData.animName` must resolve to a clip after removing the active prefix.
   - Export root motion for clips referenced through `ActionAnimData.rootMotion`. When previewing those actions, assign the same preset to `ActionEditor.rootMotionPreset`; otherwise the preview intentionally warns that no layout source is available.

4. Test actions on a scene instance with `ActionEditor` in Play Mode. Treat this as an editor preview, not proof of ProjSAO runtime behavior.
5. Assign the `AIPreset` to `RoleModInfo.aiPreset`. Confirm `Assets/Build/Common/Script/AIPresetLoaderScript.txt` is included through a registered Common root and will receive the address `LuaScript/<prefix>AIPresetLoaderScript`.
6. Run the config's **AIPresetSetup** action with the exact Role prefab. It adds or updates a root `LuaBehaviour`, assigns the prefixed loader script, injects the `AIPreset` as `aiPreset`, injects `roleName` as `name`, and saves through Unity. Reopen the prefab and verify all three values. The helper matches `<roleName minus prefix>` to the prefab filename and uses the first match when duplicates exist, so keep that relation one-to-one.

Do not hand-create game-side `CL.*` APIs or rewrite the loader script unless the requested behavior actually requires Lua.

## Optional Dismemberment

1. Add `RagdollDismembermentVisual` to the Role root. If its Inspector reports non-readable meshes, use **Auto-fix all meshes** and allow Unity to reimport them.
2. Add the first fragment as `ROOT`, assign the skeleton root bone, and include every dismemberable `SkinnedMeshRenderer` in its `SkinnedMeshes` list. Use `ignoreEdgeCreation` for renderers that should not produce cut caps and `ignoreDismember` for renderers that must never be cut.
3. Add only the needed body fragments. Bind each to the correct Humanoid bone, then use the current helpers—**Automatic plane by bone**, **Automatic CutRadius**, **Automatic bounds**, and **Automatic size**—as starting points. Inspect the Scene gizmos and tune the cut plane, radius, bounds, and binormal manually.
4. Preview each fragment with `Tools > Mod Toolkit > Dismember Preview`. A good preview is required before saving, but still does not prove runtime physics or effects.
5. Save and reopen the prefab. Verify the `ROOT` renderer list and every fragment bone, especially after reimporting or replacing the model.

## Optional Body Grab Points

ProjSAO consumes the serialized `RagdollReactAnimation.dizzyGrabPoses` array and enables/disables those `InteractBase` objects with character state.

- For a standard valid Humanoid, add `RagdollReactAnimation` to the Role root and prefer **ConfigureMemeNpcGripPoint** when its fixed set of head, chest, upper/lower arms, and upper/lower legs matches the design. Despite its legacy name, the current helper validates the Humanoid Avatar, creates or reuses `InteractBase`, `Rigidbody`, and `AttachLine`, runs `SetAsBodyPart`, assigns `selfRB`/`interact`, and refreshes `dizzyGrabPoses`.
- Inspect every generated line in the Scene view. Adjust line direction, `lineStartPoint`, `lineEndPoint`, and optional `lineOffset`; remove unwanted points through Unity.
- For a custom subset, configure manually on each target bone: add `InteractBase`, run **SetAsBodyPart**, add a Rigidbody, place `ModProj/Assets/Toolkit/Prefabs/AttachLine.prefab` beneath the bone, assign its `interact` and `selfRB`, then run **FindAllInteractBaseAsGrabPoses** on the root.
- `FindAllInteractBaseAsGrabPoses` collects all active and inactive child `InteractBase` components. Check the resulting array so unrelated weapon or armor interactables are not registered as body grabs.

## Validation And Delivery

After the first substantive asset change, let the batch editor finish import and domain reload. Inspect its log and require no new compilation or Console errors before using the changed types.

1. Reload the Role prefab and config through `AssetDatabase`. Check the valid root Animator/Avatar, disabled child Animator, exact four slot names and references, missing scripts, inactive children, `RoleModInfo`, and all optional systems used. Unless the user requested a restricted list, verify each `replaceRole` matches the current `ReplaceableCharacterConfig.characters` plus `memeCharacters`, and verify `defaultReplacement` belongs to that result. Open assets in the editor only for required visual inspection.
2. Invoke `ItemInfoConfig.Check` through Unity when its checks are applicable. It checks the active prefix, display name, and known replacement identifiers, then opens `Character Builder Tools` for the first matching Role prefab. Use the graphical tool only for **Check Material Textures** and **Check Mesh Complexity**. Its camera-occlusion and hand-pose tools are for Avatar mods, not Role mods.
3. Call `AddressableHelper.CreateAndRefreshAddressables()` from the batch entry point. Inspect through the Addressables settings API, or use the UI when diagnosis requires it, for at least:

   - `Role/<prefix><PrefabName>`
   - `Config/<prefix><ConfigName>`
   - optional `Action/<prefix>...`
   - optional `Character/ArmorProfile/<prefix>...`
   - optional `Character/BrokenArmor/<prefix>...`
   - optional `LuaScript/<prefix>AIPresetLoaderScript`

4. Call `AddressableHelper.ValidateAddressables()`. Account for validator gaps: explicitly compare `roleName` with the prefab address, inspect `leftWeapon` and `rightWeapon`, and inspect Lua components on inactive children.
5. Build only the requested Windows or Android target using the current repository build workflow. For ProjSAO delivery, inspect `ModProj/Assets/Editor/AddressabesBuilder.cs` and active `ProjectSettings` instead of hardcoding Battle Talent package or product names; output/install directories use `Application.productName`, and this branch's Android builder targets `com.CrossLink.SAO`.
6. Report separately: Unity import/Console state, Addressables validation, bundle creation, file installation, ProjSAO mod discovery, NPC replacement/spawn, animation/actions, weapons/slots, armor, dismemberment, grab points, and gameplay. Never infer later stages from an earlier success.

Checks that require ProjSAO or a headset remain unverified until directly observed.
