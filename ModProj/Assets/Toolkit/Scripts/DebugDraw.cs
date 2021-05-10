using UnityEngine;
using System.Collections;

namespace CrossLink
{

    public static class DebugDraw
    {

        static public Color alpha05 = new Color(0, 0, 0, 0.5f);
        static public Color alpha025 = new Color(0, 0, 0, 0.25f);

        public static void DrawBone(Transform tran, bool depthTest = false)
        {
            DrawBone(tran, Color.cyan - DebugDraw.alpha05, depthTest);
        }

        public static void DrawBone(Transform tran, Color c, bool depthTest = false)
        {
#if false
        if (tran.childCount == 0 && tran.parent != null)
        {
            var dir = tran.position - tran.parent.position;
            dir = dir.normalized * 0.01f;
            //dir = tran.localRotation * dir;
            dir = (Quaternion.Inverse(tran.parent.rotation) * tran.rotation)* dir;
            Debug.DrawLine(tran.position,
                tran.position + dir, c, 0, depthTest);
            return;
        }
#endif

            for (int i = 0; i < tran.childCount; ++i)
            {
                var fchild = tran.GetChild(i);
                Debug.DrawLine(tran.position, fchild.position, c, 0, depthTest);
                DrawBone(fchild, c, depthTest);
            }
        }

        public static void DrawMarker(Vector3 position, float size, Color color, float duration, bool depthTest = true)
        {
#if UNITY_EDITOR
            Vector3 line1PosA = position + Vector3.up * size * 0.5f;
            Vector3 line1PosB = position - Vector3.up * size * 0.5f;

            Vector3 line2PosA = position + Vector3.right * size * 0.5f;
            Vector3 line2PosB = position - Vector3.right * size * 0.5f;

            Vector3 line3PosA = position + Vector3.forward * size * 0.5f;
            Vector3 line3PosB = position - Vector3.forward * size * 0.5f;

            Debug.DrawLine(line1PosA, line1PosB, color, duration, depthTest);
            Debug.DrawLine(line2PosA, line2PosB, color, duration, depthTest);
            Debug.DrawLine(line3PosA, line3PosB, color, duration, depthTest);
#endif
        }

        // Courtesy of robertbu
        public static void DrawPlane(Vector3 position, Vector3 normal, float size, Color color, float duration, bool depthTest = true)
        {
#if false
        Vector3 v3;
 
        if (normal.normalized != Vector3.forward)
            v3 = Vector3.Cross(normal, Vector3.forward).normalized * normal.magnitude;
        else
            v3 = Vector3.Cross(normal, Vector3.up).normalized * normal.magnitude;;
 
        Vector3 corner0 = position + v3 * size;
        Vector3 corner2 = position - v3 * size;
 
        Quaternion q = Quaternion.AngleAxis(90.0f, normal);
        v3 = q * v3;
        Vector3 corner1 = position + v3 * size;
        Vector3 corner3 = position - v3 * size;

        Debug.DrawLine(corner0, corner2, color, duration, depthTest);
        Debug.DrawLine(corner1, corner3, color, duration, depthTest);
        Debug.DrawLine(corner0, corner1, color, duration, depthTest);
        Debug.DrawLine(corner1, corner2, color, duration, depthTest);
        Debug.DrawLine(corner2, corner3, color, duration, depthTest);
        Debug.DrawLine(corner3, corner0, color, duration, depthTest);
        Debug.DrawRay(position, normal * size, color, duration, depthTest);
#endif
        }

        public static void DrawVector(Vector3 position, Vector3 direction, float raySize, float markerSize, Color color, float duration, bool depthTest = true)
        {
#if UNITY_EDITOR
            Debug.DrawRay(position, direction * raySize, color, duration, false);
            DebugDraw.DrawMarker(position + direction * raySize, markerSize, color, duration, false);
#endif
        }

        public static void DrawTriangle(Vector3 a, Vector3 b, Vector3 c, Color color)
        {
#if UNITY_EDITOR
            Debug.DrawLine(a, b, color);
            Debug.DrawLine(b, c, color);
            Debug.DrawLine(c, a, color);
#endif
        }

        public static void DrawTriangle(Vector3 a, Vector3 b, Vector3 c, Color color, Transform t)
        {
#if UNITY_EDITOR
            a = t.TransformPoint(a);
            b = t.TransformPoint(b);
            c = t.TransformPoint(c);

            Debug.DrawLine(a, b, color);
            Debug.DrawLine(b, c, color);
            Debug.DrawLine(c, a, color);
#endif
        }

        public static void DrawMesh(Mesh mesh, Color color, Transform t)
        {
#if UNITY_EDITOR
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                DrawTriangle(mesh.vertices[mesh.triangles[i]], mesh.vertices[mesh.triangles[i + 1]], mesh.vertices[mesh.triangles[i + 2]], color, t);
            }
#endif
        }

        // pos is center pos
        public static void DrawBox(Vector3 pos, Quaternion rot, Vector3 size, Color c, float duration = 0.001f)
        {
            var forward = rot * Vector3.forward;
            var right = rot * Vector3.right;
            var up = rot * Vector3.up;

            // forward and backward
            Debug.DrawLine(pos + forward * size.z / 2f - right * size.x / 2f + up * size.y / 2f,
                pos + forward * size.z / 2f + right * size.x / 2f + up * size.y / 2f, c, duration);
            Debug.DrawLine(pos + forward * size.z / 2f - right * size.x / 2f - up * size.y / 2f,
                pos + forward * size.z / 2f + right * size.x / 2f - up * size.y / 2f, c, duration);

            Debug.DrawLine(pos - forward * size.z / 2f - right * size.x / 2f + up * size.y / 2f,
        pos - forward * size.z / 2f + right * size.x / 2f + up * size.y / 2f, c, duration);
            Debug.DrawLine(pos - forward * size.z / 2f - right * size.x / 2f - up * size.y / 2f,
                pos - forward * size.z / 2f + right * size.x / 2f - up * size.y / 2f, c, duration);

            // right and left
            Debug.DrawLine(pos + forward * size.z / 2f - right * size.x / 2f + up * size.y / 2f,
                pos - forward * size.z / 2f - right * size.x / 2f + up * size.y / 2f, c, duration);
            Debug.DrawLine(pos + forward * size.z / 2f - right * size.x / 2f - up * size.y / 2f,
                pos - forward * size.z / 2f - right * size.x / 2f - up * size.y / 2f, c, duration);

            Debug.DrawLine(pos + forward * size.z / 2f + right * size.x / 2f + up * size.y / 2f,
        pos - forward * size.z / 2f + right * size.x / 2f + up * size.y / 2f, c, duration);
            Debug.DrawLine(pos + forward * size.z / 2f + right * size.x / 2f - up * size.y / 2f,
                pos - forward * size.z / 2f + right * size.x / 2f - up * size.y / 2f, c, duration);

            // up and down
            Debug.DrawLine(pos + forward * size.z / 2f - right * size.x / 2f + up * size.y / 2f,
                pos + forward * size.z / 2f - right * size.x / 2f - up * size.y / 2f, c, duration);
            Debug.DrawLine(pos + forward * size.z / 2f + right * size.x / 2f + up * size.y / 2f,
                pos + forward * size.z / 2f + right * size.x / 2f - up * size.y / 2f, c, duration);

            Debug.DrawLine(pos - forward * size.z / 2f - right * size.x / 2f + up * size.y / 2f,
                pos - forward * size.z / 2f - right * size.x / 2f - up * size.y / 2f, c, duration);
            Debug.DrawLine(pos - forward * size.z / 2f + right * size.x / 2f + up * size.y / 2f,
                pos - forward * size.z / 2f + right * size.x / 2f - up * size.y / 2f, c, duration);
        }

        public static Color RandomColor()
        {
            return new Color(Random.value, Random.value, Random.value);
        }

#if false
    public static void DrawLineInGame(Transform matrix, Vector3 start, Vector3 end, Color color)
    {
#if UNITY_EDITOR
        GL.PushMatrix();
        GL.MultMatrix(matrix.localToWorldMatrix);
        // Draw lines
        GL.Begin(GL.LINES);
        GL.Color(color);
        GL.Vertex3(start.x, start.y, start.z);
        GL.Vertex3(end.x, end.y, end.z);
        GL.End();
        GL.PopMatrix();

#if false
        GameObject myLine = new GameObject();
        myLine.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInHierarchy;
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lr.SetColors(color, color);
        lr.SetWidth(0.1f, 0.1f);
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        GameObject.Destroy(myLine, duration);
#endif
#endif
    }

    public static void DrawMarkerInGame(Transform matrix, Vector3 position, float size, Color color)
    {
#if UNITY_EDITOR
        position = matrix.InverseTransformPoint(position);

        Vector3 line1PosA = position + Vector3.up * size * 0.5f;
        Vector3 line1PosB = position - Vector3.up * size * 0.5f;

        Vector3 line2PosA = position + Vector3.right * size * 0.5f;
        Vector3 line2PosB = position - Vector3.right * size * 0.5f;

        Vector3 line3PosA = position + Vector3.forward * size * 0.5f;
        Vector3 line3PosB = position - Vector3.forward * size * 0.5f;

        DrawLineInGame(matrix, line1PosA, line1PosB, color);
        DrawLineInGame(matrix, line2PosA, line2PosB, color);
        DrawLineInGame(matrix, line3PosA, line3PosB, color);
#endif
    }
#endif
    }

}