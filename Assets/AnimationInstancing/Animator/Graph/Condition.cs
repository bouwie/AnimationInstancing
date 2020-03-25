using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AnimationInstancing
{
    [System.Serializable]
    public class Condition : Parameter
    {
        public Parameter targetParameter;
        public FConditions fCondition;
        public IConditions iCondition;

        public Condition(string _name, Type _type) : base(_name, _type) {

        }

        public new Condition Clone() {
            Condition clone = new Condition(name, type);
            clone.targetParameter = targetParameter.Clone();
            clone.fCondition = fCondition;
            clone.fValue = fValue;

            clone.bValue = bValue;

            clone.iCondition = iCondition;
            clone.iValue = iValue;

            return clone;
        }

        public void OnRuntimeInitialize(AIRuntimeAnimatorController _runTimeController) {
            foreach(Parameter parameter in _runTimeController.parameters) {
                if(parameter.name == name) {
                    targetParameter = parameter;
                    return;
                }
            }
            Debug.LogError("Parameter with name " + name + " is missing in the AIAnimator");
        }

        public enum FConditions
        {
            GreaterThan,
            SmallerThan,
            Equal
        }

        public enum IConditions
        {
            GreaterThan,
            SmallerThan,
            Equal
        }

        //Editor Stuff

#if UNITY_EDITOR
        public override bool Draw() {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name);
            switch(type) {
                case Type.Boolean:
                bValue = EditorGUILayout.Toggle(bValue);
                break;

                case Type.Float:
                fCondition = (FConditions)EditorGUILayout.EnumPopup(fCondition);
                fValue = EditorGUILayout.FloatField(fValue);
                break;

                case Type.Int:
                iCondition = (IConditions)EditorGUILayout.EnumPopup(iCondition);
                iValue = EditorGUILayout.IntField(iValue);
                break;
            }

            if(GUILayout.Button("Delete")) {
                return false;
            }
            GUILayout.EndHorizontal();
            return true;
        }
        #endif

        public bool IsTrue() {
            switch(type) {
                case Type.Boolean:
                return targetParameter.bValue == bValue;

                case Type.Float:
                switch(fCondition) {
                    case FConditions.Equal:
                    return targetParameter.fValue == fValue;

                    case FConditions.GreaterThan:
                    return targetParameter.fValue > fValue;

                    case FConditions.SmallerThan:
                    return targetParameter.fValue < fValue;
                }
                break;


                case Type.Int:
                switch(iCondition) {
                    case IConditions.Equal:
                    return targetParameter.iValue == iValue;

                    case IConditions.GreaterThan:
                    return targetParameter.iValue > iValue;

                    case IConditions.SmallerThan:
                    return targetParameter.iValue < iValue;
                }
                break;
            }
            return false;
        }

    }

}