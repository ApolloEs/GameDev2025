using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Reflection;
using AdvancedShooterKit;

public class TimeManipulator : MonoBehaviour
{
    [Header("Time Settings")]
    [SerializeField] private float slowdownFactor = 0.9f;
    [SerializeField] private float slowdownDuration = 5f;
    [SerializeField] private float cooldownDuration = 10f;
    [SerializeField] private Image cooldownIndicator;

    [Header("Visual Effects")]
    [SerializeField] private Color timeSlowColor = new Color(0.5f, 0.8f, 1f);
    [SerializeField] private AudioClip timeSlowSound;
    [SerializeField] private AudioClip timeResumeSound;

    // ASK components
    private FirstPersonController fpController;
    private FirstPersonWeaponSway weaponSway;
    private Animator playerAnimator;

    // Chached speed values from FirstPersonController
    private float originalWalkSpeed;
    private float originalRunSpeed;
    private float originalBackwardsSpeed;
    private float originalSideWaysSpeed;
    private float originalJumpForce;
    private float originalGravityMultiplier;

    // Original animations speed 
    private float originalAnimatorSpeed = 1f;
    private float originalWeaponSwaySpeed = 1f;

    // State tracking
    private bool isSlowed = false;

    // Fields for reflection access
    private FieldInfo walkSpeedField;
    private FieldInfo runSpeedField;
    private FieldInfo backwardsSpeedField;
    private FieldInfo sidewaysSpeedField;
    private FieldInfo jumpForceField;
    private FieldInfo gravityMultiplierField;

    // Misc
    private bool canUsePower = true;
    private float cooldownTimer = 0f;
    private PlayerCharacter playerCharacter;

    private void Awake()
    {
        playerCharacter = GetComponent<PlayerCharacter>();
        fpController = GetComponent<FirstPersonController>();

        // Find weapon sway component
        weaponSway = GetComponentInChildren<FirstPersonWeaponSway>();

        // Find animator
        playerAnimator = GetComponentInChildren<Animator>();

        // Get FieldInfo for speed variables using reflection
        BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
        walkSpeedField = typeof(FirstPersonController).GetField("walkSpeed", flags);
        runSpeedField = typeof(FirstPersonController).GetField("runSpeed", flags);
        backwardsSpeedField = typeof(FirstPersonController).GetField("backwardsSpeed", flags);
        sidewaysSpeedField = typeof(FirstPersonController).GetField("sidewaysSpeed", flags);
        jumpForceField = typeof(FirstPersonController).GetField("jumpForce", flags);
        gravityMultiplierField = typeof(FirstPersonController).GetField("gravityMultiplier", flags);

        // Cache original values
        if (fpController != null && walkSpeedField != null)
        {
            originalWalkSpeed = (float)walkSpeedField.GetValue(fpController);
            originalRunSpeed = (float)runSpeedField.GetValue(fpController);
            originalBackwardsSpeed = (float)backwardsSpeedField.GetValue(fpController);
            originalSideWaysSpeed = (float)sidewaysSpeedField.GetValue(fpController);
            originalJumpForce = (float)jumpForceField.GetValue(fpController);
            originalGravityMultiplier = (float)gravityMultiplierField.GetValue(fpController);
        }
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

        // Check for input (O key)
        if (Input.GetKeyDown(KeyCode.O))
        {
            StartCoroutine(SlowTimeNow());
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            ResetTimeScale();
        }
    }



    public void SlowDownTime()
    {
        if (isSlowed) return;

        Debug.Log("TimeManipulator: Applying slow down to player");
        isSlowed = true;

        // Apply slowdown to FirstPersonController variabes
        if (fpController != null && walkSpeedField != null)
        {
            float slowedWalkSpeed = originalWalkSpeed * slowdownFactor;
            walkSpeedField.SetValue(fpController, slowedWalkSpeed);

            float slowedRunSpeed = originalRunSpeed * slowdownFactor;
            runSpeedField.SetValue(fpController, slowedRunSpeed);

            float slowedJumpForce = originalJumpForce * Mathf.Sqrt(slowdownFactor);
            jumpForceField.SetValue(fpController, slowedJumpForce);

            float slowedGravity = originalGravityMultiplier * slowdownFactor;
            gravityMultiplierField.SetValue(fpController, slowedGravity);

            backwardsSpeedField.SetValue(fpController, originalBackwardsSpeed * slowdownFactor);
            sidewaysSpeedField.SetValue(fpController, originalSideWaysSpeed * slowdownFactor);
        }

        if (weaponSway != null)
        {
            originalWeaponSwaySpeed = weaponSway.moveSpeed;
            weaponSway.moveSpeed *= slowdownFactor;
        }

        // Slow animations
        if (playerAnimator != null)
        {
            originalAnimatorSpeed = playerAnimator.speed;
            playerAnimator.speed = slowdownFactor;
        }

        // Play effect sound
        if (timeSlowSound != null)
        {
            AudioSource.PlayClipAtPoint(timeSlowSound, transform.position);
        }

    }

    public IEnumerator SlowTimeNow()
    {
        canUsePower = false;

        SlowDownTime();

        float timer = 0f;
        while (timer < slowdownDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        ResetTimeScale();
    }

    public bool ResetTimeScale()
    {
        if (!isSlowed) return false;

        Debug.Log("TimeManipulator: Removing the time effect from player");
        isSlowed = false;

        // Restore FirstPersonController speed variables
        if (fpController != null && walkSpeedField != null)
        {
            walkSpeedField.SetValue(fpController, originalWalkSpeed);
            runSpeedField.SetValue(fpController, originalRunSpeed);
            backwardsSpeedField.SetValue(fpController, originalBackwardsSpeed);
            sidewaysSpeedField.SetValue(fpController, originalSideWaysSpeed);
            jumpForceField.SetValue(fpController, originalJumpForce);
            gravityMultiplierField.SetValue(fpController, originalGravityMultiplier);
        }

        // Restore weapon sway
            if (weaponSway != null)
            {
                weaponSway.moveSpeed = originalWeaponSwaySpeed;
            }

        // Restore animator speed
        if (playerAnimator != null)
        {
            playerAnimator.speed = originalAnimatorSpeed;
        }

        // Play resume sound
        if (timeResumeSound != null)
        {
            AudioSource.PlayClipAtPoint(timeResumeSound, transform.position);
        }

        cooldownTimer = cooldownDuration;

        return true;

        // // Return to normal time
        // Time.timeScale = 1f;
        // Time.fixedDeltaTime = 0.02f;

        // // Play resume sound
        // if (timeResumeSound != null)
        // {
        //     AudioSource.PlayClipAtPoint(timeResumeSound, transform.position);
        // }

        // // Remove visual effects
        // Debug.Log("finished slowdown");
        // // start cooldown
        // cooldownTimer = cooldownDuration;

        // return true;
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
