using System;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.Util;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrossLink.EditorTools
{
    public sealed class PathFinderPreviewCuts
    {
        private sealed class Snapshot
        {
            public NavmeshCutSetup source;
            public string data;
            public Matrix4x4 matrix;
            public int meshVersion;
        }

        private readonly List<Snapshot> snapshots;
        public int Count { get; private set; }

        private PathFinderPreviewCuts(List<Snapshot> snapshots)
        {
            this.snapshots = snapshots;
        }

        public static PathFinderPreviewCuts Apply(AstarPath astar, RecastGraph graph, Transform temporaryRoot)
        {
            PathFinderPreviewCuts result = new PathFinderPreviewCuts(Capture());
            TileHandler handler = null;
            foreach (Snapshot snapshot in result.snapshots)
            {
                NavmeshCutSetup setup = snapshot.source;
                if ((setup.graphMask & (1 << (int)graph.graphIndex)) == 0)
                    continue;
                Validate(setup);
                Transform proxy = CopyTransformHierarchy(setup.transform, temporaryRoot);
                NavmeshCut cut = proxy.gameObject.AddComponent<NavmeshCut>();
                cut.hideFlags = HideFlags.HideAndDontSave;
                cut.InitializeForPreview();
                cut.type = (NavmeshCut.MeshType)setup.type;
                cut.mesh = setup.mesh;
                cut.rectangleSize = setup.rectangleSize;
                cut.circleRadius = setup.circleRadius;
                cut.circleResolution = setup.circleResolution;
                cut.height = setup.height;
                cut.meshScale = setup.meshScale;
                cut.center = setup.center;
                cut.updateDistance = setup.updateDistance;
                cut.isDual = setup.isDual;
                cut.cutsAddedGeom = setup.cutsAddedGeom;
                cut.updateRotationDistance = setup.updateRotationDistance;
                cut.useRotationAndScale = setup.useRotationAndScale;
                cut.graphMask = setup.graphMask;
                if (handler == null)
                {
                    handler = new TileHandler(graph);
                    handler.CreateTileTypesFromGraph();
                }
                handler.cuts.Add(cut, graph.GetTouchingTilesInGraphSpace(cut.GetBounds(graph.transform)));
                result.Count++;
            }

            if (handler != null)
            {
                // Reuse ProjSAO's clipping, triangulation and node reconnection, not a visual overlay.
                handler.StartBatchLoad();
                try
                {
                    handler.ReloadInBounds(new IntRect(0, 0, graph.tileXCount - 1, graph.tileZCount - 1));
                }
                finally
                {
                    handler.EndBatchLoad();
                }
                astar.FlushWorkItems();
            }
            return result;
        }

        public bool HasChanged()
        {
            List<Snapshot> current = Capture();
            if (current.Count != snapshots.Count)
                return true;
            for (int i = 0; i < current.Count; i++)
            {
                Snapshot before = snapshots[i];
                Snapshot after = current[i];
                if (before.source != after.source || before.data != after.data ||
                    before.matrix != after.matrix || before.meshVersion != after.meshVersion)
                    return true;
            }
            return false;
        }

        private static List<Snapshot> Capture()
        {
            NavmeshCutSetup[] setups = Object.FindObjectsOfType<NavmeshCutSetup>();
            Array.Sort(setups, (left, right) => left.GetInstanceID().CompareTo(right.GetInstanceID()));
            List<Snapshot> result = new List<Snapshot>();
            foreach (NavmeshCutSetup setup in setups)
            {
                if (!setup.isActiveAndEnabled || EditorUtility.IsPersistent(setup) ||
                    !setup.gameObject.scene.IsValid() || !setup.gameObject.scene.isLoaded ||
                    EditorSceneManager.IsPreviewScene(setup.gameObject.scene))
                    continue;
                result.Add(new Snapshot
                {
                    source = setup,
                    data = EditorJsonUtility.ToJson(setup),
                    matrix = setup.transform.localToWorldMatrix,
                    meshVersion = setup.mesh != null ? EditorUtility.GetDirtyCount(setup.mesh) : 0
                });
            }
            return result;
        }

        private static Transform CopyTransformHierarchy(Transform source, Transform temporaryRoot)
        {
            Stack<Transform> ancestors = new Stack<Transform>();
            for (Transform current = source; current != null; current = current.parent)
                ancestors.Push(current);
            Transform parent = temporaryRoot;
            while (ancestors.Count > 0)
            {
                Transform original = ancestors.Pop();
                GameObject proxy = new GameObject("Cut Preview: " + original.name);
                proxy.SetActive(false);
                proxy.hideFlags = HideFlags.HideAndDontSave;
                proxy.transform.SetParent(parent, false);
                proxy.transform.localPosition = original.localPosition;
                proxy.transform.localRotation = original.localRotation;
                proxy.transform.localScale = original.localScale;
                parent = proxy.transform;
            }
            return parent;
        }

        private static void Validate(NavmeshCutSetup setup)
        {
            if (!Enum.IsDefined(typeof(NavmeshCutSetup.MeshType), setup.type) ||
                !IsFinite(setup.center) || !IsFinite(setup.transform.localToWorldMatrix) ||
                !PathFinderPreviewParameters.IsFinite(setup.height) || setup.height < 0 ||
                !PathFinderPreviewParameters.IsFinite(setup.rectangleSize.x) ||
                !PathFinderPreviewParameters.IsFinite(setup.rectangleSize.y) ||
                !PathFinderPreviewParameters.IsFinite(setup.circleRadius) ||
                !PathFinderPreviewParameters.IsFinite(setup.meshScale))
                throw new InvalidOperationException("NavmeshCutSetup '" + setup.name + "' has invalid shape values.");

            if (setup.type == NavmeshCutSetup.MeshType.CustomMesh &&
                (setup.mesh == null || !setup.mesh.isReadable || setup.mesh.vertexCount == 0))
                throw new InvalidOperationException("NavmeshCutSetup '" + setup.name +
                    "' requires a nonempty, readable custom mesh. Enable Read/Write in its import settings.");
        }

        private static bool IsFinite(Vector3 value)
        {
            return PathFinderPreviewParameters.IsFinite(value.x) &&
                PathFinderPreviewParameters.IsFinite(value.y) && PathFinderPreviewParameters.IsFinite(value.z);
        }

        private static bool IsFinite(Matrix4x4 value)
        {
            for (int i = 0; i < 16; i++)
                if (!PathFinderPreviewParameters.IsFinite(value[i]))
                    return false;
            return true;
        }
    }
}
