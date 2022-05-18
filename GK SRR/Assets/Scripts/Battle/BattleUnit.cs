using UnityEngine;

[RequireComponent(typeof(BattleUnitAnimation))]
public class BattleUnit : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] bool isPlayerUnit;

    public Kieszpot Kieszpot { get; private set; }
    public BattleUnitAnimation animationController { get; private set; }

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
        }
        else
        {
            animationController.SetFaceAnimation(true);
        }
    }
}
