using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    [Title("Params")]
    public float MoveThreshold;
    public float AimCurveStiffness;
    public float AimOffset;
    public float ScopeAimOffset;
    public float ToAimDuration;
    public float OutAimDuration;

    [Title("References")]
    public Character character;

    float _aimPhase;
    float _offset;

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;

        float diff = pos.x - character.transform.position.x;
        if (Mathf.Abs(diff) > MoveThreshold)
            pos.x = character.transform.position.x + Mathf.Sign(diff) * MoveThreshold;

        if (character.IsAiming)
        {
            _aimPhase += Time.deltaTime / ToAimDuration;
            _aimPhase = Mathf.Min(_aimPhase, 1);
            _offset = (1 - Mathf.Pow(1 - _aimPhase, AimCurveStiffness)) * ((character.CurrentWeapon.Scope) ? ScopeAimOffset : AimOffset);
        }
        else
        {
            _offset -= ((character.CurrentWeapon.Scope) ? ScopeAimOffset : AimOffset) * Time.deltaTime / OutAimDuration;
            _offset = Mathf.Max(_offset, 0);
            _aimPhase = 1 - Mathf.Pow(1 - (_offset / ((character.CurrentWeapon.Scope) ? ScopeAimOffset : AimOffset)), 1 / AimCurveStiffness);
        }

        transform.position = pos + Vector3.right * _offset * character.Side;
    }
}
