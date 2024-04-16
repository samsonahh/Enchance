using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private float _shakeSpeed = 20f;

    private void Awake()
    {
        Instance = this;
    }

    public void Shake(float d, float m)
    {
        StartCoroutine(ShakeCamera(d, m));
    }

    public IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;

        float elapsed = 0f;

        while(elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude * 10f;
            float y = Random.Range(-1f, 1f) * magnitude * 10f;

            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(x, y, originalPos.z), _shakeSpeed * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        while(Vector3.Distance(transform.localPosition, originalPos) >= 0.01f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPos, _shakeSpeed * Time.deltaTime);
            yield return null;
        }
        transform.localPosition = originalPos;
    }
}
