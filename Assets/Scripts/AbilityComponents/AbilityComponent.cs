using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityComponent : MonoBehaviour
{
    protected private PlayerController _playerController;
    protected private Vector3 _lastForwardDir;
    protected private Vector3 _lastMouseWorldPos;
    protected private Vector3 _lastCircleWorldPos;
    protected private GameObject _target;

    protected private float _castRadius;
    protected private float _circleCastRadius;

    public void Init(PlayerController playerController, Vector3 lastForwardDir, Vector3 lastMouseWorldPos, Vector3 lastCircleWorldPos, GameObject lastTarget, float castRadius, float circleCastRadius)
    {
        _playerController = playerController;

        _lastForwardDir = lastForwardDir;
        _lastMouseWorldPos = lastMouseWorldPos;
        _lastCircleWorldPos = lastCircleWorldPos;

        _target = lastTarget;

        _castRadius = castRadius;
        _circleCastRadius = circleCastRadius;
    }
}
