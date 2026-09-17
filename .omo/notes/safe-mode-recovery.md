# SAFE MODE 복구 계획 — Enemy.cs 미생성으로 인한 컴파일 에러

> 상태: **긴급 복구 필요** (2026-09-16)
> 작업물: `.omo/plans/combat-platformer.md` — Todo 3-1 ~ 3-2

## 원인 (확인됨)
- `Assets/Scripts/Enemy.cs` **존재하지 않음** (Todo 3-1 서브에이전트 abort로 미생성)
- `Assets/Scripts/EnemyHealth.cs:68-69`가 `GetComponent<Enemy>()` 참조
- → CS0246 컴파일 에러 → **Unity Safe Mode 진입** (에디터 실행 불가)

## 복구 단계

### Step 1: EnemyHealth.cs 수정 (즉시 — Safe Mode 해제용, 임시)
`Assets/Scripts/EnemyHealth.cs` 67~69행:
```csharp
// Hurt 상태 요청 (Enemy.cs에 public 상태 전환 메서드가 있으면 호출)
var enemy = GetComponent<Enemy>();
enemy?.SetHurtState();
```
를 아래로 교체:
```csharp
// Hurt 상태 요청: Enemy.cs 생성(Todo 3-1) 후 복원 예정
// var enemy = GetComponent<Enemy>();
// enemy?.SetHurtState();
```
- 이 외 코드 수정 금지 (파일 전체 보존, UTF-8 유지)

### Step 2: 사용자 — Unity 에디터 재시작
- Safe Mode 창 닫기 → 에디터 재실행 → 컴파일 성공 확인 → 에디터 정상 진입
- 에디터 접속 후: 콘솔 에러 0 확인

### Step 3: Wave 3 본 구현 (복구 후)
- Todo 3-1: `Enemy.cs` 상태 머신 생성 (전이/이벤트 발화/리스폰 리셋 포함)
- Todo 3-2: `EnemyHealth.cs`의 Enemy 참조 복원 (`GetComponent<Enemy>()?.SetHurtState()`)
- 다시 3-1 → 3-2 의존 순서로 진행 (병렬 금지 — 이번 사고 원인)

## 검증 (QA)
- [ ] 유니티 에디터 재진입 성공 (Safe Mode 아님)
- [ ] 콘솔 에러 0
- [ ] Play Mode 진입 가능
- [ ] 돌이켜보기: Wave 3 병렬 위임 금지 원칙 문서화 (notepad issues.md 기록)

## 관련 파일
- `Assets/Scripts/EnemyHealth.cs` — 수정 대상
- `Assets/Scripts/Enemy.cs` — 이후 생성 대상
- `.omo/plans/combat-platformer.md` — 계획서 (Todo 3-1/3-2)