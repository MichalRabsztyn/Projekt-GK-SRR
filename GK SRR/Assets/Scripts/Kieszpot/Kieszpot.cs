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
    public int MaxHp { get; private set; }
    public bool HpChanged { get; set; }

    public Dictionary<KieszpotMoveName, Move> Moves { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }

    public Condition Status { get; private set; }

    public Queue<string> StatusChanges { get; private set; } = new Queue<string>();

    public void Init()
    {
        Moves = new Dictionary<KieszpotMoveName, Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Value.Base == null || move.Value.Level > Level)
            {
                Moves.Add(move.Key, null);
                continue;
            }

            Moves.Add(move.Key, new Move(move.Value.Base));
        }

        CalculateStats();
        HP = MaxHp;

        ResetStatBoost();
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();

        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100.0f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defence * Level) / 100.0f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100.0f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefence * Level) / 100.0f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100.0f) + 5);

        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100.0f) + 10;
    }

    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0 },
            {Stat.Defense, 0 },
            {Stat.SpAttack, 0 },
            {Stat.SpDefense, 0 },
            {Stat.Speed, 0 }
        };
    }

    int GetStat(Stat stat)
    {
        int statValue = Stats[stat];

        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0) statValue = Mathf.FloorToInt(statValue * boostValues[boost]);
        else statValue = Mathf.FloorToInt(statValue / boostValues[-boost]);

        return statValue;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost > 0) StatusChanges.Enqueue($"{Base.Name}'s {stat} gone up!");
            else StatusChanges.Enqueue($"{Base.Name}'s {stat} gone down!");
        }
    }

    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }

    public int Defence
    {
        get { return GetStat(Stat.Defense); }
    }

    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }

    public int SpDefence
    {
        get { return GetStat(Stat.SpDefense); }
    }

    public int Speed
    {
        get { return GetStat(Stat.Speed); }
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

        float attack = (move.Base.Category == MoveCategory.Special) ? kieszpot.SpAttack : kieszpot.Attack;
        float defence = (move.Base.Category == MoveCategory.Special) ? kieszpot.SpDefence : kieszpot.Defence;

        float boost = Random.Range(0.85f, 1f) * typeEffect * criticalHit;
        float kieszpotPower = (2 * kieszpot.Level + 10) / 250f;
        float attackPower = kieszpotPower * move.Base.Power * ((float)attack / defence) + 2;
        int damage = Mathf.FloorToInt(attackPower * boost);

        UpdateHP(damage);

        return damageDetails;
    }

    public void SetStatus(ConditionID conditionID)
    {
        Status = ConditionsDB.Conditions[conditionID];
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
    }

    public Move GetRandomMove(ref int id)
    {
        if (Moves.Count > 0)
        {
            int loopGuard = 0;
            Move returnMove = null;

            while (returnMove == null)
            {
                if (loopGuard == 10)
                {
                    return null;
                }

                loopGuard++;
                int random = Random.Range(0, Moves.Count - 1);
                id = random;
                returnMove = Moves[(KieszpotMoveName)random];
            }

            return returnMove;
        }
        return null;
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
    }

    public void UpdateHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        HpChanged = true;
    }

    public void OnBattleOver()
    {
        ResetStatBoost();
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }

    public float Critical { get; set; }
    public float Effectiveness { get; set; }
}
