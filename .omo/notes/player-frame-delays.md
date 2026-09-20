# 플레이어 프레임 딜레이 데이터 (Player Frame Delays)

> 이 문서는 **프레임 PNG 파일명에서 딜레이 정보(`_0.XXs`)를 제거하기 전에 보존한 원본 데이터**입니다.
> 2026-09-19, 178개 파일 전부에서 추출. 파일명은 이제 `{motion}_{index}.png` 형태로 정리됨.
> 형식: `[모션]` / `프레임번호 = 딜레이(초)`

## 추가로 보존한 정보
- `fall_03 = custom(s)` — fall 루프 **홀드 프레임** 지정 마커 (원래 `fall_03_custom(s).png`).
  딜레이 없음 → 빌더에서 fallback(0.06s) 사용. unity 클립에서 직접 커스텀 홀드 길이 설정해야 함.

---

## Animations/attack
```
attack_00 = 0.02
attack_01 = 0.02
attack_02 = 0.04
attack_03 = 0.04
attack_04 = 0.08
attack_06 = 0.08
attack_08 = 0.08
attack_10 = 0.04
attack_12 = 0.02
attack_13 = 0.02
attack_14 = 0.08
attack_16 = 0.08
attack_18 = 0.08
attack_20 = 0.04
attack_21 = 0.04
attack_22 = 0.13
attack_24 = 0.08
attack_26 = 0.04
attack_27 = 0.04
attack_28 = 0.08
attack_30 = 0.08
attack_32 = 0.08
attack_34 = 0.08
attack_36 = 0.04
attack_37 = 0.04
attack_38 = 0.08
attack_40 = 0.08
attack_42 = 0.04
attack_43 = 0.04
attack_44 = 0.04
attack_45 = 0.04
attack_46 = 0.04
attack_47 = 0.04
attack_48 = 0.08
attack_50 = 0.06
attack_52 = 0.04
```

## Animations/combo_attack
```
combo_attack_00 = 0.02
combo_attack_01 = 0.02
combo_attack_02 = 0.04
combo_attack_03 = 0.04
combo_attack_04 = 0.04
combo_attack_05 = 0.04
combo_attack_06 = 0.04
combo_attack_07 = 0.04
combo_attack_08 = 0.04
combo_attack_09 = 0.02
combo_attack_10 = 0.04
combo_attack_12 = 0.02
combo_attack_13 = 0.02
combo_attack_14 = 0.04
combo_attack_15 = 0.04
combo_attack_16 = 0.04
combo_attack_17 = 0.04
combo_attack_18 = 0.08
combo_attack_20 = 0.04
combo_attack_21 = 0.04
combo_attack_22 = 0.04
combo_attack_23 = 0.04
combo_attack_24 = 0.04
combo_attack_25 = 0.04
combo_attack_26 = 0.04
combo_attack_27 = 0.04
combo_attack_28 = 0.04
combo_attack_29 = 0.04
combo_attack_30 = 0.04
combo_attack_31 = 0.04
combo_attack_32 = 0.04
combo_attack_33 = 0.04
combo_attack_34 = 0.08
combo_attack_36 = 0.08
combo_attack_38 = 0.08
combo_attack_40 = 0.08
combo_attack_42 = 0.04
combo_attack_43 = 0.04
combo_attack_44 = 0.08
combo_attack_46 = 0.04
combo_attack_47 = 0.04
combo_attack_48 = 0.08
combo_attack_50 = 0.06
combo_attack_52 = 0.04
combo_attack_54 = 0.08
combo_attack_56 = 0.12
combo_attack_59 = 0.14
combo_attack_60 = 0.04
combo_attack_62 = 0.04
combo_attack_64 = 0.04
combo_attack_66 = 0.04
combo_attack_68 = 0.08
combo_attack_70 = 0.08
combo_attack_72 = 0.08
combo_attack_74 = 0.08
combo_attack_76 = 0.08
combo_attack_78 = 0.08
combo_attack_80 = 0.08
combo_attack_82 = 0.08
```

## Animations/dash
```
dash_00 = 0.07
dash_01 = 0.07
dash_02 = 0.07
dash_03 = 0.07
dash_05 = 0.07
dash_06 = 0.07
dash_07 = 0.07
dash_08 = 0.07
dash_09 = 0.07
dash_10 = 0.07
dash_11 = 0.07
dash_12 = 0.07
```

## Animations/doublejump
```
doublejump_00 = 0.07
doublejump_01 = 0.07
doublejump_02 = 0.07
doublejump_03 = 0.07
doublejump_04 = 0.07
doublejump_05 = 0.07
doublejump_06 = 0.07
```

## Animations/fall
```
fall_00 = 0.07
fall_01 = 0.07
fall_02 = 0.07
fall_03 = custom(s)   ← 루프 홀드 프레임 마커 (딜레이 없음, Unity에서 직접 설정)
fall_04 = 0.07
fall_05 = 0.07
fall_06 = 0.07
fall_07 = 0.07
fall_08 = 0.07
```

## Animations/hurt
```
hurt_00 = 0.12
hurt_01 = 0.12
hurt_02 = 0.12
hurt_03 = 0.12
hurt_04 = 0.12
```

## Animations/idle
```
idle_00 = 0.06
idle_01 = 0.06
idle_02 = 0.06
idle_03 = 0.06
idle_04 = 0.06
idle_05 = 0.06
idle_06 = 0.06
idle_07 = 0.06
idle_08 = 0.06
idle_09 = 0.06
```

## Animations/jump
```
jump_00 = 0.07
jump_01 = 0.07
jump_02 = 0.07
jump_03 = 0.07
jump_04 = 0.07
jump_05 = 0.07
jump_06 = 0.14
```

## Animations/walk
```
walk_00 = 0.06
walk_01 = 0.06
walk_02 = 0.06
walk_03 = 0.06
walk_04 = 0.06
walk_05 = 0.06
walk_06 = 0.06
walk_07 = 0.06
walk_08 = 0.06
walk_09 = 0.06
walk_10 = 0.06
walk_11 = 0.06
walk_12 = 0.06
walk_13 = 0.06
walk_14 = 0.06
walk_15 = 0.06
walk_16 = 0.06
walk_17 = 0.06
walk_18 = 0.06
walk_19 = 0.06
walk_20 = 0.06
walk_21 = 0.06
walk_22 = 0.06
walk_23 = 0.06
```

## Effects/dash_fx
```
dash_fx_00 = 0.07
dash_fx_01 = 0.07
dash_fx_02 = 0.07
dash_fx_03 = 0.07
dash_fx_04 = 0.07
```

## Effects/doublejump_fx
```
doublejump_fx_00 = 0.07
doublejump_fx_01 = 0.07
doublejump_fx_02 = 0.07
doublejump_fx_03 = 0.07
```