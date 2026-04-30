using UnityEditor;
using UnityEngine;


public enum ItemType
{
    HealthBoost,   
    AttackSpeedBoost,     
    AttackDamageBoost,     
    EnemySpeedDebuff,      

}

[CreateAssetMenu(fileName = "NewItem", menuName = "TD/prop Item", order = 2)]
public class PropItemSO : ScriptableObject
{
    [Header("基础属性")]
    public string itemName;          
    public int price;                
    public ItemType itemType;        

    public float value;      
    [TextArea] public string desc;   

    [Header("编辑器预览")]
    public Sprite icon;              
    [HideInInspector] public string uniqueID; 


    private void OnValidate()
    {

        uniqueID = $"{itemName}_{GUID.Generate()}";

        if (string.IsNullOrEmpty(itemName))
        {
            itemName = "Unnamed Item";
        }
    }
}