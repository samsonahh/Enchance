using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyCollectable : MonoBehaviour
{
    [SerializeField] private RequiredKeys _keyType;

    [SerializeField] private Button _collectButton;

    private bool _onTrigger;

    private void Awake()
    {
        _collectButton.onClick.AddListener(() => {
            CollectKey();
        });

        _collectButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_onTrigger)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            CollectKey();
        }
    }

    private void CollectKey()
    {
        GameManager.Instance.BossRoomKeys[(int)_keyType] = 1;
        Destroy(gameObject);
    }

    public void AnimationDrop(Vector3 start, Vector3 end)
    {
        StartCoroutine(AnimateDropCoroutine(start, end));
    }

    IEnumerator AnimateDropCoroutine(Vector3 start, Vector3 end)
    {
        float distanceToDestination = Vector3.Distance(end, start);
        float timeToDestination = 1f;
        float launchMaxHeight = 1f;

        transform.position = start;

        for (float timer = 0f; timer < timeToDestination; timer += Time.deltaTime)
        {
            Vector3 horizontal = Vector3.Lerp(Vector3.zero, new Vector3(end.x - start.x, 0, end.z - start.z), timer / timeToDestination);

            float scaledParameter = (timer / timeToDestination) * distanceToDestination;
            float vertical = -((4 * launchMaxHeight) / (distanceToDestination * distanceToDestination)) * Mathf.Pow((scaledParameter - distanceToDestination / 2), 2) + launchMaxHeight;

            transform.position = start + horizontal + new Vector3(0, vertical, 0);

            yield return null;
        }
        transform.position = end;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            _collectButton.gameObject.SetActive(true);
            _onTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            _collectButton.gameObject.SetActive(false);
            _onTrigger = false;
        }
    }
}

public enum RequiredKeys
{
    Grass,
    Fire,
    Ice
}