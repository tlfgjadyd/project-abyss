# Project: Abyss - 에셋 조달 및 적용 계획

작성일: 2026-05-29

이 문서는 Project: Abyss의 8주차 마무리 단계에서 필요한 시각/사운드 에셋을 빠르게 조달하고 Unity에 적용하기 위한 실행 문서다. 현재 프로젝트는 기능 구현이 대부분 완료된 상태이며, 에셋 작업의 목표는 "완전한 고유 아트 완성"이 아니라 **플레이어가 한눈에 심해 생체 실험체 생존 액션 게임으로 인식할 수 있는 최소 완성도 확보**다.

---

## 0. 현재 판단

### 프로젝트 정체성

- 장르: 2D 탑다운 뱀서라이크
- 핵심 키워드: 심해, 비밀 연구소, 생체 실험체, 키메라, 돌연변이, 발광 생물, 복수
- 시각 톤: 어두운 청록/남색 배경 + 흰색/청록색 발광 포인트 + 일부 붉은 경고/폭주 색상
- 현재 병목: 기능은 거의 완성됐지만 실제 스프라이트/배경/사운드 자산이 부족해 게임의 정체성이 약하게 보임

### 현재 에셋 상태

- `Assets/Sprites/` 하위 폴더는 존재하나 실제 스프라이트 파일은 거의 없음
- `Assets/Audio/` 폴더는 아직 없음
- 프리팹/ScriptableObject 구조는 이미 잘 정리되어 있어 에셋만 연결하면 효과가 큼
- 스크린샷 기준, 아직 흰 사각형/단색 임시 그래픽 느낌이 강함

---

## 1. 에셋 작업 원칙

### 1-1. 완성 목표

1. 플레이어, 적, 보스가 서로 구분되어야 한다.
2. 스테이지 1~4의 분위기 차이가 보여야 한다.
3. 스킬/카피 스킬은 최소한 색과 형태로 구분되어야 한다.
4. UI 아이콘은 없어도 플레이 가능하지만, 스킬 선택/카피 선택의 느낌을 살리기 위해 일부만 우선 제작한다.
5. 사운드는 고유성이 낮아도 되므로 무료/범용 SFX를 적극 활용한다.

### 1-2. 조달 전략

무료 에셋 사이트에서 프로젝트에 딱 맞는 것을 찾기 어렵다면 아래 비율을 권장한다.

| 분류 | 추천 방식 | 이유 |
|------|----------|------|
| 플레이어 | AI 생성 + 간단 보정 | 게임 정체성의 중심이라 고유 이미지가 필요 |
| 일반 적 | 무료 에셋 + AI 생성 혼합 | 종수가 많아 전부 직접 만들면 부담 큼 |
| 보스 | AI 생성 우선 | 크고 독특해야 하므로 무료 에셋과 잘 안 맞음 |
| 배경 | 간단 제작 + 무료 텍스처 혼합 | 탑다운 뱀서류는 배경 디테일보다 분위기/가독성이 중요 |
| 스킬 이펙트 | Unity LineRenderer/Particle + 간단 PNG | 현재 코드 이펙트를 살리면 부담이 적음 |
| UI 아이콘 | AI 생성 또는 무료 아이콘 리컬러 | 작게 보이므로 완벽할 필요 없음 |
| SFX/BGM | 무료 에셋 적극 사용 | 라이선스 부담이 낮고 대체가 쉬움 |

---

## 2. 우선순위 요약

### A급 - 반드시 필요

| 항목 | 수량 | 적용 위치 |
|------|------|----------|
| 플레이어 키메라 스프라이트 | 1 | `Assets/Sprites/Player/` |
| 일반 적 대표 스프라이트 | 8~10 | `Assets/Sprites/Enemies/Stage*/` |
| 보스 스프라이트 | 4 | `Assets/Sprites/Bosses/` |
| 스테이지 배경/바닥 텍스처 | 4 | `Assets/Sprites/Background/` |
| 경험치 오브/투사체 스프라이트 | 3~5 | `Assets/Sprites/UI/` 또는 `Assets/Sprites/Effects/` |
| 기본 SFX | 6~10 | `Assets/Audio/SFX/` |

### B급 - 있으면 완성도가 크게 오름

| 항목 | 수량 | 적용 위치 |
|------|------|----------|
| 스킬 아이콘 | 12 | `Assets/Sprites/UI/SkillIcons/` |
| 카피 스킬 아이콘 | 9 | `Assets/Sprites/UI/CopySkillIcons/` |
| 보스 등장/사망 이펙트 | 4 | `Assets/Sprites/Effects/` |
| 스테이지별 BGM | 5 | `Assets/Audio/BGM/` |
| UI 클릭/선택/레벨업 SFX | 4~6 | `Assets/Audio/SFX/UI/` |

### C급 - 시간이 남으면

| 항목 | 수량 | 비고 |
|------|------|------|
| 플레이어 돌연변이별 외형 | 5 | 색상/오버레이만으로도 대체 가능 |
| 적 피격/사망 애니메이션 | 다수 | 현재 HitEffect/DeathExplosion으로 대체 가능 |
| 보스 패턴 전용 텔레그래프 | 다수 | LineRenderer/경고 원형으로 먼저 처리 |
| 배경 오브젝트 장식 | 다수 | 연구소 파편, 산호, 심해 암석 등 |

---

## 3. 폴더 구조 권장안

현재 폴더 구조를 유지하면서 아래만 추가한다.

```text
Assets/
  Audio/
    BGM/
    SFX/
      Combat/
      UI/
      Boss/
  Sprites/
    Background/
      Stage1_Lab/
      Stage2_Sea/
      Stage3_Deep/
      Stage4_Ruined/
    Bosses/
    Effects/
    Enemies/
      Stage1/
      Stage2/
      Stage3/
      Stage4/
    Player/
    UI/
      SkillIcons/
      CopySkillIcons/
```

파일명은 프리팹/데이터 이름과 맞추는 것이 좋다.

```text
Player_Chimera.png
Enemy_Guard.png
Enemy_Shooter.png
Boss_Oarfish.png
Skill_Slash.png
CopySkill_VoidPierce.png
BG_Stage3_Deep_Tile.png
SFX_PlayerHit_01.wav
BGM_Stage3_Deep.ogg
```

---

## 4. 캐릭터/적 스프라이트 계획

### 4-1. 플레이어

| 항목 | 내용 |
|------|------|
| 파일 | `Assets/Sprites/Player/Player_Chimera.png` |
| 크기 | 512x512 원본 권장, Unity에서는 96~160px 정도로 사용 |
| 방향 | 탑다운 또는 3/4 탑다운 |
| 핵심 실루엣 | 작은 몸체 + 긴 사슬팔/갈고리 팔 |
| 색 | 짙은 청록, 회백색 생체 조직, 흰/청록 발광 |

AI 생성 프롬프트 예시:

```text
top-down 2D game sprite of a deep sea chimera experiment creature, dark teal translucent skin, glowing white cyan bioluminescent spots, segmented bone chain arms ending in sharp hooks, small body, readable silhouette, action RPG enemy sprite style, transparent background, centered, no text, no UI, no shadow, high contrast
```

네거티브 프롬프트:

```text
realistic photo, 3d render, side view, front portrait, background, text, watermark, blurry, too many tiny details, horror gore
```

### 4-2. Stage 1 - 해저 연구소

| 프리팹 | 권장 비주얼 | 우선순위 |
|--------|------------|----------|
| `Enemy_Guard` | 방패 든 연구소 전투요원 | A |
| `Enemy_Default` | 근접 전투 보조요원 또는 실험복 요원 | A |
| `Enemy_Shooter` | 마취총/제압총 든 요원 | A |
| `Boss_ExperimentalSubjects` | 플레이어와 닮았지만 더 크고 불안정한 실험체 | A |

프롬프트 예시:

```text
top-down 2D game sprite of an underwater laboratory security guard, armored diver suit, rectangular shield, dark navy suit with cyan lights, readable silhouette, transparent background, centered, no text
```

```text
top-down 2D boss sprite of a failed bio experiment creature in an underwater laboratory, bulky mutated body, broken restraints, glowing cyan wounds, aggressive silhouette, transparent background, centered, no text
```

### 4-3. Stage 2 - 해저

| 프리팹 | 권장 비주얼 | 우선순위 |
|--------|------------|----------|
| `Enemy_Piranha` | 작은 발광 피라냐 | A |
| `Enemy_Marlin` | 길쭉하고 빠른 청새치 | B |
| `Enemy_Shark` | 상어 실루엣 | A |
| `Enemy_ElectricEel` | 노란/청록 전기뱀장어 | A |
| `Boss_Whale` | 거대한 향유고래, 초음파 발광 패턴 | A |

프롬프트 예시:

```text
top-down 2D game sprite of a mutated deep sea electric eel, long thin body, cyan and yellow electric glow, readable silhouette, transparent background, centered, no text
```

```text
top-down 2D boss sprite of a massive sperm whale mutated by deep sea energy, dark blue body, glowing sonar rings on forehead, intimidating but readable silhouette, transparent background, centered, no text
```

### 4-4. Stage 3 - 심해

| 프리팹 | 권장 비주얼 | 우선순위 |
|--------|------------|----------|
| `Enemy_Anglerfish` | 발광 미끼가 있는 심해아귀 | A |
| `Enemy_GiantSquid` | 긴 촉수의 대왕오징어 | A |
| `Enemy_Jellyfish` | 반투명 자폭 해파리 | A |
| `Enemy_BalloonEel` | 꼬리/몸체가 발광하는 풍선장어 | B |
| `Boss_Oarfish` | 길고 마디 발광이 있는 산갈치 | A |

프롬프트 예시:

```text
top-down 2D game sprite of a deep sea anglerfish, black blue body, bright cyan lure, monstrous teeth, compact readable silhouette, transparent background, centered, no text
```

```text
top-down 2D boss sprite of a giant oarfish, extremely long serpentine body, segmented glowing cyan nodes along the body, abyssal deep sea creature, readable boss silhouette, transparent background, centered, no text
```

산갈치 보스는 한 장짜리 긴 이미지보다 아래 둘 중 하나가 적용하기 쉽다.

1. 머리/몸통/꼬리 3개 PNG로 분리
2. 현재 구현된 마디 오브젝트에 같은 몸통 스프라이트를 반복 적용

### 4-5. Stage 4 - 파괴된 연구소

| 프리팹 | 권장 비주얼 | 우선순위 |
|--------|------------|----------|
| `Enemy_ReinforcedGuard` | 강화 방패 전투요원 | A |
| `Enemy_LaserSoldier` | 레이저 장비를 든 전투요원 | A |
| `Enemy_ReinforcedSoldier` | 빠른 강화 근접 요원 | A |
| `Enemy_Drone` | 자폭 드론 | A |
| `Boss_Director` | 인간+기계강화 연구소장 | A |

프롬프트 예시:

```text
top-down 2D game sprite of a reinforced underwater lab soldier, heavy exosuit armor, red warning lights, damaged lab equipment style, readable silhouette, transparent background, centered, no text
```

```text
top-down 2D boss sprite of a cybernetic laboratory director, human scientist fused with mechanical exosuit and bio tubes, red and cyan glowing parts, final boss silhouette, transparent background, centered, no text
```

---

## 5. 배경 에셋 계획

탑다운 뱀서라이크에서는 배경이 너무 화려하면 탄막/적/경험치가 묻힌다. 따라서 **낮은 대비의 반복 타일 + 약한 장식 오브젝트**가 가장 안전하다.

| 스테이지 | 배경 방향 | 기본 색 | 장식 |
|----------|----------|---------|------|
| Stage1 Lab | 금속 바닥, 연구소 타일 | 어두운 청회색 | 케이블, 격자, 배수구 |
| Stage2 Sea | 해저 모래/암반 | 어두운 남청색 | 산호, 암석, 해초 |
| Stage3 Deep | 거의 검은 심해 바닥 | 암청/흑청 | 발광 입자, 균열, 뼈 |
| Stage4 Ruined | 파괴된 연구소 | 검정+붉은 경고색 | 잔해, 불꽃, 금속 파편 |

AI 생성 프롬프트 예시:

```text
seamless top-down 2D game floor texture for an underwater secret laboratory, dark metal tiles, subtle grid, low contrast, dark blue gray, small cables and vents, no characters, no text, tileable, game background
```

```text
seamless top-down 2D game background texture of abyssal deep sea floor, very dark navy blue, subtle rocks and glowing particles, low contrast, readable for action game, no characters, no text, tileable
```

적용 팁:

- 배경은 고해상도 한 장보다 512x512 또는 1024x1024 타일 반복이 편하다.
- 스테이지별로 바닥색만 달라도 체감 차이가 크다.
- Stage3는 기존 시야 제한이 있으므로 배경 디테일을 적게 둔다.
- Stage4는 붉은 경고선/파손 타일을 조금만 넣어도 "복수/파괴" 느낌이 산다.

---

## 6. 스킬/이펙트 계획

### 6-1. 일반 스킬 12종

| 데이터 | 아이콘/이펙트 방향 | 우선순위 |
|--------|-------------------|----------|
| `Slash` | 휘두르는 사슬팔, 청록 부채꼴 | A |
| `PoisonNeedle` | 초록 독침 | A |
| `BionicExplosion` | 청록 원형 폭발 | A |
| `ElectricEngine` | 노란/청록 전기 | A |
| `CellRegeneration` | 세포/심장/회복 | B |
| `AcceleratedMutation` | 빠른 발톱/속도선 | B |
| `NeuralAcceleration` | 신경망/번개 | B |
| `DrainTentacle` | 흡혈 촉수 | B |
| `GlowOrgan` | 발광 방패/광원 | B |
| `MagneticInduction` | 자기장 링 | B |
| `SonicPulse` | 음파 링 | B |
| `SpikeBurst` | 가시 폭발 | B |

아이콘 프롬프트 공통 템플릿:

```text
square 2D game skill icon, [SKILL THEME], dark deep sea background, cyan bioluminescent glow, high contrast, clean readable shape, no text, no letters, no UI frame
```

### 6-2. 카피 스킬 9종

| 데이터 | 비주얼 방향 | 우선순위 |
|--------|-------------|----------|
| `CopySkill_Berserk` | 붉은 폭주 오라 | B |
| `CopySkill_Dash` | 사슬팔/몸체 잔상 대시 | A |
| `CopySkill_HealingFactor` | 회복 세포 | B |
| `CopySkill_Ultrasonic` | 고래 초음파 부채꼴 | A |
| `CopySkill_DeepPressure` | 압력 원형장 | A |
| `CopySkill_PredatorCharge` | 고래/상어 돌진 | B |
| `CopySkill_VoidPierce` | 긴 산갈치 마디 사슬 광선 | A |
| `CopySkill_GlowFrenzy` | 8개 발광 마디 폭발 | A |
| `CopySkill_BleedSwim` | 붉은 궤적 유영 | B |

`VoidPierce` 재설계 방향:

- 단순 LineRenderer 빔보다 "산갈치 마디를 카피한 사슬팔" 느낌이 좋다.
- 구현 부담이 낮은 방법:
  - 작은 원형/타원형 발광 마디 PNG 1개 제작
  - 시전 방향으로 여러 개를 일정 간격 배치
  - 위에 얇은 LineRenderer를 깔아 연결감 부여
  - 색상은 중심 흰색, 외곽 청록/보라

---

## 7. UI 에셋 계획

### 우선 제작

| 항목 | 파일 예시 | 비고 |
|------|-----------|------|
| 세포 재화 아이콘 | `UI_Cell.png` | 메타 업그레이드/보상 |
| 생체 에너지 아이콘 | `UI_BioEnergy.png` | HUD |
| HP 아이콘 | `UI_HP.png` | HUD |
| EXP 오브 아이콘 | `UI_ExpOrb.png` | 드랍/슬라이더 |
| 카피 슬롯 배경 | `UI_CopySlot_Frame.png` | Q/E/Space |

UI는 완전한 그림보다 **작은 크기에서 알아보기 쉬운 실루엣**이 중요하다.

---

## 8. 사운드 계획

### 8-1. 폴더 생성 권장

```text
Assets/Audio/BGM/
Assets/Audio/SFX/Combat/
Assets/Audio/SFX/UI/
Assets/Audio/SFX/Boss/
```

### 8-2. BGM

| 파일명 | 용도 | 분위기 |
|--------|------|--------|
| `BGM_MainMenu.ogg` | 메인 메뉴 | 어둡고 조용한 심해 |
| `BGM_Stage1_Lab.ogg` | 연구소 탈출 | 긴장, 기계음 |
| `BGM_Stage2_Sea.ogg` | 해저 | 넓고 미지의 느낌 |
| `BGM_Stage3_Deep.ogg` | 심해 | 저음, 압박감 |
| `BGM_Stage4_Ruined.ogg` | 파괴된 연구소 | 빠르고 공격적 |
| `BGM_Boss.ogg` | 보스 공통 | 강한 리듬 |

### 8-3. SFX

| 파일명 | 용도 | 우선순위 |
|--------|------|----------|
| `SFX_PlayerHit_01.wav` | 플레이어 피격 | A |
| `SFX_EnemyHit_01.wav` | 적 피격 | A |
| `SFX_EnemyDeath_01.wav` | 적 사망 | A |
| `SFX_LevelUp_01.wav` | 레벨업 | A |
| `SFX_Select_01.wav` | UI 선택 | A |
| `SFX_Slash_01.wav` | 휘두르기 | A |
| `SFX_PoisonNeedle_01.wav` | 독침 발사 | B |
| `SFX_Electric_01.wav` | 전기 기관/뱀장어 | B |
| `SFX_BossSpawn_01.wav` | 보스 등장 | A |
| `SFX_VoidPierce_01.wav` | 공허관통 | B |

사운드 조달 키워드:

```text
underwater ambience
deep sea drone
submarine alarm
organic hit
monster impact
electric zap
laser charge
sonar pulse
ui click sci fi
```

---

## 9. Unity 적용 체크리스트

### 9-1. PNG 임포트 설정

캐릭터/적/보스 스프라이트:

- Texture Type: `Sprite (2D and UI)`
- Sprite Mode: `Single`
- Pixels Per Unit: 100 기준으로 시작
- Filter Mode: 픽셀아트면 `Point`, 일반 일러스트면 `Bilinear`
- Compression: `None` 또는 `Low Quality`
- Mesh Type: `Full Rect`

배경 타일:

- Wrap Mode: `Repeat`
- Filter Mode: `Bilinear`
- Compression: `Low Quality`

UI 아이콘:

- Texture Type: `Sprite (2D and UI)`
- Alpha Is Transparency: 켜기
- Compression: 가능하면 낮게

### 9-2. 프리팹 연결 순서

1. 플레이어 스프라이트 적용
2. 일반 적 A급 스프라이트 적용
3. 보스 스프라이트 적용
4. 투사체/경험치 오브 적용
5. 배경 적용
6. 스킬 아이콘/카피 아이콘 적용
7. 사운드 연결

이 순서가 좋은 이유는 플레이 테스트에서 가장 먼저 눈에 들어오는 것이 플레이어/적/보스이기 때문이다.

### 9-3. 스케일 기준

| 대상 | 화면상 권장 크기 |
|------|----------------|
| 플레이어 | 일반 적보다 약간 큼 |
| 일반 적 | 작지만 실루엣이 보이는 크기 |
| 빠른 적 | 작고 길쭉하게 |
| 탱커 적 | 크고 둔하게 |
| 보스 | 일반 적의 3~8배 |
| 드론 | 작고 밝은 경고색 |

---

## 10. 1차 에셋 패스 추천 목록

시간이 부족하다면 아래 20개만 먼저 만든다.

### 스프라이트 17개

1. `Player_Chimera.png`
2. `Enemy_Guard.png`
3. `Enemy_Default.png`
4. `Enemy_Shooter.png`
5. `Enemy_Piranha.png`
6. `Enemy_Shark.png`
7. `Enemy_ElectricEel.png`
8. `Enemy_Anglerfish.png`
9. `Enemy_GiantSquid.png`
10. `Enemy_Jellyfish.png`
11. `Enemy_ReinforcedSoldier.png`
12. `Enemy_LaserSoldier.png`
13. `Enemy_Drone.png`
14. `Boss_ExperimentalSubjects.png`
15. `Boss_Whale.png`
16. `Boss_Oarfish.png`
17. `Boss_Director.png`

### 배경 4개

1. `BG_Stage1_Lab_Tile.png`
2. `BG_Stage2_Sea_Tile.png`
3. `BG_Stage3_Deep_Tile.png`
4. `BG_Stage4_Ruined_Tile.png`

### 사운드 8개

1. `SFX_PlayerHit_01.wav`
2. `SFX_EnemyHit_01.wav`
3. `SFX_EnemyDeath_01.wav`
4. `SFX_LevelUp_01.wav`
5. `SFX_Select_01.wav`
6. `SFX_Slash_01.wav`
7. `SFX_BossSpawn_01.wav`
8. `BGM_Stage_Common.ogg`

이 29개만 들어가도 흰 사각형 느낌은 대부분 사라지고, 프로젝트의 완성 체감이 크게 오른다.

---

## 11. 2차 에셋 패스 추천 목록

1차가 끝난 뒤 아래를 추가한다.

- 스킬 아이콘 12개
- 카피 스킬 아이콘 9개
- Stage별 BGM 5개
- Stage별 장식 오브젝트 3~5개씩
- 보스 패턴 전용 경고/공격 이펙트
- 돌연변이 선택 카드용 아이콘 5개

---

## 12. 빠른 제작 워크플로우

### 하루 안에 처리하는 방식

1. AI 이미지 생성으로 플레이어/보스 4종 먼저 만든다.
2. 일반 적은 무료 에셋/AI 생성/색변경을 섞어서 8~10종만 확보한다.
3. 배경은 타일 4개를 단순하게 만든다.
4. PNG를 `Assets/Sprites/` 하위에 정리한다.
5. Unity에서 Sprite Import Settings를 통일한다.
6. 프리팹 SpriteRenderer에 연결한다.
7. 실제 플레이 화면에서 크기/색/가독성만 조정한다.

### 작업 시 주의

- AI 생성 이미지는 작은 화면에서 디테일이 뭉개지기 쉽다. 생성 후 128px로 줄여도 실루엣이 보이는지 확인한다.
- 흰색 발광을 너무 많이 쓰면 UI/탄막과 겹친다. 플레이어/중요 스킬에만 강한 발광을 준다.
- 배경은 어둡고 낮은 대비로 둔다. 적과 경험치 오브가 더 중요하다.
- 보스는 크기와 색으로 일반 적과 확실히 구분한다.
- Stage3는 시야 제한 때문에 실루엣과 발광 포인트가 특히 중요하다.

---

## 13. 최종 권장 방향

Project: Abyss는 무료 에셋을 그대로 끼우는 것보다 **AI 생성 핵심 캐릭터 + 무료/범용 이펙트 + 단순 배경 타일** 조합이 가장 현실적이다. 특히 플레이어, 산갈치, 연구소장, 실험용 실험체만 고유하게 보여도 게임의 인상은 크게 살아난다.

따라서 8주차에는 다음 순서를 추천한다.

1. 플레이어/보스 4종 고유화
2. 일반 적 8~10종 구분 가능하게 교체
3. 스테이지 배경 4종 색/패턴 분리
4. 레벨업/보스/타격/사망 SFX 적용
5. 시간이 남으면 스킬 아이콘과 카피 스킬 이펙트 보강

이 방식이면 "완성된 게임처럼 보이는 최소 선"에 가장 빨리 도달할 수 있다.
