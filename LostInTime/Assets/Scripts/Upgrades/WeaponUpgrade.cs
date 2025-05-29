using UnityEngine;
using AdvancedShooterKit;

[CreateAssetMenu(fileName="New Weapon Upgrade", menuName="Lost in Time/Weapon Upgrade")]
public class WeaponUpgrade : Upgrade
{
    public enum UpgradeType { Damage, FireRate, ReloadSpeed, AmmoCapacity }
    public UpgradeType upgradeType;
    public float upgradeAmount = 10f;

    public override void ApplyUpgrade(GameObject player)
    {
        WeaponsManager weaponsManager = FindObjectOfType<WeaponsManager>();
        if (weaponsManager == null) return;

        switch (upgradeType)
        {
            case UpgradeType.Damage:
                // Increase weapon damage
                break;
            case UpgradeType.FireRate:
                // Increase fire rate
                break;
            case UpgradeType.ReloadSpeed:
                // Increase Reload speed
                break;
            case UpgradeType.AmmoCapacity:
                // Increase ammo capacity
                break;
        }
    }
}
