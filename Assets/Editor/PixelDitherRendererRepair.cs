using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Creates the full-screen renderer feature through AssetDatabase, which keeps
/// URP's renderer-feature map and sub-asset identifiers valid in Unity 6.3.
/// </summary>
[InitializeOnLoad]
public static class PixelDitherRendererRepair
{
    private const string RendererPath = "Assets/Settings/Rendering/New Universal Render Pipeline Asset_Renderer.asset";
    private const string MaterialPath = "Assets/Art/Materials/PixelDitherPostProcess.mat";

    static PixelDitherRendererRepair()
    {
        EditorApplication.delayCall += EnsureFeature;
    }

    private static void EnsureFeature()
    {
        var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (rendererData == null || material == null)
            return;

        var existing = rendererData.rendererFeatures
            .OfType<FullScreenPassRendererFeature>()
            .FirstOrDefault(feature => feature != null && feature.name == "Pixel Dither Post Process");

        if (existing != null)
        {
            if (existing.passMaterial != material)
            {
                existing.passMaterial = material;
                EditorUtility.SetDirty(existing);
                AssetDatabase.SaveAssets();
            }
            return;
        }

        var feature = ScriptableObject.CreateInstance<FullScreenPassRendererFeature>();
        feature.name = "Pixel Dither Post Process";
        feature.injectionPoint = FullScreenPassRendererFeature.InjectionPoint.AfterRenderingPostProcessing;
        feature.fetchColorBuffer = true;
        feature.requirements = ScriptableRenderPassInput.None;
        feature.passMaterial = material;
        feature.passIndex = 0;
        feature.bindDepthStencilAttachment = false;

        AssetDatabase.AddObjectToAsset(feature, rendererData);
        rendererData.rendererFeatures.Add(feature);
        EditorUtility.SetDirty(feature);
        EditorUtility.SetDirty(rendererData);
        AssetDatabase.SaveAssets();
    }
}
