using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AICreateAnimations))]
public class AICreateAnimationsEditor : Editor
{
    public override void OnInspectorGUI() {
        AICreateAnimations createAnimations = (AICreateAnimations)target;


        SerializedObject so = new SerializedObject(target);
        SerializedProperty meshProperty = so.FindProperty("meshesCreated");
        EditorGUILayout.PropertyField(meshProperty, true);
        so.ApplyModifiedProperties();

        GUILayout.Space(10);

        GUILayout.Label("Save Directory: " + createAnimations.saveDirectory);
        if(GUILayout.Button("Set Save Directory")) {
            string saveDir = EditorUtility.OpenFolderPanel("Set Save Directory", System.Environment.CurrentDirectory, "");
            if(saveDir != string.Empty) {
                createAnimations.saveDirectory = saveDir;
            }
        }
        GUILayout.Space(5);

        createAnimations.saveMode = (AICreateAnimations.SaveMode)EditorGUILayout.EnumPopup("Save Mode:", createAnimations.saveMode);


        switch(createAnimations.saveMode) {
            case AICreateAnimations.SaveMode.SingleAnimation:
            createAnimations.animationForSave[0] = EditorGUILayout.TextField("Animation Name: ", createAnimations.animationForSave[0]);
            createAnimations.clipLenght = EditorGUILayout.FloatField("clipLenght: ",createAnimations.clipLenght);
            break;

            case AICreateAnimations.SaveMode.Multiple:
            SerializedProperty stringsProperty = so.FindProperty("animationForSave");
            EditorGUILayout.PropertyField(stringsProperty, true); // True means show children

            if(createAnimations.animationForSave.Length <= 0) {
                createAnimations.animationForSave = new string[1];
            }

            so.ApplyModifiedProperties(); // Remember to apply modified properties
            break;
        }

        GUILayout.Space(20);


        EditorGUI.BeginDisabledGroup(!createAnimations.canCreate);
        if(!createAnimations.canCreate) {
            GUILayout.Label("Can only bake in playmode");
        }
        if(GUILayout.Button("Create")) {
            if(createAnimations.saveMode == AICreateAnimations.SaveMode.All) {
                createAnimations.SetAllAnimations();
            }
            createAnimations.GenerateAnimations();
        }
        EditorGUI.EndDisabledGroup();
    }
}
