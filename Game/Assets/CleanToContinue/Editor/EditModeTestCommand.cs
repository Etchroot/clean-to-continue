using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace CleanToContinue.Editor
{
    [InitializeOnLoad]
    public static class EditModeTestCommand
    {
        public const string MenuPath = "Tools/Clean to Continue/Run EditMode Tests";

        private static readonly string ResultPath = Path.GetFullPath(
            Path.Combine(Application.dataPath, "..", "..", "TestResults", "editmode-latest.json"));

        private static readonly ResultCallbacks Callbacks = new ResultCallbacks(ResultPath);
        private static TestRunnerApi runner;

        static EditModeTestCommand()
        {
            RegisterCallbacks();
        }

        [MenuItem(MenuPath)]
        public static void RunAll()
        {
            RegisterCallbacks();

            var directory = Path.GetDirectoryName(ResultPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(ResultPath))
            {
                File.Delete(ResultPath);
            }

            runner.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = new[] { "CleanToContinue.EditModeTests" }
            }));

            Debug.Log("[CTC_TEST] EditMode tests started.");
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
            private readonly string resultPath;

            public ResultCallbacks(string resultPath)
            {
                this.resultPath = resultPath;
            }

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

                File.WriteAllText(resultPath, JsonUtility.ToJson(report, true));
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
