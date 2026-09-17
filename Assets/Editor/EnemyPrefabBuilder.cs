using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace EpsilonGame.Editor
{
    /// <summary>
    /// Enemy 프리팹 + AnimatorController 골격 생성기 (Todo 3-3).
    /// 배치 모드: Unity.exe -batchmode -quit -projectPath ... -executeMethod EpsilonGame.Editor.EnemyPrefabBuilder.BuildEnemyPrefab
    /// 에디터 메뉴: EpsilonGame > Build Enemy Prefab
    /// </summary>
    public static class EnemyPrefabBuilder
    {
        private const string PrefabPath = "Assets/Prefabs/Enemy.prefab";
        private const string ControllerPath = "Assets/Animations/EnemyAnimator.controller";
        private const string PlaceholderTexPath = "Assets/Sprites/Placeholder_White.png";

        [MenuItem("EpsilonGame/Build Enemy Prefab")]
        public static void BuildEnemyPrefab()
        {
            Debug.Log("[EnemyPrefabBuilder] 시작");

            // 1) AnimatorController 생성
            AnimatorController controller = CreateAnimatorController();
            if (controller == null)
            {
                Debug.LogError("[EnemyPrefabBuilder] AnimatorController 생성 실패");
                return;
            }
            Debug.Log("[EnemyPrefabBuilder] AnimatorController 저장됨: " + ControllerPath);

            // 2) Placeholder 스프라이트 생성 (1x1 흰색 텍스처)
            Sprite placeholder = CreatePlaceholderSprite();
            if (placeholder == null)
            {
                Debug.LogError("[EnemyPrefabBuilder] Placeholder 스프라이트 생성 실패");
                return;
            }
            Debug.Log("[EnemyPrefabBuilder] Placeholder 스프라이트 준비됨: " + placeholder.name);

            // 3) Enemy 프리팹 생성
            CreateEnemyPrefab(controller, placeholder);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EnemyPrefabBuilder] 완료");
        }

        // ===== AnimatorController =====
        private static AnimatorController CreateAnimatorController()
        {
            // 기존 에셋이 있으면 삭제 후 재생성 (재실행 안전)
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
            {
                AssetDatabase.DeleteAsset(ControllerPath);
            }

            EnsureFolder("Assets/Animations");

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            if (controller == null)
            {
                return null;
            }

            // 파라미터 4개 (bool)
            controller.AddParameter("Summoned", AnimatorControllerParameterType.Bool);
            controller.AddParameter("GotHit", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Dead", AnimatorControllerParameterType.Bool);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;

            // 상태 4개 (placeholder, 클립 미할당)
            AnimatorState idle = sm.AddState("Idle");
            AnimatorState attack = sm.AddState("Attack");
            AnimatorState gotHit = sm.AddState("GotHit");
            AnimatorState dead = sm.AddState("Dead");
            sm.defaultState = idle;

            // 트랜지션 (표에 따라 정확히)
            // Idle -> Attack : Attack == true
            AnimatorStateTransition idleToAttack = idle.AddTransition(attack);
            idleToAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

            // Idle -> GotHit : GotHit == true
            AnimatorStateTransition idleToGotHit = idle.AddTransition(gotHit);
            idleToGotHit.AddCondition(AnimatorConditionMode.If, 0f, "GotHit");

            // GotHit -> Idle : GotHit == false
            AnimatorStateTransition gotHitToIdle = gotHit.AddTransition(idle);
            gotHitToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "GotHit");

            // Attack -> Idle : Attack == false
            AnimatorStateTransition attackToIdle = attack.AddTransition(idle);
            attackToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "Attack");

            // Any State -> Dead : Dead == true
            AnimatorStateTransition anyToDead = sm.AddAnyStateTransition(dead);
            anyToDead.AddCondition(AnimatorConditionMode.If, 0f, "Dead");

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }

        // ===== Placeholder 스프라이트 =====
        private static Sprite CreatePlaceholderSprite()
        {
            EnsureFolder("Assets/Sprites");

            // 1x1 흰색 PNG 생성
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            byte[] png = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);

            File.WriteAllBytes(PlaceholderTexPath, png);
            AssetDatabase.ImportAsset(PlaceholderTexPath, ImportAssetOptions.ForceUpdate);

            // Sprite 타입으로 임포트 설정
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(PlaceholderTexPath);
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Point;
                // 1x1 텍스처를 1x1 월드 유닛으로 렌더링 (기본 100 PPU면 0.01 유닛 — 거의 안 보임)
                importer.spritePixelsPerUnit = 1f;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(PlaceholderTexPath);
        }

        // ===== Enemy 프리팹 =====
        private static void CreateEnemyPrefab(AnimatorController controller, Sprite placeholder)
        {
            // 기존 프리팹 삭제 후 재생성
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
            {
                AssetDatabase.DeleteAsset(PrefabPath);
            }

            EnsureFolder("Assets/Prefabs");

            GameObject go = new GameObject("Enemy");

            // 레이어/태그 (TagManager에 이미 정의됨)
            go.tag = "Enemy";
            go.layer = LayerMask.NameToLayer("Enemy");

            // SpriteRenderer — placeholder 흰색 사각형 + 빨간 틴트
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = placeholder;
            sr.color = new Color(0.8f, 0.2f, 0.2f, 1f); // 임시 빨간색

            // Rigidbody2D — Interpolate, GravityScale 1, FreezeRotation Z
            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.gravityScale = 1f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // BoxCollider2D — 지면 충돌용
            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);

            // Animator — 컨트롤러 연결
            Animator animator = go.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            // Enemy.cs + EnemyHealth.cs (RequireComponent로 Rigidbody2D/Collider2D 자동 충족)
            go.AddComponent<Enemy>();
            go.AddComponent<EnemyHealth>();

            // 프리팹 저장
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);

            if (saved != null)
            {
                Debug.Log("[EnemyPrefabBuilder] 프리팹 저장됨: " + PrefabPath);
                Debug.Log("[EnemyPrefabBuilder]   layer=" + saved.layer + " (" + LayerMask.LayerToName(saved.layer) + "), tag=" + saved.tag);
                Debug.Log("[EnemyPrefabBuilder]   components: SpriteRenderer, Rigidbody2D, BoxCollider2D, Animator, Enemy, EnemyHealth");
            }
            else
            {
                Debug.LogError("[EnemyPrefabBuilder] 프리팹 저장 실패: " + PrefabPath);
            }
        }

        // ===== 폴더 보장 =====
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string folder = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}