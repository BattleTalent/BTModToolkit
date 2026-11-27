---
name: projsao-skin-mod-authoring
description: Create or modify a legacy Battle Talent player SkinMod for ProjSAO using the stock player skeleton, SkinInfo, mesh-root replacement, first-person head hiding, and Skin Addressables. Use for character skins; not for AvatarMod or NPC RoleMod content.
---

# ProjSAO SkinMod Authoring

Create a legacy player SkinMod whose prefab layout, mesh container, bone names, `SkinInfo`, icon, and generated addresses agree. Use Unity for every imported or serialized asset; never hand-edit `.prefab`, `.asset`, `.meta`, or Addressables YAML.

SkinMod is the old player-appearance system. It replaces the player's rendered meshes and reuses the game's player skeleton. It is useful when compatibility with the demo or an existing Skin workflow is required. Prefer AvatarMod when the user needs different proportions, avatar slots, avatar hand poses, or `RuntimeAvatarBuilder` behavior.

## Authority And Version Boundary

- Start from `ModProj/Assets/Toolkit/Prefabs/Skin/Skin_Player.fbx`, `Warrior_Rig.fbx`, and `Warrior_Rig_TPose Variant.prefab`; use `ModProj/Assets/Build/SimpleCharacterSkin` as the identifier/layout example.
- The source tutorial is `D:/Community-docs/community-docs/docs/support-mod-types/5.Skin/Tutorials/6.create-a-skin-mod.mdx`. Its visual steps are useful, but current toolkit code and Inspector fields take precedence.
- Check `ModProj/Assets/Toolkit/Scripts/ItemInfoConfig.cs`, `ModProj/Assets/Toolkit/TemplateWizard/Scripts/TemplateWizardAsset.cs`, and `ModProj/Assets/Toolkit/Scripts/Addressable/AddressableHelper.cs` for the current contract.
- The local checkout at `D:/ProjSAO/ProjSAO` is older: its `SkinInfo.characterName` and hard-coded `Warrior_Boy` lookup do not match the current toolkit's `SkinInfo.meshRoot`. Use its `ReSkinHelper` only as evidence that runtime replacement matches bones by exact transform name and rebinds `SkinnedMeshRenderer` bones to the player's `root`; do not copy the obsolete schema.
- If the task reaches Addressables refresh, bundle build, installation, or runtime logs, also load `battle-talent-build`.

## Prove The Model Is Suitable First

Before building content, state and check this hypothesis: **the supplied mesh can be skinned to the stock Battle Talent player rig without changing the required bone names or hierarchy**.

Use the stock Skin/Warrior rig as the comparison in Unity and the DCC source. Confirm that:

- every deform bone referenced by every `SkinnedMeshRenderer` has an exact, case-sensitive counterpart in the stock player skeleton;
- the skin has a skeleton root named `root` and a separate mesh-container transform;
- bind pose, weights, scale, and orientation fit the stock skeleton;
- the mesh includes a separable head/face renderer, or can be changed in the source model so first-person hiding does not hide the entire body.

Humanoid import validity alone does not prove SkinMod compatibility. If the model is not already bound to the compatible rig and no editable DCC source is available, stop and request a properly rigged source instead of inventing Unity-side bone mappings. Do not rename stock bones to accommodate an incompatible model.

## Scaffold And Identifier Contract

Create the mod through `Tools > Template Wizard` with type **Skin**. The name must be unique under `Assets/Build`, contain no spaces, and start with an uppercase letter. The wizard creates:

```text
Assets/Build/<ModName>/
  Config/<ModName>.asset
  ICon/<ModName>.png
  Skin/<ModName>.prefab
```

It also registers the mod root in `AddressableConfig.addressablePaths` and refreshes Addressables. Do not recreate those serialized assets or registrations with shell/file text edits.

Read the active prefix from `ModProj/Assets/Resources/AddressableConfig.asset`; `WMD_` in old examples is not a constant. Keep these values aligned:

| Element | Required value |
|---|---|
| Skin prefab or model filename | `<ModName>` |
| Generated skin address | `Skin/<prefix><ModName>` |
| `SkinInfo.skinName` | `<prefix><ModName>` without `Skin/` |
| Icon filename/address | `<ModName>.png` / `ICon/<prefix><ModName>` |
| `SkinInfo.meshRoot` | Exact, case-sensitive name of the prefab's mesh-container transform |

Set `SkinInfo.name` and `desc` deliberately. Keep the default `loadOnSpawn = true` unless the user has a specific reason to preload the mod. When updating a generated mod, do not rename the asset, prefix, or mesh root independently; refresh and inspect the config and addresses together.

## Build The Skin Asset

1. Bind the custom meshes to the stock Skin/Warrior skeleton in the source model. Preserve the stock bone names. Import through Unity and resolve import warnings before constructing the prefab.
2. Open the wizard-created `Skin/<ModName>.prefab`. Put the compatible skeleton root named `root` and the mesh container as siblings beneath the prefab root. Place all replacement `SkinnedMeshRenderer` objects beneath the mesh container named by `SkinInfo.meshRoot`.
3. For every `SkinnedMeshRenderer`, verify a non-null mesh, root bone, complete bones array, materials, and sensible bounds. Bone references must resolve to transforms below this prefab's `root`; duplicate bone names are unsafe because the runtime chooses by name.
4. Do not add `RuntimeAvatarBuilder`, avatar slots, avatar hand-pose configuration, a second gameplay Animator, colliders, or Role components merely because another character workflow uses them. SkinMod borrows the live player's skeleton and behavior.
5. Save, close, and reopen the prefab. Inspect inactive children too; there must be no missing scripts, references, or renderers outside the declared mesh root unless intentionally excluded.

If the imported FBX itself already has the correct top-level layout and filename, an existing mod may use it as the addressable Skin asset as `SimpleCharacterSkin` does. For a newly scaffolded mod, prefer editing the wizard-created prefab; do not remove that source asset merely to switch formats without the user's explicit confirmation. There must be only one intended asset with the `<ModName>` stem in that mod's `Skin` folder, or address generation can create an ambiguous contract.

## Preserve Shadows Without Blocking First Person

For each head/face renderer that could enter the VR camera:

1. Keep the original renderer on the normal layer and set **Cast Shadows** to **Shadows Only**.
2. Duplicate that renderer, set the duplicate to layer **InvisibleFPS**, and set **Cast Shadows** to **Off**.
3. Keep both copies bound to the same compatible skeleton and verify the duplicate remains beneath `meshRoot`.

This yields a visible head for external views, an original that still casts a shadow, and a render copy hidden from the first-person camera. If head and body share one inseparable renderer, split it in the source model; do not put the entire body on `InvisibleFPS`.

## Focused Validation

After the first substantive asset or editor-code change, refresh Unity, wait for import/compilation and domain reload, and require no new Console errors. Then check:

1. Compare the reopened prefab with the stock Skin rig: direct `root`/mesh-root layout, exact bone names, complete bone references, renderer bounds, materials, and head-layer/shadow setup.
2. Open the generated `ItemInfoConfig`; compare `skinName`, prefab/model stem, active prefix, and exact `meshRoot`. Run its **Check** action.
3. Refresh Addressables through the repository tools, confirm exactly one `Skin/<prefix><ModName>` entry and the matching config/icon entries, then run `ValidateAddressables`.
4. Build only through the established PC VR or Quest workflow. Report bundle creation and file installation separately from mod discovery and gameplay behavior.
5. In ProjSAO/Battle Talent, verify the skin appears under the expected selection, applies to the player, animates without missing or exploding vertices, aligns hands and feet, does not block first-person view, and still produces the intended head/body shadows. Test both Windows and Quest when both are delivery targets.

Do not claim Unity import, Addressables validation, bundle build, installation, ProjSAO discovery, or in-headset appearance succeeded unless each was actually observed.
