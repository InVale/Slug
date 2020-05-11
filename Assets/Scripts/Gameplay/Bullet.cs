using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : SerializedMonoBehaviour
{
    [Tooltip("After how long should the bullet prefab be destroyed?")]
    public float destroyAfter;

    [Title("References")]
    public Dictionary<string, Transform[]> ImpactPrefabs;

    private void Start()
    {
        StartCoroutine(DestroyAfter());
    }

    private void OnCollisionEnter(Collision collision)
    {
        string tag = collision.transform.tag;
        if (ImpactPrefabs.ContainsKey(tag))
        {
            Transform[] impacts = ImpactPrefabs[tag];
            Instantiate(impacts[Random.Range(0, impacts.Length)], transform.position, Quaternion.LookRotation(collision.contacts[0].normal));
        }

        //if (collision.transform.tag == "Enemy")
            //collision.gameObject.GetComponent<Enemy>().isHit = true;

        Destroy(gameObject);
    }

    private IEnumerator DestroyAfter()
    {
        yield return new WaitForSeconds(destroyAfter);
        Destroy(gameObject);
    }
}
