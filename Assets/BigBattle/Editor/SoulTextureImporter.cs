using UnityEditor;
using UnityEngine;

namespace BigBattle.Editor
{
    public sealed class SoulTextureImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/BigBattle/Resources/SoulVfx/") || !assetPath.EndsWith(".png"))return;
            var importer=(TextureImporter)assetImporter;importer.textureType=TextureImporterType.Default;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;
            importer.maxTextureSize=512;importer.textureCompression=TextureImporterCompression.CompressedHQ;
            importer.filterMode=FilterMode.Bilinear;
        }
    }
}
