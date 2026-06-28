using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using SignalRelicRecovery;

namespace SignalRelicRecovery.Editor
{
    public static class SetupScene
    {
        [MenuItem("Signal Relic Recovery/Setup Main Scene")]
        public static void Run()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainScene";

            // Load assets.
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/ScriptableObjects/GameConfig.asset");
            var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");

            // Voice clips will be assigned after they are loaded below.
            // We defer saving the config asset until all clips are available.
            var relicPrefab = AssetDatabase.LoadAssetAtPath<RelicStation>("Assets/Prefabs/RelicStation.prefab");
            var stationMats = new[]
            {
                AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Station_Hum.mat"),
                AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Station_Buzz.mat"),
                AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Station_Beep.mat"),
                AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Station_Drip.mat"),
                AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Station_Pulse.mat")
            };
            var stationClips = new[]
            {
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Station_Hum.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Station_Buzz.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Station_Beep.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Station_Drip.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Station_Pulse.mp3")
            };

            var voiceClips = new[]
            {
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voice/Target_Hum.wav"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voice/Target_Buzz.wav"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voice/Target_Beep.wav"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voice/Target_Drip.wav"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voice/Target_Pulse.wav")
            };
            var introClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voice/intro.wav");
            var instructionsClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voice/instructions.wav");

            // Menu/context voice clips for accessibility navigation.
            var menuContextClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voice/Menu/MenuContext.wav");
            var resultsContextClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voice/Menu/ResultsContext.wav");
            var instructionsContextClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voice/Menu/InstructionsContext.wav");
            var gameplayMenuHintClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voice/Menu/GameplayMenuHint.wav");
            var accessibilityEnabledClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voice/Menu/AccessibilityEnabled.wav");
            var startButtonClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voice/Menu/StartButton.wav");
            var instructionsButtonClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voice/Menu/InstructionsButton.wav");
            var listenInstructionsButtonClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voice/Menu/ListenInstructionsButton.wav");
            var accessibilityToggleClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voice/Menu/AccessibilityToggle.wav");
            var quitButtonClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voice/Menu/QuitButton.wav");

            // Assign voice clips to the shared config asset so GameManager and UIManager can use them.
            if (config != null)
            {
                config.introVoiceClip = introClip;
                config.instructionsVoiceClip = instructionsClip;
                if (config.targetDescriptorClips == null || config.targetDescriptorClips.Length != voiceClips.Length)
                    config.targetDescriptorClips = new AudioClip[voiceClips.Length];
                for (int i = 0; i < voiceClips.Length; i++)
                    config.targetDescriptorClips[i] = voiceClips[i];

                config.menuContextClip = menuContextClip;
                config.resultsContextClip = resultsContextClip;
                config.instructionsContextClip = instructionsContextClip;
                config.gameplayMenuHintClip = gameplayMenuHintClip;
                config.accessibilityEnabledClip = accessibilityEnabledClip;
            }

            // Camera.
            var cameraObj = new GameObject("Main Camera");
            var camera = cameraObj.AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 6, -8);
            camera.transform.rotation = Quaternion.Euler(45, 0, 0);
            cameraObj.AddComponent<UniversalAdditionalCameraData>();
            cameraObj.AddComponent<AudioListener>();
            cameraObj.tag = "MainCamera";

            // Light.
            var lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.5f;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

            // Environment / floor.
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.localScale = new Vector3(3, 1, 3);
            var floorRenderer = floor.GetComponent<Renderer>();
            var floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            floorMat.SetColor("_BaseColor", new Color(0.15f, 0.15f, 0.18f));
            floorRenderer.sharedMaterial = floorMat;

            // Spawn points parent.
            var spawnParent = new GameObject("StationSpawnPoints");
            var spawnPoints = new Transform[5];
            Vector3[] positions =
            {
                new Vector3(-4, 0.5f, 2),
                new Vector3(-2, 0.5f, -3),
                new Vector3(0, 0.5f, 3),
                new Vector3(2, 0.5f, -3),
                new Vector3(4, 0.5f, 2)
            };
            for (int i = 0; i < 5; i++)
            {
                var pt = new GameObject($"SpawnPoint_{i}");
                pt.transform.SetParent(spawnParent.transform);
                pt.transform.position = positions[i];
                spawnPoints[i] = pt.transform;
            }

            // Managers.
            var managersObj = new GameObject("Managers");

            var inputReader = managersObj.AddComponent<InputReader>();
            var inputSo = new SerializedObject(inputReader);
            inputSo.FindProperty("inputAsset").objectReferenceValue = inputAsset;
            inputSo.ApplyModifiedProperties();

            var audioManager = managersObj.AddComponent<AudioManager>();
            var audioSo = new SerializedObject(audioManager);
            audioSo.FindProperty("config").objectReferenceValue = config;
            var sfxSource = managersObj.AddComponent<AudioSource>();
            audioSo.FindProperty("sfxSource").objectReferenceValue = sfxSource;
            audioSo.FindProperty("correctClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX_Correct.wav");
            audioSo.FindProperty("wrongClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX_Wrong.wav");
            audioSo.FindProperty("focusChangeClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX_Focus.wav");
            audioSo.FindProperty("roundStartClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX_RoundStart.wav");
            audioSo.FindProperty("resultsClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX_Results.wav");
            audioSo.ApplyModifiedProperties();

            var announcementManager = managersObj.AddComponent<AnnouncementManager>();
            var annSo = new SerializedObject(announcementManager);
            annSo.FindProperty("config").objectReferenceValue = config;
            var voiceSource = managersObj.AddComponent<AudioSource>();
            annSo.FindProperty("voiceSource").objectReferenceValue = voiceSource;
            annSo.ApplyModifiedProperties();

            var focusManager = managersObj.AddComponent<StationFocusManager>();
            var focusSo = new SerializedObject(focusManager);
            focusSo.FindProperty("config").objectReferenceValue = config;
            focusSo.FindProperty("inputReader").objectReferenceValue = inputReader;
            focusSo.ApplyModifiedProperties();

            var eventLogger = managersObj.AddComponent<GameplayEventLogger>();

            var gameManager = managersObj.AddComponent<GameManager>();
            var gmSo = new SerializedObject(gameManager);
            gmSo.FindProperty("config").objectReferenceValue = config;
            gmSo.FindProperty("focusManager").objectReferenceValue = focusManager;
            gmSo.FindProperty("eventLogger").objectReferenceValue = eventLogger;
            gmSo.FindProperty("relicStationPrefab").objectReferenceValue = relicPrefab;
            var spawnProp = gmSo.FindProperty("stationSpawnPoints");
            spawnProp.arraySize = spawnPoints.Length;
            for (int i = 0; i < spawnPoints.Length; i++)
                spawnProp.GetArrayElementAtIndex(i).objectReferenceValue = spawnPoints[i];

            var defsProp = gmSo.FindProperty("stationDefinitions");
            defsProp.arraySize = 5;
            string[] names = { "Hum Core", "Buzz Node", "Beacon", "Drip Stone", "Pulse Ring" };
            string[] descriptors = { "humming", "buzzing", "beeping", "dripping", "pulsing" };
            for (int i = 0; i < 5; i++)
            {
                var def = defsProp.GetArrayElementAtIndex(i);
                def.FindPropertyRelative("stationName").stringValue = names[i];
                def.FindPropertyRelative("soundDescriptor").stringValue = descriptors[i];
                def.FindPropertyRelative("audioClip").objectReferenceValue = stationClips[i];
            }
            gmSo.ApplyModifiedProperties();

            // Event system (Input System compatible).
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            var uiInput = eventSystem.AddComponent<InputSystemUIInputModule>();
            uiInput.actionsAsset = inputAsset;

            // UI Canvas.
            var canvasObj = new GameObject("UI Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            var uiManager = canvasObj.AddComponent<UIManager>();
            var uiSo = new SerializedObject(uiManager);
            uiSo.FindProperty("config").objectReferenceValue = config;
            uiSo.FindProperty("gameManager").objectReferenceValue = gameManager;
            uiSo.FindProperty("announcementManager").objectReferenceValue = announcementManager;
            uiSo.FindProperty("focusManager").objectReferenceValue = focusManager;
            uiSo.FindProperty("inputReader").objectReferenceValue = inputReader;

            var menuNavigator = canvasObj.AddComponent<AccessibleMenuNavigator>();
            var navSo = new SerializedObject(menuNavigator);
            navSo.FindProperty("gameManager").objectReferenceValue = gameManager;
            navSo.FindProperty("announcementManager").objectReferenceValue = announcementManager;
            navSo.ApplyModifiedProperties();

            uiSo.FindProperty("menuNavigator").objectReferenceValue = menuNavigator;

            // Accessibility intro splash (shown before the menu).
            var splashPanel = CreatePanel(canvasObj.transform, "AccessibilitySplashPanel");
            var splashTitle = CreateText(splashPanel.transform, "Title", "Signal Relic Recovery", 48, new Vector2(0, 80));
            var splashHint = CreateText(splashPanel.transform, "Hint", "Press Space to enable accessibility mode.", 28, new Vector2(0, -20));

            // Menu panel.
            var menuPanel = CreatePanel(canvasObj.transform, "MenuPanel");
            var menuTitle = CreateText(menuPanel.transform, "Title", "Signal Relic Recovery", 48, new Vector2(0, 150));
            var startBtn = CreateButton(menuPanel.transform, "StartButton", "Start Training", new Vector2(0, 90), startButtonClip, "Press Enter to begin.", true);
            var instrBtn = CreateButton(menuPanel.transform, "InstructionsButton", "Instructions", new Vector2(0, 20), instructionsButtonClip, "Press Enter to read how to play.", true);
            var listenInstrBtn = CreateButton(menuPanel.transform, "ListenInstructionsButton", "Listen to Instructions", new Vector2(0, -50), listenInstructionsButtonClip, "Press Enter to hear the instructions.", true);
            var accessibilityToggle = CreateToggle(menuPanel.transform, "AccessibilityToggle", "Accessibility Mode", new Vector2(0, -120), accessibilityToggleClip, "Press Enter to toggle.");
            var quitBtn = CreateButton(menuPanel.transform, "QuitButton", "Quit", new Vector2(0, -190), quitButtonClip, "Press Enter to quit.", true);

            SetupVerticalNavigation(startBtn, instrBtn, listenInstrBtn, accessibilityToggle, quitBtn);

            uiSo.FindProperty("accessibilitySplashPanel").objectReferenceValue = splashPanel;
            uiSo.FindProperty("accessibilityHintText").objectReferenceValue = splashHint;
            uiSo.FindProperty("menuPanel").objectReferenceValue = menuPanel;
            uiSo.FindProperty("startButton").objectReferenceValue = startBtn;
            uiSo.FindProperty("instructionsButton").objectReferenceValue = instrBtn;
            uiSo.FindProperty("listenInstructionsButton").objectReferenceValue = listenInstrBtn;
            uiSo.FindProperty("quitButton").objectReferenceValue = quitBtn;
            uiSo.FindProperty("accessibilityToggle").objectReferenceValue = accessibilityToggle;

            // HUD panel.
            var hudPanel = CreatePanel(canvasObj.transform, "HUDPanel");
            var roundText = CreateAnchoredText(hudPanel.transform, "RoundText", "Round 1 / 3", 28,
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(300, 60));
            var targetText = CreateAnchoredText(hudPanel.transform, "TargetText", "Target: humming", 28,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(400, 60));
            var timerText = CreateAnchoredText(hudPanel.transform, "TimerText", "Time: 0.0s", 28,
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -20), new Vector2(300, 60));
            var focusText = CreateAnchoredText(hudPanel.transform, "FocusText", "Focused: none", 24,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 20), new Vector2(600, 60));
            var announcementText = CreateAnchoredText(hudPanel.transform, "AnnouncementText", "", 32,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 120), new Vector2(900, 80));

            // Log toggle button on HUD (top-right, below timer).
            var logToggleBtn = CreateAnchoredButton(hudPanel.transform, "LogButton", "Log",
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -90), new Vector2(120, 50));

            uiSo.FindProperty("hudPanel").objectReferenceValue = hudPanel;
            uiSo.FindProperty("roundText").objectReferenceValue = roundText;
            uiSo.FindProperty("targetText").objectReferenceValue = targetText;
            uiSo.FindProperty("timerText").objectReferenceValue = timerText;
            uiSo.FindProperty("focusText").objectReferenceValue = focusText;
            uiSo.FindProperty("announcementText").objectReferenceValue = announcementText;

            // Event log panel.
            var logPanel = CreatePanel(canvasObj.transform, "EventLogPanel");
            var logPanelRt = logPanel.GetComponent<RectTransform>();
            logPanelRt.anchorMin = new Vector2(0.5f, 0.5f);
            logPanelRt.anchorMax = new Vector2(0.5f, 0.5f);
            logPanelRt.anchoredPosition = new Vector2(0, 0);
            logPanelRt.sizeDelta = new Vector2(700, 500);
            logPanel.SetActive(false);

            var scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(logPanel.transform, false);
            var scrollRect = scrollObj.AddComponent<ScrollRect>();
            var scrollRt = scrollObj.GetComponent<RectTransform>();
            scrollRt.anchorMin = Vector2.zero;
            scrollRt.anchorMax = Vector2.one;
            scrollRt.offsetMin = new Vector2(20, 20);
            scrollRt.offsetMax = new Vector2(-20, -50);

            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollObj.transform, false);
            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(0, 0, 0, 0.3f);
            var viewportRt = viewport.GetComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;
            scrollRect.viewport = viewportRt;

            var logTextObj = new GameObject("LogText");
            logTextObj.transform.SetParent(viewport.transform, false);
            var logText = logTextObj.AddComponent<Text>();
            logText.text = "";
            logText.fontSize = 18;
            logText.alignment = TextAnchor.UpperLeft;
            logText.color = Color.white;
            var logTextRt = logTextObj.GetComponent<RectTransform>();
            logTextRt.anchorMin = new Vector2(0, 1);
            logTextRt.anchorMax = new Vector2(1, 1);
            logTextRt.pivot = new Vector2(0.5f, 1f);
            logTextRt.anchoredPosition = Vector2.zero;
            logTextRt.sizeDelta = new Vector2(0, 0);
            scrollRect.content = logTextRt;

            var logTitle = CreateAnchoredText(logPanel.transform, "LogTitle", "Event Log", 28,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(300, 50));

            // Close button inside the log panel.
            var logCloseBtn = CreateAnchoredButton(logPanel.transform, "CloseButton", "Close",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 15), new Vector2(160, 45));

            // EventLogUI component.
            var eventLogUI = canvasObj.AddComponent<EventLogUI>();
            var eventLogUISo = new SerializedObject(eventLogUI);
            eventLogUISo.FindProperty("eventLogger").objectReferenceValue = eventLogger;
            eventLogUISo.FindProperty("logPanel").objectReferenceValue = logPanel;
            eventLogUISo.FindProperty("logText").objectReferenceValue = logText;
            eventLogUISo.FindProperty("scrollRect").objectReferenceValue = scrollRect;
            eventLogUISo.FindProperty("toggleButton").objectReferenceValue = logToggleBtn;
            eventLogUISo.FindProperty("closeButton").objectReferenceValue = logCloseBtn;
            eventLogUISo.ApplyModifiedProperties();

            // Results panel.
            var resultsPanel = CreatePanel(canvasObj.transform, "ResultsPanel");
            var resultsTitle = CreateText(resultsPanel.transform, "Title", "Training Complete", 42, new Vector2(0, 150));
            var resultsText = CreateText(resultsPanel.transform, "ResultsText", "Results...", 28, new Vector2(0, 0));
            var restartBtn = CreateButton(resultsPanel.transform, "RestartButton", "Restart", new Vector2(0, -120), null, "Press Enter to play again.", true);
            var resultsMenuBtn = CreateButton(resultsPanel.transform, "MenuButton", "Main Menu", new Vector2(0, -190), null, "Press Enter to return to the main menu.", true);

            SetupVerticalNavigation(restartBtn, resultsMenuBtn);

            uiSo.FindProperty("resultsPanel").objectReferenceValue = resultsPanel;
            uiSo.FindProperty("resultsText").objectReferenceValue = resultsText;
            uiSo.FindProperty("restartButton").objectReferenceValue = restartBtn;
            uiSo.FindProperty("menuButton").objectReferenceValue = resultsMenuBtn;

            // Instructions panel.
            var instrPanel = CreatePanel(canvasObj.transform, "InstructionsPanel");
            var instrTitle = CreateText(instrPanel.transform, "Title", "How to Play", 42, new Vector2(0, 180));
            var instrBody = CreateText(instrPanel.transform, "Body",
                "You are training to recover signal relics.\n\n" +
                "Each round, you will be told what kind of relic to find.\n" +
                "Use Previous / Next to move focus between stations.\n" +
                "Press Select to choose the focused station.\n\n" +
                "Controls:\n" +
                "Arrow Keys / 1 & 2 / Gamepad D-Pad = Change focus\n" +
                "Space / Enter / E / Gamepad South = Select\n" +
                "Mouse / Touch = Click a station directly\n\n" +
                "Accessibility Mode adds longer announcements and larger text.",
                22, new Vector2(0, -20));
            var closeInstrBtn = CreateButton(instrPanel.transform, "CloseButton", "Back", new Vector2(0, -180), null, "Press Enter to go back.", true);

            uiSo.FindProperty("instructionsPanel").objectReferenceValue = instrPanel;
            uiSo.FindProperty("closeInstructionsButton").objectReferenceValue = closeInstrBtn;

            uiSo.ApplyModifiedProperties();

            // Persist voice-clip references in the shared config asset.
            if (config != null)
            {
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            }

            // Save scene.
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainScene.unity");
            Debug.Log("Signal Relic Recovery: MainScene created.");
        }

        private static GameObject CreatePanel(Transform parent, string name)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var image = panel.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.85f);
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return panel;
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize, Vector2 anchoredPosition)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var text = obj.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = new Vector2(800, 80);
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition,
            AudioClip announcementClip = null, string hint = null, bool accessible = false)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var image = obj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.5f, 0.8f);
            var button = obj.AddComponent<Button>();
            var buttonSo = new SerializedObject(button);
            buttonSo.FindProperty("m_TargetGraphic").objectReferenceValue = image;
            buttonSo.ApplyModifiedProperties();
            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = new Vector2(240, 50);

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            var text = textObj.AddComponent<Text>();
            text.text = label;
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            var textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            if (accessible)
            {
                var item = obj.AddComponent<AccessibleMenuItem>();
                var itemSo = new SerializedObject(item);
                itemSo.FindProperty("label").stringValue = label;
                itemSo.FindProperty("hint").stringValue = hint ?? "Press Enter to select.";
                itemSo.FindProperty("announcementClip").objectReferenceValue = announcementClip;
                itemSo.ApplyModifiedProperties();
            }

            return button;
        }

        private static Text CreateAnchoredText(Transform parent, string name, string content, int fontSize,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var text = obj.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;
            return text;
        }

        private static Button CreateAnchoredButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta,
            AudioClip announcementClip = null, string hint = null, bool accessible = false)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var image = obj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.5f, 0.8f);
            var button = obj.AddComponent<Button>();
            var buttonSo = new SerializedObject(button);
            buttonSo.FindProperty("m_TargetGraphic").objectReferenceValue = image;
            buttonSo.ApplyModifiedProperties();
            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            var text = textObj.AddComponent<Text>();
            text.text = label;
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            var textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            if (accessible)
            {
                var item = obj.AddComponent<AccessibleMenuItem>();
                var itemSo = new SerializedObject(item);
                itemSo.FindProperty("label").stringValue = label;
                itemSo.FindProperty("hint").stringValue = hint ?? "Press Enter to select.";
                itemSo.FindProperty("announcementClip").objectReferenceValue = announcementClip;
                itemSo.ApplyModifiedProperties();
            }

            return button;
        }

        private static Toggle CreateToggle(Transform parent, string name, string label, Vector2 anchoredPosition,
            AudioClip announcementClip = null, string hint = null)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var toggle = obj.AddComponent<Toggle>();
            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = new Vector2(240, 40);

            var bg = new GameObject("Background");
            bg.transform.SetParent(obj.transform, false);
            var bgImage = bg.AddComponent<Image>();
            bgImage.color = Color.white;
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0, 0.5f);
            bgRt.anchorMax = new Vector2(0, 0.5f);
            bgRt.anchoredPosition = new Vector2(-100, 0);
            bgRt.sizeDelta = new Vector2(30, 30);
            toggle.targetGraphic = bgImage;

            var check = new GameObject("Checkmark");
            check.transform.SetParent(bg.transform, false);
            var checkImage = check.AddComponent<Image>();
            checkImage.color = new Color(0.2f, 0.5f, 0.8f);
            var checkRt = check.GetComponent<RectTransform>();
            checkRt.anchorMin = Vector2.zero;
            checkRt.anchorMax = Vector2.one;
            checkRt.offsetMin = new Vector2(4, 4);
            checkRt.offsetMax = new Vector2(-4, -4);

            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(obj.transform, false);
            var text = labelObj.AddComponent<Text>();
            text.text = label;
            text.fontSize = 22;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = Color.white;
            var labelRt = labelObj.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0, 0.5f);
            labelRt.anchorMax = new Vector2(0, 0.5f);
            labelRt.anchoredPosition = new Vector2(20, 0);
            labelRt.sizeDelta = new Vector2(200, 40);

            var item = obj.AddComponent<AccessibleMenuItem>();
            var itemSo = new SerializedObject(item);
            itemSo.FindProperty("label").stringValue = label;
            itemSo.FindProperty("hint").stringValue = hint ?? "Press Enter to toggle.";
            itemSo.FindProperty("announcementClip").objectReferenceValue = announcementClip;
            itemSo.ApplyModifiedProperties();

            return toggle;
        }

        private static void SetupVerticalNavigation(params Selectable[] selectables)
        {
            for (int i = 0; i < selectables.Length; i++)
            {
                if (selectables[i] == null) continue;
                var nav = selectables[i].navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = selectables[i > 0 ? i - 1 : selectables.Length - 1];
                nav.selectOnDown = selectables[i < selectables.Length - 1 ? i + 1 : 0];
                selectables[i].navigation = nav;
            }
        }
    }
}
