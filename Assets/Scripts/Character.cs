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
    public LayerMask GroundLayer;

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
    float _lastX;

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
        _rgbd.velocity = Movement(input);

        CharacterAnimator.SetFloat("Movement", Velocity.x * Side);
        CharacterAnimator.SetBool("Aim", IsAiming);
        //CharacterAnimator.SetFloat("Aim Angle", AimAngle);
        CharacterAnimator.SetInteger("Side", Mathf.RoundToInt(Side));
        transform.root.eulerAngles = new Vector3(0, (Side > 0) ? 180 : 0, 0);
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

    Vector3 Movement (CharacterInput input)
    {
        float horizontalInput = 0;
        if (Mathf.Abs(input.MovementInput.x) > 0.3f)
            horizontalInput = Mathf.Sign(input.MovementInput.x);
        if (horizontalInput != 0f && !IsAiming)
            Side = Mathf.Sign(horizontalInput);

        Velocity.x /= 1 + Drag * Time.deltaTime;
        float acceleration = (!IsAiming) ? ((Input.GetButton("Cancel")) ? RunAcceleration : Acceleration) : ((Side == Mathf.Sign(horizontalInput)) ? AimForwardAcceleration : AimBackwardAcceleration);
        if (horizontalInput != 0)
            Velocity.x += Mathf.Sign(horizontalInput) * acceleration * Time.deltaTime;
        if (Mathf.Abs(Velocity.x) < Threshold && horizontalInput == 0)
            Velocity.x = 0;

        Velocity.y -= Gravity * Time.deltaTime;
        RaycastHit hit;
        Debug.DrawRay(transform.position + new Vector3(Side * 0.2f, 0.1f, 0), Vector3.down * (GroundPadding + 0.1f), Color.red, Time.deltaTime);
        if (Physics.Raycast(transform.position + new Vector3(Side * 0.2f, 0.1f, 0), Vector3.down, out hit, GroundPadding + 0.1f, GroundLayer))
        {
            Velocity.y = 0;
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle > 0.01f || angle < -0.01f)
                return new Vector3(Velocity.x, -Vector3.Dot(hit.normal, Velocity) / hit.normal.y, 0);
        }
        Debug.DrawRay(transform.position + new Vector3(0, 0.02f, 0), Vector3.down * GroundPadding, Color.red, Time.deltaTime);
        if (Physics.Raycast(transform.position + new Vector3(0, 0.02f, 0), Vector3.down, out hit, GroundPadding, GroundLayer))
        {
            Velocity.y = 0;

            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle < 0.01f || angle > -0.01f)
                _rgbd.MovePosition(hit.point);
            else if (Vector3.Dot(hit.normal, Velocity) > 0)
                return new Vector3(Velocity.x, -Vector3.Dot(hit.normal, Velocity) / hit.normal.y, 0);
        }

        return Velocity;
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
