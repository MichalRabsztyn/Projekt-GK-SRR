using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleHud : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] HPBar hpBar;

    Kieszpot _kieszpot;

    public void SetData(Kieszpot kieszpot)
    {
        _kieszpot = kieszpot;
        nameText.text = kieszpot.Base.Name;
        levelText.text = "Lvl " + kieszpot.Level;
        hpBar.SetHP((float) kieszpot.HP / kieszpot.MaxHp);
    }

    public IEnumerator UpdateHP()
    {
        yield return hpBar.SetHPAnimated((float)_kieszpot.HP / _kieszpot.MaxHp);
    }
}
