using System.Collections;
using System.Collections.Generic;
using UnityEngine;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AnimationInstancing
{
    public class ParametersWindow : Window
    {
        private AIAnimatorControllerWindow graph;

        private string newParameterName;
        private Parameter.Type newParamterType;

        public ParametersWindow(Rect _windowRect, AIAnimatorControllerWindow _graph) : base(_windowRect) {
            graph = _graph;
        }

        public override void Draw() {
            float scale = 4.5f;

            float width = graph.position.width / scale;
            float heigth = graph.position.height / scale;

            Rect windowRect = new Rect(graph.controller.panX, graph.controller.panY, width, heigth);
            windowRect = GUI.Window(-3, windowRect, Contents, "Parameters");
        }


        public override void Contents(int _id) {
            for(int i = 0; i < graph.controller.parameters.Count; i++) {
                if(!graph.controller.parameters[i].Draw()) {
                    graph.controller.RemoveParameter(graph.controller.parameters[i].name);
                    graph.controller.parameters.RemoveAt(i);
                }
            }
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            newParameterName = EditorGUILayout.TextField("Parameter name: ", newParameterName);
            newParamterType = (Parameter.Type)EditorGUILayout.EnumPopup(newParamterType);

            EditorGUI.BeginDisabledGroup(graph.controller.HasParameter(newParameterName));
            if(GUILayout.Button("Add")) {
                graph.controller.parameters.Add(new Parameter(newParameterName, newParamterType));
                newParameterName = string.Empty;
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
        }
    }
}
