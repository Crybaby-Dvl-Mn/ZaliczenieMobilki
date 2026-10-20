using UnityEngine;

public enum AttackType { High, Low } // High = rêka, Low = kopniêcie

[CreateAssetMenu(fileName = "NewAttack", menuName = "Fighting/Attack Data")]
public class AttackData : ScriptableObject
{
    [Header("Typ ataku")]
    public AttackType attackType = AttackType.High;
    public HitLevel hitLevel = HitLevel.Light;

    [Header("Obra¿enia i odepchniêcie")]
    [Tooltip("Jeœli true, u¿ywa domyœlnych wartoœci z HitLevelDefaults. Jeœli false — u¿ywa pól poni¿ej.")]
    public bool useDefaultsFromHitLevel = true;

    public int damage = 5;
    public float knockbackForce = 3f;
    public float hitstunDuration = 0.15f;

    [Header("Hitbox")]
    [Tooltip("Przesuniêcie hitboxa wzglêdem postaci. X dodatni = w stronê patrzenia.")]
    public Vector2 hitboxOffset = new Vector2(0.8f, 0f);
    public Vector2 hitboxSize = new Vector2(0.8f, 0.6f);

    [Header("Timing")]
    [Tooltip("Po jakim czasie od startu ataku hitbox siê aktywuje.")]
    public float startupTime = 0.05f;
    [Tooltip("Jak d³ugo hitbox jest aktywny.")]
    public float activeTime = 0.1f;
    [Tooltip("Po ataku — cooldown, zanim mo¿na zaatakowaæ ponownie.")]
    public float recoveryTime = 0.2f;

    // Wyliczone wartoœci — u¿ywane przez AttackExecutor
    public int GetDamage() =>
        useDefaultsFromHitLevel ? HitLevelDefaults.GetDamage(hitLevel) : damage;

    public float GetKnockbackForce() =>
        useDefaultsFromHitLevel ? HitLevelDefaults.GetKnockbackForce(hitLevel) : knockbackForce;

    public float GetHitstunDuration() =>
        useDefaultsFromHitLevel ? HitLevelDefaults.GetHitstunDuration(hitLevel) : hitstunDuration;
}