using System;
using UnityEngine;


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