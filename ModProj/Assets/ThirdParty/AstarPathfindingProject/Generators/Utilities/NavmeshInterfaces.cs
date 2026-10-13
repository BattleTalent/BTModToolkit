#if UNITY_EDITOR
// Extracted from A* 4.2.17 NavMeshGenerator.cs; the original license still applies.
namespace Pathfinding
{
    public interface INavmesh
    {
        void GetNodes(System.Action<GraphNode> del);
    }
}
#endif
