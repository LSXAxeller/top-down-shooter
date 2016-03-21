// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;

namespace Deftly
{
    [RequireComponent(typeof(LineRenderer))]
    public class LaserSight : MonoBehaviour
    {
        public float MaxDistance = 50;

        public GameObject OriginGameObject;
        private LayerMask _mask;
        private Vector3 _startPoint;
        private Vector3 _endPoint;
        private Weapon _weapon;
        private LineRenderer _lazor;

        void Start()
        {
            _lazor = GetComponent<LineRenderer>();
            _weapon = GetComponent<Weapon>();
            if (_weapon == null)
            {
                Debug.LogError("No Weapon script found for Laser Sight! Make sure a Weapon script is added to this Prefab/Object.");
                return;
            }
            if (_weapon.Stats.WeaponType == WeaponType.Melee)
            {
                Debug.LogError("Cannot use Laser Sight script on Melee Weapons!");
                return;
            }
            if (OriginGameObject == null)
            {
                Debug.LogError("You must specify an Origin Game Object for the Laser Sight! Perhaps use the Spawn Point of the Projectiles in the Weapon Prefab.");
            }
            _mask = _weapon.Stats.FiresProjectile.GetComponent<Projectile>().Mask;
        }
        void Update()
        {
            _startPoint = OriginGameObject.transform.position;
            RaycastHit hit;
            Physics.Raycast(_startPoint, OriginGameObject.transform.forward, out hit, MaxDistance, _mask);

            if (hit.transform == null) _endPoint = _startPoint + (OriginGameObject.transform.forward * MaxDistance);
            else _endPoint = hit.point;

            _lazor.SetPosition(0, _startPoint);
            _lazor.SetPosition(1, _endPoint);
        }
    }
}