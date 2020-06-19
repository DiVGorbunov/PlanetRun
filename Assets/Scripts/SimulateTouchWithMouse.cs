using UnityEngine;

namespace InputWrapper
{
    internal class SimulateTouchWithMouse
    {
        static SimulateTouchWithMouse instance;
        float lastUpdateTime;
        Vector3 prevMousePos;
        Touch? fakeTouch;


        public static SimulateTouchWithMouse Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SimulateTouchWithMouse();
                }

                return instance;
            }
        }

        public Touch? FakeTouch
        {
            get
            {
                update();
                return fakeTouch;
            }
        }

        void update()
        {
            if (Time.time != lastUpdateTime)
            {
                lastUpdateTime = Time.time;

                var curMousePos = UnityEngine.Input.mousePosition;
                var delta = curMousePos - prevMousePos;
                prevMousePos = curMousePos;

                fakeTouch = createTouch(getPhase(), delta);
            }
        }

        static TouchPhase? getPhase()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                return TouchPhase.Began;
            }
            else if (UnityEngine.Input.GetMouseButton(0))
            {
                return TouchPhase.Moved;
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                return TouchPhase.Ended;
            }
            else
            {
                return null;
            }
        }

        static Touch? createTouch(TouchPhase? phase, Vector3 delta)
        {
            if (!phase.HasValue)
            {
                return null;
            }

            var curMousePos = UnityEngine.Input.mousePosition;
            return new Touch
            {
                phase = phase.Value,
                type = TouchType.Indirect,
                position = curMousePos,
                rawPosition = curMousePos,
                fingerId = 0,
                tapCount = 1,
                deltaTime = Time.deltaTime,
                deltaPosition = delta
            };
        }
    }
}