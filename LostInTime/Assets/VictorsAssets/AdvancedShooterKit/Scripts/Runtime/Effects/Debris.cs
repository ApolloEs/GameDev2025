/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using System.Collections;
using UnityEngine;

namespace AdvancedShooterKit
{
    using Utils;

    public class Debris : MonoBehaviour
    {
        [Range( 2f, 10f )]
        public float lifetime = 7f;

        [Range( 1f, 5f )]
        public float radius = 4f;

        [Range( .1f, 5f )]
        public float force = 2f;

        [Range( 2f, 35f )]
        public float upwards = 5f;

        public AudioClip crashSound = null;


        // OnEnable
        void OnEnable()
        {
            Transform m_Transform = transform.MoveToCache();
            ASKAudio.PlayClipAtPoint( crashSound, m_Transform.position ).SetParent( m_Transform );

            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach( Collider col in colliders )
            {
                Rigidbody colRb = col.attachedRigidbody;
                if( colRb != null && colRb.isKinematic == false )
                {
                    float randomValue = Random.value;
                    colRb.linearVelocity = Vector3.zero;
                    colRb.angularVelocity = Vector3.one * randomValue;
                    colRb.angularDamping = randomValue;
                    colRb.inertiaTensorRotation = Random.rotation;
                    colRb.AddExplosionForce( force * 1.75f / ( colRb.mass > 1f ? colRb.mass : 1f ), m_Transform.position, radius, upwards / 15f, ForceMode.Impulse );
                }
            }

            StartCoroutine( RemoveDebris( colliders ) );
        }

        // Remove Debris
        private IEnumerator RemoveDebris( Collider[] colliders )
        {
            for( float el = 0f; el < lifetime; el += Time.deltaTime ) {
                yield return null;
            }

            foreach( Collider col in colliders ) {
                col.enabled = false;
            }                

            Destroy( gameObject, .75f );
        }
    };
}