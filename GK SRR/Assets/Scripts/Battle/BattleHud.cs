using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BattleHud : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text statusText;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject expBar;

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
        SetLevel();
        hpBar.SetHP((float) kieszpot.HP / kieszpot.MaxHp);
        StartCoroutine(SetExp(false));

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

    public IEnumerator SetExp(bool isSmooth, bool reset = false)
    {
        if (expBar == null) yield break;

        if (reset) expBar.transform.localScale = new Vector3(0, 1, 1);

        float normalizedExp = GetNormalizedExp();
        if (!isSmooth) expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
        else yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }

    float GetNormalizedExp()
    {
        int currentLevelExp = _kieszpot.Base.GetExpForLevel(_kieszpot.Level);
        int nextLevelExp = _kieszpot.Base.GetExpForLevel(_kieszpot.Level + 1);

        float normalizedExp = (float)(_kieszpot.Exp - currentLevelExp) / (nextLevelExp - currentLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    public void SetLevel()
    {
        levelText.text = "Lvl " + _kieszpot.Level;
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
