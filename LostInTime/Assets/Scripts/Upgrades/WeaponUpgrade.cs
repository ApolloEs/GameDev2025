using UnityEngine;
using AdvancedShooterKit;
using System.Reflection;

[CreateAssetMenu(fileName="New Weapon Upgrade", menuName="Lost in Time/Weapon Upgrade")]
public class WeaponUpgrade : Upgrade
{
    public enum UpgradeType { Damage, FireRate, AmmoCapacity }
    public UpgradeType upgradeType;
    public float upgradeAmount = 10f;

    // For display in the upgrade UI
    [Tooltip("Maximum upgrade level")]
    public int maxLevel = 5;
    [Tooltip("How the upgrade scales with each level")]
    public float upgradeScaling = 1.0f; // Higher value = faster scaling

    public override void ApplyUpgrade(GameObject player)
    {
        // Get all active firearms 
        Firearms[] allWeapons = player.GetComponentsInChildren<Firearms>(true);
        if (allWeapons.Length == 0)
        {
            Debug.LogWarning("No weapons found on player");
            return;
        }
        foreach (Firearms weapon in allWeapons)
        {
            if (weapon.name.ToLower().Contains("HandGrenade"))
            {
                // Skip upgrades for grenade as it has been changed to be a time grenade
                continue;
            }
            switch (upgradeType)
                {
                    case UpgradeType.Damage:
                        // Increase weapon damage
                        weapon.addDamage += upgradeAmount;
                        break;

                    case UpgradeType.FireRate:
                        // Increase fire rate using reflection
                        FieldInfo fireRateField = typeof(WeaponBase).GetField("rateOfFire", BindingFlags.NonPublic | BindingFlags.Instance);

                        if (fireRateField != null)
                        {
                            float currentFireRate = (float)fireRateField.GetValue(weapon);
                            // Lower value = faster fire rate
                            float newFireRate = Mathf.Max(0.05f, currentFireRate - upgradeAmount / 100);
                            fireRateField.SetValue(weapon, newFireRate);
                            Debug.Log($"Upgraded {weapon.name} fire rate. New rate: {newFireRate}");
                        }    
                        break;
    
                    case UpgradeType.AmmoCapacity:
                        // Increase ammo capacity
                        weapon.maxAmmo += (int)upgradeAmount;
                        Debug.Log($"Upgraded {weapon.name} ammo capacity by {upgradeAmount}. New capacity: {weapon.maxAmmo}");
                        break;
                }
        }
    }
}
