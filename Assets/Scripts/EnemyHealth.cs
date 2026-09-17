using UnityEngine;

namespace EpsilonGame
{
    /// <summary>
    /// 적 체력/데미지/넉백/사망 처리.
    /// IEpsilonDamagable 구현으로 전투 시스템과 결합도 낮춤.
    /// Enemy.cs의 상태 머신과 연동하되, 체력/넉백/사망만 담당.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class EnemyHealth : MonoBehaviour, IEpsilonDamagable
    {
        [Header("Health")]
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private float knockbackForce = 6f;
        [SerializeField] private float deathVanishTime = 0.5f;

        private Rigidbody2D rb;
        private Collider2D col;
        private int currentHealth;
        private bool isDead;
        private float deathTimer;

        public int CurrentHealth => currentHealth;
        public bool IsDead => isDead;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            currentHealth = maxHealth;
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
            if (isDead)
            {
                deathTimer -= Time.unscaledDeltaTime;
                if (deathTimer <= 0f)
                {
                    Destroy(gameObject);
                }
            }
        }

        public void TakeDamage(int amount, Vector2 direction, float knockback)
        {
            if (isDead) return;

            currentHealth = Mathf.Max(0, currentHealth - amount);

            // 넉백 적용 (호출자가 0을 넘기면 직렬화된 기본값 사용)
            Vector2 knockbackDir = direction.normalized;
            float force = knockback > 0f ? knockback : knockbackForce;
            rb.linearVelocity = knockbackDir * force;

            // Hurt 상태 요청 — Enemy.cs 상태 머신 연동
            var enemy = GetComponent<Enemy>();
            enemy?.SetHurtState();

            // 사망 처리
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            isDead = true;
            deathTimer = deathVanishTime;

            // 콜라이더 비활성화 (추가 충돌 방지)
            if (col != null)
            {
                col.enabled = false;
            }

            // Rigidbody 정지 (밀리지 않게)
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        /// <summary>
        /// 리스폰 시 체력/상태 초기화
        /// </summary>
        public void ResetForRespawn()
        {
            currentHealth = maxHealth;
            isDead = false;

            // 콜라이더 재활성화
            if (col != null)
            {
                col.enabled = true;
            }

            // Rigidbody 복구
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
        }
    }
}