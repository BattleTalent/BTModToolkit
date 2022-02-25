using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CrossLink
{
    /// <summary>
    /// Used to load resources
    /// addressable path also works here
    /// </summary>
    /// <example>
    /// addressables path: Effect/abc
    /// then you can load it by ResourceMgr.Load("Effect/abc")
    /// </example>
    public class ResourceMgr : CoreMgr
    {
        static Dictionary<string, Object> loadedObj = new Dictionary<string, Object>();


        static public void ClearCache()
        {
            loadedObj.Clear();
        }

        static public void RemoveCache(string path)
        {
            if (loadedObj.ContainsKey(path))
            {
                loadedObj.Remove(path);
            }
        }

        static public void Preload(string path)
        {
            if (!loadedObj.ContainsKey(path))
            {
                loadedObj[path] = Resources.Load(path);
            }
        }

        static public bool IsLoaded(string path)
        {
            return loadedObj.ContainsKey(path);
        }


        //static Dictionary<string, Object> injectedRes = new Dictionary<string, Object>();

        static public void InjectRes(Dictionary<string, Object> res)
        {
            var ite = res.GetEnumerator();
            while(ite.MoveNext())
            {
                if (loadedObj.ContainsKey(ite.Current.Key))
                {
                    //Debug.Log("Replace Res:" + ite.Current.Key);
                }
                loadedObj[ite.Current.Key] = ite.Current.Value;
            }
        }

        // only affects
        // script/character/effect
        static public Object Load(string prefix, string file, bool cache = true)
        {
            if (cache && IsLoaded(file))
            {
                return loadedObj[file];
            }
            else
            {
                
                var obj = Load(StringCache.GetString(file, prefix), cache);
                // avoid resource conflict, so don't mess up with all the short names
                // after the short res loaded, it'll just work
#if false
                if (cache && obj != null)
                {
                    loadedObj[file] = obj;
                }
#endif
#if UNITY_EDITOR
                if (obj == null)
                {
                    Debug.LogError("Trying to Load a null obj:" + StringCache.GetString(file, prefix));
                }
#endif

                return obj;
            }
        }
                                                        //LuaScript / WMD_FlySpellBaseScript

        static public Object Load(string path, bool cache = true)
        {
#if UNITY_EDITOR
            if (cache == false || Application.isPlaying == false)
#else
            if (cache == false)
#endif
            {
                return Resources.Load(path);
            }
            else if (!loadedObj.ContainsKey(path))
            {
                loadedObj[path] = Resources.Load(path);
            }

            return loadedObj[path];
        }

        static public void LoadAsync(string path, System.Action<Object> cb, bool cache = true)
        {
            if (loadedObj.ContainsKey(path))
            {
                if (cb != null)
                {
                    cb(loadedObj[path]);
                }
            }
            else
            {
                var asyncRequest = Resources.LoadAsync(path);
                asyncRequest.completed += (ao) => {
                    if (ao.isDone)
                    {
                        var ass = asyncRequest.asset;
                        if (cache)
                            loadedObj[path] = ass;

                        if (cb != null)
                        {
                            cb(ass);
                        }
                    }
#if UNITY_EDITOR
                    else
                    {
                        Debug.Log("NO RES:" + path);
                    }
#endif
                };
            }
        }

        static public Object Instantiate(Object prefab)
        {
            return Object.Instantiate(prefab);
        }

        static public Object Instantiate(Object prefab, Vector3 pos, Quaternion rot)
        {
            return Object.Instantiate(prefab, pos, rot);
        }

        static public Object Instantiate(string path, bool cache = true)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var ld = Load(path, cache);
            if (ld == null)
            {
#if UNITY_EDITOR
                Debug.LogError("Not Exist:" + path);
#endif
                return null;
            }
            return Object.Instantiate(ld);
        }

        static public Object Instantiate(string path, Vector3 pos, Quaternion rot, bool cache = true)
        {
            var ld = Load(path, cache);
            if (ld == null)
            {
#if UNITY_EDITOR
                Debug.LogError("Not Exist:" + path);
#endif
                return null;
            }
            return Object.Instantiate(ld, pos, rot);
        }

        static public void Destroy(Object o, float t = 0)
        {
            if (t == 0)
            {
                Object.Destroy(o);
            }
            else
            {
                Object.Destroy(o, t);
            }
        }
    }

}