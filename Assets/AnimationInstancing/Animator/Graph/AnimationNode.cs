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
        public float playbackSpeed = 1;
        public bool loop;

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
            clone.playbackSpeed = playbackSpeed;
            clone.loop = loop;

            foreach(Transition transition in transitions) {
                clone.transitions.Add(transition.Clone());
            }

            clone.animation = animation;

            return clone;
        }



        //Logic
        public override void Enter() {
            startTime = Time.time;

          //  float endTime = (startTime + (animation.length / playbackSpeed));
        //    Debug.Log(name + " enter: " + startTime + "   endTime: " + endTime);

            runtimeController.SetCurrentState(this);
            runtimeController.PlayAnimationNode(animation, playbackSpeed,true);

        }

        public override IEnumerator Update() {
            float endTime = (startTime + (animation.length / playbackSpeed));

            //are we at the end of the animation?
            bool hasTransitioned = false;

            while(Time.time < endTime && !hasTransitioned) {
                float normTime = (startTime - Time.time) / endTime;

                //check if we can transition
                foreach(Transition transition in transitions) {
                    if(transition.hasExitTime) {
                        if(normTime >= transition.exitTime) {
                            if(transition.MeetsRequirements()) {
                                //Start transition
                                transition.Enter();
                                hasTransitioned = true;
                                break;
                            }
                        }
                    } else {
                        if(transition.MeetsRequirements()) {
                            //Start transition
                            transition.Enter();
                            hasTransitioned = true;
                            break;
                        }
                    }
                }
                yield return new WaitForEndOfFrame();
            }
            //only stop playing if we could not enter a transition
            if(!hasTransitioned) {
                Exit();
            }
        }

        public override void Exit() {
            //check if we can transition one last time
            foreach(Transition transition in transitions) { 
                    if(transition.MeetsRequirements()) {
                        //Start transition
                        transition.Enter();
                        return;
                    }
                
            }

            if(!loop) {
                runtimeController.Stop();
            }
        }

        public override AIAnimation FetchLastAnimation() {
            return animation;
        }

        public override float FetchLastPlaybackSpeed() {
            return playbackSpeed;
        }
    }

}