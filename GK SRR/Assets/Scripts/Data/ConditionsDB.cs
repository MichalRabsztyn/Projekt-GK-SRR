using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var pair in Conditions)
        {
            var conditionId = pair.Key;
            var condition = pair.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
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
            ConditionID.brn,
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
        },
        {
            ConditionID.prl,
            new Condition()
            {
                Name = "Paralyze",
                StartMessage = "has been paralyzed!",
                OnBeforeMove = (Kieszpot kieszpot) =>
                {
                    if(Random.Range(1, 5) == 1)
                    {
                        kieszpot.StatusChanges.Enqueue($"{kieszpot.Base.Name}'s paralyzed and can't move");
                        return false;
                    }
                    return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Condition()
            {
                Name = "Freeze",
                StartMessage = "has been frozen",
                OnBeforeMove = (Kieszpot kieszpot) =>
                {
                    if(Random.Range(1, 5) == 1)
                    {
                        kieszpot.CureStatus();
                        kieszpot.StatusChanges.Enqueue($"{kieszpot.Base.Name}'s is not frozen anymore");
                        return true;
                    }
                    return false;
                }
            }
        },
        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "has fallen asleep",
                OnStart = (Kieszpot kieszpot) =>
                {
                    kieszpot.StatusTime = Random.Range(1, 4);
                    Debug.Log($"Will be asleep for {kieszpot.StatusTime} moves");
                },
                OnBeforeMove = (Kieszpot kieszpot) =>
                {
                    if(kieszpot.StatusTime <= 0)
                    {
                        kieszpot.CureStatus();
                        kieszpot.StatusChanges.Enqueue($"{kieszpot.Base.Name} woke up!");
                        return true;
                    }
                    kieszpot.StatusTime--;
                    kieszpot.StatusChanges.Enqueue($"{kieszpot.Base.Name} is sleeping");
                    return false;
                }
            }
        },
        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "has been confused",
                OnStart = (Kieszpot kieszpot) =>
                {
                    kieszpot.VolatileStatusTime = Random.Range(1, 5);
                    Debug.Log($"Will be confused for {kieszpot.VolatileStatusTime} moves");
                },
                OnBeforeMove = (Kieszpot kieszpot) =>
                {
                    if(kieszpot.VolatileStatusTime <= 0)
                    {
                        kieszpot.CureStatus();
                        kieszpot.StatusChanges.Enqueue($"{kieszpot.Base.Name} kicked out of confusion!");
                        return true;
                    }
                    kieszpot.VolatileStatusTime--;
                    if(Random.Range(1, 3) == 1) return true;
                    kieszpot.StatusChanges.Enqueue($"{kieszpot.Base.Name} is confused");
                    kieszpot.UpdateHP(kieszpot.MaxHp / 8);
                    kieszpot.StatusChanges.Enqueue($"It hurt itself due to confusion");
                    return false;
                }
            }
        }
    };
}

public enum ConditionID
{
    none, psn, brn, slp, prl, frz, confusion
}
