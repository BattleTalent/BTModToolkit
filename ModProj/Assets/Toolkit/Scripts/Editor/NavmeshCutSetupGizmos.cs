using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CrossLink
{
    public static class NavmeshCutSetupGizmos
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void Draw(NavmeshCutSetup setup, GizmoType gizmoType)
        {
            Color previous = Gizmos.color;
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = new Color(37f / 255, 184f / 255, 239f / 255);
            bool selected = (gizmoType & GizmoType.Selected) != 0;
            try
            {
                switch (setup.type)
                {
                    case NavmeshCutSetup.MeshType.Rectangle:
                        Vector2 half = setup.rectangleSize * 0.5f;
                        Vector3 a = new Vector3(-half.x, 0, -half.y);
                        Vector3 b = new Vector3(half.x, 0, -half.y);
                        Vector3 c = new Vector3(half.x, 0, half.y);
                        Vector3 d = new Vector3(-half.x, 0, half.y);
                        DrawEdge(setup, a, b, selected);
                        DrawEdge(setup, b, c, selected);
                        DrawEdge(setup, c, d, selected);
                        DrawEdge(setup, d, a, selected);
                        break;
                    case NavmeshCutSetup.MeshType.Circle:
                        int count = Mathf.Max(3, setup.circleResolution);
                        for (int i = 0; i < count; i++)
                        {
                            float angle = i * Mathf.PI * 2 / count;
                            float next = (i + 1) * Mathf.PI * 2 / count;
                            a = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * setup.circleRadius;
                            b = new Vector3(Mathf.Cos(next), 0, Mathf.Sin(next)) * setup.circleRadius;
                            DrawEdge(setup, a, b, selected);
                        }
                        break;
                    case NavmeshCutSetup.MeshType.CustomMesh:
                        DrawMesh(setup, selected);
                        break;
                }
            }
            finally
            {
                Gizmos.color = previous;
                Gizmos.matrix = previousMatrix;
            }
        }

        private static void DrawMesh(NavmeshCutSetup setup, bool selected)
        {
            Mesh mesh = setup.mesh;
            if (mesh == null || !mesh.isReadable)
                return;

            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Dictionary<ulong, int> edges = new Dictionary<ulong, int>();
            for (int i = 0; i + 2 < triangles.Length; i += 3)
            {
                CountEdge(edges, triangles[i], triangles[i + 1]);
                CountEdge(edges, triangles[i + 1], triangles[i + 2]);
                CountEdge(edges, triangles[i + 2], triangles[i]);
            }
            foreach (KeyValuePair<ulong, int> edge in edges)
            {
                if (edge.Value != 1)
                    continue;
                int from = (int)(edge.Key >> 32);
                int to = (int)(edge.Key & uint.MaxValue);
                DrawEdge(setup, vertices[from] * setup.meshScale, vertices[to] * setup.meshScale, selected);
            }
        }

        private static void CountEdge(Dictionary<ulong, int> edges, int from, int to)
        {
            uint min = (uint)Mathf.Min(from, to);
            uint max = (uint)Mathf.Max(from, to);
            ulong key = ((ulong)min << 32) | max;
            int count;
            edges.TryGetValue(key, out count);
            edges[key] = count + 1;
        }

        private static void DrawEdge(NavmeshCutSetup setup, Vector3 from, Vector3 to, bool selected)
        {
            from = OutlinePoint(setup, from);
            to = OutlinePoint(setup, to);
            Gizmos.DrawLine(from, to);
            if (!selected)
                return;

            // ProjSAO projects the outline to XZ and extrudes an unscaled, vertical height.
            float mid = OutlinePoint(setup, Vector3.zero).y;
            Vector3 fromLow = new Vector3(from.x, mid - setup.height * 0.5f, from.z);
            Vector3 toLow = new Vector3(to.x, fromLow.y, to.z);
            Vector3 fromHigh = new Vector3(from.x, mid + setup.height * 0.5f, from.z);
            Vector3 toHigh = new Vector3(to.x, fromHigh.y, to.z);
            Gizmos.DrawLine(fromLow, toLow);
            Gizmos.DrawLine(fromHigh, toHigh);
            Gizmos.DrawLine(fromLow, fromHigh);
        }

        private static Vector3 OutlinePoint(NavmeshCutSetup setup, Vector3 point)
        {
            return setup.useRotationAndScale
                ? setup.transform.TransformPoint(point + setup.center)
                : setup.transform.position + setup.center + point;
        }
    }
}
