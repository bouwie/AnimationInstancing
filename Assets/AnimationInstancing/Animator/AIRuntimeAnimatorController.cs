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

        [System.NonSerialized]
        public List<Node> runtimeNodes;

        public delegate void UpdateStarter(State _state);
        private UpdateStarter updateStarter;

        public void Initalize(AIAnimatorController _copy, MeshRenderer[] _meshRenderers, UpdateStarter _updateStarter) {
            entryNodeId = _copy.entryNodeId;
            runtimeNodes = new List<Node>();
            nodes = _copy.nodes;

            foreach(Node node in _copy.nodes) {
                runtimeNodes.Add((Node)node.Clone());
            }

            meshRenderers = _meshRenderers;
            updateStarter = _updateStarter;

            foreach(Node animationNode in runtimeNodes) {
                animationNode.OnRuntimeInitialize(this);
            }
        
        }

        public Node GetRuntimeNode(string _name) {
            foreach(Node node in runtimeNodes) {
                if(node.name == _name) {
                    return node;
                }

            }
            return null;
        }

        public Node GetRuntimeNode(int _id) {
            for(int i = 0; i < runtimeNodes.Count; i++) {
                if(runtimeNodes[i].id == _id) {
                    return runtimeNodes[i];
                }
            }
            Debug.LogError("Node Not Found with id" + _id);
            return null;
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

        public void Entry() {
            SetSpeed(1);

            foreach(Node node in runtimeNodes) {
                if(node.id == entryNodeId) {
                    //plays the entry animation
                    node.Enter();
                    break;
                }
            }

        }


        public void Stop() {
            SetSpeed(0);
        }

        public void SetSpeed(float _speed) {

            int i = 0;

            foreach(MeshRenderer renderer in meshRenderers) {
                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(propertyBlock);

                //animation speed
                propertyBlock.SetFloat("_AnimationSpeed", _speed);

                renderer.SetPropertyBlock(propertyBlock);

                i++;
            }
        }

        public float CalculateZeroReset(float _animationLength, float _animationSpeed) {
            float dividedTime = (Time.time * _animationSpeed) / _animationLength;

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

        public void PlayAnimationNode(AIAnimation _animation, float _speed, bool inheritBlendZeroReset = false) {

            float zeroReset = 0;

            if(inheritBlendZeroReset) {
                zeroReset = blendZeroReset;
            } else {
                //we need to calculate back what the shader is doing so we can start at time 0
                zeroReset = CalculateZeroReset(_animation.length, _speed);
            }

            //Debug.Log("Play" + meshRenderers[0].transform.name);

            int i = 0;

            foreach(MeshRenderer renderer in meshRenderers) {
                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(propertyBlock);

                //animation duration
                propertyBlock.SetFloat("_AnimationDuration", _animation.length);

                //animation texture
                propertyBlock.SetTexture("_AnimationTexture", _animation.animationTexture);

                //animation speed
                propertyBlock.SetFloat("_AnimationSpeed", _speed);

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


        public void PlayTransition(Transition _transition, AIAnimation _endAnimation, float _endSpeed) {
            //we need to calculate back what the shader is doing so we can start at time 0
            blendZeroReset = CalculateZeroReset(_endAnimation.length, _endSpeed);
            float lerpZeroReset = CalculateZeroReset(_transition.transitionTime, 1);


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

                //animation speed
                propertyBlock.SetFloat("_BlendAnimationSpeed", _endSpeed);

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