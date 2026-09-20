using System.Text;
using UnityEditor;
using UnityEngine;

public static class CleanupMainScene
{
    public static void Main()
    {
        var log = new StringBuilder();
        int removedComponents = 0;
        int deletedObjects = 0;

        var player = GameObject.Find("Player");
        if (player != null)
        {
            System.Func<int> countMissing = () => System.Array.FindAll(player.GetComponents<Component>(), c => c == null).Length;
            int missingBefore = countMissing();
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(player);
            int removedMissing = missingBefore - countMissing();
            if (removedMissing > 0)
            {
                log.AppendLine("Removed MISSING SCRIPT on /Player x" + removedMissing);
                removedComponents += removedMissing;
            }

            var added = PrefabUtility.GetAddedComponents(player);
            foreach (var a in added)
            {
                var ic = a.instanceComponent;
                if (ic == null) continue;
                string tn = ic.GetType().Name;
                if (tn == "PlayerCombat" || tn == "PlayerHealth")
                {
                    Undo.DestroyObjectImmediate(ic);
                    log.AppendLine("Removed scene-added duplicate: " + tn);
                    removedComponents++;
                }
            }
        }
        else log.AppendLine("WARN: /Player not found");

        int vcamTarget = -1;
        var all = Object.FindObjectsOfType<GameObject>();
        foreach (var go in all)
        {
            if (go.name != "CM vcam1") continue;
            bool hasCam = false;
            foreach (var c in go.GetComponents<Component>())
                if (c != null && c.GetType().Name == "CinemachineCamera") { hasCam = true; break; }
            if (hasCam) vcamTarget = go.GetInstanceID();
        }
        foreach (var go in all)
        {
            if (go.name != "CM vcam1") continue;
            if (go.GetInstanceID() == vcamTarget) continue;
            string goneName = go.name;
            Undo.DestroyObjectImmediate(go);
            log.AppendLine("Deleted empty duplicate: " + goneName);
            deletedObjects++;
        }
        if (vcamTarget >= 0)
        {
            var real = EditorUtility.InstanceIDToObject(vcamTarget) as GameObject;
            if (real != null)
            {
                int combatCamCount = 0;
                foreach (var comp in real.GetComponents<Component>())
                {
                    if (comp != null && comp.GetType().Name == "CombatCamera")
                    {
                        combatCamCount++;
                        if (combatCamCount > 1)
                        {
                            Undo.DestroyObjectImmediate(comp);
                            log.AppendLine("Removed duplicate CombatCamera on " + real.name);
                            removedComponents++;
                        }
                    }
                }
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        log.AppendLine("DONE removed=" + removedComponents + " deletedObjects=" + deletedObjects);
        Debug.Log(log.ToString());
    }
}