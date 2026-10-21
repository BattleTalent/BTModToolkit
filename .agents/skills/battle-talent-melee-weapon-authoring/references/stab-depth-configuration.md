# Stab Depth Configuration

Use this reference when creating or repairing the depth profile of a
`StabObject.StabGeometry`. Do not change serialized assets outside Unity.

## Source Contract

Start with `ModProj/Assets/Toolkit/Scripts/StabObject.cs`: field tooltips,
`CanNotBuildStab`, coordinate transforms, and `OnDrawGizmosSelected`.
Inspect one comparable prefab under `ModProj/Assets/Build`, but treat its
numbers as an example, not a preset.

If a local ProjSAO checkout is available, inspect its
`Assets/Scripts/GamePlay/RagdollCharacter/StabObject.cs`, especially
`BuildStab`, `UpdateStabSpring`, and the update loop that checks `deepStab`
and removes connections. Use it as runtime evidence for that checkout, not a
guarantee that every shipped game version behaves identically. Do not require
a machine-specific absolute path.

## Meaning And Sign

The runtime measures projected displacement along the Geo's forward direction.
The Inspector stores negative offsets; a more negative value corresponds to a
greater positive depth. These fields are not box dimensions or damage values.

| Field | Meaning |
|---|---|
| `stabMin` | Shallow endpoint used by the slide calculation and withdrawal test. The checked runtime removes a connection when depth retreats across `-stabMin`; it is not the initial hit or damage trigger. |
| `stabMax` | Deep endpoint for clamping the connection's slide calculation and normalizing depth-dependent spring/damper behavior. It is not a guarantee of a perfectly rigid physical penetration stop. `CanNotBuildStab` returns true for a nonnegative value. |
| `stabTie` | Retention threshold. In the checked runtime's ordinary dynamic, unheld-weapon branch, depth greater than `-stabTie` keeps the connection, unless `bypassStabTie` is set. Shallower depth does not guarantee immediate detachment, and this is not a universal held-weapon lock threshold. |

Select positive values `shallowDepth`, `deepDepth`, and `retentionDepth`, then:

```text
0 < shallowDepth < deepDepth
shallowDepth <= retentionDepth <= deepDepth

stabMin = -shallowDepth
stabMax = -deepDepth
stabTie = -retentionDepth
```

Do not compare negative values as if "min" were numerically smaller than "max".
Reject NaN/infinity, inverted/equal slide endpoints, nonnegative penetration
endpoints, and retention values outside the interval. Endpoint equality for
retention is allowed by existing examples; prefer an interior value for a new
profile and explain any endpoint choice.

## Fit Behavior To The Actual Blade

- Finalize physical scale first. The checked runtime projects world-space
  displacement onto `GetForwardWorld`; depths are distances, not proportions
  of the imported mesh's arbitrary unit scale. Keep frame directions nonzero,
  normalized, and perpendicular; zero `geoThick` is not a valid thickness axis.
- For a tip Geo (`sideGeo = -1`), aim `geoForward` at the tip. Select a deep
  limit that leaves the guard/grip outside the intended embedded portion.
  Measure the usable tip-to-guard distance in the final model frame.
- For a side-cut Geo (`sideGeo >= 0`), aim `geoForward` out through the sharpened
  edge and `sliceDir` along the blade. Tune penetration along that cutting
  direction using blade cross-section and desired cut behavior, not the
  full edge length represented by `geoWidth`. Do not copy the tip profile.
- Box coverage does not define the slide range. A localized tip detection box
  may be shorter than the chosen deep limit; do not require
  `abs(stabMax) <= geoFwdDis`. Conversely, a long detection box does not justify
  deeper penetration through the guard.
- Revisit retention separately: a threshold closer to zero reaches the
  unheld retention condition at a shallower depth; a more negative threshold
  requires deeper entry. Validate release behavior rather than assuming that
  tweaking retention fixes incorrect direction, box placement, or colliders.

As comparison data, `Dagger_Bleed/Weapon/Dagger_Bleed.prefab` uses:

| Geo role | `stabMin` | `stabMax` | `stabTie` |
|---|---|---|---|
| Tip | `-0.05` | `-0.15` | `-0.06` |
| Side-cut | `-0.02` | `-0.04` | `-0.04` |

The class defaults (`-0.08`, `-0.2`, `-0.12`) are not a universal sword profile,
especially for a side-cut Geo. Report measurements and the chosen behavior
instead of treating any example as a certified fit for a generated model.

## Verify The Saved Result

After the creator or repair saves through Unity, reload the prefab and report
each Geo's role, `geoForward`, `geoThick`, `sliceDir` where applicable, explicit
depth values, positive interpretations, and assigned blade colliders. Check
the full profile, not just that a `StabObject` exists.

The Toolkit Gizmo begins at the Geo's forward-face center:

```text
F = GetGeoPoint(transform) + GetForwardWorld(transform) * geoFwdDis
```

It draws a black line from `F` to `F + forward * stabMin`, a red line from that
point to `F + forward * stabMax`, and a red sphere at
`F + forward * stabTie`. Use these to inspect the chosen offsets, but do not
confuse the display origin with every runtime collision anchor.

In Battle Talent, test tip and side-cut behavior separately: shallow entry,
deeper entry, slow withdrawal, and release while embedded. Record runtime
verification as pending until observed. A numeric pass cannot prove collision,
retention, or VR handling behavior.
