using UnityEngine;

namespace CrossLink
{
    /// <summary>
    /// Authoring data for a game-side NavmeshCut. This component does not cut navigation meshes.
    /// </summary>
    [AddComponentMenu("Battle Talent/Navmesh Cut Setup")]
    public class NavmeshCutSetup : MonoBehaviour
    {
        public enum MeshType
        {
            Rectangle = 0,
            Circle = 1,
            CustomMesh = 2
        }

        [Tooltip("Shape projected onto the navigation graph's XZ plane.")]
        public MeshType type;

        [Tooltip("Flat XZ mesh whose boundary is used as the cut. Enable Read/Write in its import settings.")]
        public Mesh mesh;

        [Tooltip("Rectangle width along X and depth along Z, in world units unless rotation and scale are enabled.")]
        public Vector2 rectangleSize = Vector2.one;

        [Tooltip("Circle radius, in world units unless rotation and scale are enabled.")]
        public float circleRadius = 1;

        [Min(3)]
        [Tooltip("Number of circle vertices. Low resolutions produce fewer navigation triangles.")]
        public int circleResolution = 6;

        [Min(0)]
        [Tooltip("Vertical cutting range, centered on Center. Transform scale does not scale this height.")]
        public float height = 1;

        [Tooltip("Additional scale applied to the custom mesh.")]
        public float meshScale = 1;

        [Tooltip("Center offset. Local space when Use Rotation And Scale is enabled; otherwise a world-space offset.")]
        public Vector3 center;

        [Min(0)]
        [Tooltip("World-space movement required before the game updates this cut.")]
        public float updateDistance = 0.4f;

        [Tooltip("Split navigation triangles at the boundary without removing the interior. Not a blocking obstacle.")]
        public bool isDual;

        [Tooltip("Also cut geometry introduced by NavmeshAdd components.")]
        public bool cutsAddedGeom = true;

        [Range(0, 180)]
        [Tooltip("Rotation in degrees required before updating. Only used with Use Rotation And Scale.")]
        public float updateRotationDistance = 10;

        [Tooltip("Transform the outline and center using this object's rotation and scale. Height remains unscaled.")]
        public bool useRotationAndScale;

        // Store the bitmask without depending on MTK's editor-only Pathfinding.GraphMask.
        [Tooltip("Affected graph bitmask. -1 means all graphs. The runtime bridge must assign NavmeshCut.graphMask.value.")]
        public int graphMask = -1;
    }
}
