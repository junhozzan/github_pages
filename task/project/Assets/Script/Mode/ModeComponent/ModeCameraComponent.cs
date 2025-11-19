using System;
using UnityEngine;

namespace ModeComponent
{
    public sealed class ModeCameraComponent : ModeBaseComponent
    {
        private Camera camera = null;

        public ModeCameraComponent(XComponent parent, Mode mode) : base(parent, mode)
        {

        }

        public override void OnEnable()
        {
            base.OnEnable();
            camera = Camera.main;
        }

        public override void OnDisable()
        {
            camera = null;
            base.OnDisable();
        }

        public Vector2 ScreenToWorldPoint(Vector2 screen)
        {
            return camera.ScreenToWorldPoint(screen);
        }

        public Vector2 WorldToScreenPoint(Vector2 world)
        {
            return camera.WorldToScreenPoint(world);
        }
    }
}