using UnityEngine;
using AdvancedShooterKit;

public class ZombieAttack : MonoBehaviour
{
    public float damageAmount = 15f;
    public float attackRange = 2f;

    // Call this from the animation event!
    public void DealDamage()
    {
        // Find the player in range (you can optimize with a collider or distance check)
        PlayerCharacter player = FindObjectOfType<PlayerCharacter>();
        if (player != null)
        {
            DamageInfo dmgInfo = new DamageInfo(
                damageAmount,
                transform,
                null,
                EDamageType.Melee
            );

            player.TakeDamage(dmgInfo);
            Debug.Log("Zombie attack animation dealt damage!");
        }
        else
        {
            Debug.Log("Player not found for damage.");
        }
    }
}
