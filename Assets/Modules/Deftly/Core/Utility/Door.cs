// (c) Copyright Cleverous 2015. All rights reserved.

using System.Collections;
using UnityEngine;

namespace Deftly
{
    public class Door : MonoBehaviour
    {
        [Header("Door setup assumes Closed position")] 
        public GameObject DoorMesh;
        public LayerMask CanUseThisDoor;
        public Vector3 OpenDirection;
        public AnimationCurve OpenCurve;
        public AnimationCurve CloseCurve;
        
        [Space][Space]
        public float TotalTimeToUse;
        public AudioClip OpenSound;
        public AudioClip CloseSound;

        private bool _doorIsMoving;
        private bool _doorIsOpen;
        private float _progress;

        public bool ManuallyTriggered;
        public bool Locked;
        public bool StartClosed;

        private int _occupancy;
        private Vector3 _closedPosition;
        private Vector3 _openPosition;
        private AudioSource _noiseMaker;

        void Reset()
        {
            OpenDirection = Vector3.right;
            OpenCurve = AnimationCurve.Linear(0, 0, 1, 1);
            CloseCurve = AnimationCurve.Linear(0, 0, 1, 1);
            TotalTimeToUse = 0.5f;
            OpenSound = null;
            CloseSound = null;
            ManuallyTriggered = false;
            Locked = false;
        }
        void Awake()
        {
            _closedPosition = gameObject.transform.position;
            _openPosition = _closedPosition + gameObject.transform.InverseTransformDirection(OpenDirection);
        }
        void Start()
        {
            _noiseMaker = GetComponent<AudioSource>();
            _occupancy = 0;
            if (StartClosed) StartCoroutine(StartClose());
        }

        IEnumerator StartClose()
        {
            yield return new WaitForSeconds(0.5f);
            Close();
        }
        void OnTriggerEnter(Collider col)
        {
            if (ManuallyTriggered || Locked || !DoorMesh) return;
            if (!StaticUtil.LayerMatchTest(CanUseThisDoor, col.gameObject)) return;
            _occupancy++;
        }
        void OnTriggerExit(Collider col)
        {
            if (ManuallyTriggered || Locked || !DoorMesh) return;
            if (!StaticUtil.LayerMatchTest(CanUseThisDoor, col.gameObject)) return;
            _occupancy--;
        }
        void Update()
        {
            if (_doorIsMoving) _progress += Time.deltaTime;
            if (ManuallyTriggered || Locked) return;
            if (_doorIsOpen && !_doorIsMoving && _occupancy == 0) Close();
            if (!_doorIsOpen && !_doorIsMoving && _occupancy > 0) Open();
        }

        public void Open()
        {
            StartCoroutine(OpenDoor());
        }
        private IEnumerator OpenDoor()
        {
            while (_doorIsMoving) yield return null;
            if (!_doorIsOpen)
            {
                _progress = 0;
                _doorIsMoving = true;
            }
            else yield break;

            if (OpenSound != null)
            {
                _noiseMaker.clip = OpenSound;
                _noiseMaker.Play();
            }

            while (_progress <= TotalTimeToUse)
            {
                DoorMesh.transform.position = _closedPosition + (gameObject.transform.InverseTransformDirection(OpenDirection * CloseCurve.Evaluate(_progress / TotalTimeToUse)));
                yield return null;
            }

            _doorIsMoving = false;
            _doorIsOpen = true;
            yield return null;
        }
        public void Close()
        {
            StartCoroutine(CloseDoor());
        }
        private IEnumerator CloseDoor()
        {
            while (_doorIsMoving) yield return null;
            if (_doorIsOpen)
            {
                _progress = 0;
                _doorIsMoving = true;
            }
            else yield break;

            if (CloseSound != null)
            {
                _noiseMaker.clip = CloseSound;
                _noiseMaker.Play();
            }

            while (_progress <= TotalTimeToUse)
            {
                DoorMesh.transform.position = _openPosition + (gameObject.transform.InverseTransformDirection(-OpenDirection * CloseCurve.Evaluate(_progress / TotalTimeToUse)));
                yield return null;
            }

            _doorIsMoving = false;
            _doorIsOpen = false;
            yield return null;
        }
    }
}