using UnityEngine;

/// <summary>
/// Mobilne UI – sterowanie Graczem 2 (P2) przez przyciski na ekranie.
/// Dodaj ten komponent do obiektu Canvas w scenie.
/// Przypisz pole player2 w Inspectorze (obiekt P2 z komponentem FighterController).
/// 
/// Każdy przycisk ma Event Trigger:
///   Pointer Down  → wywołaj odpowiednią metodę OnXXXDown()
///   Pointer Up    → wywołaj odpowiednią metodę OnXXXUp()
///
/// Wyjątki: Skok, HighAttack, LowAttack mają tylko PointerDown.
/// </summary>
public class MobileUI : MonoBehaviour
{
    [Header("Gracz kontrolowany przez przyciski (P2)")]
    public FighterController player2;

    // ──────────────────────────────────────────────
    // RUCH LEWO
    // ──────────────────────────────────────────────
    public void OnLeftDown()
    {
        if (player2 != null) player2.MobilePressLeft(true);
    }

    public void OnLeftUp()
    {
        if (player2 != null) player2.MobilePressLeft(false);
    }

    // ──────────────────────────────────────────────
    // RUCH PRAWO
    // ──────────────────────────────────────────────
    public void OnRightDown()
    {
        if (player2 != null) player2.MobilePressRight(true);
    }

    public void OnRightUp()
    {
        if (player2 != null) player2.MobilePressRight(false);
    }

    // ──────────────────────────────────────────────
    // SKOK (tylko PointerDown)
    // ──────────────────────────────────────────────
    public void OnJump()
    {
        if (player2 != null) player2.MobileJump();
    }

    // ──────────────────────────────────────────────
    // ATAKI (tylko PointerDown)
    // ──────────────────────────────────────────────
    public void OnHighAttack()
    {
        if (player2 != null) player2.MobileHighAttack();
    }

    public void OnLowAttack()
    {
        if (player2 != null) player2.MobileLowAttack();
    }
}

