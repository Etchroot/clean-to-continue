using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CleanToContinue.Editor
{
    public static class WebBuildCommand
    {
        public const string MenuPath = "Clean to Continue/Build Web Release";
        public const string OutputPath = "Builds/WebGL";

        [MenuItem(MenuPath)]
        public static void BuildWebRelease()
        {
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            if (scenes.Length != 6)
            {
                throw new InvalidOperationException($"Expected six enabled scenes, found {scenes.Length}.");
            }

            Directory.CreateDirectory(Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, OutputPath));
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Web build failed: {report.summary.result}, {report.summary.totalErrors} errors.");
            }

            Debug.Log($"[Clean to Continue] Web build succeeded: {report.summary.totalSize} bytes at {OutputPath}");
        }
    }
}
