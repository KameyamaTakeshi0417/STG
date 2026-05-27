using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class CheckUIStructure
{
    [MenuItem("Tools/Check UI Structure")]
    public static void Check()
    {
        var objs = Selection.gameObjects;
        foreach (var obj in objs)
        {
            string res = "Structure of " + obj.name + "\n";
            res += GetStructure(obj.transform, 0);
            Debug.Log(res);
        }
    }

    static string GetStructure(Transform t, int depth)
    {
        string indent = new string(' ', depth * 2);
        string comps = "";
        foreach (var c in t.GetComponents<Component>())
        {
            if (c == null) continue;
            comps += c.GetType().Name + ", ";
        }
        string res = indent + "- " + t.name + " [" + comps + "]\n";
        foreach (Transform child in t)
        {
            res += GetStructure(child, depth + 1);
        }
        return res;
    }
}
