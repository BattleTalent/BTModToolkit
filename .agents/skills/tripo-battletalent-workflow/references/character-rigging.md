# Character Rig Preparation

Use this stage after model approval and before AvatarMod or NPC RoleMod
scaffolding. It prepares the source asset; the selected authoring skill still
owns its Unity setup and validation contract.

## Decide Whether Binding Is Needed

The player-supplied rigged fast path takes precedence over the checks below.
When the player supplies a file and explicitly states it is already rigged,
accept that input and start the Avatar/Role authoring skill without a separate
rig audit or Tripo check. Do not parse weights, count body/finger bones, run
joint/deformation tests, or render pose contact sheets for this path unless
the player requests diagnostics or an actual builder/import failure requires
a narrowly scoped investigation. Acceptance is not a claim of independent
validation.

- Inspect the selected model or a trustworthy prior Unity inspection for a
  skeleton, skin weights, and suitability for the requested category.
  An FBX filename, "rigged" prompt, or successful geometry task is not evidence.
- If existing binding passes the category's source checks, reuse it and record
  that rigging was skipped. Do not upload or re-rig it just to follow a checklist.
- If a Tripo-generated Avatar/NPC model has no binding, rig-check and rigging
  are required before proceeding to mod creation, subject to credit approval.
- If it has binding but the bones, skin weights, or required hand chains are
  unsuitable, stop and explain the source failure. Offer manual correction
  through Mixamo or Tripo Studio; do not silently discard an authored rig or
  submit another paid rig as a default repair.
- Do not apply this stage to static weapons or props. For model-only work, rig
  only when requested. Legacy SkinMod requires compatibility with the stock
  player skeleton; a new Tripo/Mixamo skeleton is not a substitute.
- The current Avatar/Role routes require Humanoid sources. If the requested
  creature is not suitable for that contract, stop and resolve the authoring
  route before paying for a quadruped or other non-Humanoid rig.

## Prepare The Character And Plan

Read the selected Avatar or Role skill before creating the concept image.
Prefer a complete T-Pose or suitable A-Pose with separated limbs, visible
hands, and no held weapon fused into the body. Avatar finger motion additionally
needs the finger chains required by its hand-pose workflow; do not assume that
an automatic body rig supplies them.

Confirm the installed CLI's current behavior without submitting a task:

```powershell
tripo.cmd docs --topic commands/make
tripo.cmd docs --topic examples/animation
tripo.cmd anim check --help
tripo.cmd anim rig --help
```

After approving the generated mesh, identify its exact source task and local
model file. Prefer the native GLB/source task for rig-check; if import or format
conversion is needed, include that operation in the plan.

For an appropriate biped Humanoid, request `rig_type=biped`, `spec=mixamo`, and
`out_format=fbx`. Do not hardcode a model version; validate the installed CLI's
accepted parameters. Keep platform-specific mesh budgets, materials, and scale
requirements from the original asset plan.

Example offline plan for an approved local source (replace the path with the
real approved file):

```powershell
$approvedModel = "tripo-out/MyCharacter/model.glb"
tripo.cmd make $approvedModel --then "rig-check,rig:rig_type=biped,spec=mixamo,out_format=fbx" --dry-run --json
```

This plan includes an import for local model inputs. Review the resulting
payloads, warnings, and validation errors; do not treat dry-run validity as
proof that the model is riggable. Show the player the check, import/conversion,
and rig operations and obtain approval before submitting them. Standalone
`anim check` is an API task, not a local dry-run; do not run it under an
assumption that it is free.

Do not use `--for anim` as a shortcut without reviewing its processing chain:
it may include animation retargeting beyond the requested skeleton binding.
Animation clips and retargeting require separate authorization when requested.

## Execute Check Then Rig

After approval, execute the reviewed check-and-rig chain with `--json --yes`
and wait for its completed output. Use the CLI's rig-check gate: if riggability
is negative, unavailable, or inconclusive, stop before rig submission and
report the result. A successful check task status alone is not a positive
riggability verdict.

If running separate commands, `tripo.cmd anim check` checks the source and
`tripo.cmd anim rig` accepts `--rig-type biped --spec mixamo --out-format fbx`.
Use the approved source model/task for rig input, not the rig-check task ID:
rig-check does not produce a model. Inspect current CLI docs before resolving
task references, download paths, or any missing flags.

Record check/rig task IDs, result status, actual downloaded file, textures,
and reported credits. Preserve the unrigged source and do not overwrite an
existing authored model. If rigging fails or credits are insufficient, stop;
do not repeat a paid task without approval or hand off the static mesh as ready.

## Manual Fallback And Resume

If rig-check is negative/inconclusive, rigging fails, or its output lacks
required bones or acceptable skinning, stop automatic character creation.
Explain which operation failed and preserve the accepted geometry, textures,
source/check/rig task IDs, and error result. Tell the player they can rig or
correct the model manually in Mixamo or Tripo Studio and provide the exported
rigged FBX and any accompanying textures. Do not operate their login, upload
the model to another service, or spend additional credits without authorization.

After the player supplies the manually rigged file and asks to continue:

1. Accept that file as the binding source and start the selected Avatar/NPC
   authoring skill directly. Do not run Tripo or Unity rig preflight checks
   just to certify a source the player says is already rigged.
2. Preserve its importer scale/units, skeleton, weights, bind poses, and
   internal transforms. Do not resize or normalize without an explicit size
   request; a new model wrapper uses unit scale.
3. Recover approved original materials/textures when a rig-only export omits
   them, through the authoring route's asset pipeline. Do not treat a missing
   color texture as proof that the skeleton failed.
4. Continue AvatarMod or RoleMod creation. If normal import or builder execution
   actually fails, report that failure and investigate only what blocks
   creation; do not add a separate acceptance-testing pipeline.

A supplied manual rig does not need another concept approval when the approved
appearance is unchanged. Respect an explicit model-review request or obtain
approval again if the visual design actually changed.

## Verify And Hand Off

This section applies to automatically rigged outputs, not player-declared
rigged inputs. Inspect the downloaded result, not merely the pre-rig preview. Through Unity,
check that the body has a `SkinnedMeshRenderer`, referenced bones, skin weights
and bind poses, and a valid Humanoid Avatar with the bones required by the
selected authoring skill. Inspect representative joint deformation and the
required Avatar finger chains when applicable. Setting ModelImporter to
Humanoid does not create a skeleton or skin weights.

Keep these checks proportional and reuse unchanged validated sources. A
standalone diagnostic report is not the final deliverable of a requested mod;
perform checks within the authoring pass and continue when they pass.

For all sources, preserve default imported size unless the player explicitly
requests a height/size change. Do not infer a unit conversion and compensate
for it in the mesh, skeleton, importer, or prefab hierarchy.

Use the authoring route's safe source-import location; do not pre-create the
final mod root if doing so would conflict with Template Wizard uniqueness.
Stop before scaffolding on missing bones, invalid mapping, absent skinning,
or unacceptable deformation. Explain any required source/DCC correction.

Hand off the rigged model and textures plus check/rig evidence, Unity source
validation, and any limitations to the existing Avatar/Role skill. If the rig
result changed the approved geometry or appearance, obtain model approval
again. Tripo rig success, Unity source validity, prefab completion, and gameplay
are separate observations.
