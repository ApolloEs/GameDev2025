/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;
using System.Collections;
using AdvancedShooterKit.Utils;
using AdvancedShooterKit.Events;

namespace AdvancedShooterKit
{
    public class Health : DamageHandler
    {
        [SerializeField]
        private bool immortal = false;
        public bool Immortal
        {
            get { return immortal; }
            set { immortal = value; }
        }

        [SerializeField]
        protected int maxHealth = 100;
        public int MaxHealth
        {
            get { return maxHealth; }
            set { maxHealth = value; }
        }

        [SerializeField]
        protected int currentHealth = 75;
        public int CurrentHealth { get { return currentHealth; } }

        [SerializeField]
        private bool regeneration = false;
        public bool Regeneration
        {
            get { return regeneration; }
            set { regeneration = value; }
        }

        [SerializeField]
        private int regAmount = 1;
        public int RegAmount
        {
            get { return regAmount; }
            set { regAmount = value; }
        }

        [SerializeField]
        private float regDelay = 2f;
        public float RegDelay
        {
            get { return regDelay; }
            set { regDelay = value; }
        }

        [SerializeField]
        private float regInterval = 1.25f;
        public float RegInterval
        {
            get { return regInterval; }
            set { regInterval = value; }
        }


        [SerializeField]
        private float
            spawnObjectsDelay = 0f
            , destroyBodyDelay = 0f;

        /*[SerializeField]
        private GameObject deathBody = null;*/

        [SerializeField]
        private GameObject[] deathObjects = null;
        [SerializeField]
        private bool dropOnlyOnePickup = false;
        [SerializeField]
        private Pickup[] deathDrops = null;


        public bool isFull { get { return currentHealth >= maxHealth; } }

        public override bool isAlive { get { return currentHealth > 0; } }

        
        [SerializeField]
        protected ASKEvent
            OnDamage = new ASKEvent()
            , OnDead = new ASKEvent();


#if UNITY_EDITOR
        [HideInInspector]
        public bool mainFo, eventsFo;
#endif


        // Health InPercent
        public int HealthInPercent
        {
            get
            {
                float percent = ( ( float )currentHealth / ( float )maxHealth ) * 100f;
                return Mathf.RoundToInt( percent );
            }
        }

        private int nativeHealth = 0;
        
        
        // Start
        protected virtual void Start()
        {
            nativeHealth = currentHealth;

            if( isAlive && regeneration && currentHealth < maxHealth )
                StartCoroutine( "StartRegeneration" );            
        }

        // OnEnable
        protected virtual void OnRespawn()
        {
            currentHealth = nativeHealth;
        }
        

        // Take Damage
        public override void TakeDamage( DamageInfo damageInfo )
        {
            base.TakeDamage( damageInfo );
            DecrementHealth( CalcDamage( damageInfo.value ) );
            OnDamage.Invoke();
        }

        // Increment Health
        public virtual bool IncrementHealth( int addАmount )
        {           
            if( ( currentHealth >= maxHealth ) || ( addАmount <= 0 ) )
                return false;
            
            currentHealth = Mathf.Min( maxHealth, currentHealth += addАmount );            
            return true;
        }

        // Decrement Health
        public virtual bool DecrementHealth( int damage )
        {
            if( immortal || isAlive == false || damage <= 0 )
                return false;

            currentHealth = Mathf.Max( 0, currentHealth -= damage ); //Debug.Log( "DecrementHealth! " + damage );

            if( isAlive )
            {
                if( regeneration )
                {
                    StopCoroutine( "StartRegeneration" );
                    StartCoroutine( "StartRegeneration" );
                }
            }
            else
            {
                OnDie();
            }            

            return true;
        }


        // OnDie
        protected virtual void OnDie()
        {
            if( regeneration ) {
                StopCoroutine( "StartRegeneration" );
            }                

            Vector3 nativePos = transform.position;            

            System.Action OnCallSpawning = () =>
            {
                var rend = GetComponentInChildren<Renderer>();
                Vector3 size = ( rend != null ) ? rend.bounds.size : Vector3.one;
                size /= 4f;

                nativePos.y += size.y / 2f;

                foreach( GameObject deathObj in deathObjects )
                {
                    if( deathObj != null )
                    {
                        deathObj.SpawnCopy( nativePos, Random.rotation )
                                .MoveToCache()
                                .Send( "SetOwner", lastDamage.owner )
                                .Send( "OnSpawn" );
                    }
                }                

                foreach( Pickup drop in deathDrops )
                {
                    if( drop != null )
                    {
                        Vector3 randomPos = nativePos;
                        randomPos.x += Random.Range( -size.x, size.x );
                        randomPos.y += Random.Range( -size.y, size.y );                        
                        randomPos.z += Random.Range( -size.z, size.z );

                        drop.SpawnCopy( randomPos, Random.rotation )
                            .MoveToCache()
                            .Send( "OnSpawn" );

                        if( dropOnlyOnePickup )
                            break;
                    }
                }                    
            };

            OnDead.Invoke();

            if( spawnObjectsDelay > 0 )            
                this.RunAction( OnCallSpawning, spawnObjectsDelay );            
            else            
                OnCallSpawning.Invoke();            

            if( isPlayer == false ) {
                Respawner.StartRespawn( gameObject, destroyBodyDelay );
            }
        }


        // Start Regeneration
        private IEnumerator StartRegeneration()
        {
            if( regDelay > 0f )
                yield return new WaitForSeconds( regDelay );

            float el = 0f;
            for( bool add = true; add; el += Time.deltaTime )
            {
                if( el > regInterval )
                {
                    el = 0f;
                    add = IncrementHealth( regAmount );
                }

                yield return null;
            }
        }
    };
}