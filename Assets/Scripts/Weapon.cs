using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FireModeFlag
{
    Single = 1<<0,
    Burst = 1<<1,
    Automatic = 1<<2
}

public class Weapon : MonoBehaviour
{
    [Title("Gun Params")]
    /**/ public float StockRecoil;
    public float RecoilRecovery;
    /**/ public float ScopeSightDistance;

    [Title("Debug")]
    [ReadOnly] public bool Reloading;
    [ReadOnly] public WeaponItem CurrentWeapon;
    [Space]
    public bool Laser;
    public bool RedDot;
    public bool Scope;
    public bool Stock;

    [Title("References")]
    public WeaponItem WeaponAsset;
    public GameObject BulletPrefab;
    [Space]
    public Transform MagazineTransform;
    public Transform BulletPivot;
    [Space]
    public ParticleSystem muzzleParticles;
    public ParticleSystem sparkParticles;
    public Light muzzleflashLight;
    [Space]
    public GameObject LaserVisual;
    public GameObject RedDotVisual;
    public GameObject ScopeVisual;
    public GameObject StockVisual;

    float _initialRecoilVelocity;
    float _initialRecoilAngle;
    float _lastRecoilTimestamp;
    float _currentRecoilVelocity;
    float _currentRecoilAngle;

    Coroutine _muzzleFlash;
    float _lastShotTimestamp;
    bool _pressed;
    Vector2 _lastCross;
    float _reloadingTimestamp;

    private void Start()
    {
        muzzleflashLight.enabled = false;
        CurrentWeapon = Instantiate(WeaponAsset);
        CurrentWeapon.ItemCreated();
    }

    private void Update()
    {
        DebugMounts();
        CheckFire();
        ComputeRecoil(Time.time - _lastShotTimestamp);
        Reload();

        Character.Instance.CharacterAnimator.SetFloat("Aim Angle", Character.Instance.AimAngle + _currentRecoilAngle);
    }

    void DebugMounts ()
    {
        Vector2 cross = new Vector2(Input.GetAxis("Cross Horizontal"), Input.GetAxis("Cross Vertical"));
        if (cross.x == 1.0 && _lastCross.x != cross.x)
        {
            RedDot = !RedDot;
            if (RedDot)
                Scope = false;
        }
        else if (cross.x == -1.0 && _lastCross.x != cross.x)
        {
            Scope = !Scope;
            if (Scope)
                RedDot = false;
        }
        if (cross.y == 1.0 && _lastCross.y != cross.y)
            Stock = !Stock;
        else if (cross.y == -1.0 && _lastCross.y != cross.y)
            Laser = !Laser;
        _lastCross = cross;

        return;
        LaserVisual?.SetActive(Laser);
        RedDotVisual?.SetActive(RedDot);
        ScopeVisual?.SetActive(Scope);
        StockVisual?.SetActive(Stock);
    }

    void CheckFire ()
    {
        if (!InventoryUI.Instance.InMenu && !Reloading && CurrentWeapon.Magazine.AmmoCount > 0)
        {
            if (Input.GetAxis("Fire") >= 0.5f && (CurrentWeapon.FireMode == FireModeFlag.Automatic || !_pressed))
            {
                if (_lastShotTimestamp + (60f / CurrentWeapon.Firerate) <= Time.time)
                {
                    Fire(!_pressed);
                    _pressed = true;
                }
            }
            else
                _pressed = false;
        }
    }

    void Fire(bool firstShot)
    {
        CurrentWeapon.Magazine.AmmoCount--;

        Character.Instance.CharacterAnimator.SetTrigger("Shoot");

        if (_muzzleFlash != null)
            StopCoroutine(_muzzleFlash);
        _muzzleFlash = StartCoroutine(MuzzleFlashLight());
        muzzleParticles.Emit(1);
        sparkParticles.Emit(UnityEngine.Random.Range(1, 7));

        GameObject bullet = Instantiate(BulletPrefab, BulletPivot.transform.position, BulletPivot.transform.rotation);
        Vector3 vel = bullet.transform.forward * 400f;
        bullet.GetComponent<Rigidbody>().velocity = vel;

        float time = _lastShotTimestamp;
        if (firstShot)
            _lastShotTimestamp = Time.time;
        else
        {
            _lastShotTimestamp += 60f / CurrentWeapon.Firerate;
            bullet.transform.position += vel * (Time.time - _lastShotTimestamp);
        }

        ComputeRecoil(_lastShotTimestamp - time);
        float recoil = (Stock) ? StockRecoil : CurrentWeapon.Recoil;
        _initialRecoilAngle = _currentRecoilAngle;
        _initialRecoilVelocity = _currentRecoilVelocity + recoil;
    }

    private IEnumerator MuzzleFlashLight()
    {
        muzzleflashLight.enabled = true;
        yield return new WaitForSeconds(0.2f);
        muzzleflashLight.enabled = false;
    }

    //Smoothed curve (x: 0 -> 1) (y: 0 -> 1) (a: 1 -> inf.)
    //1 - (1 - x) ^ a

    //Harmonic Motion / Critical Damping with Inertia
    //Position: bx * exp(-ax)
    //Velocity: (1 - ax) * b * exp(-ax)
    //Acceleration: (ax - 2) * ab * exp(-ax)
    void ComputeRecoil(float time)
    {
        _currentRecoilVelocity = (-RecoilRecovery * time * _initialRecoilVelocity - RecoilRecovery * _initialRecoilAngle + _initialRecoilVelocity)
            * Mathf.Exp(-RecoilRecovery * time);
        _currentRecoilAngle = (time * _initialRecoilVelocity + _initialRecoilAngle) * Mathf.Exp(-RecoilRecovery * time);

        if (_currentRecoilAngle <= 0)
        {
            _currentRecoilVelocity = 0;
            _currentRecoilAngle = 0;
        }
        //transform.localEulerAngles = Vector3.forward * _currentRecoilAngle;
    }

    public bool CheckAimAssist ()
    {
        if (RedDot || Scope)
        {
            RaycastHit hit;
            if (Physics.Raycast(BulletPivot.position, BulletPivot.forward, out hit))
                return (hit.transform.tag == "Blood");
        }

        return false;
    }

    void Reload()
    {
        if (!Reloading)
        {
            if (Input.GetButtonDown("Reload"))
            {
                bool hasMagazine = false;
                foreach (Item item in Character.Instance.PlayerInventory.Items.Keys)
                {
                    MagazineItem mag = item as MagazineItem;
                    if (mag != null && mag.Ammo == CurrentWeapon.Ammo)
                    {
                        hasMagazine = true;
                        break;
                    }
                }

                if (hasMagazine)
                {
                    Reloading = true;
                    Character.Instance.CharacterAnimator.SetTrigger("Reload");
                    _reloadingTimestamp = Time.time;
                }
            }
        }
        else if (_reloadingTimestamp + CurrentWeapon.ReloadDuration <= Time.time)
        {
            Reloading = false;

            MagazineItem magazine = null;
            foreach (Item item in Character.Instance.PlayerInventory.Items.Keys)
            {
                MagazineItem mag = item as MagazineItem;
                if (mag != null && mag.Ammo == CurrentWeapon.Ammo)
                    if (magazine == null || mag.AmmoCount > magazine.AmmoCount)
                        magazine = mag;
            }

            if (magazine)
            {
                Character.Instance.PlayerInventory.TryAddItem(CurrentWeapon.Magazine);
                Character.Instance.PlayerInventory.RemoveItem(magazine);
                CurrentWeapon.Magazine = magazine;
            }
        }
    }
}
