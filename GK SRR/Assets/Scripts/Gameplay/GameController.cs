using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { Exploring, Battle, Dialog, Paused }
public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    GameState state;
    GameState stateBeforePause;
    public static GameController Instance { get; private set; }

    private void Awake()
    {
        KieszpotDB.Init();
        ConditionsDB.Init();
        Instance = this;
    }

    private void Start()
    {
        battleSystem.OnBattleOver += EndBattle;

        DialogManager.Instance.OnShowDialog += () => { state = GameState.Dialog; };
        DialogManager.Instance.OnCloseDialog += () => { if(state == GameState.Dialog)state = GameState.Exploring; };
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            stateBeforePause = state;
            state = GameState.Paused;
        }
        else
        {
            state = stateBeforePause;
        }
    }

    public void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<KieszpotParty>();
        var wildKieszpot = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildKieszpot();

        var wildKieszpotCopy = new Kieszpot(wildKieszpot.Base, wildKieszpot.Level);

        battleSystem.StartBattle(playerParty, wildKieszpotCopy);
    }

    void EndBattle(bool playerWon)
    {
        state = GameState.Exploring;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (state == GameState.Exploring)
        {
            playerController.HandleUpdate();

            if (Input.GetKeyDown(KeyCode.K)) SavingSystem.i.Save("saveSlot1");
            if (Input.GetKeyDown(KeyCode.L)) SavingSystem.i.Load("saveSlot1");
        }
        else if (state == GameState.Battle) battleSystem.HandleUpdate();
        else if (state == GameState.Dialog) DialogManager.Instance.HandleUpdate();
    }
}
