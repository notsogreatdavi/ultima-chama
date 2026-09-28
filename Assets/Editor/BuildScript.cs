using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Build WebGL por linha de comando:
// Unity -batchmode -quit -projectPath . -executeMethod BuildScript.BuildWebGL
public static class BuildScript
{
    [MenuItem("Build/WebGL")]
    public static void BuildWebGL()
    {
        // Sem compressão: roda no GitHub Pages sem configurar headers do servidor.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.productName = "Ultima Chama";

        string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (scenes.Length == 0) scenes = new[] { "Assets/Scenes/SampleScene.unity" };

        BuildReport report = BuildPipeline.BuildPlayer(scenes, "Builds/WebGL", BuildTarget.WebGL, BuildOptions.None);
        Debug.Log("Build WebGL: " + report.summary.result);
        if (Application.isBatchMode && report.summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
    }
}
