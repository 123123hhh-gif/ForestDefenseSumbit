using System;
using UnityEngine;

// 玩家数据类，用于存储单个玩家的排行榜信息
[Serializable]
public class PlayerData
{
    public string playerId;

    public int totalStars;


    public PlayerData(string id, int stars)
    {
        playerId = id;
        totalStars = stars;
    }
}