using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;

    public Kieszpot Kieszpot { get; set; }

    public void Setup(Kieszpot kieszpot)
    {
        Kieszpot = kieszpot;
        if (isPlayerUnit)
        {
            //GetComponent<Image>().sprite = kieszpot.Base.BackSprite;
        }
        else
        {
            //GetComponent<Image>().sprite = kieszpot.Base.FrontSprite;
        }
    }
}
