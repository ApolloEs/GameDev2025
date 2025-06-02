using UnityEngine;
using AdvancedShooterKit;

public class PlayerEquipmentInitializer : MonoBehaviour
{
    [Header("Startup Weapons")]
    [SerializeField] private string primaryWeaponName = "Crossbow6";
    [SerializeField] private string secondaryWeaponName = "Pistol1";
    [SerializeField] private string grenade = "HandGranade4";

    [Header("Startup Ammo")]
    [SerializeField] private string[] ammoTypes = { "ArrowSimple", "PistolAmmo_9mm", "FragGrenade" };
    [SerializeField] private int[] ammoAmounts = { 30, 10, 5 };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke("InitializePlayerEquipment", 0.1f);
    }

    private void InitializePlayerEquipment()
    {
        // Find the player
        PlayerCharacter player = FindObjectOfType<PlayerCharacter>();
        if (player == null)
        {
            Debug.LogError("No PlayerCharacter found in scene!");
            return;
        }

        // Get weapon manager and ammo backpack
        WeaponsManager weaponsManager = FindObjectOfType<WeaponsManager>();
        AmmoBackpack ammoBackpack = FindObjectOfType<AmmoBackpack>();

        if (weaponsManager == null || ammoBackpack == null)
        {
            Debug.LogError("Could not find WeaponsManager or AmmoBackpack!");
            return;
        }

        // Give weapons
        GiveWeapon(weaponsManager, primaryWeaponName);
        GiveWeapon(weaponsManager, secondaryWeaponName);
        GiveWeapon(weaponsManager, grenade);

        // Add ammo
        for (int i = 0; i < ammoTypes.Length; i++)
        {
            if (i < ammoAmounts.Length)
            {
                int amount = ammoAmounts[i];
                AddAmmo(ammoBackpack, ammoTypes[i], amount);
            }
        }

        // Force weapon manager to update the HUD
        weaponsManager.UpdateHud(true);

        Debug.Log("Player equipment initialized successfully!");
    }

    private void GiveWeapon(WeaponsManager weaponsManager, string weaponName)
    {
        // Use the existing pickup method from ASK
        bool success = weaponsManager.PickupWeapon(weaponName, true, 30);

        if (!success)
        {
            Debug.LogWarning($"Failed to give weapon: {weaponName}. Check that the name matches exactly.");
        }
    }

    private void AddAmmo(AmmoBackpack ammoBackpack, string ammoType, int amount)
    {
        // Get current and set new amount
        int current = ammoBackpack.GetCurrentAmmo(ammoType);
        ammoBackpack.SetCurrentAmmo(ammoType, current + amount);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
