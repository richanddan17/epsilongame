# 에셋 슬롯 규칙 (Asset Slot Convention)

## 목적
외부 에셋(스프라이트, 애니메이션, VFX, UI 아트 등)이 준비되지 않은 상태에서 개발을 진행하고, 사용자 제공 시 슬롯 교체만으로 즉시 적용되도록 한다.

## 규칙
1. 모든 외부 에셋 참조는 `[SerializeField] private` 필드로 선언
2. 필드 타입: `GameObject` (프리팹), `AnimationClip`, `Sprite`, `AudioClip` 등
3. Placeholder: 컬러 사각형(Sprite.Create) 또는 기본 Particle System 사용
4. null 안전: `?.Invoke()`, `??` 연산자 등으로 null 체크 필수
5. 각 신규 스크립트 상단에 이 규칙을 Header 주석으로 명시

## 적용 예시
```csharp
[Header("VFX Slots")]
[SerializeField] private GameObject slashEffectPrefab; // null 허용
[SerializeField] private Transform slashSpawnPoint;
```

## 기존 계획서 정리 (승인됨)
- `.omo/plans/platformer-basic.md` → `.omo/archive/platformer-basic.md` 이동
- 헤더에 "SUPERSEDED by combat-platformer.md" 표기
- AGENTS.md §1 관련 문서 줄 수정은 별도 인간 승인 후 반영