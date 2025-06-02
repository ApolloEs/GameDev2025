using UnityEngine;
using System.Collections.Generic;
using AdvancedShooterKit;

public class SlowTimeEffect : MonoBehaviour, ITimeEffect
{
    [SerializeField] private float timeScale = 0.3f;
    [SerializeField] private LayerMask affectedLayers;
    [SerializeField] private Color color;

    private Dictionary<Rigidbody, float> originalDrags = new Dictionary<Rigidbody, float>();
    private Dictionary<Character, float> originalMoveSpeeds = new Dictionary<Character, float>();

    public Color GetZoneColor()
    {
        return new Color(0.5f, 0.8f, 1f, 0.3f);
    }

    public void ApplyToRigidBody(Rigidbody rb)
    {
        // Skip if this object shouldnt be affected
        if (!ShouldAffectObject(rb.gameObject))
            return;

        // Store origina values
        if (!originalDrags.ContainsKey(rb))
            originalDrags[rb] = rb.linearDamping;

        // Increase drag to simulate slower movement
        rb.linearDamping = originalDrags[rb] * (1 / timeScale);
    }

    public void ApplyToCharacter(Character character)
    {
        // skip if this shouldnt be affected
        if (!ShouldAffectObject(character.gameObject))
            return;

        // Store original speed values
        if (!originalMoveSpeeds.ContainsKey(character))
        {
            // For now use a placeholder value - this will be updated when we know more about enemy movement
            originalMoveSpeeds[character] = 1.0f;
        }
        // Apply slowing effect - Await for enemy to be implemented
    }
    public void RemoveFromRigidbody(Rigidbody rb)
    {
        if (originalDrags.ContainsKey(rb))
        {
            rb.linearDamping = originalDrags[rb];
            originalDrags.Remove(rb);
        }
    }

    public void RemoveFromCharacter(Character character)
    {
        if (originalMoveSpeeds.ContainsKey(character))
        {
            // Restore original speed
            // Will need implmentation based on enemy movement
            originalMoveSpeeds.Remove(character);
        }
    }

    private bool ShouldAffectObject(GameObject obj)
    {
        // Check if this object is on an affected layer
        return (affectedLayers.value & (1 << obj.layer)) != 0;
    }
}
