# Project: Abyss - 5주차 TODO

작성일: 2026-05-10
보완일: 2026-05-10 (Day 28 검증 결과 반영, 페이스 보완 일정 추가)

## 1. 5주차 목표

5주차의 핵심 목표는 1스테이지 프로토타입을 넘어, 스테이지 진행 구조와 2스테이지 기반을 연결하는 것이다. Day 28 통합 검증에서 발견된 페이스 이슈도 함께 보완한다.

현재 Day 24~28 + Day 28.5 일부까지 다음 작업이 완료된 상태다.

- 보스 피격/페이즈2 색상 피드백 정리
- 카피 스킬 슬롯 HUD 추가
- 스테이지 전환 골격 추가 (StageData, StageManager, PlayerProgressData, StageTransitionUI 스켈레톤)
- 압력 시스템을 StageData 기반으로 정리
- 압력 저항은 페널티만 줄이고, 경험치 보상은 RawPressure 기준으로 유지되도록 변경
- 돌연변이 2종 추가 (감각 붕괴, 독성 과부화)
- 의태 기관을 기획 문서 방향으로 변경
- OvgrownTentacle 오타 수정
- NotoSansKR 폰트 한글 음절 전체 추가, atlas 4096 × 4096으로 정리
- 기본 스킬 Slash가 시작 시 Lv1로 등록되도록 수정
- Day 28 4주차 통합 검증 완료 (페이스 이슈 발견)
- Day 28.5 5-1, 5-2 완료: DifficultyManager 도입 + EnemySpawner/EnemyBase 시간 기반 스케일링

5주차는 다음 흐름을 완성하는 데 집중한다.

```text
1스테이지 보스 처치
→ 카피 스킬 선택
→ 검은 화면 페이드 아웃
→ 다음 스테이지 이름 표시
→ 씬 전환
→ 페이드 인
→ 2스테이지 시작
```

## 2. 사전 결정 사항

5주차 작업 시작 전에 확정해 두는 항목. Day 30 이후 작업이 이 결정에 의존한다.

### 2-1. 스테이지 분리 방식: 씬 단위

4주차 Day 25에서 합의한 대로, 4스테이지를 각각 독립 씬으로 분리한다.

- `Stage1_Lab`
- `Stage2_Sea`
- `Stage3_Deep`
- `Stage4_Ruined`

각 씬은 동일한 매니저 셋을 가지며, 씬에 배치된 `StageData` 참조에 따라 동작이 분기된다. `PlayerProgressData`가 `DontDestroyOnLoad`로 진행 데이터를 다음 씬으로 운반한다.

### 2-2. 스테이지별 압력 파라미터

4주차 Day 26에서 합의한 점진 강화 디자인. `StageData` 에셋에 다음 값을 입력한다.

| 스테이지 | pressureEnabled | startY | maxY | movePenalty | atkPenalty | expBonus |
|---------|-----------------|--------|------|-------------|------------|----------|
| 1 (Lab) | false | (무관) | (무관) | (무관) | (무관) | (무관) |
| 2 (Sea) | true  | -5 | -30 | 0.20 | 0.20 | 0.30 |
| 3 (Deep)| true  | -5 | -40 | 0.35 | 0.35 | 0.50 |
| 4 (Ruined) | false | (무관) | (무관) | (무관) | (무관) | (무관) |

### 2-3. PlayerProgressData 정책

- 스테이지 간 영구 데이터 운반용. 메타 업그레이드용 세이브 파일(세포 등)과는 별개의 시스템이다.
- 최초 생성: 1스테이지 시작 씬에 GameObject로 미리 배치한다. (`StageManager`도 같은 위치에 둔다.)
- 첫 씬 진입 시 자동으로 `DontDestroyOnLoad` 처리. 이후 씬 전환에서도 유지된다.
- GameOver 시 `ResetAll()` 호출하여 진행 데이터 초기화. 메타 재화(세포)는 별도 시스템에서 보존한다.
- 새 게임 시작 시에도 `ResetAll()` 호출.

### 2-4. 카피 스킬 ID 확장 시점

`CopySkillID` enum 확장은 Day 33 (2스테이지 보스 카피 스킬 작성) 시작 시점에 한다. 이때 `CopySkillSelectCardUI.FindSkillComponent()`도 함께 갱신한다.

### 2-5. 스테이지 타이밍 단축

Day 28 검증 결과 1스테이지가 8~9분으로 너무 길어 후반 인플레이션이 발생함. 모든 스테이지의 타이밍을 단축한다.

| 항목 | 기존 | 변경 |
|------|------|------|
| stageDuration (1~2스테이지) | 540s (9분) | **420s (7분)** |
| bossSpawnTime (1~2스테이지) | 480s (8분) | **360s (6분)** |
| stageDuration (3~4스테이지) | 420s (7분) | **360s (6분)** |
| bossSpawnTime (3~4스테이지) | 360s (6분) | **300s (5분)** |

> 적 스폰 가속과 세트로 적용한다. 시간만 줄이면 레벨 곡선이 압축되어 카피/돌연변이 시스템이 의미를 잃는다.

### 2-6. 스킬 풀 확장은 6주차로

Day 28 검증에서 일반 스킬 7종(공격 4 + 패시브 3)이 4스테이지 길이 대비 부족하다는 점이 확인되었다. 다만 신규 스킬은 디자인/구현/밸런싱이 묶여있어 5주차에 끼워넣으면 스테이지 전환 작업이 밀린다. 따라서 **6주차 별도 일정**으로 분리한다.

추가 후보 (6주차 디자인 시):
- 공격: 음파 진동, 가시 발사, 흡혈 촉수
- 패시브: 압력 적응, 발광 감각

## 3. 스테이지 전환 흐름

보스 처치 후 카피 스킬을 선택하면 즉시 끊기지 않고, 검은 화면으로 페이드 아웃된다. 페이드 중 다음 스테이지 이름을 표시하고, 이후 페이드 인으로 새 스테이지를 시작한다. 메뉴식 결과 화면보다 몰입감 있는 연속 진행이 목표다.

구현 흐름:

1. `CopySkillSelectCardUI.OnClick()`
2. `CopySkillManager.AssignSkill()`
3. `CopySkillSelectPanel.Hide()`
4. `GameManager.TriggerStageClear()`
5. `StageManager.TransitionToNext()`
6. `PlayerProgressData.Capture()`
7. `StageTransitionUI.PlayFadeOut(nextStage.displayName, callback)`
8. 검은 화면 + 스테이지 이름 자막 (1.0s + 1.5s)
9. `SceneManager.LoadScene(nextStage.sceneName)`
10. 새 씬 Start: `PlayerProgressData.Restore()` + `StageTransitionUI.PlayFadeIn()`

## 4. Day 28 - 4주차 통합 검증 (완료)

4주차 작업 전체가 1스테이지 흐름에서 정상 동작하는지 마지막 검증.

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

- [x] `PressureSystem.isActive` — StageData 자동 연결로 운용. 인스펙터 값은 false로
- [x] `BioEnergyManager.testMode` = false
- [x] `CopySkillManager.testMode` = false
- [x] `BossSpawner.bossSpawnTime` — 단축 테스트값에서 본 값으로 복귀
- [x] `MutationManager` 트리거 레벨 [7, 12] 확인

### 4-6. 마무리

- [x] 콘솔 에러 / 경고 확인
- [x] 4주차 작업 커밋

### 4-7. 검증 결과 메모

**플레이 결과 (압력 100% 켠 상태로 진행)**
- 보스 등장 직전 레벨 25 도달
- 모든 일반 스킬 만렙 (Lv4)
- 보스 1초만에 처치

**발견된 페이스 이슈**
1. 적 스폰량이 시간에 따라 증가하지 않아 후반부 긴박감 부족
2. 스테이지 8~9분이 너무 길어 후반 인플레이션 발생
3. 스킬 풀(7종)이 4스테이지 분량 대비 부족
4. 만렙 도달 후 추가 레벨업 보상 부재

**대응 일정**
- 이슈 1, 2, 4: §5 Day 28.5 페이스 보완에서 처리
- 이슈 3: 6주차 별도 일정으로 분리

## 5. Day 28.5 - 페이스 보완

Day 28 검증에서 발견된 페이스 이슈를 Day 29 본 작업 진입 전에 빠르게 해결한다. 작업량 합계 약 2~3시간.

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

**검증 메모**
- 압력 0 상태에서도 28레벨 도달 (스폰 가속으로 경험치 획득량 증가 영향)
- 난이도는 아직 쉬운 편이지만, 1스테이지 단독 밸런싱보다 전체 스테이지 완성 후 일괄 조정이 더 효율적이라고 판단
- 추가 밸런싱은 6주차 또는 4스테이지 완성 후로 미룸

### 5-3. 만렙 보상 (LevelManager) — 내일 진행 예정

- [ ] `LevelManager`에 만렙 도달 후 레벨업 분기 추가
- [ ] 모든 스킬이 maxLevel인지 체크하는 헬퍼 메서드 추가
- [ ] 만렙 후 레벨업 시 보상 지급:
  - [ ] 세포 +5 (PlayerProgressData 또는 임시 카운터에 누적)
  - [ ] HP 회복 (최대 HP의 15%)
- [ ] HUD에 세포 카운터 표시 (간단한 텍스트)
- [ ] 만렙 후 레벨업 시 LevelUpPanel을 띄울지 보상 알림으로 대체할지 결정
- [ ] 보상 적용이 정상 동작하는지 플레이 테스트

### 5-4. 검증

- [ ] 1스테이지 시작부터 보스 등장까지 전체 플레이
- [ ] 6분 시점 적 수와 강도가 초반 대비 명확히 증가하는지 확인
- [ ] 만렙 후에도 레벨업 진행감(세포/HP 회복)이 있는지 확인
- [ ] 보스 처치가 1초 이내가 아닌 적당한 전투 길이가 되는지 확인

## 6. Day 29 - 스테이지 전환 실제 연결

스테이지 전환 골격을 실제 동작 가능한 상태로 만든다. 이 작업은 Stage1 단일 씬 안에서 검증한다.

### 6-1. StageTransitionUI 풀 구현

- [ ] 풀스크린 검은 Image + CanvasGroup 게임오브젝트 생성
- [ ] 중앙 TMP_Text 자막 게임오브젝트 생성
- [ ] `StageTransitionUI` 컴포넌트에 두 참조 연결
- [ ] `PlayFadeOut()` 코루틴 실제 구현 (alpha 0 -> 1, 자막 표시, hold)
- [ ] `PlayFadeIn()` 코루틴 실제 구현 (alpha 1 -> 0)
- [ ] `unscaledDeltaTime` 사용으로 `Time.timeScale = 0` 상태에서도 동작 확인
- [ ] 페이드 중 입력/클릭 차단 확인 (`blocksRaycasts = true`)

### 6-2. 호출 연결

- [ ] `StageManager.TransitionToNext()`에서 `StageTransitionUI.PlayFadeOut()` 호출
- [ ] `CopySkillSelectCardUI.OnClick()` 마지막에 `StageManager.TransitionToNext()` 호출 추가
- [ ] StageClear 패널 표시 정책 결정: 별도 패널 없이 페이드만 사용
- [ ] 전환 후 `GameManager` 상태가 정상적으로 `Playing`으로 복귀하는지 확인

### 6-3. 검증

- [ ] 1스테이지 보스 처치 → 카피 스킬 선택 → 페이드 → 자막 → 페이드 인 흐름이 끊김 없이 동작
- [ ] 자기 자신 씬으로 다시 로드되어도 매니저 중복 생성이 없는지 확인

## 7. Day 30 - StageData 기반 스테이지 설정

`사전 결정 2-2, 2-5` 표를 기준으로 4개 스테이지의 StageData 에셋을 만든다.

### 7-1. StageData 에셋 생성

- [ ] `Stage1_Lab.asset` 생성 (stageNumber=1, sceneName="Stage1_Lab", displayName="해저 연구소")
- [ ] `Stage2_Sea.asset` 생성 (stageNumber=2, sceneName="Stage2_Sea", displayName="해저")
- [ ] `Stage3_Deep.asset` 생성 (stageNumber=3, sceneName="Stage3_Deep", displayName="심해")
- [ ] `Stage4_Ruined.asset` 생성 (stageNumber=4, sceneName="Stage4_Ruined", displayName="파괴된 연구소")
- [ ] `nextStage` 체인 연결: 1 → 2 → 3 → 4 → null

### 7-2. 스테이지별 파라미터 입력

- [ ] 1~2스테이지: stageDuration=420s, bossSpawnTime=360s
- [ ] 3~4스테이지: stageDuration=360s, bossSpawnTime=300s
- [ ] 압력 파라미터를 사전 결정 2-2 표대로 입력
- [ ] 4스테이지는 압력 비활성화로 결정 (별도 환경 효과는 6주차 이후 검토)

### 7-3. 시스템 연동

- [ ] `GameManager`가 `StageData.stageDuration`을 읽도록 변경
- [ ] `BossSpawner`가 `StageData.bossSpawnTime`을 읽도록 변경
- [ ] `StageManager`에 `Stage1_Lab.asset`을 startingStage로 설정
- [ ] 1스테이지 씬 Play 시 압력이 비활성, 인스펙터 fallback 값이 무시되는지 확인

## 8. Day 31 - 플레이어 진행 데이터 보존

`PlayerProgressData`의 Capture/Restore 실제 구현. 매니저들에서 데이터를 모으고, 새 씬에서 복원한다.

### 8-1. 영구 데이터 항목 정의

- [ ] HP, 최대 HP, 압력 저항 저장
- [ ] 현재 레벨, 누적 경험치 저장
- [ ] 일반 스킬 레벨 저장 방식 결정 (string ID 기반 권장)
- [ ] 카피 스킬 슬롯 (Q/E/Space ID) 저장
- [ ] 선택한 돌연변이 ID 목록 저장
- [ ] 세포 카운터 보존 (Day 28.5에서 추가된 임시 카운터)

### 8-2. Capture / Restore 구현

- [ ] `LevelManager`에 `GetSkillLevelsCopy()`, `SetState(level, exp, dict)` 메서드 추가
- [ ] `CopySkillManager`에 `AssignFromIDs(qID, eID, spaceID)` 메서드 추가
- [ ] `MutationManager`에 `GetOwnedIDs()`, `RestoreOwnedIDs(list)` 메서드 추가
- [ ] `PlayerProgressData.Capture()` 실제 구현 — 위 매니저들에서 수집
- [ ] `PlayerProgressData.Restore()` 실제 구현 — 새 씬에서 매니저들에 적용

### 8-3. 라이프사이클

- [ ] 첫 씬에 `PlayerProgressData` GameObject 배치
- [ ] 씬 전환 시 `DontDestroyOnLoad` 중복 생성 방지 동작 확인
- [ ] GameOver 시 `PlayerProgressData.ResetAll()` 호출하는 위치 결정 (`GameOverPanel`의 재시작 버튼 권장)
- [ ] 새 게임 시작 시 `ResetAll()` 호출 확인

## 9. Day 32 - 2스테이지 기본 구성

`Stage2_Sea` 씬을 새로 만들고, 1스테이지 씬을 복제한 뒤 2스테이지 데이터로 교체한다.

### 9-1. 씬 생성 및 매니저 셋업

- [ ] `Stage2_Sea.unity` 씬 생성 (Stage1 복제 또는 신규)
- [ ] 씬 내 매니저 오브젝트 동일 구성 확인
- [ ] BuildSettings에 새 씬 등록

### 9-2. 2스테이지 화이트박스 필드

- [ ] 배경/맵 화이트박스 구성 (1스테이지와 시각적으로 구분)
- [ ] 카메라 범위와 플레이어 시작 위치 조정
- [ ] 압력 페널티 체감 확인을 위한 Y 범위 검증

### 9-3. 적 데이터/프리팹

- [ ] 피라냐 적 데이터/프리팹 생성 (기본 추적형, 1스테이지 Default보다 빠름)
- [ ] 빠른 적 계열(청새치/상어) 초안 생성 (높은 이속, 낮은 체력)
- [ ] 2스테이지 EnemySpawner 웨이브 설정
- [ ] Day 28.5에서 추가한 difficulty curve가 2스테이지에서도 동작하는지 확인

### 9-4. 원거리 적 시스템 (1·2스테이지 공통)

1스테이지 `Enemy_Shooter`가 이름만 Shooter였던 상태를 정리하면서, 2스테이지 풍유장어와 공유할 범용 원거리 공격 시스템을 만든다. 4스테이지 레이저도 이 base 위에 변형으로 올린다.

- [ ] `EnemyRangedAttack` 컴포넌트 신규 생성 (발사 간격, 사거리, 정지 거리, 투사체 prefab 참조)
- [ ] 적용 EnemyAI 분기: 사거리 안에 들어오면 정지 후 발사, 너무 가까우면 후퇴
- [ ] 적용 투사체 풀 시스템 (PoisonNeedle 구조 참고)
- [ ] 1스테이지 `Enemy_Shooter`에 컴포넌트 부착 + 마취총 컨셉으로 투사체 느림/저데미지 설정
- [ ] 2스테이지 풍유장어 데이터에 동일 컴포넌트 적용 (발광 꼬리 컨셉, 더 빠른 투사체)
- [ ] 원거리 적이 스폰되는 웨이브에서 실제 발사 동작 확인
- [ ] 디버그용 발사 콘솔 로그는 정리

### 9-4. 검증

- [ ] 2스테이지 진입 시 PressureSystem 자동 활성화 확인
- [ ] 압력 페널티 체감 (이동/공격속도 -20%, 경험치 +30%)
- [ ] Q 슬롯 (1스테이지에서 획득한) 카피 스킬 사용 가능 확인
- [ ] 플레이어 HP/레벨/스킬레벨이 1스테이지에서 이어지는지 확인

## 10. Day 33 - 2스테이지 보스 / 카피 스킬 초안

향유고래 보스와 2스테이지 카피 스킬 3종 추가. 시간이 빡빡하면 보스 패턴은 단순한 형태로 시작한다.

### 10-1. CopySkillID 확장

- [ ] `CopySkillID` enum에 2스테이지 카피 스킬 3종 추가 (Ultrasonic, DeepPressure, PredatorCharge)
- [ ] `CopySkillSelectCardUI.FindSkillComponent()` 갱신
- [ ] 새 카피 스킬 컴포넌트 3종을 Player 오브젝트에 추가 (비활성 상태로 시작)

### 10-2. 향유고래 보스

- [ ] `BossData_Whale.asset` 생성 (HP 많음, 이속 느림)
- [ ] `Boss_Whale.prefab` 생성 (BossBase, BossAI, HitEffect, BossPhaseEffect 부착)
- [ ] 기본 추적/접촉 피해 동작 확인
- [ ] 초음파 공격 — 1차 구현은 단순한 부채꼴 즉발 공격으로 시작 (디자인 문서의 풀 패턴은 6주차로 미룸)
- [ ] 페이즈2 시 이동속도 증가 + 추가 패턴 여부 결정

### 10-3. 2스테이지 카피 스킬 데이터

- [ ] `CopySkill_Ultrasonic.asset` (E 슬롯, 부채꼴 대피해, 140E)
- [ ] `CopySkill_DeepPressure.asset` (E 슬롯, 주변 디버프, 80E)
- [ ] `CopySkill_PredatorCharge.asset` (E 슬롯, 직선 돌진, 90E)
- [ ] BossData_Whale의 copySkillOptions에 3종 등록, copySkillSlot=1

### 10-4. 검증

- [ ] 2스테이지 보스 처치 후 카피 스킬 선택 패널이 E 슬롯용으로 뜨는지 확인
- [ ] 선택한 카피 스킬이 E 슬롯 HUD에 표시되는지 확인
- [ ] E 키 눌렀을 때 정상 발동, 에너지 소비, 사용 가능 여부 표시 정상 동작

## 11. Day 34 - 5주차 통합 테스트

1스테이지부터 2스테이지 카피 스킬 사용까지의 전체 흐름을 끊김 없이 검증한다.

### 11-1. 흐름 검증

- [ ] 1스테이지 시작 → 보스 처치 → Q 슬롯 카피 스킬 선택
- [ ] 페이드 아웃 → "해저" 자막 → 페이드 인 → 2스테이지 시작
- [ ] 2스테이지 진입 직후 HUD 상태 확인 (HP/EXP/레벨/카피슬롯/돌연변이)
- [ ] 2스테이지에서 압력 슬라이더 정상 동작
- [ ] Q 슬롯 카피 스킬 사용 가능 확인
- [ ] 2스테이지 보스 처치 → E 슬롯 카피 스킬 선택
- [ ] (시간 허용 시) 페이드 → 3스테이지 진입 시도

### 11-2. 데이터 보존 검증

- [ ] 플레이어 HP / 최대 HP / 압력 저항 보존
- [ ] 레벨 / 경험치 보존
- [ ] 일반 스킬 레벨 보존
- [ ] 카피 스킬 슬롯 보존 (Q는 1스테이지 것 그대로)
- [ ] 선택한 돌연변이 보존
- [ ] 세포 카운터 보존

### 11-3. 페이스 검증

- [ ] 1스테이지가 7분 안에 적당한 난이도로 마무리되는지 확인
- [ ] 적 스폰 가속이 양쪽 스테이지에서 모두 동작하는지 확인
- [ ] 보스 처치 시간이 1초가 아닌 진짜 전투 시간이 되는지 확인
- [ ] 만렙 후 보상이 두 스테이지 동안 정상 누적되는지 확인

### 11-4. 환경 검증

- [ ] 1스테이지에서 압력 비활성, 2스테이지에서 활성 확인
- [ ] 콘솔 에러 / 경고 확인
- [ ] 페이드 전환 중 UI 겹침 / 잔상 확인
- [ ] DontDestroyOnLoad 오브젝트 중복 생성 없는지 확인

### 11-5. 마무리

- [ ] 불필요한 디버그 로그 정리
- [ ] 테스트용 단축값 본 값으로 복귀
- [ ] 5주차 작업 커밋

## 12. 6주차로 미루는 작업

다음 항목은 5주차 일정에서 의도적으로 제외했다. 6주차 또는 그 이후에 별도 일정으로 다룬다.

- **스킬 풀 확장** — 공격 스킬 2~3종, 패시브 1~2종 추가 (디자인 + 구현 + 밸런싱 묶음)
- **메타 업그레이드 시스템** — 세포 사용 화면, 영구 강화 옵션 트리
- **향유고래 풀 패턴** — 초음파 광역 + 페이즈2 추가 패턴
- **3, 4스테이지 보스/적/카피 스킬 본격 작업**
- **3스테이지 시야 제한 (원형 마스킹)**
- **4스테이지 직선 레이저 원거리 적** — Day 32에서 만든 `EnemyRangedAttack`을 변형해 구현

## 13. 주의할 점

### 시스템 책임 분리

- `GameManager` — 전역 게임 상태 (Playing/Paused/StageClear 등)와 스테이지 타이머
- `StageManager` — 현재 스테이지 추적, 다음 스테이지 결정과 전환 트리거
- `PlayerProgressData` — 스테이지 간 운반 데이터, 세이브 파일 아님
- `StageTransitionUI` — 페이드/자막 연출만 담당, 흐름 제어는 안 함

위 4개의 책임이 겹치지 않도록 유지한다. 의존 방향: `GameManager` ← `StageManager` → `PlayerProgressData`, `StageTransitionUI`.

### 시간 제어

- 스테이지 전환 중 `Time.timeScale`이 0일 수 있으므로 페이드는 반드시 `unscaledDeltaTime` 기준으로 처리한다.
- 의태 기관 무적 코루틴은 `WaitForSeconds`(timeScale 영향)이므로 일시정지 시 자동 정지 — 의도된 동작.

### 카피 스킬 ID 확장 순서

`CopySkillID` enum 멤버 순서를 변경하면 ScriptableObject 에셋의 직렬화가 깨질 수 있다. 추가는 항상 enum 끝에만 한다 (None, Berserk, Dash, HealingFactor 다음에 Ultrasonic, DeepPressure, PredatorCharge).

### 압력 저항 디자인

저항은 페널티 감소 전용이며, 경험치 보상은 `RawPressure` 기준을 유지한다. 메타 업그레이드의 가치를 보존하면서 위험 환경의 보상은 그대로 두는 디자인 의도.

### 적 스폰 가속과 만렙 보상

Day 28.5의 적 스폰 difficulty curve는 모든 스테이지에서 동일하게 적용된다. 스테이지별 차등은 6주차에서 검토한다. 만렙 보상은 일반 스킬 7종 기준이므로, 6주차에 스킬 추가 시 만렙 도달 조건이 자동으로 늘어난다.

### 향유고래 패턴 단순화

Day 33의 향유고래 보스는 시간 부족 시 추적 + 접촉 피해 + 단순 부채꼴 공격까지만 구현한다. 디자인 문서의 풀 패턴(초음파 광역 + 스턴 등)은 6주차로 미룬다.

### GameOver 흐름

GameOver 시 `PlayerProgressData.ResetAll()` 호출 위치를 명확히 한다. 권장: `GameOverPanel`의 재시작 버튼 클릭 시점. `GameManager.TriggerGameOver()` 시점에 호출하면 게임오버 화면에서 진행 데이터 표시가 불가능해진다.

### 세포 카운터

Day 28.5에서 추가하는 세포는 만렙 보상 임시 카운터다. 본격적인 메타 업그레이드 화면은 6주차에 만든다. 그때까지 세포는 누적만 되고 사용처는 없다.

### 빌드 경고

`StageTransitionUI`, `PlayerProgressData`는 Day 29 / Day 31 전까지 스켈레톤 상태이므로 일부 메서드가 TODO 주석 + Debug.Log만 출력한다. 빌드 시 경고 확인하고 무시 가능.
