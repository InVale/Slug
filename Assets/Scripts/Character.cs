using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public static Character Instance;

    [Title("Movement")]
    public float Acceleration;
    public float RunAcceleration;
    public float AimForwardAcceleration;
    public float AimBackwardAcceleration;
    public float Drag;
    public float Threshold;
    public float Gravity;
    public float GroundPadding;

    [Title("Aiming")]
    public float AimingAngleSpeed;
    public float AimingAngleSpeedGradient;
    public Vector2 AimingAngleLimits;

    [Title("Inventory")]
    public int InventoryLength;
    public int InventoryHeight;
    public Item[] StartItems;

    [Title("Debug")]
    [ReadOnly] public Vector3 Velocity;
    [ReadOnly] public float AimAngle;
    [ReadOnly] public bool IsAiming;
    [ReadOnly] public float Side = 1;

    [Title("References")]
    public GameObject BulletPrefab;
    [Space]
    public Weapon CurrentWeapon;
    public Animator CharacterAnimator;

    [NonSerialized] public Inventory PlayerInventory;
    [NonSerialized] public Inventory CurrentLoot;

    Rigidbody _rgbd;

    private void Awake()
    {
        if (Instance)
            Destroy(this);
        else
            Instance = this;
    }

    void Start()
    {
        _rgbd = GetComponent<Rigidbody>();

        PlayerInventory = new Inventory(InventoryLength, InventoryHeight);
        foreach (Item item in StartItems)
            PlayerInventory.TryAddItem(Instantiate(item));
    }

    void Update()
    {
        if (CurrentLoot != null && !InventoryUI.Instance.InMenu && Input.GetButtonDown("Submit"))
            InventoryUI.Instance.RenderInventories(PlayerInventory, CurrentLoot);

        CharacterInput input = AcquireInputs();

        Aiming(input);
        Movement(input);

        CharacterAnimator.SetFloat("Movement", Velocity.x * Side);
        CharacterAnimator.SetBool("Aim", IsAiming);
        CharacterAnimator.SetFloat("Aim Angle", AimAngle);
        CharacterAnimator.SetInteger("Side", Mathf.RoundToInt(Side));
    }

    struct CharacterInput
    {
        public Vector2 MovementInput;
        public Vector2 AimInput;
    }
    CharacterInput AcquireInputs ()
    {
        CharacterInput input = new CharacterInput();
        if (!InventoryUI.Instance.InMenu)
        {
            input.MovementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            input.AimInput = new Vector2(Input.GetAxis("AimHorizontal"), Input.GetAxis("AimVertical"));
        }

        return input;
    }

    void Movement (CharacterInput input)
    {
        Velocity = _rgbd.velocity;

        float horizontalInput = 0;
        if (Mathf.Abs(input.MovementInput.x) > 0.4f)
            horizontalInput = Mathf.Sign(input.MovementInput.x);
        if (horizontalInput != 0f && !IsAiming)
            Side = Mathf.Sign(horizontalInput);

        Velocity /= 1 + Drag * Time.deltaTime;
        float acceleration = (!IsAiming) ? ((Input.GetButton("Cancel")) ? RunAcceleration : Acceleration) : ((Side == Mathf.Sign(horizontalInput)) ? AimForwardAcceleration : AimBackwardAcceleration);
        if (horizontalInput != 0)
            Velocity.x += Mathf.Sign(horizontalInput) * acceleration * Time.deltaTime;
        if (Mathf.Abs(Velocity.x) < Threshold && horizontalInput == 0)
            Velocity.x = 0;

        Velocity.y -= Gravity * Time.deltaTime;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, GroundPadding))
        {
            Velocity.y = 0;
            _rgbd.MovePosition(hit.point);
        }

        _rgbd.velocity = Velocity;
    }

    void Aiming(CharacterInput input)
    {
        if (input.AimInput.magnitude < 0.4f)
        {
            input.AimInput = Vector2.zero;
            IsAiming = false;
        }
        else
            IsAiming = true;

        float angleSpeed = AimingAngleSpeed;
        if (CurrentWeapon.CheckAimAssist())
            angleSpeed *= AimingAngleSpeedGradient;
        float angle = Mathf.Clamp(Vector2.Angle(Vector2.right * Side, input.AimInput) * Mathf.Sign(input.AimInput.y), AimingAngleLimits.x, AimingAngleLimits.y);
        AimAngle = (Mathf.Abs(angle - AimAngle) <= angleSpeed * Time.deltaTime) ? angle : AimAngle + angleSpeed * Mathf.Sign(angle - AimAngle) * Time.deltaTime;
    }
}
