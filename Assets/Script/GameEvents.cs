using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// Used together with EventManager to trigger and listen for events.

public static class GameEvents
{
    public const string HeadChanged = "HeadChanged";   // Triggered when the player's health value changes
    public const string TooltipChanged = "TooltipChanged";// Triggered when tooltip content needs to be updated

    public const string NameChanged = "NameChanged";// Triggered when a displayed name (e.g., tower/enemy) changes
}
