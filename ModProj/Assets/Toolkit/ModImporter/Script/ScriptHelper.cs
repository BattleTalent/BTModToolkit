using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{
    public class ScriptHelper
    {
        public static void RefleshScripts(GameObject go)
        {
            Dictionary<Component, Component> mcDic = new Dictionary<Component, Component>();

            ReplaceScripts(go, mcDic);

            foreach(var data in mcDic)
            {
                CopyReferenceVariable(data.Key, data.Value, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            }

            foreach (var data in mcDic)
            {
                Object.DestroyImmediate(data.Key);
            }
        }

        public static void ReplaceScripts(GameObject go, Dictionary<Component, Component> dic)
        {
            var monos = go.GetComponents<MonoBehaviour>();

            if (monos == null)
            {
                return;
            }

            foreach (var mono in monos)
            {
                var type = mono.GetType();
                //Debug.Log("-----------------class-----------------  " + type);
                var cp = go.AddComponent(type);

#if UNITY_EDITOR
                UnityEditorInternal.ComponentUtility.CopyComponent(mono);

                UnityEditorInternal.ComponentUtility.PasteComponentValues(cp);
#endif
                dic.Add(mono, cp);
            }



            Transform child;
            for(int i = 0; i < go.transform.childCount; i++)
            {
                child = go.transform.GetChild(i);
                ReplaceScripts(child.gameObject, dic);
            }
        }


        static void CopyReferenceVariable(object form, object to, System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Default)
        {
            var type = form.GetType();

            var fields = type.GetFields(flags);

            for (int i = 0; i < fields.Length; i++)
            {
                //Debug.Log("field Type:" + fields[i]);
                //base type is Monobehaviour
                if (typeof(MonoBehaviour).IsAssignableFrom(fields[i].FieldType))
                {
                    ResetVariable(to, fields[i]);
                }
                else if (fields[i].FieldType.IsArray)
                {
                    string typeName = fields[i].FieldType.FullName.Replace("[]", string.Empty);
                    if (typeof(MonoBehaviour).IsAssignableFrom(System.Type.GetType(typeName)))
                    {
                        MonoBehaviour[] mArr = (MonoBehaviour[])fields[i].GetValue(to);

                        for (int j = 0; j < mArr.Length; j++)
                        {
                            //ResetVariable(to, fields[i]);

                            if (mArr[j] != null)
                            {
                                var go = mArr[j].gameObject;

                                var c = go.GetComponents(System.Type.GetType(typeName));
                                if (c.Length > 2)
                                {
                                    Debug.LogError("Ambiguous variable:" + fields[i] + "at GameObject:" + go.name + "  need to set refence manually.");
                                }
                                else
                                {
                                    mArr[j] = (MonoBehaviour)c[c.Length - 1];
                                }
                                //Debug.Log("--------miss Field:" + info + " IsAssignableFrom MonoBehaviour:" + info.FieldType);
                            }
                        }

                        fields[i].SetValue(to, mArr);
                    }
                    else if (IsInjectionArray(fields[i].FieldType.FullName))
                    {
                        Injection[] iArr = (Injection[])fields[i].GetValue(to);

                        System.Type tp;
                        for (int j = 0; j < iArr.Length; j++)
                        {
                            tp = iArr[j].value.GetType();
                            if (typeof(MonoBehaviour).IsAssignableFrom(tp))
                            {
                                MonoBehaviour m = (MonoBehaviour)iArr[j].value;
                                var c = m.gameObject.GetComponents(tp);
                                if (c.Length > 2)
                                {
                                    Debug.LogError("Ambiguous variable:" + fields[i] + "at GameObject:" + m.gameObject.name + "  need to set refence manually.");
                                }
                                else
                                {
                                    iArr[j].value = (UnityEngine.Object)c[c.Length - 1];
                                }
                            }
                        }

                        fields[i].SetValue(to, iArr);
                    }
                }
                else if (fields[i].FieldType == typeof(LuaScript))
                {
                    CopyReferenceVariable(fields[i].GetValue(form), fields[i].GetValue(to), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                }
                
                //else if (fields[i].FieldType.IsGenericType) //list
                //{
                //    System.Type[] genericArgTypes = type.GetGenericArguments();
                //    if (genericArgTypes[0].IsAssignableFrom(fields[i].FieldType))
                //    {
                //        Debug.Log("--------miss Field:" + fields[i] + " IsAssignableFrom MonoBehaviour:" + fields[i].FieldType);
                //    }
                //}
            }
        }
        static void ResetVariable(object cp, System.Reflection.FieldInfo info)
        {
            MonoBehaviour value = (MonoBehaviour)info.GetValue(cp);

            if (!value)
                return;
            
            var c = value.gameObject.GetComponents(value.GetType());
            info.SetValue(cp, c[c.Length-1]);
        }

        static bool IsInjectionArray(string fullName)
        {
            return fullName.Replace("[]", string.Empty) == typeof(Injection).FullName;
        }
    }
}
