// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;

namespace Deftly
{
    [System.Serializable]
    public class PlayerControllerStats
    {
        [SerializeField] public float TurnSpeed;
        [SerializeField] public float MoveSpeed;
        [SerializeField] public string AnimatorMoveSpeed;
        [SerializeField] public string AnimatorHorizontal;
        [SerializeField] public string AnimatorVertical;
        [SerializeField] public string AnimatorDie;
        [SerializeField] public string AnimatorRevive;
        [SerializeField] public string AnimatorReload;
        [SerializeField] public string AnimatorSwap;
        [SerializeField] public string AnimatorWeaponType;
    }
}