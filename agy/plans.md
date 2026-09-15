# agy/plans.md — Antigravity 진행·보고 문서

> 이 문서는 **Antigravity** 전용 진행 추적 및 보고 문서입니다.
> 구현 상세 계획은 `.omo/plans/` 에 위치하며, 이 파일은 보고·진행 상태만 기록합니다.

---

## 1. 역할 요약

| 에이전트 | 파이프라인 역할 | 담당 범위 | 작업 위치 |
|---|---|---|---|
| **Antigravity** | **Coder + Tester** | EPSILON GAME 구현·검증 (2D 플랫포머 이동/점프/이단점프, Unity 6000.3.12f1 / URP 17.3.0 / Cinemachine 3.1.7 / Input System 1.19.0) | `epsilongame/` |
| opencode | Planner + Documenter | 작업 분해·계획서 작성, 승인 게이트 운영, 문서·커밋 정리 | `ppt/` (플래닝 워크스페이스) |
| 발표자료 게임 | 미배정 | WebGL 발표 게임 (별도 프로젝트) | 확정 후 배정 |

---

## 2. 현재 진행 상황 (2026-09-12 기준)

- [x] **Wave 0~1** — 패키지 정리, Cinemachine, 2D 물리, 씬
- [ ] **Wave 2~4** — Player Prefab+Controller → 카메라 → 레벨

> 상세 계획: `.omo/plans/platformer-basic.md`

---

## 3. 보고 규칙

- **AGENTS.md §3 배정 준수**: 배정된 파이프라인 역할(§1 참조) 외 단계를 수행하지 않는다. 범위 침범 금지.
- **AGENTS.md §6 커밋 규칙**: `타입: 요약` (feat/fix/config/test/docs/chore), 작게·자주. 커밋 금지 대상(`.omo/boulder.json`, `.omo/run-continuation/`, `.omo/notepads/`, `error_log.txt`, `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`) 절대 커밋 금지.

---

## 4. 작업 규칙

AGENTS.md §2(기본 원칙)·§5(코드 컨벤션)·§7(금지 사항) 필수 준수:

- **§2 기본 원칙**: AI는 제안하고 인간이 결정; 한 번에 하나의 작업; 증거 남기기; 변경 최소화; 검증 없이 완료 금지.
- **§5 코드 컨벤션**: 네임스페이스 `EpsilonGame`, `[SerializeField] private` 필드 + `[Header]`, 입력 `Update()`, 물리 `FixedUpdate()`, **Rigidbody2D Interpolation = Interpolate 필수**.
- **§7 금지 사항**: 승인 없는 패키지 추가/제거, 파일 대량 삭제/이동, 범위 외 기능 추가, 인증 정보 취급, AGENTS.md·.omo 문서 무단 수정, 승인 없는 빌드/배포 명령 금지.
