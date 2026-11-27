#define ASTAR_NO_GRID_GRAPH
#define ASTAR_NO_POINT_GRAPH
#define ASTAR_NO_LINKS
#define ASTAR_NO_ZIP
#define ASTAR_NoGUI
#if UNITY_EDITOR
namespace Pathfinding {
	[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
	public class UniqueComponentAttribute : System.Attribute {
		public string tag;
	}
}

#endif
