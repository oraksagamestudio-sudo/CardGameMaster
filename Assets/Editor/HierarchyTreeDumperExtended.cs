#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Reflection;

public class HierarchyTreeDumperExtended
{
    [MenuItem("GameObject/Copy Hierarchy + Components", false, 50)]
    private static void CopyHierarchyTreeWithComponents()
    {
        var obj = Selection.activeGameObject;
        if (obj == null)
        {
            Debug.LogWarning("No object selected.");
            return;
        }

        StringBuilder sb = new StringBuilder();
        DumpObject(obj, 0, sb);

        GUIUtility.systemCopyBuffer = sb.ToString();
        Debug.Log("Hierarchy + components copied.\n" + sb.ToString());
    }

    private static void DumpObject(GameObject obj, int indent, StringBuilder sb)
    {
        string prefix = new string('─', indent * 2);
        sb.AppendLine($"{prefix}{obj.name}");

        // 1) 컴포넌트 덤프 (Transform 제외)
        Component[] components = obj.GetComponents<Component>();
        foreach (var comp in components)
        {
            if (comp == null) continue;

            string typeName = comp.GetType().Name;

            // 공통 컴포넌트는 제외
            if (IsIgnoredComponent(typeName))
                continue;

            sb.AppendLine($"{prefix}  • {typeName}");

            // 실제 Inspector에 표시되는 필드들만 출력
            DumpInspectorFields(comp, indent + 2, sb);
        }

        // 2) 자식들 재귀
        foreach (Transform child in obj.transform)
        {
            DumpObject(child.gameObject, indent + 1, sb);
        }
    }

    // 제외할 기본 컴포넌트 리스트
    private static bool IsIgnoredComponent(string typeName)
    {
        string[] ignore =
        {
            "CanvasRenderer",
            "GraphicRaycaster",
            "RectTransform",
            "Image",
            "TextMeshProUGUI",
            "Animator"
        };

        foreach (string s in ignore)
        {
            if (typeName == s)
                return true;
        }

        return false;
    }

    // 인스펙터에서 표시되는 필드만 가져오기
    private static void DumpInspectorFields(Component component, int indent, StringBuilder sb)
    {
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var fields = component.GetType().GetFields(flags);

        string prefix = new string('─', indent * 2);

        foreach (var field in fields)
        {
            // HideInInspector는 제외
            if (field.GetCustomAttribute<HideInInspector>() != null)
                continue;

            // Unity Inspector에 노출되는 조건:
            // - public
            // - 또는 [SerializeField]
            bool visible =
                field.IsPublic ||
                field.GetCustomAttribute<SerializeField>() != null;

            if (!visible)
                continue;

            object value = field.GetValue(component);
            sb.AppendLine($"{prefix}- {field.Name}: {value}");
        }
    }
}
#endif