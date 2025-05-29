using UnityEngine;

[CreateAssetMenu(fileName = "Upgrades", menuName = "Scriptable Objects/Upgrades")]
public abstract class Upgrade : ScriptableObject
{
    public string upgradeName;
    public string description;
    public Sprite icon;
    public int cost = 100;

    // This will be overridden by child classes
    public abstract void ApplyUpgrade(GameObject player);
}
