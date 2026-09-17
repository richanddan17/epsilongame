using UnityEngine;

namespace EpsilonGame
{
    /// <summary>
    /// 전투 상태 관리 + 슬로우 모션 연출 (싱글턴)
    /// 적 텔레그래프 + 플레이어 공격 둘 다 슬로우 모션 트리거
    /// </summary>
    public class CombatDirector : MonoBehaviour
    {
        [Header("Slow Motion")]
        [SerializeField] private float slowMotionScale = 0.3f;
        [SerializeField] private float slowMotionDuration = 0.4f;
        [SerializeField] private float playerAttackSlowMotionDuration = 0.2f;

        [Header("Combat State")]
        [SerializeField] private GameObject currentEnemy;
        [SerializeField] private bool isInCombat = false;

        private float slowMotionEndTime = -999f;
        private float noEnemyDetectedTime = -999f;
        private const float CombatEndDelay = 3f;

        public static CombatDirector Instance { get; private set; }
        public GameObject CurrentEnemy => currentEnemy;
        public bool IsInCombat => isInCombat;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            CombatEvents.OnEnemyAttackTelegraph += HandleAttackTelegraph;
            CombatEvents.OnPlayerAttack += HandlePlayerAttack;
        }

        private void OnDisable()
        {
            CombatEvents.OnEnemyAttackTelegraph -= HandleAttackTelegraph;
            CombatEvents.OnPlayerAttack -= HandlePlayerAttack;
        }

        private void Update()
        {
            UpdateSlowMotion();
            UpdateCombatState();
        }

        private void HandleAttackTelegraph(GameObject enemy)
        {
            if (enemy == null) return;

            currentEnemy = enemy;
            EnterCombat(enemy);
            TriggerSlowMotion(slowMotionDuration);
        }

        private void HandlePlayerAttack()
        {
            TriggerSlowMotion(playerAttackSlowMotionDuration);
        }

        private void TriggerSlowMotion(float duration)
        {
            Time.timeScale = slowMotionScale;
            slowMotionEndTime = Time.unscaledTime + duration;
        }

        private void UpdateSlowMotion()
        {
            if (Time.timeScale < 1f && Time.unscaledTime >= slowMotionEndTime)
            {
                Time.timeScale = 1f;
            }
        }

        private void EnterCombat(GameObject enemy)
        {
            if (!isInCombat)
            {
                isInCombat = true;
                OnCombatStart(enemy);
            }
            noEnemyDetectedTime = -999f;
        }

        private void UpdateCombatState()
        {
            if (!isInCombat) return;

            if (currentEnemy == null)
            {
                if (noEnemyDetectedTime < 0f)
                {
                    noEnemyDetectedTime = Time.unscaledTime;
                }
                else if (Time.unscaledTime >= noEnemyDetectedTime + CombatEndDelay)
                {
                    ExitCombat();
                }
            }
            else
            {
                noEnemyDetectedTime = -999f;
            }
        }

        private void ExitCombat()
        {
            isInCombat = false;
            currentEnemy = null;
            OnCombatEnd();
        }

        private void OnCombatStart(GameObject enemy)
        {
            // 카메라 확대 등 연출은 CombatCamera.cs가 구독하여 처리
        }

        private void OnCombatEnd()
        {
            // 카메라 복귀 등 연출은 CombatCamera.cs가 구독하여 처리
        }
    }
}