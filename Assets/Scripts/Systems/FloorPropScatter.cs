using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 바닥 타일 격자에 맞춰 장식 프롭(환풍구/패널 등)을 드문드문 박아넣는다.
/// gridAlign=true면 각 프롭을 타일 셀 중심에 스냅 + 셀 중복 방지하여
/// "그 칸의 바닥 타일이 환풍구로 바뀐" 것처럼 보이게 한다. 충돌 없는 순수 시각 장식.
/// 고정 seed로 재현 가능. 런타임 Start에서 1회 생성.
/// </summary>
public class FloorPropScatter : MonoBehaviour
{
    [Tooltip("흩뿌릴 프롭 sprite 목록 (랜덤 선택)")]
    [SerializeField] private Sprite[] props;
    [Tooltip("총 개수")]
    [SerializeField] private int count = 110;
    [Tooltip("배치 영역 (중심 기준 가로/세로 전체 크기)")]
    [SerializeField] private Vector2 area = new Vector2(120f, 120f);
    [SerializeField] private int seed = 1234;
    [Tooltip("정렬 순서 (바닥 -100 위, 엔티티 0 아래)")]
    [SerializeField] private int sortingOrder = -99;
    [SerializeField] private Material material;
    [Tooltip("밝기 변주 최소/최대")]
    [SerializeField] private float minBrightness = 0.95f;
    [SerializeField] private float maxBrightness = 1f;
    [Tooltip("프롭 색 틴트 (바닥 틴트와 맞춤. 기본 흰색)")]
    [SerializeField] private Color tint = Color.white;

    [Header("격자 정렬")]
    [Tooltip("타일 셀 중심에 스냅 (바닥 타일을 대체하는 느낌)")]
    [SerializeField] private bool gridAlign = true;
    [Tooltip("타일 한 칸 크기 (바닥 PPU와 일치, 보통 1)")]
    [SerializeField] private float gridSize = 1f;
    [Tooltip("셀 중심 오프셋 (Tiled 바닥 타일 중심이 half-integer라 0.5)")]
    [SerializeField] private float gridOffset = 0.5f;
    [Tooltip("90도 단위 랜덤 회전 (격자 정렬 시엔 보통 끔)")]
    [SerializeField] private bool randomRotate90 = false;

    void Start()
    {
        Generate();
    }

    void Generate()
    {
        if (props == null || props.Length == 0) return;
        var rng = new System.Random(seed);
        var used = new HashSet<Vector2Int>();
        int placed = 0, guard = 0;

        while (placed < count && guard < count * 20)
        {
            guard++;
            float x = ((float)rng.NextDouble() - 0.5f) * area.x;
            float y = ((float)rng.NextDouble() - 0.5f) * area.y;

            if (gridAlign)
            {
                x = Mathf.Round((x - gridOffset) / gridSize) * gridSize + gridOffset;
                y = Mathf.Round((y - gridOffset) / gridSize) * gridSize + gridOffset;
                var cell = new Vector2Int(Mathf.RoundToInt(x / gridSize), Mathf.RoundToInt(y / gridSize));
                if (!used.Add(cell)) continue; // 같은 칸 중복 방지
            }

            var go = new GameObject("Prop_" + placed);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(x, y, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = props[rng.Next(props.Length)];
            if (material != null) sr.sharedMaterial = material;
            sr.sortingOrder = sortingOrder;

            float b = Mathf.Lerp(minBrightness, maxBrightness, (float)rng.NextDouble());
            sr.color = new Color(tint.r * b, tint.g * b, tint.b * b, tint.a);

            if (randomRotate90)
                go.transform.localRotation = Quaternion.Euler(0f, 0f, 90f * rng.Next(4));

            placed++;
        }
    }
}
