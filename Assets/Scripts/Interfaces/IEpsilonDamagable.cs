using UnityEngine;

namespace EpsilonGame
{
    /// <summary>
    /// 데미지 가능한 엔티티 공용 인터페이스.
    /// PlayerHealth, EnemyHealth 등에서 구현하여 전투 시스템과 결합도 낮춤.
    /// </summary>
    public interface IEpsilonDamagable
    {
        /// <summary>
        /// 데미지 적용 + 넉백.
        /// </summary>
        /// <param name="amount">데미지량</param>
        /// <param name="direction">넉백 방향 (정규화된 벡터)</param>
        /// <param name="knockback">넉백 힘</param>
        void TakeDamage(int amount, Vector2 direction, float knockback);
    }
}