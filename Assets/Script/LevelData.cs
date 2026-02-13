using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "TD/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName;
    public int levelIndex;
    public int initialGold;

    [Header("TowerItemConfig")]
    public GameObject towerItemPrefab;
    public List<TowerData> availableTowers;
}
