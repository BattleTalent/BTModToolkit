using System;
using Pathfinding;
using Pathfinding.Util;
using UnityEditor;
using UnityEngine;

namespace CrossLink.EditorTools
{
    public sealed class PathFinderPreviewRenderer : IDisposable
    {
        private readonly AstarPath astar;
        private readonly RecastGraph graph;
        private readonly RetainedGizmos gizmos = new RetainedGizmos();
        private bool disposed;

        public bool Visible { get; set; } = true;
        public int DrawCount { get; private set; }
        public string LastError { get; private set; }

        public PathFinderPreviewRenderer(AstarPath astar, RecastGraph graph)
        {
            this.astar = astar;
            this.graph = graph;
            string materialRoot = PathFinderPreviewIntegration.PackageRoot + "/Editor/EditorAssets/Materials/";
            gizmos.surfaceMaterial = LoadMaterial(materialRoot + "Navmesh.mat");
            gizmos.lineMaterial = LoadMaterial(materialRoot + "NavmeshOutline.mat");
            SceneView.duringSceneGui += Draw;
        }

        private static Material LoadMaterial(string path)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null || material.shader == null || !material.shader.isSupported ||
                ShaderUtil.ShaderHasError(material.shader))
                throw new InvalidOperationException("PathFinder Preview material is missing or unsupported: " + path);
            return material;
        }

        private void Draw(SceneView sceneView)
        {
            if (disposed || !Visible || astar == null || AstarPath.active != astar ||
                astar.isScanning || Event.current == null || Event.current.type != EventType.Repaint ||
                sceneView.camera == null || !sceneView.drawGizmos)
                return;

            Camera previousCamera = Camera.current;
            try
            {
                Handles.SetCamera(sceneView.camera);
                astar.colorSettings.PushToStatic(astar);
                graph.OnDrawGizmos(gizmos, true);
                gizmos.FinalizeDraw();
                DrawCount++;
            }
            catch (Exception exception)
            {
                Visible = false;
                LastError = exception.Message;
                Debug.LogException(exception);
            }
            finally
            {
                if (previousCamera != null)
                    Handles.SetCamera(previousCamera);
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            SceneView.duringSceneGui -= Draw;
            gizmos.ClearCache();
            SceneView.RepaintAll();
        }
    }
}
