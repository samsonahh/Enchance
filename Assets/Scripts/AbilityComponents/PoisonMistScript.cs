using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonMistScript : MonoBehaviour
{
    [SerializeField] private int _poisonTicks = 3;
    [SerializeField] private float _duration = 3f;
    [SerializeField] private float _timeToDestination = 1f;
    [SerializeField] private float _launchMaxHeight = 2f;

    private float _radius;
    private Vector3 _destination;

    void Start()
    {
        transform.position = PlayerController.Instance.transform.position;
        _radius = AbilityCaster.Instance.CurrentAbilities[AbilityCaster.Instance.SelectedAbility].CircleCastRadius;
        _destination = PlayerController.Instance.LastCircleWorldPosition;

        StartCoroutine(LaunchAtDestination());
    }

    IEnumerator LaunchAtDestination()
    {
        Vector3 startPos = transform.position;

        for(float timer = 0f; timer < _timeToDestination; timer += Time.deltaTime)
        {
            Vector3 horizontal = Vector3.Lerp(Vector3.zero, new Vector3(_destination.x - startPos.x, 0, _destination.z - startPos.z), timer / _timeToDestination);

            float distanceToDestination = Vector3.Distance(_destination, startPos);
            float scaledParameter = (timer / _timeToDestination) * distanceToDestination;
            float vertical = -((4 * _launchMaxHeight) / (distanceToDestination * distanceToDestination)) * Mathf.Pow((scaledParameter - distanceToDestination/2), 2) + _launchMaxHeight;

            yield return null;
        }
        transform.position = _destination;

        StartCoroutine(ActivatePoisonMist());
    }

    IEnumerator ActivatePoisonMist()
    {
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = _radius;

        yield return null;
    }

}
