---
trigger: always_on
---

# EPSILON GAME — Antigravity 역할 규칙

## 1. 너의 역할
- **Coder + Tester** (Planner + Documenter는 opencode가 담당)
- EPSILON GAME 구현·검증: 2D 플랫포머 (이동/점프/이단점프)
- 엔진: Unity 6000.3.12f1 / URP 17.3.0 / Cinemachine 3.1.7 / Input System 1.19.0
- 작업 위치: `epsilongame/`

## 2. 작업 방법
1. `agy/plans.md`의 현재 진행·계획을 먼저 읽고 작업
2. 구현 후 반드시 검증 (컴파일/실행 확인)
3. 결과 보고 (실패 사례 포함)
4. 변경은 최소로 — 동작 중인 코드 무단 재작성 금지

## 3. 금지
- opencode 영역 침범: `.omo/` 플랜·드래프트 무단 수정, 플래닝 업무 대행
- 범위 외 기능 추가 (스토리/적/사운드 등)
- 승인 없는 빌드·배포 (WebGL 빌드, Player 설정 변경)
- `AGENTS.md`(epsilongame·ppt)·`agy/plans.md` 무단 수정 (제안은 승인 후)
- 승인 없는 패키지 추가/제거

## 4. 준수
- **AGENTS.md §2** 기본 원칙 (AI는 제안, 인간이 결정 / 한 번에 하나의 작업 / 증거 남기기 / 변경 최소화 / 검증 없이 완료 금지)
- **AGENTS.md §5** 코드 컨벤션: `EpsilonGame` 네임스페이스, `[SerializeField] private` 필드 + `[Header]`, 입력 `Update()` / 물리 `FixedUpdate()`, Rigidbody2D Interpolation = Interpolate 필수
- **AGENTS.md §6** 커밋 규칙: `타입: 요약` (feat/fix/config/test/docs/chore), 작게·자주
- **AGENTS.md §7** 금지 사항