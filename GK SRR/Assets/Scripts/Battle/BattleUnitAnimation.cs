using System;
using System.Collections;
using UnityEngine;
using UnityEditor.Animations;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class BattleUnitAnimation : MonoBehaviour
{
    [Header("Animation Keys")]
    [SerializeField] string faceAnimationName = "isFront";
    [SerializeField] string specialAttackAnimationName = "isSpecialAttack";
    [SerializeField] string attackAnimationName = "attack";

    [Header("Settings")]
    [SerializeField] float attackAnimationTime = 1.0f;
    [SerializeField] Color originalColor = Color.white;
    [SerializeField] Color tintColor = Color.gray;

    Animator animator;
    SpriteRenderer spriteRenderer;

    Vector3 orginalPosition;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        orginalPosition = transform.localPosition;
    }

    public void Setup(AnimatorController controller)
    {
        animator.runtimeAnimatorController = controller;
    }

    public void SetFaceAnimation(bool isPlayerUnit)
    {
        try
        {
            float faceValue = isPlayerUnit ? 1.0f : 0.0f;
            animator.SetFloat(faceAnimationName, faceValue);
            spriteRenderer.material.SetColor("_Color", originalColor);
        }
        catch (NullReferenceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public void PlayEnterAnimation(bool isPlayerUnit)
    {
        if (isPlayerUnit) transform.localPosition = new Vector3(500f, orginalPosition.y);
        else transform.localPosition = new Vector3(-500f, orginalPosition.y);

        transform.DOLocalMoveX(orginalPosition.x, 2f);
    }

    public void PlayMoveAnimation(KieszpotMoveName move, bool isPlayerUnit)
    {
        switch (move)
        {
            case KieszpotMoveName.Attack:
                PlayNormalAttack(isPlayerUnit);
                break;

            case KieszpotMoveName.SpecialAttack:
                PlaySpecialAttack(isPlayerUnit);
                break;

            case KieszpotMoveName.Heal:
                StartCoroutine(PlayJumpAnimation());
                break;

            case KieszpotMoveName.Boost:
                Debug.LogWarning("Animation controller doesn't support Unknown move yet!");
                break;

            default:
                Debug.LogWarning("How did we get here?");
                break;
        }
    }

    private void PlaySpecialAttack(bool isPlayerUnit)
    {
        try
        {
            StartCoroutine(PlayAttackCoroutine(1.0f, attackAnimationTime, isPlayerUnit));
        }
        catch (NullReferenceException ex)
        {
            Debug.LogError(ex.GetType());
        }
    }

    private void PlayNormalAttack(bool isPlayerUnit)
    {
        try
        {
            StartCoroutine(PlayAttackCoroutine(0.0f, attackAnimationTime, isPlayerUnit));
        }
        catch (NullReferenceException ex)
        {
            Debug.LogError(ex.GetType());
        }
    }

    private IEnumerator PlayAttackCoroutine(float isSpecial, float animationTime, bool isPlayerUnit)
    {
        animator.SetFloat(specialAttackAnimationName, isSpecial);
        animator.SetBool(attackAnimationName, true);
        yield return new WaitForSeconds(animationTime / 2);
        PlayMoveXAnimation(isPlayerUnit);
        yield return new WaitForSeconds(animationTime / 2);
        animator.SetBool(attackAnimationName, false);
    }

    private IEnumerator PlayJumpAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        yield return transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
    }

    void PlayMoveXAnimation(bool isPlayerUnit)
    {
        var sequence = DOTween.Sequence();
        if (isPlayerUnit)
        {
            sequence.Append(transform.DOLocalMoveX(orginalPosition.x + 50f, 0.25f));
            sequence.Append(transform.DOLocalMoveX(orginalPosition.x, 0.2f));
        }
        else
        {
            sequence.Append(transform.DOLocalMoveX(orginalPosition.x - 50f, 0.25f));
            sequence.Append(transform.DOLocalMoveX(orginalPosition.x, 0.2f));
        }
    }

    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(spriteRenderer.material.DOColor(tintColor, 0.5f));
        sequence.Append(spriteRenderer.material.DOColor(originalColor, 0.5f));
    }

    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOLocalMoveY(orginalPosition.y - 150f, 0.5f));
        sequence.Join(spriteRenderer.material.DOFade(0f, 0.5f));
    }

    public IEnumerator PlayCaptureAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(spriteRenderer.material.DOFade(0, 0.5f));
        sequence.Join(transform.DOLocalMoveY(orginalPosition.y + 50f, 0.5f));
        sequence.Join(transform.DOScale(new Vector3(0.3f, 0.3f, 1f), 0.5f));
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlayBreakoutAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(spriteRenderer.material.DOFade(1, 0.5f));
        sequence.Join(transform.DOLocalMoveY(orginalPosition.y, 0.5f));
        sequence.Join(transform.DOScale(new Vector3(200f, 200f, 200f), 0.5f));
        yield return sequence.WaitForCompletion();
    }
}
