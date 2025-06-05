using UnityEngine;
using System.Collections.Generic;
using AdvancedShooterKit;

public class UpgradeManager : MonoBehaviour
{
    [Header("Upgrade Settings")]
    [SerializeField] private List<Upgrade> availableUpgrades = new List<Upgrade>();
    [SerializeField] private int upgradesPerSelection = 3;
    [SerializeField] private int playerPoints = 0;

    [Header("Experience System")]
    [SerializeField] private int pointsPerKill = 50;
    [SerializeField] private int currentKills = 0;
    [SerializeField] private int totalKills = 0;
    [SerializeField] private int[] killsRequiredForUpgrade = new int[] { 1, 3, 4, 5, 7, 10, 13, 15, 20, 30, 40, 50 };
    [SerializeField] private int currentKillThresholdIndex = 0;
    [SerializeField] private float experienceProgress = 0f; // 0-1 for UI display

    [Header("UI")]
    [SerializeField] private UpgradeSelectionUI upgradeSelectionUI;

    [Header("Testing")]
    [SerializeField] private KeyCode testUpgradeKey = KeyCode.U;

    private List<Upgrade> appliedUpgrades = new List<Upgrade>();
    private GameObject playerObject;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        playerObject = FindObjectOfType<PlayerCharacter>().gameObject;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(testUpgradeKey))
        {
            ShowUpgradesSelection();
        }
    }

    public void AddPoints(int points)
    {
        playerPoints += points;
    }

    public void ShowUpgradesSelection()
    {
        // Await for UI
        // For now log the options and select a random one
        List<Upgrade> options = GetRandomUpgrades(upgradesPerSelection);

        if (options.Count > 0)
        {
            if (upgradeSelectionUI != null)
            {
                // Show the UI
                upgradeSelectionUI.ShowUpgradeOptions(options);
            }
            else
            {
                // Fallback
                Debug.LogWarning("UpgradeSelectionUI not assigned, using random selection");
                ApplyUpgrade(options[0]);
            }
        }
    }

    public void ApplySelectedUpgrade(Upgrade upgrade)
    {
        ApplyUpgrade(upgrade);
    }

    private void ApplyUpgrade(Upgrade upgrade)
    {
        upgrade.ApplyUpgrade(playerObject);
        appliedUpgrades.Add(upgrade);

        Debug.Log($"Applied upgrade: {upgrade.upgradeName}");
    }

    private List<Upgrade> GetRandomUpgrades(int count)
    {
        List<Upgrade> result = new List<Upgrade>();
        List<Upgrade> availableCopy = new List<Upgrade>(availableUpgrades);

        count = Mathf.Min(count, availableCopy.Count);
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, availableCopy.Count);
            result.Add(availableCopy[index]);
            availableCopy.RemoveAt(index);
        }

        return result;
    }

    // Call this from Game manager
    public void OnWaveComplete(int waveNumber)
    {
        // Award points based on wave number
        AddPoints(waveNumber * 100);

        // Show upgrade selection
        ShowUpgradesSelection();
    }
}
