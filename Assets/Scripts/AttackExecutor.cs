using System;
using System.Collections;
using UnityEngine;

public class AttackExecutor : MonoBehaviour
{
    public LayerMask hittableLayers;

    private AttackVisualizer _visualizer;

    void Awake()
    {
        _visualizer = GetComponent<AttackVisualizer>();
    }

    public void Execute(FighterController owner, AttackData data, Action onFinished)
    {
        StartCoroutine(Run(owner, data, onFinished));
    }

    IEnumerator Run(FighterController owner, AttackData data, Action onFinished)
    {
        if (_visualizer != null) _visualizer.PlayStartup(data.startupTime);
        if (data.startupTime > 0f) yield return new WaitForSeconds(data.startupTime);

        bool hasHit = false;
        float elapsed = 0f;

        if (_visualizer != null) _visualizer.ShowHitbox(data, owner);

        while (elapsed < data.activeTime)
        {
            if (!hasHit)
            {
                Vector2 center = GetHitboxCenter(owner, data);
                Collider2D[] hits = Physics2D.OverlapBoxAll(center, data.hitboxSize, 0f, hittableLayers);
                foreach (var col in hits)
                {
                    if (col.gameObject == owner.gameObject) continue;
                    var target = col.GetComponent<FighterController>();
                    if (target == null) continue;
                    if (target.Health.IsDead) continue;
                    target.ReceiveHit(data, (Vector2)owner.transform.position);
                    if (_visualizer != null) _visualizer.PlayHit();
                    hasHit = true;
                    break;
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_visualizer != null) _visualizer.HideHitbox();
        if (onFinished != null) onFinished();
    }

    public Vector2 GetHitboxCenter(FighterController owner, AttackData data)
    {
        float dir = owner.IsFacingRight ? 1f : -1f;
        return (Vector2)owner.AttackOrigin.position + new Vector2(data.hitboxOffset.x * dir, data.hitboxOffset.y);
    }

    void OnDrawGizmos()
    {
        var owner = GetComponent<FighterController>();
        if (owner == null) return;
        if (owner.highAttack != null)
        {
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.4f);
            Gizmos.DrawWireCube(GetHitboxCenter(owner, owner.highAttack), owner.highAttack.hitboxSize);
        }
        if (owner.lowAttack != null)
        {
            Gizmos.color = new Color(0.3f, 0.5f, 1f, 0.4f);
            Gizmos.DrawWireCube(GetHitboxCenter(owner, owner.lowAttack), owner.lowAttack.hitboxSize);
        }
    }
}
