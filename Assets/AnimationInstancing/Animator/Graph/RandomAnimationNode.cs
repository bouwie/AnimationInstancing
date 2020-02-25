using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AnimationInstancing
{
    [System.Serializable]
    public class RandomAnimationNode : AnimationNode
    {
        public List<AIAnimation> animations = new List<AIAnimation>();
        public RandomAnimationNode(Vector2 _pos, int _id) : base(_pos, _id) {

        }

        public override object Clone() {
            RandomAnimationNode clone = new RandomAnimationNode(new Vector2(windowRect.x, windowRect.y), id);
            clone.name = name;
            clone.animations = animations;

            foreach(Transition transition in transitions) {
                clone.transitions.Add(transition.Clone());
            }

            clone.animation = animation;

            return clone;
        }

        public override AIAnimation FetchLastAnimation() {
            return animation;
        }

        
        protected override void DrawWindowContents(int _id) {

            int newCount = Mathf.Max(0, EditorGUILayout.DelayedIntField("size", animations.Count));
            while(newCount < animations.Count)
                animations.RemoveAt(animations.Count - 1);
            while(newCount > animations.Count)
                animations.Add(null);

            for(int i = 0; i < animations.Count; i++) {
                animations[i] = (AIAnimation)EditorGUILayout.ObjectField(animations[i], typeof(AIAnimation));
            }

            windowRect.height = 75 + (animations.Count * 20);

            base.DrawWindowContents(_id);
        }

        //Logic
        public override void Enter() {
            if(animations.Count > 0) {
                int randomInt = Random.Range(0, animations.Count - 1);
                animation = animations[randomInt];
            }
            base.Enter();
        }
  

        public override void Exit() {

        }
    }

}