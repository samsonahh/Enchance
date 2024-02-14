using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityInstance : MonoBehaviour
{
    protected AbilityCaster _abilityCaster;

    public virtual void Init(AbilityCaster ac)
    {
        _abilityCaster = ac;
    }
}
