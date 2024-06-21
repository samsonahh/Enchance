using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LavaSpawnerTotem : MonoBehaviour
{
    [SerializeField] private EnemySpawnerCircleArea _lavaSpawnerPrefab;
    [SerializeField] private KeyCollectable _fireKeyPrefab;
    [SerializeField] private GameObject _wallsObject;
    [SerializeField] private GameObject _flameObject;

    [SerializeField] private Button _startButton;
    [SerializeField] private TMP_Text _timerText;

    [SerializeField] private float _duration = 60f;

    private float _timer;
    private bool _started;
    private bool _onTrigger;
    private EnemySpawnerCircleArea _lavaSpawnerObject;

    private void Awake()
    {
        _startButton.onClick.AddListener(() => {
            StartSpawner();
        });
    }

    private void Start()
    {
        _startButton.gameObject.SetActive(false);
        _timerText.gameObject.SetActive(false);
        _wallsObject.SetActive(false);
        _flameObject.SetActive(false);

        _timer = _duration;

        LoadObjective();
    }

    private void Update()
    {
        if (_started)
        {
            _wallsObject.SetActive(true);
            _startButton.gameObject.SetActive(false);
            _timerText.gameObject.SetActive(true);

            _timerText.text = _timer.ToString(".00");

            _timer -= Time.deltaTime;

            if(_timer <= 0f) // win
            {
                _timer = 0f;
                _started = false;

                KeyCollectable key = Instantiate(_fireKeyPrefab, transform.position, Quaternion.identity);

                key.AnimationDrop(transform.position, transform.position - Vector3.forward);

                _lavaSpawnerObject.Stop();

                SaveObjective();
                DisableObjective();
            }
        }
        else
        {
            _wallsObject.SetActive(false);
            _timerText.gameObject.SetActive(false);

            if (_onTrigger)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    StartSpawner();
                }
            }
        }
    }

    public void SaveObjective()
    {
        PlayerPrefs.SetInt("LavaChallenge", 1);
    }

    public void LoadObjective()
    {
        if (!PlayerPrefs.HasKey("LavaChallenge"))
        {
            return;
        }

        if(PlayerPrefs.GetInt("FireKey") == 0)
        {
            Instantiate(_fireKeyPrefab, transform.position - Vector3.forward, Quaternion.identity);
        }

        DisableObjective();
    }

    private void DisableObjective()
    {
        Destroy(_startButton.transform.parent.gameObject);
        Destroy(_wallsObject);

        _flameObject.SetActive(true);

        Destroy(this);
    }

    private void StartSpawner()
    {
        _started = true;

        _timer = _duration;

        _lavaSpawnerObject = Instantiate(_lavaSpawnerPrefab, transform.position, Quaternion.identity);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_started) return;

        if(other.tag == "Player")
        {
            _startButton.gameObject.SetActive(true);
            _onTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            _startButton.gameObject.SetActive(false);
            _onTrigger = false;
        }
    }
}
