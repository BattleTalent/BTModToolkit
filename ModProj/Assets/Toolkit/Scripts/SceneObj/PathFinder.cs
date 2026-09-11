using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
#if UNITY_EDITOR
    [TextArea(10, 10)]
#endif
    public string tips = @"
    1.Surfaces that need navigation, such as floors, rooftops, platforms, and stairs, must be on the CharacterObstacle layer.

    2.Meshes used by MeshCollider must have Read/Write Enabled turned on; otherwise, Astar cannot read the mesh data.
    ";
}
