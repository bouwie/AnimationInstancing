using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AnimationInstancing
{

    [System.Serializable]
    public class Parameter
    {

        public string name;
        public Type type;

        public enum Type
        {
            Boolean,
            Float,
            Int
        }

        public bool bValue;
        public float fValue;
        public int iValue;

        public Parameter(string _name, Type _type) {
            name = _name;
            type = _type;
        }

        public Parameter Clone() {
            Parameter clone = new Parameter(name, type);
            clone.bValue = bValue;
            clone.fValue = fValue;
            clone.iValue = iValue;

            return clone;
        }


        //Editor Stuff

        #if UNITY_EDITOR
        public virtual bool Draw() {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name);
            switch(type) {
                case Type.Boolean:
                bValue = EditorGUILayout.Toggle(bValue);
                break;

                case Type.Float:
                fValue = EditorGUILayout.FloatField(fValue);
                break;

                case Type.Int:
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
    }

}