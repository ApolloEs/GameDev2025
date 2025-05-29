/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using System.Collections;
using UnityEngine;
using AdvancedShooterKit.Utils;

namespace AdvancedShooterKit
{
    [RequireComponent( typeof( CapsuleCollider ) )]
    [RequireComponent( typeof( Rigidbody ) )]
    [RequireComponent( typeof( AudioSource ) )]       
    public class Shell : MonoBehaviour, IShell
    {
        [Range( 1f, 120f )]
        public float lifetime = 5f;
        public AudioClip hitSFX = null;

        AudioSource m_Audio;
        Rigidbody m_Rigidbody;


        // OnSpawn
        void IShell.OnSpawn( Vector3 outForce )
        {
            Init( outForce );
        }

        // Configure
        private void Init( Vector3 outForce )
        {
            this.MoveToCache();

            m_Audio = GetComponent<AudioSource>();
            m_Audio.SetupForSFX();

            m_Rigidbody = GetComponent<Rigidbody>();
            m_Rigidbody.AddForce( outForce, ForceMode.Impulse );
            m_Rigidbody.maxAngularVelocity = Random.Range( 26f, 38f );
            m_Rigidbody.linearVelocity = m_Rigidbody.angularVelocity = new Vector3( 0f, 2.5f, 0f );
            m_Rigidbody.constraints = RigidbodyConstraints.None;

            AddTorque( Random.Range( -.25f, .4f ) );

            StartCoroutine( RemoveShell() );
        }

        // Remove Shell
        private IEnumerator RemoveShell()
        {
            for( float el = 0f; el < lifetime; el += Time.deltaTime )
                yield return null;

            GetComponent<Collider>().enabled = false;
            Destroy( gameObject, .32f );
        }

        // OnCollisionEnter
        void OnCollisionEnter( Collision collision )
        {
            if( collision.relativeVelocity.magnitude < .5f )
            {
                return;
            }                

            m_Rigidbody.maxAngularVelocity = Random.Range( 12f, 18f );
            AddTorque( Random.Range( -.35f, .5f ) );
            m_Audio.pitch = Time.timeScale;
            m_Audio.PlayOneShot( hitSFX );
        }

        // AddTorque
        private void AddTorque( float value )
        {
            bool isPositive = ( Random.value > .5f );
            m_Rigidbody.AddRelativeTorque( Random.rotation.eulerAngles * ( isPositive ? value : -value ) );            
        }
    };
}