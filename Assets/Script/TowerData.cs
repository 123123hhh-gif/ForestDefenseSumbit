using UnityEngine;

// 塔的配置数据，每个塔型/等级对应一个实例
[CreateAssetMenu(fileName = "NewTowerData", menuName = "TD/Tower Data")]
public class TowerData : ScriptableObject
{

    public string towerName; 
    public int level; 
    public int cost; 
    public float attackRange; 
    public float attackRate; 
    public int damage; 
    

    [Header("TurretConfig")]
    public float rotateSpeed; 

 
    public TowerData nextLevelData; 
    public GameObject towerPrefab; 
}