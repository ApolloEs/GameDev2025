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
    [RequireComponent( typeof( Camera ) )]
    public sealed class PlayerCamera : MonoBehaviour
    {
        public LayerMask hitMask = 1;


        public Transform getTransform
        {
            get
            {
                if( m_Transform == null ) {
                    m_Transform = transform;
                }                    

                return m_Transform;
            }
        }
        public AudioSource getAudio { get; private set; }

        public Camera mainCamera { get; private set; }

        public bool isShaking { get; private set; }
        public bool isHitted { get; private set; }

        public RaycastHit hitInfo { get { return m_HitInfo; } }
        RaycastHit m_HitInfo;


        // need for calculation         
        bool dirtHair, inputUse;
        Collider hitCollider, prevHitCollider;
        Pickup tmpPickup;
        ECrosshair.EPointMode crossPointMode;

        Transform m_Transform;
        PlayerCharacter m_Player;
        HudElements m_Hud { get { return m_Player.hud; } }
        WeaponsManager m_WeaponsManager { get { return m_Player.weaponsManager; } }

        FirstPersonWeaponSway m_CurrentFPWeapSway;
        bool m_SwayZoomedOrZooming
        {
            get { return ( m_CurrentFPWeapSway != null && ( m_CurrentFPWeapSway.ironsightZoomed || m_CurrentFPWeapSway.ironsightZooming ) ); }
        }


        // Awake
        void Awake()
        {
            m_Transform = transform;
            mainCamera = GetComponent<Camera>();
            getAudio = GetComponentInChildren<AudioSource>();

            Transform root = m_Transform.root;
            m_Player = root.GetComponent<PlayerCharacter>();
            m_Player.SetCamera( this );
        }

        // SetCurrent FPWeapSway
        public void SetCurrentFPWeapSway( FirstPersonWeaponSway fpWeapSway )
        {
            m_CurrentFPWeapSway = fpWeapSway;
        }


        // Fixed Update
        void FixedUpdate()
        {
            UpdateHit();
        }

        // Update Hit
        public void UpdateHit()
        {
            isHitted = Physics.Raycast( m_Transform.position, m_Transform.forward, out m_HitInfo, Mathf.Infinity, hitMask );
            if( isHitted == false )
            {
                ResetPickup();
                return;
            }                

            hitCollider = m_HitInfo.collider;
            
            if( hitCollider.IsPickup() )
            {
                dirtHair = true;
                PickupActivity();                
            }
            else
            {
                SetCrosshairColor();
                ResetPickup();
            }
            
            prevHitCollider = hitCollider;
            // Debug.DrawLine( m_transform.position, hit.point );
        }


        // Set CrosshairColor
        private void SetCrosshairColor()
        {
            if( GameSettings.DamageIndication == EDamageIndication.OFF || hitCollider == prevHitCollider )
            {
                return;
            }

            DamageHandler handler = hitCollider.GetComponent<DamageHandler>();
            bool ready = ( handler != null && handler.isAlive );

            if( GameSettings.DamageIndication == EDamageIndication.ForAll )
            {
                if( ready && handler.isPlayer == false )
                    m_Hud.crosshair.SetColor( ECrosshair.EColor.Damager );
                else
                    m_Hud.crosshair.SetColor( ECrosshair.EColor.Normal );
            }
            else if( GameSettings.DamageIndication == EDamageIndication.OnlyCharacters )
            {
                if( ready && handler.isNPC )
                    m_Hud.crosshair.SetColor( ECrosshair.EColor.Damager );
                else
                    m_Hud.crosshair.SetColor( ECrosshair.EColor.Normal );
            }
        }


        // Use Item
        internal void UseItem()
        {
            if( inputUse || crossPointMode == ECrosshair.EPointMode.Cancel || m_Player.isAlive == false )
            {
                return;
            }               

            if( m_CurrentFPWeapSway != null )
            {
                if( m_CurrentFPWeapSway.ironsightZooming || m_CurrentFPWeapSway.isChanging || m_CurrentFPWeapSway.isPlaying )
                {
                    return;
                }   

                if( m_CurrentFPWeapSway.ironsightZoomed )
                {
                    m_CurrentFPWeapSway.IronsightUnzoom();
                    return;
                }
            }

            inputUse = true;
        }

        // Pickup Activity
        private void PickupActivity()
        {
            if( inputUse )
            {
                if( checkPickup ) {
                    tmpPickup.PickupItem( m_Player );
                }                    

                inputUse = false;
                ResetPickup();
            }
            else
            {
                if( hitCollider != prevHitCollider || tmpPickup == null )
                {
                    tmpPickup = hitCollider.GetComponent<Pickup>();
                    m_Hud.crosshair.SetColor( ECrosshair.EColor.Normal );
                }                                

                if( checkPickup == false || m_SwayZoomedOrZooming )
                {
                    return;
                }

                //if( GameSettings.ShowCrosshair && FirstPersonWeaponSway.ironsightZoomed == false && FirstPersonWeaponSway.ironsightZooming == false )


                switch( tmpPickup.PickupType )
                {
                    case Pickup.EType.Melee:
                    case Pickup.EType.Firearms:
                    case Pickup.EType.Thrown:
                        if( m_WeaponsManager.WeaponIsAvailable( tmpPickup.WeaponType ) )
                        {
                            if( ( tmpPickup.PickupType != Pickup.EType.Melee ) && ( tmpPickup.Amount > 0 ) && m_Player.ammoBackpack.IsFull( tmpPickup.AmmoType ) == false )
                                crossPointMode = ECrosshair.EPointMode.Ammo;
                            else
                                crossPointMode = ECrosshair.EPointMode.Cancel;
                        }
                        else if( m_WeaponsManager.WeaponTypeIsStandart( tmpPickup.WeaponType ) && m_WeaponsManager.crowded )
                            crossPointMode = ECrosshair.EPointMode.Swap;
                        else
                            crossPointMode = ECrosshair.EPointMode.Hand;
                        break;

                    case Pickup.EType.Ammo:
                        if( m_Player.ammoBackpack.IsFull( tmpPickup.AmmoType ) )
                            crossPointMode = ECrosshair.EPointMode.Cancel;
                        else
                            crossPointMode = ECrosshair.EPointMode.Ammo;
                        break;

                    case Pickup.EType.Health:
                        if( m_Player.isFull )
                            crossPointMode = ECrosshair.EPointMode.Cancel;
                        else
                            crossPointMode = ECrosshair.EPointMode.Health;
                        break;

                    default:
                        crossPointMode = ECrosshair.EPointMode.Hand;
                        break;
                }

                //crosshairMode
                m_Hud.crosshair.SetPointSprite( crossPointMode );
            }
        }

        // Check Pickup
        private bool checkPickup { get { return ( tmpPickup != null && tmpPickup.CheckDistance( m_Player.getTransform ) ); } }


        // Reset Pickup
        private void ResetPickup()
        {
            if( dirtHair == false )
            {
                return;
            }

            dirtHair = false;
            tmpPickup = null;
            prevHitCollider = null;

            if( m_SwayZoomedOrZooming )
            {
                return;
            }

            ECrosshair.EView view;
            crossPointMode = m_WeaponsManager.GetCurrentWeaponPointMode( out view );
            m_Hud.crosshair.SetPointSprite( crossPointMode );
            m_Hud.crosshair.SetActive( view );
        }
        

        // Shake
        public void Shake( float duration, float intensity )
        {
            if( isShaking )
            {
                return;
            }                

            isShaking = true;
            StopCoroutine( "StartShake" );
            StartCoroutine( StartShake( duration, intensity * 250f ) );
        }
        // Shake OneShot
        public void ShakeOneShot( float intensity )
        {
            StopCoroutine( "StartShake" );
            StartCoroutine( StartShake( .1f, intensity * 50f ) );
        }

        // Private Shake
        private IEnumerator StartShake( float duration, float intensity )
        {
            Vector3 originalPos =  m_Transform.localPosition;
            Quaternion originalRot = m_Transform.localRotation;
            
            for( float el = 0f; el < duration; el += Time.deltaTime )
            {
                float percentComplete = el / duration;
                float damper = 1f - Mathf.Clamp01( 4f * percentComplete - 3f );
                float shakeRange = damper * Random.Range( -intensity, intensity );

                Vector3 shakePos = originalPos + Random.insideUnitSphere * shakeRange * .035f;
                Quaternion shakeRot = originalRot * Quaternion.Euler( -shakeRange * .15f, 0f, shakeRange * 1.75f );

                float deltaStep = Time.smoothDeltaTime * 2f;
                m_Transform.localPosition = Vector3.Slerp( m_Transform.localPosition, shakePos, deltaStep );
                m_Transform.localRotation = Quaternion.Slerp( m_Transform.localRotation, shakeRot, deltaStep );

                yield return null;
            }

            isShaking = false;

            if( m_Player.isAlive == false )
            {
                PlayerDie();
            }                
        }
        

        // Player Die
        internal void PlayerDie()
        {
            m_Transform.localEulerAngles = Vector3.forward * 64f;
        }
    };
}