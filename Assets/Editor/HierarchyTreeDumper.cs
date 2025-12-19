#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class HierarchyTreeDumper
{
    [MenuItem("GameObject/Copy Hierarchy Tree", false, 49)]
    private static void CopyHierarchyTree()
    {
        var obj = Selection.activeGameObject;
        if (obj == null)
        {
            Debug.LogWarning("No object selected.");
            return;
        }

        string result = Dump(obj, 0);
        GUIUtility.systemCopyBuffer = result;

        Debug.Log("Hierarchy copied to clipboard.\n" + result);
    }

    private static string Dump(GameObject obj, int indent)
    {
        string prefix = new string('─', indent * 2);
        string line = $"{prefix}{obj.name}\n";

        foreach (Transform child in obj.transform)
        {
            line += Dump(child.gameObject, indent + 1);
        }
        return line;
    }
}
#endif