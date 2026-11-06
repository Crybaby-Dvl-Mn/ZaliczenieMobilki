using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Glowny menedzer szachownicy.
/// Automatycznie odnajduje rzedy (Row1(TopUpRow), Row2...) i kafelki (Chessboard (1) do (8)),
/// konfiguruje kafelki ChessTile w siatke 8x8 oraz zarzadza interakcja (zaznaczanie, klikanie, ruchy).
/// </summary>
public class ChessBoard : MonoBehaviour
{
    public static ChessBoard Instance;

    [Header("Referencje")]
    [Tooltip("Obiekt szachownicy zawierajacy rzedy (jesli puste, uzyje obiektu Chessboard lub tego transformu)")]
    public Transform boardRoot;
    public Camera chessCamera;

    [Header("Siatka kafelkow (8x8)")]
    [SerializeField] private ChessTile[] flatTiles = new ChessTile[64]; // do podgladu w inspektorze
    private ChessTile[,] _grid = new ChessTile[8, 8];

    [Header("Aktualny stan")]
    public ChessTile selectedTile;
    public ChessTile hoveredTile;

    // Zdarzenia do podpiecia logiki figur i ruchow
    public event Action<ChessTile> OnTileSelectedEvent;
    public event Action<ChessTile, ChessTile> OnTileMoveRequestedEvent; // (fromTile, toTile)

    void Awake()
    {
        Instance = this;
        InitializeBoard();
    }

    void Start()
    {
        if (chessCamera == null)
        {
            var camObj = GameObject.Find("ChessCamera");
            if (camObj != null) chessCamera = camObj.GetComponent<Camera>();
            if (chessCamera == null) chessCamera = Camera.main;
        }
    }

    void Update()
    {
        HandleMouseRaycast();
    }

    void HandleMouseRaycast()
    {
        Camera cam = (chessCamera != null && chessCamera.enabled) ? chessCamera : Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
            var tile = hit.collider.GetComponent<ChessTile>();
            if (tile != null)
            {
                if (hoveredTile != tile)
                {
                    if (hoveredTile != null) OnTileHoverExit(hoveredTile);
                    OnTileHoverEnter(tile);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    OnTileClicked(tile);
                }
                return;
            }
        }

        if (hoveredTile != null)
        {
            OnTileHoverExit(hoveredTile);
        }
    }

    /// <summary>
    /// Inicjalizuje szachownice - odnajduje rzedy i pola, przypisuje indeksy (row: 0..7, col: 0..7).
    /// </summary>
    [ContextMenu("Zainicjalizuj szachownice")]
    public void InitializeBoard()
    {
        if (boardRoot == null)
        {
            var cbObj = GameObject.Find("Chessboard");
            boardRoot = (cbObj != null) ? cbObj.transform : transform;
        }

        _grid = new ChessTile[8, 8];

        List<Transform> rows = GetRowsFromRoot(boardRoot);

        if (rows.Count == 0)
        {
            Debug.LogWarning("[ChessBoard] Nie znaleziono zadnych rzedow w " + boardRoot.name);
            return;
        }

        Debug.Log($"[ChessBoard] Znaleziono {rows.Count} rzedow. Konfiguracja pol...");

        for (int r = 0; r < rows.Count && r < 8; r++)
        {
            Transform rowTransform = rows[r];
            List<Transform> tileTransforms = GetTilesFromRow(rowTransform);

            foreach (var tileTr in tileTransforms)
            {
                int colIndex = ParseColumnIndex(tileTr.name);

                if (colIndex >= 0 && colIndex < 8)
                {
                    var tile = tileTr.GetComponent<ChessTile>();
                    if (tile == null)
                    {
                        tile = tileTr.gameObject.AddComponent<ChessTile>();
                    }

                    if (tileTr.GetComponent<Collider>() == null)
                    {
                        tileTr.gameObject.AddComponent<BoxCollider>();
                    }

                    tile.Initialize(r, colIndex);
                    _grid[r, colIndex] = tile;
                    flatTiles[r * 8 + colIndex] = tile;
                }
            }
        }

        Debug.Log("[ChessBoard] Szachownica zostala pomyslnie zainicjalizowana!");
    }

    private List<Transform> GetRowsFromRoot(Transform root)
    {
        List<Transform> list = new List<Transform>();
        Transform topRow = null;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            string n = child.name;

            if (n.StartsWith("Row", StringComparison.OrdinalIgnoreCase))
            {
                if (n.Contains("TopUpRow") && !n.Contains("Row2") && !n.Contains("(1)"))
                {
                    topRow = child;
                }
                else
                {
                    list.Add(child);
                }
            }
        }

        list.Sort((a, b) =>
        {
            int numA = ExtractFirstNumber(a.name);
            int numB = ExtractFirstNumber(b.name);
            if (numA != numB) return numA.CompareTo(numB);
            return b.position.y.CompareTo(a.position.y);
        });

        if (topRow != null)
        {
            list.Insert(0, topRow);
        }

        return list;
    }

    private List<Transform> GetTilesFromRow(Transform rowTransform)
    {
        List<Transform> tiles = new List<Transform>();
        for (int i = 0; i < rowTransform.childCount; i++)
        {
            Transform child = rowTransform.GetChild(i);
            tiles.Add(child);
        }

        tiles.Sort((a, b) =>
        {
            int colA = ParseColumnIndex(a.name);
            int colB = ParseColumnIndex(b.name);
            if (colA != colB) return colA.CompareTo(colB);
            return a.position.x.CompareTo(b.position.x);
        });

        return tiles;
    }

    public static int ParseColumnIndex(string name)
    {
        Match match = Regex.Match(name, @"\((\d+)\)");
        if (match.Success)
        {
            int num = int.Parse(match.Groups[1].Value);
            return Mathf.Clamp(num - 1, 0, 7);
        }
        return 0;
    }

    private static int ExtractFirstNumber(string str)
    {
        Match match = Regex.Match(str, @"\d+");
        if (match.Success)
        {
            return int.Parse(match.Value);
        }
        return 999;
    }

    public void OnTileHoverEnter(ChessTile tile)
    {
        if (tile == null) return;
        hoveredTile = tile;

        if (tile != selectedTile && tile.CurrentState == ChessTile.HighlightState.None)
        {
            tile.SetHighlight(ChessTile.HighlightState.Hover);
        }
    }

    public void OnTileHoverExit(ChessTile tile)
    {
        if (tile == null) return;
        if (hoveredTile == tile) hoveredTile = null;

        if (tile != selectedTile && tile.CurrentState == ChessTile.HighlightState.Hover)
        {
            tile.SetHighlight(ChessTile.HighlightState.None);
        }
    }

    public void OnTileClicked(ChessTile tile)
    {
        if (tile == null) return;

        Debug.Log($"[Szachy] Wybrano pole: {tile.coordinateNotation} (Rzad: {tile.row + 1}, Kolumna: {tile.col + 1}) | Zajete: {tile.IsOccupied}");

        if (selectedTile == tile)
        {
            DeselectCurrentTile();
            return;
        }

        if (selectedTile != null)
        {
            OnTileMoveRequestedEvent?.Invoke(selectedTile, tile);
            DeselectCurrentTile();
            SelectTile(tile);
            return;
        }

        SelectTile(tile);
    }

    public void SelectTile(ChessTile tile)
    {
        DeselectCurrentTile();
        selectedTile = tile;
        if (selectedTile != null)
        {
            selectedTile.SetHighlight(ChessTile.HighlightState.Selected);
            OnTileSelectedEvent?.Invoke(selectedTile);
        }
    }

    public void DeselectCurrentTile()
    {
        if (selectedTile != null)
        {
            selectedTile.SetHighlight(ChessTile.HighlightState.None);
            selectedTile = null;
        }
    }

    public ChessTile GetTile(int row, int col)
    {
        if (row < 0 || row >= 8 || col < 0 || col >= 8) return null;
        return _grid[row, col];
    }

    public ChessTile GetTile(string notation)
    {
        if (string.IsNullOrEmpty(notation) || notation.Length < 2) return null;

        char file = char.ToLower(notation[0]);
        int col = file - 'a';

        if (int.TryParse(notation.Substring(1), out int rank))
        {
            int row = 8 - rank;
            return GetTile(row, col);
        }
        return null;
    }

    public void ClearAllHighlights()
    {
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                if (_grid[r, c] != null)
                    _grid[r, c].SetHighlight(ChessTile.HighlightState.None);
            }
        }
        selectedTile = null;
    }

    [ContextMenu("Uzupelnij brakujace rzedy do 8x8")]
    public void CompleteMissingRows()
    {
        if (boardRoot == null)
        {
            var cbObj = GameObject.Find("Chessboard");
            boardRoot = (cbObj != null) ? cbObj.transform : transform;
        }

        List<Transform> rows = GetRowsFromRoot(boardRoot);
        if (rows.Count < 2)
        {
            Debug.LogWarning("[ChessBoard] Potrzeba co najmniej 2 rzedow (Row1 i Row2), aby wyliczyc odstep.");
            return;
        }

        Vector3 delta = rows[1].position - rows[0].position;
        Debug.Log($"[ChessBoard] Odstep miedzy rzedami: {delta}");

        for (int i = rows.Count; i < 8; i++)
        {
            int rowNum = i + 1;
            GameObject newRow = Instantiate(rows[1].gameObject, boardRoot);
            newRow.name = $"Row{rowNum}";
            newRow.transform.position = rows[0].position + delta * i;
            newRow.transform.rotation = rows[1].rotation;
            newRow.transform.localScale = rows[1].localScale;
            Debug.Log($"[ChessBoard] Utworzono rzad: Row{rowNum}");
        }

        InitializeBoard();
    }
}
