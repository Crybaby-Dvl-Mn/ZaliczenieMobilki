using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Menu edytora dla Szachownicy:
/// Tools -> Chess Setup
/// </summary>
public static class ChessBoardSetup
{
    [MenuItem("Tools/Chess Setup/1. Zainicjalizuj Szachownice (Dodaj ChessBoard i ChessTile)")]
    public static void SetupChessboard()
    {
        GameObject boardObj = GameObject.Find("Chessboard");
        if (boardObj == null) boardObj = GameObject.Find("CheesGame");
        if (boardObj == null) boardObj = GameObject.Find("ChessGame");

        if (boardObj == null)
        {
            EditorUtility.DisplayDialog("Blad", "Nie znaleziono obiektu 'Chessboard' ani 'CheesGame' w scenie!", "OK");
            return;
        }

        ChessBoard boardComp = boardObj.GetComponent<ChessBoard>();
        if (boardComp == null)
        {
            boardComp = boardObj.AddComponent<ChessBoard>();
        }

        var cbRoot = GameObject.Find("Chessboard");
        if (cbRoot != null) boardComp.boardRoot = cbRoot.transform;

        var camObj = GameObject.Find("ChessCamera");
        if (camObj != null) boardComp.chessCamera = camObj.GetComponent<Camera>();

        boardComp.InitializeBoard();

        EditorUtility.SetDirty(boardObj);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Sukces!",
            "Szachownica zostala pomyslnie skonfigurowana!\n" +
            "- Dodano komponent ChessBoard\n" +
            "- Wszystkie pola posiadaja teraz ChessTile i BoxCollider\n" +
            "- Pola reaguja na najechanie myszka i klikniecie w Play Mode",
            "OK");
    }

    [MenuItem("Tools/Chess Setup/2. Uzupelnij rzedy do 8x8 (Jesli masz tylko Row1 i Row2)")]
    public static void CompleteRows()
    {
        GameObject boardObj = GameObject.Find("Chessboard");
        if (boardObj == null) boardObj = GameObject.Find("CheesGame");

        if (boardObj == null)
        {
            EditorUtility.DisplayDialog("Blad", "Nie znaleziono obiektu 'Chessboard'!", "OK");
            return;
        }

        ChessBoard boardComp = boardObj.GetComponent<ChessBoard>();
        if (boardComp == null) boardComp = boardObj.AddComponent<ChessBoard>();

        boardComp.CompleteMissingRows();

        EditorUtility.SetDirty(boardObj);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Gotowe!",
            "Utworzono brakujace rzedy szachownicy az do Row8!\n" +
            "Pola zostaly zindeksowane od a1 do h8.",
            "OK");
    }
}
