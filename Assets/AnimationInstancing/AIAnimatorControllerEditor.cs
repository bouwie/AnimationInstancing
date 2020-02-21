using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
