using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewShop", menuName = "TD/Shop", order = 3)]
public class ShopSO : ScriptableObject
{
    public string shopName;         
    public List<PropItemSO> sellItems;  
}