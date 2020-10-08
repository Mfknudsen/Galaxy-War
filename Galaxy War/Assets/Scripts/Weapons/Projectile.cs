using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Information:")]
    [SerializeField] protected LayerMask hitMask = 0;

    [Header(" - Gravity:")]
    [SerializeField] protected bool gravityEffect = false;
    [SerializeField] protected float gravity = -9.81f;

    [Header(" - Speed")]
    [SerializeField] protected float moveSpeed;

    public void Move()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        if (gravityEffect)
            transform.position += transform.up * gravity * Time.deltaTime;
    }

    public void ApplyDamage()
    {

    }
}
