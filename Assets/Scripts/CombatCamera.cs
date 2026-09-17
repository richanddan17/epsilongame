using UnityEngine;
using Unity.Cinemachine;

namespace EpsilonGame
{
    /// <summary>
    /// 전투 카메라 확대/복귀 연출 (Cinemachine 3.x)
    /// 적 텔레그래프 + 플레이어 공격 둘 다 확대 트리거, Follow 유지
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class CombatCamera : MonoBehaviour
    {
        [Header("Zoom")]
        [SerializeField] private float combatZoomSize = 4.5f;      // 1.2x 확대 (5.4 → 4.5)
        [SerializeField] private float normalSize = 5.4f;           // 기본 크기
        [SerializeField] private float zoomSpeed = 3f;              // 확대 Lerp 속도
        [SerializeField] private float unzoomSpeed = 2f;            // 복귀 Lerp 속도
        [SerializeField] private float playerAttackZoomSize = 4.8f; // 플레이어 공격 시 확대 (1.1x)
        [SerializeField] private float playerAttackHoldDuration = 0.15f; // 플레이어 공격 확대 유지 시간

        private CinemachineCamera vcam;
        private float targetSize;
        private float currentZoomSpeed;
        private bool isZooming = false;
        private float playerAttackZoomEndTime = -999f;

        private void Awake()
        {
            vcam = GetComponent<CinemachineCamera>();
            targetSize = normalSize;
        }

        private void OnEnable()
        {
            CombatEvents.OnEnemyAttackTelegraph += HandleEnemyTelegraph;
            CombatEvents.OnPlayerAttack += HandlePlayerAttack;
        }

        private void OnDisable()
        {
            CombatEvents.OnEnemyAttackTelegraph -= HandleEnemyTelegraph;
            CombatEvents.OnPlayerAttack -= HandlePlayerAttack;
        }

        private void Update()
        {
            // Lerp로 부드럽게 OrthographicSize 변경 (Follow 유지됨)
            if (vcam != null && Mathf.Abs(vcam.Lens.OrthographicSize - targetSize) > 0.01f)
            {
                vcam.Lens.OrthographicSize = Mathf.Lerp(vcam.Lens.OrthographicSize, targetSize, currentZoomSpeed * Time.unscaledDeltaTime);
            }

            // 플레이어 공격 확대 유지 시간 체크 (unscaledTime으로 슬로우 모션 영향 없음)
            if (isZooming && playerAttackZoomEndTime > 0f && Time.unscaledTime >= playerAttackZoomEndTime)
            {
                ReturnToNormal();
            }
        }

        /// <summary>
        /// 적 텔레그래프 시 확대 (1.2x, 4.5)
        /// </summary>
        private void HandleEnemyTelegraph(GameObject enemy)
        {
            if (enemy == null) return;

            targetSize = combatZoomSize;
            currentZoomSpeed = zoomSpeed;
            isZooming = true;
            playerAttackZoomEndTime = -999f; // 적 텔레그래프는 무기한 유지 (전투 종료 시 복귀)
        }

        /// <summary>
        /// 플레이어 공격 시 확대 (1.1x, 4.8) - 0.15초 유지 후 복귀
        /// </summary>
        private void HandlePlayerAttack()
        {
            targetSize = playerAttackZoomSize;
            currentZoomSpeed = zoomSpeed;
            isZooming = true;
            playerAttackZoomEndTime = Time.unscaledTime + playerAttackHoldDuration;
        }

        /// <summary>
        /// 기본 크기로 복귀
        /// </summary>
        private void ReturnToNormal()
        {
            targetSize = normalSize;
            currentZoomSpeed = unzoomSpeed;
            isZooming = false;
            playerAttackZoomEndTime = -999f;
        }
    }
}