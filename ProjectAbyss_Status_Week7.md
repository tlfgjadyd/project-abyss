# Project: Abyss - 7주차 TODO

작성일: 2026-05-19 (1차 빌드 + 외부 피드백 반영 후 재구성)

이 문서는 **새 세션에서 컨텍스트 없이 봐도 작업 시작이 가능**하도록 작성됨. 파일 경로 / 수치 before-after / 구현 위치를 명시. 모든 작업은 6주차 완료 상태(Stage1~4 + 메뉴 + 메타 + 첫 빌드 완료)에서 시작.

---

## 0. 새 세션 cold-start 가이드

### 0-1. 현재 프로젝트 상태 (6주차 완료 시점, 2026-05-19)
- **Stages 모두 플레이 가능**: Stage1_Lab (실험실, 압력 X) → Stage2_Sea (바다, 압력 O) → Stage3_Deep (심해, 압력 + 시야 제한) → Stage4_Ruined (연구소, 압력 X)
- **메인 메뉴 + 엔딩 + 메타 사이클**: `MainMenu` 씬 → Stage 1~4 → 연구소장 처치 → `VictoryPanel` → 메인 메뉴 복귀 → 메타 업그레이드 5종 (HP/이속/공속/공격력/압력저항) → 재시작
- **카피 스킬 슬롯**: Q(1스 보스 카피), E(2스), Space(3스). 4스는 마지막 보스라 카피 보상 없음
- **첫 Windows Standalone 빌드 완료**: `Build/Windows/Project_Abyss.exe` (228 MB, 226초 소요)

### 0-2. 주요 파일/폴더 빠른 참조

| 분류 | 경로 |
|------|------|
| 스테이지 데이터 | `Assets/ScriptableObjects/StageData/Stage{1-4}_*.asset` |
| 적 데이터 | `Assets/ScriptableObjects/EnemyData/EnemyData_*.asset` |
| 보스 데이터 | `Assets/ScriptableObjects/BossData/Boss_*.asset` |
| 카피 스킬 데이터 | `Assets/ScriptableObjects/CopySkillData/CopySkill_*.asset` |
| 일반 스킬 데이터 | `Assets/ScriptableObjects/SkillData/` 안 (Slash/PoisonNeedle 등) |
| 적 prefab | `Assets/Prefabs/Enemies/Enemy_*.prefab` |
| 보스 prefab | `Assets/Prefabs/Bosses/Boss_*.prefab` |
| 씬 | `Assets/Scenes/MainMenu.unity`, `Stage{1-4}_*.unity` |
| 폰트 | `Assets/Fonts/NotoSansKR-Regular SDF.asset` (한글 깨지면 이걸로) |
| 플레이어 스탯 | `Assets/Scripts/Player/PlayerStats.cs` (Awake에 메타 적용) |
| 플레이어 컨트롤 | `Assets/Scripts/Player/PlayerController.cs` (`FaceDirection`) |
| 게임 매니저 | `Assets/Scripts/Systems/GameManager.cs` (state, timer, PauseGame 가능성) |
| 레벨 매니저 | `Assets/Scripts/Systems/LevelManager.cs` (`CurrentCells`, `cellsPerSelection`) |
| 메타 데이터 | `Assets/Scripts/Systems/MetaProgressData.cs` (PlayerPrefs 정적 클래스) |
| 카피 스킬 베이스 | `Assets/Scripts/Skills/CopySkillBase.cs` |
| 카피 스킬 컴포넌트 | `Assets/Scripts/Skills/{Berserk,Dash,HealingFactor,Ultrasonic,DeepPressure,PredatorCharge,VoidPierce,GlowFrenzy,BleedSwim}Skill.cs` |
| 적 베이스 | `Assets/Scripts/Enemy/EnemyBase.cs` |
| 보스 베이스 | `Assets/Scripts/Enemy/BossBase.cs` (OnPhase2Entered 이벤트) |
| 적 스폰 | `Assets/Scripts/Systems/EnemySpawner.cs` (Wave[] 배열) |
| 보스 스폰 | `Assets/Scripts/Systems/BossSpawner.cs` |
| 돌연변이 | `Assets/Scripts/Systems/MutationManager.cs` (Overload, OvgrownTentacle, MimicryOrgan) |
| 시야 제한 (Stage3 한정) | `[Map]/GlobalLight2D_Dark`, `[Player]/Player/VisionLight2D` (URP 2D Light) |
| 빌드 산출물 | `Build/Windows/Project_Abyss.exe` |
| 피드백 원문 | `Summary/feedback_memo.md` |

### 0-3. 작업 우선순위
1. **Day 42 (빌드 버그 + 1차 밸런싱)** — 절대 먼저. 이게 안정돼야 다른 작업 검증 가능
2. **Day 43~44 (UI 픽스 + 시야 강화 + VoidPierce)** — 피드백 직접 대응
3. **Day 45~46 (스킬 풀 확장 + 보스 패턴)** — 6주차에서 미룬 콘텐츠
4. **Day 47~48 (매니저 prefab + 사운드 + 이펙트)** — 시스템 정리/폴리싱
5. **Day 49 (UI 폴리싱 잔존 + 2차 빌드)**

### 0-4. 작업 시 표준 절차
- 스크립트 수정: `Assets/Scripts/...` 직접 편집 → Unity 자동 컴파일 대기
- 씬 수정: MCP `manage_scene` load → 수정 → save
- prefab 수정: MCP `manage_prefabs modify_contents` 사용 (component_properties로 직렬화 필드 일괄 변경)
- 한글 텍스트 깨짐: `NotoSansKR-Regular SDF` 폰트 강제 적용 (Day 37·43 픽스 패턴 참고)

---

## 1. 7주차 목표

6주차에서 게임의 큰 골격(1~4스테이지 + 메타 + 엔딩 + 첫 빌드)을 완성했다. 7주차의 핵심 목표는 다음 셋이다.

1. **외부 피드백 직접 대응** — 빌드 버그 + 밸런스 + UI 가독성 + 컨셉 미활용 문제 해결
2. **6주차에서 미룬 콘텐츠 보강** — 스킬 풀 확장, 보스 패턴 보강, 산갈치 마디, 연구소장 드론/보호막
3. **시스템 정리 + 폴리싱** — 매니저 prefab 변환, 사운드 시스템 기초, 이펙트 강화, UI 폴리싱

### 6주차 대비 차이
- 6주차: **새 콘텐츠 추가 + 빌드 가능 상태 도달** (개발 중심)
- 7주차: **이미 있는 시스템 보강 + 사용자 경험 개선 + 피드백 대응** (폴리싱 중심)

---

## 2. 외부 피드백 요약 (2026-05-19 수신)

표본 적음 — 신호로만 사용. 자세한 원문은 `Summary/feedback_memo.md`.

### 2-1. 스테이지별 평가

| 스테이지 | 클리어 평균 레벨 | 평가 |
|---------|--------------|------|
| 1 | 22 | 초반 원거리 몹 때문에 도전적 → 후반 너무 쉬워짐 |
| 2 | 37 | 가만히 있어도 클리어 |
| 3 | 45 | 너무 쉬움. 시야 컨셉은 좋음 |
| 4 | 52 | 너무 쉬움. 보스만 6초 정도 버팀 |
| 클리어 시 세포 | 평균 130 | 메타 업그레이드 필요성 못 느낌 |

### 2-2. 도출된 문제 (Day 42~44에서 대응)

1. **밸런스 망가짐**: 후반으로 갈수록 쉬워짐. 1스에서 만렙 가까이 도달 (28레벨 + 모든 스킬 4Lv)
2. **빌드 버그**: 1스 원거리 몹 0초 시작 (Day 38 테스트 잔재가 빌드에 그대로 포함)
3. **시스템 사기**: 세포 재생 1Lv도 너무 강함, 독침 4Lv + 감각붕괴 돌연변이 사기
4. **돌연변이 시기**: 2종 모두 1스에서 획득 가능
5. **시야 제한 활용도 낮음**: 원거리 스킬이 시야 밖 적을 죽임. 원거리 적이 시야 밖에서도 잘 보임(초록색)
6. **VoidPierce 평가 "쓰레기"**: 방향 조준 어려움(FaceDirection), 일반 스킬보다 약함
7. **UI 이슈**: 레벨/세포 텍스트 화면 밖 잘림. 만렙 보상 카드 비대칭. HP/EXP/에너지 슬라이더 구분 어려움. 카피 슬롯 폰트 색상
8. **신규 요구**: 일시정지, 기본 공격 시각 효과

---

## 3. 7주차 작업 큰 흐름

| Day | 주제 | 의존 |
|-----|------|------|
| 42 | 빌드 버그 픽스 + 1차 밸런싱 | (시작점) |
| 43 | UI 픽스 + 일시정지 | 42 |
| 44 | 3스 시야 강화 + VoidPierce 재설계 | 42, 43 |
| 45 | 스킬 풀 확장 + BleedSwim 재설계 | 42 |
| 46 | 보스 패턴 보강 (산갈치 마디, 연구소장 드론/보호막, 향유고래) | 45 |
| 47 | 매니저 prefab + 사운드 기초 | (병렬 가능) |
| 48 | 이펙트 강화 (페이즈/카피/사망/카메라/기본공격) | 45, 46 |
| 48b | **2차 밸런싱** (Day 42a~c 후 신규 콘텐츠 반영) | 45, 46 |
| 49 | UI 폴리싱 잔존 + 2차 빌드 | 모두 |

원래 6주차에서 미룬 작업 + 외부 피드백 대응. 총 8일. Day 42가 가장 무거우니 분량 보고 Day 42a/42b로 나눠도 됨.

---

## 4. Day 42 - 빌드 버그 픽스 + 1차 밸런싱

★ 가장 중요. 이거 안 끝나면 다음 작업의 효과 측정 불가.

### 4-1. 빌드 버그: 1스 원거리 적 시작 시간

**문제**: Stage1_Lab의 EnemySpawner Wave 중 원거리 적(Enemy_Shooter)이 0초부터 스폰. Day 38 작업 후 테스트용으로 시간 줄였던 게 빌드에 그대로 포함됨.

- [ ] Stage1_Lab 씬 로드 → `EnemySpawner` GameObject 찾기 → `waves` 배열 확인
- [ ] Enemy_Shooter 사용하는 Wave의 `startTime` 0 → **90s** (1스 진행상 중후반)
- [ ] 다른 스테이지(2/3/4)의 Wave도 확인 — startTime이 의도된 값인지 일괄 점검
  - Stage3: Wave 0 Anglerfish 0s / Wave 1 GiantSquid 60s / Wave 2 Jellyfish 90s / Wave 3 BalloonEel 120s
  - Stage4: Wave 0 ReinforcedSoldier 0s / Wave 1 ReinforcedGuard 45s / Wave 2 LaserSoldier 90s

### 4-2. 세포 곡선 너프 (★ 가장 영향 큼)

**목표**: 클리어 시 세포 평균 130 → **50~65** (절반~1/3). 메타 업그레이드 필요성 회복.

- [ ] 모든 EnemyData의 `energyDrop` **절반**으로
  - 위치: `Assets/ScriptableObjects/EnemyData/*.asset` 각 파일의 `energyDrop:` 줄
  - 일괄 처리 예시 (sed): 현재 5 → 2~3, 7 → 3, 8 → 4
  - 또는 MCP `execute_code`로 `AssetDatabase.LoadAssetAtPath<EnemyData>` 순회하며 `energyDrop *= 0.5f`
- [ ] 만렙 보상 카드 세포 보상 너프
  - 위치: `Assets/Scripts/Systems/LevelManager.cs`의 `cellsPerSelection` 필드 (현재 5)
  - 신규: **2**
  - 또는 매 만렙 보상이 아니라 N회마다 보상으로 변경

### 4-3. 세포 재생 너프

**문제**: 1Lv도 너무 강함 — "거의 맞으면서 해도 OK"

- [ ] 세포 재생 스킬 컴포넌트 찾기 (`Assets/Scripts/Skills/` 검색 `hpRegen` 또는 `세포재생` 또는 `CellRegen`)
- [ ] 레벨별 회복량 너프
  - 추정 현재: 1Lv부터 충분히 회복
  - 신규 안: 1Lv 0.3/s → 2Lv 0.6/s → 3Lv 1.0/s → 4Lv 1.5/s (또는 실제 현재 값의 30~50%)
- [ ] 또는 회복 시작 조건 추가 (예: 3초간 피격 없을 때만 회복)

### 4-4. 독침 너프 + 일반 스킬 데미지 조정

**문제**: 독침 4Lv 사기

- [ ] PoisonNeedle 스킬 (`Assets/Scripts/Skills/PoisonNeedle*.cs`)
  - 4Lv 데미지/발사 횟수/관통 수 중 하나 너프 (-30%)
- [ ] 다른 일반 스킬(Slash, BioticExplosion, ElectricEngine)도 4Lv 시 데미지 -15~20%
  - 또는 SkillData 자산의 레벨별 데미지 배율 조정 (자산이 그렇게 구성돼 있다면)

### 4-5. 돌연변이 획득 시기 조정

**문제**: 2종 모두 1스에서 획득 가능. 너무 빨리 강해짐.

- [ ] `Assets/Scripts/Systems/MutationManager.cs`에서 돌연변이 등장 조건 확인
- [ ] 등장 조건에 레벨 임계값 추가
  - 1번째 돌연변이: 레벨 **15 이상** (현재 추정 10)
  - 2번째 돌연변이: 레벨 **25 이상** OR Stage 2 이상
- [ ] **감각붕괴(Overload) 돌연변이** 효과 너프
  - 현재 효과 확인 후 데미지 배율 -25%
- [ ] **의태기관(MimicryOrgan)** 너프 검토 — 회복 차단 디버프인데 보상이 과한지

### 4-6. 보스 HP 추가 ↑

Day 41 +30% 이후에도 페이즈2 도달 전 처치되는 평가.

- [ ] Boss_Whale: 1300 → **1700** (`Assets/ScriptableObjects/BossData/Boss_Whale.asset` `maxHp`)
- [ ] Boss_Oarfish: 1700 → **2300**
- [ ] Boss_Director: 2000 → **2800**

옵션: 페이즈2 도달 보장 로직 (BossBase 페이즈 전환 조건에 "최소 경과 시간" 추가) — 시간 여유 시.

### 4-7. 검증
- [ ] 빌드 → 풀 사이클 1회 → 다음 기준 충족 확인:
  - 1스 클리어 시 레벨 15~18 (현재 22)
  - 4스 클리어 시 레벨 38~45 (현재 52)
  - 클리어 시 세포 50~65 (현재 130)
- [ ] 메타 풀 만렙 도달까지 5~7회 클리어 필요 정도가 적당

---

## 5. Day 43 - UI 픽스 + 일시정지

### 5-1. HUD 슬라이더 가시성

**피드백**: HP/EXP/에너지 슬라이더 구분 어려움. 간격 좁고 색 비슷함.

- [ ] HUD 슬라이더 색상 분리 (Stage 씬의 `[UI]/Canvas/HUD/` 내부)
  - HP: 빨강 `#E63946`
  - EXP: 파랑 `#3A86FF`
  - 에너지: 초록 `#06D6A0`
- [ ] 슬라이더 간 간격 ↑ (현재 너무 붙어있음 — RectTransform 위치 조정)
- [ ] 라벨 추가 검토 (각 슬라이더 좌측에 아이콘 또는 텍스트)

### 5-2. 레벨/세포 텍스트 화면 밖 잘림

**피드백**: 어떤 해상도에서 텍스트가 화면 밖으로 나감

- [ ] HUD 내 레벨 텍스트 / 세포 텍스트 RectTransform 앵커 재설정
  - 화면 안전 영역(좌상단/우상단 등)에 anchor + pivot 명시
- [ ] CanvasScaler가 `ScaleWithScreenSize` 1920×1080 기준인지 확인 (MainMenu는 이미 그렇게 설정됨)

### 5-3. 만렙 보상 카드 비대칭

**피드백**: 회복 카드와 세포 카드가 좌우 대칭이 아님

- [ ] `Assets/Scripts/UI/MaxLevelRewardPanel.cs` 또는 해당 prefab/UI 확인
- [ ] 카드 2개를 좌우 대칭 배치 (가로 정렬, 같은 크기, 같은 폰트 색)

### 5-4. 레벨업 카드 2개 등장 시 비대칭

- [ ] `Assets/Scripts/UI/LevelUpPanel.cs` 검증
- [ ] 카드 수에 따라 자동 정렬 (HorizontalLayoutGroup 또는 수동 위치)
  - 1개: 가운데
  - 2개: 좌우 대칭
  - 3개: 좌중우

### 5-5. 카드 UI 폰트 색상 / 일괄 NotoSansKR 점검

**피드백**: 카피스킬 슬롯 폰트 색상 (또?)

- [ ] 모든 씬에서 TMP_Text 일괄 NotoSansKR 강제 (Day 37 패턴 재사용)
  ```csharp
  var noto = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansKR-Regular SDF.asset");
  foreach (var t in GameObject.FindObjectsOfType<TMP_Text>(true)) {
      if (t.font != noto) { Undo.RecordObject(t, "font fix"); t.font = noto; EditorUtility.SetDirty(t); }
  }
  ```
- [ ] 폰트 색상: 검정/회색 배경에 잘 보이는 흰색 또는 노란색으로 일관 (현재 슬롯 일부 어두운 색상일 가능성)

### 5-6. 일시정지 기능 (★ 신규)

**피드백**: 게임 도중 일시정지 안 됨

- [ ] `GameManager.GameState`에 `Paused`가 이미 enum에 있음 (확인됨) — `Time.timeScale = 0` 처리도 이미 됨
- [ ] ESC 키로 토글하는 입력 처리 추가
  - 위치: `GameManager.Update` 또는 신규 `PauseInput.cs`
  - 코드:
    ```csharp
    if (Input.GetKeyDown(KeyCode.Escape) && CurrentState == GameState.Playing)
        ChangeState(GameState.Paused);
    else if (Input.GetKeyDown(KeyCode.Escape) && CurrentState == GameState.Paused)
        ChangeState(GameState.Playing);
    ```
- [ ] `PausePanel.cs` 신규 (VictoryPanel.cs 패턴 참고)
  - 위치: `Assets/Scripts/UI/PausePanel.cs`
  - 표시: "일시정지" 타이틀 + "계속하기" / "메인 메뉴" 2버튼
  - 메인 메뉴 클릭 시 VictoryPanel.CleanupPersistentManagers 패턴 재사용 + `SceneManager.LoadScene("MainMenu")`
- [ ] 각 Stage 씬에 PausePanel GameObject 배치 (활성 상태로 저장 — Start에서 자동 SetActive(false))
- [ ] `GameManager.OnGameStateChanged` 구독 → Paused 진입/탈출 시 패널 표시/숨김

### 5-7. 검증
- [ ] 빌드 → 모든 스테이지 UI 가독성 OK
- [ ] ESC 누르면 일시정지 패널 / 게임 멈춤 / 다시 누르거나 "계속하기"로 재개
- [ ] "메인 메뉴" 버튼 → 매니저 정리 + 메뉴 로드 확인

---

## 6. Day 44 - 3스 시야 강화 + VoidPierce 재설계

### 6-1. 시야 제한 강화 (★ 피드백 핵심 — 컨셉 미활용 해결)

**현 상태**: Stage3_Deep의 `GlobalLight2D_Dark` intensity 0.1 / VisionLight2D outerRadius 6. 너무 밝아서 시야 밖 적이 잘 보이고, 원거리 스킬이 시야 밖에서도 죽임.

- [ ] Stage3_Deep 씬 로드 → `[Map]/GlobalLight2D_Dark` 선택 → Light2D 컴포넌트
  - `m_Intensity` 0.1 → **0.04** (더 어둡게)
  - `m_Color` (0.4, 0.5, 0.7) → **(0.2, 0.3, 0.5)** (더 차게)
- [ ] `[Player]/Player/VisionLight2D` Light2D 컴포넌트
  - `m_PointLightOuterRadius` 6 → **5** (시야 자체도 약간 좁게 → 적이 더 가까이 접근해야 보임)
  - `m_PointLightInnerRadius` 3 → 2.5 (밝은 영역도 줄임)

### 6-2. 시야 밖 원거리 적 가시성 차단 + 발사 직전 반짝임

**피드백**: 원거리 적이 초록색이라 시야 밖에서도 위치 보임 → 컨셉 무력화. "발사 직전 반짝임" 정도가 적당하다는 피드백.

- [ ] `Assets/Scripts/Enemy/EnemyRangedAttack.cs`에 조준 단계 추가
  - 현재 흐름: cooldown ≤ 0 → 즉시 Fire
  - 신규 흐름:
    1. cooldown ≤ 0 → **조준 단계 0.3s 시작** (`SpriteRenderer.color` 점멸 또는 발광 강조)
    2. 0.3s 후 발사
  - 코루틴 `AimAndFire`로 분리. cooldown 리셋은 발사 직후
  - SpriteRenderer를 LineRenderer 대신 활용 (시야 밖이라 라인 보이면 컨셉 깨짐 — 적 자체가 잠깐 발광)
- [ ] EnemyLaserAttack도 동일 패턴이지만 LineRenderer가 두꺼움 → Stage4 한정이라 그대로 둬도 됨
- [ ] BalloonEel(Stage3) 원거리 적도 동일 EnemyRangedAttack 사용 — 자동 적용

### 6-3. VoidPierce(공허 관통) 재설계

**피드백**: "쓰레기" 평가. 방향 조준 어려움(`controller.FaceDirection` 사용), 일반 스킬보다 약함.

**채택 안: A + B 조합** (조준 방식 변경 + 데미지 ↑)

- [ ] `Assets/Scripts/Skills/VoidPierceSkill.cs`
- [ ] 조준 방식 변경 (마우스 커서 방향)
  ```csharp
  // 기존: Vector2 dir = controller.FaceDirection;
  // 변경:
  Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
  mouseWorld.z = 0;
  Vector2 dir = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
  ```
- [ ] 데미지 / 다단 히트 ↑
  - `damageMultiplier` 1.2 → **1.8**
  - `hitTicks` 4 → **5**
  - `length` 9 → 10 (사거리도 약간 ↑)

### 6-4. 검증
- [ ] Stage3 진입 → 화면 대부분이 진짜로 어둡고 적이 시야 밖에서 안 보임
- [ ] 원거리 적이 화면 밖에서 발사할 때 잠깐 반짝임 → 위치 식별 + 회피 가능
- [ ] VoidPierce를 마우스로 조준 → 적에 명중 → 일반 스킬보다 명백히 강함 체감

---

## 7. Day 45 - 스킬 풀 확장 + BleedSwim 재설계

디자인 문서 §3에 따른 추가 스킬 5종 + 6주차 §11 미룸 항목(BleedSwim 재설계) 흡수.

### 7-1. 공격 스킬 3종
- [ ] **음파 진동** (Sonic Pulse) — 주기적 광역 펄스, 적 넉백
  - 위치: `Assets/Scripts/Skills/SonicPulseSkill.cs` (자동 공격 베이스 따라 신규)
  - 일반 스킬 베이스(`PlayerCombat`이 관리하는 SkillData 패턴) 따름
- [ ] **가시 발사** (Spike Burst) — 다방향 발사체
  - Slash + EnemyProjectile 응용. 8방향 동시 발사
- [ ] **흡혈 촉수** (Drain Tentacle) — 가장 가까운 적 자동 추적, 데미지 + 흡혈
  - 매 N초 가까운 적 1체에 라인 형태 데미지 + 받은 데미지의 일정 비율을 HP로 회복

### 7-2. 패시브 스킬 2종

- [ ] **자기 유도** (Magnetic Induction) — 경험치/세포 흡인 범위 ↑
  - 구현: `Assets/Scripts/Systems/ExpOrbPool.cs` 또는 `ExpOrb` 컴포넌트의 추적 범위 파라미터
  - 패시브 레벨에 따라 추적 범위 증가 (1Lv +20% / 2Lv +40% / 3Lv +60% / 4Lv +100%)
  - 뱀서라이크 표준 자석 기능. 모든 스테이지 활용
- [ ] **발광 기관** (Glow Organ) — 일정 쿨다운마다 빛나는 보호막 (적 공격 1회 흡수)
  - 보호막 활성 중에는 **Stage3 시야 반경 +25%** (컨셉: 발광하니까 잘 보인다)
  - 쿨다운: 12초 (Lv↑ 시 -2초씩, 4Lv 만렙 6초)
  - 구현 위치: `PlayerStats.TakeDamage` 시작부 인터셉트 (의태 기관 `healingBlocked` 패턴 재활용)
    ```csharp
    public void TakeDamage(float amount) {
        if (glowShieldActive) {
            ConsumeShield();  // 시각 OFF, 쿨다운 시작
            return;  // 데미지 무효
        }
        ...
    }
    ```
  - 시각: Player 자식 `GlowShieldVisual` (Light2D Point 작은 반경 + 발광 sprite) — 활성/비활성 토글
  - VisionLight2D 보너스: `glowShieldActive`일 때만 `outerRadius *= 1.25`
- ~~압력 적응~~ — 제거 (2·3스 한정 + 메타 압력 저항과 중복감)

### 7-3. BleedSwim(절단 유영) 재설계 — Week6 §11 미룸 흡수

**배경**: 6주차 Day 37에서 "가속+출혈 트레일"로 구현했으나 사용자 의도("순간이동 베기")와 다름. 기존 돌진기 2종과 차별화 필요.

| 카피 | 거리 | 시간 | 차별점 |
|------|------|------|--------|
| Dash (1스) | 짧음 | 짧음 | 단순 회피 |
| PredatorCharge (2스) | 길게 | 0.4s | 무적 + 즉시 대피해 + 스턴 |
| **BleedSwim 신안** | 중간 | 1.5s | **부딪힌 적 출혈 지속 피해(DoT)** |

- [ ] `Assets/Scripts/Skills/BleedTrailNode.cs` **삭제**
- [ ] `Assets/Scripts/Skills/BleedSwimSkill.cs` 수정:
  - `duration` 4s → **1.5s**
  - `speedMultiplier` 2.5 → **3.5** (거의 순간이동 느낌)
  - 가속 중 매 프레임 `OverlapCircleAll`로 부딪힌 적 검출
  - `HashSet<Collider2D>` 중복 타격 방지
  - 가속 동안 `stats.IsInvincible = true` (PredatorCharge 패턴)
- [ ] 출혈 디버프 구현 — 옵션:
  - (단순) BleedSwim 자체에서 hit list 유지하며 N초간 0.3s 간격 데미지
  - (재사용성) `Assets/Scripts/Enemy/BleedDebuff.cs` 신규 — 적에 부착되는 컴포넌트, 자체 코루틴으로 지속 피해. 향후 다른 스킬/적도 활용
  - **권장**: 첫 구현은 단순 안. 사용성 보고 BleedDebuff 컴포넌트화

### 7-4. LevelManager 스킬 풀 등록
- [ ] 신규 5종 SkillData 자산 생성 (공격 3 + 패시브 2)
  - 위치: `Assets/ScriptableObjects/SkillData/SkillData_*.asset`
- [ ] LevelUpPanel 카드 후보군에 포함 — LevelManager의 스킬 풀 배열에 추가
- [ ] 첫 자동 부여 스킬에는 포함하지 않음 (선택성 유지)

---

## 8. Day 46 - 보스 패턴 보강

### 8-1. 향유고래 풀 패턴 (Boss_Whale)
- [ ] **초음파 광역** — 페이즈1 추가 패턴
  - `UltrasonicSkill.cs`의 부채꼴 판정 응용
  - 새 컴포넌트 `BossWhaleUltrasonic.cs` 또는 BossBase 확장
- [ ] **페이즈2 추가 패턴** — DeepPressure 응용 광역 슬로우/취약 디버프
  - `EnemyBase.ApplySlow` / `ApplyVulnerability` 기존 메서드 활용

### 8-2. 산갈치 마디 판정 (Boss_Oarfish) — Week6 §11 미룸
- [ ] 다중 collider로 마디별 hit 처리
  - Boss_Oarfish prefab에 자식 GameObject 4~6개 (각 마디)
  - 각 마디에 Collider2D + 자식용 `OarfishSegment.cs` 컴포넌트 (부모 BossBase로 데미지 전달)
- [ ] 마디별 SpriteRenderer (자식)
- [ ] 마디별 발광 효과 (Light2D Point 또는 emissive material)

### 8-3. 연구소장 드론 소환 (Boss_Director) — Week6 §11 미룸
- [ ] **자폭 드론** — `EnemySuicideExplode.cs` 재활용
  - 드론 prefab 신규 (`Assets/Prefabs/Enemies/Enemy_Drone.prefab`)
  - 작은 크기, 추적, 자폭 시 광역 폭발
- [ ] `BossDirectorAttack.cs`에 드론 소환 패턴 추가 (페이즈2 또는 신규 페이즈3)
- [ ] Director 자식 spawn point 위치 마커

### 8-4. 연구소장 보호막 — Week6 §11 미룸
- [ ] 특정 HP 구간(예: 25%)에서 일시 무적 + 보호막 시각 표시
  - `BossBase.cs` 또는 `BossDirectorShield.cs` 신규
  - HP가 그 구간 진입 시 `IsInvincible = true` 5초간
  - 보호막 해제 후 행동 패턴 변화 (드론만 소환)
- [ ] 보호막 시각: BossPhaseEffect 패턴 응용 (자식 발광 sprite)

---

## 9. Day 47 - 매니저 prefab + 사운드 기초

### 9-1. 매니저 prefab 변환
- [ ] `[Managers]` prefab 생성 (`Assets/Prefabs/Managers/[Managers].prefab`)
  - 자식: GameManager, LevelManager, StageManager, BioEnergyManager, CopySkillManager, PlayerProgressData
  - 각 매니저는 기존 DontDestroyOnLoad 패턴 유지
- [ ] 각 Stage 씬에 prefab 인스턴스 1개씩 배치 (현재 씬별 개별 GameObject)
- [ ] MainMenu 씬에는 PlayerProgressData만 따로 (다른 매니저는 Stage1 진입 시 생성)
- [ ] 단독 씬 테스트 편의성 ↑ (Stage3만 따로 켜도 매니저 자동 로드)

### 9-2. 사운드 시스템 기초
- [ ] `Assets/Scripts/Systems/AudioManager.cs` 신규 (싱글톤, DontDestroyOnLoad)
- [ ] BGM 슬롯 + 스테이지별 다른 트랙 (저작권 무료 자산)
  - 메인 메뉴 BGM, Stage1~4 BGM, 보스 BGM(공통)
- [ ] SFX 풀: 공격/피격/사망/카피 발동/레벨업/구매 등 10종 내외
- [ ] 마스터/BGM/SFX 볼륨 분리 (메타 패널 옆에 설정 패널 또는 일시정지 패널에)

### 9-3. 사운드 자산 조달 옵션
- freesound.org (CC0/CC-BY)
- Unity Asset Store 무료 팩
- 시간 부족하면 **BGM 1개 + SFX 5개**로 최소 구현, 7주차 후반 또는 8주차 확장

---

## 10. Day 48 - 이펙트 강화

### 10-1. 보스 페이즈 전환
- [ ] 페이즈2 진입 시 화면 흔들림 + 짧은 시간 정지 (Hit Stop)
- [ ] 사운드 큐 (SFX) + 색상 톤 변경 (BossPhaseEffect 이미 있음, 확장)

### 10-2. 카피 스킬 발동 + 손맛 강화 (★ BleedSwim 액션감 대응)
- [ ] Q/E/Space 발동 시 짧은 파티클 / 라인 글로우
- [ ] **카피 스킬 손맛 공통 컴포넌트** `HitEffectGlobal.cs` (가칭)
  - 적 충돌 순간 hitstop 0.05s (`Time.timeScale = 0` 짧게)
  - 카메라 미세 흔들림 (BleedSwim/PredatorCharge 같은 돌진/베기 류)
  - 모든 스킬 충돌 코드에서 호출 → BleedSwim 외 다른 스킬도 자동 혜택

### 10-3. 적 사망
- [ ] Enemy 사망 시 작은 폭발 파티클 (현재는 즉시 SetActive(false))
- [ ] ExpOrb 드롭 모션 강화 (튀어나오는 애니메이션)

### 10-4. 카메라 효과
- [ ] 보스 접근 시 미세 줌 (orthographicSize 살짝 ↓)
- [ ] 보스 페이즈2 진입 시 짧은 흔들림 (DOTween 또는 코루틴)

### 10-5. 기본 공격 시각 효과 (★ 피드백 신규)
**문제**: 플레이어 기본 공격이 시각적으로 약함

- [ ] **Slash**: 휘두름 호 효과 (BossSwingAttack의 LineRenderer 부채꼴 패턴 응용)
- [ ] **PoisonNeedle**: 발사 시 시작점 작은 파티클
- [ ] **BioticExplosion**: 폭발 반경 표시 (반투명 원형 spawn → 페이드 아웃)
- [ ] **ElectricEngine**: 전기 호 효과 (Z 모양 LineRenderer)

---

## 10b. Day 48b - 2차 밸런싱 (신규 콘텐츠 반영 후)

Day 42 1차 밸런싱(42a/b/c) 이후 신규 콘텐츠가 추가됨:
- Day 45: 일반 스킬 5종 (공격 3 + 패시브 2)
- Day 46: 보스 패턴 (산갈치 마디, 연구소장 드론/보호막, 향유고래 풀)
- Day 44: VoidPierce 재설계 (마우스 조준 + 데미지↑)
- Day 45: BleedSwim 재설계 (1.5s 가속 + 출혈 DoT)

신규 위협이 들어왔으므로 현재 수치가 다시 변할 수 있다.

### 10b-1. 측정 (풀 사이클 1회)
- [ ] 1~4스 클리어 시 레벨 / 시간 / 사망 수 / 세포 누적 기록
- [ ] 각 보스 페이즈2 도달 + 처치까지 시간
- [ ] 신규 스킬 5종 — 어느 게 사기/쓰레기?
- [ ] 신규 보스 패턴 — 위협적인가 보여주기용인가?

### 10b-2. 조정 (1차 결과에 따라)
- [ ] 보스 HP: Day 42c 기준(ExperimentalSubjects 6000/Whale 10000/Oarfish 14000/Director 17000)에서 ±20% 범위로 조정
- [ ] 신규 스킬 5종 데미지/쿨다운/효과 비교 — 일반 스킬(Slash/PoisonNeedle 등)과 동등 수준 맞추기
- [ ] **DrainTentacle (흡혈촉수) 너프** (Day 45 사용자 피드백) — 현재 cooldown 2s / damageMultiplier 누적 / lifestealRatio 0.3. 흡혈량 또는 데미지 둘 중 하나 조정. 후보:
  - lifestealRatio 0.3 → 0.15~0.2 (회복량 절반)
  - 또는 cooldown 2 → 3s (발동 횟수 ↓)
  - 또는 base damageMultiplier 1.0 → 0.7 (단발 데미지 ↓ 대신 흡혈 가치 유지)
- [ ] 보스 신규 패턴 데미지 — 페이즈2 진입 시 위협 체감 확인
- [ ] Stage3 — Day 44 시야 강화 + outerRadius 5 적용 후 실제 어려운지 측정. 안 어렵다면 적 추가 상향
- [ ] 카메라 ortho 6.5 — Stage3 시야 시스템과 정합 재확인
- [ ] **보스전 일반몹 동시 스폰** (Day 45 후속 적용) — 페이즈2 + 일반몹 + 보스 패턴 조합이 너무 가혹한지 측정. 가혹하면 보스 등장 시 EnemySpawner.intervalScale ×1.3~1.5 (스폰 빈도 살짝 ↓)
- [ ] **타이머 카운트업** (Day 45 후속) — 시간 초과 GameOver 제거됨. 보스 안 잡으면 무한 스폰되는 점이 정상 동작인지, 적정 클리어 시간(보스 처치까지) 측정

### 10b-3. 검증 목표 (2차 빌드 직전)
- 풀 사이클 5~7회 클리어 시 메타 업그레이드 풀 만렙 도달 (지금 ≈ 39세포/런 기준 적절)
- 각 보스 페이즈2 도달 후 처치
- 1·2·3·4스 난이도 점진 상승 (현재 후반 갈수록 쉬워지는 곡선 해소 여부 확인)
- 일반 스킬 + 카피 스킬 + 신규 스킬 풀이 빌드별 다양성을 만드는지

---

## 11. Day 49 - UI 폴리싱 잔존 + 2차 빌드

### 11-1. 메뉴/패널 디자인 (Day 43에서 처리 못한 디자인 차원)
- [ ] MainMenu 배경 (단색 → 그래디언트 또는 이미지)
- [ ] 메타 업그레이드 패널 행 디자인 (현재 단색 박스)
- [ ] VictoryPanel 디자인 보강 (서브 텍스트, 통계 등)
- [ ] CopySkillSelectPanel 카드 디자인 강화

### 11-2. 폰트/아이콘
- [ ] 타이틀 폰트 별도 (NotoSansKR 외 디자인 폰트 검토)
- [ ] 카피 스킬 슬롯 아이콘 (현재 텍스트만)
- [ ] HP/EXP/세포/에너지 HUD 아이콘

### 11-3. 튜토리얼 (선택)
- [ ] 첫 플레이 시 기본 조작 안내 (WASD, 마우스, Q/E/Space)
- [ ] PlayerPrefs로 "본 적 있음" 플래그

### 11-4. 2차 빌드
- [ ] Windows Standalone (Release build, no dev symbols)
  - MCP `manage_build`: `development: false`
  - output_path: `Build/Windows_Release/Project_Abyss.exe`
- [ ] 빌드 크기 비교 (1차 228 MB → ?)
- [ ] 외부 테스터에 2차 빌드 전달

### 11-5. 7주차 통합 테스트
- [ ] 풀 사이클 (메뉴 → 1~4스 → 엔딩 → 메뉴)
- [ ] 새 스킬 5종 모두 레벨업 카드로 등장 확인
- [ ] 새 보스 패턴 정상 동작 (산갈치 마디, 연구소장 드론/보호막, 향유고래 풀 패턴)
- [ ] 사운드 정상 재생
- [ ] 일시정지 정상

---

## 12. 7주차 외 (8주차 이후 후보)

폴리싱이 더 필요한 항목들. 우선순위 낮음.

### 디자인 보강
- 스킬 해금 시스템 (특정 스킬은 메타로 해금해야 첫 선택지에 등장)
- 더 많은 돌연변이 (현재 3종 → 6~8종)
- 일별 챌린지 / 시드 시스템
- 보스 페이즈3 (디자인 문서에 언급된 부분 대응)

### 시스템
- 설정 메뉴 (해상도, 풀스크린, 키 바인딩)
- 세이브 데이터 변조 방지 (JSON + 해시) — Week6 §12 단순화 원칙에서 이월
- 통계 화면 (런별 클리어 시간, 처치 수 등)
- 도전 과제 / 업적

### 폴리싱
- 로컬라이제이션 (영문 지원)
- Steam 배포 준비
- 사운드 자산 자체 제작/구매

---

## 13. 주의할 점

### 13-1. 작업 순서
- **Day 42를 가장 먼저**. 그 다음 Day 43~44는 병렬 가능 (다른 사람 있다면).
- 새 콘텐츠(Day 45~46)는 밸런싱(Day 42)이 안정된 후 추가해야 효과 측정 가능.
- Day 47(매니저 prefab + 사운드)는 다른 작업과 의존 적어서 시간 빌 때 끼워넣기.

### 13-2. 외부 피드백 의존
1차 빌드 피드백 받음 → Day 42~44에서 80% 직접 대응.
2차 빌드(Day 49)에서 한 번 더 피드백 받을 수 있음.

### 13-3. UI 작업 분산
Day 43에서 **시급한 가독성/잘림/일시정지 픽스** 위주, Day 49에서 **디자인적 폴리싱**. 디자인 부분(그래디언트, 아이콘)은 8주차로 미뤄도 됨.

### 13-4. 사운드 시스템 = 큰 변수
사운드는 자산 조달 + 시스템 구현 둘 다 시간 소요. 7주차 안에 부담스럽다 싶으면 **BGM 1개 + SFX 5개**로 최소 구현하고 8주차에 확장.

### 13-5. 검증 기준 (Day 42 끝났을 때)
- 1스 클리어 시 레벨 15~18 (현재 22)
- 4스 클리어 시 레벨 38~45 (현재 52)
- 클리어 시 세포 50~65 (현재 130)
- 보스 페이즈2 도달 후 처치
- 가만히 있으면 클리어 안 됨

### 13-6. 7주차 산출물
- 빌드 버그 해결 + 밸런스 안정
- 스킬 풀 5종 추가 (매 런마다 다른 빌드 가능)
- 보스 4종 풀 패턴 (각 보스 고유성 ↑)
- 일시정지 + 사운드 + 이펙트 (게임감 ↑)
- UI 가독성 ↑
- **2차 외부 빌드** → 한 번 더 피드백 받을 수 있는 상태

### 13-7. 새 세션에서 작업 시작 시 체크
1. 현재 어느 Day까지 완료됐는지 → `Summary/` 폴더 또는 메모리 인덱스 확인
2. 작업할 Day의 의존성 충족 여부 (§3 표 참고)
3. 변경할 파일/경로 명시 (각 Day 항목에 모두 적혀있음)
4. 작업 후 검증 항목 (각 Day 마지막 `### X-Y. 검증` 절)
