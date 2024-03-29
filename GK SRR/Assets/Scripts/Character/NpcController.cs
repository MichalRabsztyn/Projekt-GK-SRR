using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog;
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;
    [SerializeField] bool canHeal = false;

    NPCState state;
    float idleTimer = 0f;
    int currentPattern = 0;

    Character character;
    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void Interact(Transform initiator)
    {
        if (state == NPCState.Idle)
        {
            state = NPCState.Dialog;

            character.LookTowards(initiator.position);

            StartCoroutine(DialogManager.Instance.ShowDialog(dialog));

            state = NPCState.Idle;

            if (canHeal)
            {
                foreach (var kieszpot in GameController.Instance.playerController.GetComponent<KieszpotParty>().Kieszpots)
                {
                    kieszpot.HP = kieszpot.MaxHp;
                    foreach (var move in kieszpot.Moves)
                    {
                        move.Value.PP = move.Value.Base.PP;
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (DialogManager.Instance.isShowing) return;

        if (state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if(idleTimer > timeBetweenPattern)
            {
                idleTimer = 0f;
                if (movementPattern.Count > 0)
                    StartCoroutine(Walk());
            }
        }
        character.HandleUpdate();
    }
    IEnumerator Walk()
    {
        state = NPCState.Walking;

        var oldPos = transform.position;

        yield return character.Move(movementPattern[currentPattern]);

        if (transform.position != oldPos)
        {
            currentPattern = (currentPattern + 1) % movementPattern.Count;
        }
        state = NPCState.Idle;
    }
}

public enum NPCState { Idle, Walking, Dialog }
