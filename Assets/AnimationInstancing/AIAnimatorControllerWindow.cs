using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AIAnimatorControllerWindow : EditorWindow
{

    public AIAnimatorController controller;

    private Texture2D gridTexture;
    public static Texture2D arrowTexture;

    private Vector2 areaSize = new Vector2(100000, 100000);

    public bool isConnecting;
    private AIAnimatorController.Node connectionStart;
    private bool mouseInGrid;
    private float moveSpeed = 4;

    private string parameterName;
    private AIAnimatorController.Parameter.Type parameterType;
    private int conditionIndex;

    public static void ShowEditor(AIAnimatorController _controller) {
        AIAnimatorControllerWindow editor = (AIAnimatorControllerWindow)EditorWindow.GetWindow(typeof(AIAnimatorControllerWindow), false);
        editor.Init(_controller);
    }


    public void Init(AIAnimatorController _controller) {
        controller = _controller;
        controller.OnEditorWindowOpen(this);

            isConnecting = false;
        connectionStart = null;
        gridTexture = Resources.Load("Grid", typeof(Texture2D)) as Texture2D;
        arrowTexture = Resources.Load("Arrow", typeof(Texture2D)) as Texture2D;
        gridTexture.wrapMode = TextureWrapMode.Repeat;

        controller.panX = areaSize.x/2;
        controller.panY = areaSize.y/2;
    }

    private void OnDestroy() {
        EditorUtility.SetDirty(controller);
    }

    void OnGUI() {
        if(controller == null) {
            return;
        }

        GUI.BeginGroup(new Rect(-controller.panX, -controller.panY, areaSize.x, areaSize.y));

        GUI.DrawTextureWithTexCoords(new Rect(0, 0, areaSize.x, areaSize.y), gridTexture, new Rect(0, 0, areaSize.x / gridTexture.width, areaSize.y / gridTexture.height));

        Event current = Event.current;

        if(current.keyCode == KeyCode.Delete) {
            DeleteNode(controller.selectedNode);
            DeleteTransition(controller.selectedTransition);
        }


        if(current.type == EventType.ContextClick) {
            // create the menu and add items to it
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("New AnimationNode"), false, AddAnimationNode);
            menu.AddItem(new GUIContent("New BlendTreeNode"), false, AddRandomAnimationNode);

            // display the menu
            menu.ShowAsContext();
        }

        BeginWindows();

        for(int i = 0; i < controller.nodes.Count; i++) {
            AIAnimatorController.Node currentNode = controller.nodes[i];

            currentNode.Draw();

            currentNode.containsMouse = currentNode.windowRect.Contains(current.mousePosition);
            if(currentNode.containsMouse) {

                //handle connecting
                if(isConnecting && connectionStart != currentNode && !connectionStart.HasConnectionTo(currentNode)) {
                    AIAnimatorController.Transition.DrawTransition(connectionStart.windowRect, currentNode.windowRect, 0);
                    Repaint();
                }

                mouseInGrid = false;
            }

        }
        DrawParametersWindow();
        DrawTransitionWindow();
        DrawNodeWindow();

        EndWindows();

        //unselect
        if(current.button == 0 && current.type == EventType.MouseDown && mouseInGrid) {
            controller.selectedNode = null;
            isConnecting = false;
            connectionStart = null;
            controller.selectedTransition = null;
            Repaint();
        }


        if(mouseInGrid && isConnecting && connectionStart != null) {
            AIAnimatorController.Transition.DrawTransition(connectionStart.windowRect, new Rect(current.mousePosition, new Vector2(1, 1)), 0);
            Repaint();
        }



        GUI.EndGroup();

        MoveView(current);

        mouseInGrid = true;
    }

    private void DeleteNode(AIAnimatorController.Node _node) {
        controller.nodes.Remove(_node);
        Repaint();
    }

    private void DeleteTransition(AIAnimatorController.Transition _transition) {
        if(_transition == null) {
            return;
        }

        _transition.Delete();
        Repaint();
    }

    private void MoveView(Event _current) {
        if(_current.keyCode == KeyCode.W) {
           controller.panY -= moveSpeed;
            Repaint();
        }

        if(_current.keyCode == KeyCode.S) {
            controller.panY += moveSpeed;
            Repaint();
        }
        if(_current.keyCode == KeyCode.D) {
            controller.panX += moveSpeed;
            Repaint();
        }

        if(_current.keyCode == KeyCode.A) {
            controller.panX -= moveSpeed;
            Repaint();
        }
    }

    private void AddAnimationNode() {
        Vector2 pos = new Vector2(areaSize.x / 2 + position.width / 2, areaSize.y / 2 + position.height / 2);
        AIAnimatorController.AnimationNode newNode = new AIAnimatorController.AnimationNode(pos, controller.animNodeCount);
        newNode.OnEditorWindowOpen(this);
       controller.animNodeCount++;

        controller.nodes.Add(newNode);
    }

    private void AddRandomAnimationNode() {
        Vector2 pos = new Vector2(areaSize.x / 2 + position.width / 2, areaSize.y / 2 + position.height / 2);
        AIAnimatorController.RandomAnimationNode newNode = new AIAnimatorController.RandomAnimationNode(pos, controller.animNodeCount);
        newNode.OnEditorWindowOpen(this);
        controller.animNodeCount++;

        controller.nodes.Add(newNode);
    }

    public void StartConnecting(AIAnimatorController.Node _start) {
        connectionStart = _start;
        isConnecting = true;
    }

    public void EndConnecting(AIAnimatorController.Node _end) {
        if(isConnecting) {
            isConnecting = false;
            MakeTransition(connectionStart, _end);
        }
    }

    private void MakeTransition(AIAnimatorController.Node _start, AIAnimatorController.Node _end) {
        if(!_start.HasConnectionTo(_end)) {

            AIAnimatorController.Transition newTransition;
            if(_end.HasConnectionTo(_start)) {
                newTransition = new AIAnimatorController.Transition(controller,_start.id, _end.id, 10);
            } else {
                newTransition = new AIAnimatorController.Transition(controller,_start.id, _end.id, -10);
            }
            newTransition.OnEditorWindowOpen(this);
            _start.transitions.Add(newTransition);
        }
    }

    public void SetSelectedTransition(AIAnimatorController.Transition _target) {
        mouseInGrid = false;
        controller.selectedNode = null;
        controller.selectedTransition = _target;
        Repaint();
    }

    public void SetSelectedNode(AIAnimatorController.Node _target) {
        mouseInGrid = false;
        controller.selectedTransition = null;
        controller.selectedNode = _target;
        Repaint();
    }



    #region Windows
    private void DrawTransitionWindow() {
        if(controller.selectedTransition == null || controller.selectedTransition.myController == null) {
            return;
        }

        float scale = 4.5f;

        float width = position.width / scale;
        float heigth = position.height / scale;

        Rect windowRect = new Rect(controller.panX + position.width - width, controller.panY, width, heigth);
        windowRect = GUI.Window(-2, windowRect, TransitionWindowContents, "Transition");
    }

    private void TransitionWindowContents(int _id) {
        AIAnimatorController.Transition targetTransition = controller.selectedTransition;

        GUILayout.Label("from: " + targetTransition.start.name + " to: " + targetTransition.end.name);
        targetTransition.hasExitTime = GUILayout.Toggle(targetTransition.hasExitTime, "has exit time");
        if(targetTransition.hasExitTime) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("exit time: ");
            targetTransition.exitTime = EditorGUILayout.FloatField(targetTransition.exitTime);
            GUILayout.EndHorizontal();
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("transiton time: ");
        targetTransition.transitionTime = EditorGUILayout.FloatField(targetTransition.transitionTime);
        GUILayout.EndHorizontal();


        GUILayout.Space(20);
        GUILayout.Label("conditions:");
        for(int i = 0; i < targetTransition.conditions.Count; i++) {
            if(!targetTransition.conditions[i].Draw()) {
                targetTransition.conditions.RemoveAt(i);
            }

        }

        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        string[] parameterNames = controller.GetParameterNames();

        if(parameterNames.Length > 0) {
            conditionIndex = EditorGUILayout.Popup(conditionIndex, parameterNames);

            EditorGUI.BeginDisabledGroup(targetTransition.ContainsCondition(parameterNames[conditionIndex]));
            if(GUILayout.Button("Add")) {
                AIAnimatorController.Parameter parameter = controller.GetParameter(parameterNames[conditionIndex]);

                targetTransition.conditions.Add(new AIAnimatorController.Condition(parameter.name, parameter.type));
            }
            EditorGUI.EndDisabledGroup();
        }

        GUILayout.EndHorizontal();
    }



    private void DrawNodeWindow() {

        if(controller.selectedNode == null || controller.selectedNode.graph == null) {
            return;
        }

        float scale = 4.5f;

        float width = position.width / scale;
        float heigth = position.height / scale;


        Rect windowRect = new Rect(controller.panX + position.width - width, controller.panY, width, heigth);
        windowRect = GUI.Window(-2, windowRect, NodeWindowContents, controller.selectedNode.GetType().Name);
    }

    private void NodeWindowContents(int _id) {
        GUILayout.Label(controller.selectedNode.name);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Name: ");
        controller.selectedNode.name = EditorGUILayout.TextField(controller.selectedNode.name);
        GUILayout.EndHorizontal();


        //did we select a animationNode?
        AIAnimatorController.AnimationNode animationNode = controller.selectedNode as AIAnimatorController.AnimationNode;

        if(animationNode != null) {
            animationNode.animation = (AIAnimation)EditorGUILayout.ObjectField("Animation: ", animationNode.animation, typeof(AIAnimation));
        }

        //did we select a randomAnimationNode?
        

        GUILayout.Label("Transitions:");
        for(int i = 0; i < controller.selectedNode.transitions.Count; i++) {
            GUILayout.Label("from: " + controller.selectedNode.transitions[i].start.name + " to: " + controller.selectedNode.transitions[i].end.name);
        }
    }


    private void DrawParametersWindow() {
        float scale = 4.5f;

        float width = position.width / scale;
        float heigth = position.height / scale;

        Rect windowRect = new Rect(controller.panX, controller.panY, width, heigth);
        windowRect = GUI.Window(-2, windowRect, ParametersWindowContents, "Parameters");
    }

    private void ParametersWindowContents(int _id) {
        for(int i = 0; i < controller.parameters.Count; i++) {
            if(!controller.parameters[i].Draw()) {
                controller.RemoveParameter(controller.parameters[i].name);
                controller.parameters.RemoveAt(i);
            }
        }
        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        parameterName = EditorGUILayout.TextField("Parameter name: ",parameterName);
        parameterType = (AIAnimatorController.Parameter.Type)EditorGUILayout.EnumPopup(parameterType);

        EditorGUI.BeginDisabledGroup(controller.HasParameter(parameterName));
        if(GUILayout.Button("Add")) {
            controller.parameters.Add(new AIAnimatorController.Parameter(parameterName, parameterType));
            parameterName = string.Empty;
        }
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();
    }


    #endregion

}
