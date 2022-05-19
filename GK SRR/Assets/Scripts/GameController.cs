using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { Exploring, Battle }
public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    GameState state;

    private void Start()
    {
        playerController.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;
    }

    void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<KieszpotParty>();
        var wildKieszpot = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildKieszpot();

        battleSystem.StartBattle(playerParty, wildKieszpot);
    }

    void EndBattle(bool playerWon)
    {
        state = GameState.Exploring;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (state == GameState.Exploring) playerController.HandleUpdate();
        else if (state == GameState.Battle) battleSystem.HandleUpdate();
    }
}
