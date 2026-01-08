using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Land", menuName = "Land/New Land")]
public class LandDataSO : ScriptableObject
{
    [Header("Base Settings")]
    public LandGrade Grade;
    public GameObject NotOwnedLandPrefab;
    public GameObject OwnedLandPrefab;

    [Header("Building Progression")]
    [Tooltip("Assign BuildingDataSO files here in order of progression.")]
    public List<BuildingDataSO> BuildingLevels;
}

