using UnityEngine;
using AdvancedShooterKit;

public class ZombieDamageHandler : DamageHandler
{
    public ZombieStateMachine zombie;
    public GameObject bloodEffectPrefab;
    public Transform[] bloodSpawnPoints;
    public GameObject headshotExplosionPrefab;

    void Awake()
    {
        if (zombie == null)
            zombie = GetComponentInParent<ZombieStateMachine>();
    }

    public override bool isAlive => zombie != null && zombie.CurrentHealth > 0;

    public override bool isNPC => true;

    public override void TakeDamage(DamageInfo damageInfo)
    {
        if (bloodEffectPrefab != null && bloodSpawnPoints.Length > 0)
        {
            Transform randomPoint = bloodSpawnPoints[Random.Range(0, bloodSpawnPoints.Length)];

            GameObject bloodFX = Instantiate(bloodEffectPrefab, randomPoint.position, Quaternion.identity);
            bloodFX.transform.SetParent(randomPoint);


            if (damageInfo.source != null)
            {
                Vector3 dirToAttacker = (damageInfo.source.position - transform.position).normalized;
                bloodFX.transform.rotation = Quaternion.LookRotation(dirToAttacker);
            }
        }

        int amount = Mathf.RoundToInt(damageInfo.value);
        
        Vector3 direction = (damageInfo.source != null)
            ? (transform.position - damageInfo.source.position).normalized
            : Vector3.back;

        zombie?.TakeDamage(amount, direction); 
        
        AdvancedShooterKit.HudElements hud = GameObject.FindObjectOfType<AdvancedShooterKit.HudElements>();
        
        if (hud != null)
        {
            hud.ShowDamegeIndicator();
        }
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
