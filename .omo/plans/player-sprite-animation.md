# player-sprite-animation - Work Plan

## TL;DR (For humans)

**What you'll get:** 게임 속 파란 사각형 플레이어가 실제 캐릭터 이미지로 바뀌고, 가만히 있을 때(멍때리기), 걸을 때(걷기), 점프할 때(점프) 각각 알맞은 애니메이션이 재생됩니다.

**Why this approach:** Unity 에디터를 열어두고 실시간으로 작업해서 완성 직후 바로 확인합니다. 캐릭터 크기도 기존 몸통(1×1)과 일치하도록 맞춥니다.

**What it will NOT do:** 공격/패링 애니메이션은 만들지 않습니다 (준비된 이미지에 공격 컷이 없음 — 나중에 에셋 추가 시 연결). 적이나 레벨은 건드리지 않습니다.

**Effort:** Short
**Risk:** Low - 스프라이트 슬라이스 일부(pivot/크기)가 제각각이라 점프 애니메이션 정렬에 미세 조정 가능성
**Decisions to sanity-check:** 캐릭터 크기 기준을 PPU 100 (1 유닛)으로 통일한 것, 애니메이션이 12fps일 것

Your next move: 계획을 승인하면 에디터를 열고 실행 단계로 넘어갑니다. 전체 실행 상세는 아래에 있습니다.

---

> TL;DR (machine): Short effort, Low risk — Player 스프라이트 3종(PPU 100) + Idle/Walk/Jump 클립 3개 + PlayerAnimator.controller + PlayerController 연동 + 프리팹 적용 + Play Mode QA

## Scope
### Must have
- 스프라이트 임포트 설정: `Assets/sprite/player/`의 idle.png / revision-walking.png / jump.png PPU 149.25 → **100**, jump.png sprite pivot (0,0) → **(0.5, 0.5)** (idle/walk와 통일 — 안 그러면 점프 애니메이션에서 캐릭터가 왼쪽 아래로 어긋남)
- AnimationClip 3개: `Assets/Animations/PlayerIdle.anim` (idle_0~3, 12fps 루프), `PlayerWalk.anim` (walk_0~7, 12fps 루프), `PlayerJump.anim` (jump_0~3, 12fps 루프)
- `Assets/Animations/PlayerAnimator.controller`: 파라미터 **Speed (float)**, **IsGrounded (bool)**; 상태 Idle/Walk/Jump; 전이 6개 (아래 Todo 3-1 전이표)
- `Assets/Scripts/PlayerController.cs` 수정: Animator 참조 + Update에서 Speed/IsGrounded 파라미터 갱신 + MoveInput 부호로 localScale.x 반전 (이동/점프 물리 로직 불변)
- `Assets/Prefabs/Player.prefab`: SpriteRenderer.sprite = idle_0 할당, Animator 컴포넌트 부착 + PlayerAnimator.controller 연결
- Play Mode QA: Idle/Walk/Jump 전환 3시나리오 PASS + 콘솔 에러 0

### Must NOT have (guardrails, anti-slop, scope boundaries)
- 공격/패링 애니메이션 클립 (스프라이트에 공격 컷 없음 — 사용자 에셋 대기, VfxSlot과 무관)
- Enemy 프리팹/컨트롤러/EnemyPrefabBuilder.cs 수정
- PixelPerfectCamera / 2d-pixel-perfect 전면 적용 (PPU만 변경 — 카메라 세팅 범위 외)
- Input System 마이그레이션 / VfxSlot 적용 (combat-platformer Wave 4 별도)
- PlayerController의 이동/점프 파라미터 값 변경 (moveSpeed/jumpForce/maxJumps 불변)
- `Assets/Scenes/Main.unity` 수정
- Animator Controller를 YAML 수작업 편집 (에디터 라이브 중 — Unity CLI/에디터 API로만 변경)

## Verification strategy
> Zero human intervention - all verification is agent-executed.
- Test decision: **none** (Unity Play Mode 수동 시나리오 — 기존 계획서 패턴 동일, TDD 부적합)
- QA 도구: 에디터 라이브에서 `unity command editor_play` 진입 → 스크립트 평가로 파라미터 강제 + 씬 스크린샷/로그 수집
- Evidence: .omo/evidence/task-6-player-sprite-animation.png (+ .log)

## Execution strategy
### Parallel execution waves
- **Wave 1** (Todo 1): 스프라이트 임포트 설정 (PPU 100 + jump pivot) — 다른 모든 작업의 선행
- **Wave 2** (Todo 2~3): PlayerAnimationBuilder.cs 작성 → 클립 3개 + 컨트롤러 1개 생성 (같은 스크립트/한 번의 에디터 실행)
- **Wave 3** (Todo 4~5): PlayerController.cs 연동 → 프리팹 적용
- **Wave 4** (Todo 6): Play Mode QA + 커밋

전제: **에디터가 열려 있어야 함** — 실행 시작 전 사용자/워커가 `unity open`(또는 사용자 수동)로 에디터 오픈, `unity status` state=ready 확인 후 진행. 라이브 에디터 중 YAML 직접 편집 금지 — 모든 변경은 `unity command eval` / [MenuItem] (에디터 스크립트) 경유.

### Dependency matrix
| Todo | Depends on | Blocks | Can parallelize with |
| --- | --- | --- | --- |
| 1 (임포트 설정) | 에디터 ready | 2,3,5 | — |
| 2 (클립 생성) | 1 | 3,5 | — |
| 3 (컨트롤러 생성) | 2 | 5 | — |
| 4 (PlayerController 연동) | 2 (파라미터 명세) | 5 | 3 |
| 5 (프리팹 적용) | 2,3,4 | 6 | — |
| 6 (QA) | 5 | — | — |

## Todos
> Implementation + Test = ONE todo. Never separate.
<!-- APPEND TASK BATCHES BELOW THIS LINE WITH edit/apply_patch - never rewrite the headers above. -->

### Wave 1: 스프라이트 임포트 설정

#### Todo 1: PPU 100 통일 + jump pivot 정렬
- [x] 1. **File/Directory**: `Assets/sprite/player/idle.png`, `revision-walking.png`, `jump.png` (+ .meta)
  What to do: 에디터 라이브에서 TextureImporter 설정 변경 →
  - 3개 파일 `spritePixelsToUnits` 149.25 → **100** (SerializedObject로 `m_SpritePixelsToUnits` 설정 후 Apply)
  - `jump.png`만 추가로 sprite pivot → **(0.5, 0.5)** (현재 (0,0) — idle/walk와 불일치, 점프 시 캐릭터 어긋남 원인)
  Must NOT do: .meta 파일 직접 편집 (에디터 라이브 중 — unity-cli 스킬 원칙), PPU 이외 임포트 설정 변경 (filterMode/compression/alpha 유지)
  Parallelization: Wave 1 | Blocked by: 에디터 상태 ready | Blocks: 2,3,5
  References: Unity CLI `unity status` (state=ready 확인) → `unity command eval` + SerializedObject/TextureImporter API (선례: Assets/Editor/EnemyPrefabBuilder.cs의 SerializedObject 패턴); 현재 값 idle.png.meta:63, jump.png.meta:63,120
  Acceptance criteria: 3개 meta에서 `spritePixelsToUnits: 100`, jump.png.meta에서 `spritePivot: {x: 0.5, y: 0.5}` 확인 (에디터 인스펙터 또는 AssetDatabase로 검증)
  QA scenarios: happy = 에디터 라이브에서 텍스처 인스펙터 PPU=100 표시, 스프라이트가 100px = 1 유닛으로 렌더링 / failure = PPU 미반영 시 AssetDatabase.Refresh() 후 재적용, pivot 미반영 시 jump.png만 별도 재설정, Evidence .omo/evidence/task-1-player-sprite-animation.log
  Commit: Y | `config: set player sprite PPU to 100 and fix jump pivot`

### Wave 2: 애니메이션 클립 + 컨트롤러 생성

#### Todo 2: PlayerAnimationBuilder.cs — 클립 3개 생성
- [x] 2. **File/Directory**: `Assets/Editor/PlayerAnimationBuilder.cs` (신규) + `Assets/Animations/PlayerIdle.anim`, `PlayerWalk.anim`, `PlayerJump.anim` (신규)
  What to do: [MenuItem] 에디터 스크립트 작성 (EnemyPrefabBuilder.cs 선례 방식) →
  - `AnimationUtility.SetObjectReferenceCurve`로 Sprite 키프레임 생성 (스프라이트 GUID+fileID 참조: idle_0~3, walk_0~7, jump_0~3 — 슬라이스 이름은 meta의 nameFileIdTable과 일치)
  - PlayerIdle: idle_0~3, 12fps, 루프 (wrapMode Loop)
  - PlayerWalk: walk_0~7, 12fps, 루프 (wrapMode Loop)
  - PlayerJump: jump_0~3, 12fps, 루프 (wrapMode Loop) — 점프 클립은 실제 점프 자연스럽게 1회 재생 후 Idle 복귀(전이에서 관리, 클립 자체는 Loop 유지)
  - 파일 저장: `Assets/Animations/` (PlayerController와 같은 폴더 규칙)
  Must NOT do: .anim YAML 수작업, EnemyAnimator.controller 수정, 스프라이트 재슬라이스
  Parallelization: Wave 2 | Blocked by: 1 | Blocks: 3,5 | Can parallelize with: —
  References: Assets/Editor/EnemyPrefabBuilder.cs (컨트롤러 생성 선례), sprite 메타의 `spriteID: <guid>`/`internalID` (idle.png.meta:127-128 등), AnimationUtility.SetObjectReferenceCurve (UnityEditor API — Editor 폴더 스크립트 필수)
  Acceptance criteria: `Assets/Animations/PlayerIdle.anim`/`PlayerWalk.anim`/`PlayerJump.anim` 존재, 각각 프레임 수(4/8/4)와 마지막 키프레임 wrapMode Loop 설정 확인 (AssetDatabase.LoadAllAssetsAtPath로 Sprite 키프레임 카운트 검증)
  QA scenarios: happy = [MenuItem] 실행 → 클립 3개 생성 + 에디터에서 클립 열면 스프라이트 키프레임 보임 / failure = 키프레임 누락 시 SetObjectReferenceCurve의 타임 샘플링(0.0833s 간격) 확인, 잘못된 스프라이트 참조 시 nameFileIdTable 대조, Evidence .omo/evidence/task-2-player-sprite-animation.log
  Commit: Y | `feat: add player idle/walk/jump animation clips`

#### Todo 3: PlayerAnimator.controller — 상태 머신
- [x] 3. **File/Directory**: `Assets/Animations/PlayerAnimator.controller` (신규)
  What to do: PlayerAnimationBuilder.cs에 컨트롤러 생성 루틴 포함 (또는 별도 MenuItem) →
  - 파라미터: `Speed` (float, 기본 0), `IsGrounded` (bool, 기본 true)
  - 상태 3개 + 클립 연결: **Idle**→PlayerIdle, **Walk**→PlayerWalk, **Jump**→PlayerJump, DefaultState = Idle
  - 전이표 (Worker가 그대로 구현):
    | From | To | 조건 |
    |---|---|---|
    | Idle | Walk | `Speed > 0.1` |
    | Walk | Idle | `Speed < 0.1` |
    | Idle | Jump | `IsGrounded == false` |
    | Walk | Jump | `IsGrounded == false` |
    | Jump | Idle | `IsGrounded == true && Speed < 0.1` |
    | Jump | Walk | `IsGrounded == true && Speed > 0.1` |
  - 전이 duration 0.05~0.1 (끊김 없음 유지)
  Must NOT do: bool 파라미터 4개짜리 Enemy 패턴 복사 (Speed float + IsGrounded bool 고정), 클립 미연결 상태로 저장
  Parallelization: Wave 2 | Blocked by: 2 | Blocks: 5 | Can parallelize with: —
  References: Assets/Animations/EnemyAnimator.controller (AnimatorController YAML 구조: 1107 상태머신/1102 상태/1101 전이/9100000 컨트롤러), UnityEditor.Animations API (AnimatorController.CreateAnimatorControllerAtPath, AddParameter, AddMotion 등)
  Acceptance criteria: 컨트롤러에 파라미터 Speed/IsGrounded + 상태 3개 + 전이 6개, 각 상태 m_Motion이 해당 클립 GUID 참조, default state Idle
  QA scenarios: happy = 에디터 Animator 창에서 3상태/6전이 그래프 확인, 클립 연결 확인 / failure = 전이 누락 시 AddTransition 조건 확인, 클립 미연결 시 m_Motion 값 확인, Evidence .omo/evidence/task-3-player-sprite-animation.log
  Commit: Y | `feat: add player animator controller`

### Wave 3: 코드 연동 + 프리팹 적용

#### Todo 4: PlayerController.cs Animator 연동
- [x] 4. **File/Directory**: `Assets/Scripts/PlayerController.cs` (수정)
  What to do: 최소 수정 —
  - `[Header("Animation")] [SerializeField] private Animator animator;` 추가 (없으면 Awake에서 GetComponent)
  - Update() 끝에: `animator.SetFloat("Speed", Mathf.Abs(moveInput)); animator.SetBool("IsGrounded", isGrounded);` — 단, isGrounded는 FixedUpdate 갱신이므로 CheckGrounded 결과를 그대로 사용
  - 좌우 반전: `if (moveInput != 0) transform.localScale = new Vector3(Mathf.Sign(moveInput), 1f, 1f);` (Update에서 — **localScale.x 부호 유지 필수** — PlayerCombat.cs:101,179가 공격 방향 판정에 사용)
  Must NOT do: moveSpeed/jumpForce/maxJumps 값 변경, 물리(질량/중력/interpolate) 변경, Update/FixedUpdate 구조 변경, SpriteRenderer.flipX 방식으로 반전 (localScale과 혼용 금지)
  Parallelization: Wave 3 | Blocked by: 2 (파라미터 명세) | Blocks: 5 | Can parallelize with: 3
  References: Assets/Scripts/PlayerController.cs:29-47 (Update/FixedUpdate 구조), :60-61 (IsGrounded/MoveInput — 이미 존재, 중복 추가 금지), Assets/Scripts/PlayerCombat.cs:101,179 (localScale.x 의존 — sway 주의)
  Acceptance criteria: 빌드 컴파일 에러 0 (배치 컴파일 또는 에디터 콘솔), 프리팹/씬의 Player 인스턴스에서 Animator 파라미터가 이동/점프 상태와 연동 (Play Mode에서 Inspector 확인)
  QA scenarios: happy = Play Mode에서 가만히(Speed=0/Idle), 이동(Speed=1/Walk), 점프(IsGrounded=false/Jump) 파라미터 변화 / failure = 컴파일 에러 시 구문 확인, 애니메이션 미전환 시 파라미터 값 로그로 확인, Evidence .omo/evidence/task-4-player-sprite-animation.log
  Commit: Y | `feat: wire player controller to animator`

#### Todo 5: Player.prefab 스프라이트 + Animator 적용
- [x] 5. **File/Directory**: `Assets/Prefabs/Player.prefab` (수정 — 에디터 API 경유)
  What to do: 에디터 라이브에서 Prefab을 열고 (또는 ScriptableObject/직렬화 API로) —
  - SpriteRenderer.sprite = `idle_0` (Assets/sprite/player/idle.png 내부ID 8793639957104055765)
  - SpriteRenderer.sortingOrder 0 유지, color 흰색(1,1,1,1)으로 복원 (현재 파란색 (0.2,0.6,1) — PlayerHealth가 originalColor로 저장하므로 프리팹 기본값을 흰색으로)
  - Animator 컴포넌트 추가 + `runtimeAnimatorController` = PlayerAnimator.controller
  - 기존 컴포넌트(PlayerController/PlayerCombat/PlayerHealth/Rigidbody2D/BoxCollider2D/groundCheck) 전부 유지
  - Prefab 저장 (PrefabUtility.SavePrefabAsset)
  Must NOT do: .prefab YAML 직접 편집 (에디터 라이브 — unity-cli 원칙), PlayerHealth/PlayerCombat 필드 변경, 콜라이더/리지드바디 크기 변경
  Parallelization: Wave 3 | Blocked by: 2,3,4 | Blocks: 6
  References: Assets/Prefabs/Player.prefab:72-130 (SpriteRenderer), :131-157 (Rigidbody2D), color 현재값 :122, idle_0 내부ID idle.png.meta:127-128, Animator API (GameObject.AddComponent<Animator> + runtimeAnimatorController)
  Acceptance criteria: 프리팹 YAML에서 SpriteRenderer m_Sprite가 idle_0 내부ID 참조, color (1,1,1,1), Animator 컴포넌트 + 컨트롤러 GUID 연결 — 에디터 재오픈 후에도 유지
  QA scenarios: happy = 씬의 Player가 더 이상 파란 사각형이 아니라 캐릭터 스프라이트로 보임, Animator 창에 컨트롤러 연결 표시 / failure = 스프라이트 미표시 시 m_Sprite 참조/매터리얼 확인, Animator 미연결 시 GUID 대조, Evidence .omo/evidence/task-5-player-sprite-animation.png
  Commit: Y | `feat: apply sprites and animator to player prefab`

### Wave 4: QA + 커밋

#### Todo 6: Play Mode 통합 QA
- [x] 6. **File/Directory**: 전체 (Player 프리팹 + 씬)
  What to do: 에디터 Play Mode 진입 → 시나리오 3개 실행 (씬에 Player 인스턴스가 있어야 함 — Main.unity의 Player가 프리팹 인스턴스인지 확인, 아니면 프리팹 오버라이드 반영):
  - S1 Idle: 입력 없이 대기 → Speed=0, IsGrounded=true, Idle 애니메이션 재생 (정지 프레임 유지/루프)
  - S2 Walk: 좌우 이동 (A/D) → Speed>0, Walk 루프, 최대 이동 중에도 끊김 없음
  - S3 Jump: 점프(스페이스, 이단점프 포함) → IsGrounded=false, Jump 재생 → 착지 시 Idle/Walk 복귀 (Speed에 따라)
  - 추가: 좌우 반전 시 캐릭터 뒤집힘 + 공격 방향(PlayerCombat) 일치, 콘솔 에러 0, 스프라이트가 콜라이더(1×1)와 심하게 어긋나지 않음
  Must NOT do: 게임플레이 로직 수정, 씬 저장 (Main.unity 범위 외 — 단, 프리팹 변경사항이 씬 인스턴스에 반영 안 되면 프리팹 오버라이드만 갱신)
  Parallelization: Wave 4 | Blocked by: 5
  References: combat-platformer Wave 6 통합 테스트 패턴 (.omo/plans/combat-platformer.md:351-366), Evidence 경로 규칙
  Acceptance criteria: S1/S2/S3 전부 PASS + 콘솔 에러 0 + 스크린샷(Idle/Walk/Jump 각 1장) + 에디터 로그 저장 — 실패 시 해당 Wave todo로 환류
  QA scenarios: happy = 3시나리오 PASS (파라미터 Inspector/로그로 확인) / failure = Jump→Idle 복귀 안 되면 전이 조건(Speed 임계 0.1) 확인, Walk 중 끊김/점프 시 스프라이트 점프(어긋남) 시 pivot/PPU 재확인, Evidence .omo/evidence/task-6-player-sprite-animation.{png,log}
  Commit: Y | `test: verify player sprite animation playback`

## Final verification wave
> Runs in parallel after ALL todos. ALL must APPROVE. Surface results and wait for the user's explicit okay before declaring complete.
- [x] F1. Plan compliance audit — Todo 1~6 전부 구현, acceptance criteria 충족 (특히 PPU 100, 전이 6개, localScale 반전 유지)
- [x] F2. Code quality review — PlayerController.cs 최소 수정 원칙 준수, 네임스페이스/SerializeField+Header 컨벤션, 에셋 슬롯 규칙(null 안전) 영향 없음
- [x] F3. Real manual QA — 에디터 Play Mode에서 Idle/Walk/Jump 전환 + 끊김 없음 + 콜라이더 정렬 (스크린샷 3장)
- [x] F4. Scope fidelity — 공격 애니메이션/Enemy/씬/VfxSlot 미변경 (Must NOT have 준수)

## Commit strategy
| Todo | 커밋 메시지 |
|---|---|
| 1 | `config: set player sprite PPU to 100 and fix jump pivot` |
| 2 | `feat: add player idle/walk/jump animation clips` |
| 3 | `feat: add player animator controller` |
| 4 | `feat: wire player controller to animator` |
| 5 | `feat: apply sprites and animator to player prefab` |
| 6 | `test: verify player sprite animation playback` |

커밋 규칙: AGENTS.md §6 (`타입: 요약`, 작게·자주). 커밋 금지: `.omo/boulder.json`, `.omo/run-continuation/`, `.omo/notepads/`, `error_log.txt`, `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`. Animations/EnemyAnimator.controller, Editor/EnemyPrefabBuilder.cs 등 본 작업과 무관한 미커밋 변경은 **별도 커밋으로 분리** (이번 범위에 섞지 않음).

## Success criteria
1. 에디터 Play Mode에서 플레이어가 **캐릭터 스프라이트**(파란 사각형 아님)로 표시됨
2. Idle → Walk → Jump 전환이 끊김 없이 자연스럽게 재생됨 (컨트롤러 전이 6개 정상 동작)
3. 좌우 이동 시 캐릭터 방향 반전 + 공격 방향(PlayerCombat) 일치 유지
4. 이동/점프 조작감 불변 (moveSpeed 8 / jumpForce 14 / maxJumps 2 유지, 물리 설정 불변)
5. 콘솔 에러 0, 임포트 설정/프리팹/클립/컨트롤러가 커밋되어 재오픈 후에도 유지

## Success criteria
