using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Exploring, Battle, Dialog, Paused, Menu }
public class GameController : MonoBehaviour
{
    [SerializeField] public PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    GameState state;
    GameState stateBeforePause;
    public static GameController Instance { get; private set; }
    int activeSceneIndex;

    SaveLoadMenuController SLmenuController;

    private void Awake()
    {
        KieszpotDB.Init();
        ConditionsDB.Init();
        Instance = this;
        SLmenuController = GetComponent<SaveLoadMenuController>();
    }

    private void Start()
    {
        activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        AudioManager.i.PlayMusic(activeSceneIndex);
        battleSystem.OnBattleOver += EndBattle;

        DialogManager.Instance.OnShowDialog += () => { state = GameState.Dialog; };
        DialogManager.Instance.OnCloseDialog += () => { if(state == GameState.Dialog)state = GameState.Exploring; };

        SLmenuController.onBack += () =>
        {
            state = GameState.Exploring;
        };

        SLmenuController.onMenuSelected += OnMenuSelected;
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

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SLmenuController.OpenMenu();
                state = GameState.Menu;
            }


           // if (Input.GetKeyDown(KeyCode.K)) SavingSystem.i.Save("saveSlot1");
           // if (Input.GetKeyDown(KeyCode.L)) SavingSystem.i.Load("saveSlot1");
        }
        else if (state == GameState.Battle) battleSystem.HandleUpdate();
        else if (state == GameState.Menu) SLmenuController.HandleUpdate();
        else if (state == GameState.Dialog) DialogManager.Instance.HandleUpdate();
    }

    public void SetCurrentSceneActive(int SceneIndex)
    {
        activeSceneIndex = SceneIndex;
        AudioManager.i.PlayMusic(activeSceneIndex);
    }

    void OnMenuSelected(int selectedItem)
    {
        if(selectedItem == 0)
        {
            //save
            SavingSystem.i.Save("saveSlot1");
        }
        else if (selectedItem == 1)
        {
            //load
            SavingSystem.i.Load("saveSlot1");
        }

        state = GameState.Exploring;
    }
}
