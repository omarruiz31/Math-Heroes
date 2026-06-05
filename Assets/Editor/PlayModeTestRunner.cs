using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace Unity.AI.Assistant.PlayModeTest
{
    [InitializeOnLoad]
    internal static class PlayModeTestRunner
    {
        private const string StateKey = "PlayModeTest.State";
        private const string ResultKey = "PlayModeTest.Result";
        private const string ScriptPathKey = "PlayModeTest.ScriptPath";
        private const string SentinelLog = "PLAY_MODE_TEST_COMPLETE";

        private static readonly int WaitFrames = SessionState.GetInt("PlayModeTest.WaitFrames", 10);
        private static readonly float TestTimeout = SessionState.GetFloat("PlayModeTest.TestTimeout", 15.0f);

        private static List<string> _capturedLogs = new List<string>();
        private const int MaxCapturedLogs = 50;

        static PlayModeTestRunner()
        {
            string state = SessionState.GetString(StateKey, "Idle");
            switch (state)
            {
                case "WaitingForCompile":
                    EditorApplication.delayCall += () =>
                    {
                        SessionState.SetString(StateKey, "EnteringPlayMode");
                        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                        EditorApplication.isPlaying = true;
                    };
                    break;
                case "EnteringPlayMode":
                    if (EditorApplication.isPlaying)
                    {
                        SessionState.SetString(StateKey, "InPlayMode");
                        EditorApplication.update += WaitFramesThenRun;
                    }
                    break;
                case "InPlayMode":
                    if (EditorApplication.isPlaying) EditorApplication.update += WaitFramesThenRun;
                    break;
                case "Done":
                    EditorApplication.delayCall += SelfDestruct;
                    break;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                SessionState.SetString(StateKey, "InPlayMode");
                EditorApplication.update += WaitFramesThenRun;
            }
        }

        private static int _frameCount = 0;
        private static bool _setupDone = false;
        private static bool _testDone = false;
        private static double _testStartTime = 0;
        private static int _stepClicked = 0;
        private static BattleTutorial _tutorial;
        private static float _lastClickTime = 0;

        private static void WaitFramesThenRun()
        {
            _frameCount++;
            if (_frameCount < WaitFrames) return;
            if (_testDone) return;

            if (!_setupDone)
            {
                _setupDone = true;
                Application.logMessageReceived += OnLogMessage;
                _testStartTime = EditorApplication.timeSinceStartup;
                try { Setup(); }
                catch (System.Exception e) { FinishTest(true, e.Message); return; }
                return;
            }

            float elapsed = (float)(EditorApplication.timeSinceStartup - _testStartTime);
            if (elapsed >= TestTimeout) { FinishTest(true, "Timeout after " + elapsed + "s"); return; }

            try
            {
                if (Tick(elapsed)) FinishTest(false, null);
            }
            catch (System.Exception e) { FinishTest(true, e.Message); }
        }

        private static void FinishTest(bool isError, string errorMessage)
        {
            _testDone = true;
            EditorApplication.update -= WaitFramesThenRun;
            Application.logMessageReceived -= OnLogMessage;
            string resultJson = JsonUtility.ToJson(new TestResult { success = !isError, error = errorMessage, logs = _capturedLogs.ToArray() });
            SessionState.SetString(ResultKey, resultJson);
            SessionState.SetString(StateKey, "Done");
            EditorApplication.isPlaying = false;
        }

        private static void OnLogMessage(string message, string stackTrace, LogType type)
        {
            if (_capturedLogs.Count < MaxCapturedLogs) _capturedLogs.Add("[" + type + "] " + message);
        }

        private static void SelfDestruct()
        {
            string scriptPath = SessionState.GetString(ScriptPathKey, "");
            if (!string.IsNullOrEmpty(scriptPath)) AssetDatabase.DeleteAsset(scriptPath);
            SessionState.EraseString(StateKey);
        }

        [System.Serializable]
        private class TestResult { public bool success; public string error; public string[] logs; }

        private static void Setup()
        {
            _tutorial = Object.FindFirstObjectByType<BattleTutorial>();
            if (_tutorial == null) throw new System.Exception("BattleTutorial not found");
            Debug.Log("[Test] Found BattleTutorial on " + _tutorial.gameObject.name);
            if (!_tutorial.tutorialRoot.activeInHierarchy) {
                Debug.Log("[Test] Tutorial not active, starting it...");
                BattleUI bui = Object.FindFirstObjectByType<BattleUI>();
                _tutorial.StartTutorial(bui);
            }
        }

        private static bool Tick(float elapsed)
        {
            if (_tutorial == null) return true;
            if (elapsed - _lastClickTime < 0.5f) return false;

            if (_tutorial.nextButton != null && _tutorial.nextButton.interactable)
            {
                Debug.Log("[Test] Clicking NextButton. Step count: " + _stepClicked);
                _tutorial.nextButton.onClick.Invoke();
                _stepClicked++;
                _lastClickTime = elapsed;
                
                if (_stepClicked >= 6) {
                    Debug.Log("[Test] Reached final step.");
                    return true;
                }
            }
            return false;
        }
    }
}
