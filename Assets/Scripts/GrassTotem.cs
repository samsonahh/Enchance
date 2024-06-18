using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassTotem : MonoBehaviour
{
    [SerializeField] private Transform _emSpawn1;
    [SerializeField] private Transform _emSpawn2;
    [SerializeField] private Transform _emSpawn3;

    [SerializeField] private GameObject _grassKeyPrefab;
    [SerializeField] private GameObject _flameObject;

    private bool _keyDropped = false;

    private void Start()
    {
        _flameObject.SetActive(false);

        LoadObjective();
    }

    private void Update()
    {
        if(_emSpawn1.childCount == 0 && _emSpawn2.childCount == 0 && _emSpawn3.childCount == 0)
        {
            if (!_keyDropped)
            {
                _keyDropped = true;

                DisableObjective();

                if(PlayerPrefs.GetInt("GrassKey") == 0)
                {
                    Instantiate(_grassKeyPrefab, transform.position - Vector3.forward, Quaternion.identity);
                }
            }
        }
    }

    public void SaveObjective()
    {
        PlayerPrefs.SetInt("GrassChallenge", 1);
    }

    public void LoadObjective()
    {
        if (!PlayerPrefs.HasKey("GrassChallenge"))
        {
            return;
        }

        if (PlayerPrefs.GetInt("GrassKey") == 0)
        {
            Instantiate(_grassKeyPrefab, transform.position - Vector3.forward, Quaternion.identity);
        }

        DisableObjective();
    }

    private void DisableObjective()
    {
        _flameObject.SetActive(true);

        Destroy(this);
    }
}
