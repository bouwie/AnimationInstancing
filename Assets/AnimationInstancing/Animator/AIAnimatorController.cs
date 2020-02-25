using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AnimationInstancing
{

    [CreateAssetMenu(fileName = "AIAnimatorController", menuName = "Animation Instancing/AIAnimatorController", order = 1)]
    public class AIAnimatorController : ScriptableObject
    {
        protected void Clone(AIAnimatorController _target) {
            this.nodes = new List<Node>(_target.nodes.Count);
            this.animNodeCount = _target.animNodeCount;
            this.parameters = new List<Parameter>(_target.parameters);
            foreach(Node node in _target.nodes) {
                this.nodes.Add((Node)node.Clone());
            }
        }

        [SerializeReference]
        public List<Node> nodes = new List<Node>();
        public int animNodeCount = 0;
        public State selected;
        //   public Transition selectedTransition;

        public float panX = 0;
        public float panY = 0;

        public float zoom = 0;

        //Parameters
        public List<Parameter> parameters = new List<Parameter>();


        public string[] GetParameterNames() {
            List<string> stringList = new List<string>();

            foreach(Parameter parameter in parameters) {
                stringList.Add(parameter.name);
            }

            return stringList.ToArray();
        }
        public Parameter GetParameter(string _name) {
            foreach(Parameter parameter in parameters) {
                if(parameter.name == _name) {
                    return parameter;
                }
            }
            return null;
        }

        public void RemoveParameter(string _name) {
            foreach(Node node in nodes) {
                node.RemoveParameter(_name);
            }
        }

        public bool HasParameter(string _name) {
            foreach(Parameter parameter in parameters) {
                if(parameter.name == _name) {
                    return true;
                }
            }
            return false;
        }

        //Functionality
        public Node GetNode(string _name) {
            foreach(Node node in nodes) {
                if(node.name == _name) {
                    return node;
                }

            }
            return null;
        }

        public void RemoveNode(Node _node) {
            foreach(Node node in nodes) {
                for(int i = 0; i < node.transitions.Count; i++) {
                    if(node.transitions[i].end == _node) {
                        node.RemoveTransition(node.transitions[i]);
                    }
                }
            }
            nodes.Remove(_node);
        }

        public Node GetNode(int _id) {
            for(int i = 0; i < nodes.Count; i++) {
                if(nodes[i].id == _id) {
                    return nodes[i];
                }
            }
            return null;
        }

        //Editor Stuff
#if UNITY_EDITOR
        public void OnEditorWindowOpen(AIAnimatorControllerWindow _graph) {
            foreach(Node node in nodes) {
                node.OnEditorWindowOpen(_graph);
            }
        }
#endif


    }

}
