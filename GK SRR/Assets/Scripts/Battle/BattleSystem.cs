using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, ActionSelection, MoveSelection, Turn, Busy, PartyScreen, BattleOver }

public enum BattleAction { Move, SwitchKieszpot, CatchKieszpot, Run}

public class BattleSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] GameObject kieszboxSprite;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;
    int currentAction;
    int currentMove;
    int currentMember;
    int escapeAttempts;

    KieszpotParty playerParty;
    Kieszpot wildKieszpot;

    int prevMusic;
    public void StartBattle(KieszpotParty playerParty, Kieszpot wildKieszpot)
    {
        prevMusic = AudioManager.i.currentMusic;
        this.playerParty = playerParty;
        this.wildKieszpot = wildKieszpot;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        AudioManager.i.PlayMusic(((int)MusicClip.Battle));
        playerUnit.Setup(playerParty.GetHealthyKieszpot());
        enemyUnit.Setup(wildKieszpot);

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Kieszpot.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Kieszpot.Base.Name} appeared.");

        escapeAttempts = 0;
        ActionSelection();
    }

    private IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.Turn;
        int enemyCurrentMove = 0;
        if (playerAction == BattleAction.Move)
        {
            playerUnit.Kieszpot.CurrentMove = playerUnit.Kieszpot.Moves[(KieszpotMoveName)currentMove];
            enemyUnit.Kieszpot.CurrentMove = enemyUnit.Kieszpot.GetRandomMove(ref enemyCurrentMove);
            bool firstMovePlayer = playerUnit.Kieszpot.Speed >= enemyUnit.Kieszpot.Speed;
            var firstUnit = (firstMovePlayer) ? playerUnit : enemyUnit;
            var secondUnit = (firstMovePlayer) ? enemyUnit : firstUnit;
            var secondKieszpot = secondUnit.Kieszpot;

            yield return RunMove(firstUnit, secondUnit, firstUnit.Kieszpot.CurrentMove, firstUnit == playerUnit ? currentMove : enemyCurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            if (secondKieszpot.HP > 0)
            {
                yield return RunMove(secondUnit, firstUnit, secondUnit.Kieszpot.CurrentMove, secondUnit == playerUnit ? currentMove : enemyCurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }
        }
        else
        {
            if(playerAction == BattleAction.SwitchKieszpot)
            {
                var selectedKieszpot = playerParty.Kieszpots[currentMember];
                state = BattleState.Busy;
                yield return SwitchKieszpot(selectedKieszpot);
            }
            else if(playerAction == BattleAction.CatchKieszpot)
            {
                dialogBox.EnableActionSelector(false);
                yield return ThrowKieszbox();
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return EscapeBattle();
            }

            var enemyMove = enemyUnit.Kieszpot.GetRandomMove(ref enemyCurrentMove);
            yield return RunMove(enemyUnit, playerUnit, enemyMove, enemyCurrentMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }

        if (state != BattleState.BattleOver) ActionSelection();
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move, int moveID)
    {
        bool canRunMove = sourceUnit.Kieszpot.OnBeforeMove();

        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Kieszpot);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Kieszpot);

        move.PP--;

        yield return dialogBox.TypeDialog($"{sourceUnit.Kieszpot.Base.Name} used {move.Base.Name}");

        if (CheckMoveHit(move, sourceUnit.Kieszpot, targetUnit.Kieszpot))
        {
            bool isPlayer = sourceUnit == playerUnit;
            AudioManager.i.PlaySFX(2);
            sourceUnit.animationController.PlayMoveAnimation((KieszpotMoveName)moveID, isPlayer);
            yield return new WaitForSeconds(0.5f);

            if (move.Base.Category == MoveCategory.Status)
            {
                targetUnit.animationController.PlayHitAnimation();
                yield return RunMoveEffects(move, sourceUnit.Kieszpot, targetUnit.Kieszpot);
            }
            else if(move.Base.Category == MoveCategory.Heal)
            {
               yield return RunMoveHeal(move, sourceUnit.Kieszpot);
               yield return sourceUnit.Hud.UpdateHP();
            }
            else
            {
                targetUnit.animationController.PlayHitAnimation();
                var damageDetails = targetUnit.Kieszpot.TakeDamage(move, sourceUnit.Kieszpot);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if (targetUnit.Kieszpot.HP <= 0) yield return HandleKieszpotFainted(targetUnit);
        }
        else yield return dialogBox.TypeDialog($"{sourceUnit.Kieszpot.Base.Name}'s attack missed");
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
        if(effects.VolatileStatus != ConditionID.none) target.SetVolatileStatus(effects.VolatileStatus);

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunMoveHeal(Move move, Kieszpot kieszpot)
    {
        int healBoost = UnityEngine.Random.Range(25, 100);
        int hpBonus = kieszpot.HP * healBoost / 100;
        int newHP = kieszpot.HP + hpBonus;

        if (newHP > kieszpot.MaxHp) kieszpot.UpdateHP(-kieszpot.MaxHp);
        else kieszpot.UpdateHP(-(newHP));

        kieszpot.StatusChanges.Enqueue($"{kieszpot.Base.Name}'s HP gone up by {hpBonus} points!");
        yield return null;
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.Turn);

        sourceUnit.Kieszpot.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Kieszpot);
        yield return sourceUnit.Hud.UpdateHP();

        if (sourceUnit.Kieszpot.HP <= 0)
        {
            yield return HandleKieszpotFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.Turn);
        }
    }

    bool CheckMoveHit(Move move, Kieszpot sourceUnit, Kieszpot targetUnit)
    {
        if (move.Base.AlwaysHits) return true;

        float moveAccuracy = move.Base.Accuracy;
        int accuracy = sourceUnit.StatBoosts[Stat.Accuracy];
        int evasion = sourceUnit.StatBoosts[Stat.Evasion];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (accuracy > 0) moveAccuracy *= boostValues[accuracy];
        else moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0) moveAccuracy /= boostValues[evasion];
        else moveAccuracy *= boostValues[-evasion];

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
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
        AudioManager.i.PlayMusic(prevMusic);
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
            else if (currentAction == 1) StartCoroutine(RunTurns(BattleAction.CatchKieszpot));
            else if (currentAction == 2)
            {
                prevState = state;
                OpenPartyScreen();
            }
            else if (currentAction == 3) StartCoroutine(RunTurns(BattleAction.Run));
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
                var move = playerUnit.Kieszpot.Moves[(KieszpotMoveName)currentMove];
                if (move.PP == 0) return;

                dialogBox.EnableMoveSelector(false);
                dialogBox.EnableDialogText(true);
                StartCoroutine(RunTurns(BattleAction.Move));
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

            if (prevState == BattleState.ActionSelection)
            {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchKieszpot));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchKieszpot(selectedMember));
            }
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

    IEnumerator HandleKieszpotFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Kieszpot.Base.Name} fainted");
        faintedUnit.animationController.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if(!faintedUnit.IsPlayerUnit)
        {
            int expYield = faintedUnit.Kieszpot.Base.ExperienceYield;
            int enemyLevel = faintedUnit.Kieszpot.Level;
            int expGain = Mathf.FloorToInt(expYield * enemyLevel) / 7;
            playerUnit.Kieszpot.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Kieszpot.Base.Name} gained {expGain} exp");
            yield return playerUnit.Hud.SetExp(true);

            while (playerUnit.Kieszpot.LevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Kieszpot.Base.Name} gained {playerUnit.Kieszpot.Level} lvl!");
                yield return playerUnit.Hud.SetExp(true, true);
            }

            yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(faintedUnit);
    }

    IEnumerator SwitchKieszpot(Kieszpot newKieszpot)
    {
        if (playerUnit.Kieszpot.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Kieszpot.Base.Name}");
            playerUnit.animationController.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newKieszpot);
        dialogBox.SetMoveNames(newKieszpot.Moves);

        yield return dialogBox.TypeDialog($"Go {newKieszpot.Base.Name}!");

        state = BattleState.Turn;
    }

    IEnumerator ThrowKieszbox()
    {
        state = BattleState.Busy;

        yield return dialogBox.TypeDialog($"Player used Kieszbox!");

        Vector3 offsetInstance = new Vector3(2, 0);
        Vector3 offsetJump = new Vector3(0, 2);
        var kieszboxObj = Instantiate(kieszboxSprite, playerUnit.transform.position - offsetInstance, Quaternion.identity);
        var kieszbox = kieszboxObj.GetComponent<SpriteRenderer>();

        yield return kieszbox.transform.DOJump(enemyUnit.transform.position + offsetJump, 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.animationController.PlayCaptureAnimation();
        yield return kieszbox.transform.DOLocalMoveY(enemyUnit.transform.position.y - 1, 0.5f).WaitForCompletion();

        int shakeCount = CatchKieszpot(enemyUnit.Kieszpot);

        for (int i = 0; i < Mathf.Min(shakeCount, 3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return kieszbox.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Kieszpot.Base.Name} was caught!");
            yield return kieszbox.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddKieszpot(enemyUnit.Kieszpot);
            yield return dialogBox.TypeDialog($"{enemyUnit.Kieszpot.Base.Name} was added to your party!");

            Destroy(kieszbox);
            BattleOver(true);
        }
        else
        {
            //yield return new WaitForSeconds(1f);
            kieszbox.DOFade(0, 1.5f);
            yield return enemyUnit.animationController.PlayBreakoutAnimation();

            if(shakeCount < 2) yield return dialogBox.TypeDialog($"{enemyUnit.Kieszpot.Base.Name} broke free!");
            else yield return dialogBox.TypeDialog($"{enemyUnit.Kieszpot.Base.Name} was almost caught!");

            Destroy(kieszbox);
            state = BattleState.Turn;
        }
    }

    int CatchKieszpot(Kieszpot kieszpot)
    {
        float a = (3 * kieszpot.MaxHp - 2 * kieszpot.HP) * kieszpot.Base.CatchRate * ConditionsDB.GetStatusBonus(kieszpot.Status) / (3 * kieszpot.MaxHp);

        if (a >= 100) return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while(shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b) break;
            shakeCount++;
        }

        return shakeCount;
    }

    IEnumerator EscapeBattle()
    {
        state = BattleState.Busy;
        escapeAttempts++;

        int playerSpeed = playerUnit.Kieszpot.Speed;
        int enemySpeed = enemyUnit.Kieszpot.Speed;

        if(enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Ran away safely!");
            BattleOver(true);
        }
        else
        {
            float chance = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            chance = chance % 256;

            if(UnityEngine.Random.Range(0, 256) < chance)
            {
                yield return dialogBox.TypeDialog($"Ran away safely!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"Player couldn't escape safely!");
                state = BattleState.Turn;
            }
        }
    }
}