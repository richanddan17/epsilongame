# AGENTS.md — EPSILON GAME 협업 규약 (Collaboration Contract)

> 이 파일은 **EPSILON GAME(epsilongame)** 개발에 참여하는 모든 AI 에이전트(Google Antigravity, opencode 등)와 인간 팀원이 함께 지키는 규칙입니다.
> AI 에이전트는 이 파일의 지침을 시스템 프롬프트처럼 준수하세요. 인간 팀원은 에이전트가 이 규칙을 어기면 즉시 지적하세요.

## 1. 프로젝트 개요
| 항목 | 내용 |
|---|---|
| 프로젝트명 | EPSILON GAME (`epsilongame`) |
| 엔진 | Unity 6000.3.12f1 / URP 17.3.0 |
| 주요 패키지 | Cinemachine 3.1.7, Input System 1.19.0 |
| 장르 | 2D 플랫포머 (이동 / 점프 / 이단점프) |
| 빌드 타깃 | **Windows 독립 실행** |
| 개발 기간 | 장기 개발 (~3개월, 2026-09 ~ 2026-12) |
| 핵심 요구사항 | **카메라 끊김 없음** > 핵심 게임플레이 > 폴리싱 |

관련 문서: `.omo/plans/platformer-basic.md`, `agy/plans.md`

## 2. 협업 기본 원칙
1. **AI는 제안하고, 인간이 결정한다.** 파괴적/광범위 변경(패키지 설치·제거, Library/ProjectSettings 대량 수정, 파일 대량 삭제·이동)은 인간 승인 후 실행.
2. **한 번에 하나의 작업.** 계획→구현→검증→보고 순서로 완결.
3. **증거를 남겨라.** 보고에 스크린샷/콘솔 로그/파일 diff/실행 결과 포함.
4. **변경은 최소로.** 동작 중인 코드 무단 재작성 금지, 이유 보고.
5. **검증 없이 "완료" 금지.** 컴파일/실행/테스트로 확인한 것만 완료 보고, 실패 사례도 보고.

## 3. 역할 분담
### 인간 팀 (변경 없음)
| 역할 | 책임 |
|---|---|
| 팀 리드 (인간) | 방향·승인·최종 결정, 발표 (단일 결정권) |
| 개발 담당 | 에셋·씬 구성, 게임플레이 검토 (조정 가능) |
| 발표 담당 | 발표자료·데모 준비 (조정 가능) |
| 아트 담당 | 스프라이트·사운드 선별 (조정 가능) |

### 작업 파이프라인 (변경 없음)
`Planner(계획) → Coder(구현) → Tester(검증) → Documenter(문서)` — 순서 고정.
Tester 실패 시 Coder 수정 → Tester 재검증 (재시도 2회 후 인간 보고).

### AI 에이전트 배정 (툴 → 역할 매핑)
| 에이전트 | 파이프라인 역할 | 담당 범위 | 작업 위치 |
|---|---|---|---|
| opencode | Planner + Documenter | 작업 분해·계획서 작성, 승인 게이트 운영, 문서·커밋 정리 | `ppt/` (플래닝 워크스페이스) |
| Antigravity | Coder + Tester | EPSILON GAME 구현·검증 (2D 플랫포머, Unity) | `epsilongame/` |
| 발표자료 게임 | 미배정 | WebGL 발표 게임 (별도 프로젝트) | 확정 후 배정 |

- 각 에이전트는 배정된 파이프라인 역할 외 단계를 수행하지 않는다 (범위 침범 금지).
- 위 배정은 AGENTS.md §2(기본 원칙)·§7(금지 사항)을 대체하지 않는다.

## 4. 작업 흐름
`요청 접수 → PLAN(인간 승인) → IMPLEMENT → VERIFY → REPORT → COMMIT(인간 확인)`

## 5. 코드 컨벤션
- 네임스페이스 `EpsilonGame`, `[SerializeField] private` 필드 + `[Header]`
- 입력 `Update()`, 물리 `FixedUpdate()`, **Rigidbody2D Interpolation = Interpolate 필수**
- 파일 위치: Scripts/Scenes/Prefabs 폴더 규칙, 물리 설정 기준 (Gravity Y=-20 등)

## 6. 커밋 규칙
- `타입: 요약` (feat/fix/config/test/docs/chore), 작게·자주
- 커밋 금지: `.omo/boulder.json`, `.omo/run-continuation/`, `.omo/notepads/`, `error_log.txt`, `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`

## 7. 금지 사항 (Must NOT)
1. 승인 없는 패키지 추가/제거
2. 승인 없는 파일 대량 삭제/이동
3. 범위 외 기능 추가 (스토리/적/사운드 등)
4. 인증 정보 취급
5. AGENTS.md·.omo 문서 무단 수정 (제안은 승인 후)
6. 승인 없는 빌드/배포 명령 (WebGL 빌드, Player 설정 변경)

## 8. 커뮤니케이션 규칙
- 요청·보고 한국어, 코드 식별자 영어
- `WORKING: <작업명> - <단계>` / `BLOCKED: <사유>`
- 완료 보고: 한 일 / 검증 방법 / 증거 / 남은 리스크

## 9. 현재 진행 상황 (2026-09-12)
- [x] Wave 0~1 (패키지 정리, Cinemachine, 2D 물리, 씬)
- [ ] Wave 2~4 (Player Prefab+Controller → 카메라 → 레벨)
- 상세: `.omo/plans/platformer-basic.md`

*이 규약은 인간 팀의 승인으로만 변경됩니다.*
