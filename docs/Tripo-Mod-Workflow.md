# Tripo + Codex Quick Setup

Create BattleTalent mods with Codex, Tripo, and the BattleTalent Mod Toolkit.
This repository includes a Windows setup launcher and a shared Codex workflow.

## Prerequisites

- Windows 10 or Windows 11.
- Codex.
- Git for cloning and syncing the repository.
- Unity Hub and Unity Editor `2020.3.48f1`.
- Your own Tripo account.

The setup script checks Node.js and Tripo CLI and can install them when needed.
Each user must configure their own Tripo credentials, either through browser
authorization or an existing API key. Accounts, credentials, and credits are
not shared through Git.

The repository's Codex skill calls Tripo CLI directly, so the separate Tripo 3D
Codex plugin is optional. If you already use the plugin, you can keep it
installed; the shared mod workflow is defined by the files in this repository.

## First-Time Setup

1. Clone or download the repository.
2. Double-click `Setup-TripoWorkflow.cmd` in the repository root.
3. If Node.js is missing or older than version 20, allow setup to install or switch to the current LTS release. Windows may require administrator permission.
4. Complete browser authorization using the address and one-time code shown in the terminal, or configure an existing API key as described below. Setup uses the international account region (`ov`) automatically, without asking you to select a region.
5. Wait for `tripo doctor` to finish.
6. Open the repository root in Codex.

Setup does not generate models or submit paid generation tasks. It checks the
Unity project version but does not install Unity or verify that the Editor is
running. Install and open the required Unity Editor separately before importing
assets or configuring a mod.

## Use an Existing API Key

If you already have a Tripo API key, configure it in your own PowerShell terminal:

```powershell
tripo.cmd login --paste
```

Paste the key at the CLI prompt, then run setup again. Setup detects existing
credentials and skips browser authorization. If setup is already waiting for
browser approval, stop it with Ctrl+C before running the command.

Do not paste your key into a chat, pass it as a command argument, or save it in
scripts or Git. When no credentials are configured, setup displays these
instructions after the browser authorization steps, while waiting for approval.
Check-only mode displays them without starting browser authorization.

## Create Your First Mod

Start by stating what the asset is for: a model export, an existing mod, or a
new mod. Include the category and target platform. For example:

```text
Use Tripo to create a low-poly one-handed sword for Meta Quest, then turn the approved model into a BattleTalent weapon mod.

First generate a concept image from my description and show it to me.
Wait for my approval before using that image to generate a 3D model with Tripo.
```

Codex first uses Tripo to turn your description into a concept image and shows it for review.
Ask for visual changes until you are happy with the image. Tripo model generation
must wait for your explicit approval of a specific image version, then use that
image as its input rather than sending the description directly to Tripo.

## Choose The Intended Use

The workflow does not assume that every model is a weapon. Codex first resolves
the intended use and reads the matching existing authoring skill when preparing
a mod asset:

| Intended use | Existing authoring skill |
|---|---|
| Model export only | No mod setup; deliver the reviewed model and textures. |
| Melee weapon | `battle-talent-melee-weapon-authoring` with shared mod-authoring rules. |
| Firearm, including pistol, rifle, SMG, or shotgun | `battle-talent-gun-mod-authoring`. |
| Bow, spell, or standalone projectile | `battle-talent-mod-authoring` and a matching example. |
| NPC character / RoleMod | `projsao-role-mod-authoring`. |
| Player character / AvatarMod | `projsao-avatar-mod-authoring`. |
| Legacy player appearance / SkinMod | `projsao-skin-mod-authoring`. |
| Scene, environment prop, or asset for an existing mod | `battle-talent-mod-authoring` and the matching scene or host-mod example. |

If you request a "character" without specifying its role, Codex should ask
whether it is an NPC, a player Avatar, or a legacy Skin before generating
the concept. Those categories have different rig and component requirements.
The workflow uses the current toolkit's source requirements without asking
you to enter a game version. A generated rig still needs category-specific
validation, and successful authoring does not prove compatibility with every
released game version.

For a model-only request, try:

```text
Create a fantasy treasure chest for model export only, not a BattleTalent mod.
Show me a concept image first and wait for approval before Tripo generation.
Do not import into Unity or change Addressables.
```

Standalone song mods use `battle-talent-song-mod-authoring`, not Tripo model
generation. Scripted behavior and delivery use the existing Lua and build
skills only when needed.

You can also provide a finished image and explicitly ask Codex to use it as-is.
Concept generation uses Tripo's text-to-image API by default, with your existing
Tripo credentials after you approve its credit spending. Other image tools
being available does not change that default. You do not need an OpenAI API key.
To use a different provider, request it explicitly. If the Tripo image endpoint
is unavailable for your account, provide a concept image or choose an alternative;
the workflow will not silently switch services. The setup script installs Tripo
CLI; it does not install a separate image-generation service.

For example, to start a character without a version questionnaire:

```text
Create an OrcWarrior NPC RoleMod for Meta Quest using the current toolkit.
Do not ask me for a game version.
Generate and show a full-body T-pose concept first.
Use Tripo text-to-image for the concept and show me the credit-spending
plan before submission.
Wait for image approval before 3D generation, then follow the character
rig preparation and NPC authoring skills.
Do not build or install until I request it.
```

Before the Tripo task, review the target platform, triangle budget, dimensions,
and planned processing operations. Approving a concept image does not by itself
authorize credit spending. Model generation, retries, remeshing, rigging, and
conversions may spend credits; approve the plan before proceeding. Once a model
is generated, review its preview separately before Unity integration.

## Asset Workflow

Use this sequence for NPCMod, AvatarMod, and WeaponMod:

1. Supply a concept image approved for production, or confirm that Tripo should generate one. Review any generated image before proceeding.
2. Review the Tripo model generation plan and approve its credit spending.
3. Review the generated model. Weapons can then proceed to their authoring skill.
4. For NPCs and Avatars, accept player-provided models declared already rigged and proceed directly to authoring without a rig audit. For other generated sources, inspect existing binding and reuse it when suitable; if unbound, approve rig-check and biped/Mixamo binding with FBX output.
5. If the check, binding, or required source suitability fails, stop and rig/correct the model manually in Mixamo or Tripo Studio. Supply the exported FBX to resume; no automatic paid retries.
6. Create the mod through its matching skill: NPC Role, Avatar, firearm, or melee weapon. Finish the prefab/configuration and focused validation, rather than only producing a source-check report.

When you provide a model and state that it is already rigged, the workflow
accepts it and starts mod creation. It does not audit joints/weights, bake
test poses, render deformation contact sheets, or submit another Tripo rig.
Only actual import or builder failures prompt focused diagnosis. Routine options
use safe defaults and are reported unless you explicitly require confirmation.
Build and installation remain separate actions performed only when requested.

For AvatarMod and NPC RoleMod, the binding branch is:

```text
Approved character model
  -> Player-provided rigged model: direct authoring, no rig preflight
     OR automatically generated model: inspect existing binding
  -> Reuse it if suitable
     OR approve rig-check and rigging, then download the rigged FBX
  -> On failure: stop for manual Mixamo/Tripo Studio correction
  -> Resume from the supplied corrected model directly into authoring
  -> Automatic rig outputs only: focused Unity source validation
  -> Matching Avatar or NPC authoring skill
```

Unbound humanoid sources use Tripo rig-check followed by biped rigging with
Mixamo-compatible naming and FBX output, after approval of the API operations.
A failed or inconclusive check stops the workflow before rigging and offers
the manual correction route. Successful
rigging still needs Unity validation, and unsuitable skinning or missing bones
must be repaired before mod creation. Existing suitable binding is reused
instead of generating another paid rig.

Player-declared rigged inputs skip that source-validation stage. Standard
component configuration, saved references, and identifier checks still run;
skipped source audits and untested gameplay are not reported as passed.

## Preserve Model Size

Unless you explicitly request a height or size change, models retain their
existing Unity importer units/scale and internal transforms. New wrappers use
unit scale. The workflow does not normalize to a template, infer a 1.9-meter
height, convert units through compensating transforms, or bake corrective
scale into the mesh/skeleton. Slots, grips, and colliders are fitted to the
model as supplied. This applies to NPCs, Avatars, melee weapons, and guns.

Rigging does not automatically include animation clips. Animation retargeting
is separate, and legacy SkinMods require the stock player rig rather than a
new generic skeleton. Static weapons and props skip this character stage.

```text
Your model description and references
  -> Intended use, category, platform, and current toolkit source requirements
  -> Tripo concept image generation (default)
  -> Image review and revisions
  -> Explicit approval of a specific image
  -> Tripo plan review and credit-spending approval
  -> Tripo image-to-3D using the approved image
  -> Local generation output in tripo-out/
  -> Model review and approval
  -> Required character binding preparation (Avatar/NPC only)
     Failure: pause for manual rig/correction, then direct authoring from the supplied rig
  -> Model export and finish
     OR
  -> Matching existing mod-authoring skill
  -> Approved source assets under ModProj/Assets/Build/<ModName>
  -> Category-specific Unity import, configuration, and validation
  -> Addressables and build/install when requested
  -> Gameplay verification
```

`tripo-out/` is local staging and is excluded from Git. Concept images can be
stored in `tripo-out/concepts/` while you review them. Model-only outputs stay
outside Unity unless you ask for integration. For mod work, only approved source
assets and their Unity-generated `.meta` files belong in
`ModProj/Assets/Build/<ModName>`. Keep the selected concept and prompt with the
source assets when needed to document the approved design.

## Check or Update Your Setup

Check your environment without installing dependencies or starting login:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Setup-TripoWorkflow.ps1 -CheckOnly -SkipLogin
```

Update Tripo CLI:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Setup-TripoWorkflow.ps1 -ForceCliUpdate
```

The international account region (`ov`) is used automatically. Only override
it if your account belongs to the China service:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Setup-TripoWorkflow.ps1 -Region cn
```

An existing authenticated session is retained; the region option applies when
setup needs to start a new login.

## What Git Shares

Git shares the setup scripts, Codex skills, BattleTalent authoring rules,
templates, and approved mod source assets.

Each user's machine keeps the following separate:

- Node.js and npm installations.
- Codex installation and sign-in.
- Tripo OAuth credentials and API keys.
- Tripo account balance and credits.
- Local setup status in `.tripo-workflow/`.
- Unapproved generation output in `tripo-out/`.

Never commit account credentials, API keys, or login tokens.

## Troubleshooting

### npm Cannot Connect to a Proxy

An error such as `ECONNREFUSED 127.0.0.1:7890` means npm could not connect to
the configured local proxy. Start that proxy if your network requires it.
If your network permits direct access, rerun setup from the repository root:

```powershell
.\Setup-TripoWorkflow.cmd -NpmNoProxy
```

This option overrides npm proxy settings and proxy environment variables only
for the npm installation command. It does not edit `.npmrc`, change permanent
settings, or disable certificate verification. The previous process environment
is restored even if installation fails. It does not bypass proxy settings for
Node.js installation or subsequent Tripo commands.

### Codex CLI Is Not on PATH

This warning is not a setup failure and does not mean the Codex desktop app is
missing. Open the repository in the desktop app. Install the standalone Codex
CLI only if you want to use Codex from a terminal.
