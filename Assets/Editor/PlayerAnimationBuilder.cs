using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Builds the player's sprite animation clips (idle / walk / jump) from the
/// sliced player sprite sheets at 12 fps with Loop wrap mode.
/// Menu: Tools > Player > Build Animations
/// </summary>
public static class PlayerAnimationBuilder
{
    private const float FrameRate = 12f;
    private const float FrameTime = 1f / FrameRate;

    [MenuItem("Tools/Player/Build Animations")]
    public static void BuildAnimations()
    {
        BuildClip("PlayerIdle", "Assets/sprite/player/idle.png", "idle_", 4);
        BuildClip("PlayerWalk", "Assets/sprite/player/revision-walking.png", "walk_", 8);
        BuildClip("PlayerJump", "Assets/sprite/player/jump.png", "jump_", 4);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[PlayerAnimBuilder] all clips created");
    }

    private static void BuildClip(string clipName, string spritePath, string spritePrefix, int frameCount)
    {
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spritePath)
            .OfType<Sprite>()
            .Where(s => s.name.StartsWith(spritePrefix))
            .OrderBy(s => int.Parse(s.name.Substring(spritePrefix.Length)))
            .Take(frameCount)
            .ToArray();

        if (sprites.Length != frameCount)
        {
            Debug.LogError("[PlayerAnimBuilder] expected " + frameCount + " sprites with prefix '" +
                           spritePrefix + "' in " + spritePath + ", found " + sprites.Length);
            return;
        }

        var clip = new AnimationClip { frameRate = FrameRate };

        var keyframes = new ObjectReferenceKeyframe[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i * FrameTime,
                value = sprites[i]
            };
        }

        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        string path = "Assets/Animations/" + clipName + ".anim";
        AssetDatabase.CreateAsset(clip, path);
        Debug.Log("[PlayerAnimBuilder] created: " + path + " (" + frameCount + " frames @ " + FrameRate + "fps)");
    }
}