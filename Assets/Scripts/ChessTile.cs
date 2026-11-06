using UnityEngine;
using System;

/// <summary>
/// Reprezentuje pojedyncze pole na szachownicy (np. a1, e4, h8).
/// Obsluguje wspolrzedne, podswietlanie (hover, zaznaczenie, dozwolony ruch, atak)
/// oraz klikniecia mysza/dotykiem.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ChessTile : MonoBehaviour
{
    public enum HighlightState
    {
        None,
        Hover,
        Selected,
        ValidMove,
        Attack
    }

    [Header("Wspolrzedne pola")]
    [Tooltip("Indeks rzedu (0 = Top / Row1 / Rank 8, 7 = Bottom / Row8 / Rank 1)")]
    public int row;
    [Tooltip("Indeks kolumny (0 = Kolumna 1 / a, 7 = Kolumna 8 / h)")]
    public int col;

    [Header("Notacja szachowa")]
    [Tooltip("Np. a8, b8, e4, h1")]
    public string coordinateNotation;

    [Header("Stan")]
    public bool isLightSquare;
    public GameObject currentPiece;
    public bool IsOccupied => currentPiece != null;

    [Header("Kolory podswietlenia")]
    public Color hoverColor = new Color(0.6f, 0.9f, 1f, 1f);       // blekitny
    public Color selectedColor = new Color(1f, 0.92f, 0.2f, 1f);   // zolty
    public Color validMoveColor = new Color(0.3f, 1f, 0.4f, 1f);   // zielony
    public Color attackColor = new Color(1f, 0.3f, 0.3f, 1f);      // czerwony

    [Header("Punkt postawienia figury")]
    public Vector3 pieceStandOffset = new Vector3(0f, 0.1f, 0f);

    private Renderer _renderer;
    private Color _originalColor;
    private HighlightState _currentState = HighlightState.None;
    private bool _hasOriginalColor = false;

    public HighlightState CurrentState => _currentState;

    void Awake()
    {
        InitializeRenderer();
    }

    public void Initialize(int r, int c)
    {
        row = r;
        col = c;
        char fileChar = (char)('a' + col);
        int rankNum = 8 - row;
        coordinateNotation = $"{fileChar}{rankNum}";
        isLightSquare = ((row + col) % 2 == 0);

        InitializeRenderer();
    }

    void InitializeRenderer()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null && !_hasOriginalColor)
            {
                _originalColor = _renderer.material.color;
                _hasOriginalColor = true;
            }
        }
    }

    /// <summary>
    /// Zwraca pozycje w swiecie, na ktorej powinna stac figura na tym polu.
    /// </summary>
    public Vector3 GetPieceStandPosition()
    {
        Collider colComp = GetComponent<Collider>();
        if (colComp != null)
        {
            // Bierzemy gore collidera
            Vector3 center = colComp.bounds.center;
            return center + new Vector3(0f, colComp.bounds.extents.y + pieceStandOffset.y, 0f);
        }
        return transform.position + pieceStandOffset;
    }

    /// <summary>
    /// Ustawia stan podswietlenia tego pola.
    /// </summary>
    public void SetHighlight(HighlightState state)
    {
        _currentState = state;
        InitializeRenderer();

        if (_renderer == null || !_hasOriginalColor) return;

        switch (state)
        {
            case HighlightState.None:
                _renderer.material.color = _originalColor;
                break;
            case HighlightState.Hover:
                _renderer.material.color = Color.Lerp(_originalColor, hoverColor, 0.65f);
                break;
            case HighlightState.Selected:
                _renderer.material.color = Color.Lerp(_originalColor, selectedColor, 0.75f);
                break;
            case HighlightState.ValidMove:
                _renderer.material.color = Color.Lerp(_originalColor, validMoveColor, 0.75f);
                break;
            case HighlightState.Attack:
                _renderer.material.color = Color.Lerp(_originalColor, attackColor, 0.8f);
                break;
        }
    }

    // --- Obsluga zdarzen myszy ---
    void OnMouseEnter()
    {
        if (ChessBoard.Instance != null)
        {
            ChessBoard.Instance.OnTileHoverEnter(this);
        }
        else if (_currentState == HighlightState.None)
        {
            SetHighlight(HighlightState.Hover);
        }
    }

    void OnMouseExit()
    {
        if (ChessBoard.Instance != null)
        {
            ChessBoard.Instance.OnTileHoverExit(this);
        }
        else if (_currentState == HighlightState.Hover)
        {
            SetHighlight(HighlightState.None);
        }
    }

    void OnMouseDown()
    {
        HandleClick();
    }

    public void HandleClick()
    {
        if (ChessBoard.Instance != null)
        {
            ChessBoard.Instance.OnTileClicked(this);
        }
        else
        {
            Debug.Log($"[ChessTile] Kliknieto pole: {coordinateNotation} (Row: {row + 1}, Col: {col + 1})");
            SetHighlight(_currentState == HighlightState.Selected ? HighlightState.None : HighlightState.Selected);
        }
    }

    void OnDestroy()
    {
        // Przywracamy kolor aby nie zostal na materiale w edytorze
        if (_renderer != null && _hasOriginalColor)
        {
            _renderer.material.color = _originalColor;
        }
    }
}

