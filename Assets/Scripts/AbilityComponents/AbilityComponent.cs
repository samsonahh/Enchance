using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityComponent : MonoBehaviour
{
    protected private PlayerController _playerController;
    protected private AbilityCaster _abilityCaster;

    protected private float _castRadius;
    protected private float _circleCastRadius;
    protected private Vector3 _lastMouseWorldPosition;
    protected private Vector3 _lastCircleWorldPosition;
    protected private Vector3 _lastForwardDirection;

    protected private GameObject _lastTarget;
    
    public void Init(AbilityCaster ac, float cr, float ccr, Vector3 lmwp, Vector3 lcwp, Vector3 lfd, GameObject lt)
    {
        _playerController = ac.GetComponent<PlayerController>();
        _abilityCaster = ac;

        _castRadius = cr;
        _circleCastRadius = ccr;

        _lastMouseWorldPosition = lmwp;
        _lastCircleWorldPosition = lcwp;
        _lastForwardDirection = lfd;

        _lastTarget = lt;
    }
}
