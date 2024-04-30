using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonMistScript : AbilityComponent
{
    [SerializeField] private int _poisonTicks = 3;
    [SerializeField] private float _poisonSlowFraction = 0.75f;
    [SerializeField] private float _duration = 3f;

    [SerializeField] private ParticleSystem _poisonLaunchParticle;
    [SerializeField] private ParticleSystem _poisonCircleParticle;
    [SerializeField] private ParticleSystem _poisonSplashPrefab;
    [SerializeField] private GameObject _poisonCircle;

    private float _radius;
    private float _maxRadius;
    private Vector3 _destination;
    private bool _detectingCollisions;

    void Start()
    {
        transform.position = _playerController.transform.position;
        _radius = _circleCastRadius;
        _maxRadius = _castRadius;
        _destination = _lastCircleWorldPos;

        _poisonCircleParticle.gameObject.SetActive(false);

        var shape = _poisonCircleParticle.shape;
        shape.radius = _radius;

        _poisonCircle.transform.localScale = new Vector3(2f * _radius, 2f * _radius, 1);

        StartCoroutine(LaunchAtDestination());
    }

    IEnumerator LaunchAtDestination()
    {
        Vector3 startPos = transform.position;
        float distanceToDestination = Vector3.Distance(_destination, startPos);
        float timeToDestination = 0.1f + 0.9f * (distanceToDestination / _maxRadius);
        float launchMaxHeight = 2f * (distanceToDestination / _maxRadius);

        for (float timer = 0f; timer < timeToDestination; timer += Time.deltaTime)
        {
            Vector3 horizontal = Vector3.Lerp(Vector3.zero, new Vector3(_destination.x - startPos.x, 0, _destination.z - startPos.z), timer / timeToDestination);

            float scaledParameter = (timer / timeToDestination) * distanceToDestination;
            float vertical = -((4 * launchMaxHeight) / (distanceToDestination * distanceToDestination)) * Mathf.Pow((scaledParameter - distanceToDestination/2), 2) + launchMaxHeight;

            transform.position = startPos + horizontal + new Vector3(0, vertical, 0);

            yield return null;
        }
        transform.position = _destination;

        _poisonLaunchParticle.Stop();
        StartCoroutine(ActivatePoisonMist());
    }

    IEnumerator ActivatePoisonMist()
    {
        Instantiate(_poisonSplashPrefab, transform.position, Quaternion.identity);

        _poisonCircleParticle.gameObject.SetActive(true);

        Vector3 originalScale = _poisonCircle.transform.localScale;
        for (float timer = 0f; timer < 0.15f; timer += Time.deltaTime)
        {
            _poisonCircle.transform.localScale = Vector3.Slerp(new Vector3(0, 0, 1), originalScale, timer / 0.15f);
            yield return null;
        }
        _poisonCircle.transform.localScale = originalScale;

        _detectingCollisions = true;
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = _radius;

        yield return new WaitForSeconds(_duration);

        _detectingCollisions = false;

        _poisonCircle.SetActive(false);
        _poisonCircleParticle.Stop();
        while (_poisonCircleParticle)
        {
            if (!_poisonCircleParticle.IsAlive())
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
            player.ChangeCurrentMoveSpeed(player.PlayerRegularMoveSpeed * _poisonSlowFraction, _duration);
            player.PoisonTicks = _poisonTicks;
        }
        if (other.TryGetComponent(out EnemyController enemy))
        {
            enemy.PoisonEnemy(_poisonTicks);
            enemy.ChangeCurrentMoveSpeed(enemy.EnemyRegularMoveSpeed * _poisonSlowFraction, _duration);
            enemy.PoisonTicks = _poisonTicks;
        }
        if (other.TryGetComponent(out BossAI boss))
        {
            boss.PoisonEnemy(_poisonTicks);
            boss.ChangeCurrentMoveSpeed(_poisonSlowFraction, _duration);
            boss.PoisonTicks = _poisonTicks;
        }
    }
}
