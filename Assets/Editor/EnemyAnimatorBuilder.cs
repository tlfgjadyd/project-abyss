using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// 적 폴더(예: Assets/Sprites/Enemies/Stage2/shark/)에서
/// Idle/Walk/Attack/Death/Hurt 시트를 찾아 AnimatorController + Clip을 자동 생성.
///
/// 사용법:
///   1. Project 창에서 적 폴더(시트들이 들어있는 폴더)를 선택
///   2. 메뉴 → Project Abyss → Build Enemy Animator
///   3. 같은 폴더에 {폴더명}.controller + {폴더명}_{Anim}.anim 들이 생성됨
///   4. Enemy prefab의 SpriteRenderer를 _0 sprite로 교체하고, Animator.controller에 컨트롤러 할당
///
/// 동작:
///   - Idle/Walk: looping. Attack/Death/Hurt: non-looping.
///   - 기본 상태(Default): Walk가 있으면 Walk, 아니면 Idle.
///   - 다른 상태는 트리거("Attack","Death","Hurt")로 진입 + 종료 후 Default로 복귀(Death 제외).
///   - 프레임 레이트 10fps 기본.
/// </summary>
public static class EnemyAnimatorBuilder
{
    const float FrameRate = 10f;

    // Project 창 우클릭 → Build Enemy Animator (가장 확실한 호출 경로)
    [MenuItem("Assets/Build Enemy Animator", false, 100)]
    static void BuildFromContext() => Build();

    [MenuItem("Project Abyss/Build Enemy Animator", false, 100)]
    static void Build()
    {
        string folder = ResolveFolder();
        if (string.IsNullOrEmpty(folder))
        {
            EditorUtility.DisplayDialog("Build Enemy Animator",
                "Project 창에서 적 폴더(또는 그 안의 파일)를 한 번 클릭해 선택한 뒤 다시 실행하세요.\n" +
                "예: Assets/Sprites/Enemies/Stage2/shark\n\n" +
                "Tip: 폴더를 더블클릭하면 그 안으로 들어가버려 선택이 풀립니다. 한 번만 클릭하세요.",
                "OK");
            return;
        }

        string enemyName = Path.GetFileName(folder);
        string controllerPath = $"{folder}/{enemyName}.controller";

        // 1. 폴더 내 시트 찾기 (대소문자 무시)
        string[] expectedStates = { "Idle", "Walk", "Attack", "Death", "Hurt" };
        var sheetByState = new Dictionary<string, Sprite[]>();
        foreach (var state in expectedStates)
        {
            var sprites = FindSheetSprites(folder, state);
            if (sprites != null && sprites.Length > 0)
                sheetByState[state] = sprites;
        }

        if (sheetByState.Count == 0)
        {
            EditorUtility.DisplayDialog("Build Enemy Animator",
                $"폴더 '{folder}'에서 Idle/Walk/Attack/Death/Hurt 시트를 찾지 못했습니다.\n" +
                "파일명이 'Walk.png' 'Idle.png' 같이 되어 있는지 확인하세요.",
                "OK");
            return;
        }

        // 2. AnimatorController 생성/재생성
        if (File.Exists(controllerPath)) AssetDatabase.DeleteAsset(controllerPath);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // 3. 트리거 파라미터 추가 (Attack/Death/Hurt만)
        foreach (var state in new[] { "Attack", "Death", "Hurt" })
            if (sheetByState.ContainsKey(state))
                controller.AddParameter(state, AnimatorControllerParameterType.Trigger);

        var rootSm = controller.layers[0].stateMachine;
        var statesByName = new Dictionary<string, AnimatorState>();

        // 4. 각 시트에 대해 Clip + State 생성
        foreach (var kv in sheetByState)
        {
            string stateName = kv.Key;
            var sprites = kv.Value;
            bool isLoop = stateName == "Idle" || stateName == "Walk";

            string clipPath = $"{folder}/{enemyName}_{stateName}.anim";
            var clip = BuildClip(sprites, isLoop);
            if (File.Exists(clipPath)) AssetDatabase.DeleteAsset(clipPath);
            AssetDatabase.CreateAsset(clip, clipPath);

            var state = rootSm.AddState(stateName);
            state.motion = clip;
            statesByName[stateName] = state;
        }

        // 5. 기본 상태 결정 (Walk 우선, 없으면 Idle, 둘 다 없으면 첫 상태)
        AnimatorState defaultState = null;
        if (statesByName.TryGetValue("Walk", out var walkState)) defaultState = walkState;
        else if (statesByName.TryGetValue("Idle", out var idleState)) defaultState = idleState;
        else foreach (var s in statesByName.Values) { defaultState = s; break; }
        if (defaultState != null) rootSm.defaultState = defaultState;

        // 6. 트리거 전이 추가
        foreach (var stateName in new[] { "Attack", "Hurt", "Death" })
        {
            if (!statesByName.TryGetValue(stateName, out var targetState)) continue;

            // AnyState → target (trigger)
            var anyT = rootSm.AddAnyStateTransition(targetState);
            anyT.AddCondition(AnimatorConditionMode.If, 0, stateName);
            anyT.duration = 0.05f;
            anyT.canTransitionToSelf = false;

            // Death는 종료 후 그대로(되살아나지 않음). Attack/Hurt는 종료 후 Default로 복귀.
            if (stateName != "Death" && defaultState != null)
            {
                var exitT = targetState.AddTransition(defaultState);
                exitT.hasExitTime = true;
                exitT.exitTime = 0.95f;
                exitT.duration = 0.05f;
            }
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Build Enemy Animator",
            $"'{enemyName}' 컨트롤러 생성 완료.\n" +
            $"상태: {string.Join(", ", sheetByState.Keys)}\n" +
            $"기본: {(defaultState != null ? defaultState.name : "(none)")}",
            "OK");
        Selection.activeObject = controller;
    }

    /// <summary>Selection에서 폴더 경로를 너그럽게 추출.
    /// 폴더면 그대로, 파일이면 부모 폴더, 다중 선택이면 첫 항목 기준.</summary>
    static string ResolveFolder()
    {
        // 1. Selection.assetGUIDs (Project 창에서 클릭한 모든 항목 — Hierarchy 선택은 제외)
        foreach (var guid in Selection.assetGUIDs)
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(p)) continue;
            if (AssetDatabase.IsValidFolder(p)) return p;
            // 파일이면 부모 폴더
            string parent = System.IO.Path.GetDirectoryName(p).Replace("\\", "/");
            if (AssetDatabase.IsValidFolder(parent)) return parent;
        }
        // 2. activeObject 백업
        var obj = Selection.activeObject;
        if (obj != null)
        {
            string p = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(p))
            {
                if (AssetDatabase.IsValidFolder(p)) return p;
                string parent = System.IO.Path.GetDirectoryName(p).Replace("\\", "/");
                if (AssetDatabase.IsValidFolder(parent)) return parent;
            }
        }
        return null;
    }

    /// <summary>폴더에서 stateName.png 시트를 찾아 sub-sprite들을 인덱스 순으로 반환.</summary>
    static Sprite[] FindSheetSprites(string folder, string stateName)
    {
        var guids = AssetDatabase.FindAssets($"{stateName} t:Texture2D", new[] { folder });
        foreach (var guid in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            // 정확히 stateName.png (대소문자 무시) — 다른 단어 포함 시 제외
            string fname = Path.GetFileNameWithoutExtension(p);
            if (!string.Equals(fname, stateName, System.StringComparison.OrdinalIgnoreCase)) continue;

            var assets = AssetDatabase.LoadAllAssetsAtPath(p);
            var list = new List<Sprite>();
            foreach (var a in assets)
                if (a is Sprite s) list.Add(s);

            // sub-sprite 이름의 trailing 숫자로 정렬
            list.Sort((a, b) => TrailingNumber(a.name).CompareTo(TrailingNumber(b.name)));
            if (list.Count > 0) return list.ToArray();
        }
        return null;
    }

    static int TrailingNumber(string name)
    {
        int end = name.Length;
        int start = end;
        while (start > 0 && char.IsDigit(name[start - 1])) start--;
        if (start == end) return 0;
        int.TryParse(name.Substring(start), out int n);
        return n;
    }

    static AnimationClip BuildClip(Sprite[] sprites, bool loop)
    {
        var clip = new AnimationClip { frameRate = FrameRate };
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        var binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite",
        };
        var keys = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / FrameRate, value = sprites[i] };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        return clip;
    }
}
