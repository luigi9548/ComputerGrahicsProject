using UnityEngine;
using UnityEngine.Perception.Randomization.Randomizers;

[System.Serializable]
public class LightingPreset
{
    public Material skybox;
    public float lightIntensity;
    public float lightTemperature;
}

[AddRandomizerMenu("Custom/Lighting Randomizer")]
public class LightingRandomizer : Randomizer
{
    public LightingPreset[] presets;

    protected override void OnIterationStart()
    {
        if (presets == null || presets.Length == 0) return;

        int index = UnityEngine.Random.Range(0, presets.Length);
        LightingPreset preset = presets[index];

        // Applica skybox
        RenderSettings.skybox = preset.skybox;
        DynamicGI.UpdateEnvironment();

        // Applica luce
        Light[] lights = Object.FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                light.intensity = preset.lightIntensity;
                light.colorTemperature = preset.lightTemperature;
                light.useColorTemperature = true;
            }
        }
    }
}