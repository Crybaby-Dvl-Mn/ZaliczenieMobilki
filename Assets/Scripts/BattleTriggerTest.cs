using UnityEngine;

public class BattleTestTrigger : MonoBehaviour
{
    public FighterController attacker;
    public FighterController defender;
    public KeyCode startKey = KeyCode.B;

    void Update()
    {
        if (Input.GetKeyDown(startKey))
        {
            if (BattleManager.Instance == null)
            {
                Debug.LogError("Brak BattleManager w scenie!");
                return;
            }
            if (BattleManager.Instance.IsBattleActive)
            {
                Debug.Log("Walka ju¿ trwa.");
                return;
            }

            BattleManager.Instance.StartBattle(attacker, defender, attackerWon =>
            {
                Debug.Log($"[TEST] Walka zakoñczona. Atakuj¹cy wygra³: {attackerWon}");
            });
        }
    }
}