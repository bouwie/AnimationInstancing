using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AnimationInstancing
{
    [System.Serializable]
    public class Node : State
    {
#if UNITY_EDITOR
        [System.NonSerialized] public AIAnimatorControllerWindow graph;
#endif

        public string name;
        public Rect windowRect;
        public int id;
        public List<Transition> transitions;

        protected float x;
        protected float y;
        protected float width = 200;
        protected float height = 75;

        [System.NonSerialized] public bool containsMouse;

        public Node(Vector2 _pos, int _id) {
            x = _pos.x;
            y = _pos.y;
            id = _id;
            windowRect = new Rect(_pos.x, _pos.y, width, height);
            transitions = new List<Transition>();
            name = "Empty" + _id;
        }

        public virtual object Clone() {
            Node clone = new Node(new Vector2(windowRect.x, windowRect.y), id);
            clone.name = name;

            foreach(Transition transition in transitions) {
                clone.transitions.Add(transition.Clone());
            }

            return clone;
        }

        public virtual AIAnimation FetchLastAnimation() {
            return null;
        }

        public override void OnRuntimeInitialize(AIRuntimeAnimatorController _runtimeController) {
            base.OnRuntimeInitialize(_runtimeController);
            foreach(Transition transition in transitions) {
                transition.OnRuntimeInitialize(_runtimeController);
            }
        }

#if UNITY_EDITOR
        public void OnEditorWindowOpen(AIAnimatorControllerWindow _graph) {
            graph = _graph;
            foreach(Transition transition in transitions) {
                transition.OnEditorWindowOpen(_graph);
            }
        }
#endif

        public bool HasConnectionTo(Node _target) {
            for(int i = 0; i < transitions.Count; i++) {
                if(transitions[i].endId == _target.id) {
                    return true;
                }
            }
            return false;
        }

        public void RemoveTransition(Transition _transition) {
            transitions.Remove(_transition);
        }

        public void RemoveParameter(string _name) {
            foreach(Transition transition in transitions) {
                transition.RemoveParameter(_name);
            }
        }

        //Editor Stuff

        #if UNITY_EDITOR
        public virtual void Draw() {
   
           windowRect = GUI.Window(id, windowRect, DrawWindowContents, name);

            for(int i = 0; i < transitions.Count; i++) {
                transitions[i].Draw();
            }
        }

        protected virtual void DrawWindowContents(int _id) {
            if(containsMouse) {

                if(Event.current.type == EventType.MouseDown) {
                    if(Event.current.button == 1) {
                        // create the menu and add items to it
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Connect"), false, StartConnecting);

                        // display the menu
                        menu.ShowAsContext();
                    } else if(Event.current.button == 0) {
                        OnSelect();
                    }
                }
            }

            GUI.DragWindow();
        }

        public override void OnSelect() {
            if(graph.isConnecting) {
                graph.EndConnecting(this);
            }
            graph.SetSelected(this);
        }

        private void StartConnecting() {
            graph.StartConnecting(this);
        }

        public override void Delete() {
            base.Delete();
            graph.controller.RemoveNode(this);
        }
#endif

        //Logic
        public override void Enter() {

        }


        public override void Exit() {

        }

    }
}
