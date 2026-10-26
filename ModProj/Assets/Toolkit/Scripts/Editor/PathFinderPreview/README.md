# PathFinder Preview

Open `Tools > PathFinder Preview`, select a scene BoxCollider,
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

## Navmesh Cuts

Enabled `CrossLink.NavmeshCutSetup` components in loaded regular scenes now cut
the generated graph, using ProjSAO's original NavmeshCut/TileHandler algorithms.
This changes the actual graph triangles and connections, not just their drawing.
Rectangle, Circle and Custom Mesh shapes, Center, Height, full transform
hierarchies, Dual and graph masks are forwarded to the original cutting code.
Disabled components, inactive objects, prefab assets and preview scenes are ignored.
The preview contains one graph (index 0): Affected Graphs must include Graph 0,
or Everything, for a setup to affect it. `Cuts` counts enabled setups accepted
by this graph, including ones whose bounds lie outside the scan.

Cut changes mark the preview stale, including enabling/disabling, adding/removing,
moving, rotating/scaling, field changes and dirty custom meshes. Use Regenerate
Preview to apply them. Runtime movement thresholds do not delay manual scans.
Custom meshes must be readable. Invalid setup data fails generation with the
setup's name instead of displaying an apparently successful uncut graph.
Dual splits triangle boundaries without removing the interior. `cutsAddedGeom`
does not disable cuts on the scanned floor; no NavmeshAdd authoring bridge is
provided here.

Native cut proxies and their mirrored transform hierarchies are inactive,
hidden, nonpersistent children of the temporary preview host. Nothing is added
to the authored obstacle. They are destroyed with the preview, and no game-side
component or initialization script is created or changed.

Saved scene ranges have separate settings in
`ProjectSettings/PathFinderPreviewSettings.asset`, keyed by GlobalObjectId.
Unsaved scene ranges use in-memory settings. The scene and its range object
must be saved before those settings can be persisted.
Existing four-parameter settings are upgraded automatically: the original values
are retained, and `minRegionSize = 2`, `cellSize = 0.25` and `maxSlope = 30`
are added. Restore Defaults resets all seven editable parameters.

The temporary Astar object, graph nodes and drawing caches are never serialized
into a scene. They are released when the window closes, the scene changes,
the range is deleted, Play Mode starts or assemblies reload.

## Integration constraints

The vendor code is a Recast-only subset of the local ProjSAO A* 4.2.17 package.
Recast voxelization, mesh construction, visualization and the original tile
cutting algorithms are retained; unrelated runtime features and original editor
UI have been removed. Cutting is applied explicitly after a preview scan,
without restoring the game's automatic NavmeshUpdates lifecycle.

Unity 2020.3 cannot attach MonoBehaviours compiled in an Editor-only assembly,
even to temporary objects. The core assembly is therefore a normal assembly
whose non-Editor C# files are mechanically guarded with `UNITY_EDITOR`.
Player compilation has no A* types. Editor assemblies remain Editor-only.
The cutting code uses the original ProjSAO `Pathfinding.ClipperLib.dll` and
`Pathfinding.Poly2Tri.dll` under `Plugins/Editor`. Their importers are configured
and validated as Editor-only, excluded from Windows and Android players.
There are no runtime Resources dependencies.

Do not remove these guards or enable the cutting DLLs for players.
The imported NavmeshCut has a preview-only initializer for inactive proxies and
is hidden from Add Component; NavmeshAdd is present only as a TileHandler type
dependency, not a runtime or authoring feature.
The original cut bounds now union all contours instead of resetting at each
contour, so disconnected custom contours in different tiles are not omitted.
Confirm the vendor redistribution rights before distributing this prototype.

## Validation

Developer checks and demo helpers are retained as public static methods, but
are not registered as menu items or keyboard shortcuts.

- `PathFinderPreviewIntegration.RunSmokeCheck()` runs a minimal plane
  scan and checks original shader support.
- `PathFinderPreviewValidation.Run()` checks
  collection switches, climb connectivity, ramp and narrow geometry, transformed
  bounds, stale detection, invalid inputs, settings disk reload, foreign singleton
  conflicts, new parameter effects, legacy settings migration and cleanup in an
  unsaved additive scene.
- `PathFinderPreviewValidation.ShowDemo()` opens an unsaved visual fixture.
  `PathFinderPreviewValidation.CloseDemo()` closes only that generated scene.
- `PathFinderPreviewValidation.ValidateDomainReload()` opens that fixture, requests a live script
  reload, and checks scene cleanup and reload-time errors without editing source.
- `PathFinderPreviewCutValidation.Run()` checks actual removed
  triangle area/coverage, three shapes, graph masks, activation, Dual, center,
  height, rotation/negative scale/shear, rotated graphs, stale detection and
  failure/repeated-clear cleanup in an unsaved additive scene.

Reports are generated in `Library/PathFinderPreviewSmokeCheck.txt` and
`Library/PathFinderPreviewValidation.txt`, plus `Library/PathFinderPreviewReloadCheck.txt`.
Cut results are written to `Library/PathFinderPreviewCutValidation.txt`.
No Addressables refresh, build or
installation is performed. Visual comparison against ProjSAO and persisted
saved-scene IDs require separate verification.
