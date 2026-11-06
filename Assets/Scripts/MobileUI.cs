using UnityEngine;

/// <summary>
/// Mobilne UI - sterowanie Graczem 2 (P2) przez przyciski ekranowe.
/// </summary>
public class MobileUI : MonoBehaviour
{
    [Header("Gracz kontrolowany przez przyciski (P2)")]
    public FighterController player2;

    void Start()
    {
        if (player2 == null)
        {
            var p2Obj = GameObject.Find("P2");
            if (p2Obj != null)
                player2 = p2Obj.GetComponent<FighterController>();
        }
    }

    public void OnLeftDown()
    {
        if (player2 != null) player2.MobilePressLeft(true);
    }

    public void OnLeftUp()
    {
        if (player2 != null) player2.MobilePressLeft(false);
    }

    public void OnRightDown()
    {
        if (player2 != null) player2.MobilePressRight(true);
    }

    public void OnRightUp()
    {
        if (player2 != null) player2.MobilePressRight(false);
    }

    public void OnJump()
    {
        if (player2 != null) player2.MobileJump();
    }

    public void OnHighAttack()
    {
        if (player2 != null) player2.MobileHighAttack();
    }

    public void OnLowAttack()
    {
        if (player2 != null) player2.MobileLowAttack();
    }
}
