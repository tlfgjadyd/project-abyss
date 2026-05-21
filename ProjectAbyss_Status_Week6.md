# Project: Abyss - 6주차 TODO

작성일: 2026-05-18

## 1. 6주차 목표

5주차에서 1~2스테이지와 스테이지 전환 시스템을 완성했다. 6주차의 핵심 목표는 다음 셋이다.

1. **3, 4스테이지 완성** — 게임의 컨텐츠를 일단락 (1런 = 4스테이지)
2. **메타 업그레이드 시스템 도입** — 세포 사용처 마련, 게임 사이클 완성
3. **엔딩 화면 + 빌드 가능 상태** — 1주 플레이 시작 → 4스테이지 클리어까지의 흐름 완성

### 진행 가능한 그림

```text
메인 메뉴 (시작 / 메타 업그레이드)
  → 1스테이지 → 2스테이지 → 3스테이지 (시야 제한) → 4스테이지 (마지막)
  → 보스 처치 (카피 없음) → "EXPERIMENT COMPLETE" 엔딩
  → 결과 화면 (세포 정산) → 메인 메뉴 복귀
  → 메타 업그레이드 후 다시 1스테이지부터
```

### 5주차에서 미룬 작업과의 분리

- **이번 주(6주차)에 처리**: 3·4스테이지, 메타 업그레이드, 엔딩
- **7주차로 미룸**: 스킬 풀 확장, 향유고래 풀 패턴, 매니저 prefab 변환, 본격 밸런싱

## 2. 사전 결정 사항 ✅ (2026-05-17 확정)

| 항목 | 결정 |
|------|------|
| §2-1 시야 제한 | **(A) URP 2D Light** — 프로젝트가 이미 URP+Renderer2D 사용 |
| §2-2 자폭 적 | 문서 그대로 (`EnemySuicideExplode`) |
| §2-3 레이저 | **(B) 별도 컴포넌트** `EnemyLaserAttack` — 발사 모델/시각화가 EnemyRangedAttack과 다름 |
| §2-4 메타 항목 | PlayerPrefs 저장. 압력저항만 5→4단계 +40% 만렙으로 조정 (50%면 압력 컨셉 소멸), 비용 `10/25/60/150`. 나머지 4종은 문서 표 그대로 |
| §2-5 엔딩 | **같은 씬 VictoryPanel** — 별도 Ending 씬 X. `StageManager.LoadEnding`이 패널만 호출 |
| §2-6 메인 메뉴 | **옵션 B** — MainMenu 씬 1개에 메인패널/메타패널 토글 |

각 Day가 이 결정에 의존한다.

### 2-1. 3스테이지 시야 제한 구현 방식

디자인 문서: "시야 제한 (원형 마스킹)". 구현 옵션:

| 옵션 | 설명 |
|------|------|
| **(A) URP 2D Light** | URP가 설치되어 있다면 가장 자연스러움. 원형 라이트 + 어두운 글로벌 라이트 |
| **(B) 풀스크린 검은 Image + 원형 구멍** | 카메라 자식 Canvas + 셰이더로 중앙에 투명 원형. URP 의존 없음 |
| **(C) SpriteMask** | 단순. 다만 적/투사체 가시성 처리 까다로움 |

→ 프로젝트의 URP 사용 여부 확인 후 Day 36 시작 전 결정.

### 2-2. 자폭 적 시스템

심해해파리(3스테이지)와 향후 연구소장 드론 패턴에서 공통 사용.

- 새 컴포넌트 `EnemySuicideExplode`
- 일정 거리 안에 들어오면 **경고 모션** (색상 점멸) → 1초 후 **폭발** → 광범위 피해 + 자기 사망
- 인스펙터 노출: `triggerRange`, `warningDuration`, `explosionRadius`, `damage`

### 2-3. 직선 레이저 시스템

4스테이지 원거리 전투요원 + 연구소장 공통 사용.

- 옵션 A: `EnemyRangedAttack`의 변형 — 즉발 라인 발사 + Raycast/OverlapBox
- 옵션 B: 새 컴포넌트 `EnemyLaserAttack`
- 권장: B (즉발 vs 투사체 발사 방식이 달라 별도 컴포넌트가 깔끔)
- 시각: LineRenderer로 짧은 시간 라인 표시 (조준 단계 + 발사 단계)

### 2-4. 메타 업그레이드 항목

디자인 문서 §9 메타 업그레이드 기준 + 단계별 권장값.

| 항목 | 단계별 효과 | 단계별 비용 (세포) |
|------|------------|------------------|
| 최대 HP | +10 / +20 / +30 / +40 | 5 / 15 / 35 / 75 |
| 이동속도 | +5% / +10% / +15% / +20% | 5 / 15 / 35 / 75 |
| 공격속도 | +5% / +10% / +15% / +20% | 5 / 15 / 35 / 75 |
| 공격력 | +5% / +10% / +15% / +20% | 5 / 15 / 35 / 75 |
| 압력 저항 | +10% / +20% / +30% / +40% / +50% | 10 / 25 / 50 / 100 / 200 |

- 세이브: `PlayerPrefs` (간단) 또는 JSON 파일. **권장: PlayerPrefs** (6주차 단순화)
- 메타 업그레이드 화면에서만 세포 사용. 인게임 PlayerProgressData의 세포 카운터와 별개로 누적/소비.

### 2-5. 엔딩 화면

- 단순 VictoryPanel (같은 씬, 별도 패널 — 4스테이지 보스 처치 후 표시)
- 표시 내용: "EXPERIMENT COMPLETE" + 클리어 시간 + 누적 세포 + 재시작 버튼
- 재시작 버튼 → 메인 메뉴 또는 1스테이지 새 게임

### 2-6. 메인 메뉴

빌드 가능한 상태가 되려면 진입점인 메인 메뉴가 필요.

- 옵션 A (권장): 6주차 안에 간단히 추가 (Day 41 또는 42에 끼워넣기). 시작 / 메타 업그레이드 / 종료 3버튼
- 옵션 B: 메타 업그레이드 화면 = 메인 메뉴 통합 (Day 40에 처리)
- 옵션 C: 7주차로 미루기 (메인 메뉴 없이 1스테이지 직접 시작)

권장: B 또는 A. Day 40 메타 업그레이드 작업과 묶어서 처리하면 효율적.

### 2-7. 단순화 원칙

각 작업에서 디자인 문서의 풀 사양이 부담스러울 때 7주차로 미룬다:

- **산갈치 마디 판정** — 디자인 문서: "매우 긺, 마디마디 발광효과". 6주차에는 단순 큰 적 + 돌진 + 회전 공격으로 시작. 마디 판정은 7주차.
- **연구소장 드론 소환** — 7주차로 미룸. 6주차에는 추적+레이저+페이즈2만.
- **연구소장 보호막** — 7주차로 미룸.

## 3. 디자인 가이드

### 3-1. 3스테이지 — 심해

| 항목 | 내용 |
|------|------|
| 분위기 | 어둠, 압박감, 시야 제한 |
| 적 종류 | 심해아귀(기본), 대왕오징어(빠름), 심해해파리(자폭), 풍선장어(원거리) |
| 보스 | 산갈치 |
| 환경 | 시야 제한 + 강한 압력 (-35%, +50%) |
| 시간 | stageDuration 360s, bossSpawnTime 300s (StageData 기존 값) |

### 3-2. 4스테이지 — 파괴된 연구소

| 항목 | 내용 |
|------|------|
| 분위기 | 복수, 파괴, 대량 적 |
| 적 종류 | 강화 방패(돌진), 원거리(레이저), 강화 근접(빠름+강함) |
| 보스 | 연구소장 (마지막) |
| 환경 | 압력 없음 |
| 카피 스킬 | 없음 (마지막 스테이지) |

### 3-3. 산갈치 보스 (3스테이지)

디자인 문서: "매우 긺, 마디마디 발광효과, 돌진공격, 휘두르는 공격"

**6주차 단순 구현**:
- 큰 단일 적 (보스 sprite 크기 ×3)
- 추적 + 돌진 (BossAI 재사용)
- 추가 패턴: 휘두르는 공격 = 부채꼴 광역 (Slash 구조 응용)
- 페이즈2: 이동속도 + 돌진 빈도 증가

### 3-4. 연구소장 보스 (4스테이지, 마지막)

디자인 문서: "인간+기계강화, 페이즈 변화 예정", 패턴: 드론 소환 + 레이저 + 보호막

**6주차 단순 구현**:
- 추적 + 접촉 피해
- 페이즈2 진입 시 레이저 발사 패턴 추가
- 7주차에 드론 소환/보호막 추가

## 4. Day 35 - 3스테이지 기본 구성 ✅ (2026-05-17 완료)

### 4-1. 씬 생성

- [x] `Stage3_Deep.unity` 씬 생성 (Stage2 복사). DontDestroyOnLoad 매니저는 Stage2 구조 그대로 유지 (싱글톤 중복 검사 자체 처리)
- [x] BuildSettings에 등록 (인덱스 2)
- [x] 카메라 배경색을 짙은 남색 `(0.02, 0.04, 0.1, 1)`로 변경 (심해 분위기)

### 4-2. 자폭 시스템 신규

- [x] `EnemySuicideExplode` 컴포넌트 생성 (`Assets/Scripts/Enemy/EnemySuicideExplode.cs`)
  - [x] triggerRange (자폭 발동 거리)
  - [x] warningDuration (1초 점멸)
  - [x] explosionRadius (폭발 반경)
  - [x] damage
- [x] 색상 점멸 (originalColor ↔ warningColor 빨강)로 경고 모션
- [x] 폭발 시 OverlapCircleAll로 PlayerStats 피해 + 자기 사망 (SetActive(false))

### 4-3. 3스테이지 적 데이터/프리팹

- [x] `EnemyData_Anglerfish.asset` (심해아귀 — HP35/이속2.2)
- [x] `EnemyData_GiantSquid.asset` (대왕오징어 — HP50/이속3.6, 빠름+높은 체력)
- [x] `EnemyData_Jellyfish.asset` (심해해파리 — HP15/이속2.6, 접촉피해0/자폭전용)
- [x] 풍선장어 자산은 Day 32에서 만든 것 재활용
- [x] 4종 prefab 색상 차별화 (Anglerfish 어두운 보라, GiantSquid 진한 자주, Jellyfish 연한 분홍 반투명)

### 4-4. EnemySpawner 웨이브 설정

- [x] Wave 0: 심해아귀 (0s, interval 2s, pool 25)
- [x] Wave 1: 대왕오징어 (60s, interval 3.5s, pool 12)
- [x] Wave 2: 심해해파리 (자폭, 90s, interval 5s, pool 10)
- [x] Wave 3: 풍선장어 (원거리, 120s, interval 4s, pool 10)

### 4-5. 검증

- [x] 3스테이지 진입 시 압력 자동 활성화 (Stage3_Deep StageData에 따라)
- [x] 적 4종 모두 정상 동작 (Play 모드 스폰 확인)
- [x] 심해해파리가 가까이 가면 경고 모션 후 자폭, 광범위 피해

## 5. Day 36 - 3스테이지 시야 제한 ✅ (2026-05-17 완료)

사전 결정 §2-1: **(A) URP 2D Light** 채택 (프로젝트가 이미 URP 14.0.12 + Renderer2D 사용).

### 5-1. 시야 제한 시스템 구현

- [x] 옵션 A (URP 2D Light) 채택
- [x] `[Map]/GlobalLight2D_Dark`: Light2D type=Global, intensity 0.1, color (0.4, 0.5, 0.7) — 짙은 청색 어둠
- [x] `[Player]/Player/VisionLight2D`: Light2D type=Point, intensity 1.0, innerRadius 3 / outerRadius 6, falloffIntensity 0.6, 따뜻한 화이트
- [x] Point Light가 Player 자식이므로 카메라/플레이어 이동에 자동 추적

### 5-2. 적/투사체 가시성 조정

- [x] 모든 sprite 머티리얼이 `Sprite-Lit-Default` (URP 2D) → Light2D에 자동 반응. 시야 밖 적은 어두워지지만 충돌/AI는 정상 동작
- [x] 별도 sorting layer/order 조정 불필요 (URP Renderer2D가 자동 처리)
- [x] EnemyProjectile도 동일 머티리얼 사용으로 일관 처리

### 5-3. 시야 반경 조정 옵션

- [ ] 압력 저항 메타와 시야 반경 연동 → **Day 40 메타 시스템 작업 시 함께 처리** (현재 미적용)
- [x] 인스펙터에서 `VisionLight2D`의 outerRadius/innerRadius 조정 가능 (Light2D 컴포넌트 직렬화 필드)

### 5-4. 검증

- [x] 3스테이지에서 시야가 제한된 상태로 게임 진행 (Play 모드 확인)
- [x] Player가 prefab 인스턴스가 아닌 씬 로컬 GameObject → Stage3에만 VisionLight2D 적용, Stage1/2 영향 없음
- [x] 적이 시야 밖에서 다가오는 긴장감 체감 (사용자 확인)

## 6. Day 37 - 산갈치 보스 + Space 슬롯 카피 스킬 ✅ (2026-05-18 완료)

### 6-1. CopySkillID 확장

- [x] `CopySkillID` enum에 VoidPierce(7), GlowFrenzy(8), BleedSwim(9) 추가 (enum 끝에만 추가, 직렬화 호환 유지)
- [x] `CopySkillManager.FindSkillComponent()` + `CopySkillSelectCardUI.FindSkillComponent()` 3종 케이스 추가

### 6-2. 산갈치 보스

- [x] `BossData_Oarfish.asset` 생성 (HP 1300 / 이속 2.1 / 접촉 22 / 페이즈2 이속 ×1.7 / 돌진 16 속도·0.45s)
- [x] `Boss_Oarfish.prefab` 생성 (Boss_Whale 복제 → BossData 교체)
- [x] 큰 크기 (Transform.localScale 3.0), 발광 청록 색상 (0.4, 0.95, 0.95)
- [x] BossAI 기본 추적 + 돌진 활용 (Whale 컴포넌트 그대로)
- [x] **휘두르는 공격** = `BossSwingAttack` 컴포넌트 신규 (Slash/Ultrasonic의 부채꼴 판정 응용)
  - 조준 단계 0.7s (LineRenderer 부채꼴 표시, 주황 경고)
  - 공격 단계 즉발 OverlapCircle + 각도 필터 (100° 부채꼴, range 5)
  - 페이즈2 진입 시 attackInterval × 0.6 (자주 발동)
- [x] 페이즈2 진입 시 돌진 빈도 ↑ (BossAI EnterPhase2 기존 로직 + phase2SpeedMultiplier 1.7)
- [ ] 마디 발광은 7주차로 미룸 (단순화)

### 6-3. 3스테이지 카피 스킬

- [x] `CopySkill_VoidPierce.asset` (공허 관통 — 직선 다단 히트, 100E, ID=7)
- [x] `CopySkill_GlowFrenzy.asset` (발광 폭주 — 발광 노드 8개, 폭발 + 넉백, 120E, ID=8)
- [x] `CopySkill_BleedSwim.asset` (절단 유영 — 고속 이동 + 궤적 출혈, 130E, ID=9)
- [x] BossData_Oarfish.copySkillOptions에 3종 등록, copySkillSlot=2 (Space)

### 6-4. 스킬 컴포넌트 구현

- [x] `VoidPierceSkill` — Physics2D.BoxCastAll로 직선 다단 히트 (length 9, width 1.2, 4틱)
- [x] `GlowFrenzySkill` — 8개 노드를 원형 배치. 보조 `GlowFrenzyNode` 컴포넌트: 트리거 닿으면 OverlapCircle 폭발 + Rigidbody2D 넉백
- [x] `BleedSwimSkill` — speedMultiplier 2.5로 4s 가속. 보조 `BleedTrailNode` 컴포넌트: 동적 sprite + tick 데미지
- [x] Stage3 Player에 3종 컴포넌트 부착 + enemyLayer=Enemy(6) 설정
- [ ] Stage4 Player에는 Day 38 씬 생성 시 함께 부착

### 6-5. Stage3 BossSpawner 설정

- [x] BossSpawner.bossPrefab = Boss_Oarfish

### 6-6. 검증

- [x] 컴파일 클린 (CS 에러 없음)
- [x] Boss_Oarfish prefab 컴포넌트 구조 정상 (BossBase / BossAI / HitEffect / BossPhaseEffect / BossSwingAttack)
- [x] 인스펙터에서 BossData 참조 / 카피 스킬 옵션 3종 / copySkillSlot=2 확인
- [x] 산갈치 등장 + 휘두르기 부채꼴 회피 가능 확인 (사용자 검증, 2026-05-18)
- [x] **버그 픽스**: Slot_Q/Slot_Space의 TMP_Text 폰트가 LiberationSans → 한글 깨짐. Stage1/2/3 모든 슬롯의 텍스트(키/이름/비용)를 `NotoSansKR-Regular SDF`로 일괄 교체 (총 25개 컴포넌트)
- [ ] **사용자 피드백**: 카피 스킬 누적으로 산갈치 패턴 발현 전에 사망. → §10-3 1차 밸런싱에 반영
- [ ] **사용자 피드백**: BleedSwim 동작이 의도와 다름. → §11 "BleedSwim 재설계" 7주차로 이월

## 7. Day 38 - 4스테이지 기본 구성 + 레이저 시스템 ✅ (2026-05-18 완료)

### 7-1. 씬 생성

- [x] `Stage4_Ruined.unity` 씬 생성 (Stage1_Lab 복사 — 둘 다 압력 비활성)
- [x] BuildSettings에 등록 (인덱스 3)
- [x] 카메라 배경색을 짙은 적/회색 `(0.08, 0.04, 0.04, 1)`으로 변경 (파괴된 연구소 분위기)

### 7-2. 직선 레이저 시스템 (사전 결정 §2-3 (B))

- [x] `EnemyLaserAttack` 컴포넌트 생성 (`Assets/Scripts/Enemy/EnemyLaserAttack.cs`)
- [x] 흐름: 조준 단계 1s (LineRenderer 얇은 빨간 선) → 발사 단계 (OverlapBoxAll 즉발 + 두꺼운 흰 선 잔상 0.1s)
- [x] 사거리(attackRange) / 데미지 / attackInterval / aimDuration / beamWidth 인스펙터 노출
- [x] StopDistance public 속성으로 EnemyAI가 추적 거리 유지

### 7-3. 4스테이지 적 데이터/프리팹

- [x] `EnemyData_ReinforcedGuard.asset` (HP 70 / 이속 2.4 / 접촉 12) — 1스 Guard 강화
- [x] `EnemyData_LaserSoldier.asset` (HP 30 / 이속 1.6 / 접촉 5) — 원거리, 레이저
- [x] `EnemyData_ReinforcedSoldier.asset` (HP 35 / 이속 3.4 / 접촉 10) — 빠른 강한 근접
- [x] 3종 prefab 색상 차별화 (Guard 진한 적, Soldier 밝은 적, LaserSoldier 노란빛)
- [x] Enemy_LaserSoldier에서 EnemyRangedAttack 제거 + EnemyLaserAttack 부착

### 7-4. EnemySpawner 웨이브 설정

- [x] Wave 0: 강화 근접 (0s, interval 1.5s, pool 30) — 많이
- [x] Wave 1: 강화 방패 (45s, interval 4.5s, pool 12) — 적게
- [x] Wave 2: 원거리 (90s, interval 6s, pool 8) — 너무 많으면 답답하지 않게 적게

### 7-5. 추가 작업

- [x] Stage4 Player에 VoidPierce/GlowFrenzy/BleedSwim 카피 스킬 3종 부착 + enemyLayer=Enemy(6) (Day 37 spec 후속)

### 7-6. 검증

- [x] 컴파일 클린
- [x] Play 모드 진입 시 Wave 0 (강화 근접) 정상 스폰 확인
- [x] Stage4 압력 비활성 (StageData pressureEnabled=0 확인)
- [ ] 레이저 적 조준→발사 실제 동작 (Wave 1·2 등장 후) — 사용자 검증 필요
- [ ] 강화 적들의 차별화된 위협 체감 — 사용자 검증 필요

## 8. Day 39 - 연구소장 보스 (페이즈1 + 페이즈2 레이저) ✅ (2026-05-18 완료)

### 8-1. BossData_Director 생성

- [x] `BossData_Director.asset` 생성
- [x] HP 1500 (마지막 보스, 가장 강함)
- [x] 이속 2.5, 접촉 25, 페이즈2 이속 ×1.4
- [x] 페이즈2 비율 0.5
- [x] **copySkillOptions = []** (빈 배열 — 마지막 보스, 카피 스킬 선택 없음)

### 8-2. Boss_Director.prefab

- [x] `Boss_Director.prefab` 생성 (Boss_ExperimentalSubjects 복제 → 데이터 교체)
- [x] 시각 차별화: 회색+짙은 빨강 `(0.55, 0.18, 0.18)`
- [x] BossBase, BossAI, HitEffect, BossPhaseEffect 부착 (복제 시 자동 포함)

### 8-3. 레이저 패턴 (페이즈1부터 발동, 페이즈2 위협도 증가)

- [x] `BossDirectorAttack` 컴포넌트 신규 (`Assets/Scripts/Enemy/BossDirectorAttack.cs`)
- [x] **사용자 피드백**: 페이즈1이 너무 단조로움 → 페이즈1부터 레이저 발동으로 변경
  - 스폰 후 initialDelay 2.5s 대기 → 페이즈1 fireInterval 4.5s
  - 페이즈2 진입 시 phase2IntervalMultiplier 0.55 → 약 2.5s 간격으로 위협도 증가
  - 페이즈2 추가는 BossAI 돌진과 시너지 (이중 압박)
- [x] EnemyLaserAttack 패턴 응용 (조준 1s LineRenderer → OverlapBoxAll 즉발 + 잔상 0.15s)
- [x] activeFx 추적 + OnDisable 정리 — 보스 사망 시 라인 잔상 버그 사전 방지
- [x] range 14 / beamWidth 0.6 / damage 30 (보스급 데미지)

### 8-4. 마지막 보스 카피 스킬 없음 처리

- [x] `BossSpawner.OnBossDied`에서 `boss.Data.copySkillOptions`가 null/빈 배열이면 CopySkillSelectPanel 띄우지 않고 `GameManager.TriggerStageClear()` + `StageManager.TransitionToNext()` 호출
- [x] 흐름 검증: Stage4 `nextStage = null` → `TransitionToNext` 안의 `isEnding` 분기 진입 → 현재는 "Ending" 씬 시도 (Day 40에서 VictoryPanel.Show로 교체 예정)

### 8-5. Stage4 BossSpawner 설정

- [x] BossSpawner.bossPrefab = Boss_Director

### 8-6. 검증

- [x] BossSpawner 강제 호출 → Boss_Director 인스턴스화 정상 (HP 1500, copySkillOptions 길이=0 확인)
- [x] 보스 즉사 후 GameManager 상태 = `StageClear` 확인 (분기 정상 동작)
- [ ] 페이즈2 레이저 실제 발사 동작 / 일반 5분 플레이 시 보스 등장 + 50% HP에서 레이저 — 사용자 검증 필요

## 9. Day 40 - 메타 업그레이드 시스템 + 엔딩 + 메인 메뉴 ✅ (2026-05-18 완료)

### 9-1. PlayerPrefs 기반 메타 데이터

- [x] `MetaProgressData` 정적 클래스 (`Assets/Scripts/Systems/MetaProgressData.cs`)
- [x] PlayerPrefs 키 6종: `Meta_TotalCells`, `Meta_MaxHpLevel`, `Meta_MoveSpeedLevel`, `Meta_AttackSpeedLevel`, `Meta_AttackPowerLevel`, `Meta_PressureResistanceLevel`
- [x] `TotalCells` 프로퍼티, `AddCells`, `GetLevel`, `TryPurchase`, `GetNextCost`, `GetMaxHpBonus`/`GetMoveSpeedMultiplier`/`GetAttackSpeedMultiplier`/`GetAttackPowerMultiplier`/`GetPressureResistanceBonus`, `ResetAll`

### 9-2. 세포 인게임 → 메타로 이동

- [x] `GameManager.AccumulateRunCellsToMeta()` 신규 — `LevelManager.CurrentCells` → `MetaProgressData.AddCells` + `LevelManager.ConsumeAllCells()` (중복 누적 방지)
- [x] GameState.GameOver 진입 시 자동 호출
- [x] 4스 엔딩 시 `VictoryPanel.Show()`에서 호출 (UI 표시 전 캡처 → 누적)
- [x] `LevelManager.ConsumeAllCells()` 메서드 신규 추가

### 9-3. 게임 시작 시 메타 스탯 적용

- [x] `PlayerStats.Awake`에서 메타 보너스 적용:
  - `maxHp += GetMaxHpBonus()` (+10 / +20 / +30 / +40)
  - `moveSpeed *= GetMoveSpeedMultiplier()` (×1.05/×1.10/×1.15/×1.20)
  - `attackSpeed *= GetAttackSpeedMultiplier()`
  - `attackPower *= GetAttackPowerMultiplier()`
  - `pressureResistance += GetPressureResistanceBonus()` (+0.1/+0.2/+0.3/+0.4)

### 9-4. 메인 메뉴 + 메타 업그레이드 UI

- [x] `MainMenu.unity` 씬 신규 생성 (`Assets/Scenes/MainMenu.unity`)
- [x] BuildSettings 재정렬: MainMenu(0) → Stage1(1) → Stage2(2) → Stage3(3) → Stage4(4)
- [x] 메인 패널: START / META UPGRADE / QUIT 3버튼
- [x] 메타 업그레이드 패널: 5종 스탯 행 (스탯명/Lv/효과/비용/구매 버튼) + 누적 세포 표시 + 메인 메뉴 복귀
- [x] 스크립트: `MainMenuController.cs`, `MetaUpgradePanel.cs`, `MetaUpgradeRow.cs`
- [x] UI 구성은 일회성 `Assets/Scripts/Editor/MainMenuBuilder.cs`로 자동 생성 (NotoSansKR-Regular SDF 사용)

### 9-5. VictoryPanel (엔딩 패널)

- [x] `VictoryPanel.cs` 신규 생성 (`Assets/Scripts/UI/VictoryPanel.cs`)
- [x] Stage4_Ruined Canvas에 패널 추가 (코드로 동적 생성)
- [x] 표시 내용:
  - "EXPERIMENT COMPLETE" 타이틀
  - 클리어 시간 (마지막 스테이지 `Time.timeSinceLevelLoad` — 7주차에 스테이지별 합산 가능)
  - 획득 세포 (이번 런)
  - 누적 세포 (메타에 적립 후)
  - "메인 메뉴로" 버튼
- [x] DontDestroyOnLoad 매니저 6종 정리 후 MainMenu 씬 로드 (클린 상태 보장)
- [x] **픽스 (사용자 피드백)**: 씬 저장 시 비활성 상태로 두면 Awake가 호출 안 되어 `VictoryPanel.Instance`가 null. 패널은 **활성 상태로 저장**, `Start()`가 자동 숨김 처리하도록 패턴 통일

### 9-6. 흐름 연결

- [x] `StageManager.LoadEnding` → `VictoryPanel.Instance.Show()` 호출
- [x] `StageManager.TransitionToNext`의 isEnding 분기 → `LoadEnding()` 호출 (기존 "Ending" 씬 로드 코드 제거)
- [x] `VictoryPanel` "메인 메뉴로" → 매니저 정리 + `SceneManager.LoadScene("MainMenu")`
- [x] 메인 메뉴 START → `PlayerProgressData.ResetAll()` + Stage1_Lab 로드

### 9-7. 검증

- [x] 컴파일 클린 (스크립트 5개 신규, 4개 수정)
- [x] MainMenu 씬 Canvas/MainPanel/MetaPanel/Controller 구조 정상 확인 (런타임 inspection)
- [ ] 1회 풀 사이클 (메인 메뉴 → 1~4스 → 엔딩 → 메인 메뉴 → 메타 구매 → 재플레이) — 사용자 검증 필요

## 10. Day 41 - 6주차 통합 테스트 + 1차 밸런싱 ✅ (2026-05-19 완료)

### 10-1. 전체 흐름 테스트

- [x] 메인 메뉴 → 1스테이지 → 2스테이지 → 3스테이지 → 4스테이지 → 엔딩 → 메인 메뉴 (사용자 풀 사이클 확인)
- [x] 각 스테이지 적/보스 동작 정상
- [x] 카피 슬롯 Q/E/Space 모두 동작
- [x] 메타 업그레이드 적용 후 다시 게임 시작 → 스탯 증가 체감

### 10-2. 환경 검증

- [x] StageData 확인: 1·4 압력 비활성(0), 2·3 활성(1)
- [x] 3스테이지 시야 제한 동작 (Day 36 검증)
- [x] 콘솔 컴파일 에러 없음
- [x] DontDestroyOnLoad 매니저는 VictoryPanel.CleanupPersistentManagers에서 6종 정리

### 10-3. 1차 밸런싱

전체 컨텐츠가 완성되었으므로 한 번 일괄 조정.

**보스 HP +30~33% (★ Day 37 피드백 반영)** — 카피 스킬 누적 강도 대응:
- [x] Whale: 1000 → **1300** HP
- [x] Oarfish: 1300 → **1700** HP
- [x] Director: 1500 → **2000** HP
- 카피 에너지 비용 / 페이즈1 최소 지속 시간은 7주차 외부 피드백 후 2차 조정

**7주차 2차 조정 대기 항목** (현 단계에서는 데이터 부족):
- [ ] 1스 보스 처치까지 약 5~6분 (시간 측정 필요)
- [ ] 2스 압력 체감 (-20%가 답답하면 -15%로)
- [ ] 3스 시야 + 압력 체감 (시야 너무 좁으면 반경 ↑)
- [ ] 4스 적 밀도 (Wave 0 interval 1.5s가 과한지)
- [ ] 메타 업그레이드 비용 (5세포가 너무 싸면 10~15로)
- [ ] 만렙 보상 카드 비율 (현재 회복 30% / 세포 +5)

### 10-4. 마무리

- [x] 테스트 토글 일괄 정리:
  - Stage1 BossSpawner.overrideStageData true→false
  - Stage1·2·3·4 DifficultyManager.overrideStageData true→false
  - Stage2·3·4 BossSpawner는 이미 false (Day 38·39 작업 후 복귀됨)
- [x] 인스펙터 테스트 값 정상화: `EnemyData_Default.expAmount` 2000 → 8
- [x] 컴파일 에러/경고 없음 확인
- [x] **첫 빌드 성공** ✅ (Windows Standalone)
  - 출력: `Build/Windows/Project_Abyss.exe` (228.28 MB)
  - 빌드 시간: 226초
  - Job ID: build-0c7d564c12
  - Development build (디버그 심볼 포함)
- [ ] 6주차 작업 커밋 — 사용자 시점에서 수행

### 10-5. 6주차 목표 달성 ✅

- ✅ Stage1~4 모두 플레이 가능
- ✅ 카피 스킬 슬롯 Q/E/Space 모두 활용
- ✅ 엔딩 → 메인 메뉴 → 새 게임 사이클
- ✅ 메타 업그레이드로 영구 진행 동기 부여
- ✅ 첫 외부 빌드 가능 (Windows Standalone)

## 11. 7주차로 미루는 작업

다음 항목은 6주차에서 의도적으로 제외했다.

### 디자인 보강
- **스킬 풀 확장** — 공격 스킬 2~3종 (음파 진동, 가시 발사, 흡혈 촉수), 패시브 1~2종 (압력 적응, 발광 감각)
- **BleedSwim(절단 유영) 재설계** — Day 37에서 "가속+출혈 트레일"로 구현했으나 사용자 의도와 다름. 스킬 풀 확장 시 함께 재작업
- **향유고래 풀 패턴** — 초음파 광역 + 페이즈2 추가 패턴
- **산갈치 마디 판정** — 다중 collider로 마디별 hit 처리, 마디별 발광 효과
- **연구소장 드론 소환** — 자폭 드론 패턴
- **연구소장 보호막** — 페이즈3 또는 특정 HP 구간

### 시스템 정리
- **매니저 prefab 변환** — 각 스테이지 씬 독립 테스트 편의성
- **본격 밸런싱 2차** — 첫 빌드 외부 피드백 받은 후 2차 조정
- **사운드 시스템** — BGM, SFX (5~7주차에 미정)
- **이펙트 강화** — 보스 페이즈 전환, 카피 스킬 발동 등에 파티클

### 폴리싱
- **메타 업그레이드 항목 확장** — 스킬 해금 시스템 (디자인 문서 §9 후반)
- **UI 폴리싱** — 메뉴/패널 디자인 정리, 폰트/아이콘
- **튜토리얼** — 첫 플레이 시 기본 조작 안내

## 12. 주의할 점

### 작업 순서 의존성

- Day 35 (3스테이지 적) → Day 36 (시야 제한, 3스테이지에만 적용)
- Day 38 (레이저 시스템) → Day 39 (연구소장 페이즈2가 레이저 패턴 사용)
- Day 40 (메타 시스템) → Day 41 (통합 테스트에서 메타 적용 검증)

### 마지막 스테이지 흐름

`Stage4_Ruined.nextStage = null`로 두면 `StageManager.TransitionToNext`가 `IsFinalStage` 분기로 `LoadEnding` 호출. 5주차에 만든 분기 흐름 그대로 활용.

`BossSpawner.OnBossDied`에서 `BossData.copySkillOptions`가 비어있으면 CopySkillSelectPanel을 띄우지 않고 직접 TriggerStageClear + TransitionToNext 호출하도록 수정 필요.

### 메타 데이터 영구 저장

PlayerPrefs는 단순하지만 변조가 쉽다. 빌드 후 사용자가 직접 PlayerPrefs를 수정하면 메타 값이 변할 수 있다. 6주차에는 신경 쓰지 않고 7주차 이후 JSON + 해시로 검증 고려.

### 시야 제한 + 압력 동시 적용

3스테이지는 시야 제한 + 강한 압력(-35%)이라 답답할 가능성이 큼. Day 41 통합 테스트에서 답답함을 체크하고, 너무하면:
- 시야 반경 ↑ 또는
- 압력 강도 약간 ↓ (Stage3_Deep.asset 인스펙터 수정)

### 자폭 적 / 레이저 적의 페어플레이

- 자폭 적은 반드시 **경고 모션** 후 폭발 (1초). 시각적으로 회피 가능해야 함
- 레이저 적은 반드시 **조준 라인** 표시 후 발사 (1초). 회피 시간 확보
- 두 경고 모두 색상 점멸 또는 LineRenderer로 명확하게

### 메타 업그레이드 적용 시점

`PlayerStats.Awake`에서 메타 적용. 이때 인스펙터 기본값에 더한다.

```csharp
maxHp += MetaProgressData.GetMaxHpBonus();
moveSpeed *= MetaProgressData.GetMoveSpeedMultiplier();
```

PlayerProgressData.Restore 시점에는 stats가 이미 메타 적용된 상태이므로, currentHp Clamp 범위가 자동으로 메타-증가된 maxHp까지 확장됨.

### 빌드 가능 상태의 의미

6주차 끝(Day 41) 시점에:
- Stage1 ~ Stage4 모두 플레이 가능
- 카피 스킬 슬롯 Q/E/Space 모두 활용
- 엔딩 → 메인 메뉴 → 새 게임 사이클
- 메타 업그레이드로 영구 진행 동기 부여
- 첫 외부 빌드 가능 (Windows Standalone)

이게 6주차의 명확한 결과물이다. 미완 요소(스킬 풀, 보스 풀 패턴)는 7주차 이후 폴리싱.
