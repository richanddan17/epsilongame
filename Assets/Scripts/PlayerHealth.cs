using UnityEngine;
using System;

namespace EpsilonGame
{
    /// <summary>
    /// 플레이어 체력/데미지/넉백/사망 처리.
    /// IEpsilonDamagable 구현으로 전투 시스템과 결합도 낮춤.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerHealth : MonoBehaviour, IEpsilonDamagable
    {
        [Header("Health")]
        [SerializeField] private int maxHealth = 5;
        [SerializeField] private float invincibleTime = 0.5f;
        [SerializeField] private float knockbackForce = 8f;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private int currentHealth;
        private bool isDead;
        private bool isInvincible;
        private bool isComboInvincible;
        private float invincibleTimer;
        private float knockbackTimer;
        private Color originalColor;

        public float knockbackDuration = 0.2f;
        public bool IsInKnockback => knockbackTimer > 0f;

        public int CurrentHealth => currentHealth;
        public bool IsDead => isDead;
        public int MaxHealth => maxHealth;

        /// <summary>
        /// 체력 변경 이벤트 (current, max) — UI 갱신용
        /// </summary>
        public static event Action<int, int> OnHealthChanged;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            currentHealth = maxHealth;
            originalColor = spriteRenderer.color;
        }

        private void OnEnable()
        {
            CombatEvents.OnPlayerRespawn += ResetHealth;
        }

        private void OnDisable()
        {
            CombatEvents.OnPlayerRespawn -= ResetHealth;
        }

        private void Update()
        {
            if (isInvincible)
            {
                invincibleTimer -= Time.deltaTime;
                if (invincibleTimer <= 0f)
                {
                    EndInvincibility();
                }
            }

            if (knockbackTimer > 0f)
            {
                knockbackTimer -= Time.deltaTime;
            }
        }

        /// <summary>
        /// 그랩/콤보 연출 중 강제 무적 설정 (타이머 없이, PlayerCombat이 해제)
        /// </summary>
        public void SetComboInvincible(bool state)
        {
            isComboInvincible = state;
        }

        public void TakeDamage(int amount, Vector2 direction, float knockback)
        {
            if (isDead || isInvincible || isComboInvincible) return;

            currentHealth = Mathf.Max(0, currentHealth - amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            StartInvincibility();

            // 넉백 적용 (호출자가 0을 넘기면 직렬화된 기본값 사용)
            Vector2 knockbackDir = direction.normalized;
            float force = knockback > 0f ? knockback : knockbackForce;
            rb.linearVelocity = knockbackDir * force;
            knockbackTimer = knockbackDuration;

            // 사망 처리
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void StartInvincibility()
        {
            isInvincible = true;
            invincibleTimer = invincibleTime;
            // 깜빡임 시작
            InvokeRepeating(nameof(ToggleSprite), 0f, 0.1f);
        }

        private void EndInvincibility()
        {
            isInvincible = false;
            CancelInvoke(nameof(ToggleSprite));
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;
        }

        private void ToggleSprite()
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
        }

        private void Die()
        {
            isDead = true;
            CancelInvoke(nameof(ToggleSprite));
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;

            // GameManager 싱글턴 통해 사망 알림 (Wave 5에서 생성, null 안전)
            GameManager.Instance?.PlayerDied();
        }

        /// <summary>
        /// 체력 회복 (리스폰/힐링용)
        /// </summary>
        public void Heal(int amount)
        {
            if (isDead) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// 최대 체력으로 리셋 (리스폰용)
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            isDead = false;
            isInvincible = false;
            CancelInvoke(nameof(ToggleSprite));
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}