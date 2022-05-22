using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(BattleUnitAnimation))]
public class BattleUnit : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] BattleHud hud;

    [Header("Settings")]
    [SerializeField] bool isPlayerUnit;

    public Kieszpot Kieszpot { get; private set; }
    public BattleUnitAnimation animationController { get; private set; }
    public bool IsPlayerUnit { get { return isPlayerUnit; } }

    public BattleHud Hud { get { return hud; } }

    private void Awake()
    {
        animationController = GetComponent<BattleUnitAnimation>();
    }

    public void Setup(Kieszpot kieszpot)
    {
        Kieszpot = kieszpot;
        animationController.Setup(Kieszpot.Base.AnimatorController);

        if (isPlayerUnit)
        {
            animationController.SetFaceAnimation(false);
            animationController.PlayEnterAnimation(false);
        }
        else
        {
            animationController.SetFaceAnimation(true);
            animationController.PlayEnterAnimation(true);
        }

        hud.SetData(kieszpot);
    }

    public void FaceKieszpot()
    {
        if (isPlayerUnit) animationController.SetFaceAnimation(false);
        else animationController.SetFaceAnimation(true);
    }
}
