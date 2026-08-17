#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Rendering;

[InitializeOnLoad]
public static class WindowsGraphicsApiSetup
{
    static WindowsGraphicsApiSetup()
    {
        BuildTarget target = BuildTarget.StandaloneWindows64;
        GraphicsDeviceType[] current = PlayerSettings.GetGraphicsAPIs(target);
        if (current.Length == 1 && current[0] == GraphicsDeviceType.Direct3D11) return;

        PlayerSettings.SetUseDefaultGraphicsAPIs(target, false);
        PlayerSettings.SetGraphicsAPIs(target, new[] { GraphicsDeviceType.Direct3D11 });
    }
}
#endif
