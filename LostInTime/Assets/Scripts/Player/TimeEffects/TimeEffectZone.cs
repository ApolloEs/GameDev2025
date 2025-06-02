using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AdvancedShooterKit;

public class TimeEffectZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [SerializeField] private float radius = 5f;
    [SerializeField] private float duration = 10f;
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float fadeOutTime = 0.5f;

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
    private ITimeEffect currentEffect;
    private float timeRemaining;
    private List<Rigidbody> affectedRigidbodies = new List<Rigidbody>();
    private List<Character> affectedCharacters = new List<Character>();

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
                    currentEffect.ApplyToRigidBody(rb);
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
    }

    private void OnTriggerEnter(Collider other)
    {
        // Skip projectiles
        if (other.CompareTag("Projectile"))
            return;

        // Add to tracked objects
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && !affectedRigidbodies.Contains(rb))
        {
            affectedRigidbodies.Add(rb);
            currentEffect.ApplyToRigidBody(rb);
        }

        Character character = other.GetComponent<Character>();
        if (character != null && !affectedCharacters.Contains(character))
        {
            affectedCharacters.Add(character);
            currentEffect.ApplyToCharacter(character);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Removeeffects when leaving zone
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
        affectedRigidbodies.Clear();
        affectedCharacters.Clear();
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeInTime)
        {
            float t = elapsed / fadeInTime;
            transform.localScale = Vector3.one * t;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.one;
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
