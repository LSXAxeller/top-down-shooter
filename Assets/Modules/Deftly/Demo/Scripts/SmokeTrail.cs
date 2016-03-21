// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;

namespace Deftly
{
    public class SmokeTrail : MonoBehaviour
    {
        public AnimationCurve Curve;
        public Vector3 Scalar;
        public Vector3 Sanity;

        ParticleSystem _system;
        ParticleSystem.Particle[] _particles;

        void Reset()
        {
            Scalar = new Vector3(500f,500f,500f);
            Scalar = new Vector3(20f, 20f, 20f);
        }
        private void LateUpdate()
        {
            if (_system == null)
            {
                _system = GetComponent<ParticleSystem>();
                if (Scalar.x <= 0 | Scalar.y <= 0 | Scalar.z <= 0) Debug.LogError("SmokeTrail cannot process zero or negative values. Use a positive number.");
                if (Sanity.x <= 0 | Sanity.y <= 0 | Sanity.z <= 0) Debug.LogError("SmokeTrail cannot process zero or negative values. Use a positive number.");
            }
            if (_particles == null || _particles.Length < _system.maxParticles) 
                _particles = new ParticleSystem.Particle[_system.maxParticles];

            int count = _system.GetParticles(_particles);
            for (int i = 0; i < count; i++)
            {
                _particles[i].velocity += new Vector3(
                    Curve.Evaluate(i / Scalar.x * Random.Range(0.7f, 1.3f)) / Sanity.x,
                    Curve.Evaluate(i / Scalar.y * Random.Range(0.7f, 1.3f)) / Sanity.y,
                    Curve.Evaluate(i / Scalar.z * Random.Range(0.7f, 1.3f)) / Sanity.z);
            }
            _system.SetParticles(_particles, count);
        }
    }
}