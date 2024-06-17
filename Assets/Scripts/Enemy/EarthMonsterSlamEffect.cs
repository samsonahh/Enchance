using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthMonsterSlamEffect : MonoBehaviour
{
    private float _maxRadius;
    private float _duration;

    public void Init(float maxRadius, float duration)
    {
        _maxRadius = maxRadius;
        _duration = duration;
    }

    private void Start()
    {
        StartCoroutine(ExpandCouroutine());

        Destroy(gameObject, _duration * 1.5f);
    }

    IEnumerator ExpandCouroutine()
    {
        transform.localScale = new Vector3(0f, 25f, 0f);

        for(float t = 0; t < _duration; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(new Vector3(0f, 25f, 0f), new Vector3(_maxRadius, 25f, _maxRadius), t / _duration);

            yield return null;
        }

        transform.localScale = new Vector3(_maxRadius, 25f, _maxRadius);
    }
}
