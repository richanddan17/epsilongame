PLAYER 스프라이트 팩
====================
폴더 구조
Player/
  Animations/  idle, walk, attack, dash, jump, fall, doublejump, hurt, combo_attack
  Effects/     dash_fx, doublejump_fx
  Shadow/      shadow.png
  Extras/      attack_prop.png
  Preview/     all_motions.gif, Animations/(모션별 GIF), Effects/(이펙트 GIF)

공통 설정: Pixels Per Unit 100 / Filter Mode Point (no filter) / Compression None
바라보는 방향: 왼쪽 (오른쪽으로 쓰려면 Flip X)
파일 이름 규칙: 모션이름_번호_딜레이.png  (예: fall_00_0.07s.png)
번호는 재생 순서대로 00부터, 딜레이는 참고용이에요. 유니티는 읽지 않으니 애니메이션 클립에서 간격을 직접 설정하세요.

폴더 / 프레임 수 / 캔버스 크기
idle            10 frames   46x55
walk            24 frames   45x58
attack          54 frames   260x138
dash            14 frames   114x74
dash_fx          5 frames   76x157
jump             7 frames   68x66
fall             9 frames   68x96
doublejump       7 frames   66x70
doublejump_fx    4 frames   74x13
hurt             5 frames   88x66
combo_attack    83 frames   261x246

피벗
- idle, walk 를 뺀 나머지는 몸통 중심이 캔버스 중앙 => Center (0.5, 0.5)
- combo_attack 만 Custom: X 0.8965, Y 0.1143 (캐릭터 시작 위치, 이동이 프레임에 그대로 남아 있어요)
- idle / walk 는 발이 캔버스 맨 아래라서 Center 로 두면 다른 모션보다 발이 2.5px / 4px 아래로 내려가요.

모션 메모
- 그림자는 프레임에서 전부 제거했어요. shadow.png 를 플레이어 자식으로 붙이세요 (로컬 Y -0.25 ~ -0.28, Sorting Order 는 플레이어보다 낮게).
- 이동은 프레임에서 뺐어요(dash, attack, jump, fall, doublejump). 이동은 코드로 처리하세요.
- dash_fx : dash_07 프레임이 시작될 때 대시 시작 위치에 생성, 캐릭터 중심에서 오른쪽 0.88 / 위 0.09 (왼쪽을 볼 때 기준). 5프레임, 각 0.07초.
- doublejump_fx : doublejump_01 프레임이 시작될 때 2단 점프 시작 위치에 생성, 캐릭터 중심에서 오른쪽 0.49 / 아래 0.65. 4프레임.
- hurt : 5프레임, 각 0.12초. Loop Time 끄기.
- combo_attack : 10~93번 프레임. 컷신처럼 통째로 재생하고 재생 중에는 플레이어 스프라이트를 숨기세요.
- Extras/attack_prop.png : attack 원본에 있던 바닥의 작은 돌(사용 안 해도 됨).
- Preview/ : 확인용 GIF 모음이에요. 게임에 쓰는 파일이 아니에요. dash.gif, doublejump.gif 는 이펙트를 합성한 화면이에요.

원본 프레임 번호 대응은 frame_map.txt 를 보세요.
(똑같은 이미지가 연속된 프레임은 하나로 합치고 딜레이를 더했어요: dash_04, jump_06 = 0.14s, combo_attack_82 = 0.08s)
