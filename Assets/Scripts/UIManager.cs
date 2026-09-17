using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace EpsilonGame
{
    /// <summary>
    /// UI 매니저 — 체력 바, 게임오버/클리어 화면 관리 (싱글턴)
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Image hpFillImage;          // HP 바 fill — HP_HUD.png 슬라이스 후 할당 예정
        [SerializeField] private Text hpText;                // HP 텍스트 — 임시
        [SerializeField] private GameObject gameOverPanel;   // GameOver 텍스트/패널 — 처음 inactive
        [SerializeField] private GameObject clearPanel;      // Clear 텍스트/패널 — 처음 inactive

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 패널 비활성화 확인
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
            if (clearPanel != null)
                clearPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void OnEnable()
        {
            // PlayerHealth 이벤트 구독
            PlayerHealth.OnHealthChanged += UpdateHealthUI;
        }

        private void OnDisable()
        {
            // PlayerHealth 이벤트 구독 해제
            PlayerHealth.OnHealthChanged -= UpdateHealthUI;
        }

        private void Update()
        {
            // R키로 전체 재시작 (GameOver/Clear 상태에서만)
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (GameManager.Instance != null &&
                    (GameManager.Instance.CurrentState == GameManager.GameState.GameOver ||
                     GameManager.Instance.CurrentState == GameManager.GameState.Clear))
                {
                    RestartGame();
                }
            }
        }

        /// <summary>
        /// HP 바 fillAmount 및 텍스트 갱신
        /// </summary>
        public void UpdateHealthUI(int current, int max)
        {
            if (hpFillImage != null)
            {
                hpFillImage.fillAmount = max > 0 ? (float)current / max : 0f;
            }

            if (hpText != null)
            {
                hpText.text = $"{current} / {max}";
            }
        }

        /// <summary>
        /// 게임오버 패널 표시 + 시간 정지
        /// </summary>
        public void ShowGameOver()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            Time.timeScale = 0f;
        }

        /// <summary>
        /// 클리어 패널 표시
        /// </summary>
        public void ShowClear()
        {
            if (clearPanel != null)
                clearPanel.SetActive(true);
        }

        /// <summary>
        /// 전체 게임 재시작 (SceneManager.LoadScene) — 체크포인트 리스폰과 구분
        /// </summary>
        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}