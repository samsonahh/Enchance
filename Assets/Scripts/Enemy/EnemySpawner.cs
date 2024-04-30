using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemySpawner : NetworkBehaviour
{
    [SerializeField] private EnemyController _rockPrefab;

    bool _spawned = false;

    private void Update()
    {
        if (IsOwner)
        {
            if (!_spawned)
            {
                var rock = Instantiate(_rockPrefab, new Vector3(32f, 0, 4f), Quaternion.identity);
                rock.GetComponent<NetworkObject>().Spawn();
                _spawned = true;
            }
        }
    }
}
