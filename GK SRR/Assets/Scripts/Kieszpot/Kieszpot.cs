using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kieszpot
{
    KieszpotBase _base;
    int level;

    public int HP { get; set; }

    public List<Move> Moves { get; set; }

    public Kieszpot(KieszpotBase kBase, int kLevel)
    {
        _base = kBase;
        level = kLevel;
        HP = _base.MaxHp;

        Moves = new List<Move>();
        foreach (var move in _base.LearnMoves)
        {
            if (move.Level <= level)
            {
                Moves.Add(new Move(move.Base));
            }

            if (Moves.Count >= 4) break;
        }
    }

    public int MaxHp
    {
        get { return Mathf.FloorToInt((_base.MaxHp * level) / 100.0f) + 10; }
    }

    public int Attack
    {
        get { return Mathf.FloorToInt((_base.Attack * level) / 100.0f) + 5; }
    }

    public int Defence
    {
        get { return Mathf.FloorToInt((_base.Defence * level) / 100.0f) + 5; }
    }

    public int SpAttack
    {
        get { return Mathf.FloorToInt((_base.SpAttack * level) / 100.0f) + 5; }
    }

    public int SpDefence
    {
        get { return Mathf.FloorToInt((_base.SpDefence * level) / 100.0f) + 5; }
    }

    public int Speed
    {
        get { return Mathf.FloorToInt((_base.Speed * level) / 100.0f) + 5; }
    }
}