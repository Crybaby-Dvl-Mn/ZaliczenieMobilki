using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Narzedzie do przygotowania sceny bijatykowej:
/// Menu: Tools -> Battle Scene Setup
/// </summary>
public static class BattleSceneSetup
{
    private const string AttackDataFolder = "Assets/ScriptableObjects/Attacks";

    [MenuItem("Tools/Battle Scene Setup/Wszystko (Kompletny Setup)")]
    public static void SetupAll()
    {
        bool ok = EditorUtility.DisplayDialog(
            "Battle Scene Setup",
            "Skrypt wykona:\n" +
            "- Tworzy AttackData assety dla P1 i P2\n" +
            "- Podpina ataki do prefabow P1 i P2\n" +
            "- Naprawia GroundCheck i warstwy w prefabach\n" +
            "- Dodaje DeathZone (strefa smierci) do sceny\n" +
            "- Dodaje DynamicCamera do kamery glownej\n" +
            "- Tworzy podloge jesli brakuje\n" +
            "- Tworzy BattleManager jesli brakuje\n\n" +
            "Kontynuowac?",
            "Tak", "Anuluj");

        if (!ok) return;

        CreateAttackDataAssets();
        AssignAttacksAndFixPrefabs();
        AddTagIfMissing("DeathZone");
        SetupDeathZone();
        SetupGround();
        SetupDynamicCamera();
        SetupBattleManager();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Gotowe!", "Scena bijatykowa skonfigurowana pomyslnie.", "OK");
    }

    [MenuItem("Tools/Battle Scene Setup/1. Utworz Attack Data Assets")]
    public static void CreateAttackDataAssets()
    {
        if (!Directory.Exists(AttackDataFolder))
            Directory.CreateDirectory(AttackDataFolder);
        AssetDatabase.Refresh();

        CreateAttackIfMissing("P1_HighAttack", AttackType.High, HitLevel.Medium,
            new Vector2(0.8f, 0.2f), new Vector2(0.7f, 0.5f), 0.05f, 0.12f, 0.3f);

        CreateAttackIfMissing("P1_LowAttack", AttackType.Low, HitLevel.Light,
            new Vector2(0.7f, -0.2f), new Vector2(0.6f, 0.4f), 0.08f, 0.1f, 0.25f);

        CreateAttackIfMissing("P2_HighAttack", AttackType.High, HitLevel.Medium,
            new Vector2(0.8f, 0.2f), new Vector2(0.7f, 0.5f), 0.05f, 0.12f, 0.3f);

        CreateAttackIfMissing("P2_LowAttack", AttackType.Low, HitLevel.Light,
            new Vector2(0.7f, -0.2f), new Vector2(0.6f, 0.4f), 0.08f, 0.1f, 0.25f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[BattleSetup] AttackData assety gotowe w " + AttackDataFolder);
    }

    private static void CreateAttackIfMissing(string name, AttackType type, HitLevel level,
        Vector2 offset, Vector2 size, float startup, float active, float recovery)
    {
        string path = $"{AttackDataFolder}/{name}.asset";
        if (AssetDatabase.LoadAssetAtPath<AttackData>(path) != null) return;

        var data = ScriptableObject.CreateInstance<AttackData>();
        data.attackType = type;
        data.hitLevel = level;
        data.useDefaultsFromHitLevel = true;
        data.hitboxOffset = offset;
        data.hitboxSize = size;
        data.startupTime = startup;
        data.activeTime = active;
        data.recoveryTime = recovery;

        AssetDatabase.CreateAsset(data, path);
    }

    [MenuItem("Tools/Battle Scene Setup/2. Przypisz Ataki do Prefabow")]
    public static void AssignAttacksAndFixPrefabs()
    {
        FixPrefab("Assets/Prefabs/P1.prefab",
            "Assets/ScriptableObjects/Attacks/P1_HighAttack.asset",
            "Assets/ScriptableObjects/Attacks/P1_LowAttack.asset", isPlayer1: true);

        FixPrefab("Assets/Prefabs/P2.prefab",
            "Assets/ScriptableObjects/Attacks/P2_HighAttack.asset",
            "Assets/ScriptableObjects/Attacks/P2_LowAttack.asset", isPlayer1: false);

        Debug.Log("[BattleSetup] Ataki podpiete do prefabow P1 i P2");
    }

    private static void FixPrefab(string prefabPath, string highPath, string lowPath, bool isPlayer1)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) return;

        var highAttack = AssetDatabase.LoadAssetAtPath<AttackData>(highPath);
        var lowAttack  = AssetDatabase.LoadAssetAtPath<AttackData>(lowPath);

        string assetPath = AssetDatabase.GetAssetPath(prefab);
        using (var scope = new PrefabUtility.EditPrefabContentsScope(assetPath))
        {
            GameObject root = scope.prefabContentsRoot;
            var fc = root.GetComponent<FighterController>();
            if (fc == null) return;

            if (highAttack != null) fc.highAttack = highAttack;
            if (lowAttack != null) fc.lowAttack = lowAttack;

            Transform gc = root.transform.Find("GroundCheck");
            if (gc != null && fc.groundCheck == null) fc.groundCheck = gc;

            var executor = root.GetComponent<AttackExecutor>();
            if (executor != null && executor.hittableLayers.value == 0)
                executor.hittableLayers = 1 << 6;

            if (!isPlayer1)
            {
                fc.leftKey = KeyCode.LeftArrow;
                fc.rightKey = KeyCode.RightArrow;
                fc.crouchKey = KeyCode.DownArrow;
                fc.jumpKey = KeyCode.UpArrow;
                fc.highAttackKey = KeyCode.Keypad1;
                fc.lowAttackKey = KeyCode.Keypad2;
            }
            else
            {
                fc.leftKey = KeyCode.A;
                fc.rightKey = KeyCode.D;
                fc.crouchKey = KeyCode.S;
                fc.jumpKey = KeyCode.W;
                fc.highAttackKey = KeyCode.X;
                fc.lowAttackKey = KeyCode.C;
            }

            EditorUtility.SetDirty(root);
        }
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/Battle Scene Setup/3. Dodaj Death Zone")]
    public static void SetupDeathZone()
    {
        AddTagIfMissing("DeathZone");

        GameObject zone = GameObject.Find("DeathZone");
        if (zone == null)
        {
            zone = new GameObject("DeathZone");
            zone.transform.position = new Vector3(0f, -6f, 0f);
        }

        zone.tag = "DeathZone";

        BoxCollider2D col = zone.GetComponent<BoxCollider2D>();
        if (col == null) col = zone.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(30f, 1f);

        EditorUtility.SetDirty(zone);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    [MenuItem("Tools/Battle Scene Setup/4. Dodaj Podloge (Ground)")]
    public static void SetupGround()
    {
        if (GameObject.Find("Ground") != null) return;

        GameObject ground = new GameObject("Ground");
        ground.transform.position = new Vector3(0f, -1f, 0f);
        ground.layer = LayerMask.NameToLayer("Default");

        BoxCollider2D col = ground.AddComponent<BoxCollider2D>();
        col.size = new Vector2(20f, 0.5f);

        SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.3f, 0.2f, 0.1f);
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        ground.transform.localScale = new Vector3(20f, 0.5f, 1f);

        EditorUtility.SetDirty(ground);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    [MenuItem("Tools/Battle Scene Setup/5. Konfiguruj Dynamic Camera")]
    public static void SetupDynamicCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        cam.orthographic = true;
        cam.orthographicSize = 8f;
        cam.transform.position = new Vector3(0f, 1f, -10f);

        DynamicCamera dyn = cam.GetComponent<DynamicCamera>();
        if (dyn == null) dyn = cam.gameObject.AddComponent<DynamicCamera>();

        if (dyn.player1 == null)
        {
            GameObject p1 = GameObject.Find("P1");
            if (p1 != null) dyn.player1 = p1.transform;
        }
        if (dyn.player2 == null)
        {
            GameObject p2 = GameObject.Find("P2");
            if (p2 != null) dyn.player2 = p2.transform;
        }

        dyn.smoothSpeed = 0.1f;
        dyn.minSize = 5f;
        dyn.maxSize = 12f;
        dyn.zoomFactor = 0.6f;

        EditorUtility.SetDirty(cam.gameObject);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    [MenuItem("Tools/Battle Scene Setup/6. Konfiguruj Battle Manager")]
    public static void SetupBattleManager()
    {
        GameObject bm = GameObject.Find("BattleManager");
        if (bm == null) bm = new GameObject("BattleManager");

        BattleManager manager = bm.GetComponent<BattleManager>();
        if (manager == null) manager = bm.AddComponent<BattleManager>();

        GameObject p1go = GameObject.Find("P1");
        GameObject p2go = GameObject.Find("P2");

        if (p1go != null && manager.player1 == null) manager.player1 = p1go.GetComponent<FighterController>();
        if (p2go != null && manager.player2 == null) manager.player2 = p2go.GetComponent<FighterController>();

        if (manager.spawnPoint1 == null) manager.spawnPoint1 = GetOrCreateSpawnPoint("SpawnPoint1", new Vector3(-3f, 0f, 0f));
        if (manager.spawnPoint2 == null) manager.spawnPoint2 = GetOrCreateSpawnPoint("SpawnPoint2", new Vector3(3f, 0f, 0f));

        if (manager.battleCamera == null) manager.battleCamera = Camera.main;

        EditorUtility.SetDirty(bm);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private static Transform GetOrCreateSpawnPoint(string name, Vector3 pos)
    {
        GameObject existing = GameObject.Find(name);
        if (existing != null) return existing.transform;

        GameObject sp = new GameObject(name);
        sp.transform.position = pos;
        return sp.transform;
    }

    private static void AddTagIfMissing(string tag)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (assets.Length == 0) return;
        SerializedObject tagManager = new SerializedObject(assets[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue.Equals(tag)) return;
        }
        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
}
