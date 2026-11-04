using System.IO;
using FishHunter.Animation;
using FishHunter.Config;
using FishHunter.Core;
using FishHunter.Enemies;
using FishHunter.Gameplay;
using FishHunter.UI.Core;
using FishHunter.UI.Screens;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FishHunter.EditorTools
{
    /// <summary>
    /// Builds the whole game from generated art: config assets, prefabs, the Game scene and its UI.
    /// Menu: Fish Hunter/Build Scene (run Generate Art first).
    /// </summary>
    public static class FishHunterSceneBuilder
    {
        private const string Base = "Assets/_FishHunter";
        private const string DataDir = Base + "/Data";
        private const string PrefabDir = Base + "/Prefabs";
        private const string UiPrefabDir = PrefabDir + "/UI";
        private const string ScenePath = Base + "/Scenes/Game.unity";

        private const float PixelsPerUnit = 16f;
        private const float WorldPixelScale = 4f; // each art pixel = 4 screen pixels at 1080p
        private const int UiTextScale = 4;

        private static readonly Color _textColor = new Color32(255, 248, 228, 255);
        private static readonly Color _buttonTextColor = new Color32(255, 250, 236, 255);
        private static readonly Color _buttonTextShadow = new Color32(110, 50, 10, 255);

        private static Material _spriteMaterial;
        private static PixelFont _font;

        [MenuItem("Fish Hunter/Build Scene", priority = 1)]
        public static void Build()
        {
            foreach (string dir in new[] { DataDir, PrefabDir, UiPrefabDir, Base + "/Scenes" }) Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();

            _spriteMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat")
                              ?? AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            _font = AssetDatabase.LoadAssetAtPath<PixelFont>(PixelArtGenerator.FontAssetPath);
            if (_font == null) throw new FileNotFoundException("PixelFont missing. Run Fish Hunter/Generate Art first.");

            GameConfig config = CreateGameConfig();
            EnemyTypeConfig[] types = CreateEnemyTypes();

            CreateProjectilePrefab();
            CreateEnemyPrefab();
            CreateEffectPrefab();
            AssetDatabase.SaveAssets();

            // Opening a new scene unloads unused assets, which invalidates in-memory references.
            // Create the scene first, then (re)load every asset by path.
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            _spriteMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat")
                              ?? AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            _font = AssetDatabase.LoadAssetAtPath<PixelFont>(PixelArtGenerator.FontAssetPath);
            config = AssetDatabase.LoadAssetAtPath<GameConfig>(DataDir + "/GameConfig.asset");
            types = new[]
            {
                AssetDatabase.LoadAssetAtPath<EnemyTypeConfig>(DataDir + "/EnemyType_A_Standard.asset"),
                AssetDatabase.LoadAssetAtPath<EnemyTypeConfig>(DataDir + "/EnemyType_B_Fast.asset"),
                AssetDatabase.LoadAssetAtPath<EnemyTypeConfig>(DataDir + "/EnemyType_C_Swift.asset")
            };
            Projectile projectilePrefab = AssetDatabase.LoadAssetAtPath<Projectile>(PrefabDir + "/Projectile.prefab");
            Enemy enemyPrefab = AssetDatabase.LoadAssetAtPath<Enemy>(PrefabDir + "/Enemy.prefab");
            OneShotEffect effectPrefab = AssetDatabase.LoadAssetAtPath<OneShotEffect>(PrefabDir + "/OneShotEffect.prefab");

            BuildScene(scene, config, types, projectilePrefab, enemyPrefab, effectPrefab);
            AssetDatabase.SaveAssets();
            Debug.Log("[FishHunter] Scene built at " + ScenePath);
        }

        // ---------------- Data ----------------

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static GameConfig CreateGameConfig()
        {
            GameConfig config = LoadOrCreate<GameConfig>(DataDir + "/GameConfig.asset");
            config.PlayerIdleFrames = PixelArtGenerator.LoadFrames("Player", "Player_Idle", 4);
            config.PlayerShootFrames = PixelArtGenerator.LoadFrames("Player", "Player_Shoot", 4);
            config.MuzzleFrames = PixelArtGenerator.LoadFrames("Effects", "Muzzle", 4);
            config.NoseOffset = 0.95f;
            EditorUtility.SetDirty(config);
            return config;
        }

        private static EnemyTypeConfig[] CreateEnemyTypes()
        {
            EnemyTypeConfig a = LoadOrCreate<EnemyTypeConfig>(DataDir + "/EnemyType_A_Standard.asset");
            Fill(a, "Type A (standard)", EnemyMovementKind.Standard, "EnemyA", 1.5f, 2.0f, 10, 80f, 35f, 0.45f, 7f);

            EnemyTypeConfig b = LoadOrCreate<EnemyTypeConfig>(DataDir + "/EnemyType_B_Fast.asset");
            Fill(b, "Type B (fast)", EnemyMovementKind.Fast, "EnemyB", 2.5f, 3.2f, 25, 18f, 40f, 0.38f, 10f);

            EnemyTypeConfig c = LoadOrCreate<EnemyTypeConfig>(DataDir + "/EnemyType_C_Swift.asset");
            Fill(c, "Type C (swift)", EnemyMovementKind.Swift, "EnemyC", 3.9f, 4.8f, 50, 2f, 25f, 0.33f, 14f);

            return new[] { a, b, c };
        }

        private static void Fill(EnemyTypeConfig type, string displayName, EnemyMovementKind movement, string art,
            float speedMin, float speedMax, int points, float weightStart, float weightEnd, float radius, float swimFps)
        {
            type.DisplayName = displayName;
            type.Movement = movement;
            type.SpeedMin = speedMin;
            type.SpeedMax = speedMax;
            type.Points = points;
            type.SpawnWeightStart = weightStart;
            type.SpawnWeightEnd = weightEnd;
            type.ColliderRadius = radius;
            type.SwimFrames = PixelArtGenerator.LoadFrames("Enemies", art + "_Swim", 4);
            type.SwimFps = swimFps;
            type.DeathFrames = PixelArtGenerator.LoadFrames("Effects", art + "_Death", 6);
            type.DeathFps = 14f;
            EditorUtility.SetDirty(type);
        }

        // ---------------- Prefabs ----------------

        private static SpriteRenderer AddSprite(GameObject go, Sprite sprite, int order)
        {
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            if (_spriteMaterial != null) sr.sharedMaterial = _spriteMaterial;
            return sr;
        }

        private static SpriteFrameAnimator AddAnimator(GameObject go, SpriteRenderer target, Sprite[] frames, float fps, bool loop, bool playOnEnable, bool randomStart = false)
        {
            SpriteFrameAnimator animator = go.AddComponent<SpriteFrameAnimator>();
            Wire(animator, ("_target", target));
            WireArray(animator, "_frames", frames);
            SetValue(animator, "_framesPerSecond", fps);
            SetValue(animator, "_loop", loop);
            SetValue(animator, "_playOnEnable", playOnEnable);
            SetValue(animator, "_randomStartFrame", randomStart);
            return animator;
        }

        private static T SavePrefab<T>(GameObject go, string path) where T : Component
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<T>();
        }

        private static Projectile CreateProjectilePrefab()
        {
            Sprite[] frames = PixelArtGenerator.LoadFrames("Effects", "Projectile", 3);
            var go = new GameObject("Projectile");
            SpriteRenderer sr = AddSprite(go, frames[0], 5);
            AddAnimator(go, sr, frames, 12f, true, true, true);

            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.22f;

            TrailRenderer trail = go.AddComponent<TrailRenderer>();
            trail.time = 0.14f;
            trail.minVertexDistance = 0.05f;
            trail.widthCurve = new AnimationCurve(new Keyframe(0f, 0.34f), new Keyframe(1f, 0f));
            trail.numCapVertices = 0;
            trail.numCornerVertices = 0;
            trail.sortingOrder = 4;
            trail.sharedMaterial = _spriteMaterial;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(new Color32(210, 248, 255, 255), 0f), new GradientColorKey(new Color32(80, 190, 240, 255), 0.5f), new GradientColorKey(new Color32(40, 120, 200, 255), 1f) },
                new[] { new GradientAlphaKey(0.85f, 0f), new GradientAlphaKey(0.45f, 0.5f), new GradientAlphaKey(0f, 1f) });
            gradient.mode = GradientMode.Fixed; // stepped colors read as pixel bands
            trail.colorGradient = gradient;

            Projectile projectile = go.AddComponent<Projectile>();
            Wire(projectile, ("_body", body), ("_trail", trail));
            return SavePrefab<Projectile>(go, PrefabDir + "/Projectile.prefab");
        }

        private static Enemy CreateEnemyPrefab()
        {
            var go = new GameObject("Enemy");
            SpriteRenderer sr = AddSprite(go, PixelArtGenerator.LoadSprite("Enemies", "EnemyA_Swim_0"), 0);
            SpriteFrameAnimator animator = AddAnimator(go, sr, null, 8f, true, false);

            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;

            Enemy enemy = go.AddComponent<Enemy>();
            Wire(enemy, ("_spriteRenderer", sr), ("_animator", animator), ("_bodyCollider", col));
            return SavePrefab<Enemy>(go, PrefabDir + "/Enemy.prefab");
        }

        private static OneShotEffect CreateEffectPrefab()
        {
            var go = new GameObject("OneShotEffect");
            SpriteRenderer sr = AddSprite(go, null, 20);
            SpriteFrameAnimator animator = AddAnimator(go, sr, null, 14f, false, false);
            OneShotEffect effect = go.AddComponent<OneShotEffect>();
            Wire(effect, ("_animator", animator), ("_spriteRenderer", sr));
            return SavePrefab<OneShotEffect>(go, PrefabDir + "/OneShotEffect.prefab");
        }

        // ---------------- Scene ----------------

        private static void BuildScene(Scene scene, GameConfig config, EnemyTypeConfig[] types, Projectile projectilePrefab, Enemy enemyPrefab, OneShotEffect effectPrefab)
        {
            // Camera
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 1080f / 2f / (PixelsPerUnit * WorldPixelScale);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color32(10, 40, 80, 255);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 50f;
            camGo.AddComponent<AudioListener>();

            // Background + animated caustics
            var bg = new GameObject("Background");
            AddSprite(bg, PixelArtGenerator.LoadSprite("Background", "Seabed"), -100);
            var caustics = new GameObject("Caustics");
            caustics.transform.SetParent(bg.transform, false);
            Sprite[] causticsFrames = PixelArtGenerator.LoadFrames("Background", "Caustics", BackgroundPainter.CausticFrameCount);
            SpriteRenderer causticsSr = AddSprite(caustics, causticsFrames[0], -90);
            AddAnimator(caustics, causticsSr, causticsFrames, BackgroundPainter.CausticFps, true, true);

            // Player
            var playerGo = new GameObject("Player");
            Rigidbody2D playerBody = playerGo.AddComponent<Rigidbody2D>();
            playerBody.bodyType = RigidbodyType2D.Dynamic;
            playerBody.gravityScale = 0f;
            playerBody.constraints = RigidbodyConstraints2D.FreezeAll;
            CircleCollider2D playerCol = playerGo.AddComponent<CircleCollider2D>();
            playerCol.isTrigger = true;
            playerCol.radius = 0.6f;
            var visual = new GameObject("Visual");
            visual.transform.SetParent(playerGo.transform, false);
            SpriteRenderer playerSr = AddSprite(visual, config.PlayerIdleFrames[0], 10);
            SpriteFrameAnimator playerAnim = AddAnimator(visual, playerSr, config.PlayerIdleFrames, config.PlayerIdleFps, true, false);
            PlayerFish player = playerGo.AddComponent<PlayerFish>();
            Wire(player, ("_visual", visual.transform), ("_spriteRenderer", playerSr), ("_animator", playerAnim));
            PlayerAim aim = playerGo.AddComponent<PlayerAim>();
            Wire(aim, ("_player", player));
            PlayerShooter shooter = playerGo.AddComponent<PlayerShooter>();
            Wire(shooter, ("_player", player), ("_aim", aim), ("_projectilePrefab", projectilePrefab), ("_muzzleEffectPrefab", effectPrefab));

            // Spawner + pools
            var spawnerGo = new GameObject("EnemySpawner");
            EnemySpawner spawner = spawnerGo.AddComponent<EnemySpawner>();
            Wire(spawner, ("_enemyPrefab", enemyPrefab), ("_deathEffectPrefab", effectPrefab));
            WireArray(spawner, "_enemyTypes", types);
            var pools = new GameObject("Pools");

            // UI
            UIScreenService screens = BuildUi();

            // Bootstrap
            var bootstrapGo = new GameObject("GameBootstrap");
            GameBootstrap bootstrap = bootstrapGo.AddComponent<GameBootstrap>();
            Wire(bootstrap, ("_gameConfig", config), ("_gameCamera", cam), ("_poolRoot", pools.transform),
                ("_player", player), ("_spawner", spawner), ("_screens", screens));

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        // ---------------- UI ----------------

        private static UIScreenService BuildUi()
        {
            var canvasGo = new GameObject("UI", typeof(RectTransform))
            {
                layer = LayerMask.NameToLayer("UI")
            };
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;
            canvasGo.AddComponent<GraphicRaycaster>();
            UIScreenService screens = canvasGo.AddComponent<UIScreenService>();

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();

            Sprite heartFull = PixelArtGenerator.LoadSprite("UI", "Heart_Full");
            Sprite heartEmpty = PixelArtGenerator.LoadSprite("UI", "Heart_Empty");

            // HUD (top-left)
            HudView hud = CreateScreen<HudView>(canvasGo.transform, "HudView");
            PixelText score = Text(hud.transform, "ScoreText", "SCORE: 0", 5, PixelText.Align.Left, _textColor);
            Place(score.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -36), new Vector2(800, 40));
            var hearts = new Image[3];
            for (int i = 0; i < hearts.Length; i++)
            {
                hearts[i] = Icon(hud.transform, $"Heart_{i}", heartFull);
                Place(hearts[i].rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(40 + i * 66, -94), new Vector2(55, 50));
            }
            Wire(hud, ("_scoreText", score), ("_heartFull", heartFull), ("_heartEmpty", heartEmpty));
            WireArray(hud, "_hearts", hearts);

            // Main menu
            MainMenuView menu = CreateScreen<MainMenuView>(canvasGo.transform, "MainMenuView");
            Image logo = Icon(menu.transform, "Logo", PixelArtGenerator.LoadSprite("UI", "Logo"));
            logo.SetNativeSize();
            Place(logo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 250), logo.rectTransform.sizeDelta);
            Button start = MakeButton(menu.transform, "StartButton", "START GAME", new Vector2(0, -110));
            Button exit = MakeButton(menu.transform, "ExitButton", "EXIT", new Vector2(0, -250));
            PixelText best = Text(menu.transform, "BestText", "", UiTextScale, PixelText.Align.Center, new Color32(255, 220, 120, 255));
            Place(best.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -370), new Vector2(800, 40));
            PixelText hint = Text(menu.transform, "HintText", "AIM WITH MOUSE - RIGHT CLICK TO SHOOT - ESC TO PAUSE", 3, PixelText.Align.Center, new Color32(190, 230, 245, 255));
            Place(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 50), new Vector2(1800, 30));
            Wire(menu, ("_startButton", start), ("_exitButton", exit), ("_bestScoreText", best));

            // Pause
            PauseView pause = CreateScreen<PauseView>(canvasGo.transform, "PauseView");
            Dim(pause.transform);
            RectTransform pausePanel = Panel(pause.transform, new Vector2(760, 560));
            PixelText pauseTitle = Text(pausePanel, "Title", "PAUSED", 9, PixelText.Align.Center, _textColor);
            Place(pauseTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 160), new Vector2(700, 70));
            Button resume = MakeButton(pausePanel, "ResumeButton", "RESUME", new Vector2(0, 10));
            Button pauseMenu = MakeButton(pausePanel, "MenuButton", "RETURN TO MENU", new Vector2(0, -135));
            Wire(pause, ("_resumeButton", resume), ("_menuButton", pauseMenu));

            // Game over
            GameOverView over = CreateScreen<GameOverView>(canvasGo.transform, "GameOverView");
            Dim(over.transform);
            RectTransform overPanel = Panel(over.transform, new Vector2(840, 700));
            PixelText overTitle = Text(overPanel, "Title", "GAME OVER", 9, PixelText.Align.Center, new Color32(255, 120, 110, 255));
            Place(overTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 230), new Vector2(800, 70));
            PixelText finalScore = Text(overPanel, "FinalScoreText", "SCORE: 0", 6, PixelText.Align.Center, _textColor);
            Place(finalScore.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 110), new Vector2(800, 50));
            PixelText bestOver = Text(overPanel, "BestText", "BEST: 0", UiTextScale, PixelText.Align.Center, new Color32(255, 220, 120, 255));
            Place(bestOver.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(800, 40));
            Button again = MakeButton(overPanel, "PlayAgainButton", "PLAY AGAIN", new Vector2(0, -85));
            Button overMenu = MakeButton(overPanel, "MenuButton", "RETURN TO MENU", new Vector2(0, -230));
            Wire(over, ("_finalScoreText", finalScore), ("_bestScoreText", bestOver), ("_playAgainButton", again), ("_menuButton", overMenu));

            Wire(screens, ("_mainMenuView", menu), ("_hudView", hud), ("_pauseView", pause), ("_gameOverView", over));

            // Save each screen as a prefab (connected instances stay in the scene).
            foreach (ViewBase view in new ViewBase[] { hud, menu, pause, over })
                PrefabUtility.SaveAsPrefabAssetAndConnect(view.gameObject, $"{UiPrefabDir}/{view.name}.prefab", InteractionMode.AutomatedAction);

            return screens;
        }

        private static RectTransform UiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform))
            {
                layer = LayerMask.NameToLayer("UI")
            };
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
        }

        private static T CreateScreen<T>(Transform parent, string name) where T : ViewBase
        {
            RectTransform rt = UiObject(name, parent);
            Stretch(rt);
            CanvasGroup group = rt.gameObject.AddComponent<CanvasGroup>();
            T view = rt.gameObject.AddComponent<T>();
            Wire(view, ("_canvasGroup", group));
            return view;
        }

        private static PixelText Text(Transform parent, string name, string text, int scale, PixelText.Align align, Color color)
        {
            RectTransform rt = UiObject(name, parent);
            PixelText label = rt.gameObject.AddComponent<PixelText>();
            label.Font = _font;
            label.Text = text;
            label.PixelScale = scale;
            label.Alignment = align;
            label.color = color;
            label.raycastTarget = false;
            return label;
        }

        private static Image Icon(Transform parent, string name, Sprite sprite)
        {
            RectTransform rt = UiObject(name, parent);
            Image image = rt.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        private static void Dim(Transform parent)
        {
            RectTransform rt = UiObject("Dim", parent);
            Stretch(rt);
            Image image = rt.gameObject.AddComponent<Image>();
            image.color = new Color(0.02f, 0.06f, 0.14f, 0.6f);
        }

        private static RectTransform Panel(Transform parent, Vector2 size)
        {
            RectTransform rt = UiObject("Panel", parent);
            Place(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
            Image image = rt.gameObject.AddComponent<Image>();
            image.sprite = PixelArtGenerator.LoadSprite("UI", "Panel");
            image.type = Image.Type.Sliced;
            return rt;
        }

        private static Button MakeButton(Transform parent, string name, string label, Vector2 position)
        {
            RectTransform rt = UiObject(name, parent);
            Place(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, new Vector2(560, 110));
            Image image = rt.gameObject.AddComponent<Image>();
            image.sprite = PixelArtGenerator.LoadSprite("UI", "Button_Normal");
            image.type = Image.Type.Sliced;

            Button button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState
            {
                highlightedSprite = PixelArtGenerator.LoadSprite("UI", "Button_Hover"),
                pressedSprite = PixelArtGenerator.LoadSprite("UI", "Button_Pressed"),
                selectedSprite = PixelArtGenerator.LoadSprite("UI", "Button_Normal"),
                disabledSprite = PixelArtGenerator.LoadSprite("UI", "Button_Pressed")
            };
            button.navigation = new Navigation { mode = Navigation.Mode.None };

            PixelText text = Text(rt, "Label", label, 5, PixelText.Align.Center, _buttonTextColor);
            Stretch(text.rectTransform);
            text.rectTransform.offsetMin = new Vector2(0, 4); // sit slightly above the bottom shade
            var so = new SerializedObject(text);
            so.FindProperty("_shadowColor").colorValue = _buttonTextShadow;
            so.ApplyModifiedPropertiesWithoutUndo();
            return button;
        }

        // ---------------- Serialized wiring helpers ----------------

        private static void Wire(Object target, params (string name, Object value)[] refs)
        {
            var so = new SerializedObject(target);
            foreach ((string name, Object value) in refs)
            {
                SerializedProperty prop = so.FindProperty(name) ?? throw new MissingReferenceException($"{target.GetType().Name}.{name} not found");
                prop.objectReferenceValue = value;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireArray(Object target, string name, Object[] values)
        {
            var so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(name) ?? throw new MissingReferenceException($"{target.GetType().Name}.{name} not found");
            values ??= new Object[0];
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetValue(Object target, string name, float value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(name).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetValue(Object target, string name, bool value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(name).boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
