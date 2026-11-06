using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Komponent przypisywany do przycisku mobilnego.
/// Przekazuje wcisniecie (PointerDown) i puszczenie (PointerUp) do MobileUI.
/// </summary>
public class MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum ActionType
    {
        Left,
        Right,
        Jump,
        HighAttack,
        LowAttack
    }

    [Header("Akcja przycisku")]
    public ActionType action;

    [Header("Referencja (jesli puste, znajdzie automatycznie)")]
    public MobileUI mobileUI;

    void Awake()
    {
        EnsureTarget();
    }

    void EnsureTarget()
    {
        if (mobileUI == null)
        {
            mobileUI = GetComponentInParent<MobileUI>();
            if (mobileUI == null)
            {
                mobileUI = FindFirstObjectByType<MobileUI>();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        EnsureTarget();
        if (mobileUI == null) return;

        switch (action)
        {
            case ActionType.Left:
                mobileUI.OnLeftDown();
                break;
            case ActionType.Right:
                mobileUI.OnRightDown();
                break;
            case ActionType.Jump:
                mobileUI.OnJump();
                break;
            case ActionType.HighAttack:
                mobileUI.OnHighAttack();
                break;
            case ActionType.LowAttack:
                mobileUI.OnLowAttack();
                break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        EnsureTarget();
        if (mobileUI == null) return;

        switch (action)
        {
            case ActionType.Left:
                mobileUI.OnLeftUp();
                break;
            case ActionType.Right:
                mobileUI.OnRightUp();
                break;
        }
    }
}

