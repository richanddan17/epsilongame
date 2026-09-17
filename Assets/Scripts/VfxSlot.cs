using UnityEngine;

namespace EpsilonGame
{
    /// <summary>
    /// 베기 이펙트/완벽 패링 이펙트 스폰 슬롯.
    /// 사용자 에셋 제공 전까지 null 허용 — 크래시 없이 로그 경고만 출력.
    /// </summary>
    public class VfxSlot : MonoBehaviour
    {
        [Header("VFX Slots")]
        [SerializeField] private GameObject slashEffectPrefab;
        [SerializeField] private GameObject perfectParryEffectPrefab;
        [SerializeField] private Transform slashSpawnPoint;

        /// <summary>
        /// 베기 이펙트 스폰. 공격 방향에 맞춰 회전.
        /// </summary>
        /// <param name="direction">공격 방향 (정규화됨)</param>
        public void SpawnSlashEffect(Vector2 direction)
        {
            if (slashEffectPrefab == null)
            {
                Debug.LogWarning("[VfxSlot] slash VFX not assigned");
                return;
            }

            if (slashSpawnPoint == null)
            {
                Debug.LogWarning("[VfxSlot] slashSpawnPoint not assigned");
                return;
            }

            var effect = Instantiate(slashEffectPrefab, slashSpawnPoint.position, Quaternion.identity);
            
            // 방향에 맞춰 회전 (오른쪽이 기본 forward라고 가정)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            effect.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        /// <summary>
        /// 완벽 패링 이펙트 스폰. 일반 패링보다 강한 시각 피드백.
        /// </summary>
        /// <param name="position">이펙트 생성 위치 (월드 좌표)</param>
        public void SpawnPerfectParryEffect(Vector2 position)
        {
            if (perfectParryEffectPrefab == null)
            {
                Debug.LogWarning("[VfxSlot] perfectParry VFX not assigned");
                return;
            }

            Instantiate(perfectParryEffectPrefab, position, Quaternion.identity);
        }
    }
}