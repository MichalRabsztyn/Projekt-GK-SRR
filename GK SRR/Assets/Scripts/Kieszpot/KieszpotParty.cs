using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KieszpotParty : MonoBehaviour
{
    [SerializeField] List<Kieszpot> kieszpots;

    public List<Kieszpot> Kieszpots
    {
        get
        {
            return kieszpots;
        }
        set
        {
            kieszpots = value;
        }
    }

    private void Start()
    {
        foreach (var kieszpot in kieszpots)
        {
            kieszpot.Init();
        }
    }

    public Kieszpot GetHealthyKieszpot()
    {
        return kieszpots.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddKieszpot(Kieszpot caughtKieszpot)
    {
        if(kieszpots.Count < 8) kieszpots.Add(caughtKieszpot);
        else
        {
            ;
        }
    }
}
