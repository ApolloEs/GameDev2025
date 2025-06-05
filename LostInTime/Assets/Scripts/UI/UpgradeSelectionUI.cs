using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using AdvancedShooterKit;

public class UpgradeSelectionUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private TextMeshProUGUI headerText;

    [Header("Upgrade Option slots")]
    [SerializeField] private UpgradeOptionUI[] upgradeSlots = new UpgradeOptionUI[3];

    [Header("Animation")]
    [SerializeField] private Animator panelAnimator;
    [SerializeField] private float appearDuration = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip selectSound;

    private UpgradeManager upgradeManager;
    private List<Upgrade> currentOptions = new List<Upgrade>();
    private bool isSelectionActive = false;

    // Add reference to ASKInputManger
    private ASKInputManager inputManager;

    private void Awake()
    {
        upgradeManager = FindObjectOfType<UpgradeManager>();

        // Find the ASKInputManager
        inputManager = FindObjectOfType<ASKInputManager>();

        selectionPanel.SetActive(false);

        // Subscribe to click events for each option
        for (int i = 0; i < upgradeSlots.Length; i++)
        {
            int index = i;
            upgradeSlots[i].button.onClick.AddListener(() => OnUpgradeSelected(index));
        }
    }

    public void ShowUpgradeOptions(List<Upgrade> options)
    {
        if (isSelectionActive) return;

        currentOptions = options;
        isSelectionActive = true;

        // Show and unlock cursor
        if (inputManager != null)
        {
            inputManager.UnblockCursor();
            Debug.Log("Cursor unblocked");
        }

        // Pause game
        Time.timeScale = 0f;

        // Set up each option slot
        for (int i = 0; i < upgradeSlots.Length; i++)
        {
            if (i < options.Count)
            {
                upgradeSlots[i].gameObject.SetActive(true);
                upgradeSlots[i].SetUpgradeInfo(options[i]);
            }
            else
            {
                upgradeSlots[i].gameObject.SetActive(false);
            }
        }

        // Show panel
        selectionPanel.SetActive(true);

        // play animation if available
        if (panelAnimator != null)
        {
            panelAnimator.SetTrigger("Show");
        }

        // Play sound
        if (openSound != null)
        {
            AudioSource.PlayClipAtPoint(openSound, Camera.main.transform.position);
        }
    }

    private void OnUpgradeSelected(int index)
    {
        if (index >= 0 && index < currentOptions.Count)
        {
            // Apply the selected upgrade
            upgradeManager.ApplySelectedUpgrade(currentOptions[index]);

            // Play sound
            if (selectSound != null)
            {
                AudioSource.PlayClipAtPoint(selectSound, Camera.main.transform.position);
            }

            ClosePanel();
        }
    }

    public void ClosePanel()
    {
        isSelectionActive = false;

        // Hide and lock cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Resume game if paused
        Time.timeScale = 1f;

        // Hide panel
        selectionPanel.SetActive(false);

        if (inputManager != null)
        {
            inputManager.BlockCursor();
            Debug.Log("Cursor blocked");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
