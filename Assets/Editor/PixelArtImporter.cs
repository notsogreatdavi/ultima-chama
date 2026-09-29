using System.IO;
using UnityEditor;
using UnityEngine;

// Configura automaticamente os PNGs gerados por tools/sprites como pixel art.
public class PixelArtImporter : AssetPostprocessor
{
    const string Root = "Assets/Resources/Sprites/";

    void OnPreprocessTexture()
    {
        string path = assetPath.Replace('\\', '/');
        if (!path.StartsWith(Root)) return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 16;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        // a UI é ampliada em tempo de execução, então precisa ser legível
        importer.isReadable = path.Contains("/ui/");

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
    }

    [MenuItem("Tools/Reimportar sprites")]
    static void Reimport()
    {
        AssetDatabase.ImportAsset("Assets/Resources/Sprites", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
    }
}

// Garante um material de sprite sem iluminação dentro de Resources,
// assim o shader entra no build e os sprites que brilham ignoram a luz 2D.
[InitializeOnLoad]
static class UnlitMaterialSetup
{
    const string MaterialPath = "Assets/Resources/Materials/SpriteUnlit.mat";

    static UnlitMaterialSetup()
    {
        EditorApplication.delayCall += Ensure;
    }

    static void Ensure()
    {
        if (AssetDatabase.LoadAssetAtPath<Material>(MaterialPath) != null) return;

        var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null)
        {
            Debug.LogWarning("Shader Sprite-Unlit-Default não encontrado; sprites que brilham ficarão iluminados.");
            return;
        }
        Directory.CreateDirectory("Assets/Resources/Materials");
        AssetDatabase.CreateAsset(new Material(shader), MaterialPath);
        AssetDatabase.SaveAssets();
    }
}
