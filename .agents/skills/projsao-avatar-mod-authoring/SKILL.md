---
name: projsao-avatar-mod-authoring
description: Create or modify a Battle Talent AvatarMod for ProjSAO using a Humanoid rig, RuntimeAvatarBuilder, AvatarInfo, hand poses, slots, and optional cloth physics. Use for player-avatar content; not for NPC Role mods or skins.
---

# ProjSAO AvatarMod Authoring

Create a shippable AvatarMod whose rig, prefab name, `AvatarInfo`, and generated Addressables identifier agree. Use the Unity Editor for all imported and serialized assets; never hand-edit `.prefab`, `.asset`, or `.meta` YAML.

This repository's checked Avatar implementation is the contract for ProjSAO-targeted content: `RuntimeAvatarBuilder`, `CharacterBuilderTools`, `AvatarInfo`, and the folder mapping in `AddressableHelper`. No separate `ProjSAO`-named API exists in this checkout, so do not invent one or add game-side dependencies without source evidence.

## Read First

- Start from `ModProj/Assets/Build/SimpleAvatar` for the ordinary avatar layout. Use `SimpleAvatar_Huge` only when scale/audio behavior is relevant.
- Check `ModProj/Assets/Toolkit/Scripts/Avatar/RuntimeAvatarBuilder.cs` for current builder behavior, `ModProj/Assets/Toolkit/HandPoseHelper/support/HandPoseControl.cs` for hand serialization/runtime fields, and `ModProj/Assets/Toolkit/Scripts/ItemInfoConfig.cs` for `AvatarInfo` fields.
- Check `ModProj/Assets/Resources/AddressableConfig.asset` for the active prefix and registered mod roots. `WMD_` in examples and community documentation is an example, not a value to copy blindly.
- When the work includes Addressables, packaging, installation, or runtime logs, also load `battle-talent-build`. When it adds a `LuaBehaviour` for clothing, also load `battle-talent-lua`.

## Identifier And Layout Contract

Use `Tools > Template Wizard` to create an **Avatar** mod. Its generated layout is:

```text
Assets/Build/<ModName>/
  Avatar/<PrefabName>.prefab
  Config/<ModName>_Info.asset
  ICon/<icon>
```

- Keep the generated root registered in `AddressableConfig.addressablePaths`; the wizard normally registers it and refreshes Addressables.
- Keep the avatar prefab under `Avatar/`. The current helper assigns it the address `Avatar/<prefix><PrefabName>`.
- Set `AvatarInfo.avatarName` to exactly `<prefix><PrefabName>`; it is the runtime prefab identifier. Set its display `name` and `desc` deliberately.
- Do not rename the prefab or change its prefix after configuration unless the matching `AvatarInfo` and Addressables entries are refreshed and inspected together.

Before editing, state this local hypothesis and perform its cheapest check: **the model can become a valid Humanoid avatar with all mandatory body and hand bones.** Select the imported model in Unity, configure its Avatar in T-Pose if needed, and verify the Avatar is valid/Humanoid before building content around it.

## Build The Avatar Prefab

1. Import the custom model through Unity. In the model import settings, set **Animation Type** to **Humanoid** and **Avatar Definition** to **Create From This Model**, then Apply. A valid Animator/Avatar and these bones are required: hips, spine, chest, head, both upper/lower arms, both hands, both upper/lower legs, and both feet.
2. Open the generated prefab in `Avatar/`, place the model beneath its root, and assign that model root to `RuntimeAvatarBuilder.fbxPrefab`.
3. Run **Auto Config Builder**. It assigns hand transforms, creates slot transforms, enables off-screen skinned-mesh updates, and opens the character tool. Stop and correct Humanoid mapping if Unity reports missing bones or missing hand-pose controls.
4. If the character needs a non-default scale, set and finalize the model's scale **before** configuring/applying slots. Then position the generated left/right hand slots over the corresponding hands and the back-slot group on the back. Run **Set Slot Tool** only after that scale is final; it temporarily reparents slots and resets their local scale before returning them to the hand/chest bones. Scaling the character after the slots are applied scales the attached slots as well.
5. After **Set Slot Tool**, inspect the slot transforms in the Hierarchy and Scene view: they must be parented to the intended bones, have the expected world size, and still align with the hands/back. Reapply the tool after any deliberate character-scale change, then verify again.
6. If the first-person camera sees the face, put only the face mesh on the `InvisibleFPS` layer. If head and body share a mesh, solve that in the source model rather than hiding the whole renderer.

Save, close, and reopen the prefab before continuing. Verify no missing scripts or references, including inactive children.

## Configure Finger Poses

For each hand, select the generated `HandPoseControl` from the builder:

1. In **both** left and right `HandPoseControl` components, explicitly set **Num Of Finger** (`numOfFinger`) to the model's finger count—normally `5` for Thumb, Index, Middle, Ring, Pinky. Do this even for a normal five-finger Humanoid: the template serializes `0`, which overrides the C# default.
2. Run **Auto Config Fingers**, entering the actual number of GameObjects/bones *within one finger* (usually 3 or 4). This depth parameter is not `Num Of Finger`; the current method fills `fingers` but does not update `numOfFinger`.
3. Confirm that `fingers.Length` equals **Num Of Finger** and that the order is Thumb, Index, Middle, Ring, Pinky. Auto-configuration depends on that order; repair the rig/order instead of compensating with arbitrary finger weights.
4. Use the matching left/right preview slider and **Preview hand pose** to visually check fully closed and fully open states. Save **Fingers Close Pose** and **Fingers Open Pose** respectively on that hand's control. Do not rely on a tutorial caption; judge the mesh state in the preview.
5. Use the view buttons to confirm both saved states, repeat for the other hand, and save the prefab.

## Configure AvatarInfo And Grip Hand Poses

`AvatarInfo.handposes` contains avatar-specific corrections for default game poses (`HoldPose`, `GunPose`, `GunPose2`, `GrabPose`, `DefaultPose`, and `GlovePose`). A pose records left/right positions, rotations, and five finger weights in the order Thumb, Index, Middle, Ring, Pinky.

`HandPoseOffset` must come from the configured FBX T-Pose; do not invent or copy its quaternions. `RuntimeAvatarBuilder.AutoConfigBuilder` opens Character Builder Tools but clears `characterTPose` (and its no-config overload also clears `itemInfoConfig`), so that button alone cannot calculate an offset.

Use this exact workflow before manually entering offsets:

1. Expand the imported FBX in Unity, select its Avatar asset, choose **Configure Avatar**, and make/fix the T-Pose.
2. In the **Hierarchy**, identify the root GameObject of that configured T-Pose character. This root—not merely the FBX asset reference—is the value for **Character T-Pose**.
3. Open `Tools > Character Builder Tools`; assign that Hierarchy root to **Character T-Pose** and this avatar mod's `ItemInfoConfig` to **Item Info Config**. Both fields are mandatory.
4. Run **Calculate Avatar Hand Pose Offset**. The tool reads the default avatar and the target root's valid Humanoid Animator, then writes the root-relative rotation difference for each hand to `AvatarInfo.handposeOffset`: `inverse(defaultHandRootRotation) * targetHandRootRotation`.
5. Confirm the Console reports the intended `AvatarInfo` was updated. With multiple `AvatarInfo` entries, the target root name must match its `avatarName` after prefix/clone-suffix normalization; with one entry, that entry is used.
6. Run **Generate Avatar Handposes From Offset** only after a valid offset is saved. The generator preserves a hand pose with the same name. Review preserved poses explicitly; delete/recreate through Unity only when replacement is intended.
7. Use the built `HandPoseTool` in Battle Talent only to fine-tune an actual weapon grip. Copy the observed values to the corresponding left/right fields of the target `AvatarInfo.handposes` entry. `SetDefaultPoseValueToAvatar` is useful for filling a named pose, but it must use the exact prefixed avatar name and existing pose name.

When a workflow repeatedly has both objects available, an editor-only convenience entry point may accept `ItemInfoConfig` and the Hierarchy T-Pose root together and assign both fields before opening/calculating. It must not guess either input; preserve the normal tool's valid-Humanoid and hand-bone checks.

The ItemInfoConfig **Check** action is still useful, but it does not validate Avatar identifiers. Manually compare `avatarName`, prefab filename, active prefix, and final `Avatar/` Addressables address.

## Optional Clothing Or Hair Physics

Use this path only for a clothing/hair skeleton that needs dynamic motion.

- Add `LuaBehaviour` to the model node and set its script identifier to `<prefix>ClothBoneToolScript`.
- Add an object injection named `root` pointing to the clothing skeleton root. Use the existing styles (`SoltSkirt`, `NormalSkirt`, `HardSkirt`, `LongHair`, `ShortHair`, `Tail`, `Breasts`) only when they match the asset; otherwise inject the documented numeric parameters deliberately.
- For collision, add `ClothBoneColliderHelper` to the relevant body nodes, size each collider in Unity, assign the same `LuaBehaviour`, and invoke the helper's apply action.
- Confirm the script's final address and injections, including inactive children, before packaging. Do not claim cloth behavior works until observed in Battle Talent.

## Validation And Delivery

After the first substantive edit, refresh Unity and wait for compilation/domain reload; the Console must have no new errors. Then:

1. Reopen the prefab and config; inspect the Animator, hand controls, slots, `fbxPrefab`, `AvatarInfo`, and icon reference.
2. Run ItemInfoConfig **Check**, record its limitations above, refresh Addressables, inspect the generated `Avatar/<prefix><PrefabName>` entry, and run `ValidateAddressables`.
3. Build the requested PC VR or Quest target through the established build workflow, then separately verify bundle creation, installation, game discovery, avatar selection, slots, finger motion, grip poses, and optional cloth behavior.

Report those observations separately. Unity compilation, bundle build, file installation, mod discovery, and gameplay verification are distinct outcomes.
