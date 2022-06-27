using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KieszpotDB
{
    static Dictionary<string, KieszpotBase> kieszpots;

    public static void Init()
    {
        kieszpots = new Dictionary<string, KieszpotBase>();

        var kieszpotArray = Resources.LoadAll<KieszpotBase>("");
        foreach (var kieszpot in kieszpotArray)
        {
            if (kieszpots.ContainsKey(kieszpot.Name))
            {
                Debug.LogError($"There are two kieszpots with name {kieszpot.Name}");
                continue;
            }

            kieszpots[kieszpot.Name] = kieszpot;
        }
    }

    public static KieszpotBase GetKieszpotByName(string name)
    {
        if (!kieszpots.ContainsKey(name))
        {
            Debug.LogError($"kieszpot with name {name} has been not found in database");
            return null;
        }
        return kieszpots[name];
    }
}
