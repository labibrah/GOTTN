using UnityEngine;

[System.Serializable]
public class WindGuideStep
{
    public string stepName;          // e.g. "Guide to QuebecArea" — for debugging in Inspector
    public Transform targetPoint;    // where the wind leads the player
    public BoolValue exploredFlag;   // shared SO, same pattern as your Breakable persistence
    public bool isFinalStep;         // true only for World1BossFight — never picked until all others are explored
}