using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationIndicator : MonoBehaviour
{
    private GameObject _sprite;

    void Start()
    {
        _sprite = transform.GetChild(0).gameObject;

        MakeSpriteVisible(false);
    }

    public void SetIndicatorPostion(Vector3 pos)
    {
        MakeSpriteVisible(true);
        transform.position = pos;
    }

    public void MakeSpriteVisible(bool b)
    {
        _sprite.SetActive(b);
    }
}
