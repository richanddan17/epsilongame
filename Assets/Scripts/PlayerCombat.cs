using UnityEngine;

namespace EpsilonGame
{
    /// <summary>
    /// 플레이어 전투 시스템: 칼 공격 + 패링(일반/완벽) + 그랩 콤보(Combo Attack 2)
    /// Wave 2-1, 2-2: PlayerCombat.cs 단일 파일에 모두 구현
    /// guide(combo_attack_guide.txt): 패링 성공 후 0.3s 내 좌클릭/선입력 → GrabHitbox → GrabPoint 고정 → 무적 → 애니메이션 완전 종료 순간 데미지+넉백
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Attack")]
        [SerializeField] private int attackDamage = 1;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldown = 0.4f;
        [SerializeField] private float attackDuration = 0.15f;
        [SerializeField] private Vector2 attackOffset = new Vector2(0.8f, 0f);

        [Header("Parry")]
        [SerializeField] private float parryWindowBefore = 0.2f;
        [SerializeField] private float parryWindowAfter = 0.08f;
        [SerializeField] private float perfectParryFraction = 0.5f;
        [SerializeField] private float parryCooldown = 0.3f;

        [Header("Grab (Combo Attack 2)")]
        [SerializeField] private float grabRange = 1.2f;
        [SerializeField] private Vector2 grabOffset = new Vector2(0.55f, 0.15f); // GrabPoint 로컬 위치 (전방)
        [SerializeField] private float parryGrabWindow = 0.3f;                    // guide: 패링 성공 후 그랩 입력 가능 시간
        [SerializeField] private float grabKnockbackForce = 8f;

        [Header("VFX")]
        [SerializeField] private VfxSlot vfxSlot;

        [Header("Detection")]
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Transform attackPoint;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        // 내부 상태
        private float lastAttackTime = -999f;
        private float lastParryTime = -999f;
        private float parryWindowEndTime = -999f;
        private bool isAttacking = false;
        private bool isComboAttacking = false;
        private float attackEndTime = -999f;
        private float comboAttackEndTime = -999f;
        private float attackClipLength = -1f;
        private float comboAttackClipLength = -1f;
        private bool parryWindowActive = false;
        private GameObject telegraphedEnemy = null;

        // 파링 모션 상태 (PlayerController가 IsParrying 상승 에지로 후퇴 트리거)
        private bool isParryingMotion = false;
        private float parryMotionEndTime = -999f;
        private float parryClipLength = -1f;
        private float lastParrySuccessTime = -999f;

        // 그랩 상태
        private PlayerHealth playerHealth;
        private Transform grabAnchor;
        private GameObject grabbedEnemy;
        private EnemyHealth grabbedEnemyHealth;
        private Enemy grabbedEnemyAI;
        private Rigidbody2D grabbedEnemyRigidbody;
        private RigidbodyType2D grabbedEnemyBodyType;
        private Collider2D grabbedEnemyCollider;
        private Transform grabbedEnemyParent;

        // 공개 프로퍼티
        public bool IsAttacking => isAttacking;
        public bool IsComboAttacking => isComboAttacking;

        /// <summary>
        /// 파링 모션 재생 여부 (파링 클립 동안 true) — PlayerController 후퇴/입력 잠금용.
        /// 파링 입력 윈도우(parryWindowActive)와 분리: 모션 기반.
        /// </summary>
        public bool IsParrying => isParryingMotion;
        public bool HasParryWindow => parryWindowActive && Time.unscaledTime < parryWindowEndTime;

        private void Awake()
        {
            // attackPoint가 없으면 자기 자신 사용
            if (attackPoint == null)
                attackPoint = transform;

            if (animator == null)
                animator = GetComponent<Animator>();
            playerHealth = GetComponent<PlayerHealth>();
            CacheClipLengths();

            // CombatEvents 구독
            CombatEvents.OnEnemyAttackTelegraph += OnEnemyTelegraph;
        }

        // 공격 지속 = 클립 전체 재생 시간 (각 duration 폴백)
        private void CacheClipLengths()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
                return;

            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == "PlayerAttack") attackClipLength = clip.length;
                else if (clip.name == "PlayerComboAttack") comboAttackClipLength = clip.length;
                else if (clip.name == "PlayerParry") parryClipLength = clip.length;
            }

            if (attackClipLength <= 0f) attackClipLength = attackDuration;
            if (comboAttackClipLength <= 0f) comboAttackClipLength = attackDuration;
            if (parryClipLength <= 0f) parryClipLength = 0.55f; // PlayerParry 13f @24fps 폴백
        }

        private void OnDestroy()
        {
            CombatEvents.OnEnemyAttackTelegraph -= OnEnemyTelegraph;
        }

        private void Update()
        {
            HandleAttackInput();
            HandleParryInput();
            UpdateAttackState();
            UpdateParryWindow();
            UpdateGrabbedEnemy();
        }

        private void HandleAttackInput()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                // 패링 성공 직후 그랩 콤보 우선 (guide: 0.3s 윈도우 + 패링 중 선입력 흡수)
                if (CanGrabCombo())
                    StartGrabCombo();
                else if (CanAttack())
                    PerformAttack();
            }
            else if (Input.GetButtonDown("Fire2") && CanComboAttack())
            {
                PerformComboAttack();
            }
        }

        private void HandleParryInput()
        {
            if (Input.GetKeyDown(KeyCode.E) && CanParry())
            {
                TryParry();
            }
        }

        private bool CanAttack()
        {
            return !isAttacking && !isComboAttacking && Time.unscaledTime >= lastAttackTime + attackCooldown;
        }

        private bool CanComboAttack()
        {
            return !isAttacking && !isComboAttacking && Time.unscaledTime >= lastAttackTime + attackCooldown;
        }

        private bool CanParry()
        {
            return Time.unscaledTime >= lastParryTime + parryCooldown;
        }

        private bool CanGrabCombo()
        {
            return !isAttacking && !isComboAttacking
                && Time.unscaledTime - lastParrySuccessTime <= parryGrabWindow;
        }

        private void PerformAttack()
        {
            isAttacking = true;
            attackEndTime = Time.unscaledTime + attackClipLength;
            lastAttackTime = Time.unscaledTime;

            if (animator != null)
                animator.SetBool("IsAttacking", true);

            // 플레이어 공격 이벤트 발화 (슬로우 모션/카메라 연출 트리거)
            CombatEvents.RaiseOnPlayerAttack();

            ResolveAttackHits();
        }

        private void PerformComboAttack()
        {
            isComboAttacking = true;
            comboAttackEndTime = Time.unscaledTime + comboAttackClipLength;
            lastAttackTime = Time.unscaledTime;

            if (animator != null)
                animator.SetBool("IsComboAttacking", true);

            // 플레이어 공격 이벤트 발화 (슬로우 모션/카메라 연출 트리거)
            CombatEvents.RaiseOnPlayerAttack();

            ResolveAttackHits();
        }

        // 일반/콤보 공통 판정 + VFX (기존 로직과 동일)
        private void ResolveAttackHits()
        {
            Vector2 attackPosition = (Vector2)attackPoint.position + attackOffset * Mathf.Sign(transform.localScale.x);
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPosition, attackRange, enemyLayer);

            foreach (var hit in hits)
            {
                var damagable = hit.GetComponent<IEpsilonDamagable>();
                if (damagable != null)
                {
                    Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                    damagable.TakeDamage(attackDamage, knockbackDir, 5f);
                }
            }

            // VFX 재생 (null 허용) - 공격 방향 전달
            if (vfxSlot != null)
            {
                Vector2 attackDir = new Vector2(Mathf.Sign(transform.localScale.x), 0f);
                vfxSlot.SpawnSlashEffect(attackDir);
            }
        }

        private void UpdateAttackState()
        {
            if (isAttacking && Time.unscaledTime >= attackEndTime)
            {
                isAttacking = false;
                if (animator != null)
                    animator.SetBool("IsAttacking", false);
            }

            if (isComboAttacking && Time.unscaledTime >= comboAttackEndTime)
            {
                isComboAttacking = false;
                if (animator != null)
                    animator.SetBool("IsComboAttacking", false);

                // guide: 데미지는 애니메이션 완전 종료 순간에만 적용
                ReleaseGrab();
            }

            if (isParryingMotion && Time.unscaledTime >= parryMotionEndTime)
            {
                isParryingMotion = false;
                if (animator != null)
                    animator.SetBool("IsParrying", false);
            }
        }

        private void OnEnemyTelegraph(GameObject enemy)
        {
            // 적 공격 예고 수신 → 패링 윈도우 시작
            telegraphedEnemy = enemy;
            parryWindowActive = true;
            parryWindowEndTime = Time.unscaledTime + parryWindowBefore + parryWindowAfter;
        }

        private void TryParry()
        {
            lastParryTime = Time.unscaledTime;

            if (!parryWindowActive || telegraphedEnemy == null)
            {
                // 패링 윈도우 밖: 일반 패링 실패 (쿨다운만 소모)
                return;
            }

            float timeSinceTelegraph = Time.unscaledTime - (parryWindowEndTime - parryWindowBefore - parryWindowAfter);
            float windowCenter = parryWindowBefore;
            float perfectThreshold = (parryWindowBefore + parryWindowAfter) * perfectParryFraction;

            bool isPerfect = Mathf.Abs(timeSinceTelegraph - windowCenter) <= perfectThreshold;

            // 파링 성공 → 파링 모션 시작 (PlayerController가 IsParrying 상승 에지로 후퇴 트리거)
            StartParryMotion();

            OnParrySuccess(telegraphedEnemy, isPerfect);

            // 패링 성공 시 윈도우 종료
            parryWindowActive = false;
            telegraphedEnemy = null;
        }

        private void StartParryMotion()
        {
            isParryingMotion = true;
            parryMotionEndTime = Time.unscaledTime + parryClipLength;
            lastParrySuccessTime = Time.unscaledTime;

            if (animator != null)
                animator.SetBool("IsParrying", true);
        }

        private void UpdateParryWindow()
        {
            if (parryWindowActive && Time.unscaledTime >= parryWindowEndTime)
            {
                parryWindowActive = false;
                telegraphedEnemy = null;
            }
        }

        private void OnParrySuccess(GameObject enemy, bool isPerfect)
        {
            var damagable = enemy.GetComponent<IEpsilonDamagable>();
            if (damagable != null)
            {
                // 완벽 패링: 추가 대미지/넉백/슬로우모션 등 확장 가능
                // 일반 패링: 기본 리액션
                if (isPerfect)
                {
                    // 완벽 패링 로직 (향후 확장)
                    damagable.TakeDamage(attackDamage, Vector2.left * Mathf.Sign(transform.localScale.x), 10f);
                }
                else
                {
                    // 일반 패링 로직
                    damagable.TakeDamage(0, Vector2.zero, 0f); // 대미지 없이 패링 성공 알림용
                }
            }

            // VFX 재생 (null 허용) - 완벽 패링만 전용 이펙트
            if (vfxSlot != null && isPerfect)
            {
                vfxSlot.SpawnPerfectParryEffect(enemy.transform.position);
            }
        }

        // ===== Grab (Combo Attack 2) =====

        private void StartGrabCombo()
        {
            GameObject target = FindGrabTarget();
            if (target == null)
            {
                // guide: 그랩 실패 시 조합 공격 연출 실행하지 않음
                return;
            }

            isComboAttacking = true;
            comboAttackEndTime = Time.unscaledTime + comboAttackClipLength;
            lastAttackTime = Time.unscaledTime;

            if (animator != null)
                animator.SetBool("IsComboAttacking", true);

            // 플레이어 공격 이벤트 발화 (슬로우 모션/카메라 연출 트리거)
            CombatEvents.RaiseOnPlayerAttack();

            GrabEnemy(target);
        }

        // 전방 GrabHitbox에서 생존 적 검색
        private GameObject FindGrabTarget()
        {
            Vector2 grabPosition = (Vector2)attackPoint.position + attackOffset * Mathf.Sign(transform.localScale.x);
            Collider2D[] hits = Physics2D.OverlapCircleAll(grabPosition, grabRange, enemyLayer);

            foreach (var hit in hits)
            {
                var health = hit.GetComponent<EnemyHealth>();
                if (health != null && !health.IsDead)
                    return hit.gameObject;
            }
            return null;
        }

        // 잡힌 적: AI/물리 정지 + 피격 무시 + GrabPoint 고정, 플레이어 무적
        private void GrabEnemy(GameObject enemy)
        {
            grabbedEnemy = enemy;
            grabbedEnemyHealth = enemy.GetComponent<EnemyHealth>();
            grabbedEnemyAI = enemy.GetComponent<Enemy>();
            grabbedEnemyRigidbody = enemy.GetComponent<Rigidbody2D>();
            grabbedEnemyCollider = enemy.GetComponent<Collider2D>();

            if (grabbedEnemyRigidbody != null)
            {
                grabbedEnemyBodyType = grabbedEnemyRigidbody.bodyType;
                grabbedEnemyRigidbody.bodyType = RigidbodyType2D.Kinematic;
                grabbedEnemyRigidbody.linearVelocity = Vector2.zero;
            }
            if (grabbedEnemyCollider != null) grabbedEnemyCollider.enabled = false;
            if (grabbedEnemyAI != null) grabbedEnemyAI.enabled = false;               // 이동/공격/AI 행동 중지
            if (grabbedEnemyHealth != null) grabbedEnemyHealth.SetGrabImmunity(true); // 피격/사망 판정 무시

            if (playerHealth != null) playerHealth.SetComboInvincible(true);          // 플레이어 무적

            EnsureGrabAnchor();
            grabbedEnemyParent = grabbedEnemy.transform.parent;
            grabbedEnemy.transform.SetParent(grabAnchor, true);
            grabbedEnemy.transform.position = grabAnchor.position;
        }

        // GrabPoint: 플레이어 전방 고정점 (런타임 생성 — 프리팹 수정 최소화)
        private void EnsureGrabAnchor()
        {
            if (grabAnchor != null) return;

            var go = new GameObject("GrabPoint");
            grabAnchor = go.transform;
            grabAnchor.SetParent(transform, false);
            grabAnchor.localPosition = grabOffset;
        }

        // 잡힌 적이 플레이어(전방 GrabPoint)를 따라가도록 위치 유지
        private void UpdateGrabbedEnemy()
        {
            if (grabbedEnemy != null && grabAnchor != null)
                grabbedEnemy.transform.position = grabAnchor.position;
        }

        // 콤보 종료: 적 복귀 → 데미지+넉백 → 무적 해제
        private void ReleaseGrab()
        {
            if (grabbedEnemy != null)
            {
                if (grabbedEnemy.transform.parent == grabAnchor)
                    grabbedEnemy.transform.SetParent(grabbedEnemyParent, true);

                if (grabbedEnemyCollider != null) grabbedEnemyCollider.enabled = true;
                if (grabbedEnemyRigidbody != null) grabbedEnemyRigidbody.bodyType = grabbedEnemyBodyType;
                if (grabbedEnemyAI != null) grabbedEnemyAI.enabled = true;

                // 면역 해제 후 애니메이션 완전 종료 순간 데미지+넉백
                if (grabbedEnemyHealth != null)
                {
                    grabbedEnemyHealth.SetGrabImmunity(false);
                    Vector2 dir = ((Vector2)grabbedEnemy.transform.position - (Vector2)transform.position).normalized;
                    grabbedEnemyHealth.TakeDamage(attackDamage, dir, grabKnockbackForce);
                }
            }

            if (playerHealth != null) playerHealth.SetComboInvincible(false);

            grabbedEnemy = null;
            grabbedEnemyHealth = null;
            grabbedEnemyAI = null;
            grabbedEnemyRigidbody = null;
            grabbedEnemyCollider = null;
            grabbedEnemyParent = null;
        }

        // 디버그용 기즈모
        private void OnDrawGizmosSelected()
        {
            if (attackPoint != null)
            {
                Vector2 pos = (Vector2)attackPoint.position + attackOffset * Mathf.Sign(transform.localScale.x);

                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(pos, attackRange);

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(pos, grabRange);
            }
        }
    }
}