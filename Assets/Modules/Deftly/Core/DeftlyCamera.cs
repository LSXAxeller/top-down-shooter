// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Deftly
{
    [AddComponentMenu("Deftly/Deftly Camera")]
    [ExecuteInEditMode]
    public class DeftlyCamera : MonoBehaviour
    {
        public enum MoveStyle {Loose, Stiff}
        public MoveStyle FollowingStyle;

        public enum TrackingStyle {PositionalAverage, AimingAverage}
        public TrackingStyle Tracking;

        public float TrackDistance;
        public float TrackSpeed;

        public List<GameObject> Targets = new List<GameObject>();
        public Vector3 Offset;

        public GameObject Arbiter; // The Arbiter also provides a source for orientation on the Controller.

        private Vector3[] _vectorArray;
        private Vector3 _averagePos;
        
        void Reset()
        {
            FollowingStyle = MoveStyle.Loose;
            Tracking = TrackingStyle.AimingAverage;
            TrackDistance = 2f;
            TrackSpeed = 5f;
            Offset = new Vector3(0.5f, 10.0f, -0.5f);
            Targets = new List<GameObject>();
        }
        void OnEnable()
        {
            if (Arbiter != null) return;
            Arbiter = new GameObject {name = "Camera Arbiter"};
            Arbiter.transform.SetParent(transform);
        }

        void Start()
        {
            Options.Init();
        }

        void FollowTargets()
        {
            _averagePos = GetAveragePos();

            if (!Application.isPlaying)
            {
                transform.position = _averagePos + Offset;
            }
            else
            {
                if (FollowingStyle == MoveStyle.Loose) transform.position = Vector3.Lerp(transform.position, _averagePos + Offset, Time.deltaTime * TrackSpeed);
                else transform.position = _averagePos + Offset;
            }
        }
        void SetArbiterTransform()
        {
            if (!Arbiter) return;
            Arbiter.transform.position = new Vector3(_averagePos.x, gameObject.transform.position.y, _averagePos.z);
            Arbiter.transform.rotation = Quaternion.Euler(0,gameObject.transform.rotation.eulerAngles.y,0);
        }
        Vector3 GetAveragePos()
        {
            _vectorArray = new Vector3[Targets.Count];
            for (int i = 0; i < Targets.Count; i++)
            {
                if (Targets[i] == null)
                {
                    // handle in case a target was removed
                    Debug.LogWarning("A Camera GameObject Target is null! Removing entry.");

                    // find an alternative target
                    int s = i > 0
                        ? Targets[i - 1] != null 
                            ? i - 1
                            : i + 1
                        : 1;

                    Debug.Log(s);

                    _vectorArray[s] = Tracking == TrackingStyle.PositionalAverage 
                        ? Targets[s].transform.position 
                        : Targets[s].transform.position + (Targets[s].transform.forward * TrackDistance);

                    _averagePos += _vectorArray[s];

                    // remove the null target
                    Targets.RemoveAt(i);
                }
                else
                {
                    // business as usual
                    _vectorArray[i] = Tracking == TrackingStyle.PositionalAverage
                       ? Targets[i].transform.position
                       : Targets[i].transform.position + (Targets[i].transform.forward * TrackDistance);

                    _averagePos += _vectorArray[i];
                }
            }

            return _averagePos / _vectorArray.Length;
        }

        public GameObject GetArbiter()
        {
            return Arbiter;
        }

        void OnDestroy()
        {
            DestroyImmediate(Arbiter);
        }

        void FixedUpdate()
        {
            if (Targets.Count > 0 && Targets.All(tar => tar != null))
            {
                FollowTargets();
                SetArbiterTransform();

                _averagePos.Set(0, 0, 0); // precaution
            }
            else
            {
                Debug.LogWarning("Main Camera must have one or more targets!");
            }
        }
    }
}