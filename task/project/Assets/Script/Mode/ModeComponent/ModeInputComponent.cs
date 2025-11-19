using UnityEngine.EventSystems;
using UnityEngine;

namespace ModeComponent
{
    /// <summary>
    /// (구) Input시스템
    /// </summary>
    public sealed class ModeInputComponent : ModeBaseComponent
    {
        private ModeCameraComponent cameraCom = null;

        public ModeInputComponent(XComponent parent, Mode mode) : base(parent, mode)
        {

        }

        public override void Initialize()
        {
            base.Initialize();
            cameraCom = GetComponent<ModeCameraComponent>();

        }

        public bool IsTouchedUI()
        {
#if UNITY_EDITOR
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }
#else
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    return true;
                }
            }
#endif
            return false;
        }

        public bool IsTouchDown(out Vector2 pos)
        {
            pos = Vector2.zero;
#if UNITY_EDITOR
            if (!Input.GetMouseButtonDown(0))
            {
                return false;
            }

            pos = cameraCom.ScreenToWorldPoint(Input.mousePosition);
#else
            if (Input.touchCount <= 0)
            {
                return false;
            }

            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began)
            {
                return false;
            }

            pos = cameraCom.ScreenToWorldPoint(touch.position);
#endif
            return true;
        }

        public bool IsTouchUp(out Vector2 pos)
        {
            pos = Vector2.zero;
#if UNITY_EDITOR
            if (!Input.GetMouseButtonUp(0))
            {
                return false;
            }
            pos = cameraCom.ScreenToWorldPoint(Input.mousePosition);

#else
            if (Input.touchCount <= 0)
            {
                return false;
            }

            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Ended)
            {
                return false;
            }

            pos = cameraCom.ScreenToWorldPoint(touch.position);
#endif

            return true;
        }

        public bool IsDragging(out Vector2 pos)
        {
            pos = Vector2.zero;
#if UNITY_EDITOR
            if (!Input.GetMouseButton(0))
            {
                return false;
            }
            pos = cameraCom.ScreenToWorldPoint(Input.mousePosition);
#else
            if (Input.touchCount <= 0)
            {
                return false;
            }

            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Moved && touch.phase != TouchPhase.Stationary)
            {
                return false;
            }

            pos = cameraCom.ScreenToWorldPoint(touch.position);
#endif

            return true;
        }

        public Vector3 GetWorldPoint()
        {
#if UNITY_EDITOR
            return cameraCom.ScreenToWorldPoint(Input.mousePosition);
#else
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began
                    || touch.phase == TouchPhase.Moved
                    || touch.phase == TouchPhase.Stationary)
                {
                    return cameraCom.ScreenToWorldPoint(Input.GetTouch(0).position);
                }
            }

            return Vector3.zero;
#endif
        }
    }
}