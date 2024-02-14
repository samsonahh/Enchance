using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityInstance: MonoBehaviour
{
    private AbilityCaster _abilityCaster;

    public void Init(AbilityCaster ac)
    {
        _abilityCaster = ac;
    }


}
