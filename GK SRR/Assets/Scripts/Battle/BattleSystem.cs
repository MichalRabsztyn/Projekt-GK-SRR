using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy, PartyScreen }

public class BattleSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;

    KieszpotParty playerParty;
    Kieszpot wildKieszpot;

    public void StartBattle(KieszpotParty playerParty, Kieszpot wildKieszpot)
    {
        this.playerParty = playerParty;
        this.wildKieszpot = wildKieszpot;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHealthyKieszpot());
        enemyUnit.Setup(wildKieszpot);
        playerHud.SetData(playerUnit.Kieszpot);
        enemyHud.SetData(enemyUnit.Kieszpot);

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Kieszpot.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Kieszpot.Base.Name} appeared.");

        PlayerAction();
    }

    private IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;
        Move move = null;

        try
        {
            move = playerUnit.Kieszpot.Moves[(KieszpotMoveName)currentMove];
            move.PP--;
        }
        catch (NullReferenceException ex)
        {
            Debug.LogError($"Player Unit can do not have move: {(KieszpotMoveName)currentMove}\n" + ex.Message);
        }
        yield return dialogBox.TypeDialog($"{playerUnit.Kieszpot.Base.Name} used {move.Base.Name}");

        playerUnit.animationController.PlayMoveAnimation((KieszpotMoveName)currentMove, true);
        yield return new WaitForSeconds(0.5f);

        enemyUnit.animationController.PlayHitAnimation();

        var damageDetails = enemyUnit.Kieszpot.TakeDamage(move, playerUnit.Kieszpot);
        yield return enemyHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Kieszpot.Base.Name} fainted");

            enemyUnit.animationController.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            OnBattleOver(false);
        }
        else StartCoroutine(EnemyMove());
    }

    private IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;
        int enemyCurrentMove = 0;
        Move move = null;

        try
        {
            move = enemyUnit.Kieszpot.GetRandomMove(ref enemyCurrentMove);
            move.PP--;
        }
        catch (NullReferenceException ex)
        {
            Debug.LogError($"Player Unit can do not have move: {(KieszpotMoveName)currentMove}\n" + ex.Message);
        }
        yield return dialogBox.TypeDialog($"{enemyUnit.Kieszpot.Base.Name} used {move.Base.Name}");

        enemyUnit.animationController.PlayMoveAnimation((KieszpotMoveName)enemyCurrentMove, false);
        yield return new WaitForSeconds(0.5f);

        playerUnit.animationController.PlayHitAnimation();

        var damageDetails = playerUnit.Kieszpot.TakeDamage(move, enemyUnit.Kieszpot);
        yield return playerHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Kieszpot.Base.Name} fainted");

            playerUnit.animationController.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);

            var nextKieszpot = playerParty.GetHealthyKieszpot();
            if (nextKieszpot != null)
            {
                playerUnit.Setup(nextKieszpot);
                playerHud.SetData(nextKieszpot);

                dialogBox.SetMoveNames(nextKieszpot.Moves);

                yield return dialogBox.TypeDialog($"Go {nextKieszpot.Base.Name}!");

                PlayerAction();
            }
            else OnBattleOver(false);
        }
        else PlayerAction();
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f) yield return dialogBox.TypeDialog("A critical hit!");
        if (damageDetails.Effectiveness > 1f) yield return dialogBox.TypeDialog("It's super effective!");
        else if (damageDetails.Effectiveness < 1f) yield return dialogBox.TypeDialog("It's not very effective!");
    }

    private void PlayerAction()
    {
        state = BattleState.PlayerAction;
        dialogBox.SetDialog("Choose an action");
        dialogBox.EnableActionSelector(true);
    }

    private void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        playerUnit.gameObject.SetActive(false);
        enemyUnit.gameObject.SetActive(false);
        partyScreen.SetPartyData(playerParty.Kieszpots);
        partyScreen.gameObject.SetActive(true);
    }

    private void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    public void HandleUpdate()
    {
        if (state == BattleState.PlayerAction) HandleActionSelection();
        else if (state == BattleState.PlayerMove) HandleMoveSelection();
        else if (state == BattleState.PartyScreen) HandlePartySelection();
    }

    private void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) ++currentAction;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) --currentAction;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow)) currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentAction == 0) PlayerMove();
            else if (currentAction == 1) ; //Bag
            else if (currentAction == 2) OpenPartyScreen();
            else if (currentAction == 3) ; //Run
        }
    }
    private void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) ++currentMove;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) --currentMove;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow)) currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Kieszpot.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Kieszpot.Moves[(KieszpotMoveName)currentMove]);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            PlayerAction();
        }
    }

    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) ++currentMember;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) --currentMember;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) currentMember += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow)) currentMember -= 2;

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Kieszpots.Count - 1);

        partyScreen.UpdatePartySelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            var selectedMember = playerParty.Kieszpots[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SendMessage("You can't battle with Kieszpot, which fainted!");
                return;
            }
            if(selectedMember == playerUnit.Kieszpot)
            {
                partyScreen.SendMessage("You need to choose new Kieszpot!");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            playerUnit.FaceKieszpot();
            enemyUnit.gameObject.SetActive(true);
            state = BattleState.Busy;
            StartCoroutine(SwitchKieszpot(selectedMember));
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            partyScreen.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            playerUnit.FaceKieszpot();
            enemyUnit.gameObject.SetActive(true);
            state = BattleState.PlayerAction;
            PlayerAction();
        }
    }

    IEnumerator SwitchKieszpot(Kieszpot newKieszpot)
    {
        yield return dialogBox.TypeDialog($"Come back {playerUnit.Kieszpot.Base.Name}");
        playerUnit.animationController.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        playerUnit.Setup(newKieszpot);
        playerHud.SetData(newKieszpot);
        dialogBox.SetMoveNames(newKieszpot.Moves);

        yield return dialogBox.TypeDialog($"Go {newKieszpot.Base.Name}!");

        StartCoroutine(EnemyMove());
    }
}