using System;
using UnityEngine;
using UnityEngine.Perception.Randomization.Randomizers;
using Random = UnityEngine.Random;

[AddRandomizerMenu("Custom/Camera Randomizer")]
public class CameraRandomizer : Randomizer
{
    [Header("Position Offset Range")]
    public float positionRangeX = 2.0f;
    public float positionRangeY = 0.5f;
    public float positionRangeZ = 2.0f;

    [Header("Rotation Offset Range (degrees)")]
    public float rotationRangeX = 10f;
    public float rotationRangeY = 15f;

    [Header("Base Transform")]
    public Vector3 basePosition = new Vector3(4.5f, 1.6f, -2.0f);
    public Vector3 baseRotation = new Vector3(10f, 0f, 0f);

    protected override void OnIterationStart()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        float randomX = basePosition.x + Random.Range(-positionRangeX, positionRangeX);
        float randomY = basePosition.y + Random.Range(-positionRangeY, positionRangeY);
        float randomZ = basePosition.z + Random.Range(-positionRangeZ, positionRangeZ);

        float randomRotX = baseRotation.x + Random.Range(-rotationRangeX, rotationRangeX);
        float randomRotY = baseRotation.y + Random.Range(-rotationRangeY, rotationRangeY);

        mainCamera.transform.position = new Vector3(randomX, randomY, randomZ);
        mainCamera.transform.rotation = Quaternion.Euler(randomRotX, randomRotY, 0f);
    }
}