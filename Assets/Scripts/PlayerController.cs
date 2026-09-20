using System.Collections;
using UnityEngine;

namespace EpsilonGame
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 2f;              // guide: 걷기 직접 결정 (추천 1.5~2)
        [SerializeField] private float airMoveSpeed = 2.5f;         // guide: 공중 이동 2.3~2.8

        [Header("Jump (거리 = 최고 도달 높이, v=√(2·g·h) 자동 계산)")]
        [SerializeField] private float jumpHeight = 3f;             // 한 번 점프 최고 상승 높이 (world units)
        [SerializeField] private float doubleJumpHeight = 2f;       // 이단 점프 추가 상승 높이 (world units)
        [SerializeField] private float maxJumps = 2f;

        [Header("Dash (거리 = 총 전진 거리, 위상별 속도 자동 스케일)")]
        [SerializeField] private float dashDistance = 6f;           // 대시 총 이동 거리 (기존 3.23 → 6)
        [SerializeField] private float dashCrouchTime = 0.14f;
        [SerializeField] private float dashLeapTime = 0.07f;
        [SerializeField] private float dashFastTime = 0.21f;
        [SerializeField] private float dashLeapSpeed = 10.86f;      // 0.07s에 앞 0.76·위 0.76
        [SerializeField] private float dashFastSpeed = 8.62f;       // 0.21s에 -1.81
        [SerializeField] private float dashDecelSpeed = 1.57f;      // 0.42s에 -0.66 → 합계 -3.23
        [SerializeField] private float dashDuration = 0.84f;
        [SerializeField] private float dashCooldown = 0.6f;

        [Header("Parry Retreat (guide: 뒤로 +1.47 = 0.04s@8.5 + 0.35s@3.2)")]
        [SerializeField] private float parryRetreatFastTime = 0.04f;
        [SerializeField] private float parryRetreatFastSpeed = 8.5f;
        [SerializeField] private float parryRetreatSlowSpeed = 3.2f;
        [SerializeField] private float parryRetreatDuration = 0.39f;

        [Header("Combo Dash (guide: 앞으로 -1.58 = 0.20s@6.85 + 0.04s@5.25)")]
        [SerializeField] private float comboDashSpeed = 6.85f;
        [SerializeField] private float comboDashFastTime = 0.20f;
        [SerializeField] private float comboDashEndSpeed = 5.25f;
        [SerializeField] private float comboDashDuration = 0.24f;

        [Header("Ground Check")]
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        [Header("VFX (commit FX 위치/타이밍 guide 준수, world-fixed)")]
        [SerializeField] private GameObject doubleJumpFx;
        [SerializeField] private GameObject dashFx;
        [SerializeField] private GameObject parryFx;
        [SerializeField] private GameObject comboAttackFx;
        [SerializeField] private GameObject comboAttackLine;
        [SerializeField] private float dashFxDelay = 0.42f;           // dash_06
        [SerializeField] private float doubleJumpFxDelay = 0.07f;     // dj_01
        [SerializeField] private float parryFxDelay = 0.04f;          // parrying_01
        [SerializeField] private float comboAttackFxDelay = 0f;       // combo_attack2_00 즉시
        [SerializeField] private float comboAttackLineDelay = 0.20f;  // combo_attack2_05

        private Rigidbody2D rb;
        private Collider2D col2d;
        private float moveInput;
        private int jumpsRemaining;
        private bool isGrounded;
        private PlayerHealth playerHealth;
        private PlayerCombat playerCombat;

        private bool isDashing;
        private float dashDirection = 1f;
        private float dashStartTime;
        private float lastDashTime = -999f;
        private float defaultGravityScale = 1f;
        private float dashSpeedScale = 1f;
        private float jumpVelocity;
        private float doubleJumpVelocity;

        // 파링/콤보 모션 상태 (IsParrying은 PlayerCombat에서 파링 모션 기반으로 재정의됨)
        private bool wasParrying;
        private bool parryRetreatActive;
        private float parryRetreatStartTime;
        private bool wasComboAttacking;
        private bool comboDashActive;
        private float comboDashStartTime;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col2d = GetComponent<Collider2D>();
            if (animator == null) animator = GetComponent<Animator>();
            playerHealth = GetComponent<PlayerHealth>();
            playerCombat = GetComponent<PlayerCombat>();
            defaultGravityScale = rb.gravityScale;

            // 점프: v = sqrt(2·g·h) (g = 중력 × gravityScale) — 목표 높이에서 초기 속도 계산
            float g = Mathf.Abs(Physics2D.gravity.y) * defaultGravityScale;
            jumpVelocity = Mathf.Sqrt(2f * g * Mathf.Max(0f, jumpHeight));
            doubleJumpVelocity = Mathf.Sqrt(2f * g * Mathf.Max(0f, doubleJumpHeight));

            // 시작 시점 점프 횟수 초기화 (스폰 직후에도 즉시 점프 가능, 착지 시 CheckGrounded가 리셋)
            jumpsRemaining = (int)maxJumps;

            // 대시: 위상별 거리 합 대비 dashDistance 비율로 속도 스케일 (시간 구조 유지)
            float baseDashDistance = dashLeapSpeed * dashLeapTime
                + dashFastSpeed * dashFastTime
                + dashDecelSpeed * Mathf.Max(0f, dashDuration - dashCrouchTime - dashLeapTime - dashFastTime);
            dashSpeedScale = baseDashDistance > 0f ? dashDistance / baseDashDistance : 1f;
        }

        // Input 읽기는 Update에서 (render rate, 카메라 끊김 방지!)
        private void Update()
        {
            bool isMotionLocked = playerCombat != null
                && (playerCombat.IsAttacking || playerCombat.IsComboAttacking || playerCombat.IsParrying);

            if (isMotionLocked || (playerHealth != null && playerHealth.IsInKnockback))
            {
                moveInput = 0f;
            }
            else
            {
                moveInput = Input.GetAxisRaw("Horizontal");

                if (Input.GetKeyDown(KeyCode.Q) && !isDashing && Time.time >= lastDashTime + dashCooldown)
                    StartDash();
            }

            // 더블점프 보류: 점프 횟수가 가득 찬 상태(땅 or 낙하 직후)에서만 1회 발동
            if (!isMotionLocked && !isDashing && Input.GetButtonDown("Jump") && jumpsRemaining >= (int)maxJumps)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
                jumpsRemaining--;
            }

            UpdateMotionTriggers();

            if (isDashing && Time.time >= dashStartTime + dashDuration)
                EndDash();

            // Animator 파라미터 갱신
            if (animator != null)
            {
                animator.SetFloat("Speed", Mathf.Abs(moveInput));
                animator.SetBool("IsGrounded", isGrounded);
                animator.SetBool("IsFalling", !isGrounded && rb.linearVelocity.y < 0f);
                animator.SetBool("IsDashing", isDashing);
                animator.SetInteger("JumpCount", (int)maxJumps - jumpsRemaining);
                if (playerHealth != null)
                    animator.SetBool("IsHurt", playerHealth.IsInKnockback);
            }

            // 좌우 반전 (localScale 방식 — PlayerCombat.cs 방향 판정 호환)
            // 스프라이트 원본이 왼쪽을 보므로 오른쪽 이동 시 scale.x = -1 (좌우 반전)
            // 기존 scale 크기 보존 (y/z는 증폭하지 않음)
            if (moveInput != 0f)
            {
                Vector3 s = transform.localScale;
                transform.localScale = new Vector3(Mathf.Abs(s.x) * -Mathf.Sign(moveInput), s.y, s.z);
            }
        }

        // 파링 후퇴 / 콤보 돌진 시작 검출 (PlayerCombat 상태 상승 에지)
        private void UpdateMotionTriggers()
        {
            bool isParrying = playerCombat != null && playerCombat.IsParrying;
            if (isParrying && !wasParrying)
            {
                parryRetreatActive = true;
                parryRetreatStartTime = Time.time;
                if (parryFx != null)
                    StartCoroutine(PlayFxDelayed(parryFx, parryFxDelay));
            }
            else if (!isParrying)
            {
                parryRetreatActive = false;
            }
            wasParrying = isParrying;

            bool isComboAttacking = playerCombat != null && playerCombat.IsComboAttacking;
            if (isComboAttacking && !wasComboAttacking)
            {
                // 그랩 콤보 발동 시 파링 후퇴 중단 → 콤보 돌진 우선
                parryRetreatActive = false;
                comboDashActive = true;
                comboDashStartTime = Time.time;
                if (comboAttackFx != null)
                    StartCoroutine(PlayFxDelayed(comboAttackFx, comboAttackFxDelay));
                if (comboAttackLine != null)
                    StartCoroutine(PlayFxDelayed(comboAttackLine, comboAttackLineDelay));
            }
            else if (!isComboAttacking)
            {
                comboDashActive = false;
            }
            wasComboAttacking = isComboAttacking;
        }

        // Velocity 설정은 FixedUpdate에서 (physics rate)
        private void FixedUpdate()
        {
            CheckGrounded();

            if (playerHealth != null && playerHealth.IsInKnockback)
                return;

            if (isDashing)
            {
                ApplyDashVelocity();
                return;
            }

            if (parryRetreatActive)
            {
                ApplyParryRetreatVelocity();
                return;
            }

            if (comboDashActive)
            {
                ApplyComboDashVelocity();
                return;
            }

            float speed = isGrounded ? moveSpeed : airMoveSpeed;
            rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
        }

        private void StartDash()
        {
            isDashing = true;
            lastDashTime = Time.time;
            dashStartTime = Time.time;
            dashDirection = -Mathf.Sign(transform.localScale.x);
            rb.gravityScale = 0f;

            if (dashFx != null)
                StartCoroutine(PlayFxDelayed(dashFx, dashFxDelay));
        }

        private void ApplyDashVelocity()
        {
            float t = Time.time - dashStartTime;
            float dir = dashDirection;

            if (t < dashCrouchTime)
            {
                rb.linearVelocity = Vector2.zero; // 웅크림 준비 — 이동 없음
            }
            else if (t < dashCrouchTime + dashLeapTime)
            {
                rb.linearVelocity = new Vector2(dashLeapSpeed * dashSpeedScale * dir, 0f);
            }
            else if (t < dashCrouchTime + dashLeapTime + dashFastTime)
            {
                rb.linearVelocity = new Vector2(dashFastSpeed * dashSpeedScale * dir, 0f); // 고속 전진, 높이 유지
            }
            else
            {
                rb.linearVelocity = new Vector2(dashDecelSpeed * dashSpeedScale * dir, 0f); // 감속
            }
        }

        private void EndDash()
        {
            isDashing = false;
            rb.gravityScale = defaultGravityScale;
        }

        // 파링 후퇴: 진행 방향 반대쪽으로 guide 수치만큼 밀림 (0.39s 후 정지)
        private void ApplyParryRetreatVelocity()
        {
            float t = Time.time - parryRetreatStartTime;
            float dir = Mathf.Sign(transform.localScale.x);

            float vx = 0f;
            if (t < parryRetreatFastTime)
                vx = parryRetreatFastSpeed * dir;
            else if (t < parryRetreatDuration)
                vx = parryRetreatSlowSpeed * dir;

            rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
        }

        // 콤보2 돌진: 진행 방향으로 0.24s 전진 후 정지 (guide: -1.58 총합)
        private void ApplyComboDashVelocity()
        {
            float t = Time.time - comboDashStartTime;
            float dir = -Mathf.Sign(transform.localScale.x);

            float vx = 0f;
            if (t < comboDashFastTime)
                vx = comboDashSpeed * dir;
            else if (t < comboDashDuration)
                vx = comboDashEndSpeed * dir;

            rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
        }

        private IEnumerator PlayFxDelayed(GameObject fx, float delay)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);
            PlayOneShotFx(fx, true);
        }

        // Facing-driven child FX: keep the authored local offset (README: offsets are authored
        // facing LEFT; parent flip mirrors them to the correct side for the current facing).
        private void PlayOneShotFx(GameObject fx, bool stayInWorld)
        {
            if (fx == null) return;

            Transform originalParent = null;
            Vector3 originalLocalPos = Vector3.zero;
            if (stayInWorld)
            {
                originalParent = fx.transform.parent;
                originalLocalPos = fx.transform.localPosition;
                fx.transform.SetParent(null, true);
            }

            fx.SetActive(true);
            var anim = fx.GetComponent<Animator>();
            if (anim != null) anim.Play("Play", 0, 0f);
            StartCoroutine(HideFxAfter(fx, anim, stayInWorld, originalParent, originalLocalPos));
        }

        private IEnumerator HideFxAfter(GameObject fx, Animator anim, bool stayInWorld, Transform parent, Vector3 localPos)
        {
            float length = 0.35f;
            if (anim != null && anim.runtimeAnimatorController != null
                && anim.runtimeAnimatorController.animationClips.Length > 0)
                length = anim.runtimeAnimatorController.animationClips[0].length;

            yield return new WaitForSeconds(length);
            fx.SetActive(false);
            if (stayInWorld && parent != null)
            {
                fx.transform.SetParent(parent, true);
                fx.transform.localPosition = localPos;
            }
        }

        private void CheckGrounded()
        {
            if (col2d == null) return;

            // 씬 인스턴스의 콜라이더 오버라이드(크기/오프셋)와 무관하게 실측 콜라이더 하단을 사용한다
            Vector2 checkOrigin = new Vector2(col2d.bounds.center.x, col2d.bounds.min.y);
            isGrounded = Physics2D.OverlapCircle(checkOrigin, groundCheckRadius, groundLayer);
            if (isGrounded)
            {
                jumpsRemaining = (int)maxJumps;
            }
        }

        public bool IsGrounded => isGrounded;
        public float MoveInput => moveInput;
    }
}