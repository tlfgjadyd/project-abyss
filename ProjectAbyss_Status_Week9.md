# Project: Abyss - 9주차 TODO

작성일: 2026-06-05 (8주차 종료 + 풀플레이 피드백 반영 + Day56 앵커형 동적 스폰 완료 후 작성)

이 문서는 **새 세션에서 컨텍스트 없이 봐도 작업 시작이 가능**하도록 작성됨. 파일 경로 / 수치 before-after / 구현 위치 명시.
9주차는 **마지막 주차(과제 제출)**. 비주얼 마감 + 디버프 시스템 + 최종 풀플레이 + 빌드 + 보고서.

---

## 0. 새 세션 cold-start 가이드

### 0-1. 현재 프로젝트 상태 (8주차 완료 시점, 2026-06-05)

**플레이 가능한 풀 사이클**: MainMenu → Stage 1~4 → 각 보스 처치 → 카피스킬 선택 → 연구소장 처치 → VictoryPanel → 메인 메뉴 → 메타 업그레이드 5종 → 재시작

**보스-스테이지 매핑** (혼동 주의 — `memory/boss_stage_mapping.md`):
- 1스 Lab: **실험용 실험체** (Boss_ExperimentalSubjects.asset)
- 2스 Sea: **향유고래** (Boss_Whale.asset)
- 3스 Deep: **산갈치** (Boss_Oarfish.asset)
- 4스 Ruined: **연구소장** (Boss_Director.asset)

**8주차까지 완료된 것**:
- 맵: 4스테이지 바닥 타일 + 격자 프롭 (InfiniteTiledBackground / FloorPropScatter)
- sprite: 플레이어(절차 애니+flip), 일반 적 14종, 보스 4종(산갈치 단일 sprite + OarfishOrient 회전)
- 색보정: 4씬 URP Volume(Atmosphere_*.asset) + 청록 틴트
- UI 통일: 카드 #12 / 슬롯 육각 #13 / 버튼 #21 / HUD 바·스탯아이콘 / 일시정지 패널 투명
- **풀플레이 피드백 batch 반영 완료** (아래 §0-3)
- **Day56 앵커형 동적 스폰 완료** — 풀플레이 검증 "딱 적당" (아래 §0-3)

**아직 없는 것 (= 9주차 작업)**:
- 스킬 VFX (현재 기본 공격 시각만, 화려한 발동 이펙트 없음)
- 스킬/돌연변이/카피스킬 **아이콘** (현재 카드·슬롯 전부 텍스트만)
- 메인화면 UI 다듬기 (그래디언트 기본형만)
- **통합 상태이상/DoT 시스템** (현재 없음 — SpikeBurst 둔화·출혈, IsStunned만 임시 존재)
- 적 디버프 **시각 표현** (어떤 디버프 걸렸는지 안 보임)

### 0-2. 주요 파일/폴더 빠른 참조

| 분류 | 경로 |
|------|------|
| 9주차 계획서 | `Project_Abyss/Project_Abyss/ProjectAbyss_Status_Week9.md` (본 문서) |
| 8주차 계획서 | `Project_Abyss/Project_Abyss/ProjectAbyss_Status_Week8.md` (참고용) |
| 디자인 문서 | `Project_Abyss/Project_Abyss/ProjectAbyss_GameDesignDocument.md` |
| 매니저 prefab | `Assets/Prefabs/Managers/[Managers].prefab` (4씬 인스턴스 공유) |
| 일반 스킬 데이터 | `Assets/ScriptableObjects/SkillData/*.asset` (12종) — class `SkillData` |
| 돌연변이 데이터 | `Assets/ScriptableObjects/MutationData/Mutation_*.asset` (5종) — class `MutationData` |
| 카피 스킬 데이터 | `Assets/ScriptableObjects/CopySkillData/CopySkill_*.asset` (9종) — class `CopySkillData` |
| 적 데이터 | `Assets/ScriptableObjects/EnemyData/Stage{1-4}/EnemyData_*.asset` |
| 스킬 컴포넌트 | 플레이어 GO에 부착: `Slash`, `PoisonNeedle`, `BioticExplosion`, `ElectricEngine`, `SpikeBurst` 등 |
| 카피 스킬 베이스 | `Assets/Scripts/Skills/CopySkillBase.cs`, `VoidPierceSkill.cs`(LineRenderer 빔 예시) |
| 적 베이스 | `Assets/Scripts/Enemy/Common/EnemyBase.cs` (TakeDamage/IsDead/IsStunned/TriggerAnim) |
| 피격 플래시 | `Assets/Scripts/Enemy/Common/HitEffect.cs` (흰색 color flash — 상태 틴트와 충돌 주의) |
| 카드 UI | `SkillCardUI.cs` / `MutationCardUI.cs` / `CopySkillSelectCardUI.cs` (모두 `Assets/Scripts/UI/`) |
| 카피슬롯 UI(HUD) | `Assets/Scripts/UI/CopySkillSlotUI.cs` (Q/E/Space, 현재 텍스트만) |
| HUD | `Assets/Scripts/UI/HUDController.cs` |
| 씬 | `Assets/Scenes/MainMenu.unity`, `Stage{1-4}_*.unity` |
| 이전 빌드 | `Build/Windows_Release/Project_Abyss.exe` (7주차 2차, 203 MB) |

### 0-3. 8주차 마지막 변경 사항 (이 위에서 시작)

**피드백 batch (2026-06-05)**:
- HUD 바(HP/Exp/Energy/Pressure) `interactable=false`+raycast off — 드래그/WASD로 값 안 바뀜 (HUDController.DisableSliderInput)
- 카드 텍스트 확대(4씬, Name20→26 / Desc autosize12~16 등) + MutationPanel 배경 검정 a1.0→a0.39 반투명 + 700→980x560 확대
- 카피슬롯 nowrap+autosize(CopySkillSlotUI.Awake)
- 카드 설명↔소모에너지/패널티 겹침 → EnergyCostText/PenaltyText top앵커 재배치(4씬)
- **스킬명 15종 한글화** (메인12 + 카피3). `skillName`은 표시 전용
- 돌연변이 첫 표시 hitch → MutationPanel.PrewarmGlyphs (TMP 글리프 미리 굽기)
- B1: Stage1 Shooter interval 1.5→3.0/pool 12 (원거리 비율↓)
- B2: Stage2 이속 Shark4.0/Eel3.3/Piranha3.1/Marlin4.4
- 추가: GiantSquid 이속 6.9→3.45, 과성장촉수 페널티 -30%→-15%(×0.85)+사거리 ×1.6

**Day56 앵커형 동적 스폰 (DifficultyManager.cs)**:
- 정상 구간 = 기존 선형 램프 그대로 보존
- 새 인스펙터 노브(4씬 DM에 기본값 자동 반영): `crisisLeadTime`(30), `crisisSpawnBoost`(1.3), `bossActiveSpawnFactor`(0.7), `globalIntensityScale`(전역 노브, 0.5~2)
- 보스 30초 전 크레셴도 → 보스 등장 시 완화. `OnBossIncoming` 이벤트로 BOSS INCOMING 경고 타이밍 교정
- B3: 3·4스 DM `hpMultiplierPeak 2.0→1.7`
- **재밸런싱 필요 시**: 각 씬 DifficultyManager 인스펙터의 `globalIntensityScale`/`crisisSpawnBoost`/`hpMultiplierPeak` 조절

---

## 1. 작업 항목 (권장 순서)

> 순서 근거: ① **디버프 시스템**은 다른 시각 작업의 토대 → 먼저. ② **VFX**는 디버프 시각화와 파티클/스프라이트 인프라를 공유 → 이어서. ③ **아이콘**은 데이터+UI 배선(독립). ④ **메인화면**은 빠르고 독립적. ⑤~⑦ 풀플레이→빌드→보고서는 마지막 고정 순서.

### A. 통합 디버프 시스템 + 시각화 + 독침 중독 ★먼저

**목표**: 독/출혈/둔화 등 상태이상을 한 곳에서 관리하고, 적이 무슨 디버프에 걸렸는지 **눈에 보이게**.

**현재 상태 (cold-start)**:
- 통합 상태이상 시스템 **없음**. 임시 구현만 산재:
  - `SpikeBurst`: 둔화(이속-40%,1.5s), Lv4 출혈(atk×0.25×3s) — piercingEnabled 플래그를 Lv4 트리거로 재사용
  - `EnemyBase.IsStunned` (BioticExplosion 등이 사용)
  - `Mutation_ToxicOverload`: "상태이상 추가 피해 +100%"는 **DoT 시스템 도입 후 적용** TODO 주석 (MutationManager.ApplyToxicOverload)
- **PoisonNeedle은 이름과 달리 실제 중독 DoT를 부여하지 않음** (현재 즉발 데미지만).

**구현 방향**:
1. `EnemyStatusEffects` 컴포넌트 신규 (EnemyBase에 부착 or GetComponent). 효과 종류: `Poison`(틱 데미지), `Bleed`(틱 데미지, 이동 시 가중 옵션), `Slow`(이속 배율), `Stun`(기존 IsStunned 흡수 고려).
   - 각 효과: 지속시간, 틱 간격, 강도. 중첩/갱신 정책 결정 필요(아래 결정사항).
   - 틱 데미지는 `EnemyBase.TakeDamage` 경유(HitEffect 흰 플래시 재사용 가능).
2. **시각화** — 결정사항 참조. 권장: 적 머리 위 **작은 상태 아이콘**(독=초록방울, 출혈=빨강, 둔화=파랑/달팽이) + 약한 색 오라(SpriteRenderer 오버레이 자식). **주의**: `HitEffect`가 메인 SpriteRenderer.color를 흰색으로 덮으므로, 상태 틴트를 메인 color에 직접 넣으면 충돌. → 별도 자식 SpriteRenderer/Particle 또는 아이콘으로 분리.
3. **라우팅**: PoisonNeedle→Poison 부여, SpikeBurst 둔화/출혈→새 시스템으로 이관, ElectricEngine 감전→Slow or 신규 Shock, ToxicOverload 추가피해 TODO 해소.

**파일**: `Assets/Scripts/Enemy/Common/EnemyBase.cs`, `EnemyAI.cs`(이속 배율 적용 지점), `HitEffect.cs`, 스킬 컴포넌트들, `MutationManager.cs`(ToxicOverload).

**결정사항 (확정됨, 2026-06-05)**:
- **시각화 = 머리 위 아이콘 + 약한 오라** (HitEffect 흰 플래시와 충돌 피하려 메인 SpriteRenderer.color 직접 사용 금지 → 자식 SpriteRenderer/Particle로 분리). 오라 진하기/아이콘으로 스택·종류 표현.
- **독(Poison) = 갱신 + 재적용 보너스**:
  - 첫 적용 → 일반 DoT(틱 데미지). 인스턴스 1개만 유지.
  - 이미 중독 중 재적중 → 지속시간 **갱신(리셋)** + **즉발 보너스 데미지 1회**(중독 "터짐"). 틱 세기는 일정.
  - 튜닝: 보너스 = `attackPower × k` or 고정값 (구현 시 결정).
- **출혈(Bleed) = 누적(stack)**:
  - 적중마다 스택 +1 → 틱 데미지 = `기본 × 스택수`. **최대 스택 캡(예: 5)**으로 무한 누적 방지.
  - 지속시간은 스택 간 공유·갱신. 출혈 종료 시 스택 0.
  - 스택 수를 오라 진하기/아이콘 숫자로 시각화.
- **둔화(Slow) / 스턴(Stun) = 갱신**(가장 강한 값 유지 + 시간 리셋). 시작 시 변경 가능.
- 라우팅: PoisonNeedle→Poison(+Lv별 강화), SpikeBurst 출혈→Bleed 스택/둔화→Slow, ElectricEngine 감전→Slow(or 신규 Shock), BioticExplosion→Stun(기존 IsStunned 흡수 검토), ToxicOverload 추가피해 TODO 해소.

### B. 스킬 VFX

**목표**: 스킬 발동이 "화려하게" 보이도록. 픽셀아트 톤 유지.

**현재 상태**: 기본 공격 시각 4종만(Slash 부채꼴 / PoisonNeedle 머즐 / BioticExplosion 원형 / ElectricEngine zigzag). 카피스킬은 VoidPierce 보라 빔(LineRenderer) 등 최소.

**구현 방향**: 스킬별 발동 이펙트(파티클 or 스프라이트 플래시). `VoidPierceSkill.cs`의 LineRenderer 패턴이 좋은 참고. URP 2D + Bloom(Volume) 켜져 있어 발광 픽셀이 글로우됨 → 밝은 색 코어 추천. 우선순위: 자주 보는 일반 공격 스킬 > 카피스킬 > 패시브.

**결정사항**: ParticleSystem vs 스프라이트 시트 vs Shader. 픽셀톤이면 스프라이트/파티클 혼용 권장. A의 디버프 파티클과 인프라 공유.

### C. 스킬 아이콘 (일반 스킬 / 돌연변이 / 카피스킬)

**목표**: 카드·슬롯에 텍스트 대신(또는 함께) 아이콘.

**현재 상태**: 전부 텍스트만. 데이터 클래스에 icon 필드 없음.

**구현 방향**:
1. 데이터 클래스에 `public Sprite icon;` 추가: `SkillData`(12), `MutationData`(5), `CopySkillData`(9) = 총 **26개 아이콘** 필요.
2. UI 배선: `SkillCardUI` / `MutationCardUI` / `CopySkillSelectCardUI`에 Image 추가 + Setup에서 icon 할당. **HUD 카피슬롯**(`CopySkillSlotUI`) Q/E/Space도 아이콘으로(현재 키+이름 텍스트).
3. 아이콘 소스: game-icons.net (CC0/CC-BY, 스탯 아이콘 때 이미 사용). 1-bit 톤 통일 vs 컬러 — 결정.

**파일**: 데이터 .cs 3종 + 카드 UI .cs 3종 + `CopySkillSlotUI.cs` + 각 .asset에 아이콘 할당(4씬 prefab/패널 Image 배선).

### D. 메인화면 UI

**목표**: 타이틀 화면 완성도 ↑.

**현재 상태**: `MainMenu.unity` — 그래디언트 배경 기본형. (시작 시 씬 열어 현황 인스펙트할 것.) 사용자 고려안 없음 → 빠르게.

**구현 방향**: 타이틀 로고/텍스트, 시작/메타업그레이드/종료 버튼(1-bit 버튼 #21 톤 통일), 분위기 배경(심해 호러). 기존 메타 업그레이드 UI와 연결 확인.

### E. 과제 제출 전 real 풀플레이

4스테이지 끝까지 정주행 1회 이상. 체크: 앵커 곡선 체감, BOSS INCOMING 타이밍, 디버프 시각/중독, 아이콘 표시, VFX, 메인화면 흐름, 한글명 표기, 밸런스(특히 3·4스). 발견 이슈는 DifficultyManager 노브(globalIntensityScale 등)로 우선 미세조정.

### F. 빌드

Windows Release 빌드(`manage_build`). 이전 산출: `Build/Windows_Release/Project_Abyss.exe`(203MB). 빌드 후 실기 1회 스모크 테스트(에디터 전용 동작 vs 빌드 차이 확인 — 의태기관 반투명/3카드/색보정 등 과거 실기검증 미뤄둔 항목 포함).

### G. 보고서

과제 제출용 최종 보고서. 재료: GDD + Week4~9 status 문서 + 스크린샷/빌드. 구성(안): 기획 요약, 독창 시스템 4종(생체에너지/카피스킬/돌연변이/압력), 개발 일정, 기술 스택(Unity URP 2D, ScriptableObject 모듈화), 회고.

---

## 2. 빠른 우선순위 요약

1. **A. 디버프 시스템 + 시각화 + 독침 중독** (토대, 결정사항 먼저)
2. **B. 스킬 VFX** (A와 인프라 공유)
3. **C. 스킬 아이콘 26종** (데이터+UI 배선)
4. **D. 메인화면 UI** (빠름)
5. **E. real 풀플레이** → 미세조정
6. **F. 빌드** → 실기 스모크
7. **G. 보고서** → 제출

> 빌드(F)는 9주차 마무리에 1회. 중간 검증은 에디터 플레이로.
