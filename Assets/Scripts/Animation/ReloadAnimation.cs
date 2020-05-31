using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadAnimation : StateMachineBehaviour
{
    public float ChangeParentTime;
    public float DeactivateTime;
    public float ReactivateTime;
    public float RevertParentTime;

    Character _character;
    bool _changed;
    bool _deactivated;
    Transform _originalParent;
    Vector3 _originalPosition;
    Quaternion _originalRotation;
    float _time;


    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (!_character)
        {
            _character = animator.transform.parent.GetComponent<Character>();
            _originalParent = _character.CurrentWeapon.MagazineTransform.parent;
            _originalPosition = _character.CurrentWeapon.MagazineTransform.localPosition;
            _originalRotation = _character.CurrentWeapon.MagazineTransform.localRotation;
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (animatorStateInfo.normalizedTime > RevertParentTime)
            RevertParent();
        else if (animatorStateInfo.normalizedTime > ChangeParentTime && !_changed)
        {
            _changed = true;
            _character.CurrentWeapon.MagazineTransform.parent = _character.LeftHand;
        }

        if (animatorStateInfo.normalizedTime > ReactivateTime)
            Activate(false);
        else if (animatorStateInfo.normalizedTime > DeactivateTime)
            Activate(true);
    }

    void RevertParent ()
    {
        if (_changed)
        {
            _changed = false;
            _character.CurrentWeapon.MagazineTransform.parent = _originalParent;
            _character.CurrentWeapon.MagazineTransform.localPosition = _originalPosition;
            _character.CurrentWeapon.MagazineTransform.localRotation = _originalRotation;
        }
    }

    void Activate (bool activatedState)
    {
        if (_deactivated != activatedState)
        {
            _deactivated = activatedState;
            _character.CurrentWeapon.MagazineTransform.gameObject.SetActive(!activatedState);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        RevertParent();
        Activate(false);
    }
}
