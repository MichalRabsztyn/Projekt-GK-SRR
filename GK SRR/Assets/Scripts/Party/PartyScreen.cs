using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] TMP_Text messageText;

    PartyMemberUI[] memberSlots;
    List<Kieszpot> kieszpots;

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }

    public void SetPartyData(List<Kieszpot> kieszpots)
    {
        this.kieszpots = kieszpots;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < kieszpots.Count) memberSlots[i].SetData(kieszpots[i]);
            else memberSlots[i].gameObject.SetActive(false);
        }

        messageText.text = "Choose a Kieszpot";
    }

    public void UpdatePartySelection(int selectedMember)
    {
        for (int i = 0; i < kieszpots.Count; i++)
        {
            if (i == selectedMember) memberSlots[i].SetSelected(true);
            else memberSlots[i].SetSelected(false);
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
