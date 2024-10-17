using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerCircleArea : MonoBehaviour
{
    [SerializeField] private GameObject[] _enemyPrefabs;

    [SerializeField] private float _spawnRadius;
    private int _currentSpawned;
    [SerializeField] private int _maxSpawn;
    [SerializeField] private Vector2 _spawnInterval;

    private bool _stopped = false;

    private void Start()
    {
        StartCoroutine(SpawnCoroutine());
    }

    private void OnDrawGizmos()
    {
        CustomGizmos.DrawWireDisk(transform.position, _spawnRadius, Color.red);
    }

    private void Update()
    {
        _currentSpawned = transform.childCount;
    }

    private IEnumerator SpawnCoroutine()
    {
        while (!_stopped)
        {
            if(_currentSpawned >= _maxSpawn)
            {
                yield return null;
                continue;
            }

            int randomEnemyTypeIndex = Random.Range(0, _enemyPrefabs.Length);
            GameObject randomEnemyPrefab = _enemyPrefabs[randomEnemyTypeIndex];

            Vector2 randomCoords = _spawnRadius * Random.insideUnitCircle;
            Vector3 randomDest = new Vector3(randomCoords.x, 0, randomCoords.y) + transform.position;

            if(Vector3.Distance(randomDest, PlayerController.Instance.transform.position) < 1.5f)
            {
                continue;
            }

            GameObject enemy = Instantiate(randomEnemyPrefab, randomDest, Quaternion.identity);
            enemy.GetComponent<EnemyController>().Aggro();
            enemy.transform.SetParent(transform);

            yield return new WaitForSeconds(Random.Range(_spawnInterval.x, _spawnInterval.y));
        }

        while(transform.childCount > 0)
        {
            yield return null;
        }

        Destroy(gameObject);
    }

    public void Stop()
    {
        _stopped = true;
    }
}
