using UnityEngine;
using AdvancedShooterKit;
using System.Reflection;

[CreateAssetMenu(fileName = "New Player Upgrade", menuName = "Lost in time/Player Upgrade")]
public class PlayerUpgrade : Upgrade
{
    public enum UpgradeType { Health, MovementSpeed, JumpHeight, SlowDownFactor, RegenInterval, RegenAmount }
    public UpgradeType upgradeType;
    public float upgradeAmount = 10f;
    // Limits to prevent op upgrades
    [Tooltip("Maximum upgrade value")]
    public float maxUpgradeValue = 100f;

    // For jumpHeight only
    [Tooltip("Maximum jump force - only used for JumpHeight upgrade")]
    public float maxJumpForce = 8f;
    public override void ApplyUpgrade(GameObject player)
    {
        PlayerCharacter playerCharacter = player.GetComponent<PlayerCharacter>();
        TimeManipulator timeManipulator = player.GetComponent<TimeManipulator>();
        FirstPersonController fpController = player.GetComponent<FirstPersonController>();
        
        if (playerCharacter == null) return;

        switch (upgradeType)
        {
            case UpgradeType.Health:
                // Increase max health
                playerCharacter.IncreaseMaxHealth((int)upgradeAmount);
                playerCharacter.IncrementHealth((int)upgradeAmount);
                break;

            case UpgradeType.MovementSpeed:
                // Increase movement speed
                if (fpController != null)
                {
                    // Get walk speed via reflection
                    FieldInfo walkSpeedField = typeof(FirstPersonController).GetField("walkSpeed", BindingFlags.NonPublic | BindingFlags.Instance);

                    // Get run speed via reflection
                    FieldInfo runSpeedField = typeof(FirstPersonController).GetField("runSpeed", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (walkSpeedField != null && runSpeedField != null)
                    {
                        // Upgrade walk speed
                        float walkSpeed = (float)walkSpeedField.GetValue(fpController);
                        float newWalkSpeed = Mathf.Min(walkSpeed + upgradeAmount / 10, maxUpgradeValue);
                        walkSpeedField.SetValue(fpController, newWalkSpeed);

                        // Upgrade run speed
                        float runSpeed = (float)runSpeedField.GetValue(fpController);
                        float newRunSpeed = Mathf.Min(runSpeed + (upgradeAmount / 10) * 2, maxUpgradeValue);
                        runSpeedField.SetValue(fpController, newRunSpeed);
                    }
                }
                break;

            case UpgradeType.JumpHeight:
                if (fpController != null)
                {
                    FieldInfo jumpForceField = typeof(FirstPersonController).GetField("jumpForce", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (jumpForceField != null)
                    {
                        float currentJumpForce = (float)jumpForceField.GetValue(fpController);
                        float newJumpForce = Mathf.Min(currentJumpForce + upgradeAmount / 20, maxJumpForce);
                        jumpForceField.SetValue(fpController, newJumpForce);
                    }
                }
                break;
            case UpgradeType.SlowDownFactor:
                if (timeManipulator != null)
                {
                    FieldInfo slowDownFactorField = typeof(TimeManipulator).GetField("slowdownFactor", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (slowDownFactorField != null)
                    {
                        float currentFactor = (float)slowDownFactorField.GetValue(timeManipulator);
                        // Higher value = less slowdown
                        float newFactor = Mathf.Clamp(currentFactor + upgradeAmount / 100, 0.1f, 0.9f);
                        slowDownFactorField.SetValue(timeManipulator, newFactor);
                    }
                }
                break;

            case UpgradeType.RegenAmount:
                if (playerCharacter != null)
                {
                    playerCharacter.increaseRegenAmount((int)upgradeAmount / 10);
                }
                break;

            case UpgradeType.RegenInterval:
                if (playerCharacter != null)
                {
                    playerCharacter.decreaseRegenInterval((float)upgradeAmount / 40);
                }
                break;
        }

    }
}
