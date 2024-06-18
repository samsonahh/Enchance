using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceTotem : MonoBehaviour
{
    [SerializeField] private Transform _iwSpawn;

    [SerializeField] private GameObject _iceKeyPrefab;
    [SerializeField] private GameObject _flameObject;

    private bool _keyDropped = false;

    private void Start()
    {
        _flameObject.SetActive(false);

        LoadObjective();
    }

    private void Update()
    {
        if (_iwSpawn.childCount == 0)
        {
            if (!_keyDropped)
            {
                _keyDropped = true;

                DisableObjective();

                if (PlayerPrefs.GetInt("IceKey") == 0)
                {
                    Instantiate(_iceKeyPrefab, transform.position - Vector3.forward, Quaternion.identity);
                }
            }
        }
    }

    public void SaveObjective()
    {
        PlayerPrefs.SetInt("IceChallenge", 1);
    }

    public void LoadObjective()
    {
        if (!PlayerPrefs.HasKey("IceChallenge"))
        {
            return;
        }

        if (PlayerPrefs.GetInt("IceKey") == 0)
        {
            Instantiate(_iceKeyPrefab, transform.position - Vector3.forward, Quaternion.identity);
        }

        DisableObjective();
    }

    private void DisableObjective()
    {
        _flameObject.SetActive(true);

        Destroy(this);
    }
}
