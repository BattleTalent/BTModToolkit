using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CrossLink
{

    public class ReSkinHelper : MonoBehaviour
    {
        public enum PropState { Drop, Equipped}
        protected SkinnedMeshRenderer[] LODS;
        protected string[][] LODBones;
        private LODGroup CurrentLODGroup;
        protected PropState propState;

#if true//UNITY_EDITOR
        public List<Transform> excludeList;

#if UNITY_EDITOR
        [EasyButtons.Button]
#endif
        public void PasteBonesTransformTo(Transform from, Transform to)
        {
            var selfTranList = from.gameObject.GetComponentsInChildren<Transform>();
            var targetTranList = to.gameObject.GetComponentsInChildren<Transform>();
            for (int i = 0; i < selfTranList.Length; ++i)
            {
                if (excludeList.Contains(selfTranList[i]))
                    continue;
#if false
                targetTranList[i].localPosition = selfTranList[i].localPosition;
                targetTranList[i].localRotation = selfTranList[i].localRotation;
                targetTranList[i].localScale = selfTranList[i].localScale;
#endif
                bool paste = false;
                for (int j = 0; j < targetTranList.Length; ++j)
                {
                    if (targetTranList[j].name == selfTranList[i].name)
                    {
                        targetTranList[j].localPosition = selfTranList[i].localPosition;
                        targetTranList[j].localRotation = selfTranList[i].localRotation;
                        targetTranList[j].localScale = selfTranList[i].localScale;
                        paste = true;
                        break;
                    }
                }
#if UNITY_EDITOR
                if (paste == false)
                {
                    Debug.Log(selfTranList[i].name + " paste target not found");
                }
#endif
            }
        }

#if UNITY_EDITOR
        [EasyButtons.Button]
#endif
        public void ReskinToBones(Transform target)
        {
            InitBoneRef();
            ReplaceBoneRef(target.name);
            ReplaceLODSToLODGroup(GetComponentInParent<LODGroup>());
        }

#if UNITY_EDITOR
        static List<Material> differentMatList = new List<Material>();
        [MenuItem("Tools/Polycount %#m")]
        static void ShowPolycount()
        {
            int totalTriCount = 0;
            int totalVertCount = 0;
            int totalObjCount = 0;
            differentMatList.Clear();

            var gos = Selection.gameObjects;
            for (int i = 0; i < gos.Length; ++i)
            {
                int tc, tv, oc;
                CalcPolycount(gos[i], out tc, out tv, out oc);
                totalTriCount += tc;
                totalVertCount += tv;
                totalObjCount += oc;
                Debug.Log(gos[i].name + " triangles: " + tc + " vertices:" + tv + " objects:" + oc);
            }


            if (differentMatList.Count > 0)
            {
                var matStr = "Mat List: ";
                for (int i = 0; i < differentMatList.Count; ++i)
                {
                    matStr += differentMatList[i].name + "; ";
                }
                Debug.Log(matStr);                
            }

            Debug.Log("GameObject Count:" + gos.Length + " [triangles]: " + totalTriCount + " [vertices]:" + totalVertCount + " [objects]:" + totalObjCount + " [mats]:" + differentMatList.Count + " [time]:" + System.DateTime.Now);


            differentMatList.Clear();


        }



        static void CalcPolycount(GameObject go, out int totalTriCount, out int totalVertCount, out int objCount)
        {
            totalTriCount = 0;
            totalVertCount = 0;
            objCount = 0;
            //Get count of all children of object
            Transform[] allChildren = go.GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildren)
            {
                if (child.gameObject.GetComponent<MeshFilter>())
                {
                    Mesh objMesh = child.gameObject.GetComponent<MeshFilter>().sharedMesh;
                    if (objMesh == null)
                    {
#if UNITY_EDITOR
                        Debug.LogError("empty mesh:" + child.gameObject.name);
#endif
                        continue;
                    }
#if UNITY_EDITOR                        
                    int triCount = objMesh.triangles.Length / 3;
                    totalTriCount += triCount;                    
                    totalVertCount += objMesh.vertices.Length;
                    ++objCount;
                    Debug.Log("Mesh:" + objMesh.name + " [triangles]: " + triCount + " [vertices]:" + objMesh.vertices.Length);
#endif
                }
                else if (child.gameObject.GetComponent<SkinnedMeshRenderer>())
                {
                    Mesh objMesh2 = child.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
                    if (objMesh2 == null)
                    {
#if UNITY_EDITOR
                        Debug.LogError("empty mesh:" + child.gameObject.name);
#endif
                        continue;
                    }
#if UNITY_EDITOR
                    int triCount2 = objMesh2.triangles.Length / 3;
                    totalTriCount += triCount2;
                    totalVertCount += objMesh2.vertices.Length;
                    ++objCount;
                    Debug.Log("SkinnedMesh:" + objMesh2.name + " [triangles]: " + triCount2 + " [vertices]:" + objMesh2.vertices.Length);
#endif
                }

                var rnd = child.gameObject.GetComponent<Renderer>();
                if (rnd != null)
                {
                    foreach(var mat in rnd.sharedMaterials)
                    {
                        if (mat == null
                            //|| differentMatList.Contains(mat)
                            )
                            continue;

                        bool unique = true;
                        for (int i=0; i<differentMatList.Count; ++i)
                        {
                            if (differentMatList[i].name == mat.name)
                            {
                                unique = false;
                                break;
                            }
                        }
                        if (unique == false)
                            continue;

                        differentMatList.Add(mat);
                    }
                }
            }
        }
#endif

#endif


#if true//UNITY_EDITOR
        // remove all the root bones in each lod group
        protected void InitBoneRef()
        {
            var lods = GetComponentsInChildren(typeof(SkinnedMeshRenderer), true);
            LODS = new SkinnedMeshRenderer[lods.Length];
            for (int i = 0; i < LODS.Length; i++) LODS[i] = (SkinnedMeshRenderer)lods[i];

            LODBones = new string[LODS.Length][];
            for (int i = 0; i < LODS.Length; i++)
            {
                List<string> Names = new List<string>();
                foreach (var bone in LODS[i].bones) Names.Add(bone.name);
                LODBones[i] = Names.ToArray();
            }
            var rootBone = LODS[0].rootBone;
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (rootBone.IsChildOf(child))
                {
#if UNITY_EDITOR
                    if (Application.isPlaying == false)
                    {
                        DestroyImmediate(child.gameObject);
                    }
                    else
#endif
                    {
                        Destroy(child.gameObject);
                    }

                    break;
                }
            }
        }



        // bind to new bones for each lod group
        // then joint parent's lod group(skip if don't has lod setup)
        protected void ReplaceBoneRef(string rootName = "Root")
        {
            transform.localScale = Vector3.one;            
            {
                //1. Bind to skeleton(the target)
                Transform[] SkeletonBones = transform.parent.Find(rootName).GetComponentsInChildren<Transform>();
                //// for each mesh we find the corresponding bones
                for (int i = 0; i < LODS.Length; i++)
                {
                    List<Transform> bones = new List<Transform>();
                    foreach (var boneName in LODBones[i])
                    {
                        try
                        {
                            bones.Add(SkeletonBones.First(b => b.name.Equals(boneName)));
                        }
                        catch { print(LODS[i].name + ":  " + boneName + " is not found!"); }
                    }
                    LODS[i].rootBone = bones[0];
                    LODS[i].bones = bones.ToArray();

                }                
            }
        }
        private void ReplaceLODSToLODGroup(LODGroup group)
        {
            if (LODS == null || LODS.Length < 2) return;
            if (CurrentLODGroup == group) return;
            //1. Remove LODs from old LOD group
            if (CurrentLODGroup)
            {
                var ParentLODS = CurrentLODGroup.GetLODs();
                for (int i = 0; i < ParentLODS.Length; i++)
                {
                    var LODList = ParentLODS[i].renderers.ToList();
                    foreach (var LOD in LODS.Where(lod => lod.name[lod.name.Length - 1] == i.ToString()[0])) LODList.Remove(LOD);
                    ParentLODS[i].renderers = LODList.ToArray();
                }
                CurrentLODGroup.SetLODs(ParentLODS);
            }
            //2. Attach LODs to new LOD group
            if (group)
            {
                var ParentLODS = group.GetLODs();
                for (int i = 0; i < ParentLODS.Length; i++)
                {
                    var LODList = ParentLODS[i].renderers.ToList();
                    foreach (var LOD in LODS.Where(lod => lod.name[lod.name.Length - 1] == i.ToString()[0])) LODList.Add(LOD);
                    ParentLODS[i].renderers = LODList.ToArray();
                }
                group.SetLODs(ParentLODS);
            }
            CurrentLODGroup = group;
        }
#endif


    }

}
