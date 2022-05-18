using System;
using System.Collections;
using UnityEngine;
using UnityEditor.Animations;

[RequireComponent(typeof(Animator))]
public class BattleUnitAnimation : MonoBehaviour
{
    [Header("Animation Keys")]
    [SerializeField] private string faceAnimationName = "isFront";
    [SerializeField] private string specialAttackAnimationName = "isSpecialAttack";
    [SerializeField] private string attackAnimationName = "attack";

    [Header("Settings")]
    [SerializeField] private float attackAnimationTime = 1.0f;

    private Animator animator;


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Setup(AnimatorController controller)
    {
        animator.runtimeAnimatorController = controller;
    }


    /*>>> Animation methods <<<*/
    public void SetFaceAnimation(bool isFront)
    {
        try
        {
            float faceValue = isFront ? 1.0f : 0.0f;
            animator.SetFloat(faceAnimationName, faceValue);
        }
        catch (NullReferenceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public void PlayAttackAnimation(KieszpotMoveName move)
    {
        switch (move)
        {
            case KieszpotMoveName.Attack:
                PlayNormalAttack();
                break;

            case KieszpotMoveName.SpecialAttack:
                PlaySpecialAttack();
                break;

            case KieszpotMoveName.Heal:
                Debug.LogWarning("Animation controller doesn't support Heal move yet!");
                break;

            case KieszpotMoveName.Unknown:
                Debug.LogWarning("Animation controller doesn't support Unknown move yet!");
                break;

            default:
                Debug.LogWarning("How did we get here?");
                break;
        }
    }

    private void PlaySpecialAttack()
    {
        try
        {
            StartCoroutine(PlayAttackCoroutine(1.0f, attackAnimationTime));
        }
        catch (NullReferenceException ex)
        {
            Debug.LogError(ex.GetType());
        }
    }

    private void PlayNormalAttack()
    {
        try
        {
            StartCoroutine(PlayAttackCoroutine(0.0f, attackAnimationTime));
        }
        catch (NullReferenceException ex)
        {
            Debug.LogError(ex.GetType());
        }
    }

    private IEnumerator PlayAttackCoroutine(float isSpecial, float animationTime)
    {
        animator.SetFloat(specialAttackAnimationName, isSpecial);
        animator.SetBool(attackAnimationName, true);
        yield return new WaitForSeconds(animationTime);
        animator.SetBool(attackAnimationName, false);
    }
}
