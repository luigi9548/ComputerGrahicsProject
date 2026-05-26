using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Editor tool: Tools ▶ Generate Crowd Scenario
/// Genera la scena "CrowdScenario": piazza urbana 4×4 tile, 4 edifici,
/// props urbani, 10 passanti, 3 aggressori armati, 1 vittima al centro.
/// I prefab mancanti vengono sostituiti con cubi colorati placeholder.
/// </summary>
public static class CrowdScenarioGenerator
{
    // ── Path degli asset ─────────────────────────────────────────────────────

    // Pavimento piazza
    private const string P_TILE = "Assets/POLYGON city pack/Prefabs/Floor/Sideway 1 prefab.prefab";

    // Edifici – uno per lato della piazza
    private const string P_BLDG_N = "Assets/POLYGON city pack/Prefabs/Buildings/Building_A1_prefab.prefab";
    private const string P_BLDG_S = "Assets/POLYGON city pack/Prefabs/Buildings/Building_B_prefab.prefab";
    private const string P_BLDG_E = "Assets/POLYGON city pack/Prefabs/Buildings/Building_C1_prefab.prefab";
    private const string P_BLDG_W = "Assets/POLYGON city pack/Prefabs/Buildings/Building_D1_prefab.prefab";

    // Props urbani
    private const string P_LAMP  = "Assets/POLYGON city pack/Prefabs/Lamps/Lamp_1_prefab.prefab";
    private const string P_BENCH = "Assets/POLYGON city pack/Prefabs/Props/bench prefab.prefab";
    private const string P_BIN   = "Assets/POLYGON city pack/Prefabs/Props/Bin prefab.prefab";

    // Personaggio base (ithappy Creative Characters FREE)
    private const string P_CHAR  = "Assets/ithappy/Creative_Characters_FREE/Prefabs/Base_Mesh.prefab";

    // Armi (ithappy Weapons FREE)
    private const string P_KNIFE  = "Assets/ithappy/Weapons_FREE/Prefabs/knife_001.prefab";
    private const string P_PISTOL = "Assets/ithappy/Weapons_FREE/Prefabs/pistol_001.prefab";

    // ── Colori placeholder ───────────────────────────────────────────────────

    private static readonly Color COL_PASSANTE   = new Color(0.10f, 0.40f, 1.00f);  // blu
    private static readonly Color COL_AGGRESSORE = new Color(1.00f, 0.10f, 0.10f);  // rosso
    private static readonly Color COL_VITTIMA    = new Color(1.00f, 0.90f, 0.00f);  // giallo
    private static readonly Color COL_ARMA       = new Color(0.05f, 0.05f, 0.05f);  // nero
    private static readonly Color COL_EDIFICIO   = new Color(0.50f, 0.50f, 0.50f);  // grigio
    private static readonly Color COL_PROP       = new Color(0.10f, 0.70f, 0.20f);  // verde

    // ── Geometria piazza ─────────────────────────────────────────────────────
    // Ogni tile Sideway ≈ 10 m × 10 m → griglia 4×4 → piazza 40 m × 40 m
    private const float TILE_STEP  = 10f;
    private const int   TILE_GRID  = 4;
    private const float PLAZA_HALF = TILE_STEP * TILE_GRID / 2f;  // 20 m

    // ── Entry point ──────────────────────────────────────────────────────────

    [MenuItem("Tools/Generate Crowd Scenario")]
    public static void GenerateCrowdScenario()
    {
        Debug.Log("[CrowdScenario] Avvio generazione scena…");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root  = new GameObject("CrowdScenario").transform;

        // Seme fisso → layout deterministico a ogni esecuzione
        UnityEngine.Random.InitState(42);

        // 1. Luce direzionale
        CreateDirectionalLight(root);

        // 2. Piazza (pavimento + edifici + props)
        BuildPlaza(root);

        // 3. Tutti i personaggi + armi
        PlaceAllCharacters(root);

        // 4. Telecamera di sorveglianza
        SetupCamera(root);

        // 5. Salvataggio scena
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        const string SCENE_PATH = "Assets/Scenes/CrowdScenario.unity";
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        AssetDatabase.Refresh();

        Debug.Log("[CrowdScenario] Scena salvata → " + SCENE_PATH);
        EditorUtility.DisplayDialog("CrowdScenario",
            "Scena generata e salvata!\n\nPath: " + SCENE_PATH, "OK");
    }

    // ── Luce ─────────────────────────────────────────────────────────────────

    private static void CreateDirectionalLight(Transform root)
    {
        var go        = new GameObject("Directional Light");
        go.transform.SetParent(root);
        var dl        = go.AddComponent<Light>();
        dl.type       = LightType.Directional;
        dl.intensity  = 1.2f;
        dl.color      = new Color(1f, 0.95f, 0.84f);
        dl.shadows    = LightShadows.Soft;
        go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    // ── Piazza ────────────────────────────────────────────────────────────────

    private static void BuildPlaza(Transform root)
    {
        var plaza = Group("Plaza", root);
        BuildFloor(plaza);
        BuildBuildings(plaza);
        PlaceProps(plaza);
    }

    // Griglia 4×4 di tile Sideway centrata in (0,0,0)
    private static void BuildFloor(Transform parent)
    {
        var floorGrp = Group("Floor", parent);
        float origin = -PLAZA_HALF + TILE_STEP * 0.5f;  // prima tile a -15 m

        for (int row = 0; row < TILE_GRID; row++)
        {
            for (int col = 0; col < TILE_GRID; col++)
            {
                Spawn(P_TILE, new Color(0.65f, 0.65f, 0.68f),
                    V3(origin + col * TILE_STEP, 0f, origin + row * TILE_STEP),
                    Quaternion.identity,
                    floorGrp, $"Tile_{row}_{col}");
            }
        }
    }

    // 4 edifici, uno per ogni lato, orientati verso il centro
    private static void BuildBuildings(Transform parent)
    {
        var bldgGrp = Group("Buildings", parent);
        float d = PLAZA_HALF + 8f;  // 28 m dal centro

        Spawn(P_BLDG_N, COL_EDIFICIO, V3(  0, 0,  d), Quaternion.Euler(0, 180, 0), bldgGrp, "Building_North");
        Spawn(P_BLDG_S, COL_EDIFICIO, V3(  0, 0, -d), Quaternion.Euler(0,   0, 0), bldgGrp, "Building_South");
        Spawn(P_BLDG_E, COL_EDIFICIO, V3(  d, 0,  0), Quaternion.Euler(0, -90, 0), bldgGrp, "Building_East");
        Spawn(P_BLDG_W, COL_EDIFICIO, V3( -d, 0,  0), Quaternion.Euler(0,  90, 0), bldgGrp, "Building_West");
    }

    // 3 lampioni, 2 panchine, 3 cestini sparsi ai margini della piazza
    private static void PlaceProps(Transform parent)
    {
        var propsGrp = Group("Props", parent);

        // Lampioni (3)
        Spawn(P_LAMP, COL_PROP, V3(-16f, 0f,  16f), Quaternion.identity,         propsGrp, "Lampione_1");
        Spawn(P_LAMP, COL_PROP, V3( 16f, 0f, -16f), Quaternion.identity,         propsGrp, "Lampione_2");
        Spawn(P_LAMP, COL_PROP, V3(  0f, 0f,  18f), Quaternion.identity,         propsGrp, "Lampione_3");

        // Panchine (2)
        Spawn(P_BENCH, COL_PROP, V3(-12f, 0f,  15f), Quaternion.Euler(0,  90, 0), propsGrp, "Panchina_1");
        Spawn(P_BENCH, COL_PROP, V3( 12f, 0f,  15f), Quaternion.Euler(0, -90, 0), propsGrp, "Panchina_2");

        // Cestini (3)
        Spawn(P_BIN, COL_PROP, V3(-18f, 0f,   0f), Quaternion.identity,         propsGrp, "Cestino_1");
        Spawn(P_BIN, COL_PROP, V3(  0f, 0f, -18f), Quaternion.Euler(0,  90, 0), propsGrp, "Cestino_2");
        Spawn(P_BIN, COL_PROP, V3( 18f, 0f,   7f), Quaternion.identity,         propsGrp, "Cestino_3");
    }

    // ── Personaggi ────────────────────────────────────────────────────────────

    private static void PlaceAllCharacters(Transform root)
    {
        var charGrp = Group("Characters", root);

        // ── 10 Passanti – posizioni casuali sparse, rotazioni Y casuali ───────
        // Evitano il centro (minRadius) dove si trova la vittima
        Vector3[] bpPositions = RandomPositions(10, PLAZA_HALF - 2f, minRadius: 6f);
        for (int i = 0; i < 10; i++)
        {
            float rotY = UnityEngine.Random.Range(0f, 360f);
            Spawn(P_CHAR, COL_PASSANTE, bpPositions[i],
                Quaternion.Euler(0f, rotY, 0f),
                charGrp, $"Passante_{i + 1}");
        }

        // ── Vittima – centro della piazza ──────────────────────────────────────
        var victimPos = Vector3.zero;
        Spawn(P_CHAR, COL_VITTIMA, victimPos,
            Quaternion.Euler(0f, 180f, 0f),
            charGrp, "Vittima");

        // ── 3 Aggressori – cluster centro-destra, convergono sulla vittima ─────
        // Armi: Aggressore_1 → coltello, _2 → pistola, _3 → coltello
        (Vector3 pos, string weapon)[] aggrSlots =
        {
            (V3(4.0f, 0f,  1.5f), "knife"),
            (V3(4.8f, 0f,  0.0f), "pistol"),
            (V3(4.0f, 0f, -1.5f), "knife"),
        };

        for (int i = 0; i < aggrSlots.Length; i++)
        {
            var (aPos, wType) = aggrSlots[i];

            // Rotazione verso la vittima
            Vector3 dir  = (victimPos - aPos).normalized;
            float   rotY = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            var aggrGO = Spawn(P_CHAR, COL_AGGRESSORE, aPos,
                Quaternion.Euler(0f, rotY, 0f),
                charGrp, $"Aggressore_{i + 1}");

            AttachWeapon(aggrGO, wType);
        }
    }

    // ── Armi ──────────────────────────────────────────────────────────────────

    private static void AttachWeapon(GameObject charGO, string weaponType)
    {
        if (charGO == null) return;

        string weaponPath = weaponType == "knife" ? P_KNIFE : P_PISTOL;

        // Cerca il bone della mano destra con i nomi comuni dei vari rig
        Transform hand = FindBone(charGO.transform,
            "mixamorig:RightHand", "mixamorig_RightHand",
            "RightHand",  "Hand_R",  "R_Hand",  "hand_r",
            "right_hand", "HandRight", "RightWrist", "R_Wrist",
            "Bip001 R Hand", "Bip01 R Hand");

        Transform anchor = hand != null ? hand : charGO.transform;
        if (hand == null)
            Debug.LogWarning(
                $"[CrowdScenario] Bone mano destra non trovato in {charGO.name}" +
                " — arma attaccata alla radice del personaggio.");

        var weaponGO = Spawn(weaponPath, COL_ARMA,
            Vector3.zero, Quaternion.identity, anchor, weaponType);

        // Offset locale per posizionare l'arma nella mano
        weaponGO.transform.localPosition = new Vector3(0f, 0f, 0.10f);
        weaponGO.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        weaponGO.transform.localScale    = Vector3.one;
    }

    // ── Telecamera di sorveglianza ────────────────────────────────────────────

    private static void SetupCamera(Transform root)
    {
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        camGO.transform.SetParent(root);
        camGO.AddComponent<AudioListener>();

        var cam           = camGO.AddComponent<Camera>();
        cam.fieldOfView   = 60f;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane  = 500f;

        // Bordo della piazza, palo a y=8, angolazione 45° verso il basso sul centro
        camGO.transform.SetPositionAndRotation(
            V3(PLAZA_HALF, 8f, PLAZA_HALF),
            Quaternion.Euler(45f, -135f, 0f));
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Istanzia il prefab al path indicato (con PrefabUtility) o crea un cubo
    /// colorato placeholder se il file non esiste.
    /// </summary>
    private static GameObject Spawn(string assetPath, Color placeholderColor,
        Vector3 worldPos, Quaternion worldRot, Transform parent, string goName)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        GameObject go;

        if (prefab != null)
        {
            go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = goName;
        }
        else
        {
            Debug.LogWarning("[CrowdScenario] Prefab non trovato: " + assetPath + "  →  placeholder");
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = goName + "_PLACEHOLDER";
            go.transform.SetParent(parent, false);
            ColorObject(go, placeholderColor);
        }

        go.transform.SetPositionAndRotation(worldPos, worldRot);
        return go;
    }

    /// <summary>Crea un GameObject vuoto come nodo-gruppo nella gerarchia.</summary>
    private static Transform Group(string name, Transform parent)
    {
        var g = new GameObject(name);
        g.transform.SetParent(parent, false);
        return g.transform;
    }

    /// <summary>
    /// Ricerca depth-first (GetComponentsInChildren) del bone per nome,
    /// confronto case-insensitive tra tutti i nomi forniti.
    /// </summary>
    private static Transform FindBone(Transform root, params string[] names)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            foreach (string n in names)
                if (string.Equals(t.name, n, StringComparison.OrdinalIgnoreCase))
                    return t;
        return null;
    }

    /// <summary>
    /// Genera <paramref name="count"/> posizioni casuali entro ±halfExtent
    /// su X e Z, ognuna ad almeno minRadius m dal centro.
    /// </summary>
    private static Vector3[] RandomPositions(int count, float halfExtent, float minRadius)
    {
        var result = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            Vector3 p;
            int tries = 0;
            do
            {
                float x = UnityEngine.Random.Range(-halfExtent, halfExtent);
                float z = UnityEngine.Random.Range(-halfExtent, halfExtent);
                p = new Vector3(x, 0f, z);
            }
            while (p.magnitude < minRadius && ++tries < 60);
            result[i] = p;
        }
        return result;
    }

    /// <summary>Assegna un materiale URP/Lit (o Standard come fallback) con il colore dato.</summary>
    private static void ColorObject(GameObject go, Color color)
    {
        var mr = go.GetComponent<MeshRenderer>();
        if (mr == null) return;
        Shader sh = Shader.Find("Universal Render Pipeline/Lit")
                 ?? Shader.Find("Standard");
        if (sh == null) return;
        mr.sharedMaterial = new Material(sh) { color = color };
    }

    /// <summary>Shorthand per new Vector3.</summary>
    private static Vector3 V3(float x, float y, float z) => new Vector3(x, y, z);
}
