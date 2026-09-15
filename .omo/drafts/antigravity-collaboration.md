# antigravity-collaboration - Draft

## Intent
CLEAR - 사용자가 협업 규약 + 계획서 작성을 명확히 요청함

## Review Required
false

## Components
1. **협업 규약 (AGENTS.md)** - EPSILON GAME × AI(Antigravity) 협업 규칙/역할 분담 문서 (인간+에이전트 공용)
2. **EPSILON GAME (장기)** - Unity 2D 플랫포머, ~3개월, Windows 빌드 (platformer-basic.md 계속)
3. **발표자료 게임 (24h)** - Unity WebGL 빌드, 다른 에셋, 웹 삽입용 플랫포머 (별도 프로젝트)
4. **웹 게임 PPT** - 게임월드 형식 발표 (비전 B 중심, "게임이 왜 재미있을까")

## Decisions Made
- 발표자료 게임 기술: **Unity WebGL 빌드** (사용자 선택 확정)
- EPSILON GAME 빌드: **Windows** (사용자 선택 확정), 장기 ~3개월
- 발표 주제: "게임이 왜 재미있을까" (학교/학원 발표)
- 발표 형식: 비전 B(게임 씬=PPT, 캐릭터가 오른쪽 진행 + 맵 기반 카메라) 중심, 비전 A(일반 슬라이드+게임 삽입) 폴백
- 협업 규약 형식: AGENTS.md (Antigravity/opencode가 자동 인식하는 크로스-툴 표준)
- 발표자료 게임은 별도 위치(이 저장소 밖) — "여기말고"

## Pending Questions (설계 논의 중)
- Q1: 발표 중 캐릭터 조종 주체 (발표자 / 관객 / 자동)
- Q2: 사용자가 찾은 에셋 종류 (Asset Store 팩 이름/종류)
- Q3: 존(슬라이드) 개수·발표 시간 (권장 5~7존)

## Status
discussion - 설계 논의 진행 중 (협업 규약 초안은 아래, 사용자 검토 대기)

---

## 협업 규약 초안 (AGENTS.md로 설치될 내용 — 2026-09-12)

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

관련 문서: `.omo/plans/platformer-basic.md`

## 2. 협업 기본 원칙
1. **AI는 제안하고, 인간이 결정한다.** 파괴적/광범위 변경(패키지 설치·제거, Library/ProjectSettings 대량 수정, 파일 대량 삭제·이동)은 인간 승인 후 실행.
2. **한 번에 하나의 작업.** 계획→구현→검증→보고 순서로 완결.
3. **증거를 남겨라.** 보고에 스크린샷/콘솔 로그/파일 diff/실행 결과 포함.
4. **변경은 최소로.** 동작 중인 코드 무단 재작성 금지, 이유 보고.
5. **검증 없이 "완료" 금지.** 컴파일/실행/테스트로 확인한 것만 완료 보고, 실패 사례도 보고.

## 3. 역할 분담
### 인간 팀
| 역할 | 책임 |
|---|---|
| 팀 리드 (인간) | 방향·승인·최종 결정, 발표 (단일 결정권) |
| 개발 담당 | 에셋·씬 구성, 게임플레이 검토 (조정 가능) |
| 발표 담당 | 발표자료·데모 준비 (조정 가능) |
| 아트 담당 | 스프라이트·사운드 선별 (조정 가능) |

### AI 에이전트 팀 (고정 순서: Planner → Coder → Tester → Documenter)
| 역할 | 임무 | 산출물 |
|---|---|---|
| Planner | 작업 분해, 리스크·영향 범위 명시 | 작업 계획서 |
| Coder | 계획에 따라 구현, 컨벤션 준수 | 코드 diff |
| Tester | 컴파일/실행 QA, 실패 재현·원인 보고 | 테스트 결과 + 스크린샷/로그 |
| Documenter | 문서 갱신, 커밋 메시지 정리 | 갱신된 문서 |

- Tester 실패 시 Coder 수정 → Tester 재검증 (재시도 2회 후 인간 보고)

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

---

## 다음 단계
1. 사용자가 협업 규약 초안 검토/수정 지시
2. 설계 질문 3건 답변 (조종 주체 / 에셋 / 존 구성)
3. 승인 게이트 → scaffold-plan → Metis → 계획서 작성 (.omo/plans/antigravity-collaboration.md)
4. 계획서 Todo 0-x: AGENTS.md 설치 (실행자가 수행)