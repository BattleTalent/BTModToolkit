---
name: tripo-battletalent-workflow
description: 'Determine the intended asset or mod type, generate concept images and models with Tripo CLI by default, review each stage, prepare required Avatar/NPC skeleton binding, and hand off to the matching BattleTalent authoring skill. Use for Tripo-assisted asset creation, image-to-3D, character rigging, or refinement of existing Tripo models.'
---

# Tripo + BattleTalent Workflow

For new assets, follow this order: supplied or Tripo-generated approved concept -> approved 3D plan and credit spending -> model generation and player review -> Avatar/NPC binding reuse or approved rig-check/rig -> matching mod-authoring skill. Weapons skip the character binding stage.

This skill owns visual planning, approval, and Tripo asset generation. Existing authoring skills own mod scaffolding, components, metadata, and category-specific validation. Do not duplicate their implementation here or default every generated model to a weapon.

## Required Workflow

1. **Establish the concept.** Use the player's supplied image when approved for production, or obtain confirmation to generate a concept with Tripo. Tripo is the default image provider, not permission to submit an unapproved paid image task. Show the generated image and wait for approval; preserve an accepted supplied image without regeneration.
2. **Plan and generate the model.** Show the Tripo image-to-3D plan and credit spending, obtain approval, run the approved task, and present the actual model/preview for player review. Do not combine unapproved binding or animation tasks into this generation step.
3. **Resolve binding for NPC/Avatar only.** A player-provided model explicitly described as already rigged goes directly to its authoring skill, without a separate binding check. For other generated models, inspect whether binding is present and reuse suitable binding. Weapons proceed directly to their authoring route.
4. **Bind an unbound character.** Show the rigging plan and confirm credits, then run rig-check followed by biped/Mixamo rigging with FBX output for a suitable humanoid. If existing binding is unsuitable, stop and explain the source problem instead of silently discarding it or re-rigging.
5. **Stop on binding failure.** If rig-check, rigging, or required source suitability fails, explain the actual reason and direct the player to manual rigging/correction in Mixamo or Tripo Studio. Wait for a corrected model; do not repeat paid tasks or scaffold a character from an invalid source. Follow [Character rig preparation](references/character-rigging.md) for this pause/resume path.
6. **Make the selected mod.** Use the accepted model, including a player-declared rigged source, with the matching authoring skill below through prefab/config creation and focused validation. Do not finish with only a handoff or rig-validation report when mod creation was requested. Build/install only when requested.

## Player-Supplied Rigged Models And Size

- Accept the player's explicit statement that the supplied model is rigged as the route selection. Start Avatar/NPC authoring directly; do not require proof through an FBX parser, weight audit, bone inventory, deformation tests, pose renders, or a standalone source validator.
- Normal Unity import and the selected builder's required component assignments still run. Do not disable built-in tool error handling: if actual import or builder execution fails, report the specific failure and address only that slice. Do not perform speculative diagnostics before creation.
- Preserve the model's existing importer unit settings and scale, mesh/bind poses, skeleton hierarchy, and internal transforms. New wrapper transforms use unit scale. Do not normalize height, add compensating scale parents, rescale skeletons, bake geometry, or set `auto_size` to resize without an explicit player size/height request. "Use defaults" means the model's existing imported size, not a template's height or an invented 1.9 m target.
- If the player explicitly requests resizing, apply only that requested change using the category's scale/slot workflow. Otherwise configure slots, colliders, and grips around the unchanged model. An unusual apparent size is not permission to resize.
- Report a supplied rig as accepted without an independent rig audit. Do not label unperformed skinning, animation, or gameplay tests as passed.

## Resolve The Intended Use First

Before generating a concept or planning paid work, establish:

- Whether the player wants a model only, an asset for an existing mod, or a new mod.
- The intended category and platform: Windows PC VR, Meta Quest, or both.
- Category requirements that affect the concept and generated asset: dimensions, pose, separable parts, textures, topology, and rig/skin compatibility.

Use an explicit category from the player's request; do not ask them to repeat it. If they only request a "character," clarify whether it is an NPC Role, a player Avatar, or a legacy Skin. A decorative sword in a scene is not automatically a held weapon. A finished model is not automatically a request to create a mod.

Do not ask for a game version or make a version questionnaire an onboarding gate. Author against the current repository's API, templates, and source requirements. Record any game information the player volunteers, but continue concept/model preparation without it. Do not promise compatibility with every released game version: report actual validation and keep runtime compatibility unverified until observed.

Read only the applicable authoring skill before concept generation when its source requirements affect the image, rig, or geometry:

| Intended use | Authoring route |
|---|---|
| Model generation/export only | No mod-authoring skill. Review and deliver the approved model, textures, concept, and paths; stop without Unity scaffolding or Addressables changes. |
| Melee weapon, such as a sword, axe, hammer, or staff | [battle-talent-melee-weapon-authoring](../battle-talent-melee-weapon-authoring/SKILL.md), plus its shared [battle-talent-mod-authoring](../battle-talent-mod-authoring/SKILL.md) rules. |
| Firearm, such as a pistol, rifle, SMG, or shotgun | [battle-talent-gun-mod-authoring](../battle-talent-gun-mod-authoring/SKILL.md), plus its shared mod-authoring rules. Do not use the melee-only skill or stop at a model with colliders. |
| Bow, spell, or standalone projectile | [battle-talent-mod-authoring](../battle-talent-mod-authoring/SKILL.md) and the closest category example. Do not substitute the firearm or melee-only skill. |
| NPC RoleMod | [projsao-role-mod-authoring](../projsao-role-mod-authoring/SKILL.md). Do not apply Avatar or Skin setup to an NPC. |
| Player AvatarMod | [projsao-avatar-mod-authoring](../projsao-avatar-mod-authoring/SKILL.md). Do not apply NPC Role components. |
| Legacy player SkinMod | [projsao-skin-mod-authoring](../projsao-skin-mod-authoring/SKILL.md). Confirm the stock-rig compatibility requirement rather than treating it as an Avatar. |
| Scene, environment prop, or visual asset for an existing mod | [battle-talent-mod-authoring](../battle-talent-mod-authoring/SKILL.md) and a matching scene/host-mod example. Create only the components and registration the intended use requires. |

The `projsao-*` skill names do not by themselves determine the installed game or active deployment path. Use their current toolkit contracts for authoring. Resolve installation destinations from active builder code, the script it actually invokes, and device evidence when available; do not infer a destination from `#if false` examples. Deployment uncertainty must not block concept/model work or cause version questions. A Tripo auto-rig is not proof of a valid Role/Avatar Humanoid or stock-rig Skin compatibility. If required source preparation is unavailable, explain that limitation before spending credits.

Load [battle-talent-lua](../battle-talent-lua/SKILL.md) only for scripted behavior and [battle-talent-build](../battle-talent-build/SKILL.md) only for Addressables, build, installation, or runtime diagnosis. A standalone SongMod belongs to [battle-talent-song-mod-authoring](../battle-talent-song-mod-authoring/SKILL.md); do not generate a 3D asset for an audio-only request.

For concept generation, follow [Concept image providers](references/concept-image-generation.md). Default to Tripo-native text-to-image with the existing Tripo credentials and credit approval, even when other image tools are available. Use another provider and its skill only when the player explicitly selects it; do not make an OpenAI API key mandatory. If a referenced category skill is unavailable in a player's checkout, disclose the missing capability rather than substituting weapon setup.

## Concept Image First

1. Preserve the player's model prompt and references. Incorporate the agreed category's source requirements without changing the intended use.
2. Generate a concept image with Tripo text-to-image before submitting any Tripo model generation task, unless the player explicitly selects another provider or supplies an image to use as-is. Built-in image tool availability does not change the default.
3. Prepare an image suitable for 3D reconstruction: one complete asset, clearly visible silhouette and proportions, plain background, readable materials, and no unrelated objects, labels, watermarks, or cropped parts. Do not add design elements the player did not request.
4. Save each version under ignored staging, such as `tripo-out/concepts/<asset-name>-v1.png`. Show the actual image inline using its absolute local path, and identify the version. Do not substitute a written description for the image.
5. Ask whether the player approves this image for 3D generation or wants changes, then stop and wait. A general request to make a mod is not approval of an unseen concept image.
6. If changes are requested, revise the image and show the new version. Approval of an earlier version does not approve the revision.
7. Once the player approves a specific image, use that exact local file as the Tripo image-to-3D input. Do not silently replace it with the original text prompt.

If the player provides a finished reference image and explicitly asks to use it as-is, treat that as the approved concept; no extra concept generation is needed. Requests to refine an existing 3D model do not require a new concept unless the visual design is changing. Direct text-to-3D is an exception only when the player explicitly asks to skip concept review.

Show the Tripo text-to-image plan and ask for credit approval unless already authorized. If that endpoint is unavailable for the account, report the actual error and offer a player-provided concept image or an explicitly selected alternative. Do not automatically switch to built-in image generation, install another provider, request credentials in chat, or switch to a different paid service without approval. Never silently fall back to Tripo text-to-3D.

## Preconditions

1. Work from the repository root.
2. Check `.tripo-workflow/status.json` if it exists.
3. Run `tripo doctor` before the first paid Tripo operation in a session, including Tripo concept image generation when used.
4. If `tripo` is unavailable, tell the user to run `Setup-TripoWorkflow.cmd`.
5. Never request an API key in chat. When authentication is missing, use `tripo login --region ov --yes` for international accounts, or `--region cn` only for accounts on the China service. Retain an existing authenticated session; do not infer the account region from the user's language or machine location.

On Windows PowerShell, prefer `tripo.cmd` if execution policy prevents `tripo.ps1` from running.

## Read Current CLI Guidance

Do not invent Tripo flags or pipeline parameters. Before planning image-to-3D, read the smallest relevant built-in documentation:

```powershell
tripo docs --topic commands/make
tripo docs --topic examples/game-asset
```

Use `tripo docs --llm` only when the focused topics do not cover the request.

## Approved Image To Model

- Confirm the target platform: `game-mobile` for Quest-focused assets and `game-pc` for Windows PC VR.
- When targeting both, agree on the shared budget or separate outputs. Platform presets do not select the mod category or establish rig compatibility.
- Clarify the asset category, style, texture requirements, triangle budget, and whether rigging or animation is needed. Preserve default model size unless the player specifies a size or height; do not ask for dimensions solely to justify normalization.
- For textured 3D generation and requested retexturing, default to `texture_quality=detailed`. Explicitly pass `-p texture_quality=detailed` for generation, including `game-mobile` / Quest; do not inherit a preset's `standard` default. Use the documented equivalent for retexturing or texture-processing steps.
- Respect an explicit player quality choice or untextured output request. This default does not change the model, triangle budget, texture resolution, or processing chain, and does not authorize retexturing existing assets.
- For an individual asset, use `tripo make` with the approved image path rather than the interactive `tripo ai` command or a text-only input. Inspect and verify that the file matches the approved version before upload.
- First prepare a `--dry-run --json` plan and check that applicable generation/texturing payloads use `texture_quality=detailed`, unless the player explicitly chose otherwise. Include its higher price tier in the spending plan and obtain credit approval before running it. Image approval alone does not authorize additional paid processing.
- Use `--json --yes` only after approval for the shown plan, and allow the CLI to wait for task completion.
- Confirm again before unapproved batches, retries, remeshing, rigging, animation, or paid conversions. A changed plan must not inherit approval from an earlier plan.
- Treat `tripo-out/` as local staging. It is intentionally ignored by Git.
- Read `preview.png`, show the model preview, and inspect the generated model before proposing another paid attempt or integrating it. Concept approval is not approval of the generated model.
- Report the task result, output directory, model path, and credits consumed.

## Category-Specific Handoff

After the player accepts the generated model, branch on the agreed intended use:

- For AvatarMod or NPC RoleMod, use the player-supplied rigged fast path above when applicable. Otherwise follow [Character rig preparation](references/character-rigging.md): reuse suitable binding or, for an unbound model, obtain approval for Tripo `rig-check` and `rig`. Failure pauses for manual correction in Mixamo or Tripo Studio. A file extension alone is not a rig declaration.
- For model-only work, deliver the selected outputs and finish. Do not create a mod folder, run the Template Wizard, import into Unity, or change Addressables without a separate request.
- For an existing mod, identify its category and source owner. Integrate into that mod rather than creating a new mod or replacing unrelated assets.
- For a new mod, hand off to the selected authoring skill. Let that skill choose the template, required folders, components, metadata, and validation; do not use a weapon template for a non-weapon category.

Provide the authoring skill with the approved model, textures, concept/prompt, intended use, platform, current toolkit contract, budget, any explicit size request, and relevant generation results. For Avatar/NPC work, record the rig task/output or the player's declaration of supplied binding. The supplied-rigged fast path takes precedence over pre-scaffolding source-check instructions in the category skill.

For sources not covered by the supplied-rigged fast path, source checks are internal authoring steps, not additional player approval gates. Reuse existing successful results for unchanged sources. Final verification focuses on assets/configuration actually created: saving, references, component assignments, and identifiers. Additional joint/weight tests and multi-view or finger-close-up acceptance renders are not default deliverables.

When the player provides a manually rigged model and asks to continue, execute the requested authoring skill directly in the same turn when feasible. Do not restart generation, audit the rig, request another paid rig, resize by default, or stop solely for routine setup questions. Use the selected skill's safe defaults and report choices; preserve explicit approvals and stop for actual tool failures or permission boundaries.

For Unity-bound work, keep accepted source assets under the selected mod's `ModProj/Assets/Build/<ModName>` root, using a subfolder such as `Source/Tripo` when compatible with its layout. Import, configure, and save through Unity; preserve Unity-generated `.meta` files. Category-only components such as weapon damage/grip points, NPC slots, Avatar builders, and Skin binding belong exclusively to their authoring routes.

Refresh Addressables and build only when requested and the selected category passes its authoring checks. Follow the authoring skill's Unity Editor or batchmode rules rather than overriding them with a parallel setup. Distinguish concept approval, model generation/review, rig-check, rig task, downloaded binding, Unity Humanoid/skinning validation, category configuration, build, installation, and gameplay verification in the completion report; mark rigging as not applicable or reused when appropriate.

Keep the approved concept and its prompt with the selected sources when needed for reproducibility. Do not copy rejected concept versions into Unity.

## Source And Credential Safety

- Never commit Tripo tokens, API keys, browser login data, or user-level CLI configuration.
- Do not treat a generated model as gameplay-ready until Unity import and component configuration have been observed.
- Do not silently replace an existing asset or delete rejected generations.
- Keep only approved source assets in `ModProj/Assets/Build`; leave disposable generations in ignored `tripo-out/`.
