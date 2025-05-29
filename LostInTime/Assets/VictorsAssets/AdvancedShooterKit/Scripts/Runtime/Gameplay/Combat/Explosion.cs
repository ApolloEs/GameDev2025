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
    public class Explosion : MonoBehaviour
    {
        [SerializeField]
        private LayerMask hitMask = 1;

        [SerializeField, Range( 0f, 250f )]
        private float damage = 50f;

        [SerializeField, Range( 1f, 15f )]
        private float radius = 7f;

        [SerializeField, Range( 5f, 350f )]
        private float force = 100f;

        [SerializeField, Range( 0f, 2f )]
        private float upwardsModifier = .28f;
        
        [SerializeField]
        private GameObject fragments = null;
        [SerializeField]
        private AudioClip[] explosionSounds = null;


        Character owner;
        bool exploded;


        void Awake()
        {
            if( enabled == false )
                enabled = true;
        }

        void Update()
        {
            if( owner != null )
            {
                enabled = false;
                DoExplode( owner );
            }                
        }        


        // SetOwner
        void SetOwner( Character owner )
        {
            this.owner = owner;
        }

        // Set HitMask
        public void SetHitMask( LayerMask mask )
        {
            hitMask = mask;
        }

        void SetOwnerAndHitMask( Character owner, LayerMask mask )
        {
            this.owner = owner;
            hitMask = mask;
        }


        // Start Explode
        public void StartExplode()
        {
            DoExplode( null );
        }
        // Start Explode
        public void StartExplode( Character owner )
        {
            DoExplode( owner );
        }
        // Start Explode
        public void StartExplode( Character owner, float delay )
        {
            StartCoroutine( StartExplodeWithDelay( Mathf.Max( 0f, delay ), owner ) );
        }
        // Start Explode
        public void StartExplode( float delay )
        {
            StartCoroutine( StartExplodeWithDelay( Mathf.Max( 0f, delay ), null ) );
        }

        // StartExplode WithDelay
        private IEnumerator StartExplodeWithDelay( float delay, Character owner )
        {
            for( float el = 0f; el < delay; el += Time.deltaTime )
                yield return null;

            DoExplode( owner );
        }        
        // Explode
        private void DoExplode( Character owner )
        {
            if( exploded )
            {
                return;
            }                

            exploded = true;            

            Transform tr = transform;

            if( fragments != null ) {
                fragments.SpawnCopy( tr );
            }                

            Collider m_Collider = GetComponent<Collider>();
            Collider[] overlapColliders = Physics.OverlapSphere( tr.position, radius, hitMask );            

            foreach( Collider hitCol in overlapColliders )
            {
                if( hitCol == m_Collider || hitCol.isTrigger )
                {
                    continue;
                }                    

                Vector3 direction = hitCol.bounds.center - tr.position;

                RaycastHit hitInfo;
                if( Physics.Raycast( tr.position, direction, out hitInfo, radius, hitMask ) == false || hitCol != hitInfo.collider )
                {
                    continue;
                }
                    
                
                float distance = Vector3.Distance( tr.position, hitInfo.point );
                float percent = ( 100f - ( distance / radius * 100f ) ) / 100f;

                DamageHandler handler = hitCol.GetComponent<DamageHandler>();
                if( handler != null )
                {
                    handler.TakeDamage( new DamageInfo( damage * percent, tr, owner, EDamageType.Explosion ) );

                    if( handler.isPlayer ) {
                        handler.castToPlayerCharacter.getCamera.Shake( percent / 2f, percent );
                    }                        
                }

                Rigidbody hitRb = hitCol.attachedRigidbody;
                if( hitRb != null && hitRb.isKinematic == false )
                {
                    percent *= 1.75f;
                    hitRb.AddExplosionForce( force * percent / ( hitRb.mass > 1f ? hitRb.mass : 1f ), tr.position, radius, upwardsModifier, ForceMode.Impulse );
                }
            }


            float lifetime = 0f;


            // Play SFX
            AudioSource m_Audio = GetComponent<AudioSource>();
            if( m_Audio != null )
            {
                m_Audio.SetupForSFX();

                AudioClip sfxClip = ASKAudio.GetRandomClip( explosionSounds );
                m_Audio.PlayOneShot( sfxClip );

                if( sfxClip != null ) lifetime = sfxClip.length;
            }
            else
            {
                ASKAudio.PlayClipAtPoint( ASKAudio.GetRandomClip( explosionSounds ), tr.position );
            }


            // Spawn FX
            ParticleSystem explosionFX = GetComponent<ParticleSystem>();
            if( explosionFX != null )
            {
                var main = explosionFX.main;
                main.playOnAwake = main.loop = false;
                float startLifetime = main.startLifetime.constant;

                explosionFX.Play();

                lifetime = Mathf.Max( lifetime, startLifetime );
            }

            Destroy( gameObject.MoveToCache(), lifetime );
        }
    };
}