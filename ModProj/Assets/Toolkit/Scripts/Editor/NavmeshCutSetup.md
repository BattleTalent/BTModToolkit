# Navmesh Cut Setup

Add `Battle Talent > Navmesh Cut Setup` to the obstacle GameObject.
`CrossLink.NavmeshCutSetup` is authoring data, following the existing MTK Setup
component pattern. It does not create a `Pathfinding.NavmeshCut` or perform cuts.
The game must provide a matching setup component and a runtime bridge before
mods containing it can work. No Lua behavior is installed automatically.
MTK's PathFinder Preview does consume these settings in the Editor: Regenerate
Preview applies the original ProjSAO cutting algorithms to the preview graph.
This Editor integration does not supply the game-side bridge.

## Inspector

The Inspector follows ProjSAO's `NavmeshCutEditor`, including multi-object editing
and Undo. Shape enum values, field names and defaults follow NavmeshCut 4.2.17.

- Rectangle displays `rectangleSize` (X width and Z depth).
- Circle displays `circleRadius` and `circleResolution` (default 6, minimum 3).
- Custom Mesh displays `mesh` and `meshScale`. Use a flat, low-poly XZ surface
  with shared boundary vertices and Read/Write enabled.
- Height and Center apply to every shape.
- Movement threshold defaults to 0.4 units. Rotation threshold defaults to
  10 degrees and is shown when rotation/scale is enabled.
- Advanced contains Dual, Cuts Added Geom and Affected Graphs.
  Dual splits triangles without removing the interior; it is not an obstacle.

The standalone integer `graphMask` defaults to -1 (all graphs). A game-side
bridge must copy it into `NavmeshCut.graphMask.value`; it is not a serialized
`Pathfinding.GraphMask` struct. This keeps the runtime setup independent of
MTK's editor-only A* preview package.

## Gizmos And Runtime Semantics

Enable Scene view Gizmos. Unselected objects show the outline; selected objects
also show the vertical cutting volume. These are obstacle guides, not a preview
of the resulting cut navigation triangles. The guide assumes a world-aligned
navigation graph.
PathFinder Preview also supports rotated graph bounds; the setup Gizmo itself
is only a world-aligned cutting-volume guide, not the resulting graph.

When `useRotationAndScale` is false, only Transform position is applied; Center
is a world-space offset. When true, the outline and Center use the full local
to world transform. Height remains an unscaled vertical distance in both cases.

The game-side cut detects movement and, when enabled, rotation changes. Changing
only size, Center or scale requires `NavmeshCut.ForceUpdate()` on the runtime cut.
The runtime bridge must handle enable/disable and cleanup itself.

## Validation

The developer check `CrossLink.NavmeshCutSetupValidation.Run()` is retained
without a menu item or keyboard shortcut.
The check uses temporary objects, saves no scenes or prefabs, and writes
`Logs/NavmeshCutSetupValidation.txt`. It covers defaults, serialized properties,
shape round trips, mesh references, graph-mask bits, multi-edit, Undo and Gizmo
geometry helpers. Actual Inspector layout and Scene view drawing still require
visual inspection. Game discovery and navigation behavior require BattleTalent.

No Addressables refresh, build or installation is performed by this component.
