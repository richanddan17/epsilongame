using UnityEngine;

namespace EpsilonGame
{
    /// <summary>
    /// 게임 매니저 싱글턴 — 게임 상태/리스폰/체크포인트/클리어 관리
    /// SceneManager.LoadScene 미사용 — 위치 이동만으로 리스폰 처리
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        /// <summary>게임 상태</summary>
        public enum GameState
        {
            Playing,
            GameOver,
            Clear
        }

        [Header("Checkpoint")]
        [SerializeField] private Vector2 _checkpointPosition = Vector2.zero;
        public Vector2 CheckpointPosition => _checkpointPosition;

        [Header("References")]
        [Tooltip("씬의 시작 위치 (PlayerStart 빈 GameObject). Todo 6-1에서 생성 예정.")]
        [SerializeField] private Transform _playerStart;

        [Header("Respawn Settings")]
        [SerializeField] private float _respawnDelay = 1.5f;

        private GameState _currentState = GameState.Playing;
        public GameState CurrentState => _currentState;

        private bool _isRespawning = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // PlayerStart가 인스펙터에서 할당되지 않았다면 이름으로 찾기 시도
            if (_playerStart == null)
            {
                var startObj = GameObject.Find("PlayerStart");
                if (startObj != null)
                    _playerStart = startObj.transform;
            }

            // 시작 위치를 기본 체크포인트로 설정
            if (_playerStart != null)
                _checkpointPosition = _playerStart.position;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// 플레이어 사망 처리 — GameOver 상태, 지연 후 리스폰
        /// </summary>
        public void PlayerDied()
        {
            if (_currentState == GameState.GameOver || _currentState == GameState.Clear || _isRespawning)
                return;

            _currentState = GameState.GameOver;
            _isRespawning = true;

            Debug.Log("[GameManager] Player died — GameOver state, respawning in " + _respawnDelay + "s");

            // 코루틴으로 지연 리스폰 (MonoBehaviour이므로 StartCoroutine 사용 가능)
            StartCoroutine(RespawnRoutine());
        }

        private System.Collections.IEnumerator RespawnRoutine()
        {
            yield return new WaitForSecondsRealtime(_respawnDelay);

            PerformRespawn();
        }

        /// <summary>
        /// 실제 리스폰 수행 — 위치 이동 + 체력/적 리셋 이벤트 발화
        /// </summary>
        private void PerformRespawn()
        {
            // 플레이어 위치 이동 (SceneManager 미사용)
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = _checkpointPosition;
                Debug.Log($"[GameManager] Player respawned at checkpoint: {_checkpointPosition}");
            }
            else
            {
                Debug.LogWarning("[GameManager] Player not found (tag: Player). Respawning at PlayerStart.");
                if (_playerStart != null)
                {
                    // PlayerStart 위치에 플레이어가 없으면 경고만 (씬에 플레이어가 있어야 함)
                }
            }

            // 체력/적 리셋 이벤트 발화 — CombatEvents 허브 경유 (직접 Invoke 금지, CS0070 방지)
            CombatEvents.RaiseOnPlayerRespawn();

            _currentState = GameState.Playing;
            _isRespawning = false;
        }

        /// <summary>
        /// 체크포인트 위치 저장 (Checkpoint.cs에서 호출)
        /// </summary>
        public void SetCheckpoint(Vector2 position)
        {
            _checkpointPosition = position;
            Debug.Log($"[GameManager] Checkpoint set to: {position}");
        }

        /// <summary>
        /// 레벨 클리어 처리 — 스테이지 끝 트리거 도달 시 호출
        /// 보스전 Wave에서는 여기서 보스 HP 0 체크로 교체 예정 (주석 슬롯)
        /// </summary>
        public void OnLevelEnd()
        {
            if (_currentState == GameState.Clear)
                return;

            _currentState = GameState.Clear;
            Debug.Log("[GameManager] Level Clear!");

            // TODO: 보스전 Wave에서 아래 로직으로 교체
            // if (BossHealth.Instance != null && BossHealth.Instance.CurrentHealth <= 0)
            //     _currentState = GameState.Clear;
        }

        /// <summary>
        /// 전체 게임 상태 리셋 (체크포인트, 체력, 적 등) — 메뉴/재시작용
        /// </summary>
        public void ResetGame()
        {
            _currentState = GameState.Playing;
            _isRespawning = false;

            // 체크포인트를 시작 위치로 리셋
            if (_playerStart != null)
                _checkpointPosition = _playerStart.position;
            else
                _checkpointPosition = Vector2.zero;

            // 리스폰 이벤트 발화로 모든 적/체력 리셋
            CombatEvents.RaiseOnPlayerRespawn();

            Debug.Log("[GameManager] Game reset — checkpoint, health, enemies cleared");
        }

        /// <summary>
        /// 외부에서 강제 리스폰 트리거 (테스트/디버그용)
        /// </summary>
        [ContextMenu("Force Respawn")]
        public void ForceRespawn()
        {
            if (!_isRespawning)
            {
                _isRespawning = true;
                StartCoroutine(RespawnRoutine());
            }
        }
    }
}