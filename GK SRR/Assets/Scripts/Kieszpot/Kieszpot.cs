using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Kieszpot
{
    [SerializeField] KieszpotBase _base;
    [SerializeField] int level;

    public KieszpotBase Base
    {
        get { return _base; }
    }

    public int Level
    {
        get { return level; }
    }

    public int HP { get; set; }

    public Dictionary<KieszpotMoveName, Move> Moves { get; set; }

    public void Init()
    {
        HP = MaxHp;

        Moves = new Dictionary<KieszpotMoveName, Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if(move.Value.Base == null)
            {
                continue;
            }
            
            if (move.Value.Level <= Level)
            {
                Moves.Add(move.Key, new Move(move.Value.Base));
            }

            if (Moves.Count >= 4) break;
        }
    }

    public int MaxHp
    {
        get { return Mathf.FloorToInt((Base.MaxHp * Level) / 100.0f) + 10; }
    }

    public int Attack
    {
        get { return Mathf.FloorToInt((Base.Attack * Level) / 100.0f) + 5; }
    }

    public int Defence
    {
        get { return Mathf.FloorToInt((Base.Defence * Level) / 100.0f) + 5; }
    }

    public int SpAttack
    {
        get { return Mathf.FloorToInt((Base.SpAttack * Level) / 100.0f) + 5; }
    }

    public int SpDefence
    {
        get { return Mathf.FloorToInt((Base.SpDefence * Level) / 100.0f) + 5; }
    }

    public int Speed
    {
        get { return Mathf.FloorToInt((Base.Speed * Level) / 100.0f) + 5; }
    }

    public DamageDetails TakeDamage(Move move, Kieszpot kieszpot)
    {
        float criticalHit = 1f;
        if (Random.value * 100f <= 6.25f) criticalHit = 2f;
        float typeEffect = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            Effectiveness = typeEffect,
            Critical = criticalHit,
            Fainted = false
        };

        float attack = (move.Base.IsSpecial) ? kieszpot.SpAttack : kieszpot.Attack;
        float defence = (move.Base.IsSpecial) ? kieszpot.SpDefence : kieszpot.Defence;

        float boost = Random.Range(0.85f, 1f) * typeEffect * criticalHit;
        float kieszpotPower = (2 * kieszpot.Level + 10) / 250f;
        float attackPower = kieszpotPower * move.Base.Power * ((float)attack / defence) + 2;
        int damage = Mathf.FloorToInt(attackPower * boost);

        HP -= damage;

        if (HP <= 0)
        {
            HP = 0;
            damageDetails.Fainted = true;
        }

        return damageDetails;
    }

    public Move GetRandomMove(ref int id)
    {
        if (Moves.Count > 0)
        {
            int random = Random.Range(0, Moves.Count - 1);
            id = random;
            return Moves[(KieszpotMoveName)random];
        }
        return null;
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }

    public float Critical { get; set; }
    public float Effectiveness { get; set; }
}
