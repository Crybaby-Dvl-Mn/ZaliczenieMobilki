using System;
using System.Collections;
using UnityEngine;

public class AttackExecutor : MonoBehaviour
{
    // Warstwy, które mog¹ zostaæ trafione — ustaw w edytorze na warstwê postaci
    public LayerMask hittableLayers;

    public void Execute(FighterController owner, AttackData data, Action onFinished)
    {
        StartCoroutine(Run(owner, data, onFinished));
    }

    IEnumerator Run(FighterController owner, AttackData data, Action onFinished)
    {
        // startup
        if (data.startupTime > 0f) yield return new WaitForSeconds(data.startupTime);

        bool hasHit = false;
        float t = 0f;

        // aktywna faza — sprawdzamy hitbox co klatkê, ale trafiamy tylko raz
        while (t < data.activeTime)
        {
            if (!hasHit)
            {
                Vector2 center = GetHitboxCenter(owner, data);
                Collider2D[] hits = Physics2D.OverlapBoxAll(center, data.hitboxSize, 0f, hittableLayers);

                foreach (var col in hits)
                {
                    if (col.gameObject == owner.gameObject) continue; // nie trafiaj siebie

                    var target = col.GetComponent<FighterController>();
                    if (target == null) continue;
                    if (target.Health.IsDead) continue;

                    target.ReceiveHit(data, owner.transform.position);
                    hasHit = true;
                    break;
                }
            }

            t += Time.deltaTime;
            yield return null;
        }

        onFinished?.Invoke();
    }

    Vector2 GetHitboxCenter(FighterController owner, AttackData data)
    {
        float dir = owner.IsFacingRight ? 1f : -1f;
        Vector2 origin = owner.AttackOrigin.position;
        return origin + new Vector2(data.hitboxOffset.x * dir, data.hitboxOffset.y);
    }

    // Rysowanie hitboxa w Scene view — bardzo pomocne przy dostrajaniu
    void OnDrawGizmos()
    {
        var owner = GetComponent<FighterController>();
        if (owner == null) return;
        // rysujemy tylko wtedy, gdy jest przypisany jakiœ atak — ale nie wiemy który.
        // Zostawiamy pomocniczo: rysuj hitbox dla highAttack i lowAttack, jeœli przypisane
        if (owner.highAttack != null) DrawHitbox(owner, owner.highAttack, Color.red);
        if (owner.lowAttack != null) DrawHitbox(owner, owner.lowAttack, Color.blue);
    }

    void DrawHitbox(FighterController owner, AttackData data, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawWireCube(GetHitboxCenter(owner, data), data.hitboxSize);
    }
}