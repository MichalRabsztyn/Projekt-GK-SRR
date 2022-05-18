using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleDialogBox : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TMP_Text dialogText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] List<TMP_Text> actionTexts;
    [SerializeField] List<TMP_Text> moveTexts;
    [SerializeField] TMP_Text movePPText;
    [SerializeField] TMP_Text typeText;

    [Header("DialogBox variables")]
    [SerializeField] int lettersPerSecond;
    [SerializeField] Color highlightedColor;


    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

        yield return new WaitForSeconds(1f);
    }

    public void EnableDialogText(bool isEnabled)
    {
        dialogText.enabled = isEnabled;
    }

    public void EnableActionSelector(bool isEnabled)
    {
        actionSelector.SetActive(isEnabled);
    }

    public void EnableMoveSelector(bool isEnabled)
    {
        moveSelector.SetActive(isEnabled);
        moveDetails.SetActive(isEnabled);
    }

    public void UpdateActionSelection(int selectAction)
    {
        for (int i = 0; i < actionTexts.Count; i++)
        {
            if (i == selectAction) actionTexts[i].color = highlightedColor;
            else actionTexts[i].color = Color.black;
        }
    }

    public void UpdateMoveSelection(int selectMove, Move move)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i == selectMove) moveTexts[i].color = highlightedColor;
            else moveTexts[i].color = Color.black;
        }

        movePPText.text = $"PP {move.PP}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString();
    }

    public void SetMoveNames(Dictionary<KieszpotMoveName, Move> moves)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i < moves.Count) moveTexts[i].text = moves[(KieszpotMoveName)i].Base.Name;
            else moveTexts[i].text = "-";
        }
    }
}
