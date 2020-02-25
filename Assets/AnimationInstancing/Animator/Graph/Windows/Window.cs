using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationInstancing {

    public abstract class Window
    {
        public Rect windowRect;

        public Window(Rect _windowRect) {
            windowRect = _windowRect;
        }

        public abstract void Draw();

        public abstract void Contents(int _id);

    
    }

}
