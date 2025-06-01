using UnityEngine;
using System.Collections.Generic;
using AdvancedShooterKit;

public class TimeEffectManager : MonoBehaviour
{
    [Header("Grenade Settings")]
    [SerializeField] private TimeGrenade timeGrenadePrefab;
    [SerializeField] private int maxGrenades = 3;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float cooldownTime = 5f;
    [SerializeField] private KeyCode throwKey = KeyCode.G;

    [Header("References")]
    [SerializeField] private Transform throwPosition;

    private int currentGrenades;
    private float cooldownTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentGrenades = maxGrenades;
    }

    // Update is called once per frame
    void Update()
    {
        // Update cooldown
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // Check for throw input
        if (Input.GetKeyDown(throwKey) && CanThrowGrenade())
        {
            ThrowGrenade();
        }
    }

    private bool CanThrowGrenade()
    {
        return currentGrenades > 0 && cooldownTimer <= 0;
    }

    private void ThrowGrenade()
    {
        // Decreases grenade count
        currentGrenades--;

        cooldownTimer = cooldownTime;

        if (throwPosition == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                throwPosition = mainCamera.transform;
            }
            else
            {
                throwPosition = transform;
            }
        }

        // Instantiate and throw grenade
        TimeGrenade grenade = Instantiate(timeGrenadePrefab, throwPosition.position, throwPosition.rotation);
        Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();

        if (grenadeRb != null)
        {
            grenadeRb.AddForce(throwPosition.forward * throwForce, ForceMode.Impulse);
        }
    }

    public void AddGrenades(int count)
    {
        currentGrenades = Mathf.Min(currentGrenades + count, maxGrenades);
    }
}
