using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AnimationInstancing
{
    [System.Serializable]
    public class AnimationNode : Node
    {
        public AIAnimation animation;

        [System.NonSerialized] private float startTime;

        private Vector2 diffPos;

        public AnimationNode(Vector2 _pos, int _id) : base(_pos, _id) {

        }


        //Editor Stuff
        #if UNITY_EDITOR
        public override void Draw() {
            //zoomAppliedRect.x += graph.controller.zoom;
            //zoomAppliedRect.y += graph.controller.zoom;
            //zoomAppliedRect.width = width + graph.controller.zoom;
            //zoomAppliedRect.height = height + graph.controller.zoom;

            //appply zoom
            GUIStyle style = GUI.skin.window;            
            style.fontSize = graph.fontSize;

        //    windowRect.position = new Vector2(x + diffPos.x + graph.controller.zoom,y + diffPos.y + graph.controller.zoom);

            windowRect = GUI.Window(id, windowRect, DrawWindowContents, name, style);

       //     diffPos = new Vector2(x - windowRect.x, y - windowRect.y);

            for(int i = 0; i < transitions.Count; i++) {
                transitions[i].Draw();
            }
        }
        protected override void DrawWindowContents(int _id) {
           GUIStyle style = GUI.skin.label;
            style.fontSize = graph.fontSize;



            GUILayout.Space(Mathf.Max(0, graph.controller.zoom));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Animation: ", style);
                  animation = (AIAnimation)EditorGUILayout.ObjectField(animation, typeof(AIAnimation), false, GUILayout.ExpandHeight(true), GUILayout.MaxHeight(Mathf.Max(20, graph.controller.zoom)));
            GUILayout.EndHorizontal();
        
            base.DrawWindowContents(_id);
        }

        private void StartConnecting() {
            graph.StartConnecting(this);
        }

        public override void OnSelect() {
            base.OnSelect();

            AnimationNodeWindow nodeWindow = new AnimationNodeWindow(new Rect(0, 0, 0, 0), graph);
            graph.RemoveAllWindows<AnimationNodeWindow>();
            nodeWindow.SetTarget(this);
            graph.AddWindow(nodeWindow);
        }

        public override void OnUnSelect() {
            graph.RemoveAllWindows<AnimationNodeWindow>();
        }
        #endif



        public override object Clone() {
            AnimationNode clone = new AnimationNode(new Vector2(windowRect.x, windowRect.y), id);
            clone.name = name;

            foreach(Transition transition in transitions) {
                clone.transitions.Add(transition.Clone());
            }

            clone.animation = animation;

            return clone;
        }



        //Logic
        public override void Enter() {
            runtimeController.SetCurrentState(this);
            runtimeController.PlayAnimationNode(animation, true);
            startTime = Time.time;
        }

        public override IEnumerator Update() {
            float endTime = (startTime + animation.length);

            //are we at the end of the animation?
            while(Time.time < endTime) {
                float normTime = Time.time / endTime;

                //check if we can transition
                foreach(Transition transition in transitions) {
                    if(transition.hasExitTime) {
                        if(normTime >= transition.exitTime) {
                            if(transition.MeetsRequirements()) {
                                //Start transition
                                Exit();
                                transition.Enter();
                                break;
                            }
                        }
                    } else {
                        if(transition.MeetsRequirements()) {
                            //Start transition
                            Exit();
                            transition.Enter();
                            break;
                        }
                    }
                }
                yield return new WaitForEndOfFrame();
            }
            Exit();
        }

        //public override void Update() {
        //    //are we at the end of the animation?
        //    float endTime = (startTime + animation.length);

        //    if(Time.time >= endTime) {
        //        Exit();
        //    }

        //    float normTime = Time.time / endTime;

        //    //check if we can transition
        //    foreach(Transition transition in transitions) {
        //        if(transition.hasExitTime) {
        //            if(normTime >= transition.exitTime) {
        //                if(transition.MeetsRequirements()) {
        //                    //Start transition
        //                    Exit();
        //                    transition.Enter();
        //                    break;
        //                }
        //            }
        //        } else {
        //            if(transition.MeetsRequirements()) {
        //                //Start transition
        //                Exit();
        //                transition.Enter();
        //                break;
        //            }
        //        }
        //    }
        //}

        public override void Exit() {

        }

        public override AIAnimation FetchLastAnimation() {
            return animation;
        }
    }

}