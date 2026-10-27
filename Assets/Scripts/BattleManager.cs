using System;
using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("Walka")]
    public FighterController player1;
    public FighterController player2;
    public Transform spawnPoint1;
    public Transform spawnPoint2;

    [Header("Dogrywka")]
    [Tooltip("Okno czasowe, w którym dwa KO s¹ traktowane jako jednoczesne.")]
    public float drawWindow = 0.5f;
    public float overtimeDelay = 1f;

    [Header("Kamery")]
    public Camera chessCamera;
    public Camera battleCamera;

    [Header("Debug")]
    public bool logEvents = true;

    private float? _firstKoTime;
    private FighterController _firstKoLoser;
    private bool _battleEnded;
    private bool _battleActive;
    private FighterController _attacker;
    private FighterController _defender;
    private Action<bool> _onBattleFinished;

    public bool IsBattleActive => _battleActive;

    void Awake()
    {
        Instance = this;
        if (battleCamera != null) battleCamera.enabled = false;
    }

    /// <summary>
    /// Startuje walkê. onFinished(true) = atakuj¹cy wygra³, onFinished(false) = atakuj¹cy przegra³.
    /// </summary>
    public void StartBattle(
        FighterController attackerFighter,
        FighterController defenderFighter,
        Action<bool> onFinished)
    {
        if (_battleActive)
        {
            Debug.LogWarning("[BattleManager] Walka ju¿ trwa — ignorujê StartBattle.");
            return;
        }

        _attacker = attackerFighter;
        _defender = defenderFighter;
        _onBattleFinished = onFinished;

        _battleEnded = false;
        _battleActive = true;
        _firstKoTime = null;
        _firstKoLoser = null;

        _attacker.opponent = _defender;
        _defender.opponent = _attacker;

        _attacker.Health.OnDeath += HandleDeath;
        _defender.Health.OnDeath += HandleDeath;

        _attacker.ResetForBattle(spawnPoint1 != null ? spawnPoint1.position : _attacker.transform.position);
        _defender.ResetForBattle(spawnPoint2 != null ? spawnPoint2.position : _defender.transform.position);

        if (chessCamera != null) chessCamera.enabled = false;
        if (battleCamera != null) battleCamera.enabled = true;

        if (logEvents) Debug.Log("[BattleManager] Walka rozpoczêta.");
    }

    void HandleDeath(Health deadHealth)
    {
        if (!_battleActive || _battleEnded) return;

        FighterController loser =
            deadHealth == _attacker.Health ? _attacker :
            deadHealth == _defender.Health ? _defender : null;
        if (loser == null) return;

        if (_firstKoTime == null)
        {
            _firstKoTime = Time.time;
            _firstKoLoser = loser;
            loser.DisableInput();
            if (logEvents) Debug.Log($"[BattleManager] Pierwsze KO: {loser.name}");
        }
        else
        {
            if (Time.time - _firstKoTime.Value <= drawWindow)
            {
                if (logEvents) Debug.Log("[BattleManager] Drugie KO w oknie — DOGRYWKA.");
                StartCoroutine(Overtime());
            }
            else
            {
                FighterController winner = (_firstKoLoser == _attacker) ? _defender : _attacker;
                EndBattle(winner);
            }
        }
    }

    void Update()
    {
        if (!_battleActive || _battleEnded || _firstKoTime == null) return;

        if (Time.time - _firstKoTime.Value > drawWindow)
        {
            FighterController winner = (_firstKoLoser == _attacker) ? _defender : _attacker;
            EndBattle(winner);
        }
    }

    IEnumerator Overtime()
    {
        yield return new WaitForSeconds(overtimeDelay);

        _firstKoTime = null;
        _firstKoLoser = null;

        _attacker.ResetForBattle(spawnPoint1 != null ? spawnPoint1.position : _attacker.transform.position);
        _defender.ResetForBattle(spawnPoint2 != null ? spawnPoint2.position : _defender.transform.position);

        if (logEvents) Debug.Log("[BattleManager] Dogrywka — obaj odrodzeni.");
    }

    void EndBattle(FighterController winner)
    {
        _battleEnded = true;
        _battleActive = false;

        _attacker.Health.OnDeath -= HandleDeath;
        _defender.Health.OnDeath -= HandleDeath;

        _attacker.DisableInput();
        _defender.DisableInput();

        bool attackerWon = (winner == _attacker);

        if (battleCamera != null) battleCamera.enabled = false;
        if (chessCamera != null) chessCamera.enabled = true;

        if (logEvents) Debug.Log($"[BattleManager] Koniec walki. Atakuj¹cy wygra³: {attackerWon}");

        var callback = _onBattleFinished;
        _onBattleFinished = null;
        callback?.Invoke(attackerWon);
    }
}