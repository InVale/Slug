using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairsEntry : MonoBehaviour
{
    public enum EntryDirection
    {
        Left = -1,
        Right = 1
    }

    [Serializable]
    public struct ColliderStruct
    {
        public Collider @Collider;
        public bool Activate;
    }

    public EntryDirection Direction = EntryDirection.Left;
    [Space]
    public ColliderStruct[] UpColliders;
    public ColliderStruct[] LevelColliders;
    public ColliderStruct[] DownColliders;

    [Header("Debug")]
    public bool Debug;
    [ShowIf(nameof(Debug))] public Material ActivatedMaterial;
    [ShowIf(nameof(Debug))] public Material DeactivatedMaterial;

    const float _stairZ = -0.75f;
    const float _floorZ = -2f;

    void Start ()
    {
        HandleColliders(UpColliders, false);
        HandleColliders(DownColliders, false);
        HandleColliders(LevelColliders, true);
        Character.Instance.transform.position = new Vector3(Character.Instance.transform.position.x, Character.Instance.transform.position.y, _floorZ);
    }

    private void OnTriggerStay(Collider other)
    {
        Character character = other.GetComponentInParent<Character>();
        if (character)
        {
            if (Mathf.Sign(character.Velocity.x) == Mathf.Sign(character.transform.position.x - transform.position.x))
            {
                if (character.Velocity.x != 0 && Mathf.Sign(character.Velocity.x) == (float)Direction)
                {
                    float input = Input.GetAxis("Vertical");
                    if (Mathf.Abs(input) < 0.4f)
                    {
                        HandleColliders(UpColliders, false);
                        HandleColliders(DownColliders, false);
                        HandleColliders(LevelColliders, true);
                        Character.Instance.transform.position = new Vector3(Character.Instance.transform.position.x, Character.Instance.transform.position.y, _floorZ);
                    }
                    else if (Mathf.Sign(input) > 0)
                    {
                        HandleColliders(DownColliders, false);
                        HandleColliders(LevelColliders, false);
                        HandleColliders(UpColliders, true);
                        Character.Instance.transform.position = new Vector3(Character.Instance.transform.position.x, Character.Instance.transform.position.y, _stairZ);
                    }
                    else if (Mathf.Sign(input) < 0)
                    {
                        HandleColliders(UpColliders, false);
                        HandleColliders(LevelColliders, false);
                        HandleColliders(DownColliders, true);
                        Character.Instance.transform.position = new Vector3(Character.Instance.transform.position.x, Character.Instance.transform.position.y, _stairZ);
                    }
                }
                else
                {
                    HandleColliders(UpColliders, false);
                    HandleColliders(DownColliders, false);
                    HandleColliders(LevelColliders, true);
                }
            }
        }
    }

    void HandleColliders(ColliderStruct[] colliders, bool activated)
    {
        foreach (ColliderStruct collider in colliders)
        {
            collider.Collider.enabled = (collider.Activate == activated);
            if (Debug)
            {
                Renderer renderer = collider.Collider.GetComponent<Renderer>();
                if (renderer)
                    renderer.material = (collider.Activate == activated) ? ActivatedMaterial : DeactivatedMaterial;
            }
        }
    }
}
