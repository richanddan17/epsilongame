# platformer-basic - Work Plan

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