using UnityEngine;
using Unity.Cinemachine;

namespace EpsilonGame
{
    /// <summary>
    /// 카메라 구역. 플레이어가 트리거에 진입하면 해당 구역 카메라를 활성화하고(Priority 상승),
    /// 떠나면 비활성화한다(Priority 하강). 구역마다 다른 Confiner 설정이 가능하도록
    /// zoneCamera를 직접 참조한다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CameraZone : MonoBehaviour
    {
        [Header("Camera Zone")]
        [SerializeField] private CinemachineCamera zoneCamera;
        [SerializeField] private float priorityEnabled = 10f;
        [SerializeField] private float priorityDisabled = 0f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                zoneCamera.Priority.Value = (int)priorityEnabled;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                zoneCamera.Priority.Value = (int)priorityDisabled;
            }
        }
    }
}