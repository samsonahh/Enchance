using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossAI : MonoBehaviour
{
    
}

public enum BossState
{
    Idle,
    Creep,
    ReadyingCharge,
    Charge,
    Cast
}
