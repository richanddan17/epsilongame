using UnityEngine;

namespace EpsilonGame
{
    /// <summary>
    /// 플레이어 전투 시스템: 칼 공격 + 패링 (일반/완벽)
    /// Wave 2-1, 2-2: PlayerCombat.cs 단일 파일에 모두 구현
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

        // 공개 프로퍼티
        public bool IsAttacking => isAttacking;
        public bool IsComboAttacking => isComboAttacking;
        public bool IsParrying => parryWindowActive;
        public bool HasParryWindow => parryWindowActive && Time.unscaledTime < parryWindowEndTime;

        private void Awake()
        {
            // attackPoint가 없으면 자기 자신 사용
            if (attackPoint == null)
                attackPoint = transform;

            if (animator == null)
                animator = GetComponent<Animator>();
            CacheClipLengths();

            // CombatEvents 구독
            CombatEvents.OnEnemyAttackTelegraph += OnEnemyTelegraph;
        }

        // 공격 지속 = 클립 전체 재생 시간 (attackDuration 폴백)
        private void CacheClipLengths()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
                return;

            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == "PlayerAttack") attackClipLength = clip.length;
                else if (clip.name == "PlayerComboAttack") comboAttackClipLength = clip.length;
            }

            if (attackClipLength <= 0f) attackClipLength = attackDuration;
            if (comboAttackClipLength <= 0f) comboAttackClipLength = attackDuration;
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
        }

        private void HandleAttackInput()
        {
            if (Input.GetButtonDown("Fire1") && CanAttack())
            {
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

            OnParrySuccess(telegraphedEnemy, isPerfect);

            // 패링 성공 시 윈도우 종료
            parryWindowActive = false;
            telegraphedEnemy = null;
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

        // 디버그용 기즈모
        private void OnDrawGizmosSelected()
        {
            if (attackPoint != null)
            {
                Vector2 pos = (Vector2)attackPoint.position + attackOffset * Mathf.Sign(transform.localScale.x);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(pos, attackRange);
            }
        }
    }
}