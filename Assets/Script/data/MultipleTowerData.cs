using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMultipleTowerData", menuName = "TD/Tower Data/Multiple Tower",order =2)]
public class MultipleTowerData : TowerData
{


    [Header("MultipleTowerConfig")]
    public int AttackNumber = 3;


}
