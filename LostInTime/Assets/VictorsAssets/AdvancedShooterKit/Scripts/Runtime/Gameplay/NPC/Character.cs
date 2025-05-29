/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using System;
using UnityEngine;
using AdvancedShooterKit.Utils;
using AdvancedShooterKit.Events;

namespace AdvancedShooterKit
{
    [RequireComponent( typeof( AudioSource ) )]
    public class Character : Health
    {
        [SerializeField]
        [Range( 1, 50 )]
        protected int damageToPain = 15;
        [SerializeField]
        [Range( 1, 99 )]
        protected int percentToPain = 10;

        [SerializeField]
        private AudioClip deathSound = null;
        [SerializeField]
        private AudioClip[] painSounds = null;

        [SerializeField]
        private int deathLayer = 2;

        [SerializeField]
        protected ASKEvent OnPain = new ASKEvent();


        public bool isActive { get { return gameObject.activeSelf; } }
        public override bool isNPC { get { return true; } }
        public Transform getTransform { get; protected set; }
        public AudioSource getAudio { get; protected set; }
        
        
        private int nativeLayer = 11;
        


        // Awake
        protected virtual void Awake()
        {
            getTransform = transform;
            nativeLayer = gameObject.layer;
            getAudio = GetComponent<AudioSource>();
        }

        // Start
        protected override void Start()
        {
            base.Start();
            getAudio.SetupForSFX();
        }

        // OnEnable
        protected virtual void OnEnable()
        {
            SetLayerForDamagePoints( nativeLayer );
        }

        // OnDisable
        protected virtual void OnDisable()
        {
            
        }


        // SetActive
        public void SetActive( bool value )
        {
            gameObject.SetActive( value );
        }


        // Spawn
        public Character SpawnCopy( Vector3 position, Quaternion rotation )
        {
            return SpawnCopy( position, rotation );
        }

        // OnRespawn
        protected override void OnRespawn()
        {
            base.OnRespawn();
        }


        // Kill
        public virtual void Kill()
        {
            if( isAlive )
            {
                currentHealth = 0;
                OnDie();
            }
        }

        
        // Take Damage
        public override void TakeDamage( DamageInfo damageInfo )
        {
            base.TakeDamage( damageInfo );
        }

        // DamageModifier ByDifficulty
        protected override float damageModifierByDifficulty
        {
            get
            {
                switch( GameSettings.DifficultyLevel )
                {
                    case EDifficultyLevel.Easy:    return 1.3f;
                    case EDifficultyLevel.Normal:  return 1f;
                    case EDifficultyLevel.Hard:    return .8f;
                    case EDifficultyLevel.Delta:   return .75f;
                    case EDifficultyLevel.Extreme: return .5f;
                    default: return 1f;
                }
            }
        }

        
        // Increment Health
        public override bool IncrementHealth( int addАmount )
        {
            bool result = base.IncrementHealth( addАmount );
            //...
            return result;
        }

        // Decrement Health
        public override bool DecrementHealth( int damage )
        {
            bool result = base.DecrementHealth( damage );
            
            // show pain if
            if( result && ( damage >= damageToPain || HealthInPercent <= percentToPain ) )
                PlayPainEffect();

            return result;
        }

        // Play PainEffect
        public virtual void PlayPainEffect()
        {
            OnPain.Invoke();

            if( painSounds.Length == 0 )
                return;

            getAudio.outputAudioMixerGroup = GameSettings.VoiceOutput;
            getAudio.pitch = Time.timeScale;
            getAudio.PlayOneShot( ASKAudio.GetRandomClip( painSounds ) );
        }

        // OnDie
        protected override void OnDie()
        {
            SetLayerForDamagePoints( deathLayer );

            getAudio.Stop();
            getAudio.outputAudioMixerGroup = GameSettings.VoiceOutput;
            getAudio.pitch = Time.timeScale;
            getAudio.PlayOneShot( deathSound );

            base.OnDie();
        }                

        // SetLayer
        private void SetLayerForDamagePoints( int layer )
        {
            Array.ForEach( GetComponentsInChildren<DamagePoint>(), point => point.gameObject.layer = layer );
        }
    };
}