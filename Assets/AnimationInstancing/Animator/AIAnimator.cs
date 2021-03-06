﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationInstancing
{
    public class AIAnimator : MonoBehaviour
    {
        [SerializeField] private AIAnimatorController controller;
        private AIRuntimeAnimatorController runtimeController;

        private MeshRenderer[] meshRenderers;
        private MaterialPropertyBlock propBlock;

        // [SerializeField] private AIAnimation _animation;

        void Awake() {
            meshRenderers = GetComponentsInChildren<MeshRenderer>();
            runtimeController = (AIRuntimeAnimatorController)ScriptableObject.CreateInstance("AIRuntimeAnimatorController");
            //    runtimeController = new AIRuntimeAnimatorController();
            runtimeController.Initalize(controller, meshRenderers, StartUpdate);
        }

        private void Start() {
            runtimeController.Entry();
        }

        public void Play(string _name) {
            runtimeController.Play(_name);
        }


        public void StartUpdate(State _state) {
            StopAllCoroutines();
            StartCoroutine(_state.Update());
        }

        public void SetSpeed(float _speed) {
            runtimeController.SetSpeed(_speed);
        }

        public void SetBool(string _name, bool _value) {
            runtimeController.SetBool(_name, _value);
        }


        public void SetFloat(string _name, float _value) {
            runtimeController.SetFloat(_name, _value);
        }

        public void SetInt(string _name, int _value) {
            runtimeController.SetInt(_name, _value);
        }

    }

}