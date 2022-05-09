using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class KieszpotTest : MonoBehaviour
{
    private Animator animator;
    private bool isFront = true;
    private bool isSpecialAttack = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        ChangeFaceAnimation(isFront);
        ChangeAttackTypeAnimation(isSpecialAttack);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            ToggleFaceAnimation();
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            PlayAttackAnimation();
        }
        
        if(Input.GetKeyDown(KeyCode.I))
        {
            ToggleAttackTypeAnimation();
        }
    }

    private void ToggleFaceAnimation()
    {
        ChangeFaceAnimation(!isFront);
    }

    private void ChangeFaceAnimation(bool value)
    {
        isFront = value;
        float faceValue = value ? 1.0f : 0.0f;
        animator.SetFloat("isFront", faceValue);
    }

    private void ToggleAttackTypeAnimation()
    {
        ChangeAttackTypeAnimation(!isSpecialAttack);
    }

    private void ChangeAttackTypeAnimation(bool value)
    {
        isSpecialAttack = value;
        float attackType = value ? 1.0f : 0.0f;
        animator.SetFloat("isSpecialAttack", attackType);
    }

    private void PlayAttackAnimation()
    {
        animator.SetTrigger("Attack");
    }    
}
