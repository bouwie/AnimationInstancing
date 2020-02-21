using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "AIAnimatorController", menuName = "Animation Instancing/AIAnimatorController", order = 1)]
public class AIAnimatorController : ScriptableObject
{
    protected void DeepCopy(AIAnimatorController _target) {
        this.nodes = _target.nodes;
        this.animNodeCount = _target.animNodeCount;
        this.parameters = _target.parameters;
    }

    [SerializeReference]
    public List<Node> nodes = new List<Node>();
    public int animNodeCount = 0;
    public Node selectedNode;
    public Transition selectedTransition;

    public float panX = 0;
    public float panY = 0;

    //Parameters
    public List<Parameter> parameters = new List<Parameter>();


    public string[] GetParameterNames() {
        List<string> stringList = new List<string>();

        foreach(Parameter parameter in parameters) {
            stringList.Add(parameter.name);
        }

        return stringList.ToArray();
    }
    public Parameter GetParameter(string _name) {
        foreach(Parameter parameter in parameters) {
            if(parameter.name == _name) {
                return parameter;
            }
        }
        return null;
    }

    public void RemoveParameter(string _name) {
        foreach(Node node in nodes) {
            node.RemoveParameter(_name);
        }
    }

    public bool HasParameter(string _name) {
        foreach(Parameter parameter in parameters) {
            if(parameter.name == _name) {
                return true;
            }
        }
        return false;
    }

    //Functionality
    public Node GetNode(string _name) {
        foreach(Node node in nodes) {
            if(node.name == _name) {
                return node;
            }
                
        }
        return null;
        }

    public Node GetNode(int _id) {
        for(int i  =0; i < nodes.Count; i++) {
            if(nodes[i].id == _id) {
                return nodes[i];
            }
        }
        return null;
    }

    //Editor Stuff
    public void OnEditorWindowOpen(AIAnimatorControllerWindow _graph) {
        foreach(Node node in nodes) {
           node.OnEditorWindowOpen(_graph);
        }
    }

    [System.Serializable]
    public class State {
        [System.NonSerialized] public AIRuntimeAnimatorController runtimeController;

        public virtual void OnRuntimeInitialize(AIRuntimeAnimatorController _runtimeController) {
            runtimeController = _runtimeController;
        }

        public virtual void Enter() {

        }

        public virtual void Update() {

        }

        public virtual void Exit() {

        }

    }


    [System.Serializable]
    public class Node : State
    {
        public AIAnimatorControllerWindow graph;

        public string name;
        public Rect windowRect;
        public int id;
        public List<Transition> transitions;

        [System.NonSerialized] public bool containsMouse;


        public Node(Vector2 _pos, int _id) {
            id = _id;
            windowRect = new Rect(_pos.x, _pos.y, 200, 75);
            transitions = new List<Transition>();
            name = "Empty" + _id;
        }

        public virtual AIAnimation FetchLastAnimation() {
            return null;
        }

        public override void OnRuntimeInitialize(AIRuntimeAnimatorController _runtimeController) {
            base.OnRuntimeInitialize(_runtimeController);
            foreach(Transition transition in transitions) {
                transition.OnRuntimeInitialize(_runtimeController);
            }
        }

        public void OnEditorWindowOpen(AIAnimatorControllerWindow _graph) {
            graph = _graph;
            foreach(Transition transition in transitions) {
                transition.OnEditorWindowOpen(_graph);
            }
        }

        public bool HasConnectionTo(Node _target) {
            for(int i = 0; i < transitions.Count; i++) {
                if(transitions[i].endId == _target.id) {
                    return true;
                }
            }
            return false;
        }

        public void RemoveTransition(Transition _transition) {
            transitions.Remove(_transition);
        }

        public void RemoveParameter(string _name) {
            foreach(Transition transition in transitions) {
                transition.RemoveParameter(_name);
            }
        }

        public void Draw() {
            windowRect = GUI.Window(id, windowRect, DrawWindowContents, name);

            for(int i = 0; i < transitions.Count; i++) {
                transitions[i].Draw();
            }
        }

        protected virtual void DrawWindowContents(int _id) {
            if(containsMouse) {

                if(Event.current.type == EventType.MouseDown) {
                    if(Event.current.button == 1) {
                        // create the menu and add items to it
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Connect"), false, StartConnecting);

                        // display the menu
                        menu.ShowAsContext();
                    } else if(Event.current.button == 0) {
                        if(graph.isConnecting) {
                            graph.EndConnecting(this);
                        }
                        graph.SetSelectedNode(this);
                    }
                }
            }

            GUI.DragWindow();
        }

        private void StartConnecting() {
            graph.StartConnecting(this);
        }

        //Logic
        public override void Enter() {

        }

        public override void Update() {
         
        }

        public override void Exit() {

        }
    }

    [System.Serializable]
    public class AnimationNode : Node
    {
        public AIAnimation animation;

        [System.NonSerialized]private float startTime;

        public AnimationNode(Vector2 _pos, int _id) : base(_pos, _id) {

        }

        protected override void DrawWindowContents(int _id) {
            animation = (AIAnimation)EditorGUILayout.ObjectField("Animation: ", animation, typeof(AIAnimation));

            base.DrawWindowContents(_id);
        }


        private void StartConnecting() {
            graph.StartConnecting(this);
        }

        //Logic
        public override void Enter() {
            runtimeController.PlayAnimationNode(animation, true);
            runtimeController.SetCurrentState(this);
            startTime = Time.time;
        }

        public override void Update() {
            //are we at the end of the animation?
            float endTime = (startTime + animation.length);

            if(Time.time >= endTime) {
                Exit();
            }

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
        }

        public override void Exit() {
           
        }

        public override AIAnimation FetchLastAnimation() {
            return animation;
        }
    }

    [System.Serializable]
    public class RandomAnimationNode : AnimationNode
    {
        public List<AIAnimation> animations = new List<AIAnimation>();

        public RandomAnimationNode(Vector2 _pos, int _id) : base(_pos, _id) {
       
        }


        public override AIAnimation FetchLastAnimation() {
            return null;
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

        public override void Update() {

        }

        public override void Exit() {

        }
    }

    [System.Serializable]
    public class Transition : State
    {
        public AIAnimatorController myController;

        public int startId;
        public Node start {
            get {
                if(runtimeController != null) {
                    return runtimeController.GetNode(startId);
                } else {
                    return myController.GetNode(startId);
                }
            }
        }
        public int endId;
        public Node end {
            get {
                if(runtimeController != null) {
                    return runtimeController.GetNode(endId);
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
        [System.NonSerialized] public AIAnimatorControllerWindow graph;


        public Transition(AIAnimatorController _myController, int _start, int _end, float _offset) {
            myController = _myController;
            startId = _start;
            endId = _end;
            offset = _offset;
            conditions = new List<Condition>();
        }

        public override void OnRuntimeInitialize(AIRuntimeAnimatorController _runtimeController) {
            base.OnRuntimeInitialize(_runtimeController);
            foreach(Condition condition in conditions) {
                condition.OnRuntimeInitialize(runtimeController);
            }
        }

        public void OnEditorWindowOpen(AIAnimatorControllerWindow _graph) {
            graph = _graph;
            myController = _graph.controller;
        }

        public void RemoveParameter(string _name) {
            for(int i = 0; i < conditions.Count; i++) {
                if(conditions[i].name == _name) {
                    conditions.RemoveAt(i);
                }
            }
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

        public bool ContainsCondition(string _name) {
            foreach(Condition condition in conditions) {
                if(condition.name == _name) {
                    return true;
                }
            }
            return false;
        }

        public void Delete() {
            start.RemoveTransition(this);
        }

        private void CheckForClick(Vector3[] _bezierPoints, float _radius) {
            Vector2 mousePos = Event.current.mousePosition;

            for(int i = 0; i < _bezierPoints.Length; i++) {
                Vector2 bezierPoint = new Vector2(_bezierPoints[i].x, _bezierPoints[i].y);
                float dist = Vector2.Distance(mousePos, bezierPoint);
                if(dist < _radius) {
                    OnClick();
                    break;
                }
            }
        }

        private void OnClick() {
            graph.SetSelectedTransition(this);
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

        //Logic
        public bool MeetsRequirements() {
            foreach(Condition condition in conditions) {
                if(!condition.IsTrue()) {
                    return false;
                }
            }
            return true;
        }

        public override void Enter() {
            runtimeController.PlayTransition(this, end.FetchLastAnimation());
            runtimeController.SetCurrentState(this);
            startTime = Time.time;
        }

        public override void Update() {
            //are we at the end of the transition?
            float endTime = (startTime + transitionTime);

            if(Time.time >= endTime) {
                Exit();
            }
        }

        public override void Exit() {
            end.Enter();
        }
    }

    [System.Serializable]
    public class Parameter
    {

        public string name;
        public Type type;

        public Parameter(string _name, Type _type) {
            name = _name;
            type = _type;
        }

        public enum Type
        {
            Boolean,
            Float
        }

        public bool bValue;
        public float fValue;

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
    }

    [System.Serializable]
    public class Condition : Parameter
    {
        public Parameter targetParameter;
        public FConditions fCondition;

        public Condition(string _name, Type _type) : base(_name, _type) {
   
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
                }

            if(GUILayout.Button("Delete")) {
                return false;
            }
            GUILayout.EndHorizontal();
            return true;
        }

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
            }
            return false;
        }

    }
}
    
