using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;

namespace AnimationInstancing
{

    public class AnimationNodeWindow : Window
    {
        private AnimationNode targetNode;
        private AIAnimatorControllerWindow graph;

        public AnimationNodeWindow(Rect _windowRect, AIAnimatorControllerWindow _graph) : base(_windowRect) {
            graph = _graph;
        }

        public void SetTarget(AnimationNode _target) {
            targetNode = _target;
        }

        public override void Draw() {
            float scale = 4.5f;

            float width = graph.position.width / scale;
            float heigth = graph.position.height / scale;

            windowRect = new Rect(graph.controller.panX + graph.position.width - width, graph.controller.panY, width, heigth);
            windowRect = GUI.Window(-2, windowRect, Contents, targetNode.GetType().Name);
        }

        public override void Contents(int _id) {
            GUILayout.Label(targetNode.name);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Name: ");
            targetNode.name = EditorGUILayout.TextField(targetNode.name);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Loop:");
            targetNode.loop = GUILayout.Toggle(targetNode.loop, "");
            GUILayout.EndHorizontal();
            ////did we select a animationNode?
            //AnimationNode animationNode = controller.selectedNode as AnimationNode;

            //if(animationNode != null) {
            //    animationNode.animation = (AIAnimation)EditorGUILayout.ObjectField("Animation: ", animationNode.animation, typeof(AIAnimation));
            //}

            //did we select a randomAnimationNode?


            GUILayout.Label("Transitions:");
            for(int i = 0; i < targetNode.transitions.Count; i++) {
                GUILayout.Label("from: " + targetNode.transitions[i].start.name + " to: " + targetNode.transitions[i].end.name);
            }
        }
    }
}
#endif
