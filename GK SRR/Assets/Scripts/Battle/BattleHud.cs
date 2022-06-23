using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleHud : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text statusText;
    [SerializeField] HPBar hpBar;

    [Header("Status' colors")]
    [SerializeField] Color poisonColor;
    [SerializeField] Color burnColor;
    [SerializeField] Color paralyzeColor;
    [SerializeField] Color freezeColor;
    [SerializeField] Color sleepColor;

    Kieszpot _kieszpot;
    Dictionary<ConditionID, Color> statusColors;

    public void SetData(Kieszpot kieszpot)
    {
        _kieszpot = kieszpot;
        nameText.text = kieszpot.Base.Name;
        levelText.text = "Lvl " + kieszpot.Level;
        hpBar.SetHP((float) kieszpot.HP / kieszpot.MaxHp);

        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, poisonColor },
            {ConditionID.brn, burnColor },
            {ConditionID.prl, paralyzeColor },
            {ConditionID.frz, freezeColor },
            {ConditionID.slp, sleepColor }
        };

        SetStatusText();
        _kieszpot.OnStatusChange += SetStatusText;
    }

    public void SetStatusText()
    {
        if (_kieszpot.Status == null) statusText.text = "";
        else
        {
            statusText.text = _kieszpot.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_kieszpot.Status.Id];
        }
    }

    public IEnumerator UpdateHP()
    {
        if (_kieszpot.HpChanged)
        {
            yield return hpBar.SetHPAnimated((float)_kieszpot.HP / _kieszpot.MaxHp);
            _kieszpot.HpChanged = false;
        }
    }
}
