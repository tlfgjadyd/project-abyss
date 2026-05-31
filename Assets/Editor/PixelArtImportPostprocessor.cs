using UnityEditor;
using UnityEngine;

/// <summary>
/// Assets/Sprites/ 하위로 임포트되는 모든 텍스처에 픽셀 아트 표준 설정을 자동 적용.
///
/// 적용 설정: Sprite(2D and UI) / PPU 32 / Point(no filter) / Compression None / mipmap off.
/// - 이미 임포트된 파일(.meta 존재)은 건드리지 않음 → 사용자가 PPU/슬라이스 수정해도 안전.
/// - 균일 가로 시트(width % height == 0 && width > height)는 자동으로 Multiple+Grid 슬라이스.
///   예: Walk.png 192x48 → 48x48 프레임 4개로 자동 분할.
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

        // 최초 임포트 판별:
        //   - .meta가 없거나
        //   - .meta는 있지만 importer가 아직 기본 타입(Default)인 경우
        //     (Unity가 PNG 추가 시 .meta를 즉시 생성하므로 .meta 존재만으론 부족)
        string metaPath = path + ".meta";
        bool isFirstImport = !System.IO.File.Exists(metaPath)
                             || importer.textureType == TextureImporterType.Default;
        if (!isFirstImport) return;

        importer.textureType         = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.filterMode          = FilterMode.Point;
        importer.textureCompression  = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled       = false;
        importer.alphaIsTransparency = true;

        if (importer.spriteImportMode == SpriteImportMode.None)
            importer.spriteImportMode = SpriteImportMode.Single;
    }

    // OnPreprocess 단계에선 텍스처 크기를 알 수 없어서 OnPostprocess에서 슬라이스 결정.
    void OnPostprocessTexture(Texture2D tex)
    {
        string path = assetPath.Replace("\\", "/");
        if (!path.StartsWith(TargetFolder)) return;
        var importer = (TextureImporter)assetImporter;
        // 이미 슬라이스 설정이 있거나 Multiple이면 패스 (사용자 설정 존중)
        if (importer.spriteImportMode == SpriteImportMode.Multiple) return;
        // 단일 정사각/세로형은 단일 스프라이트로 두기
        if (tex.width <= tex.height) return;
        // 가로가 세로의 배수가 아니면 균일 시트 아님 → 패스
        if (tex.width % tex.height != 0) return;
        int frameCount = tex.width / tex.height;
        if (frameCount < 2 || frameCount > 32) return; // 합리적 범위

        // 균일 가로 시트로 판단 → 자동 슬라이스
        int frameSize = tex.height;
        var sheet = new SpriteMetaData[frameCount];
        string baseName = System.IO.Path.GetFileNameWithoutExtension(path);
        for (int i = 0; i < frameCount; i++)
        {
            sheet[i] = new SpriteMetaData
            {
                rect      = new Rect(i * frameSize, 0, frameSize, frameSize),
                alignment = (int)SpriteAlignment.Center,
                pivot     = new Vector2(0.5f, 0.5f),
                name      = baseName + "_" + i,
            };
        }
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritesheet = sheet;
        // 슬라이스 메타를 디스크에 반영
        EditorApplication.delayCall += () => importer.SaveAndReimport();
    }
}
