using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongGrass : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(PlayerController player)
    {
        AudioManager.i.PlaySFX(3);
        if (UnityEngine.Random.Range(1, 101) <= 10)
        {
            GameController.Instance.StartBattle();
        }
    }
}
