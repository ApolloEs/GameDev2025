using UnityEngine;
using AdvancedShooterKit;

public class TimeGrenade : MonoBehaviour
{
    [SerializeField] private TimeEffectZone timeZonePrefab;
    private Explosion explosionComponent;

    private void Awake()
    {
        explosionComponent = GetComponent<Explosion>();

        // Disable to prevent normal explosion
        if (explosionComponent != null)
        {
            explosionComponent.enabled = false;
        }
    }

    private void OnEnable()
    {
        // Spawn time effect zone
        if (timeZonePrefab != null)
        {
            Instantiate(timeZonePrefab, transform.position, Quaternion.identity);
        }
    }
}
