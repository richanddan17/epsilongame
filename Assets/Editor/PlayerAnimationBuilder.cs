using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EpsilonGame;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// player-sprite-animation v2 — builds all player animation clips + animator controller
/// from Assets/sprite/player/ frame PNGs.
/// Naming: {motion}_{index}.png  (e.g. idle_00.png) OR {motion}_{index}_{delay}s.png
/// (e.g. attack_00_0.02s.png — the re-authored attack/combo_attack sets embed per-frame
/// delay in the filename, which is read directly; legacy timing tables below are only
/// a fallback when the filename has no delay suffix).
/// Special: fall_03.png is the fall-loop hold frame (custom duration, set in Unity).
/// Menu: Tools/Player/v2/Build All
/// </summary>
public static class PlayerAnimationBuilder
{
    private const string SpriteRoot = "Assets/sprite/player/";
    private const string AnimDir = "Assets/Animations/";
    private const string ControllerPath = AnimDir + "PlayerAnimator.controller";
    private const string PrefabPath = "Assets/Prefabs/Player.prefab";
    // 4x authored assets (README_4x.txt): PPU 400 keeps on-screen size identical to the 1x (100) set.
    private const float Ppu = 400f;
    private const float TransitionDuration = 0f;

    // Frame folders (relative to SpriteRoot). New v2 asset structure: Animations/ + Effects/.
    // combo_attack2 was merged into combo_attack (single 38-frame sequence, frames 00~12 = old combo_attack2);
    // its F/X folders live under the unified name combo_attack_fx / combo_attack_line.
    private static readonly string[] FrameFolders =
    {
        "Animations/attack",
        "Animations/combo_attack",
        "Animations/dash",
        "Animations/doublejump",
        "Animations/fall",
        "Animations/hurt",
        "Animations/idle",
        "Animations/jump",
        "Animations/parrying",
        "Animations/walk",
        "Effects/combo_attack_fx",
        "Effects/combo_attack_line",
        "Effects/dash_fx",
        "Effects/doublejump_fx",
        "Effects/parrying_fx"
    };

    // Spec counts from README Animator.md. Disk may hold fewer while new frames arrive
    // (gaps allowed per team; load what exists, only warn on shortfall).
    private static readonly Dictionary<string, int> ExpectedFrameCounts = new Dictionary<string, int>
    {
        // Re-authored sets (2026-09-19): delays embedded in filenames; counts are on-disk truth.
        { "Animations/attack", 36 },
        { "Animations/combo_attack", 38 },
        { "Animations/dash", 12 },
        { "Animations/doublejump", 7 },
        { "Animations/fall", 9 },
        { "Animations/hurt", 5 },
        { "Animations/idle", 10 },
        { "Animations/jump", 7 },
        { "Animations/parrying", 13 },
        { "Animations/walk", 24 },
        { "Effects/combo_attack_fx", 7 },
        { "Effects/combo_attack_line", 4 },
        { "Effects/dash_fx", 5 },
        { "Effects/doublejump_fx", 4 },
        { "Effects/parrying_fx", 5 }
    };

    // Jump take-off prep frames play faster than their file delay (README: jump 00~02 -> 0.03s).
    private static readonly Dictionary<int, float> JumpPrepDelays = new Dictionary<int, float>
    {
        { 0, 0.03f },
        { 1, 0.03f },
        { 2, 0.03f }
    };

    // Hurt clip frames play at 0.12s (folder default).
    private const float HurtFrameDelay = 0.12f;
    // Fallback for frame indices not covered by FolderDefaultDelays / FrameDelayOverrides.
    private const float DefaultFrameDelay = 0.06f;

    // Per-folder default frame delays (former _0.XXs filename suffix, archived 2026-09-19).
    // Uniform folders: one value covers every frame.
    private static readonly Dictionary<string, float> FolderDefaultDelays = new Dictionary<string, float>
    {
        { "Animations/dash", 0.07f },
        { "Animations/doublejump", 0.07f },
        { "Animations/fall", 0.07f },
        { "Animations/hurt", HurtFrameDelay },
        { "Animations/idle", 0.06f },
        { "Animations/jump", 0.07f },
        { "Animations/walk", 0.06f },
        { "Effects/dash_fx", 0.07f },
        { "Effects/doublejump_fx", 0.07f }
    };

    // Per-frame overrides for folders without filename delays (jump/fall).
    // attack / combo_attack are excluded here: their re-authored sets embed per-frame
    // delays in filenames (_0.XXs), which LoadFrameSpritesTimed reads directly.
    private static readonly Dictionary<string, Dictionary<int, float>> FrameDelayOverrides = new Dictionary<string, Dictionary<int, float>>
    {
        // jump_06 held longer (0.14s) than the rest of the clip (0.07s).
        { "Animations/jump", new Dictionary<int, float> { { 6, 0.14f } } },
        // fall_03 is the loop hold frame (former fall_03_custom(s)); no frame delay,
        // builder fallback 0.06s applies. Custom hold length is set in Unity.
        { "Animations/fall", new Dictionary<int, float> { { 3, DefaultFrameDelay } } }
    };

    // Pivot: idle/walk feet sit just above the canvas bottom, so drop the pivot Y to rest them on the ground.
    // Sign chosen by physical analysis (0.5 - correctionPx / heightPx). VERIFY IN-GAME; flip if floating/sinking.
    private const float IdlePivotYCorrectionPx = 2.5f;
    private const float WalkPivotYCorrectionPx = 4f;
    private const float IdleFrameHeightPx = 55f;
    private const float WalkFrameHeightPx = 58f;

    // combo_attack has an authored offset pivot in its canvas (README Animator.md).
    private const float ComboPivotX = 0.8965f;
    private const float ComboPivotY = 0.1143f;

    private static readonly Regex FrameIndexRegex = new Regex(@"_(\d+)(?:_(\d+(?:\.\d+)?)s)?$", RegexOptions.Compiled);

    [MenuItem("Tools/Player/v2/Build All")]
    public static void BuildAll()
    {
        ConfigureFrameImports();
        BuildClips();
        BuildController();
        ApplyPrefab();
        BuildVfxChildren();

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
                var pivot = PivotFor(folder);
                foreach (var kv in FrameFiles(folder))
                {
                    if (ConfigureSpriteImport(kv.Value.Path, pivot)) count++;
                }
            }

            // Shadow sprite: import settings only (prefab child wiring out of scope until approved).
            var shadowPath = SpriteRoot + "Shadow/shadow.png";
            if (File.Exists(shadowPath))
            {
                if (ConfigureSpriteImport(shadowPath, new Vector2(0.5f, 0.5f))) count++;
            }
            else
            {
                Debug.LogWarning("[PlayerAnimBuilderV2] shadow.png not found: " + shadowPath);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        AssetDatabase.Refresh();
        Debug.Log("[PlayerAnimBuilderV2] configured " + count + " frame PNGs (Single / PPU " + Ppu + " / point / uncompressed)");
    }

    [MenuItem("Tools/Player/v2/Build Clips")]
    public static void BuildClips()
    {
        // Ground loops (per-frame filename delays)
        BuildClipTimed("PlayerIdle", LoadFrameSpritesTimed("Animations/idle"), true);
        BuildClipTimed("PlayerWalk", LoadFrameSpritesTimed("Animations/walk"), true);

        // Airborne
        BuildClipTimed("PlayerJump", LoadFrameSpritesTimed("Animations/jump", JumpPrepDelays), false);
        BuildClipTimed("PlayerDoubleJump", LoadFrameSpritesTimed("Animations/doublejump"), false);

        // Fall split into 3 clips per README Animator.md: start(00..02) / loop(03, custom hold, loops) / land(04..08).
        BuildClipTimed("PlayerFallStart", LoadFrameSpritesTimed("Animations/fall", null, 0, 2), false);
        BuildClipTimed("PlayerFallLoop", LoadFrameSpritesTimed("Animations/fall", null, 3, 3), true);
        BuildClipTimed("PlayerLand", LoadFrameSpritesTimed("Animations/fall", null, 4, 8), false);

        // Attacks (folder temporarily removed; rebuilt automatically when the new sprites arrive)
        if (HasFrames("Animations/attack"))
            BuildClipTimed("PlayerAttack", LoadFrameSpritesTimed("Animations/attack"), false);
        if (HasFrames("Animations/combo_attack"))
            BuildClipTimed("PlayerComboAttack", LoadFrameSpritesTimed("Animations/combo_attack"), false);

        // Dash (new assets already numbered in play order; no replay reorder needed)
        BuildClipTimed("PlayerDash", LoadFrameSpritesTimed("Animations/dash"), false);

        // Hurt: clips only (hurt animator wiring out of scope). VFX clips are one-shot, code-triggered.
        BuildClipTimed("PlayerHurt", LoadFrameSpritesTimed("Animations/hurt", null, 0, int.MaxValue, HurtFrameDelay), false);
        BuildClipTimed("PlayerDashFX", LoadFrameSpritesTimed("Effects/dash_fx"), false);
        BuildClipTimed("PlayerDoubleJumpFX", LoadFrameSpritesTimed("Effects/doublejump_fx"), false);

        // Parry + combo_attack2 (merged into combo_attack head, frames 00~12) and their F/X.
        BuildClipTimed("PlayerParry", LoadFrameSpritesTimed("Animations/parrying"), false);
        BuildClipTimed("PlayerParryFX", LoadFrameSpritesTimed("Effects/parrying_fx"), false);
        BuildClipTimed("PlayerComboAttackFX", LoadFrameSpritesTimed("Effects/combo_attack_fx"), false);
        BuildClipTimed("PlayerComboAttackLine", LoadFrameSpritesTimed("Effects/combo_attack_line"), false);
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
        controller.AddParameter("IsParrying", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsHurt", AnimatorControllerParameterType.Bool);
        controller.AddParameter("JumpCount", AnimatorControllerParameterType.Int);

        var sm = controller.layers[0].stateMachine;

        var idle = AddState(sm, "Idle", "PlayerIdle");
        var walk = AddState(sm, "Walk", "PlayerWalk");
        var jump = AddState(sm, "Jump", "PlayerJump");
        var dbl = AddState(sm, "DoubleJump", "PlayerDoubleJump");
        var fallStart = AddState(sm, "FallStart", "PlayerFallStart");
        var fallLoop = AddState(sm, "FallLoop", "PlayerFallLoop");
        var land = AddState(sm, "Land", "PlayerLand");
        var dash = AddState(sm, "Dash", "PlayerDash");
        var parry = AddState(sm, "Parry", "PlayerParry");
        var hurt = AddState(sm, "Hurt", "PlayerHurt");
        // Attack states are added only while their folders exist; folds back in on return.
        var atk = HasFrames("Animations/attack") ? AddState(sm, "Attack", "PlayerAttack") : null;
        var combo = HasFrames("Animations/combo_attack") ? AddState(sm, "ComboAttack", "PlayerComboAttack") : null;
        sm.defaultState = idle;

        // AnyState override priority = addition order (guide: hurt > parry > combo_attack2 > attack/dash).
        AddAny(sm, hurt, Cond(AnimatorConditionMode.If, 0f, "IsHurt"));
        AddAny(sm, parry, Cond(AnimatorConditionMode.If, 0f, "IsParrying"));
        if (combo != null) AddAny(sm, combo, Cond(AnimatorConditionMode.If, 0f, "IsComboAttacking"));
        if (atk != null) AddAny(sm, atk, Cond(AnimatorConditionMode.If, 0f, "IsAttacking"));
        AddAny(sm, dash, Cond(AnimatorConditionMode.If, 0f, "IsDashing"));

        AddT(hurt, idle, Cond(AnimatorConditionMode.IfNot, 0f, "IsHurt"));
        AddT(parry, idle, Cond(AnimatorConditionMode.IfNot, 0f, "IsParrying"));

        if (atk != null) AddT(atk, idle, Cond(AnimatorConditionMode.IfNot, 0f, "IsAttacking"));
        if (combo != null) AddT(combo, idle, Cond(AnimatorConditionMode.IfNot, 0f, "IsComboAttacking"));

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
        AddT(idle, fallStart, Cond(AnimatorConditionMode.IfNot, 0f, "IsGrounded"), Cond(AnimatorConditionMode.If, 0f, "IsFalling"));
        AddT(walk, fallStart, Cond(AnimatorConditionMode.IfNot, 0f, "IsGrounded"), Cond(AnimatorConditionMode.If, 0f, "IsFalling"));

        // Air transitions
        AddT(jump, fallStart, Cond(AnimatorConditionMode.If, 0f, "IsFalling"));
        AddT(jump, dbl, Cond(AnimatorConditionMode.Equals, 2f, "JumpCount"));
        AddT(dbl, fallStart, Cond(AnimatorConditionMode.If, 0f, "IsFalling"));

        // Fall → double jump (space pressed mid-fall after using first jump: JumpCount becomes 2).
        // JumpCount==1 (falling after first jump, one jump left) must NOT transition to Jump/JumpStart
        // from fall states — that would flicker Jump↔FallStart every frame while airborne.
        AddT(fallStart, dbl, Cond(AnimatorConditionMode.Equals, 2f, "JumpCount"), Cond(AnimatorConditionMode.IfNot, 0f, "IsDashing"));
        AddT(fallLoop, dbl, Cond(AnimatorConditionMode.Equals, 2f, "JumpCount"), Cond(AnimatorConditionMode.IfNot, 0f, "IsDashing"));

        // Fall chain: start plays 00..02 then holds on the custom loop frame until grounded; land on contact.
        // Grounded check is added first so a contact during fall_start wins over the exit-time to fall_loop.
        AddT(fallStart, land, Cond(AnimatorConditionMode.If, 0f, "IsGrounded"));
        AddTExit(fallStart, fallLoop, 1f);
        AddT(fallLoop, land, Cond(AnimatorConditionMode.If, 0f, "IsGrounded"));
        AddTExit(land, idle, 0.99f, Cond(AnimatorConditionMode.If, 0f, "IsGrounded"), Cond(AnimatorConditionMode.Less, 0.1f, "Speed"));
        AddTExit(land, walk, 0.99f, Cond(AnimatorConditionMode.If, 0f, "IsGrounded"), Cond(AnimatorConditionMode.Greater, 0.1f, "Speed"));

        // Direct land from short air segments that resolve before IsFalling is set
        AddT(jump, idle, Cond(AnimatorConditionMode.If, 0f, "IsGrounded"), Cond(AnimatorConditionMode.Less, 0.1f, "Speed"));
        AddT(jump, walk, Cond(AnimatorConditionMode.If, 0f, "IsGrounded"), Cond(AnimatorConditionMode.Greater, 0.1f, "Speed"));
        AddT(dbl, idle, Cond(AnimatorConditionMode.If, 0f, "IsGrounded"), Cond(AnimatorConditionMode.Less, 0.1f, "Speed"));
        AddT(dbl, walk, Cond(AnimatorConditionMode.If, 0f, "IsGrounded"), Cond(AnimatorConditionMode.Greater, 0.1f, "Speed"));

        // Dash escape (param stays false until dash gameplay exists)
        AddT(dash, idle, Cond(AnimatorConditionMode.IfNot, 0f, "IsDashing"), Cond(AnimatorConditionMode.If, 0f, "IsGrounded"), Cond(AnimatorConditionMode.Less, 0.1f, "Speed"));
        AddT(dash, walk, Cond(AnimatorConditionMode.IfNot, 0f, "IsDashing"), Cond(AnimatorConditionMode.If, 0f, "IsGrounded"), Cond(AnimatorConditionMode.Greater, 0.1f, "Speed"));
        AddT(dash, fallStart, Cond(AnimatorConditionMode.IfNot, 0f, "IsDashing"), Cond(AnimatorConditionMode.IfNot, 0f, "IsGrounded"), Cond(AnimatorConditionMode.If, 0f, "IsFalling"));

        AssetDatabase.SaveAssets();
        Debug.Log("[PlayerAnimBuilderV2] controller rebuilt: 7 params / 10 states / " + ControllerPath);
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

        var idleFrames = FrameFiles("Animations/idle");
        if (idleFrames.Count == 0)
        {
            Debug.LogError("[PlayerAnimBuilderV2] no idle frames under " + SpriteRoot + "Animations/idle");
            return;
        }

        var firstIdlePath = idleFrames.OrderBy(kv => kv.Key).First().Value.Path;
        var idleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(firstIdlePath);
        if (idleSprite == null)
        {
            Debug.LogError("[PlayerAnimBuilderV2] failed to load idle sprite: " + firstIdlePath);
            return;
        }

        sr.sprite = idleSprite;
        sr.color = Color.white;

        var anim = prefab.GetComponent<Animator>();
        if (anim == null) anim = prefab.AddComponent<Animator>();
        anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);

        PrefabUtility.SavePrefabAsset(prefab);
        AssetDatabase.SaveAssets();
        Debug.Log("[PlayerAnimBuilderV2] prefab applied: sprite=" + sr.sprite.name + " animator=" + (anim.runtimeAnimatorController != null));
    }

    [MenuItem("Tools/Player/v2/Build VFX Children")]
    public static void BuildVfxChildren()
    {
        var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefabAsset == null)
        {
            Debug.LogError("[PlayerAnimBuilderV2] prefab not found: " + PrefabPath);
            return;
        }

        // 프리팹 자산에 직접 편집은 LoadPrefabContents/SavePrefabContents가 Unity 6에서
        // 반영되지 않으므로, 씬 인스턴스를 만들어 수정한 뒤 ApplyPrefabInstance로 반영.
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
        if (instance == null)
        {
            Debug.LogError("[PlayerAnimBuilderV2] failed to instantiate prefab: " + PrefabPath);
            return;
        }

        var sr = instance.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("[PlayerAnimBuilderV2] SpriteRenderer missing on prefab root");
            Object.DestroyImmediate(instance);
            return;
        }
        int fxSort = sr.sortingOrder + 1;

        WireShadow(instance, sr.sortingOrder - 1);

        // README offsets are authored in WORLD space with the character facing LEFT (localScale.x = -1),
        // where 'behind' = +x world (guide §9). Parented children inherit the parent flip every frame:
        // world x = local x * scale.x. So an FX that must sit BEHIND (guide +x) needs a NEGATIVE local x,
        // and one that must sit FORWARD (guide -x) needs a POSITIVE local x. Runtime-verified.
        var doubleJumpFx = WireVfxChild(instance, "DoubleJumpFX", "PlayerDoubleJumpFX", "Effects/doublejump_fx",
            new Vector2(-0.49f, -0.65f), fxSort);
        var dashFx = WireVfxChild(instance, "DashFX", "PlayerDashFX", "Effects/dash_fx",
            new Vector2(-0.88f, 0.09f), fxSort);
        var parryFx = WireVfxChild(instance, "ParryFX", "PlayerParryFX", "Effects/parrying_fx",
            new Vector2(-0.18f, -0.12f), fxSort);
        var comboAttackFx = WireVfxChild(instance, "ComboAttackFX", "PlayerComboAttackFX", "Effects/combo_attack_fx",
            new Vector2(-0.39f, -0.06f), fxSort);
        var comboAttackLine = WireVfxChild(instance, "ComboAttackLine", "PlayerComboAttackLine", "Effects/combo_attack_line",
            new Vector2(1.35f, -0.04f), fxSort);

        var pc = instance.GetComponent<PlayerController>();
        if (pc != null)
        {
            var so = new SerializedObject(pc);
            so.FindProperty("doubleJumpFx").objectReferenceValue = doubleJumpFx;
            so.FindProperty("dashFx").objectReferenceValue = dashFx;
            so.FindProperty("parryFx").objectReferenceValue = parryFx;
            so.FindProperty("comboAttackFx").objectReferenceValue = comboAttackFx;
            so.FindProperty("comboAttackLine").objectReferenceValue = comboAttackLine;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        else
        {
            Debug.LogWarning("[PlayerAnimBuilderV2] PlayerController not found on prefab; FX refs not wired");
        }

        PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.UserAction);
        Object.DestroyImmediate(instance);
        AssetDatabase.SaveAssets();
        Debug.Log("[PlayerAnimBuilderV2] vfx children built: DoubleJumpFX=" + (doubleJumpFx != null) +
                  " DashFX=" + (dashFx != null) + " ParryFX=" + (parryFx != null) +
                  " ComboAttackFX=" + (comboAttackFx != null) + " ComboAttackLine=" + (comboAttackLine != null));
    }

    private static void WireShadow(GameObject prefab, int sortingOrder)
    {
        var existing = prefab.transform.Find("Shadow");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var go = new GameObject("Shadow");
        go.transform.SetParent(prefab.transform, false);
        go.transform.localPosition = new Vector3(0f, -0.25f, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteRoot + "Shadow/shadow.png");
        sr.sortingOrder = sortingOrder;
    }

    private static GameObject WireVfxChild(GameObject prefab, string childName, string clipName, string folder,
        Vector2 localPos, int sortingOrder)
    {
        var existing = prefab.transform.Find(childName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        var go = new GameObject(childName);
        go.transform.SetParent(prefab.transform, false);
        go.transform.localPosition = localPos;

        var firstFrame = FrameFiles(folder).OrderBy(kv => kv.Key).FirstOrDefault(kv => true).Value.Path;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(firstFrame);
        sr.sortingOrder = sortingOrder;

        var anim = go.AddComponent<Animator>();
        anim.runtimeAnimatorController = EnsureVfxController(clipName);

        go.SetActive(false);
        return go;
    }

    private static AnimatorController EnsureVfxController(string clipName)
    {
        var path = AnimDir + clipName + ".controller";
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (existing != null) return existing;

        var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        var sm = controller.layers[0].stateMachine;
        var st = sm.AddState("Play");
        st.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(AnimDir + clipName + ".anim");
        return controller;
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

    private static void AddTExit(AnimatorState from, AnimatorState to, float exitTime, params AnimatorCondition[] conditions)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = true;
        t.exitTime = exitTime;
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

    private static Vector2 PivotFor(string folder)
    {
        if (folder == "Animations/combo_attack") return new Vector2(ComboPivotX, ComboPivotY);

        float height;
        float correction;
        if (folder == "Animations/idle") { height = IdleFrameHeightPx; correction = IdlePivotYCorrectionPx; }
        else if (folder == "Animations/walk") { height = WalkFrameHeightPx; correction = WalkPivotYCorrectionPx; }
        else return new Vector2(0.5f, 0.5f);

        return new Vector2(0.5f, 0.5f - correction / height);
    }

    private static bool ConfigureSpriteImport(string path, Vector2 pivot)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) return false;

        var s = new TextureImporterSettings();
        imp.ReadTextureSettings(s);
        s.textureType = TextureImporterType.Sprite;
        s.spriteMode = (int)SpriteImportMode.Single;
        s.spritePixelsPerUnit = Ppu;
        s.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        s.spriteAlignment = (int)SpriteAlignment.Custom;
        s.spritePivot = pivot;
        imp.SetTextureSettings(s);
        EditorUtility.SetDirty(imp);
        return true;
    }

    private static void BuildClipTimed(string clipName, IList<(Sprite sprite, float delay)> frames, bool loop)
    {
        if (frames == null || frames.Count == 0)
        {
            Debug.LogError("[PlayerAnimBuilderV2] " + clipName + ": no sprites");
            return;
        }
        if (frames.Any(f => f.sprite == null))
        {
            Debug.LogError("[PlayerAnimBuilderV2] " + clipName + ": null sprite in sequence");
            return;
        }

        var keys = new ObjectReferenceKeyframe[frames.Count];
        float time = 0f;
        for (int i = 0; i < frames.Count; i++)
        {
            keys[i] = new ObjectReferenceKeyframe { time = time, value = frames[i].sprite };
            time += frames[i].delay;
        }

        WriteClipAsset(clipName, keys, 30f, loop, frames.Count + " frames, " + time.ToString("F2", CultureInfo.InvariantCulture) + "s total");
    }

    private static void WriteClipAsset(string clipName, ObjectReferenceKeyframe[] keys, float frameRate, bool loop, string summary)
    {
        var clip = new AnimationClip { frameRate = frameRate };

        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        var path = AnimDir + clipName + ".anim";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(clip, path);
        Debug.Log("[PlayerAnimBuilderV2] " + clipName + ": " + summary + " loop=" + loop);
    }

    private static IList<(Sprite sprite, float delay)> LoadFrameSpritesTimed(
        string folder,
        Dictionary<int, float> delayOverrides = null,
        int from = 0,
        int to = int.MaxValue,
        float fallbackDelay = DefaultFrameDelay)
    {
        var files = FrameFiles(folder);

        if (ExpectedFrameCounts.TryGetValue(folder, out var expected) && files.Count < expected)
            Debug.LogWarning("[PlayerAnimBuilderV2] " + folder + ": " + files.Count + "/" + expected +
                             " frames on disk — " + (expected - files.Count) + " missing.");

        return files
            .Where(kv => kv.Key >= from && kv.Key <= to)
            .OrderBy(kv => kv.Key)
            .Select(kv =>
            {
                float delay = kv.Value.Delay.HasValue ? kv.Value.Delay.Value
                    : FrameDelayOverrides.TryGetValue(folder, out var overrides) && overrides.TryGetValue(kv.Key, out var od) ? od
                    : FolderDefaultDelays.TryGetValue(folder, out var folderDelay) ? folderDelay
                    : fallbackDelay;
                if (delayOverrides != null && delayOverrides.TryGetValue(kv.Key, out var ov)) delay = ov;
                return (AssetDatabase.LoadAssetAtPath<Sprite>(kv.Value.Path), delay);
            })
            .ToList();
    }

    private static bool HasFrames(string folder)
    {
        var full = SpriteRoot + folder;
        return AssetDatabase.IsValidFolder(full)
            && AssetDatabase.FindAssets("t:Texture2D", new[] { full }).Length > 0;
    }

    private static Dictionary<int, (string Path, float? Delay)> FrameFiles(string folder)
    {
        var full = SpriteRoot + folder;
        var dict = new Dictionary<int, (string, float?)>();
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { full }))
        {
            var p = AssetDatabase.GUIDToAssetPath(guid);
            var m = FrameIndexRegex.Match(Path.GetFileNameWithoutExtension(p));
            if (!m.Success) continue;
            float? delay = null;
            if (m.Groups[2].Success)
            {
                var raw = m.Groups[2].Value;
                if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) delay = d;
                else Debug.LogWarning("[PlayerAnimBuilderV2] unparsable delay suffix in " + p + ": \"" + raw + "\"");
            }
            dict[int.Parse(m.Groups[1].Value)] = (p, delay);
        }
        return dict;
    }
}