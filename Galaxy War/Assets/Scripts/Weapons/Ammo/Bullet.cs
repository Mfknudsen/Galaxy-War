using Health;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapon
{
    public class Bullet : MonoBehaviour
    {
        [Header("Object Reference")]
        public LayerMask hitLayer = 0;
        public float speed = 50, grav = -9.81f;
        private Vector3 dirForce = Vector3.zero, gravForce = Vector3.zero;

        [Header("Damage Information")]
        private Damage damage = null;

        public void startBullet(float? newSpeed = 50)
        {


            speed = newSpeed.GetValueOrDefault();

            dirForce = transform.forward * speed;
        }

        private void Update()
        {
            transform.position += (dirForce + gravForce) * Time.deltaTime;

            gravForce += transform.up * grav * Time.deltaTime;
        }

        private void HitObject(GameObject obj)
        {
            Receiver r = obj.GetComponent<Receiver>();
            if (r != null)
                r.ReceiveNewDamage(damage);

            Destroy(gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == 1 << hitLayer)
            {
                HitObject(collision.gameObject);
            }
        }
    }
}
