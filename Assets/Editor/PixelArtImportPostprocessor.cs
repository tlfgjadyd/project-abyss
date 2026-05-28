using UnityEditor;
using UnityEngine;

/// <summary>
/// Assets/Sprites/ 하위로 임포트되는 모든 텍스처에 픽셀 아트 표준 설정을 자동 적용.
/// (8주차 에셋 도입 — 매번 인스펙터에서 PPU/필터를 손볼 필요 없음)
///
/// 적용 설정: Sprite(2D and UI) / PPU 32 / Point(no filter) / Compression None / mipmap off.
/// - 이미 잘못된 설정으로 임포트된 파일은 우클릭 → Reimport 하면 재적용됨.
/// - 스프라이트 시트(여러 프레임)는 임포트 후 수동으로 Sprite Mode = Multiple + Slice 진행.
/// </summary>
public class PixelArtImportPostprocessor : AssetPostprocessor
{
    const string TargetFolder = "Assets/Sprites/";
    const int    PixelsPerUnit = 32;

    void OnPreprocessTexture()
    {
        string path = assetPath.Replace("\\", "/");
        if (!path.StartsWith(TargetFolder)) return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType         = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.filterMode          = FilterMode.Point;
        importer.textureCompression  = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled       = false;
        importer.alphaIsTransparency = true;

        // 단일 스프라이트 기본. 시트는 사용자가 수동으로 Multiple 전환.
        if (importer.spriteImportMode == SpriteImportMode.None)
            importer.spriteImportMode = SpriteImportMode.Single;
    }
}
