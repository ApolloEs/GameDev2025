using UnityEngine;
using AdvancedShooterKit;

public interface ITimeEffect
{
    Color GetZoneColor();
    void ApplyToRigidBody(Rigidbody rb);
    void ApplyToCharacter(Character character);
    void RemoveFromRigidbody(Rigidbody rb);
    void RemoveFromCharacter(Character character);
}
