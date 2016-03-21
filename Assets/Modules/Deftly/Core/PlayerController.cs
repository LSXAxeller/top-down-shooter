// (c) Copyright Cleverous 2015. All rights reserved.

using System.Collections;
using UnityEngine;

namespace Deftly
{
    [AddComponentMenu("Deftly/Player Controller")]
    [RequireComponent(typeof(Subject))]
    public class PlayerController : MonoBehaviour
    {
        public enum ControlFeel { Stiff, Loose }
        public ControlFeel AimControlFeel = ControlFeel.Loose;
        public ControlFeel MoveControlFeel = ControlFeel.Loose;
        
        [HideInInspector]
        public float AimTension;
        [HideInInspector]
        public float MoveTension;

        public string ChangeWeapon;
        public string Horizontal;
        public string Vertical;
        public string Fire1;
        public string Fire2;
        public string Reload;
        public string Interact;
        public string DropWeapon;
        public LayerMask Mask;
        public bool LogDebug;
        public float WalkAnimSpeed;
        public float RunAnimSpeed;

        [HideInInspector]
        public bool UseInControl;

        private Camera _cam;
        private GameObject _go;
        private Subject _subject;
        private Rigidbody2D _rb;
        private GameObject _arbiter;
        private Vector3 _aimInput;
        private Vector3 _aimCache;

        [HideInInspector]
        public bool InputPermission;

        void Reset()
        {
            InputPermission = false;
            AimTension = 20;
            MoveTension = 8;
            WalkAnimSpeed = 1f;
            RunAnimSpeed = 2f;
            ChangeWeapon = "Mouse ScrollWheel";
            Horizontal = "Horizontal";
            Vertical = "Vertical";
            Fire1 = "Fire1";
            Fire2 = "Fire2";
            Reload = "Reload";
            Interact = "Interact";
            DropWeapon = "DropWeapon";
            Mask = -1;
        }
        void Start()
        {
            _go = gameObject;
            _subject = GetComponent<Subject>();
            _rb = GetComponent<Rigidbody2D>();
            _aimCache = Vector3.forward;

            _cam = FindObjectOfType<DeftlyCamera>().GetComponent<Camera>();

            if (LogDebug) Debug.Log("Rigidbody: " + _rb);
            if (LogDebug) Debug.Log("Main Camera: " + _cam);
            if (_cam != null)
            {
                StartCoroutine(FindCamera());
            }
            else Debug.LogError("Main Camera not found! You must tag your primary camera as Main Camera.");

            if (LogDebug) Debug.Log("Subject: " + _subject);            
        }

        private IEnumerator FindCamera()
        {
            while (true)
            {
                if (_cam != null)
                {
                    _cam.GetComponent<DeftlyCamera>().Targets[0] = gameObject;
                    _arbiter = _cam.GetComponent<DeftlyCamera>().GetArbiter();
                }
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void FixedUpdate()
        {
            if (_subject.IsDead | !InputPermission) return;

            Aim();
            Move();
        }

        void Aim()
        {
            _aimInput = GetAimAxis;
            if (_aimInput != Vector3.zero) _aimCache = _aimInput;
            else _aimInput = _aimCache;

            Vector3 mouse = _cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));
            // Get Angle in Radians
            float AngleRad = Mathf.Atan2(mouse.y - _go.transform.position.y, mouse.x - _go.transform.position.x);
            // Get Angle in Degrees
            float AngleDeg = Mathf.Rad2Deg * AngleRad;
            // Rotate Object
            Quaternion fin = Quaternion.Euler(0, 0, AngleDeg);

            switch (AimControlFeel)
            {
                // Stiff can have a maximum turn rate.
                case ControlFeel.Stiff:
                    float angle = Quaternion.Angle(_go.transform.rotation, fin);
                    float derp = angle / _subject.ControlStats.TurnSpeed / AimTension;
                    float progress = Mathf.Min(1f, Time.deltaTime / derp);

                    _go.transform.rotation = Quaternion.Slerp
                        (transform.rotation, fin, progress);
                    break;

                // Loose is a standard smooth blend. No max turn rate can be established.
                case ControlFeel.Loose:
                    _go.transform.rotation = Quaternion.Slerp
                        (_go.transform.rotation, fin, Time.deltaTime * _subject.ControlStats.TurnSpeed);
                    break;
                
            }
        }
        void Move()
        {
            // The camera can be at any angle, so its inaccurate to use TransformDirection directly.
            // Soooo... we have an Arbiter Object above the average position with a corrected angle.
            // Once I find a better way to solve the transform formula i'll remove the arbiter and simplify this.

            // This is the input after including control variables.
            Vector3 input = GetMovementAxis;

            // This corrects the movement direction to be relative to the camera angle and applies it.
            Vector3 movement = _arbiter.transform.TransformDirection(input);

            // Now use the result to move the player
            _rb.MovePosition(_go.transform.position + movement * .01f);
        }

        public Vector3 GetMovementAxis {
            get
            {
                return new Vector3(Input.GetAxis(Horizontal) * _subject.ControlStats.MoveSpeed, Input.GetAxis(Vertical) * _subject.ControlStats.MoveSpeed, 0f);
            }
        }
        public Vector3 GetAimAxis { get { return Input.mousePosition; }}
        public float GetInputFire1 { get { return Input.GetAxis(Fire1); } }
        public float GetInputFire2 { get { return Input.GetAxis(Fire2); } }
        public bool GetInputInteract { get { return Input.GetButton(Interact); } }
        public bool GetInputDropWeapon { get { return Input.GetButton(DropWeapon); } }
        public bool GetInputReload { get { return Input.GetButton(Reload); } }
        public float InputChangeWeapon { get { return Input.GetAxisRaw(ChangeWeapon); } }
    }
}