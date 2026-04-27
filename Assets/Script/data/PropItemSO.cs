using UnityEditor;
using UnityEngine;

// 道具类型枚举（方便代码识别功能，可根据你的塔防需求扩展）
public enum ItemType
{
    HealthBoost,   // 生命提升
    AttackSpeedBoost,     // 攻速提升
    AttackDamageBoost,      // 伤害提升
    EnemySpeedDebuff,      // 速度减缓道具

}

[CreateAssetMenu(fileName = "NewItem", menuName = "TD/prop Item", order = 2)]
public class PropItemSO : ScriptableObject
{
    [Header("基础属性")]
    public string itemName;          // 道具名称
    public int price;                // 售价
    public ItemType itemType;        // 道具类型（代码识别用）

    public float value;      
    [TextArea] public string desc;   // 功能描述

    [Header("编辑器预览")]
    public Sprite icon;              // 可选：道具图标（商城显示用）
    [HideInInspector] public string uniqueID; // 唯一ID（自动生成，避免重名问题）

    // 编辑器下自动生成唯一ID（防止手动配置出错）
    private void OnValidate()
    {
        // 用物品名+GUID生成唯一ID，也可以直接用GUID
        uniqueID = $"{itemName}_{GUID.Generate()}";
        // 确保名称不为空
        if (string.IsNullOrEmpty(itemName))
        {
            itemName = "未命名道具";
        }
    }
}