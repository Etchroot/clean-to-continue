using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace CleanToContinue.Editor
{
    [InitializeOnLoad]
    public static class EditModeTestCommand
    {
        public const string EditModeMenuPath = "Tools/Clean to Continue/Run EditMode Tests";
        public const string PlayModeMenuPath = "Tools/Clean to Continue/Run PlayMode Tests";

        private const string ActiveResultPathKey = "CleanToContinue.ActiveTestResultPath";

        private static readonly ResultCallbacks Callbacks = new ResultCallbacks();
        private static TestRunnerApi runner;

        static EditModeTestCommand()
        {
            RegisterCallbacks();
        }

        [MenuItem(EditModeMenuPath)]
        public static void RunEditMode()
        {
            Run(TestMode.EditMode, "CleanToContinue.EditModeTests", "editmode-latest.json");
        }

        [MenuItem(PlayModeMenuPath)]
        public static void RunPlayMode()
        {
            Run(TestMode.PlayMode, "CleanToContinue.PlayModeTests", "playmode-latest.json");
        }

        private static void Run(TestMode mode, string assemblyName, string resultFileName)
        {
            RegisterCallbacks();
            var resultPath = Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                "..",
                "TestResults",
                resultFileName));
            SessionState.SetString(ActiveResultPathKey, resultPath);

            var directory = Path.GetDirectoryName(resultPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(resultPath))
            {
                File.Delete(resultPath);
            }

            runner.Execute(new ExecutionSettings(new Filter
            {
                testMode = mode,
                assemblyNames = new[] { assemblyName }
            }));

            Debug.Log($"[CTC_TEST] {mode} tests started.");
        }

        private static void RegisterCallbacks()
        {
            if (runner != null)
            {
                runner.UnregisterCallbacks(Callbacks);
                Object.DestroyImmediate(runner);
            }

            runner = ScriptableObject.CreateInstance<TestRunnerApi>();
            runner.RegisterCallbacks(Callbacks);
        }

        private sealed class ResultCallbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }

            public void RunFinished(ITestResultAdaptor result)
            {
                var report = new TestReport
                {
                    status = result.TestStatus.ToString(),
                    passed = result.PassCount,
                    failed = result.FailCount,
                    skipped = result.SkipCount
                };

                var resultPath = SessionState.GetString(ActiveResultPathKey, string.Empty);
                if (!string.IsNullOrEmpty(resultPath))
                {
                    File.WriteAllText(resultPath, JsonUtility.ToJson(report, true));
                }
                Debug.Log($"[CTC_TEST] Finished: {report.passed} passed, {report.failed} failed, {report.skipped} skipped.");
            }

            public void TestStarted(ITestAdaptor test) { }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result.TestStatus == TestStatus.Failed)
                {
                    Debug.LogError($"[CTC_TEST] {result.Name}: {result.Message}");
                }
            }
        }

        [System.Serializable]
        private sealed class TestReport
        {
            public string status;
            public int passed;
            public int failed;
            public int skipped;
        }
    }
}
