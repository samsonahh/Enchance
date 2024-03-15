using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleAutoDelete : MonoBehaviour
{
    private ParticleSystem _particleSystem;

    private void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (_particleSystem)
        {
            if (!_particleSystem.IsAlive())
            {
                Destroy(gameObject);
            }
        }
    }
}
