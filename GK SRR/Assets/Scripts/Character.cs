using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float movementSpeed;

    public bool isMoving { get; private set; }

    CharacterAnimator animator;

    private void Awake()
    {
        animator = GetComponent<CharacterAnimator>(); 
    }

    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver=null)
    {
        

        animator.MoveX = Mathf.Clamp(moveVec.x, -1f, 1f);
        animator.MoveY = Mathf.Clamp(moveVec.y, -1f, 1f);

        var targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        if (!IsWalkable(targetPos))
            yield break;

        isMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, movementSpeed * Time.deltaTime);
            yield return null; //zacznij od tego punktu w kolejnym Update

        }
        transform.position = targetPos;

        isMoving = false;

        OnMoveOver?.Invoke(); ;

    }

    public void HandleUpdate()
    {
        animator.isMoving = isMoving;
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.1f, GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer) != null)
        {
            return false;
        }
        return true;
    }

    public CharacterAnimator Animator
    {
        get => animator;
    }
}