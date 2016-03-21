// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;

namespace Deftly
{
    public class Mover : MonoBehaviour
    {
        public static Mover AddMover(GameObject onThis, bool usePhysics, float speed, bool constant, Subject owner, LayerMask mask)
        {
            Mover omg = onThis.AddComponent<Mover>();
            omg.UsePhysics = usePhysics;
            omg.Speed = speed;
            omg.Constantly = constant;
            omg.Owner = owner;
            omg.Mask = mask;

            return omg;
        }

        public bool UsePhysics;
        public float Speed;
        public bool Constantly;
        public Subject Owner;
        public LayerMask Mask;

        private float _distance;
        private Vector3 _heading;
        private bool _firstShot;

        private Vector3 _lastPos;
        private Vector3 _thisPos;
        private Rigidbody2D _rb;
        private bool _done;

        void Start()
        {
            _firstShot = true;
            _rb = GetComponent<Rigidbody2D>();
            if (_rb == null)
            {
                Debug.LogError("You must have a Rigidbody2D on the Projectile!");
            }
        }
        void FixedUpdate()
        {
            if (UsePhysics && !_done)
                DoPhysics(); 
            else if (!_done)
                DoTranslate();
        }
        void DoPhysics() // relies on physics for everything.
        {
            if (!Constantly)
            {
                _rb.AddForce(transform.right*Speed*2);
                _done = true;
            }
            else
            {
                _rb.velocity = transform.right * Speed;
                _done = true;
            }
        }
        void DoTranslate() // accomodates for bullet through paper issue manually.
        {
            _lastPos = gameObject.transform.position;
            transform.Translate(Vector3.right * Speed * 0.03f);
            _thisPos = gameObject.transform.position;
            _heading = (_thisPos - _lastPos);
            
            if (_firstShot)
            {
                _distance = _heading.magnitude; // only need distance once, assuming projectile speed is constant.
                _firstShot = false;
            }

            RaycastHit2D hit = Physics2D.Raycast(_lastPos, _heading, _distance, Mask);

            // Debug.DrawLine(lastPos, lastPos + (heading * distance), Color.magenta);

            if (hit.collider != null)
            {
                gameObject.transform.position = hit.point;
            }
        }
    }
}