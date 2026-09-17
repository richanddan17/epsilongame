using UnityEditor;
using UnityEngine;

public static class PlayerSpriteImportFixer
{
    [MenuItem("Tools/Player/Fix Sprite Import")]
    public static void FixSpriteImport()
    {
        string[] paths = {
            "Assets/sprite/player/idle.png",
            "Assets/sprite/player/revision-walking.png",
            "Assets/sprite/player/jump.png"
        };
        foreach (var path in paths)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) { Debug.LogError("importer null: " + path); continue; }
            importer.spritePixelsPerUnit = 100f;
            if (path.EndsWith("jump.png"))
            {
                importer.spriteImportMode = SpriteImportMode.Multiple;
                var metas = importer.spritesheet;
                for (int i = 0; i < metas.Length; i++)
                {
                    metas[i].alignment = (int)SpriteAlignment.Center;
                    metas[i].pivot = new Vector2(0.5f, 0.5f);
                }
                importer.spritesheet = metas;
            }
            EditorUtility.SetDirty(importer);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Debug.Log("fixed: " + path);
        }
    }
}
