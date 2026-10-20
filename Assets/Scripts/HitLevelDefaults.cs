using UnityEngine;

public enum HitLevel { Light, Medium, Hard }

public static class HitLevelDefaults
{
    // Domyœlne wartoœci — mo¿esz je nadpisaæ per atak w AttackData
    public static int GetDamage(HitLevel level)
    {
        switch (level)
        {
            case HitLevel.Light: return 5;
            case HitLevel.Medium: return 10;
            case HitLevel.Hard: return 15;
            default: return 5;
        }
    }

    public static float GetKnockbackForce(HitLevel level)
    {
        switch (level)
        {
            case HitLevel.Light: return 3f;
            case HitLevel.Medium: return 6f;
            case HitLevel.Hard: return 9f;
            default: return 3f;
        }
    }

    public static float GetHitstunDuration(HitLevel level)
    {
        switch (level)
        {
            case HitLevel.Light: return 0.15f;
            case HitLevel.Medium: return 0.25f;
            case HitLevel.Hard: return 0.4f;
            default: return 0.15f;
        }
    }
}