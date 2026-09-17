using UnityEngine;

namespace EpsilonGame
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float jumpForce = 14f;
        [SerializeField] private float maxJumps = 2f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        private Rigidbody2D rb;
        private float moveInput;
        private int jumpsRemaining;
        private bool isGrounded;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (animator == null) animator = GetComponent<Animator>();
        }

        // Input 읽기는 Update에서 (render rate, 카메라 끊김 방지!)
        private void Update()
        {
            moveInput = Input.GetAxisRaw("Horizontal");

            if (Input.GetButtonDown("Jump") && jumpsRemaining > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                jumpsRemaining--;
            }

            // Animator 파라미터 갱신
            if (animator != null)
            {
                animator.SetFloat("Speed", Mathf.Abs(moveInput));
                animator.SetBool("IsGrounded", isGrounded);
            }

            // 좌우 반전 (localScale 방식 — PlayerCombat.cs 방향 판정 호환)
            if (moveInput != 0f)
                transform.localScale = new Vector3(Mathf.Sign(moveInput), 1f, 1f);
        }

        // Velocity 설정은 FixedUpdate에서 (physics rate)
        private void FixedUpdate()
        {
            CheckGrounded();

            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }

        private void CheckGrounded()
        {
            if (groundCheck == null) return;

            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            if (isGrounded)
            {
                jumpsRemaining = (int)maxJumps;
            }
        }

        public bool IsGrounded => isGrounded;
        public float MoveInput => moveInput;
    }
}