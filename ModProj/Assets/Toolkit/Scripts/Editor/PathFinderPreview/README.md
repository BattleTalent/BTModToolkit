# PathFinder Preview

Open `Tools > Battle Talent > PathFinder Preview`, select a scene BoxCollider,
and generate a preview. Enable Scene view Gizmos to see the original A* surface,
outlines and bounds.

The basic parameters are `walkableClimb`, `minRegionSize`, `rasterizeTerrain`,
`rasterizeMeshes` and `rasterizeColliders`. The collapsed Advanced section
contains `cellSize` and `maxSlope`. Hover a parameter label for an English tooltip
explaining its meaning, units, defaults and important limitations.
The collapsed Fixed Settings section displays the read-only game defaults
and the BoxCollider-derived bounds, with tooltips for those fields as well.
These settings affect this editor preview only.
Game initialization scripts are unchanged. Geometry edits require a manual rescan.
The scan includes loaded scene geometry inside the bounds, as in ProjSAO.

Saved scene ranges have separate settings in
`ProjectSettings/PathFinderPreviewSettings.asset`, keyed by GlobalObjectId.
Unsaved scene ranges use in-memory settings. The scene and its range object
must be saved before those settings can be persisted.
Existing four-parameter settings are upgraded automatically: the original values
are retained, and `minRegionSize = 2`, `cellSize = 0.25` and `maxSlope = 30`
are added. Restore ProjSAO Defaults resets all seven editable parameters.

The temporary Astar object, graph nodes and drawing caches are never serialized
into a scene. They are released when the window closes, the scene changes,
the range is deleted, Play Mode starts or assemblies reload.

## Integration constraints

The vendor code is a Recast-only subset of the local ProjSAO A* 4.2.17 package.
Recast voxelization, mesh construction and visualization algorithms are retained;
unrelated runtime features and original editor UI have been removed.

Unity 2020.3 cannot attach MonoBehaviours compiled in an Editor-only assembly,
even to temporary objects. The core assembly is therefore a normal assembly
whose non-Editor C# files are mechanically guarded with `UNITY_EDITOR`.
Player compilation has no A* types. Editor assemblies remain Editor-only.
The subset has no external DLLs or runtime Resources dependencies.

Do not remove these guards or restore runtime components from the full vendor
package into the preview subset.
Confirm the vendor redistribution rights before distributing this prototype.

## Validation

- `Tools > Battle Talent > Validate PathFinder Preview` runs a minimal plane
  scan and checks original shader support.
- `Tools > Battle Talent > Validate PathFinder Preview Integration` checks
  collection switches, climb connectivity, ramp and narrow geometry, transformed
  bounds, stale detection, invalid inputs, settings disk reload, foreign singleton
  conflicts, new parameter effects, legacy settings migration and cleanup in an
  unsaved additive scene. The diagnostic shortcut is Ctrl+Shift+F7.
- `Show PathFinder Preview Test Scene` opens an unsaved visual fixture.
  `Close PathFinder Preview Test Scene` closes only that generated scene.
- `Validate Preview Domain Reload` opens that fixture, requests a live script
  reload, and checks scene cleanup and reload-time errors without editing source.

Reports are generated in `Library/PathFinderPreviewSmokeCheck.txt` and
`Library/PathFinderPreviewValidation.txt`, plus `Library/PathFinderPreviewReloadCheck.txt`.
No Addressables refresh, build or
installation is performed. Visual comparison against ProjSAO and persisted
saved-scene IDs require separate verification.
