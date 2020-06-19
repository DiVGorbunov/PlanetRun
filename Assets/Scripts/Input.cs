using UnityEngine;
using UnityEngine.Assertions;

namespace InputWrapper
{
    public static class Input
    {
        public static bool GetButton(string buttonName)
        {
            return UnityEngine.Input.GetButton(buttonName);
        }

        public static bool GetButtonDown(string buttonName)
        {
            return UnityEngine.Input.GetButtonDown(buttonName);
        }

        public static bool GetButtonUp(string buttonName)
        {
            return UnityEngine.Input.GetButtonUp(buttonName);
        }

        public static bool GetMouseButton(int button)
        {
            return UnityEngine.Input.GetMouseButton(button);
        }

        public static bool GetMouseButtonDown(int button)
        {
            return UnityEngine.Input.GetMouseButtonDown(button);
        }

        public static bool GetMouseButtonUp(int button)
        {
            return UnityEngine.Input.GetMouseButtonUp(button);
        }

        public static int touchCount
        {
            get
            {
#if UNITY_EDITOR
                return fakeTouch.HasValue ? 1 : 0;
#else
                return UnityEngine.Input.touchCount;
#endif
            }
        }

        public static Touch GetTouch(int index)
        {
#if UNITY_EDITOR
            Assert.IsTrue(fakeTouch.HasValue && index == 0);
            return fakeTouch.Value;
#else
            return UnityEngine.Input.GetTouch(index);
#endif
        }

        static Touch? fakeTouch => SimulateTouchWithMouse.Instance.FakeTouch;

        public static Touch[] touches
        {
            get
            {
#if UNITY_EDITOR
                return fakeTouch.HasValue ? new[] { fakeTouch.Value } : new Touch[0];
#else
                return UnityEngine.Input.touches;
#endif
            }
        }
    }
}