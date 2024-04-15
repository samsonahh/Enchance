using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonMistScript : MonoBehaviour
{
    [SerializeField] private int _poisonTicks = 3;
    [SerializeField] private float _duration = 3f;
    [SerializeField] private float _timeToDestination = 1f;
    [SerializeField] private float _launchMaxHeight = 2f;

    [SerializeField] private ParticleSystem _poisonLaunchParticle;
    [SerializeField] private ParticleSystem _particle;
    [SerializeField] private GameObject _poisonSplashPrefab;

    private float _radius;
    private Vector3 _destination;
    private bool _detectingCollisions;

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

            transform.position = startPos + horizontal + new Vector3(0, vertical, 0);

            yield return null;
        }
        transform.position = _destination;

        _poisonLaunchParticle.Stop();
        StartCoroutine(ActivatePoisonMist());
    }

    IEnumerator ActivatePoisonMist()
    {
        _detectingCollisions = true;
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = _radius;

        yield return new WaitForSeconds(_duration);

        _detectingCollisions = false;

        _particle.Stop();
        while (_particle)
        {
            if (!_particle.IsAlive())
            {
                Destroy(gameObject);
            }
            yield return null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_detectingCollisions) return;

        if(other.TryGetComponent(out PlayerController player))
        {
            player.PoisonPlayer(_poisonTicks);
            player.PoisonTicks = _poisonTicks;
        }
        if (other.TryGetComponent(out EnemyController enemy))
        {
            enemy.PoisonEnemy(_poisonTicks);
            enemy.PoisonTicks = _poisonTicks;
        }
        if (other.TryGetComponent(out BossAI boss))
        {
            boss.PoisonEnemy(_poisonTicks);
            boss.PoisonTicks = _poisonTicks;
        }
    }
}
