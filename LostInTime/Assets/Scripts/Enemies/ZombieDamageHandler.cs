using UnityEngine;
using AdvancedShooterKit;

public class ZombieDamageHandler : DamageHandler
{
    public ZombieStateMachine zombie;

    void Awake()
    {
        if (zombie == null)
            zombie = GetComponentInParent<ZombieStateMachine>();
    }

    public override bool isAlive => zombie != null && zombie.CurrentHealth > 0;

    public override bool isNPC => true;

    public override void TakeDamage(DamageInfo damageInfo)
    {
        base.TakeDamage(damageInfo);

        bool hasValidLastDamage = lastDamage.owner != null;

        // 🛠️ Because we can't access damageInfo.damage or .direction directly,
        // We extract what we can (based on DamageHandler implementation),
        // or use lastDamage as a fallback:
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

            // ✅ Create DamageInfo using the SDK-compatible method
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

        Destroy(gameObject);
    }
}
