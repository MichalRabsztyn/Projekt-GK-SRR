using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy }

public class BattleSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialogBox dialogBox;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;

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

        dialogBox.SetMoveNames(playerUnit.Kieszpot.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Kieszpot.Base.Name} appeared.");

        PlayerAction();
    }

    private IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;
        var move = playerUnit.Kieszpot.Moves[currentMove];
        move.PP--;
        yield return dialogBox.TypeDialog($"{playerUnit.Kieszpot.Base.Name} used {move.Base.Name}");

        var damageDetails = enemyUnit.Kieszpot.TakeDamage(move, playerUnit.Kieszpot);
        yield return enemyHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Kieszpot.Base.Name} fainted");

            yield return new WaitForSeconds(2f);
            OnBattleOver(false);
        }
        else StartCoroutine(EnemyMove());
    }

    private IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;
        var move = enemyUnit.Kieszpot.GetRandomMove();
        move.PP--;
        yield return dialogBox.TypeDialog($"{enemyUnit.Kieszpot.Base.Name} used {move.Base.Name}");

        var damageDetails = playerUnit.Kieszpot.TakeDamage(move, enemyUnit.Kieszpot);
        yield return playerHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Kieszpot.Base.Name} fainted");

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
        StartCoroutine(dialogBox.TypeDialog("Choose an action"));
        dialogBox.EnableActionSelector(true);
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
    }

    private void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 1) ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 0) --currentAction;
        }

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentAction == 0) PlayerMove();
            else if (currentAction == 1) ; //Run
        }
    }
    private void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < playerUnit.Kieszpot.Moves.Count - 1) ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove > 0) --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerUnit.Kieszpot.Moves.Count - 2) currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 1) currentAction -= 2;
        }

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Kieszpot.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
    }
}
