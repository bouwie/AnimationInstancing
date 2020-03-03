using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnimationInstancing
{
    public class AIRuntimeAnimatorController : AIAnimatorController
    {

        private MeshRenderer[] meshRenderers;
        private State currentState;

        private float animationZeroReset;
        private float blendZeroReset;

        public float speed { private set; get; }

        [System.NonSerialized]
        public List<Node> runtimeNodes;

        public delegate void UpdateStarter(State _state);
        private UpdateStarter updateStarter;

        public void Initalize(AIAnimatorController _copy, MeshRenderer[] _meshRenderers, UpdateStarter _updateStarter, string _name) {
            runtimeNodes = new List<Node>();

            foreach(Node node in _copy.nodes) {
                runtimeNodes.Add((Node)node.Clone());
            }

            meshRenderers = _meshRenderers;
            updateStarter = _updateStarter;

            name = _name;
            foreach(Node animationNode in runtimeNodes) {
                animationNode.OnRuntimeInitialize(this);
            }
            Debug.Log(name);
        }

        public void SetMeshRenderer(MeshRenderer[] _meshRenderers) {
            meshRenderers = _meshRenderers;
        }

        public void Play(string _name) {
            foreach(Node node in runtimeNodes) {
                if(node.name == _name) {
                    //plays the animation on the node to keep track off next states
                    node.Enter();
                    break;
                }
            }
        }


        public void Stop() {
            SetSpeed(0);
        }

        public void SetSpeed(float _speed) {
            speed = _speed;

            int i = 0;

            foreach(MeshRenderer renderer in meshRenderers) {
                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(propertyBlock);

                //animation speed
                propertyBlock.SetFloat("_Speed", _speed);

                renderer.SetPropertyBlock(propertyBlock);

                i++;
            }
        }

        public float CalculateZeroReset(float _animationLength) {
            float dividedTime = (Time.time * speed) / _animationLength;

            float currentAnimationNormTime = dividedTime - Mathf.FloorToInt(dividedTime);

            return currentAnimationNormTime * _animationLength;
        }

        public void SetBool(string _name, bool _value) {
            foreach(Parameter parameter in parameters) {
                if(parameter.name == _name) {
                    parameter.bValue = _value;
                    break;
                }
            }
        }

        public void SetCurrentState(State _state) {
            currentState = _state;
            updateStarter(_state);
        }

        public void PlayAnimationNode(AIAnimation _animation, bool inheritBlendZeroReset = false) {

            float zeroReset = 0;

            if(inheritBlendZeroReset) {
                zeroReset = blendZeroReset;
            } else {
                //we need to calculate back what the shader is doing so we can start at time 0
                zeroReset = CalculateZeroReset(_animation.length);
            }

            Debug.Log(name);
            //Debug.Log("Play" + meshRenderers[0].transform.name);

            int i = 0;

            foreach(MeshRenderer renderer in meshRenderers) {
                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(propertyBlock);

                //animation duration
                propertyBlock.SetFloat("_AnimationDuration", _animation.length);

                //animation texture
                propertyBlock.SetTexture("_AnimationTexture", _animation.animationTexture);

                //max magnitude of this renderer
                propertyBlock.SetFloat("_MaxMagnitude", _animation.maxMagnitudes[i]);

                //current animation width
                propertyBlock.SetFloat("_AnimationWidth", _animation.animationTexture.width);

                //reset time
                propertyBlock.SetFloat("_AnimationZeroReset", zeroReset);

                propertyBlock.SetFloat("_LerpSpeed", 0);

                renderer.SetPropertyBlock(propertyBlock);

                i++;
            }

            animationZeroReset = zeroReset;
        }


        public void PlayTransition(Transition _transition, AIAnimation _endAnimation) {
            //we need to calculate back what the shader is doing so we can start at time 0
            blendZeroReset = CalculateZeroReset(_endAnimation.length);
            float lerpZeroReset = CalculateZeroReset(_transition.transitionTime);


            int i = 0;

            foreach(MeshRenderer renderer in meshRenderers) {
                //    renderer.GetComponent<MeshFilter>().sharedMesh = currentAnimation.startMeshes[i]; 

                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(propertyBlock);

                //animation texture
                propertyBlock.SetTexture("_BlendAnimationTexture", _endAnimation.animationTexture);

                //max magnitude of this renderer
                propertyBlock.SetFloat("_BlendMaxMagnitude", _endAnimation.maxMagnitudes[i]);

                //current animation width
                propertyBlock.SetFloat("_BlendAnimationWidth", _endAnimation.animationTexture.width);

                //reset time
                propertyBlock.SetFloat("_BlendZeroReset", blendZeroReset);

                //animation duration
                propertyBlock.SetFloat("_BlendDuration", _endAnimation.length);

                //reset time
                propertyBlock.SetFloat("_LerpZeroReset", lerpZeroReset);

                //animation duration
                propertyBlock.SetFloat("_LerpSpeed", _transition.transitionTime);

                renderer.SetPropertyBlock(propertyBlock);

                i++;
            }


        }
    }

}