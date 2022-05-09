using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Kieszpot> wildKieszpots;

    public Kieszpot GetRandomKieszpot()
    {
        var wildKieszpot = wildKieszpots[Random.Range(0, wildKieszpots.Count)];
        wildKieszpot.Init();
        return wildKieszpot;
    }
}
