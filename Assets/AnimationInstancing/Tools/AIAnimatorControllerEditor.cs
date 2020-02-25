using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace AnimationInstancing
{

    [CustomEditor(typeof(AIAnimatorController))]
    public class AIAnimatorControllerEditor : Editor
    {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
        }

        void OnEnable() {
            AIAnimatorController controller = (AIAnimatorController)target;
            AIAnimatorControllerWindow.ShowEditor((controller));
        }
    }

}
#endif
