using UnityEngine;

/// <summary>
/// 타일링 바닥 배경 — 카메라를 따라다니되 위치를 tileSize 격자에 스냅시켜
/// 타일 패턴이 미끄러지지 않고 "무한 바닥"처럼 보이게 한다.
/// SpriteRenderer(drawMode=Tiled, sprite wrap=Repeat)와 함께 사용.
/// 카메라 뷰를 덮을 만큼 size를 크게 잡아두면 됨.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class InfiniteTiledBackground : MonoBehaviour
{
    [Tooltip("타일 한 칸의 월드 크기 (PPU와 일치, 보통 1)")]
    [SerializeField] private float tileSize = 1f;

    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }
        Vector3 cp = cam.transform.position;
        float x = Mathf.Round(cp.x / tileSize) * tileSize;
        float y = Mathf.Round(cp.y / tileSize) * tileSize;
        transform.position = new Vector3(x, y, transform.position.z);
    }
}
