# platformer-basic - Work Plan

## 📌 진행사항 (2026-09-16 최종 기준)

### 완료 ✅
- **Todo 0-1**: 불필요 패키지 6개 제거 (ai.assistant, ai.inference, multiplayer.center, visualscripting, timeline, collab-proxy)
- **Todo 1**: Cinemachine 3.1.7 설치
- **Todo 2**: 2D 물리 설정 (Gravity Y=-20, Contact Offset 0.01, Auto Sync Transforms)
- **Todo 3**: 기본 Scene 구조 (Assets/Scenes/Main.unity)
- **Todo 0-2 (컴파일 부분)**: `Assembly-CSharp.dll` 생성 확인 — 컴파일 성공
- **Todo 4**: Player Prefab 생성 — `Assets/Prefabs/Player.prefab`
  - SpriteRenderer(파랑), Rigidbody2D(Gravity 3, **Interpolate**, Continuous, FreezeRotation), BoxCollider2D, tag=Player
  - groundCheck 자식 포함 (위치 0,-0.5,0)
- **Todo 5**: PlayerController.cs 존재 확인 — 요구사항 충족 (moveSpeed 8, jumpForce 14, maxJumps 2, Update/FixedUpdate 분리)
- **Todo 0-2 (부착 부분)**: PlayerController 부착 + groundCheck 참조 할당
- **Todo 0-3**: Rigidbody2D 설정 검증 (Interpolate/Continuous/Gravity3) ✅
- **Unity 스킬 10개 설치**: `.opencode/skills/` (unity-cli, unity-package-management, 2d-pixel-perfect, tilemap-* 3종, sprite-editor, manage-sprite-atlas, urp-postprocessing, generate-editor-search-query)
- **Todo 7 (2026-09-16)**: CinemachineBrain 설정 — Main Camera(instanceId 41870)에 추가, UpdateMethod=LateUpdate, BlendUpdateMethod=LateUpdate, Camera orthographic=true size=5.4 ✅ (DefaultBlend만 Inspector 확인 남음)
- **Todo 8 (2026-09-16)**: CM vcam1 생성 (instanceId -2158) + CinemachineCamera(-2170) + CinemachinePositionComposer(-2176) — Follow=Player, Damping(0.3,1.5), DeadZone(0.1,0.15), Lookahead 0.3/Smoothing 15, Lens=5.4 ✅
- **Todo 9 (2026-09-16)**: LevelBounds(PolygonCollider2D 40×10 rect, isTrigger) + CinemachineConfiner2D(BoundingShape2D→LevelBounds, Damping=1.0, OversizeWindow.Enabled=True, MaxWindowSize=5.4) ✅
  - 학습: `eval_file`이 inline `eval`보다 안정적, eval 코드는 메서드 본문만, Cinemachine 타입은 리플렉션으로 검색
- **Todo 10 (2026-09-16)**: Zone1(BoxCollider2D isTrigger 40×10) + CameraZone(zoneCamera→CM vcam1, priorityEnabled=10, priorityDisabled=0) ✅
  - **이슈 수정**: CameraZone.cs `Priority`는 float가 아닌 `PrioritySettings` 구조체 → `zoneCamera.Priority.Value = (int)priorityEnabled/Disabled`로 2줄 수정 후 컴파일 성공
- **Player→Prefab 인스턴스 교체 (2026-09-16, bg_9c5a94d1) ✅**: Scene Player를 `PrefabUtility.SaveAsPrefabAsset`로 prefab 인스턴스화 (PrefabInstanceStatus=Connected, 위치 (0,3,0) 유지, prefab 자산에 PlayerController 포함) + Ground 레이어 생성 (index 3, 중복 정리 완료) + groundLayer=8 할당 — **19/19 검증 통과**, 씬 저장 완료
- **Todo 11 (2026-09-16, bg_aa79d171) ✅**: 기본 레벨 생성
  - LevelBounds world bounds: min(-10,0) max(30,10), 40×10
  - Floor(0,-0.5) 20×1 top edge y=0 + WallLeft(-9.75,4)/WallRight(10.25,4) 0.5×8 + Platform A(3,2.5) B(-4,4) C(6.5,5.5) D(0.5,7) 3×0.5 — 전부 Ground 레이어, isTrigger=false, gray
  - Quad primitive 방식 (MeshCollider 제거 + BoxCollider2D + Sprites/Default material), 씬 저장 완료
- **Todo 12 (2026-09-16, bg_899ce247) ✅**: 레벨 장식
  - Main Camera: clearFlags Skybox→**SolidColor** + backgroundColor **RGBA(0.53, 0.81, 0.98)** 밝은 하늘색 (clearFlags 변경이 배경색 표시에 필수였음)
  - 장식 6개: Cloud_1..3 (z=-1, 알파 0.85 흰색) + Hill_1..3 (z=-1, 청록 회색) — sortingOrder=-20, 콜라이더 없음, LevelBounds 내부, 씬 저장 완료
- **Todo 13 (2026-09-16, bg_89f9aebe) ✅**: 최종 통합 테스트 — **카메라 5/5 PASS**
  - DefaultBlend.Time 2→**1** 수정 (SerializedObject, 경로 `DefaultBlend.Time` — `m_Time` 아님), 씬 YAML 검증 (Player SceneRoots 포함, camera size 5.4, vcam1/LevelBounds/Zone1 존재)
  - Play Mode 5단계 테스트 (TestCameraValidator): `PASS_IDLE/MOVE_RIGHT/JUMP/CONFINER_R/CONFINER_L` 전부 **YES**, TOTAL 5/5
  - 테스트 중 발견·수정: Player.prefab `m_Constraints: 4`(FreezeRotation) 추가, Game view 종횡비 16:9(1920×1080) 설정, 씬 중복 PlayerController 제거, Player 위치 (0, 0.5, 0) 정렬
  - 임시 스크립트(TestCameraValidator.cs, TempFollowProbe.cs) 삭제 완료, 씬 저장 완료 (78,182 bytes)

### 진행 중 🔄
- **없음** — 카메라 시스템 Wave 3~4 전부 완료. 아래 ⚠️ 남은 리스크를 인간 팀이 판단.

### ⚠️ 씬 파일 동기화 문제 (2026-09-16 발견 → Todo 13에서 해결)
`Main.unity`(440줄)가 라이브 Editor 상태와 심각하게 동기화 안 됨:
- **CM vcam1, LevelBounds, Zone1** → 씬 YAML에 없음 (라이브 Editor에만 존재, 미저장) → **Todo 11~13에서 여러 차례 SaveScene → YAML에 전부 반영됨 확인** ✅
- **SceneRoots에 Player 누락** — PrefabInstance(fileID 8368372036711421826) 존재하지만 root 목록에서 빠짐 → **Todo 13 Phase 0에서 SceneRoots에 포함 확인 (line 2844)** ✅
- **Main Camera orthographic size = 5** (목표 5.4) → **5.4 확인 ✅**
- **DefaultBlend Time = 2** (목표 1 sec) → **1로 수정 완료 ✅**
- **해결 방법**: `EditorSceneManager.SaveScene` 반복 호출로 라이브 상태를 디스크에 반영 (Todo 11, 12, 13에서 각각 저장, 최종 78,182 bytes)

### Todo 11 레벨 설계 (2026-09-16 사전 준비)
기준: Player 시작 위치 ≈ (0, 3), LevelBounds 40×10 (중심 미확정 — 태스크에서 확인 필요), camera size 5.4 (가로 약 19.2 @16:9)
- **바닥**: y=0, x=-10 ~ +10 (20타일, 1유닛씩), BoxCollider2D + SpriteRenderer(회색), **Ground 레이어** (bg_9c5a94d1에서 생성 예정)
- **벽**: 좌(-10, y=0~2), 우(+10, y=0~2) — LevelBounds 안쪽에 배치
- **플랫폼 3~4개**: 높이 다양화 (예: y=3, y=4.5, y=6), 점프력 14 / 중력 -20 기준 도달 가능 범위 내
- **첫 플랫폼 배치 주의**: Player는 y=3 시작 → 바닥(y=0)까지 낙하 후 플랫폼 점프 진행
- 충돌: 각 타일/플랫폼에 BoxCollider2D(isTrigger=false) — Player가 밟을 수 있어야 함
- SceneRoots에 Player 복원 필요 (씬 저장 시 root 누락 문제 해결)

### Todo 12~13 설계 (2026-09-16 사전 준비)
- **Todo 12 (장식)**:
  - Camera Background: 현재 (0.192, 0.302, 0.475) — 기본 파랑 유지 또는 밝은 하늘색으로 변경
  - 타일 스프라이트: SpriteRenderer 기본 사각형(흰색/회색)으로 통일 — 아트 에셋 없음 → 범위 외 (Todo 6과 동일하게 스프라이트 확보 후 진행)
  - 장식 오브젝트: 배경용 단색 사각형 몇 개 (범위 최소화)
- **Todo 13 (통합 테스트)** — 실행 순서:
  1. `unity command save_scene`으로 라이브 상태 저장 (씬 동기화 문제 해결 — CM vcam1/LevelBounds/Zone1/Player 포함)
  2. 저장 후 Main.unity YAML 검증: SceneRoots에 Player 포함 여부, 카메라 size 5→5.4 확인
  3. DefaultBlend Time 2→1 수정 (계획 기준)
  4. Play Mode 테스트: `unity command play` 또는 Editor 연결로 재생 — Player 이동/점프, 카메라 끊김 확인
  5. 카메라 검증 항목: Follow 부드러움, 점프 바운스 없음, Confiner 경계 밖 안 나감

### 대기 ⏳
- **Todo 6 (Animator)**: **지연 결정** (2026-09-14 사용자 승인) — 스프라이트 확보 후 진행
- **카메라 Y 추적 튜닝 (권장, 인간 판단 필요)**: 동작에 문제 없음(5/5 PASS)이나, PositionComposer `DeadZone Size y=0.15`(화면 높이 10.8 기준 ±0.81) + `Damping y=1.5`로 **데드존 내 점진 이동 시 카메라가 무시** — 점프 추적이 느려 이단점프 플랫포머에서 플레이어가 화면 밖으로 나갈 위험. 후보: DeadZone y 0.15→0.05, Damping y 1.5→0.5 후 수동 플레이 테스트
- **스프라이트/사운드 보강**: 아트 에셋 없음 — Todo 6과 동일하게 에셋 확보 후 진행 (레벨 타일 + 장식 스프라이트)

### 중요 작업 메모 ⚠️
- ~~Scene의 Player는 아직 Prefab 인스턴스로 전환 전~~ → **완료**: Prefab 인스턴스화 (Connected) + Player 위치 (0, 0.5, 0)
- ~~groundLayer 미설정~~ → **완료**: Ground 레이어 index 3, groundLayer=8
- Unity MCP 대신 **unity-cli 스킬 경로**로 진행 (MCP 블로커 우회, 연결 상태 ready 확인됨)
- **eval_file 핵심 학습**: 5000ms 메인스레드 타임아웃은 CLI `--timeout`으로 조절 불가 → 장시간 작업은 `--detach` + `unity job wait`로 실행 (Play Mode 진입, AssetDatabase.Refresh, SaveScene 전부 해당)

---

## TL;DR (For humans)
Unity 6 + Cinemachine 기반 2D 플랫포머 게임의 기본 구색을 만듭니다.
- 플레이어 캐릭터 (이동, 점프)
- Cinemachine 카메라 시스템 (부드러운 따라가기, 구역별 전환 지원)
- 기본 레벨 구조 (바닥, 플랫폼)
- 2D 물리 설정

**이 프로젝트의 핵심**: 이전 프로젝트의 카메라 뚝뚝 끊김 문제를 해결하는 것.
Cinemachine을 올바르게 설정하면 이 문제는 해결됩니다.

**effort**: 약 4-6시간 (초기 구현)
**risk**: Cinemachine 설정 미숙으로 인한 카메라 문제 재발 가능

---

## Scope

### IN
- Player Controller (2D Rigidbody 기반)
- Cinemachine Virtual Camera + Confiner 2D
- 기본 Level Builder ( 타일 기반 또는 프리팹 기반)
- 2D Physics 설정 (Gravity, Layer)
- 기본 Input System (Keyboard)

### OUT
- 스토리 시스템
- 적 캐릭터
- UI 시스템
- 사운드
- 애니메이션 (기본 Idle/Walk 정도만)
- 저장 시스템

---

## Verification Strategy

### Agent-Executed QA
각 todo마다 Unity 플레이 모드에서 직접 테스트:
- Play 버튼 클릭 → 캐릭터 움직임 확인
- 카메라 따라가기 확인
- 레벨 구조 확인

### Test Strategy
- **TDD**: 단위 테스트 불필요 (Unity 직접 조작)
- **QA**: Unity Editor에서 Play Mode 테스트

---

## Execution Strategy

### Phase 1: 프로젝트 설정
1. Cinemachine 패키지 설치
2. 기본 Scene 구조 설정
3. 2D 물리 설정

### Phase 2: 플레이어 캐릭터
4. Player Prefab 생성
5. PlayerController 스크립트
6. 기본 이동/점프 구현

### Phase 3: 카메라 시스템
7. Cinemachine Virtual Camera 설정
8. Camera Confiner 2D 설정
9. 구역별 전환 테스트

### Phase 4: 레벨 구조
10. 기본 레벨 생성
11. 플랫폼/벽 배치
12. 최종 테스트

---

## Todos

### Wave 0: 블로커 해소 — 불필요 패키지 제거 + 컴파일 복구 (최우선)

#### Todo 0-1: 불필요한 무거운 패키지 제거 ✅
- [x] 1. **File/Directory**: `Packages/manifest.json`
- **References**: Unity Package Manager
- **Acceptance Criteria**:
  - `com.unity.ai.assistant` 제거
  - `com.unity.ai.inference` 제거
  - `com.unity.multiplayer.center` 제거
  - `com.unity.visualscripting` 제거
  - `com.unity.timeline` 제거 (플랫포머에 불필요)
  - `com.unity.collab-proxy` 제거 (Git 사용)
  - 제거 후 `manifest.json` 유효성 확인
- **QA**:
  - Happy: manifest.json에서 해당 패키지 라인 사라짐
  - Failure: JSON 문법 에러 → 이전 백업에서 복원
- **Commit**: "chore: remove unused heavy packages (AI, multiplayer, visual scripting)"

#### Todo 0-2: 컴파일 확인 + PlayerController 부착
- [ ] 2. **File/Directory**: `Assets/Scripts/PlayerController.cs`, Player GameObject
- **References**: Unity Editor compilation
- **Acceptance Criteria**:
  - Unity Editor가 자동 재컴파일 수행
  - `Library/ScriptAssemblies/Assembly-CSharp.dll` 생성 확인
  - Player 오브젝트에 `PlayerController` 스크립트 컴포넌트 부착됨
  - Inspector에서 moveSpeed, jumpForce 등 필드 표시
- **QA**:
  - Happy: Play Mode에서 콘솔 에러 없음, PlayerController가 Inspector에 보임
  - Failure: DLL 미생성 시 에디터 재시작 또는 메모리 확인
- **Commit**: "fix: attach PlayerController after successful compilation"

#### Todo 0-3: Player 설정 검증 (Rigidbody2D Interpolation)
- [ ] 3. **File/Directory**: Player GameObject Inspector
- **References**: Unity 2D Physics (카메라 끊김 방지)
- **Acceptance Criteria**:
  - Rigidbody2D:
    - Gravity Scale = 3
    - **Interpolation = Interpolate** ⭐ (카메라 끊김 방지 필수!)
    - Collision Detection = Continuous
  - BoxCollider2D: 적절한 크기
  - groundCheck Transform: Player 하위 오브젝트로 존재
- **QA**:
  - Happy: Inspector에서 모든 설정 확인
  - Failure: Interpolation이 None이면 Interpolate로 변경
- **Commit**: "config: verify player physics settings for smooth camera"

---

### Wave 1: 프로젝트 설정

#### Todo 1: Cinemachine 패키지 설치 ✅
- [x] 4. **File/Directory**: Unity Package Manager
- **References**: com.unity.cinemachine 3.1.7
- **Acceptance Criteria**: 
  - Cinemachine 패키지가 프로젝트에 설치됨
  - Cinemachine 메뉴가 Unity 에디터에 나타남
  - `Unity.Cinemachine` 네임스페이스 사용 가능
- **QA**:
  - Happy: Package Manager에서 Cinemachine 설치 확인 ✅ com.unity.cinemachine 3.1.7 in manifest
  - Failure: 패키지 설치 실패 시 수동 설치 안내
- **Commit**: "feat: add Cinemachine package"

#### Todo 2: 2D 물리 설정 (카메라 끊김 방지) ✅
- [x] 5. **File/Directory**: Project Settings > Physics 2D
- **References**: Unity 2D Physics
- **Acceptance Criteria**:
  - Gravity Y = -20 (기본값 -9.81보다 빠른 중력) ✅
  - Default Contact Offset = 0.01 ✅
  - Auto Sync Transforms = true ✅
  - **중요**: 나중에 Player Rigidbody2D에 Interpolation = Interpolate 설정
- **QA**:
  - Happy: Play Mode에서 물체가 빠르게 떨어짐
  - Failure: 물리 설정이 적용되지 않으면 Inspector에서 확인
- **Commit**: "config: setup 2D physics settings"

#### Todo 3: 기본 Scene 구조 ✅
- [x] 6. **File/Directory**: Assets/Scenes/Main.unity
- **References**: Unity Scene
- **Acceptance Criteria**:
  - 새 Scene 생성 ✅
  - Main Camera 유지 (CinemachineBrain 추가 예정) ✅
  - 기본 Directional Light 유지 ✅
- **QA**:
  - Happy: Scene이 깨끗하게 비어있음
  - Failure: 기존 오브젝트가 남아있으면 삭제
- **Commit**: "feat: create clean main scene"

### Wave 2: 플레이어 캐릭터

#### Todo 4: Player Prefab 생성 (카메라 끊김 방지 필수 설정)
- [ ] 7. **File/Directory**: Assets/Prefabs/Player.prefab
- **References**: Unity Prefab
- **Acceptance Criteria**:
  - 빈 GameObject 생성
  - SpriteRenderer 추가 (임의 색상 사각형)
  - Rigidbody2D 추가:
    - Gravity Scale = 3
    - **Interpolation = Interpolate** ⭐ (카메라 끊김 방지 필수!)
    - Collision Detection = Continuous
  - BoxCollider2D 추가
  - 태그를 "Player"로 설정
- **QA**:
  - Happy: Prefab이 Hierarchy에 나타남, Rigidbody2D Interpolation 확인
  - Failure: 컴포넌트 누락 시 Inspector에서 확인
- **Commit**: "feat: create player prefab"

#### Todo 5: PlayerController 스크립트 (Input 처리 주의)
- [ ] 8. **File/Directory**: Assets/Scripts/PlayerController.cs
- **References**: Rigidbody2D, Input System
- **Acceptance Criteria**:
  - moveSpeed = 8f
  - jumpForce = 14f
  - **Input은 Update()에서 읽기** ⭐ (카메라 끊김 방지!)
  - **Velocity는 FixedUpdate()에서 설정** ⭐
  - grounded 체크 (Physics2D.OverlapCircle)
  - 최대 점프 횟수 = 2 (대시 점프 가능)
- **QA**:
  - Happy: 화살표 키로 이동, 스페이스로 점프
  - Failure: 점프가 안 되면 grounded 체크 확인
- **Commit**: "feat: implement player controller"

#### Todo 6: Player Animator 설정 (기본)
- [ ] 9. **File/Directory**: Assets/Animations/PlayerController.controller
- **References**: Animator Controller
- **Acceptance Criteria**:
  - Idle 상태 (기본)
  - Run 상태 (이동 중)
  - Jump 상태 (점프 중)
  - Fall 상태 (낙하 중)
  - Parameter: Speed (float), IsGrounded (bool), VerticalVelocity (float)
- **QA**:
  - Happy: 이동 중 애니메이션 변경
  - Failure: 애니메이션이 안 바뀌면 Parameter 확인
- **Commit**: "feat: add basic player animations"

### Wave 3: 카메라 시스템 (핵심 - 끊김 문제 해결)

#### Todo 7: Main Camera + CinemachineBrain 설정
- [ ] 10. **File/Directory**: Main Camera GameObject
- **References**: CinemachineBrain (Unity.Cinemachine)
- **Acceptance Criteria**:
  - Main Camera에 CinemachineBrain 컴포넌트 추가
  - **Update Method = LateUpdate** ⭐ (카메라 끊김 방지 필수!)
  - **Blend Update Method = LateUpdate** ⭐
  - Default Blend = Ease In Out (1초)
  - Projection = Orthographic
  - Orthographic Size = 5.4 (기본값)
- **QA**:
  - Happy: CinemachineBrain이 LateUpdate로 설정됨
  - Failure: SmartUpdate로 되어 있으면 변경
- **Commit**: "feat: setup CinemachineBrain"

#### Todo 8: CinemachineCamera 설정 (플레이어 따라가기)
- **File/Directory**: Assets/Prefabs/MainCamera.prefab
- **References**: CinemachineCamera (Unity.Cinemachine)
- **Acceptance Criteria**:
  - 새 GameObject "CM vcam1" 생성
  - CinemachineCamera 컴포넌트 추가
  - **Tracking Target = Player** 
  - **Position Control = Position Composer** (2D용)
  - **Damping X = 0.3, Y = 1.5** ⭐ (점프 시 바운스 방지)
  - Dead Zone X = 0.1, Y = 0.15
  - Lookahead Time = 0.3, Smoothing = 높은 값
  - Lens > Orthographic Size = 5.4
- **QA**:
  - Happy: Play Mode에서 카메라가 플레이어를 부드럽게 따라감
  - Failure: 카메라가 안 움직이면 Tracking Target 확인
- **Commit**: "feat: setup CinemachineCamera"

#### Todo 9: Camera Confiner 2D 설정 (레벨 경계)
- **File/Directory**: Assets/Prefabs/CameraConfiner.prefab
- **References**: Cinemachine Confiner 2D
- **Acceptance Criteria**:
  - LevelBounds GameObject 생성
  - Polygon Collider 2D 추가 (Is Trigger = true)
  - 레벨 경계에 맞게 Collider 조정
  - CinemachineCamera에 Confiner 2D Extension 추가
  - Bounding Shape 2D = LevelBounds
  - Damping = 1.0
  - Max Window Size = 5.4 (카메라 Orthographic Size)
- **QA**:
  - Happy: 카메라가 레벨 밖으로 나가지 않음
  - Failure: Confiner가 안 먹히면 Layer 확인
- **Commit**: "feat: add camera confiner 2D"

#### Todo 10: 카메라 구역 전환 시스템 (여러 카메라 사용)
- **File/Directory**: Assets/Scripts/CameraZone.cs
- **References**: CinemachineCamera, CinemachineBrain
- **Acceptance Criteria**:
  - CameraZone 스크립트 생성
  - **구역별 CinemachineCamera 생성** (단일 카메라 변경 X)
  - Trigger Enter/Exit에서 카메라 활성화/비활성화
  - Priority로 전환 (활성 카메라 = Priority 10, 비활성 = 0)
  - 구역별 다른 Confiner 설정 가능
- **QA**:
  - Happy: 구역 이동 시 부드러운 카메라 전환
  - Failure: 전환이 안 되면 Trigger Collider 확인
- **Commit**: "feat: implement camera zone transitions"

### Wave 4: 레벨 구조

#### Todo 11: 기본 레벨 생성
- **File/Directory**: Assets/Prefabs/Level/
- **References**: Unity Tilemap 또는 Prefab
- **Acceptance Criteria**:
  - 바닥 타일 생성 (가로 20타일)
  - 기본 플랫폼 3-4개 배치 (높이 다양하게)
  - 벽 생성 (좌우)
  - 각 타일에 Collider2D + SpriteRenderer 추가
- **QA**:
  - Happy: 플레이어가 바닥과 플랫폼 위를 걸어다님
  - Failure: 충돌이 안 되면 Collider 확인
- **Commit**: "feat: create basic level layout"

#### Todo 12: 레벨 장식 (기본)
- **File/Directory**: Assets/Sprites/Environment/
- **References**: Unity Sprite
- **Acceptance Criteria**:
  - 기본 배경 색상 설정 (Camera > Background)
  - 타일에 기본 스프라이트 적용
  - 장식용 오브젝트 몇 개 배치
- **QA**:
  - Happy: 레벨이 시각적으로 구분됨
  - Failure: 스프라이트가 안 보이면 Sorting Layer 확인
- **Commit**: "feat: add basic level decoration"

#### Todo 13: 최종 통합 테스트 (카메라 끊김 확인)
- **File/Directory**: 전체 프로젝트
- **References**: Unity Play Mode
- **Acceptance Criteria**:
  - Play Mode에서 플레이어 이동/점프 정상 작동
  - **카메라가 부드럽게 따라감** ⭐ (끊김 없음!)
  - **점프 시 카메라 바운스 없음** ⭐
  - 레벨 구조가 보임
  - 프레임 안정적 (60FPS 이상)
- **QA**:
  - Happy: 게임 플레이 가능, 카메라 부드러움
  - Failure: 끊김 발생 시 Inspector에서 설정 확인
- **Commit**: "test: verify basic platformer functionality"

---

## Final Verification Wave

### F1: Plan Compliance Audit
- 모든 todo가 구현되었는지 확인
- 각 todo의 Acceptance Criteria 충족 여부 검증
- 파일 경로 및 참조 정확성 확인

### F2: Code Quality Review
- C# 스크립트 문법 검증
- Unity 컴포넌트 설정 적절성 검토
- 성능 이슈 점검

### F3: Real Manual QA
- Unity Editor에서 Play Mode 테스트
- 플레이어 이동/점프 감지
- 카메라 따라가기 부드러움 확인
- 레벨 충돌 검증

### F4: Scope Fidelity
- IN/OUT 범위 준수 여부 확인
- 불필요한 기능 추가 여부 점검
- 핵심 카메라 시스템 구현 여부 확인

---

## Commit Strategy

### Phase별 커밋
1. 프로젝트 설정: "feat: setup 2D platformer project"
2. 플레이어 캐릭터: "feat: implement player controller"
3. 카메라 시스템: "feat: setup Cinemachine camera system"
4. 레벨 구조: "feat: create basic level layout"
5. 최종 테스트: "test: verify basic platformer"

### 커밋 메시지 규칙
- feat: 기능 추가
- fix: 버그 수정
- config: 설정 변경
- test: 테스트 관련

---

## Success Criteria

1. **기본 기능**: 플레이어가 이동/점프 가능
2. **카메라**: Cinemachine이 플레이어를 부드럽게 따라옴
3. **구역 전환**: 카메라 경계가 구역별로 변경 가능
4. **레벨**: 기본적인 바닥/플랫폼 구조 존재
5. **성능**: 60FPS 이상 유지

---

## Dependencies

### Package Dependencies
- com.unity.cinemachine 3.1.7
- com.unity.inputsystem (Unity 6에 포함)

### File Dependencies
- Assets/Scripts/PlayerController.cs
- Assets/Prefabs/Player.prefab
- Assets/Prefabs/MainCamera.prefab
- Assets/Scenes/Main.unity

### External Dependencies
- Unity 6000.3.12f1
- .NET Standard 2.0