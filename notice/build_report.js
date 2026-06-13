const fs = require("fs");
const {
  Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell,
  AlignmentType, LevelFormat, HeadingLevel, BorderStyle, WidthType, ShadingType, PageBreak
} = require("docx");

const FONT = "맑은 고딕";
const CODEFONT = "Consolas";

// ---------- helpers ----------
const P = (text, opts = {}) => new Paragraph({
  spacing: { after: opts.after ?? 80, before: opts.before ?? 0, line: 276 },
  alignment: opts.align,
  children: [new TextRun({ text, bold: opts.bold, italics: opts.italics, size: opts.size ?? 20, color: opts.color, font: FONT })],
});

const runs = (children, opts = {}) => new Paragraph({
  spacing: { after: opts.after ?? 80, line: 276 }, children,
});
const T = (text, o = {}) => new TextRun({ text, bold: o.bold, italics: o.italics, size: o.size ?? 20, color: o.color, font: FONT });

const H1 = (text) => new Paragraph({ heading: HeadingLevel.HEADING_1, spacing: { before: 280, after: 140 },
  children: [new TextRun({ text, bold: true, size: 30, font: FONT, color: "1F3864" })] });
const H2 = (text) => new Paragraph({ heading: HeadingLevel.HEADING_2, spacing: { before: 200, after: 100 },
  children: [new TextRun({ text, bold: true, size: 24, font: FONT, color: "2E5496" })] });

const bullet = (text, lvl = 0) => new Paragraph({
  numbering: { reference: "b", level: lvl }, spacing: { after: 40, line: 264 },
  children: [new TextRun({ text, size: 20, font: FONT })],
});

const codeLine = (text) => new Paragraph({
  spacing: { after: 0, line: 240 },
  shading: { type: ShadingType.CLEAR, fill: "F2F2F2" },
  children: [new TextRun({ text: text === "" ? " " : text, size: 15, font: CODEFONT, color: "1A1A1A" })],
});
const code = (src) => src.split("\n").map(codeLine);

const border = { style: BorderStyle.SINGLE, size: 1, color: "BBBBBB" };
const borders = { top: border, bottom: border, left: border, right: border, insideHorizontal: border, insideVertical: border };
function table(headers, rows, widths) {
  const total = widths.reduce((a, b) => a + b, 0);
  const cell = (txt, w, head) => new TableCell({
    width: { size: w, type: WidthType.DXA }, borders,
    shading: head ? { type: ShadingType.CLEAR, fill: "D9E2F3" } : undefined,
    margins: { top: 60, bottom: 60, left: 100, right: 100 },
    children: [new Paragraph({ children: [new TextRun({ text: txt, bold: !!head, size: 18, font: FONT })] })],
  });
  const trows = [new TableRow({ tableHeader: true, children: headers.map((h, i) => cell(h, widths[i], true)) })];
  rows.forEach(r => trows.push(new TableRow({ children: r.map((c, i) => cell(c, widths[i], false)) })));
  return new Table({ width: { size: total, type: WidthType.DXA }, columnWidths: widths, rows: trows });
}

const children = [];

// ================= TITLE PAGE =================
children.push(new Paragraph({ alignment: AlignmentType.CENTER, spacing: { before: 1200, after: 200 },
  children: [new TextRun({ text: "REPORT", bold: true, size: 72, font: FONT, color: "1F3864" })] }));
children.push(new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 900 },
  children: [new TextRun({ text: "가상현실 프로젝트 최종 보고서", bold: true, size: 32, font: FONT })] }));
[
  ["과     목", "가상현실 02분반"],
  ["전     공", "소프트웨어전공"],
  ["작 성 자", "신제민"],
  ["학     번", "202101771"],
  ["제출일자", "26.06.13"],
].forEach(([k, v]) => children.push(new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 60 },
  children: [new TextRun({ text: `${k}  :  ${v}`, size: 24, font: FONT })] })));
children.push(new Paragraph({ alignment: AlignmentType.CENTER, spacing: { before: 700 },
  children: [new TextRun({ text: "Project: ABYSS", bold: true, italics: true, size: 40, font: FONT, color: "2E5496" })] }));
children.push(new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 40 },
  children: [new TextRun({ text: "심해 실험체 생존 액션 게임 — Unity 2D 탑다운 뱀서라이크", size: 20, font: FONT, color: "555555" })] }));
children.push(new Paragraph({ children: [new PageBreak()] }));

// ================= 1. 개요 =================
children.push(H1("1. 프로젝트 개요"));
children.push(P("본 프로젝트는 Unity 엔진으로 제작한 2D 탑다운 로그라이크(뱀서라이크) 게임 『Project: ABYSS』의 최종 결과물이다. 중간 계획서에서 제시한 \"심해 실험체 생존 액션\" 컨셉과 4종의 독창 시스템(압력 / 공격 수단 카피 / 생체 에너지 / 돌연변이 진화)을 모두 구현하였으며, 4개 스테이지를 보스 처치로 진행하고 최종 보스(연구소장) 격파 후 메타 업그레이드로 재도전하는 풀 게임 루프를 완성하였다."));
children.push(P("스토리: 심해 자원 개발 기업의 비밀 연구소에서 생체 실험을 당한 실험체가 변이된 심해 생물들을 헤치고 더 깊은 심해로 도망치며 다양한 능력을 흡수해 성장하고, 최종적으로 연구소로 돌아와 연구소장에게 복수한다."));
children.push(P("계획서 대비 주요 변경점(공지 안내에 따라 핵심 흐름은 유지):", { bold: true, before: 80 }));
children.push(bullet("플레이어 비주얼: 키메라 → 두족류(촉수형) 괴물로 구체화, 절차적 애니메이션 적용"));
children.push(bullet("돌연변이 선택지 2개 → 3개(레벨 14·25)로 확장, 효과 수치 일부 재밸런싱"));
children.push(bullet("1회 플레이타임 약 20~25분으로 조정(스테이지 제한시간 단축)"));
children.push(bullet("사운드(BGM/SFX)와 스킬 카드 아이콘은 시간·조달 제약으로 범위 제외(텍스트 UI로 기능 대체)"));
children.push(table(
  ["항목", "내용"],
  [
    ["장르", "뱀서라이크(Vampire Survivors 스타일) 2D 탑다운 로그라이크"],
    ["컨셉", "심해 + 생체 실험체(촉수형 괴물)"],
    ["플레이타임", "1회 약 20~25분 (4 스테이지)"],
    ["플랫폼", "PC (Windows)"],
  ], [2400, 6960]));

// ================= 2. Unity 버전 =================
children.push(H1("2. 사용한 Unity 버전"));
children.push(P("사용 버전: Unity 2022.3.62f3 (LTS)", { bold: true }));
children.push(P("렌더링: Universal Render Pipeline(URP) 2D 14.0.12 / 언어: C# / 버전 관리: Git·GitHub"));
children.push(P("선택 이유:", { bold: true, before: 60 }));
children.push(bullet("장기 지원(LTS)으로 안정성이 검증되어 있고 2D 핵심 기능(Sprite, Physics 2D, Tilemap)이 충분"));
children.push(bullet("URP 2D의 2D Light·후처리(Volume: Bloom/Vignette/Color Adjustments)로 심해 호러 분위기 연출에 유리"));
children.push(bullet("ScriptableObject·Object Pool 등 모듈식 설계에 필요한 API와 레퍼런스가 풍부"));

// ================= 3. 구현 내용 =================
children.push(H1("3. 구현 내용 (스크립트 포함)"));
children.push(P("아래 설명은 본인이 직접 설계·구현·통합한 게임 로직을 중심으로 한다. 오픈소스·AI·무료 에셋을 활용한 부분은 4장에서 별도로 구분·출처 표기하였다.", { italics: true }));

children.push(H2("3-1. 전체 아키텍처와 적용한 디자인 패턴"));
children.push(P("확장과 유지보수가 쉽도록 데이터 주도(Data-driven) + 컴포넌트 모듈화를 기본 원칙으로 삼았다."));
children.push(bullet("ScriptableObject 데이터 분리: SkillData / EnemyData / BossData / MutationData / CopySkillData 로 수치·효과를 코드와 분리 → 적·스킬 추가가 에셋 생성만으로 가능"));
children.push(bullet("컴포넌트 기반 스킬: 각 스킬(Slash, PoisonNeedle, BioticExplosion, ElectricEngine, SpikeBurst …)을 독립 MonoBehaviour로 모듈화"));
children.push(bullet("싱글톤 매니저 + [Managers] 프리팹: GameManager / LevelManager / BioEnergyManager / MutationManager / CopySkillManager / DifficultyManager 등을 한 프리팹으로 묶고 DontDestroyOnLoad로 씬 간 공유"));
children.push(bullet("옵저버(이벤트) 패턴: OnLevelChanged, OnSkillSelected, OnBossDeath, OnHpChanged, OnAnyEnemyKilled 등 C# Action 이벤트로 시스템 간 결합도 최소화"));
children.push(bullet("오브젝트 풀링: 투사체·경험치 오브·적을 UnityEngine.Pool.ObjectPool로 재사용하여 GC/생성 비용 절감"));
children.push(bullet("인터페이스 다형성: IDamageable(피해), IStatusReceiver(상태이상)로 적·보스·세그먼트를 공통 처리"));

children.push(H2("3-2. 기본 뱀서라이크 — 이동 / 자동 공격 / 레벨업"));
children.push(P("WASD 8방향 이동, 자동 공격은 Physics2D.OverlapCircle로 사거리 내 적을 탐색해 가장 가까운 적을 타격한다. 레벨업 시 3장의 카드 중 1장을 선택하며, 공격 5 / 패시브 3의 슬롯 제한으로 빌드 결정감을 부여했다."));
children.push(P("스킬 수치는 ScriptableObject(SkillData)의 레벨별 배열로 관리하고, SkillEffectApplier가 OnSkillSelected 이벤트를 받아 실제 스탯·컴포넌트에 반영한다:", { before: 60 }));
children.push(...code(
`[System.Serializable]
public struct SkillLevelStats {
    public float attackPowerDelta, attackSpeedDelta, rangeDelta;
    public float moveSpeedDelta, maxHpDelta, hpRegenDelta;
    public bool  knockbackEnabled, piercingEnabled;
    public int   extraProjectiles;
    public float cooldownDelta, stunDurationDelta;
}
// SkillEffectApplier: 레벨업 이벤트 수신 → 해당 스킬에 델타 누적
void Apply(SkillData skill, int newLevel) {
    SkillLevelStats s = skill.levelStats[newLevel - 1];
    switch (skill.skillID) {
        case SkillID.Slash:          ApplySlash(s);          break;
        case SkillID.ElectricEngine: ApplyElectricEngine(s, newLevel); break;
        // ...
    }
}`));
children.push(P("이 구조 덕분에 공격력·이동속도 같은 ‘전역 스탯’ 델타는 모든 스킬이 공유하고, 사거리·쿨다운·관통 같은 ‘스킬 전용’ 값만 분리 적용된다.", { italics: true, before: 40 }));

children.push(H2("3-3. 독창 시스템 4종"));
children.push(P("(1) 압력 시스템 — PressureSystem이 플레이어 Y좌표(하강 깊이)에 비례해 이동/공격속도에 곱연산 페널티를 주고, 대신 경험치 획득량을 늘려 리스크·리워드를 만든다(2·3스테이지). 메타 업그레이드로 저항을 최대 50%까지 영구 강화."));
children.push(P("(2) 공격 수단 카피 시스템 — 스테이지 보스를 처치하면 그 보스의 스킬 3종 중 1개를 골라 Q(1스)·E(2스)·스페이스(3스) 슬롯에 장착한다. CopySkillManager가 슬롯/발동/쿨다운을, CopySkillBase 파생 클래스가 각 스킬 효과를 담당."));
children.push(P("(3) 생체 에너지 시스템 — 적 처치 시 BioEnergyManager가 에너지를 충전(최대 200E)하고, 카피 스킬 발동 시 차등 소모(30~140E)한다. 게이지로 표시되어 ‘언제 쓸지’ 전략적 판단을 요구."));
children.push(P("(4) 돌연변이 진화 시스템 — 레벨 14·25에서 3장 중 1장을 선택(중복 불가). 모든 선택지가 하이리스크·하이리턴(강한 효과 + 페널티)이며, MutationManager가 곱연산으로 스탯을 변형한다. 예: 과부화(공격력 ×1.2 / 최대 HP ×0.7), 독성 과부화(독·감전·출혈 DoT ×1.7 / 물리 ×0.7)."));

children.push(H2("3-4. 통합 상태이상(DoT) 시스템 — IStatusReceiver (핵심 설계)"));
children.push(P("초기에는 둔화·스턴·출혈이 스킬마다 제각각 구현되어 중복·누락이 많았다. 이를 인터페이스 기반으로 일원화하여, 일반 적(EnemyBase)과 보스(BossBase)가 동일한 방식으로 상태이상을 받도록 재설계했다."));
children.push(...code(
`public interface IStatusReceiver : IDamageable {
    bool IsDead { get; }
    void ApplyPoison(float perTick, float duration, float interval, float reapplyBonus);
    void ApplyBleed (float perStackTick, float duration, float interval, int maxStacks);
    void ApplySlow  (float multiplier, float duration);
    void ApplyVulnerability(float multiplier, float duration);
    void Stun(float duration);
}`));
children.push(P("EnemyStatusEffects 컴포넌트가 독(갱신형: 재적용 시 지속 갱신 + 즉발 보너스)과 출혈(누적 스택형: 틱 = 1스택 피해 × 스택 수, 캡 5)을 코루틴으로 처리하고, 머리 위 아이콘으로 시각화한다. EnemyBase가 Awake에서 자동 부착하므로 프리팹 18종을 수정할 필요가 없다.", { before: 40 }));
children.push(...code(
`// 독: 갱신 + 재적용 즉발 보너스
public void ApplyPoison(float perTick, float dur, float interval, float reapplyBonus) {
    if (host == null || host.IsDead) return;
    if (IsPoisoned) {                       // 이미 중독 → 즉발 보너스 + 지속 갱신
        if (reapplyBonus > 0f) host.TakeDamage(reapplyBonus);
        if (poisonCo != null) StopCoroutine(poisonCo);
    }
    poisonCo = StartCoroutine(PoisonRoutine(perTick, dur, interval));
}`));
children.push(P("보스도 같은 인터페이스를 구현하되, 저항 배율(debuffResistance)과 스턴 면역을 적용해 보스전에서 DoT가 과도하지 않게 균형을 맞췄다(독성 과부화·심해 압박 같은 디버프 빌드가 보스전에서 비로소 의미를 가짐):", { before: 40 }));
children.push(...code(
`// BossBase — 저항을 적용하고 스턴은 면역
public void ApplyPoison(float perTick, float dur, float interval, float bonus) {
    float keep = 1f - debuffResistance;     // 기본 0.5 → DoT 절반
    status?.ApplyPoison(perTick*keep, dur, interval, bonus*keep);
}
public void Stun(float duration) { /* 보스는 스턴 면역 */ }`));

children.push(H2("3-5. 적 / 보스 AI"));
children.push(bullet("EnemyAI: 벡터 추적 이동, EnemyData(ScriptableObject)로 체력·속도·접촉 피해 관리, 원거리 적은 정지·후퇴 거리 유지, 풀링 스폰"));
children.push(bullet("BossBase: HP 비율 기반 페이즈2 전환 이벤트, 25% 임계 이벤트(보호막 트리거), 접촉 피해"));
children.push(bullet("보스별 패턴: 향유고래(초음파 광역 + 페이즈2 디버프), 산갈치(절차적 체인 팔로우로 긴 몸체 + 진행 방향 회전 + 텔레포트), 연구소장(드론 소환 + 직선 레이저 + 보호막)"));

children.push(H2("3-6. 동적 난이도 — 앵커형 위기 곡선"));
children.push(P("DifficultyManager가 시간 경과에 따라 스폰 밀도·적 체력을 선형으로 완만히 올리되, 보스 등장 약 30초 전부터 스폰을 크레셴도로 끌어올려 긴장을 고조시키고(BOSS INCOMING 경고), 보스가 나타나면 일반 적 스폰을 완화해 보스에 집중할 여유를 준다. 전역 강도 노브 하나로 전체 곡선을 재밸런싱할 수 있게 했다."));

children.push(H2("3-7. 비주얼 / 이펙트(VFX)"));
children.push(bullet("스프라이트 프레임 VFX: SkillVfxOneShot(1회 재생 후 자동 파괴), SpriteAnimLoop(투사체 무한 루프) 재사용 컴포넌트로 휘두르기·전기·폭발·가시·투사체에 적용"));
children.push(bullet("절차적 VFX: 에셋 조달이 어려운 효과는 코드로 생성 — 초음파(삼각형 부채꼴 Mesh), 심해 압박(코드 생성 soft-circle 텍스처가 수축), 전기 감전 필드, 전기뱀장어 필드"));
children.push(bullet("URP 2D Light + Volume 후처리로 스테이지별 색보정(차가운 회청 / 청록 / 칠흑 / 탁한 녹회)"));

// ================= 4. 오픈소스/AI =================
children.push(H1("4. 오픈소스 / AI 활용 및 출처"));
children.push(P("공지의 평가 기준(본인 구현 vs 오픈소스/AI 구분, 출처 표기)에 따라 아래와 같이 명확히 구분한다.", { italics: true }));

children.push(H2("4-1. 본인이 직접 구현한 부분"));
children.push(bullet("게임 로직 전체의 C# 스크립트 설계·구현·통합: 이동/자동공격, 레벨업·슬롯 제한, 독창 시스템 4종(압력·카피·생체에너지·돌연변이), 통합 상태이상(IStatusReceiver/EnemyStatusEffects), 적·보스 AI와 패턴, 동적 난이도, 세이브/메타 업그레이드, 씬 전환 상태 복원, UI 흐름"));
children.push(bullet("아키텍처 결정(ScriptableObject 데이터 주도, 컴포넌트 모듈화, 이벤트/싱글톤/풀링/인터페이스), 디버깅, 밸런싱"));

children.push(H2("4-2. AI · 오픈소스 · 무료 에셋 (출처)"));
children.push(P("[AI 코딩 보조 — 이해·응용]", { bold: true }));
children.push(bullet("Anthropic Claude를 페어 프로그래밍 보조로 활용: 일부 스크립트의 초안 작성과 리팩토링, 버그 원인 분석에 참고하였으며, 최종 설계 의사결정·코드 통합·동작 검증·밸런싱은 직접 수행하고 코드 내용을 이해한 상태로 사용함"));
children.push(P("[AI 이미지 생성 — 그대로/가공 적용]", { bold: true, before: 40 }));
children.push(bullet("플레이어·일부 적·타이틀 로고 등 일부 스프라이트: Google Gemini, sprite 생성형 AI 도구로 생성 후 가공(투명 배경·슬라이스·PPU 조정)"));
children.push(P("[무료 에셋 / 오픈소스 — 그대로/가공 적용]", { bold: true, before: 40 }));
children.push(bullet("스킬·투사체 VFX 스프라이트 시트: itch.io 무료 2D 이펙트 에셋 (CC0/무료 라이선스)"));
children.push(bullet("상태이상 머리 위 아이콘: game-icons.net (CC BY 3.0)"));
children.push(bullet("한글 폰트: Noto Sans KR (SIL Open Font License)"));
children.push(P("[Unity 공식 기능 — 이해·응용]", { bold: true, before: 40 }));
children.push(bullet("UnityEngine.Pool.ObjectPool(오브젝트 풀링), URP 2D Light·Volume 후처리, 2D Aseprite Importer 패키지"));
children.push(P("※ 일부 절차적 VFX(부채꼴 Mesh, soft-circle 필드 등)는 적합한 무료 에셋을 구하기 어려워 코드로 직접 생성하여 대체하였다.", { italics: true, before: 40 }));

// ================= 5. 어려웠던 점 =================
children.push(H1("5. 어려웠던 점과 해결 과정"));

children.push(H2("5-1. 보스 디버프가 들어가지 않던 문제 → 인터페이스 도입"));
children.push(P("스킬들이 GetComponent<EnemyBase>()로 디버프를 걸어, 별도 클래스인 보스(BossBase)에는 둔화·중독이 적용되지 않았다. 그 결과 ‘독성 과부화’ 같은 DoT 중심 빌드가 보스전에서 무의미했다. 공통 인터페이스 IStatusReceiver를 도입하고 호출부를 GetComponent<IStatusReceiver>()로 교체하여 적·보스를 동일 경로로 처리했고, 보스에는 저항 배율·스턴 면역을 부여해 균형을 맞췄다."));

children.push(H2("5-2. 긴 몸체(산갈치) 보스 — 마디 추적"));
children.push(P("산갈치의 긴 몸체를 마디(Segment)들의 체인 팔로우로 구현했는데, 마디를 보스의 자식으로 두니 부모 이동이 마디를 평행 이동시켜 체인 효과가 무효화되었다. Awake에서 SetParent(null, worldPositionStays:true)로 분리(로컬 스케일 보존)하여 해결했고, 부모 없는 트리거 콜라이더의 MissingComponent 오류는 Kinematic Rigidbody2D를 추가해 제거했다."));

children.push(H2("5-3. 넉백이 즉시 사라지는 문제"));
children.push(P("EnemyAI가 FixedUpdate마다 velocity를 추적 방향으로 덮어써, AddForce 넉백이 한 프레임 만에 사라졌다. EnemyBase에 ApplyKnockback과 KnockbackUntil을 두고, EnemyAI가 넉백 시간 동안 velocity 제어를 양보(IsKnockedBack이면 return)하도록 하여 해결했다."));

children.push(H2("5-4. 씬 전환 시 돌연변이 중복 지급"));
children.push(P("스테이지 이동 후 진행 상태를 복원할 때 LevelManager.SetState가 OnLevelChanged를 재발행하는데, 그 시점에 돌연변이 보유 목록이 아직 복원되지 않아 이미 지난 트리거 레벨(14)이 다시 발동되어 돌연변이를 한 번 더 지급했다. 스탯 계산 순서(스킬→돌연변이)를 유지해야 해 단순 순서 변경이 불가능했으므로, 복원 구간에만 트리거를 억제하는 플래그(SuppressTriggers)를 도입해 해결했다."));
children.push(...code(
`// PlayerProgressData.Restore — 복원 중에는 돌연변이 트리거 억제
MutationManager.Instance.SuppressTriggers = true;
LevelManager.Instance.SetState(level, exp, ...);   // OnLevelChanged 재발행되어도 무시
MutationManager.Instance.SuppressTriggers = false;`));

children.push(H2("5-5. 데이터 오류 발견 — 스탯 전수 점검"));
children.push(P("재밸런싱을 위해 모든 스킬·돌연변이의 레벨별 수치를 한 문서로 정리하던 중, ‘신경 가속’ Lv4 데이터가 attackSpeedDelta가 아닌 moveSpeedDelta 슬롯에 들어가 있어 적용 함수가 무시 → Lv4가 사실상 무효가 되는 버그를 발견하고 수정했다. 데이터 주도 설계의 장점(전수 비교의 용이함)을 활용한 사례."));

children.push(H2("5-6. 풀링 객체의 상태 잔존 / URP 2D 렌더링"));
children.push(bullet("오브젝트 풀에서 재사용되는 객체(투사체·적)의 색·코루틴·회전·디버프가 이전 상태로 남는 문제 → OnEnable에서 모든 상태를 초기화하는 패턴으로 일관 처리"));
children.push(bullet("URP 2D에서 절차적 Mesh/Sprite의 렌더·정렬(sortingOrder) 및 런타임 텍스처 생성으로 부채꼴·원형 필드 VFX 구현"));

children.push(H2("5-7. 반복 밸런싱"));
children.push(P("외부 테스터 피드백(‘후반이 너무 쉬움’, ‘UI 잘림’ 등)을 받아 보스 HP·적 밀도·스킬 수치를 여러 차례 조정했고, 단순 선형 증가의 단조로움을 ‘보스 직전 크레셴도 + 보스 등장 완화’의 앵커형 동적 난이도로 개선했다."));

// ================= 6. 창의 =================
children.push(H1("6. 창의적 / 아이디어가 돋보이는 부분"));
children.push(bullet("독창 시스템 4종(압력·카피·생체에너지·돌연변이)을 뱀서라이크 기본 위에 결합해 ‘불안정한 실험체의 성장’이라는 정체성을 구현"));
children.push(bullet("인터페이스 다형성(IStatusReceiver): 적·보스·신규 디버프를 한 추상으로 묶어 확장에 열린 구조 — 보스 저항/스턴 면역도 같은 틀에서 자연스럽게 표현"));
children.push(bullet("데이터 주도 + 컴포넌트 모듈화(ScriptableObject + MonoBehaviour): 스킬·적 추가가 에셋 생성과 컴포넌트 부착만으로 가능, 수치 전수 점검도 용이"));
children.push(bullet("이벤트(옵저버) 패턴으로 레벨업·보스 처치·적 사망 등을 느슨하게 연결해 시스템 간 의존도 최소화"));
children.push(bullet("앵커형 동적 난이도: 정상 구간은 보존하면서 보스 직전에만 위기를 연출하는 곡선 설계"));
children.push(bullet("절차적 VFX: 에셋 조달이 어려운 효과를 코드로 생성(런타임 텍스처/Mesh)하여 의존성과 용량을 줄임"));

// ================= 7. 회고 =================
children.push(H1("7. 결과 및 회고"));
children.push(P("최종적으로 메인 메뉴 → 4개 스테이지(각 보스) → 카피 스킬 선택 → 연구소장 처치 → 엔딩 → 메타 업그레이드 → 재도전으로 이어지는 완결된 게임 루프를 Windows 빌드로 플레이 가능한 상태로 완성하였다. 계획서의 4대 독창 시스템과 전체 흐름을 유지하면서, 개발 과정에서 통합 상태이상 시스템·동적 난이도·다수의 버그 수정과 밸런싱으로 완성도를 높였다."));
children.push(P("Unity와 C#을 처음 본격적으로 사용하면서 ScriptableObject·이벤트·인터페이스·오브젝트 풀링 등 실무적 설계 패턴을 직접 적용하고, 발생한 문제를 구조적으로 해결하는 경험을 얻은 것이 가장 큰 수확이었다. 사운드·스킬 아이콘 등 일부는 시간·조달 제약으로 범위에서 제외했으나, 핵심 게임성과 독창 시스템의 완성도에 집중한다는 원칙을 끝까지 유지하였다.", { before: 40 }));
children.push(P("이상으로 프로젝트 최종 보고서를 마칩니다.", { before: 120 }));

// ---------- build ----------
const doc = new Document({
  styles: { default: { document: { run: { font: FONT, size: 20 } } } },
  numbering: { config: [{ reference: "b", levels: [
    { level: 0, format: LevelFormat.BULLET, text: "•", alignment: AlignmentType.LEFT,
      style: { paragraph: { indent: { left: 540, hanging: 260 } } } },
    { level: 1, format: LevelFormat.BULLET, text: "–", alignment: AlignmentType.LEFT,
      style: { paragraph: { indent: { left: 980, hanging: 260 } } } },
  ] }] },
  sections: [{
    properties: { page: { size: { width: 11906, height: 16838 }, margin: { top: 1440, right: 1440, bottom: 1440, left: 1440 } } },
    children,
  }],
});

Packer.toBuffer(doc).then(buf => {
  const out = "C:/Users/shinjm/Unity/Project_Abyss/Project_Abyss/notice/가상현실-02-202101771-신제민_최종보고서.docx";
  fs.writeFileSync(out, buf);
  console.log("WROTE " + out + " (" + buf.length + " bytes)");
});
