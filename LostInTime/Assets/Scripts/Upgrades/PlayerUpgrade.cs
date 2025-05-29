using UnityEngine;
using AdvancedShooterKit;

[CreateAssetMenu(fileName = "New Player Upgrade", menuName = "Lost in time/Player Upgrade")]
public class PlayerUpgrade : Upgrade
{
    public enum UpgradeType { Health, MovementSpeed, JumpHeight, TimeSlowDuration, TimeSlowCooldown }
    public UpgradeType upgradeType;
    public float upgradeAmount = 10f;

    public override void ApplyUpgrade(GameObject player)
    {
        PlayerCharacter playerCharacter = player.GetComponent<PlayerCharacter>();
        TimeManipulator timeManipulator = player.GetComponent<TimeManipulator>();

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
                var fpController = playerCharacter.fpController;
                if (fpController != null)
                {
                    // access movement speed

                }
                break;

            case UpgradeType.TimeSlowCooldown:
                // Decrease time slow cooldown
                if (timeManipulator != null)
                {
                    timeManipulator.UpgradeCooldownReduction(upgradeAmount);
                }
                break;

            case UpgradeType.TimeSlowDuration:
                // Increase Time slow duration
                if (timeManipulator != null)
                {
                    timeManipulator.UpgradeSlowdownDuration(upgradeAmount);
                }
                break;
        }

    }
}
