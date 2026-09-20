# EPSILON GAME — 플레이어 모션·이펙트·배경/타일 작업 플랜 (player-motion-effects)

> 작성: 2026-09-20 | 기준 문서: `Assets/sprite/player/moving&effect_guide .txt` (최우선), `combo_attack2_guide.txt`, `movement_parry.txt`, `frame_map_parry.txt`, `README Animator.md`, `README_4x.txt`
> 워크플로: 요청 접수 → PLAN(인간 승인) → IMPLEMENT → VERIFY → REPORT → COMMIT

## 1. 배경 (Current State)

- 플레이어 프레임 에셋: **4x 버전** (idle 184×220, walk/dash/jump 등 전부 4x). `README_4x.txt`가 **PPU 400**을 요구하나 빌더 `PlayerAnimationBuilder.cs`는 `Ppu = 100f` 하드코딩 → 화면 크기 4배 거대해짐.
- **프레임 222장 전부 `.meta` 없음** → Unity 미임포트. 이전 세션에서 확인한 최초 임포트 상태와 달리 매타가 전부 없는 상태(재임포트 필요). 임포트 시 fileID가 새로 발급되어 기존 클립/프리팹 참조가 깨질 수 있음 → **클립 재빌드 필수**.
- 그랩 이전 세션 블로커: 런타임 `sr.sprite = NULL` (애니메이터는 Idle 재생) — fileID dangling 추정. 클립 재빌드로 해소.
- **combo_attack 통합 완료**: 구 `combo_attack2` 13장 + 구 `combo_attack_34~58` 25장 → `combo_attack_00~37` (총 38장, 딜레이 접미사 유지). combo_attack2 폴더는 폴더로 존재하지 않고 프레임 00~12로 통합됨.
- 빌더 현재 상태: FrameFolders 11개에 parrying / parrying_fx / combo_attack2_fx / combo_attack2_line 없음. `ExpectedFrameCounts["combo_attack"] = 59` (38로 수정 필요). 컨트롤러 상태 10개에 Parry/Hurt 스테이트 없음. VFX 자식: DoubleJumpFX/DashFX만.

## 2. 목표 (Objectives)

1. **플레이어 표시 복구** — PPU 400 임포트 + 클립/프리팹 재빌드 → Play 모드에서 스프라이트 정상 표시.
2. **신규 모션 5종 클립/스테이트 추가** — parrying(13), combo_attack2(통합 38), parrying_fx(5), combo_attack2_fx(7), combo_attack2_line(4).
3. **이동 구현** (`moving&effect_guide` 수치 그대로): parrying 후퇴 +1.47(0.39s), combo_attack2 돌진 -1.58(0.24s), dash -3.23/높이 +0.75, jump +0.8, doublejump +0.64, fall 3분할.
4. **combo_attack2 그랩 연출**: 패링 성공 후 0.3초 내 좌클릭 → GrabHitbox → GrabPoint 고정 → 연출 중 무적 → 종료 시 데미지+넉백.
5. **이펙트**: shadow 상시 자식, dash_fx/doublejump_fx/parrying_fx/combo_attack2_fx/combo_attack2_line 생성(위치·타이밍 guide 준수).
6. **배경·타일** (전 세션 잔여): ParallaxBackground 무한 루프, Main.unity BG-1~4 배치, forest 타일셋 슬라이싱+팔레트, 임시 타일맵 교체.

## 3. 작업 내역 (Tasks, 순서 고정)

| # | 작업 | 상세 | 예상시간 |
|---|---|---|---|
| 1 | ✅ combo_attack 통합 | combo_attack2+후반 → `combo_attack_00~37` 38장, 딜레이 유지 | 완료 |
| 2 | 빌더 상수/폴더 갱신 | `Ppu` 100→**400**, FrameFolders에 parrying·parrying_fx·combo_attack2_fx·combo_attack2_line 추가, ExpectedFrameCounts 갱신(combo_attack 38, parrying 13, fx 5/7/4) | 15분 |
| 3 | 클립 빌드 | PlayerParry(13)/PlayerComboAttack(38 재빌드)/PlayerParryFX(5)/PlayerComboAttack2FX(7)/PlayerComboAttack2Line(4) | 20분 |
| 4 | 컨트롤러 확장 | 파라미터 `IsParrying`/`IsHurt` 추가, Parry/Hurt 스테이트, AnyState 우선순위: hurt > parry > combo_attack2 > attack/dash | 20분 |
| 5 | Unity 임포트+Build All | 222장 .meta 재생성 확인 → `Tools/Player/v2/Build All` 실행 → sr.sprite NULL 해소 검증 | 30분 |
| 6 | 이동 구현 | 파링 +1.47(0.39s), 콤보2 -1.58(0.24s), 대시 -3.23/높이+0.75, 점프 +0.8/-0.95, 이단 +0.64 | 60분 |
| 7 | 그랩 연출 | 0.3s 선입력 좌클릭, GrabHitbox, 무적, 종료 데미지+넉백 | 45분 |
| 8 | 이펙트 | shadow 자식(-0.25/sorting 하위), dash_fx(0.88,0.09), doublejump_fx(0.49,-0.65), parrying_fx, combo2_fx, line | 40분 |
| 9 | ParallaxBackground | 무한 루프(양옆 타일) + sortingOrder 1=뒤 4=앞 | 30분 |
| 10 | Main.unity 배경 | BG-1~4 생성/배치, 임시 배경 제거 | 30분 |
| 11 | 타일 | Fantasy-TileSet.png 슬라이싱 + Rectangular 팔레트(셀 1,1,0) | 30분 |
| 12 | 타일맵 교체 | 임시 타일맵/플랫폼 → 팔레트 페인팅 | 30분 |

**총 예상: 약 6시간** (플레이어 3시간 50분 / 배경·타일 2시간)

## 4. 검증 (Verification)

- 컴파일 0 에러 (`lsp_diagnostics`/Unity Console)
- Play 모드: Player 스프라이트 표시 (sr.sprite non-null), Idle/Walk/Jump/DoubleJump/Dash 전이 재생
- parrying 후퇴·콤보2 돌진 거리 Play 테스트 (guide 수치 ±0.05 유닛)
- 그랩: 피격 적 GrabPoint 고정 → 연출 종료 데미지
- 배경: 카메라 이동 시 BG-1~4 이음새 없는 무한 루프
- 콘솔 경고/에러 0건
- 스크린샷/로그 evidence 폴더 저장

## 5. 제약 (Constraints)

- AGENTS.md 준수: 승인 없는 패키지 설치·파일 대량 삭제 금지. Unity 임포트 화면 크기 보존을 위한 PPU 변경은 가이드(README_4x)에 근거.
- FX 오브젝트 제거 금지(사용자 의도된 설계).
- combo_attack은 38장 연속 시퀀스로 재빌드 (딜레이 파일명 그대로).
- 코드 식별자 영어, 네임스페이스 `EpsilonGame`, `[SerializeField] private` + `[Header]`, 입력 `Update()` / 물리 `FixedUpdate()`, **Rigidbody2D Interpolation = Interpolate** 유지.