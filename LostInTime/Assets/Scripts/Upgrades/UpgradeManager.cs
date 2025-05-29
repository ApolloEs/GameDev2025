using UnityEngine;
using System.Collections.Generic;
using AdvancedShooterKit;

public class UpgradeManager : MonoBehaviour
{
    [Header("Upgrade Settings")]
    [SerializeField] private List<Upgrade> availableUpgrades = new List<Upgrade>();
    [SerializeField] private int upgradesPerSelection = 3;
    [SerializeField] private int playerPoints = 0;

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

        Debug.Log("==UPGRADE OPTIONS===");
        for (int i = 0; i < options.Count; i++)
        {
            Debug.Log($"{i + 1}. {options[i].upgradeName}: {options[i].description} (Cost: {options[i].cost})");
        }

        // Simulate selecting the first option
        if (options.Count > 0)
        {
            ApplyUpgrade(options[0]);
        }
    }

    private void ApplyUpgrade(Upgrade upgrade)
    {
        if (playerPoints >= upgrade.cost)
        {
            playerPoints -= upgrade.cost;
            upgrade.ApplyUpgrade(playerObject);
            appliedUpgrades.Add(upgrade);

            Debug.Log($"Applied upgrade: {upgrade.upgradeName}");
        }
        else
        {
            Debug.Log("Not enough points for this upgrade!");
        }
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
