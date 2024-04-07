using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockAI : EnemyController
{
    [Header("RockAI")]
    private RockAIState _currentState; 

    [SerializeField] private float _circleAroundPlayerRadius;

    public override void OnStart()
    {
        base.OnStart();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
    }
}

public enum RockAIState
{
    Chase
}
