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
            Float
        }

        public bool bValue;
        public float fValue;

        public Parameter(string _name, Type _type) {
            name = _name;
            type = _type;
        }

        public Parameter Clone() {
            Parameter clone = new Parameter(name, type);
            clone.bValue = bValue;
            clone.fValue = fValue;

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