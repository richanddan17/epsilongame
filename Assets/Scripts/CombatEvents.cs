using UnityEngine;

namespace EpsilonGame
{
    /// <summary>
    /// 전투 시스템 공용 이벤트 허브 (Wave 1 생성, Wave 2~5 참조)
    /// 파동 간 직접 결합 제거 — Enemy/CombatDirector/GameManager가 서로 직접 참조하지 않고 허브 경유
    /// </summary>
    public static class CombatEvents
    {
        /// <summary>적 공격 준비 시작 (슬로우 모션/카메라 확대 트리거용) — payload: enemy GameObject</summary>
        public static event System.Action<GameObject> OnEnemyAttackTelegraph;

        /// <summary>적 공격 판정 순간 (패링 판정용) — payload: enemy GameObject</summary>
        public static event System.Action<GameObject> OnEnemyAttackHit;

        /// <summary>플레이어 공격 발화 (슬로우 모션/카메라 연출 트리거용)</summary>
        public static event System.Action OnPlayerAttack;

        /// <summary>플레이어 리스폰 시 (적/체력 리셋용)</summary>
        public static event System.Action OnPlayerRespawn;

        /// <summary>공격 준비 시작 발화 (슬로우 모션/카메라 확대 트리거용) — payload: enemy GameObject</summary>
        public static void RaiseOnEnemyAttackTelegraph(GameObject enemy) => OnEnemyAttackTelegraph?.Invoke(enemy);

        /// <summary>공격 판정 순간 발화 (패링 판정용) — payload: enemy GameObject</summary>
        public static void RaiseOnEnemyAttackHit(GameObject enemy) => OnEnemyAttackHit?.Invoke(enemy);

        /// <summary>플레이어 공격 발화 (슬로우 모션/카메라 연출 트리거용)</summary>
        public static void RaiseOnPlayerAttack() => OnPlayerAttack?.Invoke();

        /// <summary>플레이어 리스폰 발화 (적/체력 리셋용)</summary>
        public static void RaiseOnPlayerRespawn() => OnPlayerRespawn?.Invoke();
    }
}