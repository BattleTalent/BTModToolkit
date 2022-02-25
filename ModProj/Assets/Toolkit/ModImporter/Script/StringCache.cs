using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossLink
{

    public class StringCache
    {
        static Dictionary<string, Dictionary<string, string>> cache = new Dictionary<string, Dictionary<string, string>>();

        static public string GetString(string main, string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
                return main;

            if (cache.ContainsKey(main) == false)
            {
                cache[main] = new Dictionary<string, string>();
            }

            var mainDict = cache[main];
            if (mainDict.ContainsKey(prefix) == false)
            {
                mainDict[prefix] = prefix + main;
            }

            return mainDict[prefix];
        }
    }

}