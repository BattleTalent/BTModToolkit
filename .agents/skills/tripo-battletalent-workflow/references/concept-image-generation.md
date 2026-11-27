# Concept Image Providers

Use a player-supplied approved concept image or generate and show a real
concept image before 3D generation. Respect an explicit provider choice;
do not switch services silently.

## Default To Tripo

- When the player supplies an image approved for production, use it directly
  and skip concept generation. Otherwise confirm that they want a Tripo
  concept and approve its image-task spending before submission.
- Generate concepts through Tripo CLI by default, using the player's existing
  Tripo credentials. This remains the default even when built-in or other image
  tools are available. Disclose the route and obtain approval for the image
  task's credit spending before submission. An OpenAI API key is not required.
- Use a different image tool/provider and its applicable skill only when the
  player explicitly selects it. A supplied finished image approved for use
  as-is skips concept generation; do not regenerate it merely to use Tripo.
- If the Tripo image endpoint is unavailable for the account, report the
  actual error and offer a player-provided image or an explicitly chosen
  alternative. Do not automatically switch to another image tool or demand a
  second service's credentials.

Unknown game versions or deployment package names do not block this stage.
Use the agreed category's visual requirements and the current toolkit's
source requirements.

## Tripo-Native Concept

Read current local guidance before submitting:

```powershell
tripo.cmd generate text-to-image --help
tripo.cmd docs --topic commands/generate
```

Show the prompt, image task, output location, and available cost information.
Do not invent a fixed credit price or promise that generation is free.
After explicit approval, the command for a humanoid concept can be:

```powershell
tripo.cmd generate text-to-image "full-body front view of an orc warrior in T-pose, simple leather armor, separated limbs and visible hands, plain background, no held weapon, no text" -p template=t_pose -o tripo-out/concepts/OrcWarrior-v1 --json --yes
```

Use `template=t_pose` only for a character whose authoring route requires that
pose, not for weapons or props. Do not hardcode an image model version; use
the player's choice or the installed CLI's documented default.
`generate text-to-image` is a live API command, not a dry-run. Do not attach
unsupported `--dry-run` flags or use text-to-model to "preview" an image.

Wait for completion, inspect the returned task result, and identify the
downloaded image from the reported files. Show it inline using its absolute
local path. Preserve its task ID, exact prompt, selected version, path, and
reported credits. Do not assume a model-preview filename for image tasks.

For revisions, inspect the current image-to-image help and request approval
for any additional paid task. Save a new version rather than overwriting the
approved file. Stop and wait for approval of the actual image before invoking
image-to-3D or rigging. Concept approval does not authorize later paid stages.
