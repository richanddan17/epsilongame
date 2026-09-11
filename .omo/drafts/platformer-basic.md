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
executing - Wave 0 완료 (컴파일 성공) → Wave 2~4 진행 대기 중 (Unity MCP 연결 블로커 해결 중)

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