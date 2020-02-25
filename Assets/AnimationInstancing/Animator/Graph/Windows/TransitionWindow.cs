using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace AnimationInstancing
{

    public class TransitionWindow : Window
    {
        private Transition targetTransition;
        private AIAnimatorControllerWindow graph;
        private int conditionIndex;

        public TransitionWindow(Rect _windowRect, AIAnimatorControllerWindow _graph) : base(_windowRect) {
            graph = _graph;
        }

        public void SetTarget(Transition _target) {
            targetTransition = _target;
        }

        public override void Draw() {
            float scale = 4.5f;

            float width = graph.position.width / scale;
            float heigth = graph.position.height / scale;

            windowRect = new Rect(graph.controller.panX + graph.position.width - width, graph.controller.panY, width, heigth);
            windowRect = GUI.Window(-2, windowRect, Contents, "Transition");
        }

        public override void Contents(int _id) {
            if(targetTransition == null) {
                return;
            }

            GUILayout.Label("from: " + targetTransition.start.name + " to: " + targetTransition.end.name);
            targetTransition.hasExitTime = GUILayout.Toggle(targetTransition.hasExitTime, "has exit time");
            if(targetTransition.hasExitTime) {
                GUILayout.BeginHorizontal();
                GUILayout.Label("exit time: ");
                targetTransition.exitTime = EditorGUILayout.FloatField(targetTransition.exitTime);
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("transiton time: ");
            targetTransition.transitionTime = EditorGUILayout.FloatField(targetTransition.transitionTime);
            GUILayout.EndHorizontal();


            GUILayout.Space(20);
            GUILayout.Label("conditions:");
            for(int i = 0; i < targetTransition.conditions.Count; i++) {
                if(!targetTransition.conditions[i].Draw()) {
                    targetTransition.conditions.RemoveAt(i);
                }

            }

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            string[] parameterNames = graph.controller.GetParameterNames();

            if(parameterNames.Length > 0) {
                conditionIndex = EditorGUILayout.Popup(conditionIndex, parameterNames);

                EditorGUI.BeginDisabledGroup(targetTransition.ContainsCondition(parameterNames[conditionIndex]));
                if(GUILayout.Button("Add")) {
                    Parameter parameter = graph.controller.GetParameter(parameterNames[conditionIndex]);

                    targetTransition.conditions.Add(new Condition(parameter.name, parameter.type));
                }
                EditorGUI.EndDisabledGroup();
            }

            GUILayout.EndHorizontal();
        }
    }
}
#endif