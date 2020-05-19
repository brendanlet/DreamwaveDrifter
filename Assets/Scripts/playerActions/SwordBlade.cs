using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordBlade : MonoBehaviour
{
    public int swordDamage;

    public GameObject bikePrefab;

    public void OnTriggerEnter(Collider col)
    {
        MonoBehaviour other = col.GetComponent<MonoBehaviour>();
        if (other is IDamageable)
        {
            IDamageable dmg = (IDamageable)other;
            dmg.TakeDamage(swordDamage);
        }
    }
}
