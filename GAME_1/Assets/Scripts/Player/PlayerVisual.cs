using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerVisual : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private const string is_run_up = "IsRunningUp";
    private const string is_run_down = "IsRunningDown";
    private const string is_run_left_right = "IsRunningLeftRight";
    //private const string is_attack = "IsAttacking"; //?
    private void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

    }
    private void Update()
    {
        anim.SetBool(is_run_up, Player.Instance.IsRunningUp());
        anim.SetBool(is_run_down, Player.Instance.IsRunningDown());
        anim.SetBool(is_run_left_right, Player.Instance.IsRunningLeftRight());
        //anim.SetBool(is_attack, Player.Instance.IsAttacking()); //?
        ReversePlayer();
    }
    private void ReversePlayer()
    {
        if (Player.Instance.Rev() && Player.Instance.IsRunningLeftRight())
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }
}
