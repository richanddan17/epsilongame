# combat-platformer - Draft

## Intent
CLEAR - 사용자가 plan.txt 설계서 + 범위 확정(인터뷰) 제공. 미기재 사항은 기본값 채택하라는 지시.

## Review Required
false - 고정밀 리뷰 요청 없음

## Components (Topology)
1. **Player Combat (공격 + 패링)** - 칼 공격, 일반/완벽 패링, 패링 윈도우 변수화
2. **Enemy (기본 1종)** - 감지/공격준비/공격/패링당함/패널티/피해/넉백/사망 상태 머신
3. **전투 연출** - 카메라 확대 + 슬로우 모션 + VFX 슬롯(에셋 대기)
4. **게임 흐름** - 체크포인트, 게임오버/클리어, 체력/리스폰
5. **스테이지 (개방형)** - 바닥+플랫폼+적 배치, 이동↔전투 반복
6. **씬 정리** - 장식(구름/언덕) 제거, 벽 제거, CameraZone 제거

## Decisions Made (인간 확정)
- 보스전 제외 (방/칸 구조는 나중에 보스전에서 채택) → 클리어 조건 = 스테이지 끝 도달 (보스 슬롯만 구조적으로 예약)
- 구름/언덕 장식 제거
- 기존 vfx 폴더(Assets/sprite/vfx/) 사용 금지 — 사용자가 새 에셋 제공 예정
- VFX 슬래시 이펙트: 코드/프리팹 틀만, 에셋 슬롯만 남김
- "에셋 없는 건 틀만, 내가 넣어주면 바로 적용 가능하게" — 스켈레톤 구조
- 카메라 끊김 방지는 기본 전제 (기존 Cinemachine 설정 유지)
- 개방형: 좌우 벽 제거, LevelBounds가 카메라 경계만 담당
- 기존 CameraZone.cs(방 전환) 제거 — 전투 확대 연출로 대체
- 플레이어 스프라이트: Assets/sprite/player/ (idle/jump/walking) 존재 → 임시 활용, 교체 가능 슬롯

## Adopted Defaults (사용자 미기재 → 기본값)
- 플레이어 HP: 5
- 적 HP: 3
- 패링 윈도우: 공격 직전 0.2s + 직후 0.08s (조정 변수)
- 완벽 패링: 선행 구간 50% 내 추가 타이밍 (0.1s), 적 패널티 = 무방비 시간 2배 (확장 가능 구조)
- 슬로우 모션: 0.3x, 공격 준비→판정 사이
- 카메라 확대: 1.2x (ortho 5.4→4.5), 0.3s 트랜지션
- 플레이어 공격: 데미지 1, 범위 1.5, 쿨다운 0.4s, 패링 후 공격 가능 시간 1.5s
- 적 공격: 데미지 1, 쿨다운 2s, 넉백 3
- 체크포인트: 2개 배치
- Input: 기존 legacy Input 유지 (Input.GetButtonDown) — 새 Input System 마이그레이션은 범위外
- 적 감지 반경: 5, 공격 도달 거리: 2

## Status
**2026-09-18: 사용자 승인 완료 ✅** — 계획서(.omo/plans/combat-platformer.md, 27 todo) 승인됨.

확정 사항 (사용자 직접 결정):
1. **플레이어 4종(폴더 5개) 전부 사용** — "아니 player안에있는 폴더ㅓ 싹다", 이후 "이제 플레이어도 4종됬을거야 나중에 메인 플레이어는 보고정하게" → **메인 플레이어 미확정 — 교체 가능한 캐릭터 슬롯 구조로 구현, 기본 적용 캐릭터는 임시 지정 + 쉬운 교체 경로**
2. **몬스터 4종(Slime/Rat/Mimic/Bat) 전부 활성화** — "4종 전부" → Enemy.cs 종류별 파라미터/애니메이션으로 확장
3. **파럴랙스 배경 확정**: 7레이어 순서 + 계수 (아래 표) — "ㅇㅇㅇ" 승인
4. **HP HUD 에셋 사용** (sprite/UI/HP_HUD.png)

**status: ✅ 승인 완료 (2026-09-18) — 워커 실행 대기. 실행 시작 방법: `/start-work`**

### 파럴랙스 배경 레이어 순서 (확정)
| 순서 | 파일 | 내용 | 계수 |
|---|---|---|---|
| 1 (뒤) | `-6.png` | 하늘 그라데이션 | 0.05 |
| 2 | `0.png` | 지평선 실루엣 | 0.15 |
| 3 | `-2.png` | 원경 도시 (밀집) | 0.30 |
| 4 | `-3.png` | 중경 도시 | 0.45 |
| 5 | `-1.png` | 근경 도시 (상세+안테나) | 0.60 |
| 6 | `-4.png` | 식생/구름 | 0.75 |
| 7 (앞) | `-5.png` | 전경 식생 | 0.90 |

### 플레이어 에셋 인벤토리 (Assets/sprite/player/)
| 폴더 | 구성 | 특징 |
|---|---|---|
| `heavy_player/` | 시트 30개 (_Idle/_Run/_Jump/_Fall/_Attack/_Attack2/_AttackCombo/_Dash/_WallSlide/_Hit/_Death/_Roll 등) | 대형 검 전사 |
| `sword_,man/` | adventurer 시트 100개+ (idle/run/jump/fall/attack1-3/hurt/die/cast/ladder/wall-slide/crnr-jmp 등) | 모험가 — 가장 풍부 |
| `sword_man2/` | 시트 10개 (IDLE/WALK/RUN/JUMP/DEFEND/ATTACK 1/ATTACK 2/ATTACK 3/HURT/DEATH) | **이전 "player_2" 지시(IDLE/WALK/RUN/JUMP/DEFEND/ATTACK 1~3/HURT/DEATH)와 정확히 일치** — 기본 적용 후보 |
| `TheHand/` | adventurer 시트 70장 | 맨손/발차기 |
| `Warrior/` | 시트 2종 (noEffect/Effect, 14행×8열) + 개별 프레임 폴더 | 칼 전사 — 2D 시트 |

### 몬스터 에셋 인벤토리 (Assets/sprite/monster/)
| 종 | 시트 | 프레임 (대략) |
|---|---|---|
| Slime | idle/walk/attack/hurt/death | idle 15, walk 6, attack 15, hurt 3 (가로 스트립) |
| Rat | idle/run/attack_bite/hurt/death | idle 10 |
| Mimic | 시트 10개 (보물상자 변신) | —
| Bat | fly/attack/hurt/death/fall | fly 12 |

모든 시트 가로 단일 스트립, 캐릭터 오른쪽 향함.

## 2026-09-17 피드백 (사용자 플레이테스트) — 계획 반영 필요
1. **플레이어 넉백 안 됨** — 코드 원인: PlayerController.FixedUpdate:61 `rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y)`가 매 프레임 x속도 덮어씀 → PlayerHealth.TakeDamage:59의 `rb.linearVelocity = knockbackDir * force`가 다음 FixedUpdate에서 즉시 소멸. **수정 todo 필요 (넉백 중 velocity 보호/AddForce 방식)**
2. **다가가면 공격 안 해도 적이 넉백됨** — 씬에 Enemy 미배치 상태로 코드상 TakeDamage 호출 경로는 공격/패링뿐 → 물리 충돌 밀림(mass/충돌행렬) 또는 테스트 잔재 가능성. 재현 상황 사용자 확인 필요
3. **카메라 클로즈업 + 슬로우모션 없음** — Wave 4(CombatDirector/CombatCamera) 미구현이 원인 → 계속 진행. "공격할때" 시점(적 telegraph vs 플레이어 공격) 확인 필요
4. **HP HUD 없음** — Wave 5-3(UIManager) 미구현이 원인 → 계속 진행
5. **방향키만 누르면 점프모션** — 사용자가 "에셋 문제였음, 새로 구해옴"으로 정정 → 추가 작업 없음
6. **플레이어 에셋 보존 + 새 프리셋 생성, 에셋은 나중에 직접 넣어줌** — "프리셋" 의미 확인 필요 (새 Player 프리팹?)

## 2026-09-17 코드베이스 재확인 (세션 재개 시점)
- Scripts 10개: PlayerController/PlayerCombat/PlayerHealth/Enemy/EnemyHealth/CombatEvents/GameManager(stub)/VfxSlot(stub)/IEpsilonDamagable/CameraZone(제거 대상, 잔존)
- Player.prefab: PlayerController+PlayerCombat+PlayerHealth 부착 완료, SpriteRenderer(idle_0), Animator(PlayerAnimator) 연결 — Todo 2-4 부착 부분 완료
- Enemy.prefab: Enemy+EnemyHealth 부착, placeholder 스프라이트, Animator(EnemyAnimator) 연결
- 씬(Main.unity): Enemy 미배치 (Wave 6-1 대기)
- boulder.json: combat-platformer-2c74e521 = paused

## 세션 기록
### 2026-09-16 인터뷰 (범위 확정)
- "카메라가 안 끊기는 건 당연한 것" — 개선 대상 아님
- "원래 총게임인데 발표용 패링+칼" — 패링+칼 시스템 우선
- "카메라는 따라가고 방의 개념은 일단 없음" — 개방형
- "보스전 빼고 확대연출은 있어야" — 카메라 확대 포함, 보스 제외
- "긴 하나의 스테이지고 개방형이랑 이동전투 반복 둘 다 맞고" — 단일 스테이지 + 이동↔전투 루프
- "구름 언덕은 불필요" — 장식 제거
- "방,칸구조는 보스전에 채택할거야" — 추후 보스전 설계 요소로 예약
- "나머지 내가 말 안한건 새로운 계획서로 넣는거야" — 미기재 사항 기본값 채택
- "아직 에셋 없는건 틀만 만들고 내가 넣어주면 바로 할수있게" — 스켈레톤 구조
- "vfx는 지금은 하지마 내가 새로 찾아옴 저건 안씀" — 기존 vfx 폴더 사용 금지

### 2026-09-16 코드베이스 탐색 결과 (로컬 확인)
- Assets/Scripts/: PlayerController.cs(legacy Input), CameraZone.cs
- Assets/Prefabs/Player.prefab (SpriteRenderer 미할당, Rigidbody2D Interpolate, groundCheck 자식)
- Assets/Scenes/Main.unity (플랫폼/벽/장식/Cinemachine vcam/Zone1/LevelBounds 포함)
- Assets/sprite/player/: idle.png, jump.png, revision-walking.png (3개)
- Assets/sprite/vfx/: 471개 (사용 금지 결정)
- .anim / .controller / .mat: 0개
- InputSystem_Actions.inputactions 존재하나 미사용 (legacy Input 사용 중)

## Approach
기존 플랫포머 기반(PlayerController + Cinemachine) 위에 패링/칼 전투 시스템을 계층 추가:
Wave 1 씬정리 → Wave 2 플레이어 전투 → Wave 3 적 → Wave 4 전투 연출 → Wave 5 게임 흐름 → Wave 6 스테이지+통합

## 2026-09-17 질문 답변
1. **프리셋**: 기존 Player 프리팹 유지, 새 에셋 슬롯만 준비 → 기존 asset-slots.md 구조가 충분, 새 작업 불필요
2. **연출 타이밍**: **둘 다** — 적 텔레그래프 시 + 플레이어 칼 공격 시 연출 (기존 계획 확장: CombatDirector가 CombatEvents.OnEnemyAttackTelegraph AND PlayerCombat.OnAttackPerformed 이벤트 구독)
3. **넉백 2**: 이전 테스트 — Play Mode QA로 종합 검증 (Wave 6-2에서)

## Pending Questions
없음 — 전부 확정 또는 기본값 채택

## 2026-09-18 세션 기록 (에셋 대량 추가 + 계획 확정)
- **플레이어 에셋 질문** → "아니 player안에있는 폴더ㅓ 싹다" + "이제 플레이어도 4종됬을거야 나중에 메인 플레이어는 보고정하게" → 폴더 5개(heavy_player, sword_,man, sword_man2, TheHand, Warrior) 전부 활용, **캐릭터 슬롯 구조** (메인 미확정 → 교체 용이), 기본 적용 = sword_man2 (이전 "player_2" 10종 구성과 일치)
- **몬스터 질문** → "4종 전부" (Slime/Rat/Mimic/Bat) → Wave 8 (Enemy.cs 파라미터화 + 종별 프리팹/애니메이션)
- **파럴랙스 배경** → "background는 레이어별로 속도 다르게 하는기법 알지?" → 순서 7레이어 분석 완료 + 계수 표 확정 → Wave 9 (ParallaxController + 씬 배치)
- **HP HUD** → `sprite/UI/HP_HUD.png` 적용 → Wave 10-1
- **계획서 갱신 완료**: Wave 7~10 추가 (Todo 20~27), Scope IN 갱신 (적 4종/파럴랙스/HP HUD/캐릭터 슬롯), Success Criteria 14개, Commit Strategy/Wave별 커밋 갱신
- **✅ 사용자 승인 (2026-09-18)** — 계획 확정, 실행은 `/start-work`로 시작