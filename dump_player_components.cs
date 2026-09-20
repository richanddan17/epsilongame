using System.Text;
using UnityEngine;

public static class DumpPlayerComponents
{
    public static void Main()
    {
        var go = GameObject.Find("Player");
        var sb = new StringBuilder();
        if (go == null)
        {
            sb.Append("[Dump] Player not found");
            Debug.Log(sb.ToString());
            return;
        }
        sb.Append("GO=").Append(go.name).Append(" prefab=")
          .Append(UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(go) != null ? UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(go).name : "none")
          .Append("\n");
        var comps = go.GetComponents<Component>();
        for (int i = 0; i < comps.Length; i++)
        {
            var c = comps[i];
            if (c == null)
            {
                sb.Append("[").Append(i).Append("] MISSING SCRIPT (null component)\n");
                continue;
            }
            var behaviour = c as Behaviour;
            sb.Append("[").Append(i).Append("] ").Append(c.GetType().Name)
              .Append(" id=").Append(c.GetInstanceID())
              .Append(" enabled=").Append(behaviour != null ? behaviour.enabled.ToString() : "-")
              .Append("\n");
        }
        Debug.Log(sb.ToString());
    }
}