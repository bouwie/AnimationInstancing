using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;


namespace AnimationInstancing
{
    public class AIAnimatorControllerWindow : EditorWindow
    {

        public AIAnimatorController controller;

        private Texture2D gridTexture;
        public static Texture2D arrowTexture;

        private Vector2 areaSize = new Vector2(100000, 100000);

        public bool isConnecting;
        private Node connectionStart;
        private bool mouseInGrid;
        private float moveSpeed = 4;

        private List<Window> windowsOpen = new List<Window>();

        private ParametersWindow parametersWindow;

        private bool isDragging;
        private Vector2 startPan;
        private Vector2 startMousePos;

        private float minZoom = -30, maxZoom = 30;
        public int fontSize { get; private set; }
        private int minFontSize = 12, maxFontSize = 20;

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

            parametersWindow = new ParametersWindow(new Rect(0,0,0,0), this);
            windowsOpen.Add(parametersWindow);

            controller.panX = areaSize.x / 2;
            controller.panY = areaSize.y / 2;
        }

        private void OnDestroy() {
            EditorUtility.SetDirty(controller);
        }

        void OnGUI() {
            if(controller == null) {
                return;
            }

            GUI.BeginGroup(new Rect(-controller.panX, -controller.panY, areaSize.x, areaSize.y));

            GUI.DrawTextureWithTexCoords(new Rect(0, 0, areaSize.x + controller.zoom, areaSize.y + controller.zoom), gridTexture, new Rect(0, 0, areaSize.x / gridTexture.width - controller.zoom, areaSize.y / gridTexture.height - controller.zoom));

            Event current = Event.current;

            if(current.keyCode == KeyCode.Delete && controller.selected != null) {
                controller.selected.Delete();
                Repaint();
            }


            if(current.type == EventType.ContextClick) {
                // create the menu and add items to it
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("New AnimationNode"), false, AddAnimationNode);
                menu.AddItem(new GUIContent("New RandomAnimationNode"), false, AddRandomAnimationNode);

                // display the menu
                menu.ShowAsContext();
            }

            BeginWindows();

            for(int i = 0; i < controller.nodes.Count; i++) {
                Node currentNode = controller.nodes[i];

                currentNode.Draw();

                currentNode.containsMouse = currentNode.windowRect.Contains(current.mousePosition);
                if(currentNode.containsMouse) {

                    //handle connecting
                    if(isConnecting && connectionStart != currentNode && !connectionStart.HasConnectionTo(currentNode)) {
                        Transition.DrawTransition(connectionStart.windowRect, currentNode.windowRect, 0);
                        Repaint();
                    }

                    mouseInGrid = false;
                }

            }
            for(int i = 0; i < windowsOpen.Count; i++) {
                windowsOpen[i].Draw();
            }

            EndWindows();

            //unselect
            if(current.button == 0 && current.type == EventType.MouseDown && mouseInGrid) {
                controller.selected.OnUnSelect();
                controller.selected = null;
                isConnecting = false;
                connectionStart = null;
                Repaint();
            }


            if(mouseInGrid && isConnecting && connectionStart != null) {
                Transition.DrawTransition(connectionStart.windowRect, new Rect(current.mousePosition, new Vector2(1, 1)), 0);
                Repaint();
            }



            GUI.EndGroup();

            MoveView(current);

            mouseInGrid = true;
        }


        private void MoveView(Event _current) {
            if(_current.button == 0 && _current.type == EventType.MouseDown && _current.alt) {
                startPan = new Vector2(controller.panX, controller.panY);
                startMousePos = _current.mousePosition;
                isDragging = true;
            }

            if(isDragging) {
                controller.panX = startPan.x + (startMousePos.x - _current.mousePosition.x);
                controller.panY = startPan.y + (startMousePos.y - _current.mousePosition.y);
                Repaint();
            }

            if(isDragging && _current.button == 0 && _current.type == EventType.MouseUp) {
                isDragging = false;
            }

            if(_current.type == EventType.ScrollWheel) {
                controller.zoom -= _current.delta.y;
                controller.zoom = Mathf.Clamp(controller.zoom, minZoom, maxZoom);

                fontSize = (int)Remap(controller.zoom, minZoom, maxZoom, minFontSize, maxFontSize);
                Debug.Log(fontSize);

                Repaint();
            }

        }

        private float Remap(float value, float from1, float to1, float from2, float to2) {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        private void AddAnimationNode() {
            Vector2 pos = new Vector2(areaSize.x / 2 + position.width / 2, areaSize.y / 2 + position.height / 2);
            AnimationNode newNode = new AnimationNode(pos, controller.animNodeCount);
            newNode.OnEditorWindowOpen(this);
            controller.animNodeCount++;

            controller.nodes.Add(newNode);
        }

        private void AddRandomAnimationNode() {
            Vector2 pos = new Vector2(areaSize.x / 2 + position.width / 2, areaSize.y / 2 + position.height / 2);
            RandomAnimationNode newNode = new RandomAnimationNode(pos, controller.animNodeCount);
            newNode.OnEditorWindowOpen(this);
            controller.animNodeCount++;

            controller.nodes.Add(newNode);
        }

        public void SetEntryNode(int _entryNodeId) {
            controller.entryNodeId = _entryNodeId;
        }

        public void StartConnecting(Node _start) {
            connectionStart = _start;
            isConnecting = true;
        }

        public void EndConnecting(Node _end) {
            if(isConnecting) {
                isConnecting = false;
                MakeTransition(connectionStart, _end);
            }
        }

        private void MakeTransition(Node _start, Node _end) {
            if(!_start.HasConnectionTo(_end)) {

                Transition newTransition;
                if(_end.HasConnectionTo(_start)) {
                    newTransition = new Transition(controller, _start.id, _end.id, 10);
                } else {
                    newTransition = new Transition(controller, _start.id, _end.id, -10);
                }
                newTransition.OnEditorWindowOpen(this);
                _start.transitions.Add(newTransition);
            }
        }


        public void SetSelected(State _target) {
            if(controller.selected != null) {
                controller.selected.OnUnSelect();
            }
            mouseInGrid = false;
            controller.selected = _target;
            Repaint();
        }

        public void AddWindow(Window _window) {
            windowsOpen.Add(_window);
        }

        public void RemoveWindow(Window _window) {
            windowsOpen.Remove(_window);
        }

        public void RemoveAllWindows<T>() {
            for(int i = 0; i < windowsOpen.Count; i++) {
                if(windowsOpen[i].GetType() == typeof(T)) {
                    windowsOpen.RemoveAt(i);
                }
            }   

        }

    }
    

}

#endif