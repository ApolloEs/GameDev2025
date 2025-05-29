/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;

namespace AdvancedShooterKit
{
    public sealed class PlayerCharacter : Character
    {
        public override bool isPlayer { get { return true; } }
        public override bool isNPC { get { return false; } }



        public ASKInputManager inputManager { get; private set; }
        public HudElements hud { get; private set; }
        public PlayerCamera getCamera { get; private set; }
        public FirstPersonController fpController { get; private set; }        
        public WeaponsManager weaponsManager { get; private set; }
        public AmmoBackpack ammoBackpack { get; private set; }


        // Awake
        protected override void Awake()
        {
            base.Awake();

            inputManager = GetComponent<ASKInputManager>();
            fpController = GetComponent<FirstPersonController>();            
            weaponsManager = GetComponentInChildren<WeaponsManager>( true );
            ammoBackpack = weaponsManager.GetComponent<AmmoBackpack>();
        }

        // Start
        protected override void Start()
        {
            hud.healthBar.UpdateBar( currentHealth, maxHealth );
            ECrosshair.EView view;
            hud.crosshair.SetPointSprite( weaponsManager.GetCurrentWeaponPointMode( out view ) );
            hud.crosshair.SetActive( view );
            base.Start();
        }

        // OnEnable
        protected override void OnEnable()
        {
            base.OnEnable();
            hud.SetActive( GameSettings.ShowHud );
        }

        // OnDisable
        protected override void OnDisable()
        {
            base.OnDisable();
            hud.SetActive( false );
        }


        // SetHud
        internal void SetHud( HudElements hud )
        {
            this.hud = hud;
        }

        // SetCamera
        internal void SetCamera( PlayerCamera camera )
        {
            this.getCamera = camera;
        }


        // Take Damage
        public override void TakeDamage( DamageInfo damageInfo )
        {
            hud.healthBar.SetDamageTargetPosition( damageInfo.source.position );

            int finalDamage = CalcDamage( damageInfo.value );
            float shakeRange = finalDamage / 10f;

            if( finalDamage < damageToPain && HealthInPercent > percentToPain )
            {
                switch( damageInfo.type )
                {
                    case EDamageType.Impact:
                    case EDamageType.Melee:
                        getCamera.ShakeOneShot( Random.Range( shakeRange, shakeRange * 1.65f ) );
                        hud.ShowPainScreen();
                        break;

                    default:
                        break;
                }
            }
            else
            {
                getCamera.ShakeOneShot( Random.Range( shakeRange, shakeRange * 1.65f ) );
                hud.ShowPainScreen();
            }

            DecrementHealth( finalDamage );
        }

        // DamageModifier ByDifficulty
        protected override float damageModifierByDifficulty
        {
            get
            {
                switch( GameSettings.DifficultyLevel )
                {
                    case EDifficultyLevel.Easy:    return .7f;
                    case EDifficultyLevel.Normal:  return 1f;
                    case EDifficultyLevel.Hard:    return 1.2f;
                    case EDifficultyLevel.Delta:   return 1.35f;
                    case EDifficultyLevel.Extreme: return 1.5f;
                    default: return 1f;
                }
            }
        }


        // Increment Health
        public override bool IncrementHealth( int addАmount )
        {
            bool result = base.IncrementHealth( addАmount );

            if( result )
                hud.healthBar.UpdateBar( currentHealth, maxHealth );

            return result;
        }

        // Decrement Health
        public override bool DecrementHealth( int damage )
        {
            bool result = base.DecrementHealth( damage );

            if( result )
                hud.healthBar.UpdateBar( currentHealth, maxHealth );

            return result;
        }        


        // OnDie
        protected override void OnDie()
        {
            base.OnDie();
            
            enabled = false;
            getCamera.PlayerDie();
            hud.PlayerDie();
            fpController.PlayerDie();

            foreach( Transform child in getCamera.getTransform ) {
                child.gameObject.SetActive( false );
            }                

            var bAnim = GetComponentInChildren<BodyAnimator>();
            if( bAnim != null ) bAnim.gameObject.SetActive( false );

            // Show menu after die.
            inputManager.PlayerDie();
        }
    };
}