using UnityEngine;

namespace EpsilonGame
{
    /// <summary>
    /// 적 AI 상태 머신 (Wave 3-1).
    /// 모든 외부 에셋 참조는 [SerializeField] private 슬롯으로만 — placeholder는 컬러 사각형 (.omo/notes/asset-slots.md 참조).
    /// CombatDirector 직접 참조 금지 — CombatEvents 허브만 사용.
    /// EnemyHealth가 Enemy를 참조하는 단방향 구조 — 순환 참조 방지 위해 EnemyHealth 타입 미참조.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Enemy : MonoBehaviour
    {
        // ===== State Machine =====
        private enum EnemyState
        {
            Idle,
            Detected,
            AttackTelegraph,
            Attacking,
            Parried,
            Penalty,
            Hurt,
            Dead
        }

        [Header("State")]
        [SerializeField] private EnemyState currentState = EnemyState.Idle;

        // ===== Detection =====
        [Header("Detection")]
        [SerializeField] private float detectRadius = 5f;
        [SerializeField] private LayerMask playerLayer = 1 << 9; // Player 레이어 = index 9
        [SerializeField] private float lostPlayerTimeout = 3f;

        private Transform playerTransform;
        private float lostPlayerTimer;

        // ===== Attack =====
        [Header("Attack")]
        [SerializeField] private int attackDamage = 1;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private Vector2 attackOffset = new Vector2(1f, 0f);
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private float telegraphTime = 0.5f;

        private Vector2 baseAttackOffset;
        private float attackCooldownTimer;
        private float telegraphTimer;
        private bool hasAttackHitFired;

        // ===== Movement =====
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 1.5f;

        private int facing = 1; // 1 = 오른쪽, -1 = 왼쪽

        // ===== Stun / Penalty =====
        [Header("Stun / Penalty")]
        [SerializeField] private float stunTime = 1f;
        [SerializeField] private float perfectParryStunMultiplier = 2f;

        private float stunTimer;

        // ===== Health / Death =====
        [Header("Health")]
        [SerializeField] private int maxHealth = 3;

        private int currentHealth;
        private bool isDead;

        // ===== Components =====
        private Rigidbody2D rb;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;

        // ===== Public API =====
        public bool IsDead => isDead;

        // ===== Unity Lifecycle =====
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalColor = spriteRenderer.color;
            currentHealth = maxHealth;
            baseAttackOffset = attackOffset;
        }

        private void OnEnable()
        {
            CombatEvents.OnPlayerRespawn += ResetForRespawn;
        }

        private void OnDisable()
        {
            CombatEvents.OnPlayerRespawn -= ResetForRespawn;
        }

        private void Update()
        {
            // 타이머는 Time.unscaledDeltaTime 사용 (슬로우 모션 대비 — CombatDirector가 timeScale 0.3 적용 예정)
            float dt = Time.unscaledDeltaTime;

            // 공격 쿨다운 타이머
            if (attackCooldownTimer > 0f)
            {
                attackCooldownTimer -= dt;
            }

            // 스턴 타이머
            if (stunTimer > 0f)
            {
                stunTimer -= dt;
                if (stunTimer <= 0f && (currentState == EnemyState.Parried || currentState == EnemyState.Penalty || currentState == EnemyState.Hurt))
                {
                    TransitionToIdle();
                }
            }

            // 플레이어 미감지 타이머 (Detected 상태에서만)
            if (currentState == EnemyState.Detected)
            {
                lostPlayerTimer -= dt;
                if (lostPlayerTimer <= 0f)
                {
                    TransitionToIdle();
                }
            }

            // 상태 머신 업데이트
            UpdateStateMachine(dt);
        }

        private void FixedUpdate()
        {
            if (rb == null) return;

            // Detected 상태에서 플레이어를 향해 걸어감 (Idle/그 외 상태는 정지)
            float horizontal = 0f;
            if (currentState == EnemyState.Detected && playerTransform != null)
            {
                float dir = Mathf.Sign(playerTransform.position.x - transform.position.x);
                facing = dir >= 0f ? 1 : -1;
                horizontal = dir * moveSpeed;
            }

            rb.linearVelocity = new Vector2(horizontal, rb.linearVelocity.y);

            if (Mathf.Abs(horizontal) > 0.01f)
            {
                Vector3 s = transform.localScale;
                transform.localScale = new Vector3(Mathf.Abs(s.x) * facing, s.y, s.z);
            }

            if (animator != null)
            {
                animator.SetBool("IsMoving", Mathf.Abs(horizontal) > 0.01f);
            }
        }

        // ===== State Machine Logic =====
        private void UpdateStateMachine(float dt)
        {
            switch (currentState)
            {
                case EnemyState.Idle:
                    UpdateIdleState();
                    break;

                case EnemyState.Detected:
                    UpdateDetectedState();
                    break;

                case EnemyState.AttackTelegraph:
                    UpdateAttackTelegraphState(dt);
                    break;

                case EnemyState.Attacking:
                    UpdateAttackingState();
                    break;

                case EnemyState.Parried:
                case EnemyState.Penalty:
                case EnemyState.Hurt:
                    // 스턴 타이머로 처리됨 (Update에서)
                    break;

                case EnemyState.Dead:
                    // 사망 상태 유지
                    break;
            }
        }

        private void UpdateIdleState()
        {
            // 플레이어 감지 체크
            Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectRadius, playerLayer);
            if (playerCollider != null)
            {
                playerTransform = playerCollider.transform;
                TransitionToDetected();
            }
        }

        private void UpdateDetectedState()
        {
            // 플레이어가 여전히 감지 범위 내에 있는지 확인
            if (playerTransform != null)
            {
                float dist = Vector2.Distance(transform.position, playerTransform.position);
                if (dist > detectRadius)
                {
                    playerTransform = null;
                    lostPlayerTimer = lostPlayerTimeout;
                }
                else
                {
                    lostPlayerTimer = lostPlayerTimeout; // 감지되면 타이머 리셋
                }
            }

            // 공격 쿨다운 확인
            if (attackCooldownTimer <= 0f)
            {
                TransitionToAttackTelegraph();
            }
        }

        private void UpdateAttackTelegraphState(float dt)
        {
            telegraphTimer -= dt;
            if (telegraphTimer <= 0f)
            {
                TransitionToAttacking();
            }
        }

        private void UpdateAttackingState()
        {
            // 공격 판정: Physics2D.OverlapCircle로 Player 레이어(7) 감지
            Vector2 attackCenter = (Vector2)transform.position + baseAttackOffset * facing;
            Collider2D hit = Physics2D.OverlapCircle(attackCenter, attackRange, playerLayer);

            if (!hasAttackHitFired)
            {
                hasAttackHitFired = true;

                if (hit != null)
                {
                    // 이벤트 발화: 공격 판정 순간
                    CombatEvents.RaiseOnEnemyAttackHit(gameObject);

                    // PlayerHealth.TakeDamage 호출 (IEpsilonDamagable 구현체)
                    var damagable = hit.GetComponent<IEpsilonDamagable>();
                    if (damagable != null)
                    {
                        Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                        damagable.TakeDamage(attackDamage, knockbackDir, 8f);
                    }
                }
            }

            // 공격 동작 완료 → Idle 복귀 (빗나가도 진행) 후 Detected에서 추적 재개
            attackCooldownTimer = attackCooldown;
            TransitionToIdle();
        }

        // ===== State Transitions =====
        private void TransitionToIdle()
        {
            currentState = EnemyState.Idle;
            hasAttackHitFired = false;
        }

        private void TransitionToDetected()
        {
            currentState = EnemyState.Detected;
            lostPlayerTimer = lostPlayerTimeout;
        }

        private void TransitionToAttackTelegraph()
        {
            currentState = EnemyState.AttackTelegraph;
            telegraphTimer = telegraphTime;
            hasAttackHitFired = false;

            // 이벤트 발화: 공격 준비 시작 (슬로우 모션/카메라 확대 트리거용)
            CombatEvents.RaiseOnEnemyAttackTelegraph(gameObject);
        }

        private void TransitionToAttacking()
        {
            currentState = EnemyState.Attacking;
            hasAttackHitFired = false;
        }

        private void TransitionToParried(float stunDuration)
        {
            currentState = EnemyState.Parried;
            stunTimer = stunDuration;
        }

        private void TransitionToPenalty(float stunDuration)
        {
            currentState = EnemyState.Penalty;
            stunTimer = stunDuration;
        }

        private void TransitionToHurt(float stunDuration)
        {
            currentState = EnemyState.Hurt;
            stunTimer = stunDuration;
        }

        private void TransitionToDead()
        {
            currentState = EnemyState.Dead;
            isDead = true;
            // 사망 처리 (비활성화 등)
            gameObject.SetActive(false);
        }

        // ===== Public API (EnemyHealth/PlayerCombat에서 호출) =====
        /// <summary>
        /// 피격 상태 진입 — EnemyHealth.cs가 호출 예정 (현재 주석 처리됨, Todo 3-2에서 복원)
        /// </summary>
        public void SetHurtState()
        {
            if (isDead) return;
            TransitionToHurt(stunTime);
        }

        /// <summary>
        /// 패링 성공 처리 — PlayerCombat.cs가 호출 예정
        /// </summary>
        /// <param name="enemy">패링당한 적 (this)</param>
        /// <param name="isPerfect">완벽 패링 여부</param>
        public void OnParrySuccess(GameObject enemy, bool isPerfect)
        {
            if (enemy != gameObject) return;
            if (isDead) return;

            float stunDuration = isPerfect ? stunTime * perfectParryStunMultiplier : stunTime;

            if (isPerfect)
            {
                TransitionToPenalty(stunDuration);
                ApplyPenalty(); // 완벽 패링 시 확장 훅
            }
            else
            {
                TransitionToParried(stunDuration);
            }
        }

        /// <summary>
        /// 완벽 패링 시 확장 훅 (기본: stun 2배, 주석으로 확장 지점 표시)
        /// TODO: 향후 추가 패널티 로직 구현 시 여기에 작성 (예: 추가 넉백, 상태이상, 점수 보너스 등)
        /// </summary>
        protected virtual void ApplyPenalty()
        {
            // 기본 구현: stunTime이 이미 2배로 설정되어 TransitionToPenalty에서 처리됨
            // 확장 지점: 추가 패널티 로직 여기에 작성
        }

        /// <summary>
        /// 리스폰 시 상태 리셋
        /// </summary>
        private void ResetForRespawn()
        {
            currentState = EnemyState.Idle;
            currentHealth = maxHealth;
            isDead = false;
            attackCooldownTimer = 0f;
            telegraphTimer = 0f;
            stunTimer = 0f;
            lostPlayerTimer = 0f;
            hasAttackHitFired = false;
            playerTransform = null;

            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
            }

            // 시각적 복원
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.color = originalColor;
            }

            gameObject.SetActive(true);
        }

        // ===== Gizmos (디버그용) =====
        private void OnDrawGizmosSelected()
        {
            // 감지 범위
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectRadius);

            // 공격 범위
            Gizmos.color = Color.red;
            Vector2 attackCenter = (Vector2)transform.position + baseAttackOffset * facing;
            Gizmos.DrawWireSphere(attackCenter, attackRange);
        }
    }
}