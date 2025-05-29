using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using AdvancedShooterKit;

public class TimeManipulator : MonoBehaviour
{
    [Header("Time Settings")]
    [SerializeField] private float slowdownFactor = 0.3f;
    [SerializeField] private float slowdownDuration = 5f;
    [SerializeField] private float cooldownDuration = 10f;
    [SerializeField] private Image cooldownIndicator;

    [Header("Visual Effects")]
    [SerializeField] private Color timeSlowColor = new Color(0.5f, 0.8f, 1f);
    [SerializeField] private AudioClip timeSlowSound;
    [SerializeField] private AudioClip timeResumeSound;

    private bool canUsePower = true;
    private float cooldownTimer = 0f;
    private PlayerCharacter playerCharacter;

    private void Awake()
    {
        playerCharacter = GetComponent<PlayerCharacter>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Check for cooldown
        if (!canUsePower)
        {
            cooldownTimer -= Time.unscaledDeltaTime;

            if (cooldownIndicator != null)
                cooldownIndicator.fillAmount = 1 - (cooldownTimer / cooldownDuration);

            if (cooldownTimer <= 0)
            {
                canUsePower = true;
                if (cooldownIndicator != null)
                    cooldownIndicator.fillAmount = 1f;
            }
        }

        // Check for input (Q key)
        if (Input.GetKeyDown(KeyCode.Q) && canUsePower)
        {
            StartCoroutine(SlowDownTime());
        }
    }

    private IEnumerator SlowDownTime()
    {
        // Start cooldown
        canUsePower = false;

        // Play time slow effect
        AudioSource.PlayClipAtPoint(timeSlowSound, transform.position);

        // Apply slow motion
        Time.timeScale = slowdownFactor;
        // adjust fixed timestep for physics
        Time.fixedDeltaTime = Time.timeScale * 0.02f;

        // Apply visual effects
        Debug.Log("started slowdown");
        // wait for duration (using unscaled time)
        float timer = 0f;
        while (timer < slowdownDuration)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        // Return to normal time
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // Play resume sound
        AudioSource.PlayClipAtPoint(timeResumeSound, transform.position);

        // Remove visual effects
        Debug.Log("finished slowdown");
        // start cooldown
        cooldownTimer = cooldownDuration;
    }

    // Method for upgrades to improve time power
    public void UpgradeSlowdownDuration(float additionalDuration)
    {
        slowdownDuration += additionalDuration;
    }

    public void UpgradeCooldownReduction(float reductionAmount)
    {
        cooldownDuration -= reductionAmount;
        cooldownDuration = Mathf.Max(3f, cooldownDuration); // ensure cd always > 3sec
    }
}
