UI

1. 체력, 에너지, exp텍스트 잘 안보임
    1. 체력이랑 에너지는 화면 밖으로 나가서 안보이고
    2. exp텍스트는 경험치 바랑 겹쳐있어서 잘 안보임
2. 카드 선택 가독성 낮음
3. 카피슬롯 글씨 활성화되면 안보임
4. 튜토리얼 버튼과 텍스트 겹침

---

밸런스

1. 흡혈촉수, 체력재생 오버벨런스임
2. 발광기관 보호막 전기뱀장어 일렉트릭필드에서 안사라지는 것 같음(이건 자세히 확인할 수 없는게 뱀장어가 너무 일찍죽어서 진위여부는 잘 모르겠다했음)
3. 2스테이지 보스 출현 5분으로 축소(피로감 호소함)
4. 3스테이지 일반몹 풀 2스테이지 처럼 많이 하는게 좋을것같음. 너무 쉬움
5. 3스테이지 일반몹 체력 상향 필요해보임 정확한 수치는 3스테이지 공격력참고(과부하 돌연변이시 54정도 없을땐 45정도로 생각하고 체력 상향 필요, 과부화 돌연변이 아닐때 2번때리면 피 조금 남을정도로 상향)
6. 3스테이지도 피로감 호소 보스 출몰시간 4분으로 축소
7. 3스테이지전에 만랩찍어서 3스테이지 컨텐츠 부족하다는 피드백 있었음(그냥 플레이타임 늘리기 용도로밖에 생각이 안든다 라고함)
8. 4스테이지도 보스 출몰시간 4분으로 축소
9. 4스테이지 원거리 레이저 적 사정거리 두배로 늘려서 좀 더 위협적이게 변경하면 좋겠다는 의견

---

개발자 피드백 추가

- 콘솔에러 확인 필요

Coroutine couldn't be started because the the game object 'Enemy_ReinforcedGuard(Clone)' is inactive!
UnityEngine.MonoBehaviour:StartCoroutine (System.Collections.IEnumerator)
HitEffect:PlayFlash () (at Assets/Scripts/Enemy/HitEffect.cs:42)
EnemyBase:TakeDamage (single) (at Assets/Scripts/Enemy/EnemyBase.cs:69)
VoidPierceSkill:ApplyTick (UnityEngine.Vector2,UnityEngine.Vector2) (at Assets/Scripts/Skills/VoidPierceSkill.cs:141)
VoidPierceSkill/<ChannelRoutine>d__12:MoveNext () (at Assets/Scripts/Skills/VoidPierceSkill.cs:114)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

공허관통 쓸때 나타난것같음

- 공허관통을 좀 더 멋있게 사거리도 두배로 늘리고 두께도 늘려서 파괴광선처럼 하는건 어떤지. 지금도 좋긴한데 보는 맛이 없다고나 할까. (원래 컨셉은 산갈치의 긴 마디를 카피해서 플레이어의 사슬팔을 길게 마디로 뻗어 전기톱처럼 적들을 갈아버리는 컨셉)