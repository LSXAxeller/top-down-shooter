// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;

namespace Deftly
{
    [AddComponentMenu("Deftly/Weapon Pickup")]
    public class WeaponPickup : MonoBehaviour
    {
        public GameObject GivesThisWeapon;
        public LayerMask CanPickThisUp;
        public AudioClip PickupSound;

        private Subject _subject;
        private PlayerController _controls;
        private bool _canPickup;

        void OnTriggerEnter(Collider col)
        {
            if (!StaticUtil.LayerMatchTest(CanPickThisUp, col.gameObject)) return;
            _subject = col.GetComponent<Subject>();
            _controls = col.GetComponent<PlayerController>();
            if (_subject == null || _controls == null)
            {
                return;
            }

            _canPickup = true;
        }
        void OnTriggerExit()
        {
            _canPickup = false;
        }
        void Update()
        {
            if (_canPickup && Input.GetButtonDown(_controls.Interact))
            {
                _subject.PickupWeapon(GivesThisWeapon);
                Done();
            }
        }
        void Done()
        {
            StaticUtil.DeSpawn(gameObject);
        }
    }
}