# platformer-basic - Draft

## Intent
CLEAR - 사용자가 2D 플랫포머 게임의 기본 구색을 명확히 요청함

## Review Required
false - 고정밀 리뷰 불필요

## Components
1. **Player Controller** - 이동, 점프, 벽타기 등 기본 플랫포머 캐릭터 제어
2. **Cinemachine Camera** - 부드러운 플레이어 따라가기 + 구역별 전환
3. **Basic Level** - 바닥, 플랫폼, 벽으로 구성된 기본 레벨
4. **Physics Setup** - 2D 물리 설정 ( Gravity, Layer, Contact)

## Status
executing - **2026-09-16 최종: Todo 7~13 전부 완료**. 카메라 통합 테스트 5/5 PASS. 남은 리스크(카메라 Y 데드존 튜닝)는 인간 팀 판단 대기.

## 협업 규칙 (2026-09-16 사용자 지시)
- **Todo 13까지 "그만"이라고 하기 전까지 자동으로 계속 진행, 질문 금지.**
- (2026-09-15 규칙: 작업 단계 시작 전 수동/자동 방식 질문 — 이후 자동 진행으로 override됨)

## 2026-09-16 세션 기록
- ✅ **Todo 9 완료** (bg_d3c9e606): LevelBounds(PolygonCollider2D 40×10 rect, isTrigger) + CinemachineConfiner2D(BoundingShape2D→LevelBounds, Damping=1.0, OversizeWindow.Enabled=True, MaxWindowSize=5.4)
  - 학습: `eval_file`이 inline eval보다 안정적 (PowerShell 인수 분리 회피), eval 코드는 메서드 본문만, Cinemachine 타입은 리플렉션으로 검색, SerializedObject로 중첩 필드 설정
- ✅ **Todo 10 완료** (bg_eca1a1e8): CameraZone 씬 구성
  - Zone1: BoxCollider2D(isTrigger, 40×10) + CameraZone(zoneCamera→CM vcam1, priorityEnabled=10, priorityDisabled=0)
  - **이슈**: CameraZone.cs 컴파일 에러 — `Priority`가 float가 아닌 `PrioritySettings` 구조체 → `zoneCamera.Priority.Value = (int)priorityEnabled/Disabled`로 최소 수정(2줄) 후 `Tundra build success` 확인
  - 학습: add_component가 미컴파일 타입 해결 불가 → AssetDatabase.Refresh() 후 eval_file+reflection으로 추가
- ✅ **Player→Prefab 교체 + Ground 레이어 완료** (bg_9c5a94d1): 19/19 검증 통과
  - Scene Player → `PrefabUtility.SaveAsPrefabAsset`으로 prefab 인스턴스화 (Connected, 위치 (0,3,0) 유지, prefab에 PlayerController 포함)
  - Ground 레이어: index 6에 추가 → **중복 발견(index 3 기존 uncommitted + index 6)** → index 6 정리, index 3 단일 유지, GetMask("Ground")=8
  - groundLayer=8을 prefab + scene 인스턴스에 할당
  - 학습: eval_file 5000ms 타임아웃 → CLI `--timeout` 미전달 → **`--detach` + `unity job wait`로 해결**
- ✅ **Todo 11 완료** (bg_aa79d171): 기본 레벨 생성
  - LevelBounds bounds 확인: min(-10,0) max(30,10). Floor(20×1, top=0) + WallL/R(0.5×8 at ±10) + Platform A-D (3×0.5, y=2.5/4/5.5/7). 전부 Ground 레이어/isTrigger=false. Quad 방식 생성. 씬 저장 완료.
- ✅ **Todo 12 완료** (bg_899ce247): 레벨 장식
  - Main Camera: clearFlags Skybox→SolidColor (필수 변경) + bg (0.53,0.81,0.98) 하늘색
  - Cloud_1..3 + Hill_1..3 (z=-1, sortingOrder=-20, 콜라이더 없음). 씬 저장 완료.
- ✅ **Todo 13 완료** (bg_89f9aebe): 최종 통합 테스트 — **카메라 5/5 PASS**
  - YAML 검증: Player SceneRoots 포함, ortho size 5.4, vcam1/LevelBounds/Zone1 존재 ✅
  - DefaultBlend.Time 2→1 (경로 `DefaultBlend.Time`, `m_Time` 아님 — SerializedObject로 발견)
  - Play Mode 5단계: PASS_IDLE/MOVE_RIGHT/JUMP/CONFINER_R/CONFINER_L 전부 YES
  - 발견·수정: Player.prefab FreezeRotation(m_Constraints:4) + Player 위치 (0,3)→(0,0.5) 정렬, Game view 16:9(1920×1080) 설정, 씬 중복 PlayerController 제거
  - 카메라 Y 추적 조사: DeadZone Size y=0.15(±0.81) → **데드존 내 점진 이동 무시 확인** — 대형 변위는 추적(2.40→6.00)하나 점프 시 느릴 수 있음 → **인간 판단 대기** (DeadZone y 0.05 / Damping y 0.5 권장)
  - 임시 스크립트(TestCameraValidator.cs, TempFollowProbe.cs) 삭제, 씬 저장(78,182 bytes), DefaultBlend Time:1 검증

## ✅ 플랜 전체 완료 (2026-09-16)
- **Todo 0~13 전부 완료** — 카메라 시스템 5/5 PASS (idle/move/jump/confiner R/L)
- 남은 작업(인간 승인 필요):
  1. **카메라 Y 데드존/댐핑 튜닝** (DeadZone y 0.15→0.05, Damping y 1.5→0.5 권장 — 이단점프 대비)
  2. **Todo 6 Animator + 스프라이트 보강** (아트 에셋 확보 후)
  3. **커밋** — bg_9c5a94d1 이후의 모든 변경은 아직 커밋 안 됨 (human 확인 후)

## ⏸ 사용자 중단 (2026-09-16, "지금은 여기서 끝내고 이따가 에셋 교체할게")
- 자동 진행 종료. 모든 Todo는 완료 상태이며 게임은 플레이 가능.
- **다음 세션 시작점**: 사용자가 아트 에셋 교체 진행 → Todo 6 (Animator) + 레벨/장식 스프라이트 적용이 첫 작업. 선택사항으로 카메라 Y 튜닝(DeadZone/Damping)과 미커밋 변경사항 커밋.

## Todo 11 레벨 생성 — ✅ 완료 (bg_aa79d171, 2026-09-16)
- 바닥: Floor 20×1 (top edge y=0), WallL(-9.75,4)/WallR(10.25,4) 0.5×8, Platform A(3,2.5) B(-4,4) C(6.5,5.5) D(0.5,7) 3×0.5
- 전부 Ground 레이어(index 3), isTrigger=false
- LevelBounds bounds: min(-10,0) max(30,10) — 벽/바닥/플랫폼 전부 내부 배치 확인

## 2026-09-14 세션 기록 (Unity CLI 경유 — MCP 미사용)
- ✅ **Todo 4**: Player Prefab 생성 완료
  - `Assets/Prefabs/Player.prefab` (GUID 133305f6459fbed4abc7f042b64c0962)
  - Player GameObject (instanceId -2212): SpriteRenderer(파란색), Rigidbody2D(Gravity 3, **Interpolate**, Continuous, FreezeRotation), BoxCollider2D, tag=Player, 위치 (0,3,0)
  - groundCheck 자식 (위치 0,-0.5,0), `Assets/Prefabs` 폴더 생성 (GUID 730208b1f6344724582c0f80a996fe3b)
  - ⚠️ Scene의 Player는 아직 Prefab 인스턴스로 전환 안 됨 (Hierarchy에 일반 GameObject로 존재)
- ✅ **Todo 5**: PlayerController.cs 이미 존재 확인 — 요구사항 모두 충족 (moveSpeed 8, jumpForce 14, maxJumps 2, Update 읽기/FixedUpdate 설정, OverlapCircle)
- ✅ **Todo 0-2**: PlayerController 부착 완료 (instanceId -2396) + groundCheck 참조 할당 완료
- ✅ **Todo 0-3**: Rigidbody2D 검증 — Interpolate/Continuous/Gravity3/freezeRotation 모두 확인
- 🔄 **Todo 6**: **지연 결정** (사용자 승인) — 스프라이트 에셋 없음, 카메라 우선 진행
- ⏸️ **Todo 7**: CinemachineBrain 추가 직전 중단 (finder 확인 후 add_component 예정)
  - 참고: Cinemachine 3.1.7, namespace Unity.Cinemachine

## Unity CLI 연결 상태 (2026-09-14)
- `unity status`: ready (port 7800, PID 14648, 6000.3.12f1)
- `unity command`: 100+ 커맨드 사용 가능 (create_gameobject, add_component, attach_script, create_prefab, eval 등)
- MCP 경로 대신 unity-cli 스킬 경로로 진행 중

## Wave 0 완료 기록 (2026-09-11, 오후)
- ✅ Todo 0-1: 패키지 6개 제거 완료
- ✅ Todo 0-2: 컴파일 성공 — Assembly-CSharp.dll 생성 (Library/ScriptAssemblies 확인)
- ✅ Todo 0-3: Player 설정 검증은 Player 오브젝트 생성(Todo 4)과 함께 수행 예정
- 🔄 **새 블로커**: Unity MCP가 opencode 세션에 연결 안 됨
  → `unity mcp` 서버 자체는 정상 (핸드셰이크 성공, unity-mcp 1.0.0-beta.6)
  → opencode.json의 MCP command를 절대 경로로 수정 (bg_2f1f25cf)
  → opencode 프로세스 시작 시각/설정 로드 여부 진단 중 (bg_491c3367)
  → 수정 후 opencode 재시작 필요

## Unity MCP 진단 결과 (2026-09-11)
- ✅ opencode.json (절대경로 수정 완료) — JSON OK, 경로 존재 확인
- ✅ 설정이 로드됨: opencode가 10:13에 `unity mcp`(PID 12736)를 자식으로 스폰함
- ✅ Editor 1개만 실행 중 (PID 10096, epsilongame) — 이중 에디터 충돌 없음
- ❌ **루트 원인 후보**: 사용자가 07:01에 수동 실행한 독립 `unity mcp`(PID 5324)가 Editor 브리지 연결을 선점
  → opencode의 인스턴스가 툴 등록 못 함 (opencode 로그에 unity MCP 활동 0건)
- 🔄 **해결 중**: PID 5324(=1068 쉘) 킬 + 전/후 테스트로 실증 (bg_5157d870)
  → 확인 후 opencode 재시작 → Unity MCP 연결 → Todo 4~13 진행

## Unity 스킬 설치 (2026-09-11, 사용자 요청 — bg_3410a5a3)
- 다운로드 폴더 `skills-main` (Unity-Technologies/skills 공식 31개) 중 **게임에 필요한 10개 선택**:
  unity-cli, unity-package-management, 2d-pixel-perfect, tilemap-palette-create,
  tilemap-ruletile-createempty, tilemap-ruletile-createfromsegment, sprite-editor,
  manage-sprite-atlas, urp-postprocessing, generate-editor-search-query
- 설치 위치: 프로젝트 `.opencode/skills/` (git 커밋 → 타 컴퓨터 이식 가능, 기존 unity-cli-mcp 이식 원칙과 일치)
- 제외 21개: new-unity-project(이미 프로젝트), migrate-birp-to-urp(이미 URP), physics-3d-collision(2D),
  shader/validate-renderer(커스텀 렌더러 없음), 서비스/광고/결제(범위外), audio/optimize/localization/ui(범위外),
  sprite-segment-3x3grid(분석 전용), initialize-ai-navigation(적 없음)
- 참고: `com.unity.pipeline` 0.6.0-exp.1 이미 설치됨 → unity-cli 스킬이 실행 중 Editor 직접 제어 가능 (MCP와 별개 경로)
- ⚠️ 스킬 로드에는 opencode 재시작 필요 → 설치 후 재시작 시 Unity MCP 연결 문제도 함께 재확인

## Approach
Unity 6 + Cinemachine 3.x 기반 2D 플랫포머 프레임워크 구축
→ **Wave 0 추가**: 불필요 AI 패키지 제거 → 컴파일 블로커 해소 → 기존 Wave로 진행

## Wave 0 실행 기록 (2026-09-11)
- ✅ Todo 0-1: 패키지 6개 제거 완료 (ai.assistant, ai.inference, multiplayer.center, visualscripting, timeline, collab-proxy)
- ⏳ Todo 0-2: 컴파일 확인 — Assembly-CSharp.dll 미생성, 마지막 Unity 크래시(ACCESS_VIOLATION)
  → Library/ScriptAssemblies, PackageCache, Bee, Temp 정리 + 에디터 재시작 진행 중
- ⏳ Todo 0-3: Player 설정 검증 (대기)

## Decisions Made
- Unity 6000.3.12f1 사용
- Cinemachine 3.1.7 패키지 사용
- 2D 물리 시스템 사용
- 기본 구조만 구현 (스토리, 적 캐릭터 등은 나중에)

## Pending Questions
- 플레이어 이동 스피드/점프력 등 튜닝 값
- 카메라 SmoothDamp 값