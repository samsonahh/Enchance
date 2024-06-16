using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    private int _initialEnemyCount;
    private int _enemyCount;
    private int _prevEnemyCount;

    private void Start()
    {
        _initialEnemyCount = transform.childCount;
        _enemyCount = _initialEnemyCount;
        _prevEnemyCount = _enemyCount;

        LoadSpawn();
    }

    private void Update()
    {
        _enemyCount = transform.childCount;

        if(_prevEnemyCount != _enemyCount)
        {
            _prevEnemyCount = _enemyCount;
            OnEnemyDeath();
        }
    }

    private void OnEnemyDeath()
    {
        SaveSpawn();
    }

    public void SaveSpawn()
    {
        PlayerPrefs.SetInt(gameObject.name, _enemyCount);
    }

    public void LoadSpawn()
    {
        if (!PlayerPrefs.HasKey(gameObject.name))
        {
            return;
        }

        CorrectSpawn(PlayerPrefs.GetInt(gameObject.name));
    }

    public void CorrectSpawn(int count)
    {
        _enemyCount = count;

        int amtToKill = _initialEnemyCount - count;

        List<Transform> children = new List<Transform>();

        foreach(Transform child in transform)
        {
            children.Add(child);
        }

        for(int i = 0; i < amtToKill; i++)
        {
            Destroy(children[i].gameObject);
        }
    }
}
