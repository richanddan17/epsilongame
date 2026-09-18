using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

/// <summary>
/// player-sprite-animation v2 — builds all player animation clips + animator controller
/// from Assets/sprite/player/player/ ONLY (single source folder).
/// Menu: Tools/Player/v2/Build All
/// </summary>
public static class PlayerAnimationBuilder
{
    private const string SpriteRoot = "Assets/sprite/player/player/";
    private const string AnimDir = "Assets/Animations/";
    private const string ControllerPath = AnimDir + "PlayerAnimator.controller";
    private const string PrefabPath = "Assets/Prefabs/Player.prefab";
    private const float Ppu = 100f;
    private const float TransitionDuration = 0.05f;

    private static readonly string[] FrameFolders =
    {
        "attack",
        "combo_attack",
        "jumpanddash/jump",
        "jumpanddash/dash",
        "jumpanddash/doublejump",
        "jumpanddash/fall"
    };

    [MenuItem("Tools/Player/v2/Build All")]
    public static void BuildAll()
    {
        ConfigureFrameImports();
        SliceSheet("idle/sprite sheets/idle.png", "idle_", 10, 1, 46, 55);
        SliceSheet("walk/sprite sheets/walk.png", "walk_", 4, 6, 45, 58);
        AssetDatabase.Refresh();

        BuildClips();
        BuildController();
        ApplyPrefab();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[PlayerAnimBuilderV2] Build All complete.");
    }

    [MenuItem("Tools/Player/v2/Configure Frame Imports")]
    public static void ConfigureFrameImports()
    {
        AssetDatabase.StartAssetEditing();
        int count = 0;
        try
        {
            foreach (var folder in FrameFolders)
            {
                foreach (var kv in FrameFiles(folder))
                {
                    var imp = AssetImporter.GetAtPath(kv.Value) as TextureImporter;
                    if (imp == null) continue;

                    var s = new TextureImporterSettings();
                    imp.ReadTextureSettings(s);
                    s.textureType = TextureImporterType.Sprite;
                    s.spriteMode = (int)SpriteImportMode.Single;
                    s.spritePixelsPerUnit = Ppu;
                    s.spriteAlignment = (int)SpriteAlignment.Center;
                    s.spritePivot = new Vector2(0.5f, 0.5f);
                    imp.SetTextureSettings(s);
                    EditorUtility.SetDirty(imp);
                    count++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        AssetDatabase.Refresh();
        Debug.Log("[PlayerAnimBuilderV2] configured " + count + " frame PNGs (Single / PPU " + Ppu + " / center pivot)");
    }

    [MenuItem("Tools/Player/v2/Slice Sheets")]
    public static void SliceSheets()
    {
        SliceSheet("idle/sprite sheets/idle.png", "idle_", 10, 1, 46, 55);
        SliceSheet("walk/sprite sheets/walk.png", "walk_", 4, 6, 45, 58);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Player/v2/Build Clips")]
    public static void BuildClips()
    {
        BuildClip("PlayerIdle", LoadSheetSprites(SpriteRoot + "idle/sprite sheets/idle.png", "idle_"), 0.06f, true);
        BuildClip("PlayerWalk", LoadSheetSprites(SpriteRoot + "walk/sprite sheets/walk.png", "walk_"), 0.05f, true);
        BuildClip("PlayerJump", LoadFrameSprites("jumpanddash/jump", 9, 16), 0.07f, false);
        BuildClip("PlayerDoubleJump", LoadFrameSprites("jumpanddash/doublejump", 25, 31), 0.07f, false);
        BuildClip("PlayerFall", LoadFrameSprites("jumpanddash/fall", 31, 39), 0.07f, false);
        BuildClip("PlayerAttack", LoadFrameSprites("attack", 0, 73), 0.06f, false);
        BuildClip("PlayerComboAttack", LoadFrameSprites("combo_attack", 0, 103), 0.06f, false);

        // Dash: explicit replay order — 09,10,11 -> 14..25 -> 11,10,09 (18 keyframes, 15 files reused)
        int[] dashOrder = { 9, 10, 11, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 11, 10, 9 };
        var dashSprites = dashOrder.Select(n => LoadFrameSprite("jumpanddash/dash", n)).ToList();
        BuildClip("PlayerDash", dashSprites, 0.07f, false);
    }

    [MenuItem("Tools/Player/v2/Build Animator Controller")]
    public static void BuildController()
    {
        if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath) != null)
            AssetDatabase.DeleteAsset(ControllerPath);

        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsFalling", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsDashing", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsAttacking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsComboAttacking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("JumpCount", AnimatorControllerParameterType.Int);

        var sm = controller.layers[0].stateMachine;

        var idle = AddState(sm, "Idle", "PlayerIdle");
        var walk = AddState(sm, "Walk", "PlayerWalk");
        var jump = AddState(sm, "Jump", "PlayerJump");
        var dbl = AddState(sm, "DoubleJump", "PlayerDoubleJump");
        var fall = AddState(sm, "Fall", "PlayerFall");
        var dash = AddState(sm, "Dash", "PlayerDash");
        var atk = AddState(sm, "Attack", "PlayerAttack");
        var combo = AddState(sm, "ComboAttack", "PlayerComboAttack");
        sm.defaultState = idle;

        // Attack / Combo override everything (no self-retrigger; explodes to Idle when flag clears).
        AddAny(sm, atk, Cond(AnimatorConditionMode.If, 0f, "IsAttacking"));
        AddAny(sm, combo, Cond(AnimatorConditionMode.If, 0f, "IsComboAttacking"));

        AddT(atk, idle, Cond(AnimatorConditionMode.IfNot, 0f, "IsAttacking"));
        AddT(combo, idle, Cond(AnimatorConditionMode.IfNot, 0f, "IsComboAttacking"));

        // Ground movement
        AddT(idle, walk,
            Cond(AnimatorConditionMode.Greater, 0.1f, "Speed"),
            Cond(AnimatorConditionMode.IfNot, 0f, "IsAttacking"),
            Cond(AnimatorConditionMode.IfNot, 0f, "IsComboAttacking"));
        AddT(walk, idle,
            Cond(AnimatorConditionMode.Less, 0.1f, "Speed"),
            Cond(AnimatorConditionMode.IfNot, 0f, "IsAttacking"),
            Cond(AnimatorConditionMode.IfNot, 0f, "IsComboAttacking"));

        // Take-off (first jump) / 2nd jump / fall from ground
        AddT(idle, jump,
            Cond(AnimatorConditionMode.IfNot, 0f, "IsGrounded"),
            Cond(AnimatorConditionMode.IfNot, 0f, "IsFalling"),
            Cond(AnimatorConditionMode.Equals, 1f, "JumpCount"),
            Cond(AnimatorConditionMode.IfNot, 0f, "IsDashing"));
        AddT(walk, jump,
            Cond(AnimatorConditionMode.IfNot, 0f, "IsGrounded"),
            Cond(AnimatorConditionMode.IfNot, 0f, "IsFalling"),
            Cond(AnimatorConditionMode.Equals, 1f, "JumpCount"),
            Cond(AnimatorConditionMode.IfNot, 0f, "IsDashing"));
        AddT(idle, dbl,
            Cond(AnimatorConditionMode.IfNot, 0f, "IsGrounded"),
            Cond(AnimatorConditionMode.Equals, 2f, "JumpCount"),
            Cond(AnimatorConditionMode.IfNot, 0f, "IsDashing"));
        AddT(walk, dbl,
            Cond(AnimatorConditionMode.IfNot, 0f, "IsGrounded"),
            Cond(AnimatorConditionMode.Equals, 2f, "JumpCount"),
            Cond(AnimatorConditionMode.IfNot, 0f, "IsDashing"));
        AddT(idle, fall, Cond(AnimatorConditionMode.IfNot, 0f, "IsGrounded"), Cond(AnimatorConditionMode.If, 0f, "IsFalling"));
        AddT(walk, fall, Cond(AnimatorConditionMode.IfNot, 0f, "IsGrounded"), Cond(AnimatorConditionMode.If, 0f, "IsFalling"));

        // Air transitions
        AddT(jump, fall, Cond(AnimatorConditionMode.If, 0f, "IsFalling"));
        AddT(jump, dbl, Cond(AnimatorConditionMode.Equals, 2f, "JumpCount"));
        AddT(dbl, fall, Cond(AnimatorConditionMode.If, 0f, "IsFalling"));

        AddT(jump, idle, Cond(AnimatorConditionMode.If, 0f, "IsGrounded"), Cond(AnimatorConditionMode.Less, 0.1f, "Speed"));
        AddT(jump, walk, Cond(AnimatorConditionMode.If, 0f, "IsGrounded"), Cond(AnimatorConditionMode.Greater, 0.1f, "Speed"));
        AddT(dbl, idle, Cond(AnimatorConditionMode.If, 0f, "IsGrounded"), Cond(AnimatorConditionMode.Less, 0.1f, "Speed"));
        AddT(dbl, walk, Cond(AnimatorConditionMode.If, 0f, "IsGrounded"), Cond(AnimatorConditionMode.Greater, 0.1f, "Speed"));
        AddT(fall, idle, Cond(AnimatorConditionMode.If, 0f, "IsGrounded"), Cond(AnimatorConditionMode.Less, 0.1f, "Speed"));
        AddT(fall, walk, Cond(AnimatorConditionMode.If, 0f, "IsGrounded"), Cond(AnimatorConditionMode.Greater, 0.1f, "Speed"));

        // Dash (param stays false until dash gameplay exists)
        AddAny(sm, dash, Cond(AnimatorConditionMode.If, 0f, "IsDashing"));
        AddT(dash, idle, Cond(AnimatorConditionMode.IfNot, 0f, "IsDashing"), Cond(AnimatorConditionMode.If, 0f, "IsGrounded"), Cond(AnimatorConditionMode.Less, 0.1f, "Speed"));
        AddT(dash, walk, Cond(AnimatorConditionMode.IfNot, 0f, "IsDashing"), Cond(AnimatorConditionMode.If, 0f, "IsGrounded"), Cond(AnimatorConditionMode.Greater, 0.1f, "Speed"));
        AddT(dash, fall, Cond(AnimatorConditionMode.IfNot, 0f, "IsDashing"), Cond(AnimatorConditionMode.IfNot, 0f, "IsGrounded"), Cond(AnimatorConditionMode.If, 0f, "IsFalling"));

        AssetDatabase.SaveAssets();
        Debug.Log("[PlayerAnimBuilderV2] controller rebuilt: 7 params / 8 states / " + ControllerPath);
    }

    [MenuItem("Tools/Player/v2/Apply Prefab")]
    public static void ApplyPrefab()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError("[PlayerAnimBuilderV2] prefab not found: " + PrefabPath);
            return;
        }

        var sr = prefab.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("[PlayerAnimBuilderV2] SpriteRenderer missing on prefab root");
            return;
        }

        var idleSprites = LoadSheetSprites(SpriteRoot + "idle/sprite sheets/idle.png", "idle_");
        if (idleSprites.Count == 0)
        {
            Debug.LogError("[PlayerAnimBuilderV2] no idle sprites (slice sheet first)");
            return;
        }

        sr.sprite = idleSprites[0];
        sr.color = Color.white;

        var anim = prefab.GetComponent<Animator>();
        if (anim == null) anim = prefab.AddComponent<Animator>();
        anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);

        PrefabUtility.SavePrefabAsset(prefab);
        AssetDatabase.SaveAssets();
        Debug.Log("[PlayerAnimBuilderV2] prefab applied: sprite=" + sr.sprite.name + " animator=" + (anim.runtimeAnimatorController != null));
    }

    // ---------- helpers ----------

    private static AnimatorState AddState(AnimatorStateMachine sm, string stateName, string clipName)
    {
        var st = sm.AddState(stateName);
        st.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(AnimDir + clipName + ".anim");
        if (st.motion == null) Debug.LogError("[PlayerAnimBuilderV2] missing clip: " + clipName);
        return st;
    }

    private static AnimatorCondition Cond(AnimatorConditionMode mode, float threshold, string parameter)
    {
        return new AnimatorCondition { mode = mode, threshold = threshold, parameter = parameter };
    }

    private static void AddT(AnimatorState from, AnimatorState to, params AnimatorCondition[] conditions)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.hasFixedDuration = true;
        t.duration = TransitionDuration;
        foreach (var c in conditions) t.AddCondition(c.mode, c.threshold, c.parameter);
    }

    private static void AddAny(AnimatorStateMachine sm, AnimatorState to, params AnimatorCondition[] conditions)
    {
        var t = sm.AddAnyStateTransition(to);
        t.hasExitTime = false;
        t.hasFixedDuration = true;
        t.duration = TransitionDuration;
        t.canTransitionToSelf = false;
        foreach (var c in conditions) t.AddCondition(c.mode, c.threshold, c.parameter);
    }

    private static void SliceSheet(string relPath, string prefix, int cols, int rows, int cellW, int cellH)
    {
        var path = SpriteRoot + relPath;
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null)
        {
            Debug.LogError("[PlayerAnimBuilderV2] no importer for " + path);
            return;
        }

        var s = new TextureImporterSettings();
        imp.ReadTextureSettings(s);
        s.textureType = TextureImporterType.Sprite;
        s.spriteMode = (int)SpriteImportMode.Multiple;
        s.spritePixelsPerUnit = Ppu;
        imp.SetTextureSettings(s);

        var metas = new SpriteMetaData[cols * rows];
        int idx = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                metas[idx] = new SpriteMetaData
                {
                    name = prefix + idx,
                    // rect origin is bottom-left; r=0 is the TOP row of the sheet
                    rect = new Rect(c * cellW, (rows - 1 - r) * cellH, cellW, cellH),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                };
                idx++;
            }
        }

        // Unity 6000.3: TextureImporter.spritesheet setter is REMOVED. Use ISpriteEditorDataProvider.
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var dataProvider = factory.GetSpriteEditorDataProviderFromObject(imp);
        dataProvider.InitSpriteEditorDataProvider();

        var editCapability = dataProvider.GetDataProvider<ISpriteFrameEditCapability>();
        if (editCapability == null ||
            !editCapability.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite) ||
            !editCapability.GetEditCapability().HasCapability(EEditCapability.EditSpriteName) ||
            !editCapability.GetEditCapability().HasCapability(EEditCapability.EditSpriteRect))
        {
            Debug.LogError("[PlayerAnimBuilderV2] " + path + ": importer does not support sprite editing (aborted)");
            return;
        }

        var spriteRects = new SpriteRect[metas.Length];
        for (int i = 0; i < metas.Length; i++)
        {
            spriteRects[i] = new SpriteRect
            {
                name = metas[i].name,
                rect = metas[i].rect,
                alignment = (SpriteAlignment)metas[i].alignment,
                pivot = metas[i].pivot,
                spriteID = GUID.Generate()
            };
        }

        // Unity 2021.2+: nameFileId pairs must be kept in sync when adding/removing sprites.
        var nameFileIdProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        if (nameFileIdProvider != null)
        {
            nameFileIdProvider.SetNameFileIdPairs(
                spriteRects.Select(sr => new SpriteNameFileIdPair(sr.name, sr.spriteID)).ToList());
        }

        dataProvider.SetSpriteRects(spriteRects);
        dataProvider.Apply();
        imp.SaveAndReimport();
        Debug.Log("[PlayerAnimBuilderV2] sliced " + path + " -> " + cols + "x" + rows + " (" + idx + " sprites, " + prefix + "0.." + (idx - 1) + ")");
    }

    private static void BuildClip(string clipName, IList<Sprite> sprites, float interval, bool loop)
    {
        if (sprites == null || sprites.Count == 0)
        {
            Debug.LogError("[PlayerAnimBuilderV2] " + clipName + ": no sprites");
            return;
        }
        if (sprites.Any(sp => sp == null))
        {
            Debug.LogError("[PlayerAnimBuilderV2] " + clipName + ": null sprite in sequence");
            return;
        }

        var clip = new AnimationClip { frameRate = 1f / interval };
        var keys = new ObjectReferenceKeyframe[sprites.Count];
        for (int i = 0; i < sprites.Count; i++)
        {
            keys[i] = new ObjectReferenceKeyframe { time = i * interval, value = sprites[i] };
        }

        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        var path = AnimDir + clipName + ".anim";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(clip, path);
        Debug.Log("[PlayerAnimBuilderV2] " + clipName + ": " + sprites.Count + " frames @ " + interval + "s loop=" + loop);
    }

    private static Dictionary<int, string> FrameFiles(string folder)
    {
        var full = SpriteRoot + folder;
        var dict = new Dictionary<int, string>();
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { full }))
        {
            var p = AssetDatabase.GUIDToAssetPath(guid);
            var m = Regex.Match(Path.GetFileNameWithoutExtension(p), @"^frame_(\d+)");
            if (m.Success) dict[int.Parse(m.Groups[1].Value)] = p;
        }
        return dict;
    }

    private static Sprite LoadFrameSprite(string folder, int frameNumber)
    {
        var files = FrameFiles(folder);
        string p;
        if (!files.TryGetValue(frameNumber, out p))
        {
            Debug.LogError("[PlayerAnimBuilderV2] frame " + frameNumber + " not found in " + folder);
            return null;
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(p);
    }

    private static List<Sprite> LoadFrameSprites(string folder, int from, int to)
    {
        var files = FrameFiles(folder);
        return files.Keys
            .Where(n => n >= from && n <= to)
            .OrderBy(n => n)
            .Select(n => AssetDatabase.LoadAssetAtPath<Sprite>(files[n]))
            .ToList();
    }

    private static List<Sprite> LoadSheetSprites(string path, string prefix)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .Where(s => s.name.StartsWith(prefix))
            .OrderBy(s => ParseIndex(s.name, prefix))
            .ToList();
    }

    private static int ParseIndex(string spriteName, string prefix)
    {
        int n;
        return int.TryParse(spriteName.Substring(prefix.Length), out n) ? n : int.MaxValue;
    }
}
