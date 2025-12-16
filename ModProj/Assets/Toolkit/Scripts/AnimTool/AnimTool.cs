#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;


namespace CrossLink
{

    public class AnimTool : EditorWindow
    {
        public static AnimLayoutDataItem[] ExportRootMotions(string animPath)
        {
            animPath = "Assets/Resources/" + animPath;
            string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { animPath });
            List<AnimLayoutDataItem> itemList = new List<AnimLayoutDataItem>();

            for (int i = 0; i < guids.Length; ++i)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);

                if (clip != null)
                {
                    var item = ExportRootMotion(clip);
                    itemList.Add(item);

                    EditorUtility.DisplayProgressBar("export animation clip root motion",
                        $"current: {clip.name} ({i + 1}/{guids.Length})",
                        (float)i / guids.Length);
                }
            }

            EditorUtility.ClearProgressBar();

            return itemList.ToArray();
        }

        public static AnimLayoutDataItem ExportRootMotion(AnimationClip clip)
        {
            if (clip == null)
                return null;
            string prefix = AddressableConfig.GetConfig().GetPrefix();

            var item = new AnimLayoutDataItem();
            item.Name = prefix + clip.name;
            item.Len = clip.length;

            var curves = AnimationUtility.GetAllCurves(clip);
            for (int i = 0; i < curves.Length; ++i)
            {
                if (curves[i].propertyName == "RootT.x")
                {
                    item.RootTx = curves[i].curve;
                }
                if (curves[i].propertyName == "RootT.y")
                {
                    item.RootTy = curves[i].curve;
                }
                if (curves[i].propertyName == "RootT.z")
                {
                    item.RootTz = curves[i].curve;
                }
                if (curves[i].propertyName == "RootQ.x")
                {
                    item.RootQx = curves[i].curve;
                }
                if (curves[i].propertyName == "RootQ.y")
                {
                    item.RootQy = curves[i].curve;
                }
                if (curves[i].propertyName == "RootQ.z")
                {
                    item.RootQz = curves[i].curve;
                }
                if (curves[i].propertyName == "RootQ.w")
                {
                    item.RootQw = curves[i].curve;
                }
            }

            // check

            if (item.RootTx == null ||
                item.RootTy == null ||
                item.RootTz == null ||
                item.RootQx == null ||
                item.RootQy == null ||
                item.RootQz == null ||
                item.RootQw == null)
            {
                Debug.LogWarning(item.Name + "Curve is missing");
            }

            return item;
        }
    }
}
#endif