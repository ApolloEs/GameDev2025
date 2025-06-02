using UnityEngine;
using AdvancedShooterKit;

public interface ITimeEffect
{
    Color GetZoneColor();
    void ApplyToRigidbody(Rigidbody rb);
    void ApplyToCharacter(Character character);
    void RemoveFromRigidbody(Rigidbody rb);
    void RemoveFromCharacter(Character character);
}
