using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using AnimationInstancing;

public class KylePlayAnimation : MonoBehaviour
{
    [SerializeField] private AIAnimator animator;
    void Start()
    {
        animator.Play("Idle");
        animator.SetSpeed(1);
    }


}
