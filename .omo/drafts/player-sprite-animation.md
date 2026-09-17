---
slug: player-sprite-animation
status: drafting
intent: clear
pending-action: write .omo/plans/player-sprite-animation.md
approach: 에디터 라이브(Unity CLI)로 Player 프리팹에 스프라이트 3종과 이동 애니메이션(Idle/Walk/Jump) 적용
---

# Draft: player-sprite-animation

## Components (topology ledger)
| id | outcome (one line) | status | evidence path |
|---|---|---|---|
| C1 | 스프라이트 임포트: PPU 100 통일 + jump pivot 0.5,0.5 | active | Assets/sprite/player/*.png.meta |
| C2 | AnimationClip 3개 (PlayerIdle/Walk/Jump) | active | Assets/Animations/Player*.anim |
| C3 | PlayerAnimator.controller (Speed float, IsGrounded bool) | active | Assets/Animations/PlayerAnimator.controller |
| C4 | PlayerController.cs 애니메이션 파라미터 연동 | active | Assets/Scripts/PlayerController.cs |
| C5 | Player.prefab: SpriteRenderer.sprite + Animator 부착 | active | Assets/Prefabs/Player.prefab |
| C6 | Play Mode QA (전환/에러/정렬) | active | .omo/evidence/task-6-player-sprite-animation.* |

## Open assumptions (announced defaults)
| assumption | adopted default | rationale | reversible? |
|---|---|---|---|
| 애니메이션 fps | 12fps | 픽셀아트 표준; idle 4프레임=0.33s, walk 8프레임=0.67s | 예 (클립 재생속도로 조절) |
| 애니메이션 파라미터 | Speed(float) + IsGrounded(bool) | PlayerController의 MoveInput/IsGrounded와 1:1 매핑 | 예 |
| 좌우 반전 | localScale.x 부호 반전 (기존 유지) | PlayerCombat이 localScale.x로 공격 방향 판정 | 예 |
| 공격/패링 애니메이션 | 미포함 (스프라이트에 공격 컷 없음) | 범위 외 — 이동 애니메이션만, 전투 클립은 사용자 에셋 대기 | 해당사항 없음 |

## Findings (cited - path:lines)
- Player.prefab: SpriteRenderer.m_Sprite = {fileID: 0} 미할당, Animator 컴포넌트 없음 (Assets/Prefabs/Player.prefab:121, 41-55)
- 스프라이트 에셋 슬라이스 완료: idle_0~3 (Assets/sprite/player/idle.png.meta:108-216), walk_0~7 (revision-walking.png.meta:337-344), jump_0~3 (jump.png.meta:110-216)
- PPU 149.25 → 100px 스프라이트가 0.67 유닛 (idle.png.meta:63) — 콜라이더 1×1과 불일치
- jump.png는 pivot (0,0) + 비균일 크기(58~85px) (jump.png.meta:120, 114-198) — idle/walk는 pivot (0.5,0.5)
- PlayerController.cs: 애니메이션 코드 없음, IsGrounded/MoveInput 프로퍼티 존재 (Assets/Scripts/PlayerController.cs:60-61)
- PlayerCombat이 transform.localScale.x 부호로 공격 방향 판정 (Assets/Scripts/PlayerCombat.cs:101,179)
- EnemyAnimator.controller YAML 패턴 (bool 파라미터 + 전이만, 클립 미할당) — Assets/Animations/EnemyAnimator.controller:148-172
- EnemyPrefabBuilder.cs 배치/에디터 스크립트로 프리팹+컨트롤러 생성한 선례 (Assets/Editor/EnemyPrefabBuilder.cs)

## Decisions (with rationale)
- **D1: 에디터 라이브 진행** (사용자 선택) — 에디터를 열고 unity CLI로 실시간 생성/수정 + Play Mode QA. YAML 직접 편집 금지 (unity-cli 스킬 원칙).
- **D2: PPU 100 통일** (사용자 선택) — 100px 스프라이트 = 1 유닛, 기존 BoxCollider2D 1×1과 일치. jump.png pivot도 (0.5,0.5)로 통일 (현재 (0,0) → 점프 애니메이션 시 캐릭터 어긋남).
- **D3: 에디터 스크립트(Assets/Editor/PlayerAnimationBuilder.cs)로 클립+컨트롤러 생성** — AnimationUtility.SetObjectReferenceCurve(Editor API)로 스프라이트 키프레임 곡선 생성. EnemyPrefabBuilder.cs 선례와 동일 방식. YAML 수작업보다 안전.
- **D4: PlayerController.cs 수정 범위 최소화** — Animator 참조 + 파라미터 설정 + localScale 반전만 추가 (물리/이동 로직 불변).

## Scope IN
- 스프라이트 임포트 설정 변경 (PPU 100, jump pivot)
- AnimationClip 3개 + PlayerAnimator.controller 생성
- PlayerController.cs Animator 연동 (파라미터 + 좌우 반전)
- Player.prefab SpriteRenderer.sprite 할당 + Animator 부착
- Play Mode QA + 커밋

## Scope OUT (Must NOT have)
- 공격/패링 애니메이션 (전투 스프라이트 없음 — 에셋 대기)
- enemy 프리팹/컨트롤러 수정
- PixelPerfectCamera/2d-pixel-perfect 전면 세팅 (관련은 아니지만 PPU 변경만 — 카메라 세팅은 범위 외)
- Input System 마이그레이션, VfxSlot 적용 (Wave 4 별도)
- PlayerController 이동/점프 파라미터 변경
- 씬(Main.unity) 수정

## Open questions
- 없음 (승인 게이트에서 확정)

## Approval gate
status: awaiting-approval
<!-- 사용자 확인: 에디터 라이브 + PPU 100 채택 (2026-09-17 질문 응답) -->