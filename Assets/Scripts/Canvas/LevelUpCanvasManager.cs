using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUpCanvasManager : MonoBehaviour
{
    [SerializeField] private LevelUpCardDescriptionHandler _cardPrefab;

    [SerializeField] private Transform[] _cardPositions = new Transform[3];
    private LevelUpCardDescriptionHandler[] _cards = new LevelUpCardDescriptionHandler[3];

    private void OnEnable()
    {
        GenerateThreeRandomCards();
    }

    private void OnDisable()
    {
        RemoveAllCards();
    }

    private void GenerateThreeRandomCards()
    {
        for(int i = 0; i < 3; i++)
        {
            _cards[i] = Instantiate(_cardPrefab, _cardPositions[i]);
        }
    }

    private void RemoveAllCards()
    {
        for(int i = 0; i < 3; i++)
        {
            Destroy(_cards[i].gameObject);
            _cards[i] = null;
        }
    }
}
