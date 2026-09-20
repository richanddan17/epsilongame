using System.Text;
using UnityEditor;
using UnityEngine;

public static class InspectPlayerPrefab
{
    public static void Main()
    {
        var sb = new StringBuilder();

        // 1. 프리팹 에셋 로드 및 컴포넌트 검사
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        if (prefab == null)
        {
            var guids = AssetDatabase.FindAssets("t:Prefab Player");
            foreach (var g in guids)
                sb.Append("found prefab: ").Append(AssetDatabase.GUIDToAssetPath(g)).Append("\n");
            if (guids.Length == 0) sb.Append("Player prefab not found by name search\n");
        }
        else
        {
            sb.Append("Prefab asset: Assets/Prefabs/Player.prefab\n");
            DumpComponents(sb, prefab, "PREFAB_SOURCE");
        }

        // 2. 씬 인스턴스
        var inst = GameObject.Find("Player");
        if (inst != null)
        {
            DumpComponents(sb, inst, "SCENE_INSTANCE");

            if (PrefabUtility.GetCorrespondingObjectFromSource(inst) != null)
            {
                var added = PrefabUtility.GetAddedComponents(inst);
                sb.Append("--- scene ADDED components (").Append(added.Count).Append(") ---\n");
                foreach (var a in added)
                    sb.Append("  added: ").Append(a.instanceComponent == null ? "MISSING SCRIPT" : a.instanceComponent.GetType().Name).Append("\n");
            }
        }
        else sb.Append("Scene Player not found\n");

        var vcams = GameObject.FindObjectsOfType<Transform>();
        sb.Append("\n--- CM vcam objects ---\n");
        foreach (var t in vcams)
            if (t.name.Contains("vcam") || t.name.Contains("Vcam"))
            {
                int cinemachineCams = 0;
                int combatCams = 0;
                string compSummary = "";
                foreach (var c in t.GetComponents<Component>())
                {
                    if (c == null) { compSummary += " MISSING"; continue; }
                    compSummary += " " + c.GetType().Name;
                    if (c.GetType().Name.Contains("CinemachineCamera")) cinemachineCams++;
                    if (c.GetType().Name.Contains("CombatCamera")) combatCams++;
                }
                sb.Append("name=").Append(t.name).Append(" parent=").Append(t.parent != null ? t.parent.name : "root")
                  .Append(" cinemachineCameras=").Append(cinemachineCams)
                  .Append(" combatCams=").Append(combatCams)
                  .Append(" components:").Append(compSummary)
                  .Append("\n");
            }

        Debug.Log(sb.ToString());
    }

    private static void DumpComponents(StringBuilder sb, GameObject go, string label)
    {
        sb.Append("--- ").Append(label).Append(": ").Append(go.name).Append(" ---\n");
        var comps = go.GetComponents<Component>();
        for (int i = 0; i < comps.Length; i++)
        {
            var c = comps[i];
            if (c == null) { sb.Append("[").Append(i).Append("] MISSING SCRIPT\n"); continue; }
            sb.Append("[").Append(i).Append("] ").Append(c.GetType().Name)
              .Append(" id=").Append(c.GetInstanceID()).Append("\n");
        }
    }
}