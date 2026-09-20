PLAYER 스프라이트 팩 (전체 4배 버전)
====================================
이 팩의 모든 이미지는 원본 픽셀 크기의 정확히 4배예요 (Nearest Neighbor, 손실 없음).
모든 파일이 같은 배율이라서 캐릭터, 이펙트, 그림자 크기 비율이 서로 맞아요.

폴더 구성
- Animations : idle, walk, attack, dash, jump, fall, doublejump, hurt, parrying, combo_attack2, combo_attack
- Effects    : dash_fx, doublejump_fx, parrying_fx, combo_attack2_fx, combo_attack2_line
- Shadow     : shadow.png
- Extras     : attack_prop.png (안 써도 되는 파일)

유니티 임포트 설정 (전부 똑같이)
- Pixels Per Unit : 400
- Filter Mode : Point (no filter)
- Compression : None
- Generate Mip Maps : 끄기
- Max Size : 2048
- 유닛 값은 4배가 되기 전과 똑같아요. 가이드(이동_이펙트_가이드.txt)의 0.88, 1.47 같은 숫자를 그대로 쓰세요.
  (PPU 가 100 에서 400 으로 바뀌었어도 1유닛의 실제 크기는 같아요.)

피벗
- Center (0.5, 0.5) : attack, dash, jump, fall, doublejump, hurt, parrying, combo_attack2, 모든 Effects, shadow
- Custom X 0.5 / Y 0.4545 : idle
- Custom X 0.5 / Y 0.4310 : walk
  (idle 과 walk 는 발이 캔버스 맨 아래에 있어서, 이렇게 해야 다른 모션과 발 높이가 맞아요. 발이 피벗보다 0.25유닛 아래에 오는 기준이에요.)
- Custom X 0.8965 / Y 0.1143 : combo_attack (옛 컷신 방식 콤보. combo_attack2 로 대체했다면 안 써도 돼요.)

프레임 수와 캔버스 크기
idle                 10프레임   184 x 220
walk                 24프레임   180 x 232
attack               36프레임   1040 x 552
dash                 12프레임   456 x 296
jump                  7프레임   272 x 264
fall                  9프레임   272 x 384
doublejump            7프레임   264 x 280
hurt                  5프레임   352 x 264
parrying             13프레임   904 x 288
combo_attack2        13프레임   1016 x 312
combo_attack         59프레임   1044 x 984
dash_fx               5프레임   304 x 628
doublejump_fx         4프레임   296 x 52
parrying_fx           5프레임   412 x 100
combo_attack2_fx      7프레임   248 x 144
combo_attack2_line    4프레임   1040 x 20
shadow.png             1장         120 x 28

파일 이름 규칙 : 모션이름_번호_딜레이.png  (예: fall_00_0.07s.png). 딜레이는 참고용이에요.
원본 프레임 번호는 frame_map.txt 와 frame_map_parry.txt 를 보세요.
이동량과 이펙트 위치는 이동_이펙트_가이드.txt 를 보세요.
