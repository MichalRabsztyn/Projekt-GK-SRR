using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; }

    public Action<Kieszpot> OnStart { get; set; }
    public Func<Kieszpot, bool> OnBeforeMove { get; set; }
    public Action<Kieszpot> OnAfterTurn { get; set; }
}
