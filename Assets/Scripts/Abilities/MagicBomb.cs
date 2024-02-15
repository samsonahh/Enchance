using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicBomb : AbilityInstance
{
    [SerializeField] float _duration;
    private float _timer;

    private void Start()
    {
        _timer = 0f;
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer > _duration)
        {
            Destroy(gameObject);
        }
    }
}
