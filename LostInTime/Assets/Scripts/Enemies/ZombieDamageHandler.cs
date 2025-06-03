using UnityEngine;
using AdvancedShooterKit;

public class ZombieDamageHandler : DamageHandler
{
    public ZombieStateMachine zombie;
    public GameObject bloodEffectPrefab;

    void Awake()
    {
        if (zombie == null)
            zombie = GetComponentInParent<ZombieStateMachine>();
    }

    public override bool isAlive => zombie != null && zombie.CurrentHealth > 0;

    public override bool isNPC => true;

    public override void TakeDamage(DamageInfo damageInfo)
    {

        if (damageInfo.source != null && bloodEffectPrefab != null)
        {
            // Determine hit position
            Vector3 hitPos = transform.position + Vector3.up; // Or use actual hit position if available

            // Instantiate blood effect
            GameObject bloodFX = Instantiate(bloodEffectPrefab, hitPos, Quaternion.identity);

            // Make the blood stick to the zombie
            bloodFX.transform.SetParent(this.transform);

            // Optionally rotate to face attacker
            Vector3 dirToAttacker = (damageInfo.source.position - transform.position).normalized;
            bloodFX.transform.rotation = Quaternion.LookRotation(dirToAttacker);
        }
        
        base.TakeDamage(damageInfo);

        bool hasValidLastDamage = lastDamage.owner != null;

        
        float amount = hasValidLastDamage ? GetDamageFromLastDamage() : 10f;
        Vector3 direction = hasValidLastDamage ? GetDirectionFromLastDamage() : Vector3.back;

        if (zombie != null)
            zombie.TakeDamage(Mathf.RoundToInt(amount), direction);
    }

    // Helper methods assuming we can't directly access damageInfo.damage
    private float GetDamageFromLastDamage()
    {
        return lastDamage.GetType().GetProperty("damage")?.GetValue(lastDamage) as float? ?? 10f;
    }

    private Vector3 GetDirectionFromLastDamage()
    {
        return lastDamage.GetType().GetProperty("direction")?.GetValue(lastDamage) as Vector3? ?? Vector3.back;
    }

    void OnCollisionEnter(Collision hitInfo)
    {
        var handler = hitInfo.collider.GetComponent<ZombieDamageHandler>();
        if (handler != null)
        {
            Vector3 hitDirection = (hitInfo.transform.position - transform.position).normalized;
            Vector3 hitPoint = hitInfo.contacts[0].point;

            
            DamageInfo dmgInfo = new DamageInfo();
            typeof(DamageInfo).GetProperty("owner")?.SetValue(dmgInfo, this);
            typeof(DamageInfo).GetProperty("damage")?.SetValue(dmgInfo, 10f);
            typeof(DamageInfo).GetProperty("direction")?.SetValue(dmgInfo, hitDirection);
            typeof(DamageInfo).GetProperty("hitPoint")?.SetValue(dmgInfo, hitPoint);

            handler.TakeDamage(dmgInfo);
            Debug.Log("Zombie damage applied!");
        }
        else
        {
            Debug.Log("No ZombieDamageHandler found on: " + hitInfo.collider.name);
        }

        //Destroy(gameObject);
    }
}
