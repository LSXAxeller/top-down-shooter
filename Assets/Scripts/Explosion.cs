using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class Explosion : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (!gameObject.GetComponent<ParticleSystem>().IsAlive())
        {
            Destroy(gameObject);
        }
    }
}