using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Image sprite;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] HPBar hpBar;

    [SerializeField] Color highlightedColor;

    Kieszpot _kieszpot;

    public void SetData(Kieszpot kieszpot)
    {
        _kieszpot = kieszpot;
        sprite.sprite = kieszpot.Base.MainSprite;
        nameText.text = kieszpot.Base.Name;
        levelText.text = "Lvl " + kieszpot.Level;
        hpBar.SetHP((float)kieszpot.HP / kieszpot.MaxHp);
    }

    public void SetSelected(bool selected)
    {
        if (selected) nameText.color = highlightedColor;
        else nameText.color = Color.black;
    }
}
