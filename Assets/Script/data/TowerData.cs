using UnityEngine;


[CreateAssetMenu(fileName = "NewTowerData", menuName = "TD/Tower Data/Base Tower", order = 1)]
public class TowerData : ScriptableObject
{

    public string towerName;

    public string iconPath;
    public int level;
    public int cost;
    public float attackRange;
    public float attackRate;
    public int damage;


    [Header("TurretConfig")]
    public float rotateSpeed;


    public TowerData nextLevelData;
    public GameObject towerPrefab;

    [Header("BulletConfig")]
    public float bulletSpeed = 30f;
    public Vector3 bulletPosOffset = Vector3.zero;
    public Vector3 bulletRotOffset = new Vector3(0, 0, 0);
}