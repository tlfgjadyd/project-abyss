# Project: Abyss - 8주차 TODO

작성일: 2026-05-26 (7주차 종료 + 개발자 풀플레이 피드백 후 작성)

이 문서는 **새 세션에서 컨텍스트 없이 봐도 작업 시작이 가능**하도록 작성됨. 파일 경로 / 수치 before-after / 구현 위치 명시. 모든 작업은 7주차 완료 상태(2차 Release 빌드 산출)에서 시작.

본 문서는 **8주차 시작 전까지 변경될 수 있음**. 외부 피드백 + 추가 개인 피드백이 들어오면 §2-2와 §3에 항목 추가하여 갱신.

---

## 0. 새 세션 cold-start 가이드

### 0-1. 현재 프로젝트 상태 (7주차 완료 시점, 2026-05-26)

**플레이 가능한 풀 사이클**: MainMenu → Stage 1~4 → 연구소장 처치 → VictoryPanel → 메인 메뉴 복귀 → 메타 업그레이드 5종 (HP/이속/공속/공격력/압력저항) → 재시작

**보스-스테이지 매핑** (혼동 주의 — `memory/boss_stage_mapping.md` 참고):
- 1스 Lab: **실험용 실험체** (Boss_ExperimentalSubjects.asset, HP 6000)
- 2스 Sea: **향유고래** (Boss_Whale.asset, HP 10000)
- 3스 Deep: **산갈치** (Boss_Oarfish.asset, HP 14000)
- 4스 Ruined: **연구소장** (Boss_Director.asset, HP 17000)

**구축 완료된 시스템** (7주차 산출물):
- 일반 스킬 풀 12종 (공격 7 + 패시브 5) + 슬롯 제한(공격 5 / 패시브 3)
- 카피 스킬 9종 (Q/E/Space 슬롯, 보스 처치 시 1개 선택)
- 돌연변이 5종 (레벨 14·25에서 선택, 패널티 동반)
- 보스 패턴 보강: 향유고래 초음파+페이즈2 디버프, 산갈치 5마디 chain + 텔레포트, 연구소장 드론+보호막
- 시야 제한 (Stage3 한정 자동추적 사거리 5.5)
- UI: HUD(슬라이더+라벨), 일시정지(스킬 패널+호버 툴팁), LevelUp/카피선택/돌연변이 카드(description 26개), MainMenu 그래디언트, VictoryPanel 통계, Tutorial(Stage1 첫 플레이만)
- 시각 이펙트: DeathExplosion, 기본 공격 시각 4종(Slash 부채꼴/PoisonNeedle 머즐/BioticExplosion 원형/ElectricEngine zigzag), 전기뱀장어 원형 데미지 필드, 산갈치 텔레포트 페이드
- 시스템: [Managers] prefab 통합, AudioManager(자산 미할당 상태 — 시스템만), 카메라 효과 시스템(현재 호출 X, 코드만 보존)
- 2차 Windows Release 빌드: `Build/Windows_Release/Project_Abyss.exe` (203 MB)

### 0-2. 주요 파일/폴더 빠른 참조

| 분류 | 경로 |
|------|------|
| 8주차 계획서 | `Project_Abyss/Project_Abyss/ProjectAbyss_Status_Week8.md` (본 문서) |
| 7주차 계획서 | `Project_Abyss/Project_Abyss/ProjectAbyss_Status_Week7.md` (참고용) |
| 디자인 문서 | `Project_Abyss/Project_Abyss/ProjectAbyss_GameDesignDocument.md` |
| 개발자 피드백 (5/26) | `Project_Abyss/Project_Abyss/Summary/feedback_memo0526.md` (작성 시 §2-2 참조) |
| 매니저 prefab | `Assets/Prefabs/Managers/[Managers].prefab` (4씬 인스턴스 공유) |
| 스테이지 데이터 | `Assets/ScriptableObjects/StageData/Stage{1-4}_*.asset` |
| 적 데이터 | `Assets/ScriptableObjects/EnemyData/EnemyData_*.asset` (14종) |
| 보스 데이터 | `Assets/ScriptableObjects/BossData/Boss_*.asset` (4종) |
| 카피 스킬 데이터 | `Assets/ScriptableObjects/CopySkillData/CopySkill_*.asset` (9종) |
| 일반 스킬 데이터 | `Assets/ScriptableObjects/SkillData/*.asset` (12종) |
| 적 prefab | `Assets/Prefabs/Enemies/Enemy_*.prefab` (Drone 포함 15종) |
| 보스 prefab | `Assets/Prefabs/Bosses/Boss_*.prefab` |
| 씬 | `Assets/Scenes/MainMenu.unity`, `Stage{1-4}_*.unity` |
| 폰트 | `Assets/Fonts/NotoSansKR-Regular SDF.asset` |
| 플레이어 스탯 | `Assets/Scripts/Player/PlayerStats.cs` (TakeDamage에 발광기관/보스디버프 처리) |
| HP 회복 차단 | PlayerStats.healingBlocked (의태기관 디버프 — 작동 의심) |
| 흡혈촉수 | `Assets/Scripts/Skills/DrainTentacle.cs` (lifestealRatio 0.1, 여전히 사기) |
| 체력 재생 | `Assets/Scripts/Skills/` + CellRegeneration.asset (Lv4 누적 1.5/s) |
| 돌연변이 매니저 | `Assets/Scripts/Systems/MutationManager.cs` (의태기관 ApplyMimicryOrgan) |
| 빌드 산출물 | `Build/Windows_Release/Project_Abyss.exe` (203 MB) |

### 0-3. 작업 우선순위

1. **Day 50 (UI 픽스 + 밸런싱 즉시 반영)** — 가장 먼저. 텍스트 가독성 + 흡혈촉수 / 의태기관 / 체력재생 검토
2. **Day 51~52 (에셋 도입 1차: 캐릭터/적/보스 sprite)** — "진짜 게임 느낌" 핵심
3. **Day 53 (에셋 도입 2차: 배경/타일/UI 아이콘)**
4. **Day 54 (사운드 자산 도입)** — 7주차에 보류했던 것
5. **Day 55~56 (통합 + 추가 밸런싱)** — 에셋 도입 후 깨질 가능성 큰 시각 정합
6. **Day 57 (외부 피드백 반영) + Day 58 (3차 빌드)**

### 0-4. 작업 시 표준 절차

- 스크립트 수정: `Assets/Scripts/...` 직접 편집 → Unity 자동 컴파일 대기
- 씬 수정: MCP `manage_scene` load → 수정 → save
- prefab 수정: MCP `execute_code`로 `PrefabUtility.LoadPrefabContents` → SerializedObject 갱신 → `SaveAsPrefabAsset`
- 에셋 임포트: `Assets/` 하위에 폴더 정리 후 드래그. `Assets/Sprites/`, `Assets/Audio/BGM/`, `Assets/Audio/SFX/` 권장
- 컴파일 후 새 컴포넌트 즉시 인식 안 되면: `manage_editor` stop → `refresh_unity mode=force` → 재시도
- 한글 텍스트는 NotoSansKR-Regular SDF 폰트 강제 (모든 씬 적용 완료)

---

## 1. 8주차 목표

7주차에서 게임의 시스템 골격(스킬 풀 + 보스 패턴 + UI + 빌드)을 완성했다. 8주차의 핵심 목표는:

1. **시각·사운드 에셋 도입** — "진짜 게임 느낌". AI 생성 + 에셋 스토어 무료 자산. 7주차까지의 LineRenderer 임시 시각 일부를 sprite/particle로 교체
2. **개발자 피드백 즉시 반영** — UI 가독성 버그 + 흡혈촉수/체력재생/의태기관 밸런싱
3. **외부 피드백 흡수** — 2차 빌드 전달 후 들어오는 피드백 반영

### 7주차 대비 차이
- 7주차: 시스템 완성 + 콘텐츠 확장 + 1·2차 빌드 (코드·로직 중심)
- 8주차: **에셋 도입 + 시각/사운드 폴리싱** (감각 자극 중심)

---

## 2. 피드백 요약

### 2-1. 개발자 5/26 풀플레이 피드백 (Day 49 종료 후)

원문: `Summary/feedback_memo0526.md` (작성 시 참고)

#### UI 가독성 버그 (Day 50에서 즉시 픽스)

1. **레벨업 카드 — 하얀 바탕에 하얀 텍스트** 일부 텍스트 안 보임
2. **카피스킬 슬롯 활성화 시** — 하얀 바탕 + 하얀 텍스트 가독성 0
3. **튜토리얼 패널 — "시작" 버튼이 마지막 텍스트("사망 시 모은...")와 겹침** Box 높이 늘리거나 본문 하단 패딩 추가 필요

#### 밸런싱 잔존 문제

4. **흡혈촉수 여전히 사기**
   - 사용자 안: 체력 비율 흡수 → **고정 0.1 회복** (10번 사용 = 1HP 회복). 현재 `damage × lifestealRatio(0.1)`인데 데미지가 높으면 회복도 큼
   - 수정 방향: `lifestealRatio` 사용 안 함 → `lifestealFlat = 0.1f` (고정값) 사용
5. **체력재생 패시브 사기** — 사용자도 어떻게 손볼지 모름
   - 후보 안: ①절대값 더 너프 (Lv4 1.5/s → 0.8/s) ②피격 후 N초 쿨다운 도입 ③최대 HP의 % 회복으로 변경하되 매우 낮은 비율
6. **의태기관 — 패널티 작동 의심 + 잘 안 쓰게 됨**
   - 현재 효과: 이속 ×2, 공격력 ×0.5, 60초마다 3초 무적
   - 잘 안 쓰는 이유 추정: 공격력 ×0.5 패널티가 후반 보스전에서 치명적 + 무적 3초가 60초 주기라 체감 약함
   - 수정 방향 후보: ①공격력 ×0.5 → ×0.7 (패널티 완화) ②무적 60초 → 40초 (빈도 ↑) ③대시같은 능동 회피 추가
   - 패널티 작동 검증: `MutationManager.ApplyMimicryOrgan`이 `stats.attackPower *= 0.5f` 실행하는지 디버그 로그 추가

### 2-2. 외부 피드백 (5/27, 2차 빌드 + 추가 개발자 정리)

원문: `Summary/feedback/feedback_memo0527.md`

5/26 피드백과 일부 중복 — 중복은 Day 50에서 일괄 처리. 신규 항목은 우선순위 따라 분배.

#### UI (외부)
- **HUD 라벨 위치 버그** (신규 — Day 49 라벨 부작용): 체력/에너지 라벨이 화면 밖, EXP 라벨이 슬라이더와 겹침. 슬라이더 좌측 (pivot=1) 배치가 화면 가장자리에 가까운 슬라이더에 부적절. **수정**: 라벨을 슬라이더 *위쪽* 작은 텍스트로 옮기거나, 슬라이더 우측으로
- 카드 가독성 / 카피슬롯 / 튜토리얼 겹침 — 5/26과 동일

#### 밸런스 (외부)
- **발광기관 보호막 + 전기뱀장어 일렉트릭필드 상호작용 의심** (신규): 보호막이 안 사라지는 것 같음. 정확한 검증 어려움 (뱀장어 빨리 죽음). **확인 필요**: `PlayerStats.TakeDamage`의 `glowShieldActive` 분기가 `ElectricEelAura.damagePerTick` 호출에도 작동하는지. 코드상으로는 `stats.TakeDamage(...)`이므로 작동해야 함. 디버그 로그로 검증
- **2스 보스 출현 5분으로** (현재 8분): StageData Stage2_Sea.bossSpawnTime 480→300, stageDuration도 540→360
- **3스 일반몹 물량 ↑** (2스처럼): 이미 Day 48b에서 ×1.5 했지만 추가
- **3스 일반몹 HP 상향**: 플레이어 평시 공격력 ~45 / 과부화 ~54 기준 2방에 피 약간 남는 수준. Anglerfish HP 52→**100**, GiantSquid HP 75→**120**
- **3스 보스 출몰 4분** (현재 8분): bossSpawnTime 480→240
- **3스 만렙 후 컨텐츠 부족** (보스 출몰 단축으로 일부 해결)
- **4스 보스 출몰 4분**: bossSpawnTime 480→240
- **4스 LaserSoldier 사거리 ×2**: EnemyLaserAttack.range 현재 값 ×2 + StopDistance 유지 (적이 멀리서도 위협)
- 흡혈촉수 / 체력재생 — 5/26과 동일

#### 콘솔 에러 (개발자 추가)
- **VoidPierce 채널링 중 죽은 적의 HitEffect.PlayFlash 호출 실패**:
  ```
  Coroutine couldn't be started because the the game object 'Enemy_*(Clone)' is inactive!
  HitEffect:PlayFlash → EnemyBase:TakeDamage → VoidPierceSkill:ApplyTick
  ```
- 원인: VoidPierce 0.15s 틱마다 BoxCast → 같은 collider가 죽은 후에도 한 tick은 검출 + TakeDamage 호출 → HitEffect.PlayFlash가 SetActive(false)된 GameObject에서 StartCoroutine 시도
- **수정**: `Assets/Scripts/Enemy/HitEffect.cs` PlayFlash 시작에 `if (!gameObject.activeInHierarchy) return;` 1줄 추가

#### 디자인 (개발자 추가)
- **공허관통 재설계**: "산갈치의 긴 마디를 카피해 플레이어 사슬팔이 마디로 뻗어 전기톱처럼 갈아버리는" 원래 컨셉으로. 사거리 두 배 + 두께 ↑ + 파괴광선 느낌. 현재 LineRenderer 단순 빔 → sprite 체인 또는 파티클 트레일

---

## 3. 8주차 작업 큰 흐름

| Day | 주제 | 의존 |
|-----|------|------|
| 50 | UI 픽스 + 밸런싱 즉시 반영 (개발자 피드백) | (시작점) |
| 51 | 에셋 도입 1차 - 플레이어/일반 적 sprite | 50 |
| 52 | 에셋 도입 1차 후속 - 보스 sprite + 카피 스킬 시각 | 51 |
| 53 | 에셋 도입 2차 - 배경/타일/UI 아이콘 | 51 |
| 54 | 사운드 자산 도입 (BGM 5 + SFX 6) | (병렬 가능) |
| 55 | 통합 정합 작업 - sprite 도입 후 깨진 색상/스케일 조정 | 51~53 |
| 56 | 추가 밸런싱 (에셋 도입 후 체감 변화 반영) | 55 |
| 57 | 외부 피드백 반영 | 2차 빌드 전달 후 |
| 58 | UI 폴리싱 잔존 + 3차 빌드 | 모두 |

원래 계획 + 외부 피드백 대응 여지. 외부 피드백 양에 따라 Day 57을 2~3일로 늘려도 됨.

---

## 4. Day 50 - UI 픽스 + 밸런싱 즉시 반영

★ 가장 먼저. 개발자 피드백 직접 대응.

### 4-1. 레벨업 카드 텍스트 가독성

**문제**: 카드 배경이 하얀색이거나 매우 밝은 톤이라 흰색 텍스트가 안 보임.

- [ ] `Assets/Scenes/Stage{1-4}_*.unity` 의 `LevelUpPanel/Card{0,1,2}` 자식 4종(NameText/TypeText/LevelText/DescText) 색상 확인
- [ ] 카드 배경 Image 색상 확인. 배경 어두운 톤(`#1A1F2E` 같은 짙은 청흑)으로 변경 또는 텍스트 색을 검정/짙은 색으로
- [ ] 4씬 모두 일괄 적용 — execute_code로 자동화 (Day 43에서 sprite 적용 패턴 참고)

### 4-2. 카피스킬 슬롯 활성화 시 가독성

**문제**: 카피 스킬을 슬롯에 할당하면 UI가 하얀색으로 변하면서 텍스트가 안 보임.

- [ ] `CopySkillHUD` 또는 슬롯 UI 확인 — Q/E/Space 슬롯 GameObject
- [ ] 활성 상태일 때 배경 색상이 흰색으로 바뀌는 코드 위치 찾기 (`Image.color = Color.white` 같은 패턴)
- [ ] 활성 색상을 어두운 톤으로 변경 또는 텍스트 색을 대비되는 색으로

### 4-3. 튜토리얼 패널 — 버튼과 본문 겹침

**문제**: 마지막 줄 "사망 시 모은 세포로 메타 업그레이드 진행. 행운을 빕니다."가 "시작" 버튼과 겹쳐 안 보임.

- [ ] Stage1 Canvas/TutorialPanel/Box 내부:
  - 옵션 A: Box.sizeDelta 높이 560 → **640** (본문/버튼 분리 공간 확보)
  - 옵션 B: Body.offsetMin (50, 110) → (50, **160**) (본문 하단 패딩 ↑, 버튼 영역 회피)
  - 권장: B (Box 크기 유지, 본문 영역만 줄임)
- [ ] 본문 텍스트 줄 수 줄여서 한 화면에 맞추기도 가능 (선택)

### 4-4. 흡혈촉수 — 고정 회복으로 전환

**문제**: 현재 `Heal(damage × 0.1)` → 데미지가 높으면 회복도 높음. 사용자 안: 고정값.

- [ ] `Assets/Scripts/Skills/DrainTentacle.cs`
  - `lifestealRatio` 필드 제거 또는 deprecated 표시
  - 신규: `lifestealFlat = 0.1f` (고정 1회 회복량, 인스펙터 노출)
  - `Drain()` 메서드: `stats.Heal(damage * lifestealRatio)` → `stats.Heal(lifestealFlat)`
- [ ] 4씬 Player의 DrainTentacle 인스턴스에 `lifestealFlat = 0.1` 직렬화 (코드 기본값으로 자동 반영되지만 확인)
- [ ] SkillData 설명 갱신 — "고정 0.1 회복 (10회 = 1HP)"
- [ ] 검증: 1초 1번 발동 × 0.1 = 0.1/s. 체력재생 Lv4(1.5/s)보다 압도적으로 약함 → 부가 효과 수준

### 4-5. 체력재생 패시브 너프

**문제**: Lv4 누적 1.5/s가 사기. 사용자 본인도 어떻게 손볼지 모름 → **두 단계로 접근**:

- [ ] **1단계: 단순 절대값 너프**
  - `Assets/ScriptableObjects/SkillData/CellRegeneration.asset` Lv별 hpRegenDelta
  - 현재: 0.3 / 0.3 / 0.4 / 0.5 (누적 0.3/0.6/1.0/1.5)
  - 신규: **0.15 / 0.15 / 0.2 / 0.25 (누적 0.15/0.3/0.5/0.75)** — 절반
- [ ] **2단계 (옵션, 1단계 후 효과 측정):** 피격 후 3초 회복 차단
  - `PlayerStats.TakeDamage`에 `lastDamageTime = Time.time` 기록
  - `PlayerStats.Update`의 hpRegen 조건에 `Time.time - lastDamageTime > 3f` 추가
  - 효과: 안 맞으면 회복, 맞으면 3초 정지 → 적극적 회피 보상

### 4-6. 의태기관 검증 + 조정

**문제**: 패널티 작동 의심 + 잘 안 쓰게 됨.

- [ ] **패널티 작동 검증** (`MutationManager.ApplyMimicryOrgan`):
  - 디버그 로그 추가: `Debug.Log($"[Mimicry BEFORE] atk={stats.attackPower} spd={stats.moveSpeed}")` + after
  - 실제로 attackPower가 절반으로 줄어드는지 Play로 확인
  - 줄어들지 않는다면 어딘가에서 덮어쓰는 코드 추적 (SkillEffectApplier? PlayerProgressData.Restore?)
- [ ] **밸런싱 조정** (검증 후):
  - 공격력 패널티 ×0.5 → **×0.7** (완화)
  - 무적 주기 60s → **40s**, 무적 시간 3s 유지
  - OR 능동 회피 추가: ESC 같은 추가 입력으로 5초 쿨다운 짧은 대시 (작업량 ↑, 8주차 후반으로 미룰 수도)

### 4-7. HUD 라벨 위치 픽스 (5/27 외부)

**문제**: Day 49에서 슬라이더 좌측(pivot.x=1, x = slider.x - 8)에 라벨을 두었는데, 슬라이더가 화면 가장자리에 있어 라벨이 화면 밖. EXP 슬라이더는 가로 전체라 라벨이 슬라이더 위에 겹침.

- [ ] 4씬 HUD의 `HPSlider_Label`, `EnergySlider_Label`, `ExpSlider_Label` 위치 재배치
- [ ] 안 A: **슬라이더 위쪽** 작은 텍스트 (anchoredPosition.y += sliderHeight + 4)
- [ ] 안 B: 슬라이더 내부 좌측 텍스트로 변경 (Slider 자식으로 이동, 가운데 정렬)
- [ ] 권장: **안 A** (sprite 도입 후 슬라이더 위 라벨이 자연스러움)

### 4-8. VoidPierce 콘솔 에러 (5/27 개발자)

- [ ] `Assets/Scripts/Enemy/HitEffect.cs` PlayFlash 시작에 활성 체크 1줄 추가
  ```csharp
  public void PlayFlash() {
      if (sr == null) return;
      if (!gameObject.activeInHierarchy) return; // ← 추가
      ...
  }
  ```

### 4-9. 발광기관 + 전기뱀장어 필드 검증 (5/27 외부)

**의심**: 보호막이 일렉트릭 필드 데미지에 안 사라짐.

- [ ] `PlayerStats.TakeDamage`에 디버그 로그 추가 (일시):
  ```csharp
  Debug.Log($"[TakeDamage] amount={amount} glowShield={glowShieldActive}");
  ```
- [ ] Stage2 진입 → 발광기관 4Lv → 전기뱀장어 필드 진입 → 첫 tick에서 shield 소모되는지 로그 확인
- [ ] 만약 소모 안 되면 `ElectricEelAura`가 `stats.TakeDamage` 외 다른 경로로 데미지 입히는지 추적
- [ ] 검증 후 디버그 로그 제거

### 4-10. 보스 출몰 시간 단축 (5/27 외부, 피로감 호소)

| 스테이지 | bossSpawnTime | stageDuration |
|---------|--------------|--------------|
| Stage1 | 480 → 그대로 (피드백 없음) | 540 |
| Stage2 | 480 → **300** (5분) | 540 → 360 |
| Stage3 | 480 → **240** (4분) | 540 → 300 |
| Stage4 | 480 → **240** (4분) | 540 → 300 |

- [ ] `Assets/ScriptableObjects/StageData/Stage{2,3,4}_*.asset` 수치 변경
- [ ] 단축에 따른 EXP 곡선 영향 측정 (Day 56)

### 4-11. 3스 일반몹 HP 상향 (5/27 외부)

플레이어 평시 공격력 ~45 / 과부화 ~54 기준 2방에 피 약간 남게.

- [ ] Anglerfish HP 52 → **100**
- [ ] GiantSquid HP 75 → **120**
- [ ] Jellyfish / BalloonEel는 그대로 (자폭 / 원거리)

### 4-12. 4스 LaserSoldier 사거리 ×2 (5/27 외부)

- [ ] `Assets/Scripts/Enemy/EnemyLaserAttack.cs`의 range 또는 `Enemy_LaserSoldier.prefab`의 EnemyLaserAttack.range 인스펙터 값 ×2
- [ ] StopDistance도 같이 ×2 (적이 멀리서 정지 + 발사)

### 4-13. 검증
- [ ] Play 모드로 4-1 ~ 4-3, 4-7 시각 확인 (UI 가독성)
- [ ] 흡혈촉수 1분 사용 → HP 회복량 ≤ 6 확인
- [ ] 체력재생 Lv4 만렙 시 1분간 회복량 ≤ 45 (이전 90)
- [ ] 의태기관 디버그 로그로 패널티 실 적용 확인
- [ ] 4-8 콘솔 에러 사라짐 (VoidPierce 5초 채널링 중)
- [ ] 4-9 발광기관 보호막 + 전기필드 정상 작동
- [ ] 4-10 보스 등장 시간 체감 (피로감 ↓)
- [ ] 4-11 3스 평시 2방, 과부화 시 2방에 피 조금 남는지
- [ ] 4-12 4스 LaserSoldier가 화면 가장자리에서 위협

---

## 5. Day 51 - 에셋 도입 1차: 플레이어/일반 적 sprite

### 5-1. 에셋 조달 전략 (확정)

**결정 사항** (2026-05-27):
- 스타일: **2D 도트 (픽셀 아트)** — 축축·어둡·암울 분위기 + 시야 제한 컨셉과 정합
- PPU: **32** (현재 카메라 ortho 6.5 기준 적정)
- Filter Mode: **Point (no filter)**, Compression: None
- Material: **Sprite-Lit-Default** (URP 2D Light 정합)
- **단일 팩 우선** → 부족분 자유 조합 (sprite-ai.art / Kenny / itch.io)
- 라이선스: 학교 제출용이라 완화 (CC0 우선, CC-BY 출처 표기도 생략 가능 — Steam 배포 시 보수적으로 재검토)

**출처 우선순위**:
1. **Kenny.nl** (CC0) — UI 팩 RPG, 단일 팩 첫 후보
2. **itch.io 도트 팩** — "16x16/32x32 horror/deep sea" 검색. 어두운 호러 톤 풍부
3. **sprite-ai.art** — 부족한 적/보스/캐릭터 sprite 생성. 팔레트 매칭으로 통일감 보정
4. **AI 생성 (Stable Diffusion 또는 sprite-ai.art)** — MainMenu 배경 1장 (좋은 무료 자산 없을 시)
5. **game-icons.net** (CC-BY) — HUD 아이콘 (HP 하트/EXP 별/에너지 번개/세포)

**카피 스킬 아이콘**: 8주차 보류 (텍스트 유지), 9주차 이월

### 5-2. 우선순위 sprite

| 대상 | 현재 | 목표 |
|------|------|------|
| Player | 단색 원/사각 sprite | 캐릭터 (4방향 또는 idle/walk 애니메이션) |
| Enemy_Default/Guard/Shooter (1스) | 단색 sprite | 실험실 적 — 변형된 생체 느낌 |
| Enemy_Piranha/Marlin/ElectricEel/Shark (2스) | 단색 sprite | 어류 (피라냐/청새치/뱀장어/상어) |
| Enemy_Anglerfish/GiantSquid/Jellyfish/BalloonEel (3스) | 단색 sprite | 심해 생물 (낚시아귀/대왕오징어/해파리/풍선뱀장어) |
| Enemy_ReinforcedSoldier/Guard/LaserSoldier/Drone (4스) | 단색 sprite | 인공/기계 (강화 병사/방패/레이저/드론) |

### 5-3. 작업 절차

- [ ] 에셋 임포트: `Assets/Sprites/Player/`, `Assets/Sprites/Enemies/Stage{1-4}/` 폴더 정리
- [ ] Texture Import Settings:
  - Texture Type: Sprite (2D and UI)
  - Pixels Per Unit: 16 또는 32 (일관 유지)
  - Filter Mode: Point (no filter) — 픽셀 아트
  - Compression: None (작은 게임이라 용량 무시 가능)
- [ ] 각 prefab의 SpriteRenderer.sprite 교체:
  - Player: 1개 prefab
  - Enemy_* 14종 + Enemy_Drone (총 15종)
- [ ] 색상 차별화는 sprite 자체에 있으므로 SpriteRenderer.color는 Color.white로 통일
- [ ] 스케일 조정: PPU + sprite 픽셀 크기에 따라 transform.localScale 재계산. 적당한 시각 크기 유지

### 5-4. 보스는 Day 52에서 별도 처리 (sprite 크기/패턴 영향 큼)

---

## 6. Day 52 - 에셋 도입 1차 후속: 보스 sprite + 카피 시각

### 6-1. 보스 sprite 4종
- [ ] ExperimentalSubjects (1스) — 변형 실험체. 크기 중간
- [ ] Whale (2스) — 향유고래. 크기 큼
- [ ] Oarfish (3스) — 산갈치 (긴 형태). **머리 + 5마디 시각 통일성 주의** (각 마디 sprite 필요할 수도)
- [ ] Director (4스) — 연구소장. 인간형/기계형

### 6-2. 산갈치 마디 처리
- [ ] OarfishBody.cs의 segments에 부착된 SpriteRenderer가 부모와 다른 sprite 사용할지 결정
  - 옵션 A: 같은 sprite (단순)
  - 옵션 B: head는 머리 sprite, segment 1~4는 body sprite (꼬리쪽 작아짐), segment 5는 꼬리 sprite (4종 sprite)

### 6-3. 공허관통 재설계 (5/27 외부) ★

**원래 컨셉**: 산갈치의 긴 마디를 카피 → 플레이어 사슬팔이 마디로 뻗어 전기톱처럼 적을 갈아버림. 현재 단순 LineRenderer 빔이라 보는 맛 X.

- [ ] `Assets/Scripts/Skills/VoidPierceSkill.cs`
  - `length` 8 → **16** (사거리 ×2)
  - `width` 1.2 → **2.0** (두께 ↑)
  - LineRenderer → **sprite 체인** 또는 트레일/파티클로 교체
- [ ] 시각: 마디 sprite (5~7개) 가 origin → end 방향으로 일렬 배치, 마디마다 회전/스케일 미세 차이로 살아있는 느낌
  - 또는: 두꺼운 LineRenderer + 표면에 전기 파티클 흩뿌리기
- [ ] 데미지 재밸런싱: 사거리/두께 ↑ → 누적 데미지 ↑. damageMultiplier 0.5 유지하되 hitTicks 누적은 그대로 (시야 확장 효과만 ↑)
- [ ] 사거리 ↑에 따라 자동추적 사거리 제한 영향 — VoidPierce는 카피 스킬이라 시야 제한 영향 안 받음 (현재 시야 제한은 자동 추적 4종 한정)

### 6-4. 카피 스킬 시각 보강 (선택)
- [ ] 다른 카피 스킬도 sprite 기반 시각 검토 (Berserk, Dash, HealingFactor, Ultrasonic, DeepPressure, PredatorCharge, GlowFrenzy, BleedSwim)
- [ ] Day 56 통합 정합 작업에서 함께 처리해도 됨

---

## 7. Day 53 - 에셋 도입 2차: 배경/타일/UI 아이콘

### 7-1. 배경/타일
- [ ] Stage1 Lab: 격자/금속 바닥
- [ ] Stage2 Sea: 바다 그라데이션 또는 산호초
- [ ] Stage3 Deep: 짙은 청-검정 (시야 제한과 정합)
- [ ] Stage4 Ruined: 폐허 연구소 (콘크리트/녹슨 금속)
- [ ] 구현: 각 씬의 카메라 background color 대신 큰 sprite 깔거나, Sprite Atlas + Tilemap 도입

### 7-2. UI 아이콘
- [ ] HP/EXP/세포/에너지 아이콘 (현재 한글 라벨만)
- [ ] 카피 스킬 슬롯 아이콘 (Q/E/Space 키 + 스킬 아이콘)
- [ ] 스킬 카드 아이콘 (12종 일반 + 9종 카피 = 21개) — 작업량 큼, 우선순위 낮음

---

## 8. Day 54 - 사운드 자산 도입

7주차에서 시스템(AudioManager)만 구축, 자산 미할당. 이번엔 자산 도입.

### 8-1. BGM 5종
- [ ] MainMenu: 차분/긴장감
- [ ] Stage1 Lab: 실험실/긴장
- [ ] Stage2 Sea: 모험/해양
- [ ] Stage3 Deep: 어두운/공포
- [ ] Stage4 Ruined: 전투/전자

### 8-2. SFX 6종 (이미 hook 위치 정의됨)
- [ ] PlayerHit: 둔탁한 타격
- [ ] EnemyDeath: 작은 폭발/squelch
- [ ] LevelUp: 밝은 chime
- [ ] CopySkillCast: woosh
- [ ] BossPhase2: 저음 충격
- [ ] UIClick: tick

### 8-3. 자산 출처 (Day 47 노트와 동일)
- freesound.org CC0
- opengameart.org
- Pixabay Audio CC0
- Unity Asset Store 무료

### 8-4. 할당 방법
- [ ] `Assets/Audio/BGM/`, `Assets/Audio/SFX/` 임포트
- [ ] `Assets/Prefabs/Managers/[Managers].prefab` 열기 → AudioManager 자식 → 인스펙터에서 슬롯에 드래그

---

## 9. Day 55 - 통합 정합 작업

에셋 도입 후 깨질 가능성 큰 부분 일괄 점검.

- [ ] sprite 크기에 맞춘 Collider2D radius/size 재조정
  - Player CircleCollider, Enemy CircleCollider, Boss CircleCollider 모두
- [ ] SpriteRenderer.color가 게임 로직(예: hit flash, 자폭 점멸)과 정합되는지
  - HitEffect.PlayFlash가 흰색 점멸인데 sprite 도입 후 어색하면 색 조정
- [ ] Day 48의 카메라 효과 코드 재활성 검토
  - CameraEffect/HitStop/HitEffectGlobal 코드는 남아있음. sprite 기반이 되면 화면 흔들림이 덜 어지러울 수 있음
  - 사용자 의견 반영
- [ ] 배경 도입 시 UI 가독성 재확인 (HUD/카드 텍스트가 새 배경 위에서 잘 보이는지)

---

## 10. Day 56 - 추가 밸런싱 + 동적 스폰 패턴

에셋 도입으로 체감 변화 가능. 재측정 + 조정. 추가로 **시간 곡선 스폰 시스템** 신설.

### 10-1. 재측정
- [ ] 풀 사이클 1회 측정 (각 스테이지 레벨/시간/사망 수)
- [ ] Day 50 1차 너프(흡혈촉수/체력재생)가 너무 약하지 않은지
- [ ] 의태기관 사용 비율 (다른 돌연변이 대비 선택률)
- [ ] 신규 sprite 도입 후 적이 너무 작아 보이거나 큰 경우 collider/속도 재조정

### 10-2. 동적 스폰 패턴 (★ 핵심 디자인 변경)

**문제**: 현재 일정 강도 스폰 → 위기감 곡선 없음. 사용자 의견 "위기감 없이 지나가는 시간이 많다".

**목표 곡선** (보스 출몰 시간 기준):
```
0~33%       : intensity 1.0 (평소 — 현재 수치)
33~66%      : 1.0 → 1.3 선형 증가
66~100%     : 1.3 → 1.8 가속 (보스 직전 물량 공세 — "위기")
100%+ (보스): 1.3 (보스에 집중 위해 살짝 ↓)
```

**구현 (옵션 A 먼저, 동작 후 B로 리팩토링 옵션)**:
- 옵션 A: `DifficultyManager`에 시간 비율 기반 곡선 코드 (단순)
- 옵션 B: `[SerializeField] AnimationCurve intensityCurve;` 스테이지별 다른 곡선
- 옵션 C: BossSpawner 이벤트 기반 (단순하지만 곡선 표현 떨어짐)

**A안 작업 절차**:
- [ ] `DifficultyManager.Update`에 `float elapsed`, `float bossSpawnTime` 참조
- [ ] `float t = Mathf.Clamp01(elapsed / bossSpawnTime)`
- [ ] intensity 계산:
  ```csharp
  float intensity;
  if (t < 0.33f) intensity = 1f;
  else if (t < 0.66f) intensity = Mathf.Lerp(1f, 1.3f, (t - 0.33f) / 0.33f);
  else if (t < 1f)    intensity = Mathf.Lerp(1.3f, 1.8f, (t - 0.66f) / 0.34f);
  else                intensity = 1.3f; // 보스전
  ```
- [ ] `CurrentMaxActiveScale = intensity`, `CurrentIntervalScale = 1f / intensity` (반비례)
- [ ] BossSpawner.OnBossSpawned 구독 → 보스 등장 후 5~10s간 추가로 ×0.7 적용 (한숨 돌리기) 후 다시 ×1.3

### 10-3. 위기감 신호 (선택)
- [ ] 보스 등장 20s 전 HUD 경고 ("BOSS INCOMING") + 화면 가장자리 빨강 vignette
- [ ] 위기 단계(66%+) 진입 시 BGM 변화 (사운드 자산 있을 때)

### 10-4. 검증
- [ ] 4스테이지 모두 보스 직전 30초가 명확히 어려움
- [ ] 보스 등장 직후 한숨 돌릴 시간 있음
- [ ] 보스전 중반부터 일반 적 다시 ↑ (Day 45 후속에서 결정된 동시 압박 유지)

---

## 11. Day 57 - 외부 피드백 반영

2차 빌드 (Day 49 산출물 `Build/Windows_Release/`) 외부 전달 후 받은 피드백을 §2-2에 기록 + 일괄 반영.

피드백 양에 따라 1~3일로 늘릴 수 있음.

---

## 12. Day 58 - UI 폴리싱 잔존 + 3차 빌드

### 12-1. 잔존 UI
- [ ] 카피 스킬 슬롯 아이콘 (Day 53에서 못 한 경우)
- [ ] 스킬 카드 아이콘
- [ ] 메뉴 디자인 추가 폴리싱

### 12-2. 3차 Windows Release 빌드
- [ ] `Build/Windows_Release_v3/Project_Abyss.exe`
- [ ] 빌드 크기 비교 (2차 203 MB → ?)
- [ ] 외부 테스터에 3차 빌드 전달

### 12-3. 8주차 통합 테스트
- [ ] 풀 사이클 (메뉴 → 1~4스 → 엔딩 → 메뉴)
- [ ] 모든 sprite 정상 표시
- [ ] BGM/SFX 정상 재생
- [ ] UI 가독성 OK
- [ ] 밸런스 잔존 문제 없음

---

## 13. 8주차 외 (9주차 이후 후보)

### 디자인 보강 (계획서 §12에서 이월)
- 스킬 해금 시스템 (특정 스킬은 메타로 해금)
- 더 많은 돌연변이 (현재 5종 → 8~10종)
- 일별 챌린지 / 시드 시스템
- 보스 페이즈3
- 새 스테이지 (Stage5?)

### 시스템
- 설정 메뉴 (해상도, 풀스크린, 키 바인딩, 볼륨 슬라이더)
- 세이브 데이터 변조 방지 (JSON + 해시)
- 통계 화면 (런별 클리어 시간, 처치 수, 사망 원인)
- 도전 과제 / 업적

### 폴리싱
- 로컬라이제이션 (영문)
- Steam 배포 준비
- 추가 카피 스킬 9종 → 12~15종

---

## 14. 주의할 점

### 14-1. 작업 순서
- **Day 50을 먼저** — UI 가독성 픽스가 안 되면 다른 작업 테스트도 어려움
- 에셋 도입은 **한 스테이지 단위로 단계적** 권장. 1스만 먼저 sprite 교체하고 검증 → 나머지 확장
- 사운드(Day 54)는 시스템 변동 없음 → 다른 작업과 병렬 가능

### 14-2. 에셋 라이센스
- **학교 제출용으로 라이선스 완화** (사용자 결정 2026-05-27). CC-BY 출처 표기도 생략 가능
- Steam 배포 결정 시 재검토 (AI 생성물 / CC-BY 출처 / Asset Store 재배포 등)

### 14-3. 밸런싱 패턴
1차(Day 50) 단순 너프 → 측정 → 부족하면 2차(Day 56) 구조적 변경. 한 번에 큰 변화 X.

### 14-4. 에셋 도입 시 깨지기 쉬운 것
- 시야 제한 Light2D — Sprite 도입 후 Sprite-Lit material 사용 여부 확인 (URP 2D Lit)
- HitEffect.PlayFlash 색상 — 새 sprite에 색 변경 효과 어울리는지
- Collider2D radius — sprite 크기와 시각 일치 여부

### 14-5. 새 세션에서 작업 시작 시 체크
1. 현재 어느 Day까지 완료됐는지 → `memory/project_progress.md` 또는 `Summary/` 확인
2. 작업할 Day의 의존성 충족 여부 (§3 표)
3. 에셋 도입 단계라면 임포트된 자산 위치 확인 (`Assets/Sprites/`, `Assets/Audio/`)
4. 작업 후 검증 항목 (각 Day 마지막 `### X-Y. 검증` 절)
