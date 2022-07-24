using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PrefabSetting : ScriptableObject
{
    public GameObject targetPrefab;
    public GameObject resetLocPrefab;
    public GameObject realWallPrefab;
    public GameObject RLPrefab;
    public GameObject arrangementRLPrefab;
    public Material realMaterial;
    public Material virtualMaterial;
    public Material obstacleMaterial;
}
