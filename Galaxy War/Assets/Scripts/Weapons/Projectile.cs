using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    [Header("Projectile Information:")]
    [SerializeField] protected LayerMask hitMask = 0;

    [Header(" - Decay:")]
    [SerializeField] protected bool decayOverTime = true;
    [SerializeField, Tooltip("Time in Seconds!")] protected float decayTime = 120;
    protected float curDecay = 0;

    [Header(" - Gravity:")]
    [SerializeField] protected bool gravityEffect = false;
    [SerializeField] protected float gravity = -9.81f;

    [Header(" - Speed")]
    [SerializeField] protected float moveSpeed;

    public virtual void Update()
    {
        if (decayOverTime)
        {
            curDecay += Time.deltaTime;

            if (curDecay >= decayTime)
                Destroy(gameObject);
        }
    }

    public void Move()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        if (gravityEffect)
            transform.position += transform.up * gravity * Time.deltaTime;
    }

    public void ApplyDamage()
    {

    }

    public void SetSpeed(float moveSpeed)
    {
        this.moveSpeed = moveSpeed;
    }

    public void SetGravity(float gravity)
    {
        this.gravity = gravity;
    }

    public void SetUseGravity(bool gravityEffect)
    {
        this.gravityEffect = gravityEffect;
    }
}
