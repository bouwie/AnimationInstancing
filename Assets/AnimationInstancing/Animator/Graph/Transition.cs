using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AnimationInstancing
{
    [System.Serializable]
    public class Transition : State
    {
        public AIAnimatorController myController;

        public int startId;
        public Node start {
            get {
                if(runtimeController != null) {
                    return runtimeController.GetRuntimeNode(startId);
                } else {
                    return myController.GetNode(startId);
                }
            }
        }
        public int endId;
        public Node end {
            get {
                if(runtimeController != null) {
                    return runtimeController.GetRuntimeNode(endId);
                } else {
                    return myController.GetNode(endId);
                }
            }
        }

        public bool hasExitTime;
        public float exitTime;
        public float transitionTime;
        public float offset;

        public List<Condition> conditions;

        [System.NonSerialized] private float startTime;

#if UNITY_EDITOR
        [System.NonSerialized] public AIAnimatorControllerWindow graph;
#endif  


        public Transition(AIAnimatorController _myController, int _start, int _end, float _offset) {
            myController = _myController;
            startId = _start;
            endId = _end;
            offset = _offset;
            conditions = new List<Condition>();
        }

        public Transition Clone() {
            Transition clone = new Transition(myController, startId, endId, offset);
            clone.hasExitTime = hasExitTime;
            clone.exitTime = exitTime;
            clone.transitionTime = transitionTime;

            foreach(Condition condition in conditions) {
                clone.conditions.Add(condition.Clone());
            }

            return clone;
        }

        public override void OnRuntimeInitialize(AIRuntimeAnimatorController _runtimeController) {
            base.OnRuntimeInitialize(_runtimeController);
            foreach(Condition condition in conditions) {
                condition.OnRuntimeInitialize(runtimeController);
            }
        }


        public void RemoveParameter(string _name) {
            for(int i = 0; i < conditions.Count; i++) {
                if(conditions[i].name == _name) {
                    conditions.RemoveAt(i);
                }
            }
        }
        //Editor Stuff
#if UNITY_EDITOR
        public void OnEditorWindowOpen(AIAnimatorControllerWindow _graph) {
            graph = _graph;
            myController = _graph.controller;
        }

        public void Draw() {
            Vector3[] bezierPoints = DrawTransition(myController.GetNode(startId).windowRect, myController.GetNode(endId).windowRect, offset);

            for(int i = 0; i < bezierPoints.Length - 1; i++) {
                Vector3 dir = (bezierPoints[i + 1] - bezierPoints[i]).normalized;

                DrawArrowEnd(bezierPoints[i], dir, Color.white, 12, 20);
            }

            if(Event.current.type == EventType.MouseDown) {
                if(Event.current.button == 0) {
                    CheckForClick(bezierPoints, 25);
                }
            }
        }

        private void DrawArrowEnd(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f) {
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(arrowHeadAngle, 0, 0) * Vector3.back;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(-arrowHeadAngle, 0, 0) * Vector3.back;
            Vector3 up = Quaternion.LookRotation(direction) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back;
            Vector3 down = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back;

            Handles.DrawLine(pos + direction, pos + direction + right * arrowHeadLength);
            Handles.DrawLine(pos + direction, pos + direction + left * arrowHeadLength);
            Handles.DrawLine(pos + direction, pos + direction + up * arrowHeadLength);
            Handles.DrawLine(pos + direction, pos + direction + down * arrowHeadLength);

        }


        private void CheckForClick(Vector3[] _bezierPoints, float _radius) {
            Vector2 mousePos = Event.current.mousePosition;

            for(int i = 0; i < _bezierPoints.Length; i++) {
                Vector2 bezierPoint = new Vector2(_bezierPoints[i].x, _bezierPoints[i].y);
                float dist = Vector2.Distance(mousePos, bezierPoint);
                if(dist < _radius) {
                    OnSelect();
                    break;
                }
            }
        }

        public override void OnSelect() {
            base.OnSelect();

            TransitionWindow transitionWindow = new TransitionWindow(new Rect(0, 0, 0, 0), graph);
            graph.RemoveAllWindows<TransitionWindow>();
            transitionWindow.SetTarget(this);
            graph.AddWindow(transitionWindow);
        }

        public override void OnUnSelect() {
            graph.RemoveAllWindows<TransitionWindow>();
        }

        public static Vector3[] DrawTransition(Rect _startRect, Rect _endRect, float _offset) {

            Vector3 startPos = FindStartPos(_startRect, _endRect, _offset);
            Vector3 endPos = FindStartPos(_endRect, _startRect, _offset);

            Vector3 startCenter = new Vector3(_startRect.x + (_startRect.width / 2), _startRect.y + (_startRect.height / 2));
            Vector3 endCenter = new Vector3(_endRect.x + (_endRect.width / 2), _endRect.y + (_endRect.height / 2));

            Vector3 startTan = startPos + (startPos - startCenter).normalized * 50;
            Vector3 endTan = endPos + (endPos - endCenter).normalized * 50;
            // Color shadowCol = new Color(0, 0, 0, 0.06f);
            Color darkGray = new Color(0, 0, 0, 0.6f);
            for(int i = 0; i < 3; i++) // Draw a shadow            
                                       //  Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
                                       //     Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.g, null, 4);
                Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.white, null, 3);
            return Handles.MakeBezierPoints(startPos, endPos, startTan, endTan, 10);
        }

        private static Vector3 FindStartPos(Rect _start, Rect _end, float offset) {

            //right
            if(_end.x > (_start.x + _start.width)) {
                return new Vector3(_start.x + _start.width, _start.y + _start.height / 2 + offset, 0);
            }     //left
            else if(_end.x + (_end.width) < _start.x) {
                return new Vector3(_start.x, _start.y + _start.height / 2 + offset, 0);
            } else {
                //down
                if(_end.y > _start.y) {
                    return new Vector3(_start.x + (_start.width / 2) + offset, _start.y + _start.height, 0);
                }
                //up

                else {
                    return new Vector3(_start.x + (_start.width / 2) + offset, _start.y, 0);
                }
            }


        }
        #endif

        //Logic
        public bool MeetsRequirements() {
            foreach(Condition condition in conditions) {
                if(!condition.IsTrue()) {
                    return false;
                }
            }
            return true;
        }

        public bool ContainsCondition(string _name) {
            foreach(Condition condition in conditions) {
                if(condition.name == _name) {
                    return true;
                }
            }
            return false;
        }

        public override void Enter() {
            startTime = Time.time;
            runtimeController.PlayTransition(this, end.FetchLastAnimation(), end.FetchLastPlaybackSpeed());
            runtimeController.SetCurrentState(this);
        }

        //void Update() {
        //    float endTime = (startTime + transitionTime);

        //    if(Time.time >= endTime) {
        //        Exit();
        //    }
        //}

        public override IEnumerator Update() {
            yield return new WaitForSeconds(transitionTime);
            //are we at the end of the transition?
                Exit();         
        }

        public override void Exit() {
            end.Enter();
        }

        public override void Delete() {
            base.Delete();
            if(start != null) {
                start.RemoveTransition(this);
            }
        }
    }

}