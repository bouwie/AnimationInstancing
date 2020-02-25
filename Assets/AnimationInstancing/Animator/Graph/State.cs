using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnimationInstancing
{
    [System.Serializable]
    public class State
    {
        [System.NonSerialized] public AIRuntimeAnimatorController runtimeController;

        public virtual void OnRuntimeInitialize(AIRuntimeAnimatorController _runtimeController) {
            runtimeController = _runtimeController;
        }

        public virtual void OnSelect() {

        }

        public virtual void OnUnSelect() {

        }

        public virtual void Enter() {

        }

        public virtual IEnumerator Update() {
            yield return null;
        }

        public virtual void Exit() {

        }

        public virtual void Delete() {
            OnUnSelect();
        }

    }
}
