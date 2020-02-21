using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KylePlayAnimation : MonoBehaviour
{
    [SerializeField] private AIAnimator animator;
    void Start()
    {
           }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.E)) {
            animator.Play("Idle");
        }

        if(Input.GetKeyDown(KeyCode.W)) {
            animator.SetSpeed(2);
        }
        if(Input.GetKeyDown(KeyCode.S)) {
            animator.SetSpeed(1);
        }
    }

}
