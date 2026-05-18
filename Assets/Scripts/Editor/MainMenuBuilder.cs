#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Day 40 MainMenu 씬 UI를 코드로 생성하는 일회성 에디터 헬퍼.
/// 메뉴에서 호출하거나, MCP execute_code에서 MainMenuBuilder.Build() 한 줄로 호출.
/// </summary>
public static class MainMenuBuilder
{
    public static string Build()
    {
        var noto = LoadNotoFont();

        // Camera
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.1f, 1f);
        cam.orthographic = true;
        cam.orthographicSize = 5;
        camGo.AddComponent<AudioListener>();

        // EventSystem
        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<EventSystem>();
        esGo.AddComponent<StandaloneInputModule>();

        // Canvas
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        // Main Panel
        var mainPanel = MakePanel("MainPanel", canvasGo.transform, new Color(0, 0, 0, 0));
        MakeText("Title", mainPanel.transform, "PROJECT: ABYSS", 96, noto,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(900, 120), new Vector2(0, -180), TextAlignmentOptions.Center);

        var startBtn = MakeButton("StartButton", mainPanel.transform, "START", noto,
            new Vector2(360, 90), new Vector2(0, 60));
        var metaBtn = MakeButton("MetaButton", mainPanel.transform, "META UPGRADE", noto,
            new Vector2(360, 90), new Vector2(0, -60));
        var quitBtn = MakeButton("QuitButton", mainPanel.transform, "QUIT", noto,
            new Vector2(360, 90), new Vector2(0, -180));

        var mmc = canvasGo.AddComponent<MainMenuController>();
        SetField(mmc, "mainPanel", mainPanel);
        SetField(mmc, "startButton", startBtn);
        SetField(mmc, "metaButton", metaBtn);
        SetField(mmc, "quitButton", quitBtn);

        // Meta Panel
        var metaPanel = MakePanel("MetaPanel", canvasGo.transform, new Color(0, 0, 0, 0.85f));
        MakeText("Title", metaPanel.transform, "META UPGRADE", 72, noto,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(700, 100), new Vector2(0, -80), TextAlignmentOptions.Center);
        var cellsText = MakeText("CellsText", metaPanel.transform, "누적 세포: 0", 42, noto,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(700, 60), new Vector2(0, -200), TextAlignmentOptions.Center);

        string[] statNames = { "최대 HP", "이동속도", "공격속도", "공격력", "압력 저항" };
        MetaProgressData.Stat[] stats = {
            MetaProgressData.Stat.MaxHp, MetaProgressData.Stat.MoveSpeed,
            MetaProgressData.Stat.AttackSpeed, MetaProgressData.Stat.AttackPower,
            MetaProgressData.Stat.PressureResistance
        };

        var rows = new List<MetaUpgradeRow>();
        for (int i = 0; i < 5; i++)
        {
            float y = -280 - i * 90;
            var rowGo = new GameObject("Row_" + statNames[i]);
            rowGo.transform.SetParent(metaPanel.transform, false);
            var rrt = rowGo.AddComponent<RectTransform>();
            rrt.anchorMin = new Vector2(0.5f, 1f);
            rrt.anchorMax = new Vector2(0.5f, 1f);
            rrt.pivot     = new Vector2(0.5f, 1f);
            rrt.sizeDelta = new Vector2(1100, 80);
            rrt.anchoredPosition = new Vector2(0, y);

            var rowBg = rowGo.AddComponent<Image>();
            rowBg.color = new Color(0.15f, 0.15f, 0.2f, 0.7f);

            var nameT = MakeText("Name", rowGo.transform, statNames[i], 30, noto,
                new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f),
                new Vector2(220, 0), new Vector2(20, 0), TextAlignmentOptions.Left);
            var lvT = MakeText("Lv", rowGo.transform, "Lv 0 / 4", 28, noto,
                new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f),
                new Vector2(160, 0), new Vector2(260, 0), TextAlignmentOptions.Left);
            var fxT = MakeText("Effect", rowGo.transform, "+0", 26, noto,
                new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f),
                new Vector2(260, 0), new Vector2(440, 0), TextAlignmentOptions.Left);
            var costT = MakeText("Cost", rowGo.transform, "비용: 5", 26, noto,
                new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f),
                new Vector2(180, 0), new Vector2(720, 0), TextAlignmentOptions.Left);

            var buyBtn = MakeButtonAnchored("Buy", rowGo.transform, "구매", noto,
                new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(160, 60), new Vector2(-20, 0));

            var row = rowGo.AddComponent<MetaUpgradeRow>();
            var rso = new SerializedObject(row);
            rso.FindProperty("stat").enumValueIndex = (int)stats[i];
            rso.FindProperty("statNameText").objectReferenceValue    = nameT;
            rso.FindProperty("levelText").objectReferenceValue       = lvT;
            rso.FindProperty("effectText").objectReferenceValue      = fxT;
            rso.FindProperty("costText").objectReferenceValue        = costT;
            rso.FindProperty("purchaseButton").objectReferenceValue  = buyBtn;
            rso.FindProperty("statDisplayName").stringValue          = statNames[i];
            rso.ApplyModifiedProperties();
            rows.Add(row);
        }

        var backBtn = MakeButtonAnchored("BackButton", metaPanel.transform, "← 메인 메뉴", noto,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(280, 80), new Vector2(40, 40));

        var mup = metaPanel.AddComponent<MetaUpgradePanel>();
        var mupSo = new SerializedObject(mup);
        mupSo.FindProperty("cellsText").objectReferenceValue = cellsText;
        var rowsProp = mupSo.FindProperty("rows");
        rowsProp.arraySize = rows.Count;
        for (int i = 0; i < rows.Count; i++)
            rowsProp.GetArrayElementAtIndex(i).objectReferenceValue = rows[i];
        mupSo.ApplyModifiedProperties();

        SetField(mmc, "metaPanel", metaPanel);
        SetField(mmc, "metaBackButton", backBtn);

        metaPanel.SetActive(false);

        return "MainMenu UI 구성 완료 (rows=" + rows.Count + ")";
    }

    // ─────────────────────────────────────────────
    // 헬퍼들

    static TMP_FontAsset LoadNotoFont()
    {
        var guids = AssetDatabase.FindAssets("NotoSansKR-Regular SDF t:TMP_FontAsset");
        if (guids.Length == 0) return null;
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    static GameObject MakePanel(string name, Transform parent, Color bg)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        if (bg.a > 0)
        {
            var img = go.AddComponent<Image>();
            img.color = bg;
        }
        return go;
    }

    static TMP_Text MakeText(string name, Transform parent, string text, int fontSize, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos,
        TextAlignmentOptions align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.font = font;
        t.fontSize = fontSize;
        t.alignment = align;
        t.color = Color.white;
        var rt = t.rectTransform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = anchoredPos;
        return t;
    }

    static Button MakeButton(string name, Transform parent, string label, TMP_FontAsset font,
        Vector2 sizeDelta, Vector2 anchoredPos)
    {
        return MakeButtonAnchored(name, parent, label, font,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            sizeDelta, anchoredPos);
    }

    static Button MakeButtonAnchored(string name, Transform parent, string label, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.3f, 0.9f);
        var btn = go.AddComponent<Button>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = anchoredPos;
        MakeText("Label", go.transform, label, 32, font,
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);
        return btn;
    }

    static void SetField(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(field);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }
    }
}
#endif
