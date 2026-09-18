# player-sprite-animation v2 - Work Plan

## TL;DR (For humans)

**What you'll get:** 플레이어가 실제 캐릭터 스프라이트로 변신 + **좌클릭 = 일반 공격, 우클릭 = 콤보 어택** 애니메이션 재생. 패링(parry)은 E키로 이동. 걷기/점프/대시/대기까지 전부 애니메이션 처리.

**Why this approach:** 사용자가 `Assets/sprite/player/player/` 폴더에 파일명 기준(딜레이 포함)으로 프레임을 분류해둠. 이 폴더만 사용하고 나머지(heavy_player, Warrior, sword_man 등)는 전부 무시.

**What it will NOT do:** `player/` 폴더 외 에셋 미사용. `ezgif-split 4.zip`(106 GIF)과 `jumpanddash` 잔여 18장(0~8, 40~48 = idle 중복)은 미압축/보류 — 범위 제외. 적/카메라/씬/게임플레이 로직은 건드리지 않음(입력 바인딩 제외).

**Effort:** Medium-Long
**Risk:** Medium - 스프라이트 시트(idle/walk)의 프레임 수/셀 크기 미확정(에디터 확인 필요), attack 74프레임 단일 클립이 약 4.4초로 김(연출 확인 필요)
**Decisions to sanity-check:** 좌/우클릭 바인딩 변경(패링 → E), attack 클립 길이, idle/walk 시트 슬라이스 규격

Your next move: 계획서 수정 완료 후 승인 → 에디터 오픈 → 실행 단계.

---

> TL;DR (machine): Medium-Long effort, Medium risk — 사용자 신규 에셋(attack 74/combo 104/dash 15·18키프레임/jump 8/doublejump 7/fall 9 + idle/walk 시트)으로 클립 8개 + 컨트롤러 재구성 + PlayerCombat 입력 변경(Fire1=attack, Fire2=combo, E=parry) + PlayerController 점프/낙하 연동 + 프리팹 적용 + Play Mode QA

## Scope

### 사용자 결정 사항 (확정)
- **좌클릭(Fire1)** = 일반 공격 → `attack/` (74프레임, delay-0.06s)
- **우클릭(Fire2)** = 콤보 어택 → `combo_attack/` (104프레임, delay-0.06s)
- **패링(parry)** = **E키**로 이동 (기존 Fire2 자리 → 콤보 어택이 차지)
- **jumpanddash 분리 (사용자 확정)**: `jump/` 8장(frame_09~16) = 점프 도약, `dash/` 15장(frame_09~11 + frame_14~25) = 대시, **`doublejump/` 7장(frame_25~31) = 공중 2단 점프**, **`fall/` 9장(frame_31~39) = 낙하** — 프레임이 클립 간 겹치는 것은 의도된 것 (복제로 각 폴더에 포함)
- **dash 재생 순서 (사용자 확정)**: `frame_09,10,11 → 14,15,...,25 → 11,10,09(역순 재생)` — 15개 파일을 18 키프레임으로 재사용
- **사용 에셋 = `Assets/sprite/player/player/`만** (파일명의 `delay-0.06s`/`delay-0.07s`를 클립 타이밍에 사용)

### 에셋 인벤토리 (사용자 분류 확정)
| 경로 (`Assets/sprite/player/player/`) | 프레임 | 규격 | 딜레이 | 용도 |
|---|---|---|---|---|
| `attack/` | 74 PNG | 729×462 | 0.06s | 좌클릭 공격 |
| `combo_attack/` | 104 PNG | 906×831 | 0.06s | 우클릭 콤보 (이미 import됨) |
| `idle/sprite sheets/idle.png` | **10프레임** (시트 460×55 = 10셀×46px, aseprite 확정) | 64×64 | 60ms | 대기 (슬라이스 필요) |
| `walk/sprite sheets/walk.png` | **26프레임 소스** (시트 180×348 = 4열×6행=24셀, 45×58 — **시트/소스 불일치 확인**) | 53×64 | 50ms | 걷기 (슬라이스 필요) |
| `jumpanddash/jump/` | 8 PNG (9~16) | 1280×490 | 0.07s | 점프 도약 |
| `jumpanddash/dash/` | 15 PNG (9~11, 14~25) | 1280×490 | 0.07s | 대시 (18키프레임: 9,10,11→14~25→11,10,09 역순) |
| `jumpanddash/doublejump/` | 7 PNG (25~31) | 1280×490 | 0.07s | 공중 2단 점프 |
| `jumpanddash/fall/` | 9 PNG (31~39) | 1280×490 | 0.07s | 낙하 (고공 낙하 대응) |
| `ezgif-split 4.zip` | 106 GIF | 미압축 | 0.06s | **보류 (범위 제외)** |
| `jumpanddash/` 잔여 18장 | 0~8, 40~48 | idle 루프 중복 | — | **보류 (범위 제외)** |

### Must have
- **스프라이트 임포트**: `attack/`, `combo_attack/`, `jumpanddash/jump/`, `jumpanddash/dash/`, `jumpanddash/doublejump/`, `jumpanddash/fall/` — 개별 PNG 프레임, PPU 100, pivot (0.5,0.5) 통일 (combo_attack은 이미 import — 설정만 확인/정렬)
- **스프라이트 시트 슬라이스**: `idle.png`(460×55), `walk.png`(180×348) — 셀 크기/프레임 수 에디터 확인 후 슬라이스 (개별 스프라이트로 분리)
- **AnimationClip 8개**: `PlayerIdle`, `PlayerWalk`, `PlayerJump`, `PlayerDash`, `PlayerDoubleJump`, `PlayerFall`, `PlayerAttack`, `PlayerComboAttack` — 파일명 딜레이를 키프레임 간격에 반영 (0.06s/0.07s), 공격계/점프/대시/낙하는 단발(Loop 해제), 이동/대기는 Loop. **PlayerJump/PlayerDoubleJump/PlayerFall은 프레임 중복 사용 가능 (같은 스프라이트 재참조)**
- **`Assets/Animations/PlayerAnimator.controller` 재구성**: 파라미터 **Speed(float)**, **IsGrounded(bool)**, **IsAttacking(bool)**, **IsComboAttacking(bool)**, **IsDashing(bool)** + **IsFalling(bool)**; 상태 Idle/Walk/Jump/DoubleJump/Fall/Dash/Attack/ComboAttack (8상태)
- **`Assets/Scripts/PlayerCombat.cs` 입력 변경**: `HandleAttackInput` Fire1 유지(→Attack), `HandleParryInput` Fire2 → **콤보 어택 트리거로 교체**, 패링은 `Input.GetKeyDown(KeyCode.E)`로 이동 — 판정 로직(불변 유지)
- **`PlayerController.cs`/`PlayerCombat.cs` Animator 연동**: Speed/IsGrounded(기존 유지) + IsAttacking/IsComboAttacking/IsDashing/IsFalling 파라미터 갱신
- **`Assets/Prefabs/Player.prefab` 적용**: Animator controller 교체, SpriteRenderer 초기 스프라이트 = idle 0번
- **Play Mode QA**: 좌클릭 공격 / 우클릭 콤보 / E 패링 / 이동 / 점프 / 2단점프 / 낙하 / 대시 전환 시나리오 PASS + 콘솔 에러 0

### Must NOT have (guardrails, anti-slop, scope boundaries)
- `player/` **외** 폴더 전부 무시 (heavy_player, Warrior, sword_,man, sword_man2, TheHand) — 이동/삭제/참조 금지
- `ezgif-split 4.zip` 처리 (GIF→PNG 변환 포함) — 사용자 결정 보류, 범위 제외
- `jumpanddash` 잔여 23장 처리 — range 제외
- Enemy 프리팹/컨트롤러/EnemyPrefabBuilder.cs 수정
- `Assets/Scenes/Main.unity` 수정
- PlayerController의 이동/점프 파라미터 값 변경 (moveSpeed/jumpForce/maxJumps 불변)
- PlayerCombat의 공격 판정/데미지/쿨다운 값 변경 (입력 바인딩만 수정)
- Animator Controller/클립을 YAML 수작업 편집 (에디터 라이브 중 — Unity CLI/에디터 API로만 변경)

## Verification strategy
> Zero human intervention - all verification is agent-executed (user가 스프라이트시트 규격/클립 길이만 확인).
- Test decision: **none** (Unity Play Mode 수동 시나리오 — 기존 계획서 패턴 동일, TDD 부적합)
- QA 도구: 에디터 라이브에서 `unity command editor_play` 진입 → 애니메이터 상태별 스크린샷/로그 수집
- Evidence: `.omo/evidence/task-7-player-sprite-animation-v2.{png,log}`

## Execution strategy
### Parallel execution waves
- **Wave 1** (Todo 1~2): 스프라이트 임포트 설정 + 시트 슬라이스 — 전 작업 선행
- **Wave 2** (Todo 3~4): PlayerAnimationBuilder.cs 작성 → 클립 8개 + 컨트롤러 재구성 (한 번의 에디터 실행)
- **Wave 3** (Todo 5~6): PlayerCombat 입력 변경 + PlayerController/PlayerCombat Animator 연동 → 프리팹 적용
- **Wave 4** (Todo 7): Play Mode QA + 커밋

전제: **에디터가 열려 있어야 함** — 실행 시작 전 사용자/워커가 `unity open`(또는 사용자 수동)로 에디터 오픈, `unity status` state=ready 확인 후 진행. 라이브 에디터 중 YAML 직접 편집 금지 — 모든 변경은 `unity command eval` / [MenuItem] 경유.

### Dependency matrix
| Todo | Depends on | Blocks | Can parallelize with |
| --- | --- | --- | --- |
| 1 (PNG 프레임 임포트) | 에디터 ready | 3 | 2 |
| 2 (시트 슬라이스) | 에디터 ready + 사용자 규격 확인 | 3 | 1 |
| 3 (클립 생성) | 1,2 | 4,5 | — |
| 4 (컨트롤러 재구성) | 3 | 6 | — |
| 5 (PlayerCombat 입력) | 3 (파라미터 명세) | 6 | 4 |
| 6 (프리팹 적용) | 3,4,5 | 7 | — |
| 7 (QA) | 6 | — | — |

## Todos
> Implementation + Test = ONE todo. Never separate.
> 사용자 검수 대기 항목: ~~jumpanddash 경계~~ (확정: jump 9~16/dash 9~11+14~25+역순/doublejump 25~31/fall 31~39), ~~idle/walk 시트 셀 크기~~ (확정: walk 시트 24셀), ~~attack 클립 길이~~ (확정: 전부 재생), jump/doublejump/fall 높이차 연동(IsFalling 기준).

### Wave 1: 스프라이트 임포트 설정

#### Todo 1: 공격/콤보/대시/점프 PNG 프레임 임포트 설정
- [x] 1. **File/Directory**: `Assets/sprite/player/player/{attack,combo_attack,jumpanddash/jump,jumpanddash/dash,jumpanddash/doublejump,jumpanddash/fall}/frame_*.png` (+ .meta)
  What to do: 에디터 라이브에서 TextureImporter 일괄 설정 →
  - `spriteImportMode` = Single (개별 프레임 그대로), `spritePixelsToUnits` = **100**, `spritePivot` = **(0.5, 0.5)**
  - combo_attack은 이미 import됨 — 동일 설정인지 확인 후 정렬만 (불일치 시 통일, 기존 meta 수정 방향)
  - 파일명 순서 유지 (`frame_000` 정렬 — 어휘순/숫자 정렬 이슈 확인: 0~73, 000~103)
  Must NOT do: readOnly/재슬라이스, filterMode/compression 변경, attack 파이널 시트와 무관한 설정
  Parallelization: Wave 1 | Blocked by: 에디터 ready | Blocks: 3
  References: Unity CLI `unity status` → `unity command eval` + TextureImporter/SerializedObject (선례: Assets/Editor/EnemyPrefabBuilder.cs, 기존 v1 Todo 1)
  Acceptance criteria: 대상 PNG 메타가 전부 PPU 100 + pivot (0.5,0.5), 프레임 파일명 순서가 클립 생성 순서와 일치 (AssetDatabase.LoadAllAssetsAtPath 검증)
  QA scenarios: happy = 인스펙터에서 프레임별 스프라이트 분리 확인 / failure = PPU 미반영 시 AssetDatabase.Refresh() 후 재적용, Evidence .omo/evidence/task-7-v2-todo1.log
  Commit: Y | `config: import player attack/combo/dash/jump sprites at PPU 100`

#### Todo 2: idle/walk 스프라이트 시트 슬라이스
- [x] 2. **File/Directory**: `Assets/sprite/player/player/{idle/sprite sheets/idle.png, walk/sprite sheets/walk.png}` (+ .meta)
  What to do: 시트 크기 확인 후 슬라이스 —
  - **idle.png 460×55 = 10셀 × 46px** (aseprite: 10프레임, 64×64, 60ms 확정 — 아날시스: 460/10=46셀, 투명 분리자로 셀 경계 일치 확인)
  - **walk.png 180×348 = 4열 × 6행 배치 (45×58 셀)** — 단 aseprite 소스는 26프레임(53×64, 50ms)으로 시트와 프레임 수 불일치(24 vs 26) → **에디터에서 스프라이트 에디터로 실제 셀 그리드 확인 필수** (누락/중복 프레임 여부, 셀 크기 확정)
  - `spriteImportMode` = Multiple + Sprite Editor 슬라이스 (셀 기반, pivot (0.5,0.5)), PPU 100
  - walk 옆 `from idle.png`(90×58) 존재 — 용도 불명, 범위 제외(건드리지 않음)
  Must NOT do: 시트 원본 수정/리네임, 추측으ロ 슬라이스 고정 (사용자 확인 전 적용 금지)
  Parallelization: Wave 1 | Blocked by: 에디터 ready + 사용자 규격 확인 | Blocks: 3
  References: Unity Sprite Editor API (TextureImporter.spritesheet, SecondarySpriteTexture), 기존 v1 idel/walk 참조 해제 상태
  Acceptance criteria: 슬라이스된 스프라이트 수 = 셀 그리드 수, 이름 규칙 `idle_0..N` / `walk_0..N`, pivot 통일
  QA scenarios: happy = 인스펙터에서 그리드 스프라이트 확인 / failure = 셀 크기 오류 시 사용자 확인 값으로 재슬라이스, Evidence .omo/evidence/task-7-v2-todo2.{png,log}
  Commit: Y | `config: slice idle/walk sprite sheets`

### Wave 2: 애니메이션 클립 + 컨트롤러 재구성

#### Todo 3: PlayerAnimationBuilder.cs — 클립 8개 생성 (새 에셋 기준)
- [x] 3. **File/Directory**: `Assets/Editor/PlayerAnimationBuilder.cs` (신규/재작성) + `Assets/Animations/PlayerIdle/PlayerWalk/PlayerJump/PlayerDash/PlayerDoubleJump/PlayerFall/PlayerAttack/PlayerComboAttack.anim` (전부 신규)
  What to do: [MenuItem] 에디터 스크립트 —
  - 기존 v1 builder 패턴(SetObjectReferenceCurve) 유지하되 **새 에셋 GUID/fileID로 교체**, 기존 `PlayerIdle/Walk/Jump.anim`은 새 에셋 기준으로 **덮어쓰기 (재생성)**
  - 딜레이를 파일명에서 파싱: `delay-0.06s` → 0.06s 간격 키프레임, `delay-0.07s` → 0.07s (`frame_48_delay-0.02s` 같은 이질 딜레이도 String.Split 후 적용)
  - 클립 사양 (사용자 확정 프레임 범위):
    | 클립 | 프레임 (재생 순서) | 간격 | 길이 | 루프 |
    |---|---|---|---|---|
    | PlayerIdle | idle_0..9 (10) | 0.06s | 0.6s (aseprite 60ms) | Loop |
    | PlayerWalk | walk_0..N (시트 24셀 — 에디터 확정) | 0.05s | 시트 확정 후 | Loop |
    | PlayerJump | jumpanddash/jump/frame_09~16 (8) | 0.07s | 0.56s | **단발 (Once)** |
    | PlayerDash | **명시적 순서**: dash/frame_09,10,11 → 14~25 → **11,10,09 역순** (18 키프레임, 15 파일 재사용) | 0.07s | 1.26s | **단발 (Once)** |
    | PlayerDoubleJump | doublejump/frame_25~31 (7) | 0.07s | 0.49s | **단발 (Once)** |
    | PlayerFall | fall/frame_31~39 (9) | 0.07s | 0.63s | **단발 (Once)** — 높이차는 전이에서 처리 |
    | PlayerAttack | attack/frame_00~73 (74) | 0.06s | 4.44s | **단발 (Once)** — 전부 재생 확정 |
    | PlayerComboAttack | combo_attack/frame_000~103 (104) | 0.06s | 6.24s | **단발 (Once)** — 전부 재생 확정 |
  - **dash는 폴더 정렬이 아니라 명시적 순서 배열 사용** (09,10,11,14..25,11,10,09) — 스프라이트 재사용은 같은 GUID 반복 참조로 구현
  - jump/doublejump/fall은 프레임 범위가 겹침(25, 31) — 폴더가 분리되어 있으므로 GUID 충돌 없음
  Must NOT do: .anim YAML 수작업, Enemy 컨트롤러 수정, 기존 PlayerAnimator.controller 손상 (재구성은 Todo 4)
  Parallelization: Wave 2 | Blocked by: 1,2 | Blocks: 4,5
  References: v1 builder의 nameFileIdTable/SetObjectReferenceCurve 패턴, 새 에셋 meta의 spriteID/internalID (attack 등은 import 후 생성됨)
  Acceptance criteria: 클립 6개 존재, 각 클립 프레임 수/딜레이 간격/루프 설정 정확 (AssetDatabase 로드 후 키프레임 카운트/타임 샘플 검증)
  QA scenarios: happy = [MenuItem] → 클립 6개 생성 + 에디터에서 키프레임 확인 / failure = 프레임 누락 시 파일명 정렬 재확인, Evidence .omo/evidence/task-7-v2-todo3.log
  Commit: Y | `feat: rebuild player animation clips with new assets`

#### Todo 4: PlayerAnimator.controller 재구성 — 상태 8개 + 파라미터 6개
- [x] 4. **File/Directory**: `Assets/Animations/PlayerAnimator.controller` (재작성)
  What to do: 기존 컨트롤러(3상태)를 확장 —
  - 파라미터: `Speed` (float), `IsGrounded` (bool), `IsAttacking` (bool), `IsComboAttacking` (bool), `IsDashing` (bool), **`IsFalling` (bool — velocity.y < 0 감지, 점프/2단점프/낙하 분기에 사용)** — 기존 Speed/IsGrounded 유지, 4개 추가
  - 상태: **Idle**(PlayerIdle), **Walk**(PlayerWalk), **Jump**(PlayerJump), **DoubleJump**(PlayerDoubleJump), **Fall**(PlayerFall), **Dash**(PlayerDash), **Attack**(PlayerAttack), **ComboAttack**(PlayerComboAttack) — Default = Idle
  - 전이표 (높이차 대응: Jump/DoubleJump 상승부 도중 velocity.y<0 → Fall 전이, 맨 아래 높이와 무관하게 낙하 상태로 스위치):
    | From | To | 조건 |
    |---|---|---|
    | Any | Attack | `IsAttacking == true` |
    | Any | ComboAttack | `IsComboAttacking == true` |
    | Attack | Idle | `IsAttacking == false` |
    | ComboAttack | Idle | `IsComboAttacking == false` |
    | Idle | Walk | `Speed > 0.1 && IsAttacking == false && IsComboAttacking == false` |
    | Walk | Idle | `Speed < 0.1 && IsAttacking == false && IsComboAttacking == false` |
    | Any | Dash | `IsDashing == true` |
    | Dash | Idle/Walk | `IsDashing == false` (Speed에 따라) |
    | Idle/Walk | Jump | `IsGrounded == false && IsFalling == false && IsDashing == false && IsAttacking == false && IsComboAttacking == false` |
    | Jump | Fall | `IsFalling == true` (정점 통과) |
    | Any(Jump) | DoubleJump | `IsGrounded == false && (2단 점프 입력)` — PlayerController가 IsFalling 토글 대신 **IsDoubleJumping** 트리거/딜레이 또는 IsGrounded 유지 방식으로 분기 (구현 시 결정, 파라미터 추가 가능) |
    | DoubleJump | Fall | `IsFalling == true` (2단 정점 통과 — 상승이 짧으므로 IsFalling이 4프레임 이내 켜짐) |
    | Fall | Idle/Walk | `IsGrounded == true` (Speed에 따라) |
  - 전이 duration 0.05~0.1, 공격/점프/대시/낙하 클립은 Loop 해제(단발)이므로 Exit Time 미사용(전이 즉시)
  Must NOT do: bool 4개짜리 Enemy 패턴 복사, 클립 미연결 상태로 저장
  Parallelization: Wave 2 | Blocked by: 3 | Blocks: 6 | Can parallelize with: 5
  References: UnityEditor.Animations API (기존 v1 Todo 3), 기존 컨트롤러의 Speed/IsGrounded 파라미터 유지
  Acceptance criteria: 파라미터 5개 + 상태 6개 + 전이 전체, 각 m_Motion이 새 클립 GUID 참조, Default Idle
  QA scenarios: happy = Animator 창에서 6상태 그래프 확인 / failure = 전이 누락 시 조건 확인, Evidence .omo/evidence/task-7-v2-todo4.log
  Commit: Y | `feat: rebuild player animator controller with attack states`

### Wave 3: 코드 연동 + 프리팹 적용

#### Todo 5: PlayerCombat.cs + PlayerController.cs 입력/Animator 연동 — Fire2→콤보, 패링→E, 이단점프/낙하 분기
- [x] 5. **File/Directory**: `Assets/Scripts/PlayerCombat.cs`, `Assets/Scripts/PlayerController.cs` (수정)
  What to do: **게임플레이 판정/속도/점프력 로직 불변, 입력 바인딩/Animator 연동만 변경** —
  - `HandleAttackInput`: Fire1 유지 (기존)
  - `HandleComboInput`(신규): `Input.GetButtonDown("Fire2")` + 쿨다운 체크 → 콤보 어택 상태 시작 (`isComboAttacking = true`, 공격 판정은 기존 PerformAttack 판정 로직 재사용 or 콤보용 복사 — 데미지/쿨다운 값 불변)
  - `HandleParryInput`: `Input.GetButtonDown("Fire2")` → `Input.GetKeyDown(KeyCode.E)`로 교체
  - PlayerCombat `Update()`에 상태 갱신: `IsAttacking`/`IsComboAttacking` 종료 타이머 관리 (기존 attackEndTime 패턴 확장) + `animator.SetBool("IsAttacking", ...)`, `SetBool("IsComboAttacking", ...)`
  - PlayerController: **IsFalling** 갱신 (`!IsGrounded && rb.velocity.y < 0` → true — Jump/DoubleJump 상승 도중 정점 통과 감지, 공중 어디서든 2단 점프든 높이와 무관하게 동일 낙하 상태로 전이됨), **IsDashing** 연동 (대시 입력이 기존에 있다면 유지/없다면 IsDashing 파라미터는 false 상시 — 대시 클립 재생은 Dash 상태/파라미터로)
  - 2단 점프: 기존 maxJumps=2 공중 점프 로직 유지 — Animator가 Jump 상태에서 2단 입력 시 재점프 모션 재생(IsGrounded==false && 두 번째 점프 트리거). 정확한 파라미터 방식은 Todo 4 구현 중 결정
  - Animator 연동: `animator.SetBool("IsAttacking", isAttacking)`, `SetBool("IsComboAttacking", isComboAttacking)`, `SetBool("IsFalling", isFalling)`, `SetBool("IsDashing", isDashing)`
  Must NOT do: attackDamage/attackRange/attackCooldown/parryWindow* 값 변경, TryParry/OnParrySuccess 로직 수정, VFX/OverlapCircle 판정 변경, Fire1 공격 바인딩 변경, maxJumps/jumpForce/moveSpeed 변경
  Parallelization: Wave 3 | Blocked by: 3 (파라미터 명세) | Blocks: 6 | Can parallelize with: 4
  References: Assets/Scripts/PlayerCombat.cs:60-92 (Update/입력부), :94-123 (PerformAttack), :141-162 (TryParry), 기존 input 바인딩 (ProjectSettings/InputManager Fire1/Fire2)
  Acceptance criteria: 컴파일 에러 0, 좌클릭=attack/우클릭=combo/E=parry 매핑, IsAttacking/IsComboAttacking/IsFalling/IsDashing 파라미터 실시간 갱신
  QA scenarios: happy = Play Mode에서 입력별 상태 전환 확인 / failure = 버튼 미인식 시 InputManager 매핑 확인, Evidence .omo/evidence/task-7-v2-todo5.log
  Commit: Y | `feat: rebind player combat input (Fire2 combo, E parry) and wire animator`

#### Todo 6: Player.prefab 적용 — 컨트롤러/스프라이트 교체
- [x] 6. **File/Directory**: `Assets/Prefabs/Player.prefab` (수정 — 에디터 API 경유)
  What to do: 에디터 라이브에서 —
  - Animator.runtimeAnimatorController → 새 PlayerAnimator.controller
  - SpriteRenderer.sprite → idle_0 (슬라이스된 idle.png의 0번 프레임)
  - 기존 컴포넌트 전부 유지 (PlayerController/PlayerCombat/PlayerHealth/Rigidbody2D/BoxCollider2D/groundCheck)
  Must NOT do: .prefab YAML 직접 편집, PlayerHealth/PlayerCombat 필드 변경, 콜라이더/리지드바디 크기 변경
  Parallelization: Wave 3 | Blocked by: 3,4,5 | Blocks: 7
  References: v1 Todo 5 선례 (AssetDatabase/PrefabUtility API), 새 idle_0 내부ID (Todo 2 결과)
  Acceptance criteria: 프리팹 YAML에서 새 컨트롤러 GUID + idle_0 스프라이트 참조, 재오픈 후 유지
  QA scenarios: happy = 씬 Player가 캐릭터 스프라이트로 표시 + Animator 창 6상태 / failure = 참조 끊김 시 GUID 대조, Evidence .omo/evidence/task-7-v2-todo6.png
  Commit: Y | `feat: apply new animator and sprites to player prefab`

### Wave 4: QA + 커밋

#### Todo 7: Play Mode 통합 QA
- [x] 7. **File/Directory**: 전체 (Player 프리팹 + 씬)
  What to do: 에디터 Play Mode 진입 → 시나리오:
  - S1 좌클릭: 공격 클립 재생 → 종료 후 Idle 복귀
  - S2 우클릭: 콤보 클립 재생 → 종료 후 Idle 복귀 (좌클릭과 겹치지 않음)
  - S3 E키: 패링 동작 (기존 패링 테스트 시나리오 재사용)
  - S4 이동/점프/2단점프/낙하/대시: Speed/IsGrounded/IsFalling/IsDashing 전환 정상, Jump→Fall 정점 전이, 2단점프 모션 재생, Dash 상태 전이 충돌 없음
  - S5 좌우 반전 유지 (localScale 기준, PlayerCombat 방향 판정 일치)
  - 콘솔 에러 0, 스크린샷 (Attack/ComboAttack/Idle/Walk/Jump/DoubleJump/Fall/Dash 각 1장)
  Must NOT do: 게임플레이 로직 수정, 씬 저장 (Main.unity 범위 외)
  Parallelization: Wave 4 | Blocked by: 6
  References: v1 Todo 6 패턴, 사용자 결정(입력 바인딩) 명세
  Acceptance criteria: S1~S5 전부 PASS + 콘솔 에러 0 + 스크린샷 6장 + 에디터 로그 저장
  QA scenarios: happy = 5시나리오 PASS / failure = 상태 전이 안 되면 전이 조건/파라미터 로그 확인, Evidence .omo/evidence/task-7-player-sprite-animation-v2.{png,log}
  Commit: Y | `test: verify player attack/combo/parry animation playback`

## Final verification wave
> Runs in parallel after ALL todos. ALL must APPROVE. Surface results and wait for the user's explicit okay before declaring complete.
- [ ] F1. Plan compliance audit — Todo 1~7 전부 구현, acceptance criteria 충족 (특히 입력 바인딩, 딜레이 반영, PPU 100)
- [ ] F2. Code quality review — PlayerCombat.cs 최소 수정 원칙(판정 로직 불변) 준수, 네임스페이스/SerializeField+Header 컨벤션
- [ ] F3. Real manual QA — 에디터 Play Mode에서 좌클릭/우클릭/E/이동/점프/대시 전환 + 끊김 없음 (스크린샷 6장)
- [ ] F4. Scope fidelity — player/ 폴더 외 미사용, ezgif-split 4.zip/잔여 프레임 미처리, 잔여 범위 준수

## Commit strategy
| Todo | 커밋 메시지 |
|---|---|
| 1 | `config: import player attack/combo/dash/jump/doublejump/fall sprites at PPU 100` |
| 2 | `config: slice idle/walk sprite sheets` |
| 3 | `feat: rebuild player animation clips with new assets` |
| 4 | `feat: rebuild player animator controller with attack states` |
| 5 | `feat: rebind player combat input (Fire2 combo, E parry) and wire animator` |
| 6 | `feat: apply new animator and sprites to player prefab` |
| 7 | `test: verify player attack/combo/parry animation playback` |

커밋 규칙: AGENTS.md §6 (`타입: 요약`, 작게·자주). 커밋 금지: `.omo/boulder.json`, `.omo/run-continuation/`, `.omo/notepads/`, `error_log.txt`, `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`. jumpanddash 분리(파일 이동)는 **별도 커밋으로 분리** (이번 범위에 섞지 않음).

## Success criteria
1. 에디터 Play Mode에서 **좌클릭 = 공격, 우클릭 = 콤보 어택** 애니메이션 재생, **E = 패링** 동작
2. Idle/Walk/Jump/DoubleJump/Fall/Dash/Attack/ComboAttack **8상태** 전환이 끊김 없이 자연스럽게 재생 (컨트롤러 전이 정상, 특히 Jump→Fall 높이차 전이)
3. 이동/점프 조작감 불변 (moveSpeed 8 / jumpForce 14 / maxJumps 2 유지), 물리 설정 불변
4. 공격/패링 판정 로직 불변 (입력 바인딩만 변경)
5. dash 클립이 `09,10,11→14~25→11,10,09` 역순 재생 포함 18 키프레임으로 생성됨
6. 콘솔 에러 0, 임포트 설정/프리팹/클립/컨트롤러가 커밋되어 재오픈 후에도 유지

## 사용자 검수 대기 항목 (중요)
1. ~~**jumpanddash 경계**~~ → **확정됨**: jump 9~16 / dash 9~11+14~25+11~9역순 / doublejump 25~31 / fall 31~39 (폴더 분리 + 복제 완료)
2. ~~**idle/walk 시트 셀 크기**~~ → **확정됨**: walk 시트 24셀(4열×6행 45×58) 기준, idle 10셀(46px)
3. ~~**attack 클립 길이**~~ → **확정됨**: attack 74프레임(4.44s) + combo 104프레임(6.24s) **전부 재생**
4. **`ezgif-split 4.zip`(106 GIF)**: 처리 방식 결정 대기 (범위 제외 상태)
5. **`jumpanddash` 잔여 18장** (0~8, 40~48): idle 중복으로 보고 범위 제외 — 삭제/유지 결정 대기
6. **jump/doublejump/fall 높이차 연동**: 2단점프는 더 높이 뜨므로 Fall 도달 시간이 다름 — IsFalling(velocity.y<0) 기반으로 높이 무관 전이 (구현 중 세부 확정, QA에서 검증)