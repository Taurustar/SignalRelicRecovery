using UnityEditor;
using UnityEngine;
using TMPro;
using SignalRelicRecovery;

namespace SignalRelicRecovery.Editor
{
    public static class SetupPrototype
    {
        [MenuItem("Signal Relic Recovery/Setup Prototype Assets")]
        public static void Run()
        {
            CreateGameConfig();
            CreateMaterials();
            CreateRelicStationPrefab();
            AssetDatabase.SaveAssets();
            Debug.Log("Signal Relic Recovery: prototype assets created.");
        }

        private static void CreateGameConfig()
        {
            string path = "Assets/ScriptableObjects/GameConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<GameConfig>(path) != null) return;

            var config = ScriptableObject.CreateInstance<GameConfig>();
            config.relicsPerRound = new int[] { 3, 4, 5 };
            config.roundTransitionDelay = 1.5f;
            config.roundStartDelay = 1.0f;
            config.focusMoveCooldown = 0.25f;
            config.selectionCooldown = 1.0f;
            config.focusedStationVolume = 1f;
            config.unfocusedStationVolume = 0.35f;
            config.sfxVolume = 0.8f;
            config.announcementVolume = 1f;
            config.accessibilityModeDefault = false;
            config.accessibilityAnnouncementDuration = 4f;
            config.standardAnnouncementDuration = 2.5f;
            config.accessibilityTextScale = 1.3f;
            config.allowRetryAfterWrongSelection = true;

            AssetDatabase.CreateAsset(config, path);
        }

        private static void CreateMaterials()
        {
            CreateMaterial("Assets/Materials/Station_Hum.mat", new Color(0.9f, 0.3f, 0.3f), "Grid");
            CreateMaterial("Assets/Materials/Station_Buzz.mat", new Color(0.3f, 0.9f, 0.3f), "Stripes");
            CreateMaterial("Assets/Materials/Station_Beep.mat", new Color(0.3f, 0.3f, 0.9f), "Dots");
            CreateMaterial("Assets/Materials/Station_Drip.mat", new Color(0.9f, 0.9f, 0.3f), "Checkers");
            CreateMaterial("Assets/Materials/Station_Pulse.mat", new Color(0.9f, 0.3f, 0.9f), "Diagonal");
        }

        private static void CreateMaterial(string path, Color color, string patternName)
        {
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var mat = new Material(shader);
            mat.SetColor("_BaseColor", color);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.2f);

            // Create a simple procedural texture so each station is visually distinct
            // even if the player cannot perceive the hue.
            Texture2D tex = new Texture2D(128, 128);
            Color[] pixels = new Color[128 * 128];
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    float v = 0f;
                    switch (patternName)
                    {
                        case "Grid":
                            v = ((x / 16) % 2 == (y / 16) % 2) ? 1f : 0f;
                            break;
                        case "Stripes":
                            v = (x / 16) % 2 == 0 ? 1f : 0f;
                            break;
                        case "Dots":
                            v = (((x - 64) * (x - 64) + (y - 64) * (y - 64)) < 900) ? 1f : 0f;
                            break;
                        case "Checkers":
                            v = ((x / 32) % 2 == (y / 32) % 2) ? 1f : 0f;
                            break;
                        case "Diagonal":
                            v = ((x + y) / 16) % 2 == 0 ? 1f : 0f;
                            break;
                    }
                    pixels[y * 128 + x] = Color.Lerp(color * 0.5f, color, v);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            string texPath = path.Replace(".mat", "_Pattern.png");
            System.IO.File.WriteAllBytes(texPath, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(texPath);
            var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.SaveAndReimport();
            }

            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texPath));
            AssetDatabase.CreateAsset(mat, path);
        }

        private static void CreateRelicStationPrefab()
        {
            string path = "Assets/Prefabs/RelicStation.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

            var root = new GameObject("RelicStation");
            root.layer = LayerMask.NameToLayer("Default");

            var station = root.AddComponent<RelicStation>();

            // Visual root - a cube by default.
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one;
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            // Audio source.
            var audioSource = root.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 2f;
            audioSource.maxDistance = 20f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;

            // Collider for clicking.
            var collider = root.AddComponent<BoxCollider>();
            collider.size = Vector3.one;
            var clickHandler = root.AddComponent<StationClickHandler>();

            // Floating label.
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(root.transform);
            labelObj.transform.localPosition = new Vector3(0, 1.2f, 0);
            var tmp = labelObj.AddComponent<TextMeshPro>();
            tmp.text = "Station";
            tmp.fontSize = 3;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            // Link via serialized fields using SerializedObject.
            var so = new SerializedObject(station);
            so.FindProperty("audioSource").objectReferenceValue = audioSource;
            so.FindProperty("visualRoot").objectReferenceValue = visual.transform;
            so.FindProperty("highlightRenderer").objectReferenceValue = visual.GetComponent<Renderer>();
            so.FindProperty("labelText").objectReferenceValue = tmp;
            so.ApplyModifiedProperties();

            var clickSo = new SerializedObject(clickHandler);
            clickSo.FindProperty("relicStation").objectReferenceValue = station;
            clickSo.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }
    }
}
