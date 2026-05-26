using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Editor tool: Tools ▶ Generate Danger Scenario
/// Builds the "DangerScenario" scene from POLYGON City Pack, ithappy Creative
/// Characters, ithappy Weapons, and Kevin Iglesias Human Animations assets.
/// Missing prefabs are replaced with coloured cube placeholders so the script
/// never throws.
/// </summary>
public static class DangerScenarioGenerator
{
    // ── Asset paths (forward-slashes, relative to project root) ─────────────

    private const string P_STREET  = "Assets/POLYGON city pack/Prefabs/Floor/Street 1 Prefab.prefab";
    private const string P_SIDEWAY = "Assets/POLYGON city pack/Prefabs/Floor/Sideway 1 prefab.prefab";
    private const string P_BLDG_L  = "Assets/POLYGON city pack/Prefabs/Buildings/Building_A1_prefab.prefab";
    private const string P_BLDG_R  = "Assets/POLYGON city pack/Prefabs/Buildings/Building_B_prefab.prefab";
    private const string P_LAMP    = "Assets/POLYGON city pack/Prefabs/Lamps/Lamp_1_prefab.prefab";
    private const string P_TRASH   = "Assets/POLYGON city pack/Prefabs/Props/Big_trash_bin prefab.prefab";
    private const string P_BENCH   = "Assets/POLYGON city pack/Prefabs/Props/bench prefab.prefab";
    private const string P_CHAR    = "Assets/ithappy/Creative_Characters_FREE/Prefabs/Base_Mesh.prefab";
    private const string P_WEAPON  = "Assets/ithappy/Weapons_FREE/Prefabs/knife_001.prefab";

    // ── Entry point ──────────────────────────────────────────────────────────

    [MenuItem("Tools/Generate Danger Scenario")]
    public static void GenerateScene()
    {
        // ── 1. New empty scene ───────────────────────────────────────────────
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Single root keeps the hierarchy tidy
        var root = new GameObject("DangerScenario").transform;

        // ── 2. Directional Light ─────────────────────────────────────────────
        var lightGO       = new GameObject("Directional Light");
        var dl            = lightGO.AddComponent<Light>();
        dl.type           = LightType.Directional;
        dl.intensity      = 1.2f;
        dl.color          = new Color(1f, 0.95f, 0.84f);   // warm sunlight
        dl.shadows        = LightShadows.Soft;
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // ── 3. Road + sidewalk – 3 tiles along Z (each tile ≈ 10 m) ─────────
        var roadGroup = Group("Road", root);
        for (int i = 0; i < 3; i++)
        {
            float z = i * 10f;
            Spawn(P_STREET,  Color.gray,                       V3(  0f, 0, z), Quaternion.identity,         roadGroup, "Street_"   + i);
            Spawn(P_SIDEWAY, new Color(0.75f, 0.70f, 0.65f),  V3(-5.5f, 0, z), Quaternion.identity,         roadGroup, "Sideway_L_" + i);
            Spawn(P_SIDEWAY, new Color(0.75f, 0.70f, 0.65f),  V3( 5.5f, 0, z), Quaternion.Euler(0, 180, 0), roadGroup, "Sideway_R_" + i);
        }

        // ── 4. Buildings – one per side, facing the road ─────────────────────
        var bldgGroup = Group("Buildings", root);
        Spawn(P_BLDG_L, new Color(0.60f, 0.50f, 0.45f), V3(-13f, 0, 10f), Quaternion.Euler(0,  90, 0), bldgGroup, "Building_Left");
        Spawn(P_BLDG_R, new Color(0.50f, 0.55f, 0.70f), V3( 13f, 0, 10f), Quaternion.Euler(0, -90, 0), bldgGroup, "Building_Right");

        // ── 5. Urban props ───────────────────────────────────────────────────
        var propsGroup = Group("Props", root);
        Spawn(P_LAMP,  Color.yellow,               V3(-4.0f, 0,  4f), Quaternion.identity,         propsGroup, "Lampione");
        Spawn(P_TRASH, new Color(0.20f, 0.45f, 0.20f), V3( 4.5f, 0,  8f), Quaternion.Euler(0, -90, 0), propsGroup, "Cassonetto");
        Spawn(P_BENCH, new Color(0.50f, 0.33f, 0.15f), V3(-4.5f, 0, 14f), Quaternion.Euler(0,  90, 0), propsGroup, "Panchina");

        // ── 6. Characters (scene centre ≈ Z = 10) ───────────────────────────
        var charGroup = Group("Characters", root);

        // Aggressore 1 – left of victim, rotated to face victim (+X direction)
        var agg1 = Spawn(P_CHAR, new Color(0.80f, 0.10f, 0.10f),
            V3(-1.2f, 0, 10f), Quaternion.Euler(0,  90f, 0), charGroup, "Aggressore_1");

        // Aggressore 2 – right of victim, rotated to face victim (-X direction)
        Spawn(P_CHAR, new Color(0.65f, 0.10f, 0.10f),
            V3( 1.2f, 0, 10f), Quaternion.Euler(0, -90f, 0), charGroup, "Aggressore_2");

        // Vittima – between aggressors, facing camera (-Z)
        Spawn(P_CHAR, new Color(0.25f, 0.45f, 0.80f),
            V3(0f, 0, 10f), Quaternion.Euler(0, 180f, 0), charGroup, "Vittima");

        // Passante – on left sidewalk, walking in the same direction as camera axis
        Spawn(P_CHAR, new Color(0.35f, 0.60f, 0.35f),
            V3(-5f, 0, 4f), Quaternion.Euler(0, 180f, 0), charGroup, "Passante");

        // ── 7. Weapon attached to Aggressore_1's right hand bone ─────────────
        AttachWeapon(agg1);

        // ── 8. Main Camera – eye height, slight downward tilt ────────────────
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam           = camGO.AddComponent<Camera>();
        cam.fieldOfView   = 60f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane  = 500f;
        camGO.AddComponent<AudioListener>();
        camGO.transform.SetPositionAndRotation(
            V3(0f, 1.6f, -2f),
            Quaternion.Euler(10f, 0f, 0f));

        // ── 9. Save scene ────────────────────────────────────────────────────
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        const string SCENE_PATH = "Assets/Scenes/DangerScenario.unity";
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        AssetDatabase.Refresh();

        Debug.Log("[DangerScenario] Scene saved → " + SCENE_PATH);
        EditorUtility.DisplayDialog("DangerScenario",
            "Scena generata e salvata!\n\nPath: Assets/Scenes/DangerScenario.unity", "OK");
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    /// <summary>Instantiates a prefab (or a placeholder cube) at world pos/rot under parent.</summary>
    private static GameObject Spawn(string assetPath, Color placeholderColor,
        Vector3 worldPos, Quaternion worldRot, Transform parent, string goName)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        GameObject go;

        if (prefab != null)
        {
            // InstantiatePrefab keeps the prefab connection intact
            go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = goName;
        }
        else
        {
            Debug.LogWarning("[DangerScenario] Prefab non trovato: " + assetPath + "  →  placeholder");
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = goName + "_PLACEHOLDER";
            go.transform.SetParent(parent, false);
            ColorObject(go, placeholderColor);
        }

        go.transform.SetPositionAndRotation(worldPos, worldRot);
        return go;
    }

    /// <summary>Creates an empty GameObject as a scene-hierarchy group.</summary>
    private static Transform Group(string name, Transform parent)
    {
        var g = new GameObject(name);
        g.transform.SetParent(parent, false);
        return g.transform;
    }

    /// <summary>
    /// Searches the character hierarchy for a right-hand bone and parents the
    /// weapon to it. Falls back to the character root when no bone is found.
    /// </summary>
    private static void AttachWeapon(GameObject charGO)
    {
        if (charGO == null) return;

        // Common right-hand bone names across Mixamo, Humanoid, Biped rigs
        Transform hand = FindBone(charGO.transform,
            "RightHand", "Hand_R", "hand_r",
            "mixamorig:RightHand", "mixamorig_RightHand",
            "Bip001 R Hand", "r_hand", "R_Hand",
            "HandRight", "hand_right");

        Transform anchor = hand != null ? hand : charGO.transform;

        if (hand == null)
            Debug.LogWarning("[DangerScenario] Bone mano destra non trovato — arma attaccata alla radice del personaggio.");

        var weapon = Spawn(P_WEAPON, new Color(0.15f, 0.15f, 0.15f),
            Vector3.zero, Quaternion.identity, anchor, "Knife");

        if (weapon != null)
        {
            weapon.transform.localPosition = new Vector3(0f, 0f, 0.1f);
            weapon.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            weapon.transform.localScale    = Vector3.one;
        }
    }

    /// <summary>Depth-first bone search; case-insensitive.</summary>
    private static Transform FindBone(Transform root, params string[] names)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            foreach (string n in names)
                if (string.Equals(t.name, n, StringComparison.OrdinalIgnoreCase))
                    return t;
        return null;
    }

    /// <summary>Assigns a solid colour material (URP/Lit with Standard fallback).</summary>
    private static void ColorObject(GameObject go, Color color)
    {
        var mr = go.GetComponent<MeshRenderer>();
        if (mr == null) return;

        Shader sh = Shader.Find("Universal Render Pipeline/Lit")
                 ?? Shader.Find("Standard");
        if (sh == null) return;

        mr.sharedMaterial = new Material(sh) { color = color };
    }

    // Compact Vector3 factory to keep layout code readable
    private static Vector3 V3(float x, float y, float z) => new Vector3(x, y, z);
}
