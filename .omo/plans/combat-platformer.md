# combat-platformer - Work Plan

## TL;DR (For humans)

기존 2D 플랫포머(EPSILON GAME) 위에 **패링 + 칼 전투** 시스템을 얹어 "발표용 액션 플랫포머"로 재구성합니다.

**얻는 것**: 플레이어가 칼로 공격하고, 적의 공격을 패링(일반/완벽)해서 반격하는 타이밍 기반 전투가 있는 게임. 전투 시 카메라 확대 + 슬로우 모션 연출. 개방형 긴 스테이지에서 이동→전투→이동 반복.

**접근**: 기존 코드(PlayerController, Cinemachine 카메라, 개방형 씬)는 유지하고, 전투 시스템을 새 스크립트로 계층 추가. 에셋이 없는 부분(적 스프라이트, VFX 이펙트)은 **틀만 만들고 에셋 슬롯만 남겨** 사용자가 넣으면 바로 적용되는 구조.

**하지 않는 것**: 보스전(방/칸 구조는 나중에 보스전에서 채택), 구름/언덕 장식, 기존 vfx 폴더 사용, 새 Input System 마이그레이션(legacy Input 유지). 추가 적 패턴(행동 차이는 파라미터로만).

**effort**: 약 2~3주 (7개 Wave, 플레이 테스트 튜닝 포함)
**risk**: 패링 타이밍 감각(윈도우 크기)이 재미를 좌우 — 변수화해 놓고 플레이 테스트로 조정 필요

---

## Scope

### IN
- 플레이어 칼 공격 (방향/타이밍 직접 결정, 쿨다운, **1→2→3 콤보 — 공격 횟수마다 모션 각각, 3 이후 쿨다운** — 2026-09-17 사용자 지시)
- 패링 시스템 (공격 직전/직후 윈도우, 일반/완벽 구분, 실패 시 데미지+넉백, **defend 모션 연동**)
- **적 4종 (Slime/Rat/Mimic/Bat — 전부 사용, 2026-09-18 확정)**: 종별 상태 머신 파라미터/애니메이션 (감지→공격준비→공격→패링당함→피해→사망)
- 전투 연출 (카메라 확대 + 슬로우 모션 + VFX 슬롯 — 적 텔레그래프 + 플레이어 공격 둘 다)
- 플레이어/적 체력, 넉백, 사망
- 체크포인트 + 리스폰 시스템
- **파럴랙스 배경 (7레이어, 레이어별 속도 차등)** — 2026-09-18 사용자 지시
- **HP HUD (sprite/UI/HP_HUD.png 적용)** — 2026-09-18 에셋 제공
- **플레이어 캐릭터 슬롯 구조 (4종 폴더 전부 활용, 메인 캐릭터는 사용자가 나중에 결정 → 교체 용이 구조)** — 2026-09-18 확정
- 게임오버 / 클리어 화면 (클리어 = 스테이지 끝 도달, 보스 슬롯 예약 — plan.txt §13의 "보스 처치" 항목은 보스전 Wave에서 충족 예정)
- 개방형 단일 스테이지 리빌드 (바닥+플랫폼+적 배치, 이동↔전투 반복)

### OUT
- 보스전 (방/칸 구조는 추후 보스전 Wave에서 별도 계획)
- 추가 적 패턴/행동 (4종의 차이는 파라미터+애니메이션으로만 — 새 상태머신 금지)
- 구체적 카메라 연출 값 확정 (프로토타입 플레이 후 결정 — 변수화만)
- VFX 이펙트 실제 에셋 (사용자 제공 대기 — 슬롯만)
- 완벽 패링의 구체적 적 패널티 종류 (확장 가능 구조만, 구체값 추후)
- 사운드 (기존 OUT 유지)
- 새 Input System 전환

---

## Verification Strategy

### Agent-Executed QA
각 todo마다 Unity Play Mode + eval/CLI로 검증:
- 컴파일 성공 → Play Mode 진입 → 동작 확인
- 전투 판정은 임시 테스트 스크립트 또는 Inspector 수치로 확인
- 증거: 콘솔 로그, 스크린샷(씬 뷰/게임 뷰), 씬 YAML

### Test Strategy
- TDD: 불필요 (Unity 직접 조작 + Play Mode)
- 각 시스템별 Play Mode 수동 시나리오 (이동/공격/패링 성공/패링 실패/적 처치/리스폰/클리어)

---

## Execution Strategy

기존 기반(PlayerController, Cinemachine, 개방형 씬) 유지 + 전투 시스템 계층 추가:

**Wave 1** 씬 정리 → **Wave 2** 플레이어 전투(공격+패링) → **Wave 3** 적 시스템 → **Wave 3.5** 넉백 수정 → **Wave 4** 전투 연출 → **Wave 5** 게임 흐름(체크포인트/UI) → **Wave 6** 스테이지 + 통합 테스트 → **Wave 7** 플레이어 캐릭터 슬롯+애니메이션(콤보) → **Wave 8** 적 4종 에셋 적용 → **Wave 9** 파럴랙스 배경 → **Wave 10** HP HUD + 최종 통합

### 에셋 상태 가이드 (2026-09-18 갱신 — 대부분 사용자 제공 완료)
| 부분 | 현재 | 비고 |
|---|---|---|
| 플레이어 스프라이트 | **제공 완료**: `sprite/player/` 4종(5폴더: heavy_player, sword_,man, sword_man2, TheHand, Warrior) | 메인 캐릭터 미확정 (사용자가 나중에 결정) — **캐릭터 슬롯 구조**로 구현, 교체 용이 |
| 적 스프라이트/애니 | **제공 완료**: Slime/Rat/Mimic/Bat (4종) | 종별 프리팹/파라미터/애니메이션 |
| VFX (베기 이펙트 등) | 프리팹에 VFX 슬롯(직렬화 필드)만, 기본 Particle 임시 | 슬롯에 프리팹 할당 |
| 배경 | **제공 완료**: `sprite/background/far city background/` 7레이어 | 파럴랙스 레이어별 속도 차등 |
| UI | **HP HUD 제공 완료** (`sprite/UI/HP_HUD.png`), 게임오버/클리어는 기본 텍스트 | HP HUD 이미지 적용, 나머지 아트 대기 |

---

## Todos

### Wave 1: 씬 정리 — 개방형 베이스 확정

#### Todo 1-1: Main.unity 정리 (장식/벽 제거 + 개방형 확정)
- [x] 1. **File/Directory**: `Assets/Scenes/Main.unity`
- **References**: 기존 씬 (Cloud_1..3, Hill_1..3, WallLeft, WallRight, Zone1, TempFollowProbe 존재)
- **Acceptance Criteria**:
  - Cloud_1..3, Hill_1..3 (장식) 제거
  - WallLeft, WallRight 제거 (개방형 — LevelBounds가 카메라 경계 담당)
  - Zone1 + CameraZone.cs 제거 (방 전환 불필요 — 전투 확대 연출로 대체)
  - TempFollowProbe 제거 (임시 스크립트 잔재)
  - Floor, Platform_A~D, LevelBounds, Player, Main Camera, vcam1 유지
  - 씬 저장 후 YAML에서 제거 대상 오브젝트 미존재 확인
- **QA**:
  - Happy: 씬 Hierarchy에 제거 대상 없음, Play Mode에서 플레이어가 좌우로 자유 이동 (벽에 막히지 않음)
  - Failure: 벽 잔존 시 제거 확인, 카메라가 레벨 밖 노출 시 LevelBounds 크기 확인
- **Commit**: `chore: remove decorations/walls for open-level design`

#### Todo 1-2: 태그/레이어 준비
- [x] 2. **File/Directory**: Project Settings (Tag Manager / Layer Manager)
- **References**: Unity Tag/Layer 시스템
- **Acceptance Criteria**:
  - Tag: `Enemy`, `Checkpoint`, `Finish` 추가 (기존 `Player` 유지) — Finish는 스테이지 끝 클리어 트리거용
  - Layer: `Enemy` (index 6~7 사이 빈 슬롯), `Player` (index 8~9 — 기존 Ground index 3과 충돌 없게)
  - Physics2D Collision Matrix: Player↔Enemy ON, Player↔Ground ON, Enemy↔Ground ON, Enemy↔Enemy OFF, Player↔Checkpoint(트리거) ON — 트리거는 충돌 행렬과 무관(isTrigger)하나 레이어 명시
- **QA**:
  - Happy: Tag/레이어 드롭다운에서 선택 가능, Player↔Enemy 물리 충돌 발생
  - Failure: 레이어 중복/잘못된 인덱스 시 재배치, 충돌 안 하면 Collision Matrix 확인
- **Commit**: `config: add Enemy/Checkpoint tags and layers`

#### Todo 1-3: 전투 공용 기반 — 에셋 슬롯 규칙 문서 + 공용 이벤트 허브
- [x] 3. **File/Directory**: `.omo/notes/asset-slots.md` (신규, **AGENTS.md 아님** — AGENTS.md 수정은 인간 승인 필요) + `Assets/Scripts/CombatEvents.cs` (신규)
- **References**: 사용자 지시 "에셋 없는건 틀만, 넣으면 바로 적용", "원래 있던건 지워도 됨" (기존 계획서 정리 승인)
- **Acceptance Criteria**:
  - 에셋 슬롯 규칙 문서 1장: 모든 외부 에셋은 `[SerializeField] private GameObject/AnimationClip` 슬롯으로 참조, placeholder는 컬러 사각형, 사용자 제공 시 슬롯 교체만으로 동작
  - 각 신규 스크립트에 이 규칙을 Header 주석으로 명시
   - **`CombatEvents.cs`**: 정적 이벤트 허브 (파동 간 결합 제거 — Wave 2~5가 여기만 참조):
     ```csharp
     public static class CombatEvents {
         public static event System.Action<GameObject> OnEnemyAttackTelegraph; // 공격 준비 시작 (슬로우/카메라용) — payload: enemy
         public static event System.Action<GameObject> OnEnemyAttackHit;       // 공격 판정 순간 (패링 판정용) — payload: enemy
         public static event System.Action OnPlayerAttack;                     // 플레이어 칼 공격 판정 순간 (슬로우/카메라 연출용) — 사용자 피드백(2026-09-17): 둘 다
         public static event System.Action OnPlayerRespawn;                    // 리스폰 시 (적/체력 리셋용)
     }
     ```
  - 기본값 규칙: 모든 이벤트는 `?.Invoke(...)` 패턴 (구독자 없어도 안전)
  - **기존 계획서 정리 (인간 승인 획득 — 2026-09-16)**: `.omo/plans/platformer-basic.md`를 `.omo/archive/platformer-basic.md`로 이동 + 헤더에 "SUPERSEDED by combat-platformer.md" 표기 (완전 삭제 대신 아카이브 — git 기록 유지). **AGENTS.md §1 관련 문서 줄을 `combat-platformer.md`로 교체하는 것은 별도 인간 승인 후 반영** (이 todo에서는 AGENTS.md 수정하지 않음)
- **QA**:
  - Happy: 컴파일 성공 (이벤트 허브 자체는 동작 없음 — Wave 2 이후 발화/구독으로 검증)
  - Failure: 타입 참조 오류 시 네임스페이스 확인
- **Commit**: `chore: add asset-slot convention and combat events hub`

### Wave 2: 플레이어 전투 — 공격 + 패링 시스템

#### Todo 2-1: PlayerCombat.cs — 칼 공격
- [x] 4. **File/Directory**: `Assets/Scripts/PlayerCombat.cs` (신규)
- **References**: `PlayerController.cs` (이동 방향 `MoveInput` 사용), Unity Physics2D.OverlapCircle
- **Acceptance Criteria**:
  - 네임스페이스 `EpsilonGame`, `[SerializeField] private` + `[Header]`
  - 공격 입력: `Input.GetButtonDown("Fire1")` (legacy) — Update()에서 감지
  - `[Header("Attack")]`: attackDamage=1, attackRange=1.5, attackCooldown=0.4f, attackDuration=0.15f, attackOffset (Vector2, 기본 (0.8,0))
  - 공격 시: 앞 방향 기준 `Physics2D.OverlapCircleAll`로 적 감지, `IEpsilonDamagable.TakeDamage(amount, direction, knockback)` 호출
  - **이벤트 발화**: `CombatEvents.OnPlayerAttack?.Invoke()` — PerformAttack의 판정 후 발화 (Wave 4 CombatDirector/CombatCamera가 구독하여 슬로우/확대 연출 — 사용자 피드백 2026-09-17: "둘 다")
  - **VfxSlot 선택 참조**: `[SerializeField] private VfxSlot vfxSlot;` (null 허용 — Wave 4에서 부착/할당. null이면 조용히 스킵, 크래시 금지) — Wave 4가 Wave 2 코드를 수정하지 않도록 훅만 준비
  - 쿨다운 타이머로 연타 제한, 공격 중 이동/점프는 `PlayerController`와 독립 (전투 중 조작 제한 없음)
  - 패링 후 공격 가능 시간 변수 `attackWindowAfterParry = 1.5f` (패링 성공 시 공격 쿨다운 리셋)
- **QA**:
  - Happy: 적 근접에서 Fire1 → 적 TakeDamage 콜 (디버그 로그), 쿨다운 중 연타 무시
  - Failure: 공격 판정이 안 나오면 attackOffset/범위 확인, 태그/Layer 확인
- **Commit**: `feat: implement player sword attack`

#### Todo 2-2: PlayerCombat.cs — 패링 시스템 (일반/완벽)
- [x] 5. **File/Directory**: `Assets/Scripts/PlayerCombat.cs` (같은 파일에 패링 추가)
- **References**: plan.txt §3 패링 설계 (공격 직전+직후 윈도우), §5 전투 중 행동 (이동/점프/공격/패링 자유)
- **Acceptance Criteria**:
  - 패링 입력: `Input.GetButtonDown("Fire2")` (legacy) — Update()에서 감지
  - `[Header("Parry")]`: parryWindowBefore=0.2f (적 공격 판정 직전), parryWindowAfter=0.08f (판정 직후), perfectParryFraction=0.5f (앞 50% = 완벽), parryCooldown=0.3f
  - **타이밍 계약**: 적 `telegraphTime`(Enemy.cs 기본 0.5f) > `parryWindowBefore`(0.2f) 필수 — 슬로우 모션 진입 → 패링 윈도우 시작 순서가 보장되도록 (값 일치는 Wave 3 QA에서 검증)
  - **이벤트 구독**: `CombatEvents.OnEnemyAttackTelegraph` (payload GameObject=enemy) 구독 → 패링 윈도우 시작. 파동 간 결합은 CombatEvents 허브(Wave 1, Todo 1-3)로만 — CombatDirector 직접 참조 금지
  - 패링 성공 분기:
    - 일반: `OnParrySuccess(enemy, isPerfect=false)` — 적 공격 중단, 적 무방비 (stunTime=1.0f, 변수), 공격 쿨다운 리셋
    - 완벽: `OnParrySuccess(enemy, isPerfect=true)` — 적 무방비 연장 (stunTime*2), `enemy.ApplyPenalty()` 확장 훅 호출
  - 패링 실패: 적 공격 판정 통과 시 플레이어 데미지 + 넉백 (`PlayerHealth.TakeDamage`, 별도 todo에서 구현)
  - 패링 상태 노출: `public bool IsParrying` / `public bool HasParryWindow`
- **QA**:
  - Happy: 적 공격 직전 패링 → 일반 성공 로그, 완벽 구간(앞 50%) → 완벽 성공 로그 + 적 stun 2배
  - Failure: 윈도우 밖에서 패링 무시, 실패 시 데미지 발생 확인
- **Commit**: `feat: implement parry system with perfect parry`

#### Todo 2-3: PlayerHealth.cs — 체력/데미지/넉백/사망
- [x] 6. **File/Directory**: `Assets/Scripts/PlayerHealth.cs` (신규)
- **References**: plan.txt §3 패링 실패 (데미지+넉백), §8 체크포인트 (사망 시 리스폰)
- **Acceptance Criteria**:
  - `IEpsilonDamagable` 인터페이스 구현: `void TakeDamage(int amount, Vector2 direction, float knockback)`
  - `[Header("Health")]`: maxHealth=5, invincibleTime=0.5f, knockbackForce=8f
  - 피해 시: HP 감소, 넉백 적용 (Rigidbody2D velocity), 무적 시간 (깜빡임 — SpriteRenderer toggle), 사망 시 `GameManager.Instance.PlayerDied()` 호출
  - `public int CurrentHealth`, `public bool IsDead`
  - 사망 원인: 체력 0 (전투 패배) — 낙사는 LevelBounds 아래 kill plane으로 처리 (범위 최소화)
- **QA**:
  - Happy: 적 공격 5회 → 사망 + GameManager 콜, 무적 시간 중 추가 피해 무시
  - Failure: 넉백 방향 반대면 sign 확인, 무적 무한 지속이면 타이머 확인
- **Commit**: `feat: implement player health and knockback`

#### Todo 2-4: Player.prefab에 전투 컴포넌트 부착 + 넉백 검증
- [x] 7. **File/Directory**: `Assets/Prefabs/Player.prefab` — PlayerCombat/PlayerHealth 부착 완료 (YAML 검증 통과: L235 PlayerCombat, L261 PlayerHealth). Safe Mode 해제 후 **Play Mode 검증**: 콘솔 에러 0 + Fire1/Fire2 반응. 추가로 **플레이어 넉백 동작 검증** (적 공격 시 뒤로 밀리는지): PlayerController의 velocity 덮어쓰기와 넉백이 충돌하는지 확인 — 이 충돌은 신규 Todo 3.5에서 해결.
- **References**: Todo 2-1~2-3 스크립트
- **Acceptance Criteria**:
  - PlayerCombat, PlayerHealth 컴포넌트 추가
  - 공격 중심 오프셋/범위는 Inspector 기본값으로
  - Player 레이어 할당 (Todo 1-2)
  - 기존 PlayerController, Rigidbody2D(Interpolate), BoxCollider2D 유지
- **QA**:
  - Happy: Play Mode 진입 시 컴포넌트 정상 로드 (콘솔 에러 0), Fire1/Fire2 입력 반응
  - Failure: 컴포넌트 null 참조 → 직렬화 필드 확인
- **Commit**: `feat: attach combat components to player prefab`

### Wave 3: 적 시스템 — 기본 적 1종

#### Todo 3-1: Enemy.cs — 상태 머신
- [x] 8. **File/Directory**: `Assets/Scripts/Enemy.cs` (신규) — 구현 완료(373줄, 상태 머신 전체) + **CS0070 수정 완료 (2026-09-17)**: CombatEvents.cs에 raise 메서드 3개 추가, Enemy.cs 221/257행이 `RaiseOnEnemyAttackHit/Telegraph` 호출로 변경. **배치 모드 컴파일 검증 통과 (2026-09-17, exit 0, error CS 0건)**. Play Mode QA는 Todo 6-2 통합 테스트에서 검증 예정.
- **References**: plan.txt §6 적 상태 (감지/공격준비/공격/패링당함/패널티/피해/넉백/사망), §11 단순 구조
- **Acceptance Criteria**:
  - 네임스페이스 `EpsilonGame`, `[SerializeField] private` + `[Header]`
  - 상태 enum: `Idle, Detected, AttackTelegraph, Attacking, Parried, Penalty, Hurt, Dead`
  - 전이: Idle→Detected (플레이어 감지 반경 `detectRadius=5`), Detected→AttackTelegraph (공격 쿨다운 종료 `attackCooldown=2f`), AttackTelegraph→Attacking (telegraphTime=0.5s 경과), Attacking→(판정) →Idle/Parried/Hurt, Attacking 중 패링 → Parried, Hur t/Parried → Idle (stunTime 경과)
  - 공격 판정: `[Header("Attack")]` attackDamage=1, attackRange=2f, attackOffset — `Physics2D.OverlapCircle`로 Player 감지 시 `PlayerHealth.TakeDamage` 호출
  - **이벤트 발화 (CS0070 패턴 수정 필수 — 2026-09-17)**: C#에서 `event`는 **선언 클래스 내에서만 발화** 가능. `CombatEvents`에 `RaiseOnEnemyAttackTelegraph(GameObject enemy)`, `RaiseOnEnemyAttackHit(GameObject enemy)`, `RaiseOnPlayerRespawn()` static 메서드를 추가하고, Enemy.cs는 `CombatEvents.RaiseOnEnemyAttackTelegraph(gameObject)` / `CombatEvents.RaiseOnEnemyAttackHit(gameObject)` 호출 (외부에서 직접 `?.Invoke` 금지 — **CS0070 컴파일 에러**, 현재 Safe Mode 원인). 구독은 `+=`/`-=` (정상 — PlayerCombat/EnemyHealth와 동일). CombatEvents 허브(Todo 1-3)만 참조, CombatDirector 직접 참조 금지
  - **리스폰 리셋**: `OnEnable()`에서 `CombatEvents.OnPlayerRespawn += ResetForRespawn`, `OnDisable()`에서 `-=` (구독 패턴 고정 — GameManager가 아닌 이벤트 허브 경유)
  - `ApplyPenalty()` — 완벽 패링 시 확장 훅 (기본: stun 2배, 추가 패널티는 추후 결정 — 주석으로 확장 지점 표시)
  - 전투 중 플레이어가 떨어지면: Detected에서 3초 미감지 시 Idle 복귀 (전투 종료)
- **QA**:
  - Happy: 플레이어 접근 시 감지→공격준비→공격 순서 (디버그 로그), 패링 시 Parried 상태 진입
  - Failure: 상태 전이가 안 되면 쿨다운/타이머 확인, OverlapCircle 레이어 확인
- **Commit**: `feat: implement enemy state machine`

#### Todo 3-2: EnemyHealth.cs — 체력/넉백/사망
- [x] 9. **File/Directory**: `Assets/Scripts/EnemyHealth.cs` (신규) — 구현 완료(114줄) + **Enemy 참조 복원 (2026-09-17)**: `GetComponent<Enemy>()?.SetHurtState()` 복원, knockbackForce fallback 적용 (CS0414 경고 제거, PlayerHealth.cs 동일 적용). **배치 모드 컴파일 검증 통과 (2026-09-17, exit 0, error CS 0건 + warning CS 0건)**. Play Mode QA는 Todo 6-2 통합 테스트에서 검증 예정.
- **References**: plan.txt §6 (피해/넉백/사망)
- **Acceptance Criteria**:
  - `IEpsilonDamagable` 구현 (PlayerHealth와 동일 인터페이스 — 공용 인터페이스는 `Assets/Scripts/Interfaces/IEpsilonDamagable.cs`)
  - `[Header("Health")]`: maxHealth=3, knockbackForce=6f, deathVanishTime=0.5f
  - 사망 시: Dead 상태, 콜라이더 비활성화, 0.5초 후 제거 (스폰 이펙트는 VFX 슬롯 todo에서)
  - **리스폰 리셋**: `OnEnable()`에서 `CombatEvents.OnPlayerRespawn += ResetForRespawn` 구독 (체력/상태 초기화), `OnDisable()`에서 해제
- **QA**:
  - Happy: 플레이어 공격 3회 → 사망 → 오브젝트 제거
  - Failure: 데미지 미적용 시 OverlapCircle 레이어/마스크 확인
- **Commit**: `feat: implement enemy health and death`

#### Todo 3-3: Enemy 프리팹 (placeholder + Animator 틀)
- [x] 10. **File/Directory**: `Assets/Prefabs/Enemy.prefab` (신규) — 구현 완료 (2026-09-17): `Assets/Editor/EnemyPrefabBuilder.cs` 배치 모드 스크립트로 프리팹+컨트롤러 생성, YAML 전체 검증 완료 (layer=8 Enemy, tag=Enemy, Rigidbody2D Interpolate, BoxCollider2D 1x1, Enemy/EnemyHealth GUID 일치, 컨트롤러 파라미터/상태/전이 5개/AnyState→Dead/default=Idle). **Placeholder 수정 (2026-09-17)**: 1x1 텍스처가 기본 100 PPU로 0.01 유닛 렌더링(거의 안 보임) → `spritePixelsPerUnit=1`로 수정 후 재빌드, SpriteRenderer `m_Size {x:1,y:1}` 확인. **레이어 버그 수정 (2026-09-17)**: 실제 레이어는 **Enemy=8, Player=9** (6/7은 빈 레이어) — `Enemy.cs:34 playerLayer 1<<7→1<<9`, `Enemy.prefab playerLayer m_Bits 128→512`, `Player.prefab 루트 m_Layer 7→9` (groundCheck 0 유지), `Player.prefab enemyLayer m_Bits 0→256`. 배치 컴파일 재검증 통과 (Tundra build success, return code 0, error CS 0건). Play Mode QA는 Todo 6-2 통합 테스트에서 검증 예정.
- **References**: 에셋 슬롯 규칙 (Todo 1-3), 2d-pixel-perfect/기존 Player.prefab 구조
- **Acceptance Criteria**:
  - SpriteRenderer: 임시 컬러 사각형 — **`Sprite.Create(Texture2D.whiteTexture, ...)` 또는 씬에서 직접 1x1 흰색 텍스처를 컬러 Material로 tint** (커스텀 스프라이트 에셋 불필요 — 잘못된 `CreateGUID` 방식 금지), Enemy 레이어, tag=Enemy
  - Rigidbody2D (Interpolate), BoxCollider2D (Ground 충돌), groundCheck 없음 (적은 점프 안 함)
  - Enemy.cs, EnemyHealth.cs 부착
  - **Animator 틀**: `Assets/Animations/EnemyAnimator.controller` 신규 생성 — Parameters: `Summoned/GotHit/Attack/Dead` (bool 4종). **상태 전이표 (Worker가 그대로 구현)**:
    | From | To | Parameter 조건 |
    |---|---|---|
    | Idle | Attack | `Attack == true` |
    | Idle | GotHit | `GotHit == true` |
    | GotHit | Idle | `GotHit == false` |
    | Attack | Idle | `Attack == false` |
    | Any | Dead | `Dead == true` |
    (클립 미할당 placeholder 스테이이트 — 사용자 에셋 대기, 전이만 구성)
  - 물리: 바닥에 안정적 착지 (Rigidbody2D Gravity=1, FreezeRotation)
- **QA**:
  - Happy: 씬에 배치 시 바닥 위 고정, 상태 변화에 Animator 파라미터 갱신 (Inspector 확인)
  - Failure: 미끄러짐 시 FreezeRotation/마찰 확인
- **Commit**: `feat: create enemy prefab with animator skeleton`

### Wave 3.5: 플레이어 넉백 수정 (사용자 피드백 #1 — "내가 넉백이 안 됨")

#### Todo 3-5: PlayerController — 넉백 중 velocity 덮어쓰기 방지
- [x] 11. **File/Directory**: `Assets/Scripts/PlayerController.cs` + `Assets/Scripts/PlayerHealth.cs`
- **References**: 사용자 피드백 #1 (2026-09-17): "내가 넉백이 안 됨". 원인 확정 (코드 진단):
  - `PlayerController.FixedUpdate:61`: `rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y)` — 매 물리 프레임 x속도를 덮어씀
  - `PlayerHealth.TakeDamage:59`: `rb.linearVelocity = knockbackDir * force` — 속도 설정 후 다음 FixedUpdate에서 x만 소멸 (y는 유지되므로 체감상 "살짝 튀는 정도" 그침)
- **Acceptance Criteria**:
  - 방식 (권장 — 최소 변경): **넉백 후 잠깐 이동 입력 무시** — PlayerHealth에 넉백 타이머(`knockbackDuration=0.2f`) 추가, PlayerController가 `PlayerHealth.IsInKnockback` 또는 별도 플래그 참조 시 `moveInput * moveSpeed` 대신 현재 velocity 유지 (또는 AddForce 방식)
  - 구현 방향: `PlayerHealth`에 `public bool IsInKnockback => knockbackTimer > 0f` 노출, `TakeDamage`에서 `knockbackTimer` 설정, `PlayerController.FixedUpdate`에서 `if (playerHealth != null && playerHealth.IsInKnockback) { /* 이동 velocity 덮어쓰기 생략 */ }` — `GetComponent<PlayerHealth>()`는 Awake에서 캐시
  - 낙사 방지 유지: y velocity는 넉백 중에도 그대로 (중력 계속)
  - 기존 이동감(가속/정지)은 위의 분기만 추가 — 다른 코드 불변
- **QA**:
  - Happy: Play Mode에서 적 공격 후 플레이어가 뒤로 밀림 (x 이동 거리 확인, 디버그 로그로 넉백 시간 기록), 넉백 후 즉시 이동 재개
  - Failure: 넉백 미발생 시 (1) TakeDamage 호출 여부 (2) IsInKnockback 타이머 (3) FixedUpdate 분기 순서 확인
- **Commit**: `fix: preserve player knockback velocity from movement overwrite`

### Wave 4: 전투 연출 — 카메라 확대 + 슬로우 모션 + VFX 슬롯

#### Todo 4-1: CombatDirector.cs — 전투 상태 관리 + 슬로우 모션
- [x] 12. **File/Directory**: `Assets/Scripts/CombatDirector.cs` (신규)
- **References**: plan.txt §9 카메라 흐름, §11 슬로우 모션 제어, **사용자 피드백 (2026-09-17): "둘 다" — 적 공격 준비 + 플레이어 공격 시 둘 다 연출**
- **Acceptance Criteria**:
  - 싱글턴 `public static CombatDirector Instance` (단순 구조 — 초보자용)
  - `[Header("Slow Motion")]`: slowMotionScale=0.3f, slowMotionDuration=0.4f
  - **雙 구독 (둘 다)**: `OnEnable()`에서:
    - `CombatEvents.OnEnemyAttackTelegraph += HandleAttackTelegraph` (적 텔레그래프 → 슬로우모션)
    - `CombatEvents.OnPlayerAttack += HandlePlayerAttack` (플레이어 칼 공격 → 슬로우모션)
    - `OnDisable()`에서 둘 다 해제
  - **CombatEvents 새 이벤트 필요** (Todo 1-3 CombatEvents.cs에 추가): `public static event System.Action OnPlayerAttack;` — PlayerCombat.PerformAttack에서 발화
  - `HandleAttackTelegraph(GameObject enemy)`: `Time.timeScale = slowMotionScale`, `slowMotionDuration` 타이머 후 `Time.timeScale = 1f` 복구
  - `HandlePlayerAttack()`: 동일하게 `slowMotionScale` 적용, `slowMotionDuration/2` (플레이어 공격 연출은 짧게: 0.2s) — 변수화
  - **주의**: Time.timeScale 사용 시 FixedUpdate 물리 영향 — 스크립트 내 `Time.deltaTime` 사용 부분은 `Time.unscaledDeltaTime`로 전환 필요. QA에 "timeScale 0.3에서도 Player 이동/넉백 물리가 정상" 항목 포함
  - `[Header("Combat State")]`: 현재 전투 중인 적 참조 `CurrentEnemy`, `IsInCombat`
  - 전투 시작: 첫 적 감지 → `OnCombatStart(enemy)` (카메라 확대 트리거)
  - 전투 종료: 적 사망 or 3s 미감지 → `OnCombatEnd()` (카메라 복귀)
- **QA**:
  - Happy: (1) 적 공격 준비 시 시간 느려짐 → duration 후 정상 복구, (2) 플레이어 공격 시 짧은 슬로우모션 → 자동 복구
  - Failure: timeScale 복구 안 되면 duration 타이머 확인, 물리 이상 시 unscaledDeltaTime 전환 검토
  - **슬로우 모션 중 넉백 검증 필수**: 적 공격 → 패링 실패 → Player 넉백 발생 시 이동 거리/시간이 timeScale=1일 때와 비례하는지 확인 (unscaledDeltaTime 적용 여부)
- **Commit**: `feat: add combat director with dual slow motion triggers`

#### Todo 4-2: 전투 카메라 — 확대/복귀 연출
- [x] 13. **File/Directory**: `Assets/Scripts/CombatCamera.cs` (신규) + `Assets/Scenes/Main.unity`
- **References**: 기존 Cinemachine 설정 (vcam1 Lens size 5.4), plan.txt §9 카메라 확대, **사용자 피드백 (2026-09-17): "둘 다" — 적 텔레그래프 + 플레이어 공격 시 확대**
- **Acceptance Criteria**:
  - `[Header("Zoom")]`: combatZoomSize=4.5f (1.2x 확대), normalSize=5.4f, zoomSpeed=3f (Lerp), unzoomSpeed=2f
  - **雙 구독**: CombatEvents.OnEnemyAttackTelegraph + CombatEvents.OnPlayerAttack 모두 구독 → 전투 확대 트리거
  - **Cinemachine 3.x API (검증 완료)**: 클래스는 **`CinemachineCamera`** (3.x 이름). `vcam.Lens.OrthographicSize` 사용
  - `HandleEnemyTelegraph`: vcam1 `Lens.OrthographicSize` → 4.5 (Lerp)
  - `HandlePlayerAttack`: 4.8 (플레이어 공격은 적 텔레그래프보다 약간만 확대 — 1.1x 수준, 0.15s 동안 유지 후 복귀)
  - `OnCombatEnd`/타이머 종료: 5.4 복귀
  - 확대 중 Follow 유지 (플레이어 추적 계속 — 카메라 끊김 없음 유지)
  - CinemachineBrain 설정 변경 없음
- **QA**:
  - Happy: (1) 적 텔레그래프 → 카메라 4.5까지 확대, (2) 플레이어 공격 시 살짝 확대(4.8) → 빠르게 복귀, (3) 종료 시 5.4 복귀. 확대 중 플레이어 따라감
  - Failure: 확대 안 되면 Lens 경로 확인 (`Lens.OrthographicSize`), 끊김 발생 시 Damping/Interpolation 확인
- **Commit**: `feat: add combat camera zoom with dual trigger`

#### Todo 4-3: VFX 슬롯 — 베기 이펙트 틀 (에셋 대기)
- [x] 14. **File/Directory**: `Assets/Scripts/VfxSlot.cs` (신규) + `Assets/Prefabs/Player.prefab`에 VfxSlot 부착
- **References**: 에셋 슬롯 규칙 (Todo 1-3), 사용자 지시 "vfx 새로 찾아옴 — 저건 안 씀"
- **Acceptance Criteria**:
  - `[Header("VFX Slots")]`: `[SerializeField] private GameObject slashEffectPrefab;` (현재 null 허용 — 로그 경고만), `[SerializeField] private GameObject perfectParryEffectPrefab;` (완벽 패링 시 — null 허용), `[SerializeField] private Transform slashSpawnPoint;` (Player 자식, 공격 방향 기준)
  - `public void SpawnSlashEffect(Vector2 direction)`: prefab null이면 `Debug.LogWarning("slash VFX not assigned")` 후 리턴, 할당되면 인스턴스화 + 방향 회전
  - `public void SpawnPerfectParryEffect(Vector2 position)`: 완벽 패링 성공 시 PlayerCombat이 호출 (일반 패링보다 강한 시각 피드백 — plan.txt §3)
  - PlayerCombat 공격 시 `VfxSlot?.SpawnSlashEffect` 호출 (VFX는 연출 전용 — 판정과 무관)
  - 사용자 에셋 제공 시: 프리팹 슬롯에 할당만 하면 동작
- **QA**:
  - Happy: 미할당 상태에서 공격 → 경고 로그 + 게임 정상 동작 (크래시 없음), 할당 시(테스트용 파티클 임시 생성) 스폰 확인
  - Failure: 할당 후 안 나오면 스폰 포인트/회전 확인
- **Commit**: `feat: add VFX slot skeleton for slash effect`

### Wave 5: 게임 흐름 — 체크포인트 / 체력 UI / 게임오버·클리어

#### Todo 5-1: Checkpoint.cs — 저장/리스폰
- [x] 15. **File/Directory**: `Assets/Scripts/Checkpoint.cs` (신규)
- **References**: plan.txt §8 체크포인트 (도달 시 저장, 사망 시 마지막 체크포인트 재시작)
- **Acceptance Criteria**:
  - tag=Checkpoint (Todo 1-2), 트리거 콜라이더 (isTrigger=true)
  - 플레이어 진입 시: `GameManager.Instance.SetCheckpoint(transform.position)`, 시각 표시 변경 (이미지/색 — 임시)
  - `OnTriggerEnter2D` + Player 태그 확인
- **QA**:
  - Happy: 플레이어가 체크포인트 밟으면 저장 로그 + 시각 반응, 사망 시 저장 위치에서 리스폰
  - Failure: 리스폰 위치가 다르면 GameManager 저장 좌표 확인
- **Commit**: `feat: add checkpoint system`

#### Todo 5-2: GameManager.cs — 게임 상태/리스폰/클리어
- [x] 16. **File/Directory**: `Assets/Scripts/GameManager.cs` (신규)
- **References**: plan.txt §10 목표 (스테이지 끝까지 → 보스→클리어, 보스는 제외 — 끝 도달 = 클리어), §13 완성 기준
- **Acceptance Criteria**:
  - 싱글턴 `public static GameManager Instance`
  - 상태 enum: `Playing, GameOver, Clear`
  - `PlayerDied()`: GameOver 상태, 1.5s 후 마지막 체크포인트(또는 시작 위치)로 `SceneManager` 없이 위치 이동 + `PlayerHealth`/`EnemyHealth` 초기화 (리스폰 시 적 상태도 리셋 — plan.txt §8)
  - **적 리셋 메커니즘**: `CombatEvents.RaiseOnPlayerRespawn()` 호출 (직접 `?.Invoke` 금지 — CS0070, Todo 3-1 메모 참고) → Enemy/EnemyHealth가 `OnEnable`에서 구독하여 상태/체력/위치 초기화 (자체 이벤트 정의 금지 — Todo 1-3 허브 사용, 구독 패턴은 Todo 3-1/3-2에 명시)
  - `OnLevelEnd()`: Clear 상태 (스테이지 끝 트리거 도달 시 호출 — 보스 슬롯 예약 주석: "보스전 Wave에서 여기서 보스 HP 0 체크로 교체")
  - 시작 위치: 씬의 `PlayerStart` 빈 GameObject (Todo 6-1에서 생성, 이름 기준 참조 — 태그 불필요)
  - `SetCheckpoint(Vector2)` 저장
- **QA**:
  - Happy: 사망 → GameOver → 리스폰 → 체력/적 초기화, 클리어 트리거 → Clear 상태
  - Failure: 리스폰 시 적 미리셋 → 적 리셋 루틴 확인
- **Commit**: `feat: add game manager with respawn and level end`

#### Todo 5-3: UI — 체력 바 + 게임오버/클리어 화면 (기본)
- [x] 17. **File/Directory**: `Assets/Scripts/UIManager.cs` (신규) + `Assets/Scenes/Main.unity` (Canvas)
- **References**: plan.txt §13 (클리어/게임오버 정상 작동), 에셋 슬롯 규칙
- **Acceptance Criteria**:
  - Canvas (Screen Space - Overlay) + 기본 uGUI: HP 텍스트 또는 이미지 바 (임시), GameOver 텍스트 (처음엔 inactive), Clear 텍스트 (inactive)
  - `UpdateHealthUI(int current, int max)` — PlayerHealth 이벤트 구독, HP 바/텍스트 갱신
  - **플로우 구분**: 사망 → GameManager가 자동 리스폰(체크포인트) — 별도 게임오버 화면 없음. GameOver는 **전투 패배 + 재시작이 필요한 경우만**: GameOver 텍스트 + 시간 스케일 0, `R키` → `SceneManager.LoadScene` 전체 재시작 (체크포인트 리스폰과 이중 시스템 명확히 구분 — plan.txt §8 "처음부터 다시 플레이 안 함"은 사망 자동 리스폰에 적용)
  - `ShowGameOver()`, `ShowClear()` — 해당 텍스트 활성화
  - UI 아트는 기본 흰색 텍스트 — 사용자 에셋 대기
- **QA**:
  - Happy: 피해 시 HP UI 갱신, 사망 → GameOver 텍스트, 클리어 → Clear 텍스트, R키 재시작
  - Failure: UI 미갱신 시 이벤트 구독 확인, 시간 스케일 복구 확인
- **Commit**: `feat: add basic HUD and game over/clear screens`

### Wave 6: 스테이지 리빌드 + 통합 테스트

#### Todo 6-1: 개방형 스테이지 구성 — 적 배치/체크포인트/클리어 트리거
- [x] 18. **File/Directory**: `Assets/Scenes/Main.unity`
- **References**: plan.txt §7 스테이지 (이동→전투→이동, 체크포인트), 인터뷰 (개방형, 적 한 번에 하나씩)
- **Acceptance Criteria**:
  - LevelBounds(40×10) 유지, 기존 Floor + Platform_A~D 유지
  - **적 5~8마리 배치**: 이동 경로상 적이 한 번에 하나씩 조우하도록 간격 배치 (전투 구역당 1마리 — OverlapCircle 기반이라 자연 분리)
  - **체크포인트 2개**: 중간(약 x=10) + 후반(약 x=20)
  - **클리어 트리거**: 스테이지 끝(약 x=28) BoxCollider2D isTrigger + tag=Finish (Todo 1-2) → `GameManager.OnLevelEnd()`
  - **PlayerStart**: 스테이지 시작부(약 x=-8) 빈 GameObject `PlayerStart` 생성 (GameManager 리스폰 기준점, 태그 불필요)
  - 이동→전투 리듬: 플랫폼 A~D(기존)를 전투 구역 사이 이동 경로로 활용
  - 씬 저장 + YAML 검증 (전투 오브젝트 존재)
- **QA**:
  - Happy: 시작→적1 조우(전투)→이동→적2 조우 순서로 진행, 중간 체크포인트 저장, 끝 트리거 → Clear
  - Failure: 적 동시 조우 시 감지 반경/배치 간격 조정
- **Commit**: `feat: arrange enemies, checkpoints, and finish trigger`

#### Todo 6-2: 최종 통합 테스트
- [ ] 19. **File/Directory**: 전체 프로젝트
- **References**: plan.txt §13 완성 기준 (보스 제외 항목), 기존 Todo 13 카메라 테스트 방식
- **Acceptance Criteria**: 모든 항목 Play Mode 검증
  - 이동/점프/이단점프 정상 (기존 유지) ✅ 확인
  - 칼 공격 판정 + 적 피해 + 적 처치
  - **플레이어 공격 → 슬로우 모션 + 카메라 확대 (짧게) → 복귀** (사용자 피드백 2026-09-17: "둘 다")
  - 적 공격 → 슬로우 모션 → 패링 윈도우
  - 패링 성공 → 반격 (일반/완벽 구분 로그)
  - 패링 실패 → 플레이어 데미지 + 넉백 (**넉백 실제로 밀리는지 확인 — Todo 3-5 수정 검증**)
  - 전투 시작/종료 시 카메라 확대/복귀 (끊김 없음 유지)
  - 사망 → GameOver → 체크포인트 리스폰 → 적/체력 초기화
  - 스테이지 끝 → Clear → R키 재시작
  - 임시 테스트 스크립트 삭제, 씬 저장
- **QA**: 9개 시나리오 각각 PASS/FAIL 기록, 실패 시 해당 Wave todo로 환류
  - 증거: 콘솔 로그 (Pass/Fail 마커), 게임 뷰 스크린샷 (전투 시작/확대/클리어)
- **Commit**: `test: verify parry-combat gameplay loop`

### Wave 7: 플레이어 캐릭터 슬롯 + 애니메이션 (콤보 1·2·3 + 패링 defend)

> **배경**: 사용자 2026-09-18 "이제 플레이어도 4종됬을거야 나중에 메인 플레이어는 보고정하게" — 메인 캐릭터 미확정. **캐릭터를 격리한 슬롯 구조**로 만들어 교체가 쉬워야 함. 기본 적용은 `sword_man2` (이전 "player_2" 지시의 10종 구성과 정확히 일치 — IDLE/WALK/RUN/JUMP/DEFEND/ATTACK 1~3/HURT/DEATH).

#### Todo 7-1: 캐릭터 슬롯 아키텍처 — Player 스프라이트/애니메이션 교체 구조
- [ ] 20. **File/Directory**: `Assets/Scripts/PlayerVisuals.cs` (신규) + `Assets/Prefabs/Player.prefab` (리팩터)
- **References**: 에셋 슬롯 규칙 (Todo 1-3), 사용자 지시 "메인 플레이어는 나중에 보고 정하게" (2026-09-18)
- **Acceptance Criteria**:
  - `PlayerVisuals.cs`: `[Header("Character Slot")]` — 캐릭터별 스프라이트 모음(클립 세트)을 직렬화 필드로 보유. Animator 파라미터(`IsMoving/IsGrounded/VerticalSpeed/Attack1/Attack2/Attack3/IsDefending/Hurt/Dead`)에 스프라이트 애니메이션을 바인딩하는 구조
  - Animator Controller는 **파라미터/상태 전이만** 공용으로 보유하고, **모션(AnimationClip)을 슬롯에서 교체** — 캐릭터 교체 = 슬롯 필드만 바꾸면 동작 (Overrides 없이 단일 컨트롤러 + 클립 교체 방식)
  - 4종 폴더를 위한 4개 슬롯 그룹 선언 (기본 활성은 sword_man2), 나머지는 `[SerializeField] private AnimationClip[]` 슬롯 유지
  - 기존 Player.prefab Animator(PlayerAnimator)의 모션을 sword_man2 클립으로 교체
- **QA**:
  - Happy: Play Mode에서 캐릭터 스프라이트가 sword_man2로 표시, 슬롯 필드 교체(Inspector)만으로 다른 캐릭터 즉시 적용
  - Failure: 클립 미할당 상태에서 애니메이터 에러 → null 가드 + 기본 Idle 유지
- **Commit**: `feat: add character slot architecture for player visuals`

#### Todo 7-2: sword_man2 시트 슬라이스 + 애니메이션 클립 생성 (기본 캐릭터)
- [ ] 21. **File/Directory**: `Assets/sprite/player/sword_man2/*.png` (10개 시트) → 슬라이스 → `Assets/Animations/Player/` 클립 생성
- **References**: 2d-pixel-perfect 스킬, 사용자 지시 (2026-09-17): "attack은 1,2,3로 콤보... 모션이 겹치면 안되니 애니메이션 시간 계산 잘해야해"
- **Acceptance Criteria**:
  - 10개 시트 (IDLE/WALK/RUN/JUMP/DEFEND/ATTACK 1/ATTACK 2/ATTACK 3/HURT/DEATH) 슬라이스 (가로 스트립 — 프레임 수는 워커가 자르며 확인)
  - 클립 10개 생성 (Idle/Walk/Run/Jump/Defend/Attack1/Attack2/Attack3/Hurt/Death):
    - 스프라이트 크기 보정: PPU 설정 (시트 원본 픽셀 크기에 따라, 캐릭터 높이 ~1.5~2 유닛 목표)
    - **애니메이션 시간 계산 (사용자 요구 중점)**: 각 클립 프레임 수 × 1/fps. 공격 콤보는 클립 길이 합이 기존 `attackDuration=0.15f`, 공격 쿨다운 `0.4f`와 안 겹치도록 조정 — Attack1(예: 3프레임@12fps=0.25s) → Attack2 → Attack3 순차 재생 시 이전 클립이 끝나야 다음 입력 수용 (PlayerCombat가 클립 종료 이벤트/시간 확인)
    - Idle/Run/Walk은 loop, 공격/피격/사망은 non-loop + ExitTime
  - Animator 파라미터·전이: 기존 PlayerAnimator 참조 — 상태별 모션 교체 (캐릭터 슬롯 Todo 7-1과 연동)
- **QA**:
  - Happy: Play Mode에서 idle/run/jump/공격1→2→3 연속 콤보가 자연스럽게 이어짐 (모션 겹침 없음, 스크린샷/영상), 각 클립 길이 확인 (Inspector)
  - Failure: 모션 겹침/버벅임 → 프레임 수 확인 + 클립 길이 재조정
- **Commit**: `feat: slice sword_man2 sheets and create animation clips`

#### Todo 7-3: PlayerCombat을 3콤보 공격으로 확장 + 패링=defend 모션
- [ ] 22. **File/Directory**: `Assets/Scripts/PlayerCombat.cs` (수정) + `Assets/Scripts/PlayerVisuals.cs` (연동)
- **References**: 사용자 지시 (2026-09-17): "attack은 1,2,3로 콤보, 공격 횟수마다 1,2,3각각 나감, 3이후엔 공격 쿨, 모션 겹치면 안되니 애니메이션 시간 계산" / "패링은 defend모션"
- **Acceptance Criteria**:
  - **콤보 상태 추가**: `comboStep` (0~3), `comboWindow=0.6f` (이전 공격 후 다음 입력 수용 시간), `comboCooldown=0.8f` (3타 후 쿨다운)
  - 공격 입력 3회 연속 → 각각 Attack1→Attack2→Attack3 모션 + 판정. 3타 후 쿨다운 시작 (comboCooldown)
  - **애니메이션 타이밍 계약**: 공격 입력은 이전 클립이 **완료된 후**에만 수용 (PlayerVisuals가 현재 재생 클립 종료 여부 노출) — 모션 겹침 제거 (사용자 핵심 요구)
  - comboWindow 내 미입력 시 콤보 리셋 (1타부터)
  - **패링 = defend 모션**: Fire2 패링 입력 시 `PlayerVisuals.PlayDefend()` (defend 클립 재생), `IsParrying=true` — 기존 패링 윈도우 로직은 유지
  - 공격 판정 데미지/범위는 기존 값 유지 (attackDamage=1, attackRange=1.5)
- **QA**:
  - Happy: 연타 3회 → Attack1→2→3 순차 재생 (스크린샷/로깅), 3타 후 연타 무시 (쿨다운), 일정 간격 연타 → 콤보 리셋, 패링 입력 시 defend 모션 재생
  - Failure: 모션 겹침 → 클립 완료 여부 노출/전이 조건 확인
- **Commit**: `feat: extend player attack to 3-hit combo and parry defend animation`

### Wave 8: 적 4종 에셋 적용

#### Todo 8-1: 적 종류 확장 — Enemy.cs 파라미터화 + 종별 프리팹
- [ ] 23. **File/Directory**: `Assets/Scripts/Enemy.cs` (수정) + `Assets/Prefabs/Enemies/` (Slime/Rat/Mimic/Bat 프리팹 4종)
- **References**: 사용자 "4종 전부" (2026-09-18), 기존 Enemy.cs 상태 머신 (Todo 3-1), 에셋 슬롯 규칙
- **Acceptance Criteria**:
  - `EnemyType` enum: `Slime, Rat, Mimic, Bat` — `[Header("Enemy Type")] public EnemyType type;`
  - 종별 파라미터 기본값 (Inspector 상수 — 별도 ScriptableObject 불필요, 범위 최소화):
    - Slime: 지상형, 느림 (이동 없음, 점프 공격), 체력 3, 감지 5
    - Rat: 지상형, 빠른 돌진 공격, 체력 2, 감지 6
    - Mimic: 지상형, 근접 대기→공격, 체력 4, 감지 4
    - Bat: **공중형** — 중력 없음(또는 낮음), 호버링 이동, 플레이어 추적, 체력 2, 감지 7
  - Bat 공중형 처리: Rigidbody2D GravityScale 0 + hover 스크립트 분기 (`if (type == EnemyType.Bat)`), 공격은 접근 후 접촉
  - Animator 파라미터는 기존(Summoned/GotHit/Attack/Dead) 유지 — 종별 클립 교체만
  - 기존 Enemy.prefab → `Prefabs/Enemies/` 이동 + 4종 프리팹 분화 (폴더 정리, 기존 배치 참조 갱신)
- **QA**:
  - Happy: 4종 각각 스폰 → 종별 행동 (Bat은 공중 호버, Rat는 돌진), 종별 체력/감지 반경 Inspector 확인
  - Failure: Bat이 바닥에 붙음 → GravityScale/이동 분기 확인
- **Commit**: `feat: parameterize enemy types with 4 prefabs`

#### Todo 8-2: 몬스터 시트 슬라이스 + 애니메이션 클립 (4종)
- [ ] 24. **File/Directory**: `Assets/sprite/monster/**/*.png` → 슬라이스 → `Assets/Animations/Enemies/` 클립 생성 + `Assets/Prefabs/Enemies/*.prefab` 모션 교체
- **References**: 2d-pixel-perfect 스킬, 종별 시트 인벤토리 (드래프트 기록)
- **Acceptance Criteria**:
  - 종별 시트 전부 슬라이스 (가로 스트립 — 프레임 수 자르며 확인):
    - Slime: idle(15)/walk(6)/attack(15)/hurt(3)/death
    - Rat: idle(10)/run/attack_bite/hurt/death
    - Mimic: 10시트 (기본/열림/공격/사망 등)
    - Bat: fly(12)/attack/hurt/death/fall
  - 클립 세트 종별 생성 → 해당 프리팹 Animator에 할당 (기존 EnemyAnimator 상태 전이 유지)
  - PPU 보정 (캐릭터 높이 기준)
- **QA**:
  - Happy: 4종 각각 애니메이션 재생 (Idle→Attack→Hurt→Dead 전이), 클립 미할당 상태 없음
  - Failure: 클립 누락 → 상태별 할당 확인
- **Commit**: `feat: slice monster sheets and create enemy animations`

### Wave 9: 파럴랙스 배경 시스템

#### Todo 9-1: ParallaxController.cs + 씬 배경 배치 (7레이어)
- [ ] 25. **File/Directory**: `Assets/Scripts/ParallaxController.cs` (신규) + `Assets/Scenes/Main.unity` (Background 루트 + 레이어 7개)
- **References**: 사용자 지시 "background는 레이어별로 속도 다르게 하는기법 알지?" (2026-09-18), 레이어 순서표 (드래프트 기록 — 확정)
- **Acceptance Criteria**:
  - `ParallaxController.cs`:
    - `[Header("Parallax")] [SerializeField] private LayerSetting[] layers;` — `[Serializable] struct LayerSetting { public Transform layer; [Range(0.0f,1.0f)] public float factor; }`
    - `LateUpdate()`: 카메라 이동량 `(camPos - prevCamPos) * factor` 만큼 레이어 오프셋 (delta 방식 — 카메라 끊김 원칙 유지)
    - 수직/수평 동일 계수 적용 (점프 시에도 레이어가 따라오되 덜 움직임)
    - 카메라 참조: `Camera.main` (씬 단일 카메라 전제)
  - **씬 배치 (확정 순서 — 드래프트 파럴랙스 표)**:
    | 순서 | 파일 | 내용 | 계수 |
    |---|---|---|---|
    | 1 | -6.png | 하늘 그라데이션 | 0.05 |
    | 2 | 0.png | 지평선 실루엣 | 0.15 |
    | 3 | -2.png | 원경 도시 | 0.30 |
    | 4 | -3.png | 중경 도시 | 0.45 |
    | 5 | -1.png | 근경 도시 | 0.60 |
    | 6 | -4.png | 식생/구름 | 0.75 |
    | 7 | -5.png | 전경 식생 | 0.90 |
  - 레이어 렌더링: SpriteRenderer `drawMode=Tiled` (넓은 폭), SortingOrder -100부터 단계적 (지면/플레이어 위 미노출), 정렬 기준: z 순서 or Sorting Layer
  - 레이어별 피벗 유지 (하단 정렬 — 지평선이 지면과 자연스럽게 맞닿도록; 워커가 Game 뷰에서 시각 확인)
  - 배경 레이어는 Ground/Player보다 뒤에 렌더링 (Sorting Order or Sorting Layer 배정)
- **QA**:
  - Happy: Play Mode에서 캐릭터가 이동할 때 원경(하늘)은 거의 안 움직이고 전경(식생)은 빠르게 움직임 (스크린샷 비교), 수직 이동 시에도 자연스러움, 레이어 간 끊김/겹침 없음
  - Failure: 레이어가 안 움직임 → delta 계산/카메라 참조 확인, 겹침 → 순서/계수 재확인
- **Commit**: `feat: add parallax background with 7 layers`

### Wave 10: HP HUD 적용 + 최종 통합

#### Todo 10-1: HP HUD 에셋 적용 — 체력 이미지 UI
- [ ] 26. **File/Directory**: `Assets/sprite/UI/HP_HUD.png` (슬라이스 필요 시) + `Assets/Scripts/UIManager.cs` (수정) + `Assets/Scenes/Main.unity` (Canvas)
- **References**: Todo 5-3 (UIManager 기본), HP_HUD 에셋 (2026-09-18 제공)
- **Acceptance Criteria**:
  - HP_HUD.png 확인: 하트/바 형식에 따라 슬라이스 또는 단일 이미지 사용 (에디터에서 확인 후 결정)
  - 체력 표시: 최대 체력 5 기준 → 하트 5개 or 바 fill 비율로 표시 (에셋 형태에 맞춤), 데미지 시 갱신
  - 기존 Todo 5-3의 기본 텍스트 HP 대체 (이미지 UI)
  - 게임오버/클리어 화면은 기본 텍스트 유지 (아트 대기)
- **QA**:
  - Happy: Play Mode에서 데미지 1회 → HUD 하트 1개 감소/바 감소 (스크린샷), 체력 0 → 하트 0
  - Failure: HUD 미갱신 → UIManager 이벤트 구독/이미지 참조 확인
- **Commit**: `feat: apply HP HUD sprite to health UI`

#### Todo 10-2: 최종 통합 테스트 (에셋 반영 후 리테스트)
- [ ] 27. **File/Directory**: 전체 프로젝트
- **References**: Todo 6-2 시나리오 + Wave 7~10 신규 항목
- **Acceptance Criteria**: Todo 6-2 9시나리오 전부 재검증 + 추가:
  - 플레이어 3콤보 공격이 모션 겹침 없이 동작 (Attack1→2→3→쿨다운)
  - 패링 시 defend 모션 재생 + 성공/실패 분기
  - 적 4종 각각 조우 → 종별 행동/애니메이션 정상
  - 파럴랙스 7레이어 이동 (Game 뷰 스크린샷 2~3컷)
  - HP HUD 이미지 갱신
  - 캐릭터 슬롯 교체 경로 검증 (다른 캐릭터 슬롯으로 바꿔도 전체 동작 — 교체 후 원복)
- **QA**: 위 시나리오 PASS/FAIL 기록, 실패 시 해당 Wave 환류. 증거: 콘솔 로그 + 스크린샷
- **Commit**: `test: verify full asset-integrated gameplay loop`

---

## Final Verification Wave

### F1: Plan Compliance Audit
- 모든 Wave 1~10 todo 구현 여부
- Acceptance Criteria 충족 여부 (특히 패링 윈도우 변수, 상태 머신 전이, 콤보 애니메이션 시간, 파럴랙스 계수)

### F2: Code Quality Review
- 네임스페이스 `EpsilonGame`, `[SerializeField] private` + `[Header]` 컨벤션 준수
- Update() 입력 / FixedUpdate() 물리 분리
- Time.timeScale 사용 시 unscaledDeltaTime 검토 (슬로우 모션 사이드 이펙트)
- 에셋 슬롯 규칙 (null 안전) 준수 — 캐릭터/클립 미할당 시 크래시 금지

### F3: Real Manual QA
- Play Mode 전체 시나리오 (이동→전투→콤보→패링→반격→처치→이동→체크포인트→클리어)
- 카메라 확대/복귀 중 끊김 없음, 파럴랙스 레이어 자연스러움

### F4: Scope Fidelity
- IN/OUT 준수: 보스전/추가 적 패턴/장식/vfx 미포함
- 클리어는 보스 대신 스테이지 끝 도달 (보스 슬롯 예약만)
- 적 4종은 파라미터+애니메이션으로만 구분 (새 상태머신 금지)

---

## Commit Strategy

### Wave별 커밋
1. 씬 정리: `chore: remove decorations/walls for open-level design` / `config: add tags layers` / `chore: add asset-slot convention and combat events hub`
2. 플레이어 전투: `feat: implement player sword attack` / `feat: implement parry system` / `feat: implement player health`
3. 적: `feat: implement enemy state machine` / `feat: create enemy prefab with animator skeleton`
4. 연출: `feat: add combat director with slow motion` / `feat: add combat camera zoom` / `feat: add VFX slot skeleton`
5. 흐름: `feat: add checkpoint system` / `feat: add game manager` / `feat: add basic HUD`
6. 통합: `feat: arrange stage with enemies` / `test: verify parry-combat gameplay loop`
7. 플레이어 에셋: `feat: add character slot architecture` / `feat: slice sword_man2 sheets and create clips` / `feat: extend player attack to 3-hit combo`
8. 적 에셋: `feat: parameterize enemy types with 4 prefabs` / `feat: slice monster sheets and create animations`
9. 배경: `feat: add parallax background with 7 layers`
10. HUD/통합: `feat: apply HP HUD sprite` / `test: verify full asset-integrated gameplay loop`

### 커밋 규칙
- AGENTS.md §6: `타입: 요약` (feat/fix/config/test/docs/chore), 작게·자주
- 커밋 금지: `.omo/boulder.json`, `.omo/run-continuation/`, `.omo/notepads/`, `error_log.txt`, `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`

---

## Success Criteria

1. 플레이어가 칼로 공격 가능 (방향/타이밍 자유) — 공격 판정 + 적 피해
2. **공격 1→2→3 콤보 — 공격 횟수마다 모션 각각 재생, 모션 겹침 없음 (애니메이션 시간 계산), 3타 후 쿨다운**
3. **패링 = defend 모션 재생**, 적의 공격 모션을 보고 패링 가능 (일반/완벽 구분)
4. 패링 실패 시 데미지 + 넉백, 전투 계속 (즉시 종료 아님)
5. 패링 성공 시 반격 기회 + 적 무방비
6. **적 4종 (Slime/Rat/Mimic/Bat) 각각 배치, 종별 행동/애니메이션 동작**
7. 적 처치 후 다음 이동 구간 진행 (이동↔전투 반복)
8. 전투 시 카메라 확대 + 슬로우 모션 연출 (복귀 정상) — **적 공격 준비 시 + 플레이어 공격 시 둘 다** (사용자 확정 2026-09-17)
9. **파럴랙스 배경: 7레이어가 레이어별 속도 차등으로 움직임** (사용자 확정 2026-09-18)
10. **HP HUD 이미지로 체력 표시** (에셋 적용)
11. **캐릭터 슬롯 구조: 메인 캐릭터 교체가 슬롯 필드 변경만으로 가능 (4종 폴더 활용)** — 사용자 확정 2026-09-18
12. 체크포인트 리스폰 + 적/체력 초기화
13. 스테이지 끝 도달 → 클리어 / 사망 → 게임오버 → 재시작
14. 미커밋 변경사항 커밋 완료 (bg_9c5a94d1 이후)

> **plan.txt §13 상충 해소 (인간 결정 기록)**: 사용자 인터뷰에서 명시적으로 보스전은 제외 결정 ("방,칸구조는 보스전에 채택할거야") — §13의 "보스를 처치할 수 있다" 항목은 이번 스코프의 완성 기준에서 제외하며, 보스 슬롯(Todo 5-2 `OnLevelEnd` 주석)만 예약한다. 보스전 Wave는 추후 별도 계획으로 수립.

---

## Dependencies

### Package Dependencies
- com.unity.cinemachine 3.1.7 (기존 — 유지)
- com.unity.inputsystem (설치됨, 미사용 — legacy Input 유지, 마이그레이션 범위外)

### File Dependencies
- Assets/Scripts/PlayerController.cs (기존, 유지)
- Assets/Scripts/CameraZone.cs (제거 대상)
- Assets/Prefabs/Player.prefab (전투 컴포넌트 + 캐릭터 슬롯 추가)
- Assets/Prefabs/Enemy.prefab → `Assets/Prefabs/Enemies/` 4종으로 분화 (Slime/Rat/Mimic/Bat)
- Assets/Scenes/Main.unity (씬 리빌드 + 파럴랙스 배경 배치)
- 신규: CombatEvents.cs, PlayerCombat.cs, PlayerHealth.cs, Enemy.cs, EnemyHealth.cs, IEpsilonDamagable.cs, CombatDirector.cs, CombatCamera.cs, VfxSlot.cs, Checkpoint.cs, GameManager.cs, UIManager.cs, PlayerVisuals.cs, ParallaxController.cs, EnemyAnimator.controller, PlayerAnimator.controller, `.omo/notes/asset-slots.md`
- 신규 에셋: `Assets/sprite/player/` 4종(5폴더), `Assets/sprite/monster/` 4종, `Assets/sprite/background/far city background/` 7레이어, `Assets/sprite/UI/HP_HUD.png`

### External Dependencies
- 사용자 제공 예정: VFX 베기 이펙트, 게임오버/클리어 UI 아트 (모두 슬롯 대기)
- 플레이어 메인 캐릭터 확정 (사용자 — 현재 미정, 기본은 sword_man2, 슬롯 교체로 전환)
- Unity 6000.3.12f1