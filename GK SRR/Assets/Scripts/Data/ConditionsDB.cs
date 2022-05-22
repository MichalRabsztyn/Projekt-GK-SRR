using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
      public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.poison,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned!",
                OnAfterTurn = (Kieszpot kieszpot) =>
                {
                    kieszpot.UpdateHP(kieszpot.MaxHp / 8);
                    kieszpot.StatusChanges.Enqueue($"{kieszpot.Base.Name} hurt itself due to poison");
                }
            }
        },
        {
            ConditionID.burn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned!",
                OnAfterTurn = (Kieszpot kieszpot) =>
                {
                    kieszpot.UpdateHP(kieszpot.MaxHp / 16);
                    kieszpot.StatusChanges.Enqueue($"{kieszpot.Base.Name} hurt itself due to burn");
                }
            }
        }

    };
}

public enum ConditionID
{
    none, poison, burn, sleep, paralized, freeze
}
