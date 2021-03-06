using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using System;

[CreateAssetMenu(fileName = "Kieszpot", menuName = "Kieszpot/Create new kieszpot")]
public class KieszpotBase : ScriptableObject
{
    [SerializeField] string name;
    [SerializeField] Type type1;
    [SerializeField] Type type2;

    [Header("Parameters")]
    [Range(1, 5000)] [SerializeField] int maxHp;
    [Range(1, 50)] [SerializeField] int attack;
    [Range(1, 50)] [SerializeField] int defence;
    [Range(1, 10)] [SerializeField] int spAttack;
    [Range(1, 10)] [SerializeField] int spDefence;
    [Range(1, 10)] [SerializeField] int speed;
    [Range(1, 100)] [SerializeField] int catchRate = 50;
    [SerializeField] int experienceYield;
    [SerializeField] GrowRate growRate;


    [Space]
    [TextArea(15, 20)] [SerializeField] string description;

    [Header("Textures")]
    [SerializeField] Sprite mainSprite;
    [SerializeField] AnimatorController animatorController;

    [Space]
    [SerializeField] LearnableMoves attackMove;
    [SerializeField] LearnableMoves specialAttackMove;
    [SerializeField] LearnableMoves healMove;
    [SerializeField] LearnableMoves boostMove;
    Dictionary<KieszpotMoveName, LearnableMoves> learnableMoves = null;


    public string Name => name;
    public string Description => description;
    public Type Type1 => type1;
    public Type Type2 => type2;
    public int MaxHp => maxHp;
    public int Attack => attack;
    public int Defence => defence;
    public int SpAttack => spAttack;
    public int SpDefence => spDefence;
    public int Speed => speed;
    public int CatchRate => catchRate;
    public int ExperienceYield => experienceYield;
    public GrowRate GrowRate => growRate;
    public Sprite MainSprite => mainSprite;
    public AnimatorController AnimatorController => animatorController;

    public Dictionary<KieszpotMoveName, LearnableMoves> LearnableMoves
    {
        get
        {
            if (learnableMoves == null)
            {
                learnableMoves = new Dictionary<KieszpotMoveName, LearnableMoves>();
                learnableMoves.Add(KieszpotMoveName.Attack, attackMove);
                learnableMoves.Add(KieszpotMoveName.SpecialAttack, specialAttackMove);
                learnableMoves.Add(KieszpotMoveName.Heal, healMove);
                learnableMoves.Add(KieszpotMoveName.Boost, boostMove);
            }

            return learnableMoves;
        }
    }

    public int GetExpForLevel(int level)
    {
        if (growRate == GrowRate.Fast) return 4 * (level * level * level) / 5;
        else if (growRate == GrowRate.Medium) return level * level * level;
        return -1;
    }
}

[System.Serializable]
public class LearnableMoves
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base { get { return moveBase; } }
    public int Level { get { return level; } }
}

public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed,
    Accuracy,
    Evasion
}

public enum Type
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon,
    Dark,
    Steel,
    Fairy
}

public enum GrowRate
{
    Fast, Medium, Slow
}

public class TypeChart
{
    static float[][] chart =
    {
        //                          Nor   Fir   Wat   Ele   Gra   Ice   Fig   Poi   Gro   Fly   Psy   Bug   Roc   Gho   Dra   Dar  Ste    Fai
        /*Normal*/      new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0.5f, 0,    1f,   1f,   0.5f, 1f},
        /*Fire*/        new float[] {1f,   0.5f, 0.5f, 1f,   2f,   2f,   1f,   1f,   1f,   1f,   1f,   2f,   0.5f, 1f,   0.5f, 1f,   2f,   1f},
        /*Water*/       new float[] {1f,   2f,   0.5f, 1f,   0.5f, 1f,   1f,   1f,   2f,   1f,   1f,   1f,   2f,   1f,   0.5f, 1f,   1f,   1f},
        /*Electric*/    new float[] {1f,   1f,   2f,   0.5f, 0.5f, 1f,   1f,   1f,   0f,   2f,   1f,   1f,   1f,   1f,   0.5f, 1f,   1f,   1f},
        /*Grass*/       new float[] {1f,   0.5f, 2f,   1f,   0.5f, 1f,   1f,   0.5f, 2f,   0.5f, 1f,   0.5f, 2f,   1f,   0.5f, 1f,   0.5f, 1f},
        /*Ice*/         new float[] {1f,   0.5f, 0.5f, 1f,   2f,   0.5f, 1f,   1f,   2f,   2f,   1f,   1f,   1f,   1f,   2f,   1f,   0.5f, 1f},
        /*Fighting*/    new float[] {2f,   1f,   1f,   1f,   1f,   2f,   1f,   0.5f, 1f,   0.5f, 0.5f, 0.5f, 2f,   0f,   1f,   2f,   2f,   0.5f},
        /*Poison*/      new float[] {1f,   1f,   1f,   1f,   2f,   1f,   1f,   0.5f, 0.5f, 1f,   1f,   1f,   0.5f, 0.5f, 1f,   1f,   0f,   2f},
        /*Ground*/      new float[] {1f,   2f,   1f,   2f,   0.5f, 1f,   1f,   2f,   1f,   0f,   1f,   0.5f, 2f,   1f,   1f,   1f,   2f,   1f},
        /*Flying*/      new float[] {1f,   1f,   1f,   0.5f, 2f,   1f,   2f,   1f,   1f,   1f,   1f,   2f,   0.5f, 1f,   1f,   1f,   0.5f, 1f},
        /*Psychic*/     new float[] {1f,   1f,   1f,   1f,   1f,   1f,   2f,   2f,   1f,   1f,   0.5f, 1f,   1f,   1f,   1f,   0f,   0.5f, 1f},
        /*Bug*/         new float[] {1f,   0.5f, 1f,   1f,   2f,   1f,   0.5f, 0.5f, 1f,   0.5f, 2f,   1f,   1f,   0.5f, 1f,   2f,   0.5f, 0.5f},
        /*Rock*/        new float[] {1f,   2f,   1f,   1f,   1f,   2f,   0.5f, 1f,   0.5f, 2f,   1f,   2f,   1f,   1f,   1f,   1f,   0.5f, 1f},
        /*Ghost*/       new float[] {0f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0.5f, 1f,   1f,   2f,   1f,   0.5f, 1f,   1f},
        /*Dragon*/      new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   2f,   1f,   0.5f, 0f},
        /*Dark*/        new float[] {1f,   1f,   1f,   1f,   1f,   1f,   0.5f, 1f,   1f,   1f,   2f,   1f,   1f,   2f,   1f,   0.5f, 1f,   0.5f},
        /*Steel*/       new float[] {1f,   0.5f, 0.5f, 0.5f, 1f,   2f,   1f,   1f,   1f,   1f,   1f,   2f,   0.5f, 1f,   1f,   1f,   0.5f, 2f},
        /*Fairy*/       new float[] {1f,   0.5f, 1f,   1f,   1f,   1f,   2f,   0.5f, 1f,   1f,   1f,   1f,   1f,   1f,   2f,   2f,   0.5f, 1f},
    };

    public static float GetEffectiveness(Type attackType, Type defenseType)
    {
        if (attackType == Type.None || defenseType == Type.None) return 1f;
        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}

public enum KieszpotMoveName
{
    Attack = 0,
    SpecialAttack = 1,
    Heal = 2,
    Boost = 3
}