using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

[InitializeOnLoad]
public static class DenkiChipFontAssetCreator
{
    private const string SourceFontPath = "Assets/Art/Font/x10y12pxDenkiChipHangul (2).ttf";
    private const string OutputFontAssetPath = "Assets/Art/Font/DenkiChipHangul TMP.asset";

    static DenkiChipFontAssetCreator()
    {
        EditorApplication.delayCall += CreateIfMissing;
    }

    [MenuItem("Tools/Debt Pit/Create DenkiChip TMP Font")]
    public static void CreateIfMissing()
    {
        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
        if (sourceFont == null)
        {
            Debug.LogError($"Source font could not be loaded: {SourceFontPath}");
            return;
        }

        TMP_FontAsset existingFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(OutputFontAssetPath);
        if (existingFontAsset != null)
        {
            if (HasAtlasAndMaterial(existingFontAsset))
                return;

            RebuildMissingResources(existingFontAsset, sourceFont);
            return;
        }

        TMP_FontAsset fontAsset = CreateDynamicFontAsset(sourceFont);
        AssetDatabase.CreateAsset(fontAsset, OutputFontAssetPath);
        SaveSubAssets(fontAsset);
        Debug.Log($"Created dynamic TMP font asset: {OutputFontAssetPath}");
    }

    private static TMP_FontAsset CreateDynamicFontAsset(Font sourceFont)
    {
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont,
            90,
            9,
            GlyphRenderMode.SDFAA,
            2048,
            2048,
            AtlasPopulationMode.Dynamic,
            true);

        if (fontAsset == null)
            throw new System.InvalidOperationException("Failed to create the DenkiChip TMP font asset. Ensure Include Font Data is enabled on the source TTF.");

        fontAsset.name = "DenkiChipHangul TMP";
        return fontAsset;
    }

    private static bool HasAtlasAndMaterial(TMP_FontAsset fontAsset)
    {
        return fontAsset.atlasTextures != null
            && fontAsset.atlasTextures.Length > 0
            && fontAsset.atlasTextures[0] != null
            && fontAsset.material != null;
    }

    private static void RebuildMissingResources(TMP_FontAsset target, Font sourceFont)
    {
        TMP_FontAsset rebuilt = CreateDynamicFontAsset(sourceFont);
        EditorUtility.CopySerialized(rebuilt, target);
        target.name = "DenkiChipHangul TMP";
        SaveSubAssets(target);
        UnityEngine.Object.DestroyImmediate(rebuilt);
        Debug.Log($"Rebuilt missing atlas resources for TMP font asset: {OutputFontAssetPath}");
    }

    private static void SaveSubAssets(TMP_FontAsset fontAsset)
    {
        foreach (var atlasTexture in fontAsset.atlasTextures)
            AssetDatabase.AddObjectToAsset(atlasTexture, fontAsset);

        AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(OutputFontAssetPath, ImportAssetOptions.ForceUpdate);
    }
}
