using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour, ISavable
{

    private Character character;
    [SerializeField] public TextMeshProUGUI OnScreenInfo;
    private bool isMoving;
    private Vector2 input;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        if (!character.isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input.x != 0)
            {
                input.y = 0;
            }

            if (input != Vector2.zero)
            {
                StartCoroutine(character.Move(input, OnMoveOver));
            }
        }
        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.E)) Interact();
    }

    void Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    private void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.i.TriggerableLayers);
        foreach (var collider in colliders)
        {
            var triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {
                character.Animator.isMoving = false;
                triggerable.OnPlayerTriggered(this);
                break;
            }
        }
        AudioManager.i.PlaySFX(4);
    }

    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            kieszpots = GetComponent<KieszpotParty>().Kieszpots.Select(p=>p.GetSaveData()).ToList()
        };
        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;

        //Position
        var pos = saveData.position;
        transform.position = new Vector3(pos[0], pos[1]);

        //Party
        GetComponent<KieszpotParty>().Kieszpots = saveData.kieszpots.Select(s => new Kieszpot(s)).ToList();
    }

    public Character Character => character;

    public void ShowOnScreenInfo()
    {
        OnScreenInfo.alpha = 255f;
        OnScreenInfo.text = SceneManager.GetActiveScene().name;

        InfoFadeOut(3f);
    }
    public void InfoFadeOut(float time)
    {
        OnScreenInfo.DOFade(0f, time);
    }

}
[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<KieszpotSaveData> kieszpots;
}