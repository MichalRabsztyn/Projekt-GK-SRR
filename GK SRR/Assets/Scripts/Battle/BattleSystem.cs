using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BattleOver }

public class BattleSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
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

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Kieszpot.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Kieszpot.Base.Name} appeared.");

        ChooseFirstTurn();
    }

    void ChooseFirstTurn()
    {
        if (playerUnit.Kieszpot.Speed >= enemyUnit.Kieszpot.Speed) ActionSelection();
        else StartCoroutine(EnemyMove());
    }

    private IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;
        Move move = null;
        move = playerUnit.Kieszpot.Moves[(KieszpotMoveName)currentMove];
        yield return RunMove(playerUnit, enemyUnit, move);

        if(state == BattleState.PerformMove) StartCoroutine(EnemyMove());
    }

    private IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;
        int enemyCurrentMove = 0;
        Move move = null;

        move = enemyUnit.Kieszpot.GetRandomMove(ref enemyCurrentMove);
        if (move != null) 
            yield return RunMove(enemyUnit, playerUnit, move);
        else
        {
            Debug.LogWarning($"Enemy unit {enemyUnit.name} has no possible moves!");
            yield return null;
        }
        
        if (state == BattleState.PerformMove) ActionSelection();
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Kieszpot.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Kieszpot);
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Kieszpot);

        move.PP--;

        yield return dialogBox.TypeDialog($"{sourceUnit.Kieszpot.Base.Name} used {move.Base.Name}");

        sourceUnit.animationController.PlayMoveAnimation((KieszpotMoveName)currentMove, true);
        yield return new WaitForSeconds(0.5f);

        targetUnit.animationController.PlayHitAnimation();

        if(move.Base.Category == MoveCategory.Status)
        {
            yield return RunMoveEffects(move, sourceUnit.Kieszpot, targetUnit.Kieszpot);
        }
        else
        {
            var damageDetails = targetUnit.Kieszpot.TakeDamage(move, sourceUnit.Kieszpot);
            yield return targetUnit.Hud.UpdateHP();
            yield return ShowDamageDetails(damageDetails);
        }
            

        if (targetUnit.Kieszpot.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{targetUnit.Kieszpot.Base.Name} fainted");
            targetUnit.animationController.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(targetUnit);
        }

        sourceUnit.Kieszpot.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Kieszpot);
        yield return sourceUnit.Hud.UpdateHP();

        if (sourceUnit.Kieszpot.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Kieszpot.Base.Name} fainted");
            sourceUnit.animationController.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(sourceUnit);
        }
    }

    IEnumerator RunMoveEffects(Move move, Kieszpot source, Kieszpot target)
    {
        var effects = move.Base.Effects;
        if (effects.Boosts != null)
        {
            if (move.Base.Target == MoveTarget.Self) source.ApplyBoosts(effects.Boosts);
            else target.ApplyBoosts(effects.Boosts);
        }

        if(effects.Status != ConditionID.none) target.SetStatus(effects.Status);

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator ShowStatusChanges(Kieszpot kieszpot)
    {
        while (kieszpot.StatusChanges.Count > 0)
        {
            var message = kieszpot.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextKieszpot = playerParty.GetHealthyKieszpot();
            if (nextKieszpot != null) OpenPartyScreen();
            else BattleOver(false);
        }
        else BattleOver(true);
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f) yield return dialogBox.TypeDialog("A critical hit!");
        if (damageDetails.Effectiveness > 1f) yield return dialogBox.TypeDialog("It's super effective!");
        else if (damageDetails.Effectiveness < 1f) yield return dialogBox.TypeDialog("It's not very effective!");
    }

    private void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Kieszpots.ForEach(P => P.OnBattleOver());
        OnBattleOver(won);
    }

    private void ActionSelection()
    {
        state = BattleState.ActionSelection;
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

    private void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection) HandleActionSelection();
        else if (state == BattleState.MoveSelection) HandleMoveSelection();
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
            if (currentAction == 0) MoveSelection();
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

        if (playerUnit.Kieszpot.Moves[(KieszpotMoveName)currentMove] != null)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                dialogBox.EnableMoveSelector(false);
                dialogBox.EnableDialogText(true);
                StartCoroutine(PlayerMove());
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
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
            if (selectedMember == playerUnit.Kieszpot)
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
            state = BattleState.ActionSelection;
            ActionSelection();
        }
    }

    IEnumerator SwitchKieszpot(Kieszpot newKieszpot)
    {
        bool currentKieszpotFainted = true;
        if (playerUnit.Kieszpot.HP > 0)
        {
            currentKieszpotFainted = false;
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Kieszpot.Base.Name}");
            playerUnit.animationController.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newKieszpot);
        dialogBox.SetMoveNames(newKieszpot.Moves);

        yield return dialogBox.TypeDialog($"Go {newKieszpot.Base.Name}!");

        if (currentKieszpotFainted) ChooseFirstTurn();
        else StartCoroutine(EnemyMove());
    }
}