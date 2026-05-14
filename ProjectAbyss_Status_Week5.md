# Project: Abyss - 5주차 TODO

작성일: 2026-05-10
최종 업데이트: 2026-05-18 (Day 32까지 완료 반영)

## 1. 5주차 목표

5주차의 핵심 목표는 1스테이지 프로토타입을 넘어, 스테이지 진행 구조와 2스테이지 기반을 연결하는 것이다. Day 28 통합 검증에서 발견된 페이스 이슈도 함께 보완한다.

### 진행 현황

**완료된 Day**

- ✅ Day 28: 4주차 통합 검증 (페이스 이슈 발견)
- ✅ Day 28.5: 페이스 보완 (적 스폰 가속 + 적 강화 + 만렙 보상 카드)
- ✅ Day 29: 스테이지 전환 실제 연결
- ✅ Day 30: StageData 기반 스테이지 설정
- ✅ Day 31: 플레이어 진행 데이터 보존
- ✅ Day 32: 2스테이지 기본 구성 (씬 분리 + 적 5종 + 원거리/오라 시스템)

**남은 Day**

- ⏳ Day 33: 향유고래 보스 + E 슬롯 카피 스킬
- ⏳ Day 34: 5주차 통합 테스트

### 완성된 핵심 흐름

```text
1스테이지 보스 처치
→ 카피 스킬 선택
→ 검은 화면 페이드 아웃 + "해저" 자막
→ Stage2_Sea 씬 자동 로드
→ 페이드 인 (PlayerProgressData가 HP/레벨/스킬/카피/돌연변이/세포 모두 복원)
→ 2스테이지 시작 (압력 자동 활성화)
```

## 2. 사전 결정 사항

5주차 작업 시작 전에 확정해 둔 항목.

### 2-1. 스테이지 분리 방식: 씬 단위

4스테이지를 각각 독립 씬으로 분리한다.

- ✅ `Stage1_Lab.unity` (구 `GameScene.unity` rename)
- ✅ `Stage2_Sea.unity` (Day 32 신규)
- ⏳ `Stage3_Deep.unity` (6주차 이후)
- ⏳ `Stage4_Ruined.unity` (6주차 이후)

`PlayerProgressData`가 `DontDestroyOnLoad`로 진행 데이터를 다음 씬으로 운반한다.

### 2-2. 스테이지별 압력 파라미터

`StageData` 에셋에 입력된 점진 강화 디자인.

| 스테이지 | pressureEnabled | startY | maxY | movePenalty | atkPenalty | expBonus |
|---------|-----------------|--------|------|-------------|------------|----------|
| 1 (Lab) | false | (무관) | (무관) | (무관) | (무관) | (무관) |
| 2 (Sea) | true  | -5 | -30 | 0.20 | 0.20 | 0.30 |
| 3 (Deep)| true  | -5 | -40 | 0.35 | 0.35 | 0.50 |
| 4 (Ruined) | false | (무관) | (무관) | (무관) | (무관) | (무관) |

### 2-3. PlayerProgressData 정책

- 스테이지 간 영구 데이터 운반용. 메타 업그레이드용 세이브 파일(세포 영구 소비)과는 별개의 시스템이다.
- 최초 생성: 1스테이지 시작 씬에 GameObject로 미리 배치. `[Managers]` 그룹 안에 둬도 자동으로 root로 reparent됨.
- 첫 씬 진입 시 자동으로 `DontDestroyOnLoad` 처리.
- `SceneManager.sceneLoaded` 구독으로 다음 씬에서 자동 Restore 트리거.
- GameOver 시 `ResetAll()` 호출하여 진행 데이터 초기화. 메타 재화(세포)는 별도 시스템에서 보존 예정.

### 2-4. 카피 스킬 ID 확장 시점

`CopySkillID` enum 확장은 Day 33 (2스테이지 보스 카피 스킬 작성) 시작 시점에 한다.

### 2-5. 스테이지 타이밍 단축

Day 28 검증 결과 1스테이지가 8~9분으로 너무 길어 후반 인플레이션이 발생함. 모든 스테이지의 타이밍을 단축했다.

| 항목 | 기존 | 변경 |
|------|------|------|
| stageDuration (1~2스테이지) | 540s (9분) | **420s (7분)** |
| bossSpawnTime (1~2스테이지) | 480s (8분) | **360s (6분)** |
| stageDuration (3~4스테이지) | 420s (7분) | **360s (6분)** |
| bossSpawnTime (3~4스테이지) | 360s (6분) | **300s (5분)** |

### 2-6. 스킬 풀 확장은 6주차로

일반 스킬 7종이 4스테이지 분량 대비 부족하지만 5주차 일정 보호 차원에서 6주차로 분리.

추가 후보:
- 공격: 음파 진동, 가시 발사, 흡혈 촉수
- 패시브: 압력 적응, 발광 감각

## 3. 스테이지 전환 흐름 (구현 완료)

1. ✅ `CopySkillSelectCardUI.OnClick()`
2. ✅ `CopySkillManager.AssignSkill()`
3. ✅ `CopySkillSelectPanel.Hide()`
4. ✅ `GameManager.TriggerStageClear()`
5. ✅ `StageManager.TransitionToNext()`
6. ✅ `PlayerProgressData.Capture()` — 모든 매니저에서 데이터 수집
7. ✅ `StageTransitionUI.PlayFadeOut(nextStage.displayName, callback)`
8. ✅ 검은 화면 (1.0s) + 자막 페이드인 (0.3s) + hold (1.5s)
9. ✅ `SceneManager.LoadScene(nextStage.sceneName)`
10. ✅ 새 씬 sceneLoaded → 1프레임 대기 후 `PlayerProgressData.Restore()` + `StageTransitionUI.PlayFadeIn()`

## 4. Day 28 - 4주차 통합 검증 (완료)

### 4-1. 핵심 흐름 검증

- [x] 처음부터 보스 처치까지 전체 플레이 테스트
- [x] 레벨업 UI 동작 확인
- [x] Slash가 시작 시 Lv1로 잡히고, 레벨업 선택지에서 `Lv.1 -> 2`로 표시되는지 확인
- [x] 보스 HP바 표시/갱신 확인
- [x] 보스 페이즈2 색상 + 펄스 + 플래시 피드백 확인
- [x] 보스 페이즈2에서 피격 시 페이즈2 톤이 유지되는지 확인
- [x] 보스 돌진 전 노란 경고 색상 확인
- [x] 보스 사망 시 일반 적이 모두 비활성화되는지 확인
- [x] 카피 스킬 선택 UI 정상 표시 확인
- [x] 카피 스킬 선택 후 StageClear 진입 확인

### 4-2. 카피 스킬 / 생체 에너지

- [x] 카피 스킬 슬롯 HUD가 우측 하단에 정상 표시되는지 확인
- [x] 슬롯에 스킬 장착 시 이름과 비용이 표시되는지 확인
- [x] 에너지 충분 시 밝게, 부족 시 어둡게 표시되는지 확인
- [x] HealingFactor가 체력 만피일 때 사용 불가로 표시되는지 확인
- [x] Dash 사용 중 0.2초 동안 어둡게 표시되는지 확인
- [x] 생체 에너지 획득/소비가 정상 반영되는지 확인
- [x] 에너지 부족 피드백 텍스트 표시 확인

### 4-3. 압력 시스템

- [x] 압력 슬라이더가 화면 우측 세로형으로 표시되는지 확인
- [x] 인스펙터에서 isActive=true로 두고 Y 이동 시 슬라이더가 차오르는지 확인
- [x] 압력 활성화 시 이동/공격 속도 페널티 체감 확인
- [x] 압력 활성화 시 경험치 획득량 증가 체감 확인
- [x] 압력 저항 0.5 설정 시 페널티만 절반으로 감소하고 경험치 보너스는 유지되는지 확인

### 4-4. 돌연변이 5종 체감 검증

- [x] mutationPool에 5종 모두 등록되어 있는지 인스펙터 확인
- [x] triggerLevels = [7, 12] 인스펙터 확인
- [x] 7레벨, 12레벨 도달 시 돌연변이 선택 패널이 뜨는지 확인
- [x] 중복 선택 방지 동작 확인
- [x] 과부화 — 공격력 ↑, 최대 HP ↓ 체감
- [x] 과성장 촉수 — 스킬 범위 ↑, 이동속도 ↓ 체감
- [x] 의태 기관 — 이속 ×2, 공격력 절반, 60초마다 3초 무적 동작 확인
- [x] 감각 붕괴 — 공속 ×2 체감, 5% 확률 스턴이 너무 잦지 않은지 확인
- [x] 독성 과부화 — PoisonNeedle/ElectricEngine 데미지 ↑, Slash/BioticExplosion 데미지 + 범위 ↓ 체감

### 4-5. 테스트 모드 정리

- [x] `PressureSystem.isActive` — StageData 자동 연결로 운용
- [x] `BioEnergyManager.testMode` = false
- [x] `CopySkillManager.testMode` = false
- [x] `BossSpawner.bossSpawnTime` — 단축 테스트값에서 본 값으로 복귀
- [x] `MutationManager` 트리거 레벨 [7, 12] 확인

### 4-6. 마무리

- [x] 콘솔 에러 / 경고 확인
- [x] 4주차 작업 커밋

### 4-7. 검증 결과 메모

**플레이 결과 (압력 100% 켠 상태)**
- 보스 등장 직전 레벨 25 도달
- 모든 일반 스킬 만렙 (Lv4)
- 보스 1초만에 처치

**발견된 페이스 이슈**
1. 적 스폰량이 시간에 따라 증가하지 않아 후반부 긴박감 부족
2. 스테이지 8~9분이 너무 길어 후반 인플레이션 발생
3. 스킬 풀(7종)이 4스테이지 분량 대비 부족
4. 만렙 도달 후 추가 레벨업 보상 부재

**대응 일정**
- 이슈 1, 2, 4: Day 28.5 페이스 보완에서 처리
- 이슈 3: 6주차 별도 일정으로 분리

## 5. Day 28.5 - 페이스 보완 (완료)

### 5-1. 적 스폰 가속화 (EnemySpawner) — 완료

- [x] `DifficultyManager` 신규 매니저 생성 (시간 기반 4종 스케일 통합)
- [x] `EnemySpawner`에 spawn interval / maxActiveEnemies 스케일 적용
- [x] peakTime은 BossSpawner.bossSpawnTime과 동기화 (보스 등장 시점이 절정)
- [x] 인스펙터 노출: intervalMultiplierPeak=0.4, maxActiveMultiplierPeak=2.0

### 5-2. 적 자체 강화 (EnemyBase) — 완료

- [x] `DifficultyManager`의 CurrentHpScale / CurrentDamageScale 사용
- [x] `EnemyBase.OnEnable()`에서 스폰 시점의 스케일로 scaledMaxHp / scaledContactDamage 캐시
- [x] 인스펙터 노출: hpMultiplierPeak=2.0, damageMultiplierPeak=1.5
- [x] 이동속도는 그대로 유지 (회피 가능성 보존)
- [x] 씬에 `[Managers]/DifficultyManager` 게임오브젝트 배치

### 5-3. 만렙 보상 (LevelManager) — 완료

원래 계획은 즉시 지급 (세포 +5 + HP 15% 회복)이었으나, 검증 중 디자인 변경: **회복 vs 세포 카드 선택 방식**으로 변경.

- [x] `LevelManager`에 만렙 도달 후 레벨업 분기 추가 (`IsAllSkillsMaxed()` 헬퍼)
- [x] 다중 레벨업 큐 처리 (`pendingLevelUps` 카운터)
- [x] `MutationManager`도 `pendingOffers` 큐로 변경 (동시 트리거 안전)
- [x] `MaxLevelRewardCardUI` + `MaxLevelRewardPanel` 신규 컴포넌트
- [x] 카드 디자인:
  - **세포 강화** — 현재 HP의 30% 회복
  - **세포 추출** — 세포 +5 누적
- [x] HUD에 CellsText 카운터 + MaxLevelRewardText 피드백 추가
- [x] 카드 선택 후 자동으로 펜딩 레벨업 카드 연쇄 표시
- [x] `CurrentState` 기반 판단으로 중첩 ChangeState 데드락 방지

**검증 메모**
- 압력 0 상태에서도 28레벨 도달 (스폰 가속 효과)
- 난이도는 여전히 쉬운 편이지만, 단독 밸런싱보다 전체 스테이지 완성 후 일괄 조정이 효율적이라 판단
- 추가 밸런싱은 6주차 또는 4스테이지 완성 후로 미룸

## 6. Day 29 - 스테이지 전환 실제 연결 (완료)

### 6-1. StageTransitionUI 풀 구현

- [x] 풀스크린 검은 Image + CanvasGroup 게임오브젝트 생성 (별도 Canvas, sortingOrder=999)
- [x] 중앙 TMP_Text 자막 게임오브젝트 생성
- [x] `StageTransitionUI` 컴포넌트에 두 참조 연결
- [x] `PlayFadeOut()` 코루틴 실제 구현 (alpha 0 -> 1, 자막 페이드인, hold)
- [x] `PlayFadeIn()` 코루틴 실제 구현 (alpha 1 -> 0, 자막 비활성)
- [x] `unscaledDeltaTime` 사용으로 `Time.timeScale = 0` 상태에서도 동작 확인
- [x] 페이드 중 입력/클릭 차단 확인 (`blocksRaycasts = true`)
- [x] `SceneManager.sceneLoaded` 구독으로 새 씬 진입 시 자동 페이드인

### 6-2. 호출 연결

- [x] `StageManager.TransitionToNext()`에서 `StageTransitionUI.PlayFadeOut()` 호출
- [x] `CopySkillSelectCardUI.OnClick()` 마지막에 `StageManager.TransitionToNext()` 호출 추가
- [x] StageClear 패널 표시 정책 결정: 별도 패널 없이 페이드만 사용
- [x] `GameOverPanel`이 StageClear에 반응 안 하도록 수정 (GameOver 전용으로 변경)
- [x] 전환 후 `GameManager` 상태가 정상적으로 `Playing`으로 복귀하는지 확인

### 6-3. 검증

- [x] 1스테이지 보스 처치 → 카피 스킬 선택 → 페이드 → 자막 → 페이드 인 흐름이 끊김 없이 동작
- [x] 자기 자신 씬으로 다시 로드되어도 매니저 중복 생성이 없는지 확인

## 7. Day 30 - StageData 기반 스테이지 설정 (완료)

### 7-1. StageData 에셋 생성

- [x] `Stage1_Lab.asset` (stageNumber=1, sceneName="Stage1_Lab", displayName="해저 연구소")
- [x] `Stage2_Sea.asset` (stageNumber=2, sceneName="Stage2_Sea", displayName="해저")
- [x] `Stage3_Deep.asset` (stageNumber=3, sceneName="Stage3_Deep", displayName="심해")
- [x] `Stage4_Ruined.asset` (stageNumber=4, sceneName="Stage4_Ruined", displayName="파괴된 연구소")
- [x] `nextStage` 체인 연결: 1 → 2 → 3 → 4 → null

### 7-2. 스테이지별 파라미터 입력

- [x] 1~2스테이지: stageDuration=420s, bossSpawnTime=360s
- [x] 3~4스테이지: stageDuration=360s, bossSpawnTime=300s
- [x] 압력 파라미터를 사전 결정 2-2 표대로 입력
- [x] 4스테이지는 압력 비활성화로 결정

### 7-3. 시스템 연동

- [x] `GameManager`가 `StageData.stageDuration`을 읽도록 변경
- [x] `BossSpawner`가 `StageData.bossSpawnTime`을 읽도록 변경
- [x] `DifficultyManager`가 `StageData.bossSpawnTime`과 peakTime 자동 동기화
- [x] `StageManager`에 `Stage1_Lab.asset`을 startingStage로 설정
- [x] `BossSpawner` / `DifficultyManager`에 `overrideStageData` 토글 추가 (테스트 단축용)
- [x] 1스테이지 씬 Play 시 압력이 비활성, StageData 값이 자동 적용되는지 확인

## 8. Day 31 - 플레이어 진행 데이터 보존 (완료)

### 8-1. 영구 데이터 항목 정의

- [x] currentHp, pressureResistance 저장
- [x] CurrentLevel, CurrentExp, ExpToNextLevel 저장
- [x] 일반 스킬 레벨 — `Dictionary<SkillData, int>` 참조 기반 (ScriptableObject 참조 유지)
- [x] 카피 스킬 슬롯 — `CopySkillData` 참조 자체 저장 (Q/E/Space 3개)
- [x] 선택한 돌연변이 `List<MutationID>` 저장
- [x] 세포 카운터 (`CurrentCells`) 보존

### 8-2. Capture / Restore 구현

- [x] `LevelManager`에 `GetSkillLevelsCopy()`, `SetState(...)` 메서드 추가
- [x] `SetState`에서 각 스킬 Lv1~targetLevel까지 `OnSkillSelected` 순차 발행 (SkillEffectApplier의 누적 delta 효과 재적용)
- [x] `CopySkillManager`에 `RestoreSlot(slot, data)`, `FindSkillComponent()` 추가
- [x] `MutationManager`에 `GetOwnedIDs()`, `RestoreOwnedIDs()` 추가
- [x] `PlayerProgressData.Capture()` 실제 구현
- [x] `PlayerProgressData.Restore()` 실제 구현 — 순서: 스킬→돌연변이→HP Clamp→카피슬롯

### 8-3. 라이프사이클

- [x] 첫 씬에 `PlayerProgressData` GameObject 배치 (`[Managers]` 안)
- [x] `Awake`에서 자식이면 root로 자동 reparent (DontDestroyOnLoad 안전)
- [x] `SceneManager.sceneLoaded` 구독 → 1프레임 대기 후 Restore (모든 매니저 Awake 완료 보장)
- [x] `GameManager.RestartGame()` → `StageManager.StartNewGame()` 흐름
- [x] `StageManager.StartNewGame()`에서 `ResetAll()` 호출 + 첫 스테이지로
- [x] BuildSettings에 씬 없으면 현재 씬 fallback

### 8-4. 진행 중 발견한 이슈와 수정

- **이슈**: 다중 레벨업 시 두 번째 카드 표시 시 timeScale이 잠시 1로 새는 현상
- **원인**: `ChangeState`에서 이벤트 발행 후 switch (timeScale)를 처리하는 순서 문제
- **수정**: switch를 먼저 처리하고 이벤트 발행을 나중에 → 중첩 호출 시 atomic 보장

### 8-5. StageManager / StageTransitionUI 정리

- [x] `StageManager.Awake`에 자식 root reparent 패턴 추가 → `[Managers]` 안에 둬도 자동 root 이동
- [x] `StageTransitionUI.Awake`에도 동일 패턴 적용
- [x] Stage1_Lab 씬에서 StageManager가 `[Managers]` 안에 정리됨

## 9. Day 32 - 2스테이지 기본 구성 (완료)

### 9-1. 씬 명명 정리 및 생성

- [x] `GameScene.unity` → `Stage1_Lab.unity` rename
- [x] `Stage1_Lab.unity` 복제 → `Stage2_Sea.unity` 생성
- [x] Stage2_Sea에서 DontDestroyOnLoad 매니저 3종 제거 (StageManager, StageTransitionUI, PlayerProgressData)
- [x] BuildSettings에 Stage1_Lab, Stage2_Sea 등록

### 9-2. 2스테이지 화이트박스 차별화

- [x] Stage2_Sea 카메라 배경색을 짙은 청록(`#0D2640`)으로 변경 (해저 분위기)
- [ ] 맵/Y범위 본격 디자인 (6주차 이후)

### 9-3. 적 시스템 신규 컴포넌트

- [x] `EnemyProjectile` — 적 투사체 (PlayerStats 대상 데미지)
- [x] `EnemyRangedAttack` — 사거리 안에서 발사, `StopDistance` 노출
- [x] `EnemyAuraDamage` — 반경 내 지속 피해 (전기뱀장어용, 재사용 가능)
- [x] `EnemyAI` 거리 유지 분기 추가 (rangedAttack 있으면 후퇴/정지/추적)

### 9-4. 1스테이지 Enemy_Shooter 정리

- [x] `Enemy_Shooter.prefab`에 `EnemyRangedAttack` 부착 (마취총 컨셉: attackRange=7, damage=5, interval=2.5s)
- [x] `EnemyProjectile.prefab` 생성 (주황색 작은 원)

### 9-5. 2스테이지 적 데이터/프리팹

디자인 문서 기준 2스테이지: 피라냐, 청새치, 상어, **전기뱀장어**.

- [x] `EnemyData_Piranha.asset` (HP 25, 이속 3.0)
- [x] `EnemyData_Marlin.asset` (HP 15, 이속 4.5 — 빠른 청새치)
- [x] `EnemyData_Shark.asset` (HP 40, 이속 4.0 — 강한 상어)
- [x] `EnemyData_ElectricEel.asset` (HP 25, 이속 3.2, 약간 빠름)
- [x] 4종 prefab 생성 + 색상 차별화
- [x] `Enemy_ElectricEel`에 `EnemyAuraDamage` 부착 (반경 1.5, 초당 3 피해, 0.5s tick)
- [x] **3스테이지용 풍선장어 자산 보존** — `Enemy_BalloonEel.prefab` + `EnemyData_BalloonEel.asset` (Day 33 이후 활용)

### 9-6. Stage2 EnemySpawner 웨이브

- [x] Wave 0: Piranha (0s, 1.8s interval) — 기본 추적
- [x] Wave 1: Marlin (60s, 2.5s interval) — 빠른 청새치
- [x] Wave 2: ElectricEel (90s, 3.5s interval) — 오라 지속 피해
- [x] Wave 3: Shark (120s, 5s interval) — 강력한 상어

### 9-7. 검증 결과

- [x] 2스테이지 진입 시 PressureSystem 자동 활성화 확인
- [x] 압력 페널티 체감 (이동/공격속도 -20%, 경험치 +30%)
- [x] Q 슬롯 (1스테이지에서 획득한) 카피 스킬 사용 가능 확인
- [x] 플레이어 HP/레벨/스킬레벨/돌연변이/세포 모두 1스테이지에서 이어짐
- [x] Enemy_Shooter 원거리 발사 및 거리 유지 동작 확인
- [x] 전기뱀장어 오라 지속 피해 동작 확인

### 9-8. 알려진 동작 (의도)

- **Stage2를 시작 씬으로 직접 Play 시 StageManager 없음** → BossSpawner가 인스펙터 값 사용
- 실 게임플레이는 항상 Stage1부터 시작이므로 문제 없음
- 테스트 단축 필요 시 `BossSpawner.overrideStageData=true` 활용

## 10. Day 33 - 2스테이지 보스 / 카피 스킬 초안

향유고래 보스와 2스테이지 카피 스킬 3종 추가. 시간이 빡빡하면 보스 패턴은 단순한 형태로 시작한다.

### 10-1. CopySkillID 확장

- [ ] `CopySkillID` enum에 2스테이지 카피 스킬 3종 추가 (Ultrasonic, DeepPressure, PredatorCharge)
- [ ] `CopySkillManager.FindSkillComponent()` 갱신
- [ ] 새 카피 스킬 컴포넌트 3종을 Player 오브젝트에 추가 (비활성 상태로 시작)

### 10-2. 향유고래 보스

- [ ] `BossData_Whale.asset` 생성 (HP 많음, 이속 느림)
- [ ] `Boss_Whale.prefab` 생성 (BossBase, BossAI, HitEffect, BossPhaseEffect 부착)
- [ ] 기본 추적/접촉 피해 동작 확인
- [ ] 초음파 공격 — 1차 구현은 단순한 부채꼴 즉발 공격으로 시작 (풀 패턴은 6주차)
- [ ] 페이즈2 시 이동속도 증가 + 추가 패턴 여부 결정

### 10-3. 2스테이지 카피 스킬 데이터

- [ ] `CopySkill_Ultrasonic.asset` (E 슬롯, 부채꼴 대피해, 140E)
- [ ] `CopySkill_DeepPressure.asset` (E 슬롯, 주변 디버프, 80E)
- [ ] `CopySkill_PredatorCharge.asset` (E 슬롯, 직선 돌진, 90E)
- [ ] BossData_Whale의 copySkillOptions에 3종 등록, copySkillSlot=1

### 10-4. Stage2_Sea에 보스 배치

- [ ] BossSpawner 인스펙터에 Boss_Whale prefab 할당
- [ ] BossHPBar가 향유고래 데이터로 정상 표시되는지 확인

### 10-5. 검증

- [ ] 2스테이지 보스 처치 후 카피 스킬 선택 패널이 E 슬롯용으로 뜨는지 확인
- [ ] 선택한 카피 스킬이 E 슬롯 HUD에 표시되는지 확인
- [ ] E 키 눌렀을 때 정상 발동, 에너지 소비, 사용 가능 여부 표시 정상 동작

## 11. Day 34 - 5주차 통합 테스트

1스테이지부터 2스테이지 카피 스킬 사용까지의 전체 흐름을 끊김 없이 검증한다.

### 11-1. 흐름 검증

- [ ] 1스테이지 시작 → 보스 처치 → Q 슬롯 카피 스킬 선택
- [ ] 페이드 아웃 → "해저" 자막 → 페이드 인 → 2스테이지 시작
- [ ] 2스테이지 진입 직후 HUD 상태 확인 (HP/EXP/레벨/카피슬롯/돌연변이/세포)
- [ ] 2스테이지에서 압력 슬라이더 정상 동작
- [ ] Q 슬롯 카피 스킬 사용 가능 확인
- [ ] 2스테이지 보스 처치 → E 슬롯 카피 스킬 선택
- [ ] (시간 허용 시) 페이드 → 3스테이지 진입 시도 (Stage3_Deep 씬 없으면 fallback)

### 11-2. 데이터 보존 검증

- [ ] 플레이어 HP / 최대 HP / 압력 저항 보존
- [ ] 레벨 / 경험치 보존
- [ ] 일반 스킬 레벨 보존 (스킬 효과 누적 재적용 확인)
- [ ] 카피 스킬 슬롯 보존 (Q는 1스테이지 것 그대로)
- [ ] 선택한 돌연변이 보존
- [ ] 세포 카운터 보존

### 11-3. 페이스 검증

- [ ] 1스테이지가 7분 안에 적당한 난이도로 마무리되는지 확인
- [ ] 적 스폰 가속이 양쪽 스테이지에서 모두 동작하는지 확인
- [ ] 보스 처치 시간이 1초가 아닌 진짜 전투 시간이 되는지 확인
- [ ] 만렙 후 보상 카드(회복/세포)가 두 스테이지 동안 정상 동작하는지 확인

### 11-4. 환경 검증

- [ ] 1스테이지에서 압력 비활성, 2스테이지에서 활성 확인
- [ ] 콘솔 에러 / 경고 확인
- [ ] 페이드 전환 중 UI 겹침 / 잔상 확인
- [ ] DontDestroyOnLoad 오브젝트 중복 생성 없는지 확인 (StageManager, StageTransitionUI, PlayerProgressData)

### 11-5. 마무리

- [ ] 불필요한 디버그 로그 정리
- [ ] 테스트용 단축값 본 값으로 복귀:
  - [ ] `BossSpawner.overrideStageData` = false
  - [ ] `DifficultyManager.overrideStageData` = false
  - [ ] BioEnergy/CopySkill testMode 모두 false
- [ ] 5주차 작업 커밋

## 12. 6주차로 미루는 작업

다음 항목은 5주차 일정에서 의도적으로 제외했다. 6주차 또는 그 이후에 별도 일정으로 다룬다.

- **스킬 풀 확장** — 공격 스킬 2~3종, 패시브 1~2종 추가 (디자인 + 구현 + 밸런싱 묶음)
- **메타 업그레이드 시스템** — 세포 사용 화면, 영구 강화 옵션 트리
- **향유고래 풀 패턴** — 초음파 광역 + 페이즈2 추가 패턴
- **3, 4스테이지 보스/적/카피 스킬 본격 작업**
- **3스테이지 시야 제한 (원형 마스킹)**
- **3스테이지 풍선장어** — Day 32에서 만든 자산(`Enemy_BalloonEel`, `EnemyData_BalloonEel`) 재활용
- **4스테이지 직선 레이저 원거리 적** — `EnemyRangedAttack`을 변형해 구현
- **매니저 prefab 변환** — 각 스테이지 씬 독립 테스트 편의성 (현재는 Stage1부터 시작 전제)
- **전체 밸런싱** — 4스테이지 완성 후 일괄 조정

## 13. 주의할 점

### 시스템 책임 분리

- `GameManager` — 전역 게임 상태 (Playing/Paused/StageClear 등)와 스테이지 타이머
- `StageManager` — 현재 스테이지 추적, 다음 스테이지 결정과 전환 트리거
- `PlayerProgressData` — 스테이지 간 운반 데이터, 세이브 파일 아님
- `StageTransitionUI` — 페이드/자막 연출만 담당, 흐름 제어는 안 함
- `DifficultyManager` — 시간 경과에 따른 적 강화/스폰 가속 곡선 (씬 매니저, DontDestroyOnLoad 아님)

의존 방향: `GameManager` ← `StageManager` → `PlayerProgressData`, `StageTransitionUI`.

### ChangeState 순서

`GameManager.ChangeState`는 다음 순서로 처리한다 (Day 31에서 수정):
1. `CurrentState` 변경
2. `switch`로 `Time.timeScale`, `TimerRunning` 설정
3. `OnGameStateChanged` 이벤트 발행

중첩 호출 시 내부 ChangeState의 switch가 마지막에 적용되어 timeScale 안정성을 보장한다.

### CurrentState 기반 핸들러

`OnGameStateChanged` 핸들러는 매개변수 `state`가 아닌 `GameManager.Instance.CurrentState`를 기준으로 판단한다 (중첩 호출 시 매개변수가 stale일 수 있음).

### 시간 제어

- 스테이지 전환 중 `Time.timeScale`이 0일 수 있으므로 페이드는 반드시 `unscaledDeltaTime` 기준으로 처리한다.
- 의태 기관 무적 코루틴은 `WaitForSeconds`(timeScale 영향)이므로 일시정지 시 자동 정지 — 의도된 동작.

### Restore 순서

`PlayerProgressData.Restore`는 다음 순서로 처리한다:
1. `LevelManager.SetState` — 스킬 효과 재적용 (`OnSkillSelected` 누적 발행)
2. `MutationManager.RestoreOwnedIDs` — 돌연변이 효과 stats에 곱셈 변환
3. `PlayerStats` 복원 — pressureResistance, currentHp Clamp
4. `CopySkillManager.RestoreSlot` × 3

스킬은 가산(`+= delta`), 돌연변이는 곱셈이라 이 순서가 자연스럽다.

### DontDestroyOnLoad 매니저들

- `StageManager`, `StageTransitionUI`, `PlayerProgressData`는 모두 `Awake`에서 자식이면 root로 자동 reparent 후 `DontDestroyOnLoad` 처리.
- 인스펙터/씬 하이어라키에서는 `[Managers]` 그룹 안에 둬도 안전.
- 첫 씬(Stage1_Lab)에만 존재. 다른 씬에 같은 GameObject가 있으면 `Awake`에서 자동 Destroy.
- **Stage2 직접 시작 시 매니저들이 없으므로 BossSpawner 등이 fallback 값 사용** — 실 게임플레이엔 영향 없음.

### 카피 스킬 ID 확장 순서

`CopySkillID` enum 멤버 순서를 변경하면 ScriptableObject 에셋의 직렬화가 깨질 수 있다. 추가는 항상 enum 끝에만 한다 (None, Berserk, Dash, HealingFactor 다음에 Ultrasonic, DeepPressure, PredatorCharge).

### 압력 저항 디자인

저항은 페널티 감소 전용이며, 경험치 보상은 `RawPressure` 기준을 유지한다.

### 적 스폰 가속과 만렙 보상

Day 28.5의 적 스폰 difficulty curve는 모든 스테이지에서 동일하게 적용된다. 스테이지별 차등은 6주차에서 검토. 만렙 보상 카드는 일반 스킬 7종 기준이므로, 6주차에 스킬 추가 시 만렙 도달 조건이 자동으로 늘어난다.

### 향유고래 패턴 단순화

Day 33의 향유고래 보스는 시간 부족 시 추적 + 접촉 피해 + 단순 부채꼴 공격까지만 구현한다. 풀 패턴은 6주차로 미룬다.

### 전기뱀장어 vs 풍선장어

디자인 문서 정확 반영을 위해 2스테이지는 **전기뱀장어**(오라 지속 피해), 3스테이지는 **풍선장어**(원거리 발사)로 분리. Day 32에서 만든 풍선장어 자산은 3스테이지용으로 보존.

### GameOver 흐름

GameOver 시 `PlayerProgressData.ResetAll()` 호출 위치를 명확히 한다. 권장: `GameOverPanel`의 재시작 버튼 클릭 시점 (`GameManager.RestartGame()` → `StageManager.StartNewGame()`).

### 세포 카운터

만렙 보상 카드 "세포 추출"로 누적되는 세포는 임시 카운터다. 본격적인 메타 업그레이드 화면은 6주차에 만든다. 그때까지 세포는 누적만 되고 사용처는 없다.

### 테스트 모드 토글

- `BossSpawner.overrideStageData` — StageData 무시하고 인스펙터의 bossSpawnTime 사용
- `DifficultyManager.overrideStageData` — StageData 무시하고 인스펙터의 peakTime 사용
- Day 34 통합 테스트 직전에 모두 false로 복귀
