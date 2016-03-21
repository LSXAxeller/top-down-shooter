// (c) Copyright Cleverous 2015. All rights reserved.

using System;
using UnityEngine;
using System.Collections;

namespace Deftly
{
    /// <summary>
    /// Sends a DeSpawn message to all components on it's host after the specified time has elapsed and then destroy's itself.
    /// </summary>
    /// TODO Entire Liftimer kind of setup idea needs to be revamped to be more efficient and sensible.
    public class Lifetimer : MonoBehaviour
    {
        public static Lifetimer AddTimer(GameObject onThis, float timeInSeconds, bool callDespawn)
        {
            Lifetimer omg = onThis.AddComponent<Lifetimer>();
            omg.Lifetime = timeInSeconds;
            omg.CallDespawn = callDespawn;

            return omg;
        }

        public float Lifetime;
        public bool CallDespawn;
        // TODO 'persist after use' option

        void Start()
        {
            StartCoroutine(StartLifetimer(Lifetime));
        }
        public IEnumerator StartLifetimer(float time)
        {
            yield return new WaitForSeconds(time);

            if (CallDespawn)
            {
                gameObject.SendMessage("DeSpawn", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                StaticUtil.DeSpawn(gameObject);
            }
            Destroy(this); // remove this script - important!
        }
    }
}