using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
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

    [MenuItem("Tools/Player/Build Animator Controller")]
    public static void BuildPlayerAnimatorController()
    {
        const string controllerPath = "Assets/Animations/PlayerAnimator.controller";

        if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath) != null)
        {
            AssetDatabase.DeleteAsset(controllerPath);
        }

        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);

        var sm = controller.layers[0].stateMachine;

        var idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/PlayerIdle.anim");
        var walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/PlayerWalk.anim");
        var jumpClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/PlayerJump.anim");

        if (idleClip == null || walkClip == null || jumpClip == null)
        {
            Debug.LogError("[PlayerAnimBuilder] missing animation clips. Run 'Tools/Player/Build Animations' first.");
            return;
        }

        var idle = sm.AddState("Idle");
        idle.motion = idleClip;

        var walk = sm.AddState("Walk");
        walk.motion = walkClip;

        var jump = sm.AddState("Jump");
        jump.motion = jumpClip;

        sm.defaultState = idle;

        float dur = 0.1f;

        var t = idle.AddTransition(walk);
        t.duration = dur;
        t.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        t = walk.AddTransition(idle);
        t.duration = dur;
        t.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

        t = idle.AddTransition(jump);
        t.duration = dur;
        t.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");

        t = walk.AddTransition(jump);
        t.duration = dur;
        t.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");

        t = jump.AddTransition(idle);
        t.duration = dur;
        t.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
        t.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

        t = jump.AddTransition(walk);
        t.duration = dur;
        t.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
        t.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        AssetDatabase.SaveAssets();
        Debug.Log("[PlayerAnimBuilder] animator controller created: " + controllerPath);
    }

    [MenuItem("Tools/Player/Apply Prefab")]
    public static void ApplyPrefab()
    {
        const string prefabPath = "Assets/Prefabs/Player.prefab";
        const string idlePath = "Assets/sprite/player/idle.png";
        const string controllerPath = "Assets/Animations/PlayerAnimator.controller";

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError("[PlayerAnimBuilder] prefab not found: " + prefabPath);
            return;
        }

        var sr = prefab.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("[PlayerAnimBuilder] SpriteRenderer missing on prefab root: " + prefabPath);
            return;
        }

        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(idlePath).OfType<Sprite>().ToArray();
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("[PlayerAnimBuilder] no sprites found in " + idlePath);
            return;
        }

        sr.sprite = sprites[0];
        sr.color = Color.white;

        var anim = prefab.GetComponent<Animator>();
        if (anim == null)
        {
            anim = prefab.AddComponent<Animator>();
        }
        anim.runtimeAnimatorController =
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);

        PrefabUtility.SavePrefabAsset(prefab);
        AssetDatabase.SaveAssets();

        Debug.Log("[PlayerAnimBuilder] prefab applied: sprite=" + sr.sprite.name +
                  " color=white animator=" + (anim.runtimeAnimatorController != null));
    }
}