using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AdvancedShooterKit;

public class TimeEffectZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [SerializeField] private float radius = 5f;
    [SerializeField] private float timeScale = 0.4f;
    [SerializeField] private float duration = 10f;
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float fadeOutTime = 0.5f;
    [SerializeField] private float checkInterval = 0.2f; // How often to check for zombies in update function
    [SerializeField] private SlowTimeEffect timeEffect;

    [Header("Visualization")]
    [SerializeField] private Material zoneMaterial;
    [SerializeField] private Color zoneColor = new Color(0.5f, 0.8f, 1f, 0.2f);

    [Header("Audio")]
    [SerializeField] private AudioClip startSound;
    [SerializeField] private AudioClip loopSound;
    [SerializeField] private AudioClip endSound;

    [Header("Zone")]
    [SerializeField] private GameObject timeZonePrefab;
    private static bool hasSpawned = false;

    //Components
    private SphereCollider zoneCollider;
    private MeshRenderer zoneMeshRenderer;
    private AudioSource audioSource;

    // Runtime tracking
    private float nextCheckTime = 0f;
    private ITimeEffect currentEffect;
    private float timeRemaining;
    private List<Rigidbody> affectedRigidbodies = new List<Rigidbody>();
    private List<Character> affectedCharacters = new List<Character>();
    private List<TimeManipulator> affectedTimeManipulators = new List<TimeManipulator>();
    private List<ZombieStateMachine> affectedZombies = new List<ZombieStateMachine>();
    private float timer;

    private void Awake()
    {
        // Setup components
        zoneCollider = GetComponent<SphereCollider>();
        if (zoneCollider == null)
        {
            zoneCollider = gameObject.AddComponent<SphereCollider>();
            zoneCollider.radius = radius;
            zoneCollider.isTrigger = true;
        }

        // Find visual representation
        zoneMeshRenderer = GetComponentInChildren<MeshRenderer>();

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1f; // 3D sound
        // audioSource.rolloffMode = AudioRollofMode.Linear;
        audioSource.maxDistance = radius * 3f;

        // Get effect component
        currentEffect = GetComponent<SlowTimeEffect>();
        if (currentEffect == null)
            currentEffect = gameObject.AddComponent<SlowTimeEffect>();

        timeRemaining = duration;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Apply visual customization based on effect
        if (zoneMeshRenderer != null && zoneMaterial != null)
        {
            zoneMeshRenderer.material = zoneMaterial;
            zoneMeshRenderer.material.color = currentEffect.GetZoneColor();
        }

        // Get the effect component if not assigned
        if (timeEffect == null)
        {
            timeEffect = GetComponent<SlowTimeEffect>();
            if (timeEffect == null)
            {
                timeEffect = gameObject.AddComponent<SlowTimeEffect>();
                //timeEffect.Initialize(timeScale);
            }
            timer = duration;
        }

        // Start with zero scale and fade in
        transform.localScale = Vector3.zero;

        // Play start sound
        if (startSound != null)
            audioSource.PlayOneShot(startSound);

        // Play loop sound
        if (loopSound != null)
        {
            audioSource.clip = loopSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        if (!hasSpawned)
        {
            hasSpawned = true;
            if (timeZonePrefab != null)
            {
                Instantiate(timeZonePrefab, transform.position, Quaternion.identity);
            }
        }
        // Reset flag after short delay
        Invoke("ResetHasSpawned", 0.5f);

        // Start fade in
        StartCoroutine(FadeIn());
    }

    private void ResetHasSpawned()
    {
        hasSpawned = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Handle zone lifetime
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            // Apply effect to all objects in zone
            foreach (Rigidbody rb in affectedRigidbodies)
            {
                if (rb != null)
                    currentEffect.ApplyToRigidbody(rb);
            }

            foreach (Character character in affectedCharacters)
            {
                if (character != null)
                    currentEffect.ApplyToCharacter(character);
            }


            // Start fade out when close to end
            if (timeRemaining <= fadeOutTime && transform.localScale.x > 0)
            {
                StartCoroutine(FadeOut());
            }
        }
        else if (gameObject.activeSelf)
        {
            // Clean up
            RemoveAllEffects();
            Destroy(gameObject, 1f);
        }

        if (Time.time > nextCheckTime)
        {
            nextCheckTime = Time.time + checkInterval;
            CheckForZombiesInRange();
        }
    }

    private void CheckForZombiesInRange()
    {
        // Get the actual radius
        float radius = zoneCollider.radius * transform.lossyScale.x;

        // Debug sphere
        Debug.DrawLine(transform.position, transform.position + Vector3.up * radius, Color.cyan, checkInterval);
        Debug.DrawLine(transform.position, transform.position + Vector3.right * radius, Color.cyan, checkInterval);

        // Find all colliders in range
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        // Track currently detected zombies
        List<ZombieStateMachine> currentZombies = new List<ZombieStateMachine>();

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Zombie"))
            {
                ZombieStateMachine zombie = col.GetComponent<ZombieStateMachine>();
                if (zombie != null)
                {
                    currentZombies.Add(zombie);

                    if (!affectedZombies.Contains(zombie))
                    {
                        zombie.ApplyTimeEffect(timeScale);
                        affectedZombies.Add(zombie);
                    }
                }
            }
        }

        // Remove effects from zombies no longer in range
        for (int i = affectedZombies.Count - 1; i >= 0; i--)
        {
            ZombieStateMachine zombie = affectedZombies[i];
            if (zombie == null || !currentZombies.Contains(zombie))
            {
                if (zombie != null)
                {
                    Debug.Log("Removing effect from zombie");
                    zombie.RemoveTimeEffect();
                }
                affectedZombies.RemoveAt(i);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        Debug.Log($"[TimeEffectZone] Trigger Enter: {other.name}, Tag: {other.tag}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}");
        // Skip projectiles
        if (other.CompareTag("Projectile"))
            return;


        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered the zone");

            // Try to find TimeManipulator component
            TimeManipulator timeManipulator = other.GetComponent<TimeManipulator>();
            if (timeManipulator != null)
            {
                affectedTimeManipulators.Add(timeManipulator);
                timeManipulator.SlowDownTime();
            }
        }

        // Handle enemies/zombies
        if (other.CompareTag("Zombie")) // Change this to an actual enemy tag when its time to add more enemies
        {
            Debug.Log($"Enemy: {other.name} entered a time zone");

            // Try to find ZombieStateMachine component
            ZombieStateMachine zombie = other.GetComponent<ZombieStateMachine>();
            if (zombie != null)
            {
                affectedZombies.Add(zombie);
                zombie.ApplyTimeEffect(timeScale);
                Debug.Log("Slowed Zombie Time");
            }
        }


        // Add to tracked objects
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && !affectedRigidbodies.Contains(rb))
        {
            affectedRigidbodies.Add(rb);
            currentEffect.ApplyToRigidbody(rb);
        }

        Character character = other.GetComponent<Character>();
        if (character != null && !affectedCharacters.Contains(character))
        {
            affectedCharacters.Add(character);
            currentEffect.ApplyToCharacter(character);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Zombie"))
        {
            ZombieStateMachine zombie = other.GetComponent<ZombieStateMachine>();
            if (zombie != null)
            {
                Debug.Log("[TimeEffectZone] Detected zombie in triggerstay");
                affectedZombies.Add(zombie);
                zombie.ApplyTimeEffect(timeScale);
                Debug.Log("Slowed Zombie Time");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Remove effects when leaving zone
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited time zone!");

            // Reset time manipulator
            TimeManipulator timeManipulator = other.GetComponent<TimeManipulator>();
            if (timeManipulator != null)
            {
                affectedTimeManipulators.Remove(timeManipulator);
                timeManipulator.ResetTimeScale();
            }
        }

        // Handle enemies/zombies
        if (other.CompareTag("NonPlayerCharacter")) // Change this to an actual enemy tag when its time to add more enemies
        {
            Debug.Log($"Enemy: {other.name} entered a time zone");

            // Try to find ZombieStateMachine component
            ZombieStateMachine zombie = other.GetComponent<ZombieStateMachine>();
            if (zombie != null)
            {
                affectedZombies.Add(zombie);
                zombie.RemoveTimeEffect();
            }
        }

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && affectedRigidbodies.Contains(rb))
        {
            affectedRigidbodies.Remove(rb);
            currentEffect.RemoveFromRigidbody(rb);
        }

        Character character = other.GetComponent<Character>();
        if (character != null && affectedCharacters.Contains(character))
        {
            affectedCharacters.Remove(character);
            currentEffect.RemoveFromCharacter(character);
        }
    }
    

    private void RemoveAllEffects()
    {
        // Stop audio
        audioSource.Stop();

        // Play end sound
        if (endSound != null)
            audioSource.PlayOneShot(endSound);

        foreach (Rigidbody rb in affectedRigidbodies)
        {
            if (rb != null)
                currentEffect.RemoveFromRigidbody(rb);
        }

        foreach (Character character in affectedCharacters)
        {
            if (character != null)
                currentEffect.RemoveFromCharacter(character);
        }

        foreach (TimeManipulator tm in affectedTimeManipulators)
        {
            if (tm != null)
            {
                tm.ResetTimeScale();
            }
        }

        foreach (ZombieStateMachine zombie in affectedZombies)
        {
            if (zombie != null)
            {
                zombie.RemoveTimeEffect();
            }
        }


        affectedZombies.Clear();
        affectedRigidbodies.Clear();
        affectedCharacters.Clear();
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeInTime)
        {
            float t = elapsed / fadeInTime;
            transform.localScale = Vector3.one * t * 8;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.one * 8;
    }

    private IEnumerator FadeOut()
    {
        float startScale = transform.localScale.x;
        float elapsed = 0f;

        while (elapsed < fadeOutTime)
        {
            float t = 1f - (elapsed / fadeOutTime);
            transform.localScale = Vector3.one * t * startScale;
            yield return null;
        }
    }
}
