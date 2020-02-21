using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAnimator : MonoBehaviour
{
    [SerializeField] private AIAnimatorController controller;
    private AIRuntimeAnimatorController runtimeController;

    private MeshRenderer[] meshRenderers;
    private MaterialPropertyBlock propBlock;

    void Awake()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        runtimeController = (AIRuntimeAnimatorController)ScriptableObject.CreateInstance("AIRuntimeAnimatorController");
    //    runtimeController = new AIRuntimeAnimatorController();
        runtimeController.Initalize(controller, meshRenderers);
    }

    public void Play(string _name) {
        runtimeController.Play(_name);
    }

    

    public void Update() {
        runtimeController.Update();
    }

    public void SetSpeed(float _speed) {
        runtimeController.SetSpeed(_speed);
    }

    public void SetBool(string _name, bool _value) {
        runtimeController.SetBool(_name, _value);
    }

}

