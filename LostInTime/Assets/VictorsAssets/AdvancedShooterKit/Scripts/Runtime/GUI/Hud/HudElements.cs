/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace AdvancedShooterKit
{
    namespace ECrosshair
    {
        public enum EView : byte
        {
            OFF,
            Point,
            Cross,
            ALL
        };

        public enum EPointMode : byte
        {
            SmallPoint,
            BigPoint,
            Cancel,
            Hand,
            Swap,
            Ammo,
            Health
        };

        public enum EColor : byte
        {
            Normal,
            Damager
        };
    }
    
    public class HudElements : MonoBehaviour
    {
        [System.Serializable]
        public sealed class HealthBar
        {
            public RectTransform healthPanel = null;
            public Text healthText = null;
            
            public Image painScreen, painPointer;

            [Range( 25f, 250f )]
            public float pointerDistance = 175f;

            [Range( .1f, 4f )]
            public float painClearDelay = 1.75f;

            public Color32 painColor = Color.white;

            internal Vector3 damageTargetPosition { get; private set; }

            // SetDamage TargetPosition
            // Use Vector3.zero for hide painPointer and show only painScreen
            internal void SetDamageTargetPosition( Vector3 targetPosition )
            {
                damageTargetPosition = targetPosition;
            }
            
            Vector2 sizeDelta;
            float starterX;


            // Awake
            internal void Awake()
            {
                sizeDelta = healthPanel.sizeDelta;
                starterX = sizeDelta.x;
                painScreen.color = Color.clear;
                painPointer.color = Color.clear;
            }

            // Update Bar
            internal void UpdateBar( int currentHealth, int maxHealth )
            {
                sizeDelta.x = ( currentHealth * starterX ) / maxHealth;
                healthText.text = currentHealth.ToString();
                healthPanel.sizeDelta = sizeDelta;
            }

            // Set Active
            internal void SetActive( bool value )
            {
                healthPanel.gameObject.SetActive( value );
                healthText.gameObject.SetActive( value );
            }
        };
        
        [System.Serializable]
        public sealed class WeaponInformer
        {
            public Text allAmmoText, currentAmmoText;
            public Image ammoIcon, shootingModeIcon;
            public Sprite SMAutomatic, SMSingle, SMDouble, SMTriple;

            // Awake
            internal void Awake()
            {
                
            }


            // Update AmmoIcon
            internal void UpdateAmmoIcon( Sprite icon )
            {
                ammoIcon.sprite = icon;
            }

            // Update CurrentAmmoInfo
            internal void UpdateCurrentAmmoInfo( int value )
            {
                currentAmmoText.text = value.ToString();
            }

            // Update AllAmmoInfo
            internal void UpdateAllAmmoInfo( int value )
            {
                allAmmoText.text = value.ToString();
            }

            // Update ShootingModeIcon
            internal void UpdateShootingModeIcon( EFiringMode value )
            {
                Sprite newIcon = null;

                switch( value )
                {
                    case EFiringMode.Automatic:
                        newIcon = SMAutomatic;
                        break;

                    case EFiringMode.Single:
                        newIcon = SMSingle;
                        break;

                    case EFiringMode.Double:
                        newIcon = SMDouble;
                        break;

                    case EFiringMode.Triple:
                        newIcon = SMTriple;
                        break;

                    default:
                        newIcon = SMAutomatic;
                        break;
                }

                shootingModeIcon.sprite = newIcon;
            }

            // Set Active
            internal void SetActive( bool value )
            {
                ammoIcon.gameObject.SetActive( value );
                currentAmmoText.gameObject.SetActive( value );
                allAmmoText.gameObject.SetActive( value );
                shootingModeIcon.gameObject.SetActive( value );
            }
        }
        
        [System.Serializable]
        public sealed class Crosshair
        {
            public Color32 normalColor = new Color32( 255, 255, 255, 168 );
            public Color32 damagerColor = new Color32( 255, 146, 38, 168 );
            public Color32 onDamageColor = new Color32( 255, 0, 0, 168 );

            internal ECrosshair.EColor currentColorType { get; private set; }

            public Sprite bigPoint, smallPoint, cancelIcon, handIcon, swapIcon, ammoIcon, healthIcon;
            public RectTransform pointRT, upRT, downRT, leftRT, rightRT; // t is Transform/RectTransform
                                                                         //UP is y or Vector3.forward * value //DOWN is -y or Vector3.back * value //LEFT is -x or Vector3.left * value //RIGHT is x or Vector3.right * value            

            public Image damageIndicator = null;
            private Image pointImg, upImg, downImg, leftImg, rightImg;

            HudElements parent;

            // Awake
            internal void Awake( HudElements parent )
            {
                pointImg = pointRT.GetComponent<Image>();

                upImg = upRT.GetComponent<Image>();
                downImg = downRT.GetComponent<Image>();
                leftImg = leftRT.GetComponent<Image>();
                rightImg = rightRT.GetComponent<Image>();

                damageIndicator.color = Color.clear;
                SetColor( ECrosshair.EColor.Normal );

                this.parent = parent;
            }


            internal void SetColor( ECrosshair.EColor colorType )
            {
                Color32 newColor = Color.clear;

                switch( colorType )
                {
                    case ECrosshair.EColor.Normal:
                        newColor = normalColor;
                        break;

                    case ECrosshair.EColor.Damager:
                        newColor = damagerColor;
                        break;

                    default:
                        newColor = normalColor;
                        break;
                }

                currentColorType = colorType;
                pointImg.color = upImg.color = downImg.color = leftImg.color = rightImg.color = newColor;
            }


            // Set PointSprite
            internal void SetPointSprite( ECrosshair.EPointMode mode )
            {
                if( mode == ECrosshair.EPointMode.SmallPoint )
                {
                    pointRT.sizeDelta = Vector2.one * 2f;
                }
                else if( mode == ECrosshair.EPointMode.BigPoint )
                {
                    pointRT.sizeDelta = Vector2.one * 10f;
                }
                else
                {
                    pointRT.sizeDelta = Vector2.one * 64f;
                    SetActive( ECrosshair.EView.Point );
                }
                    

                switch( mode )
                {
                    case ECrosshair.EPointMode.SmallPoint:
                        pointImg.sprite = smallPoint;
                        break;
                    case ECrosshair.EPointMode.BigPoint:
                        pointImg.sprite = bigPoint; 
                        break;
                    case ECrosshair.EPointMode.Cancel:
                        pointImg.sprite = cancelIcon; 
                        break;
                    case ECrosshair.EPointMode.Hand:
                        pointImg.sprite = handIcon;
                        break;
                    case ECrosshair.EPointMode.Swap:
                        pointImg.sprite = swapIcon;
                        break;
                    case ECrosshair.EPointMode.Ammo:
                        pointImg.sprite = ammoIcon; 
                        break;
                    case ECrosshair.EPointMode.Health:
                        pointImg.sprite = healthIcon;
                        break;                  

                    default: 
                        break;
                }
            }

            // Update Position
            internal void UpdatePosition( float value )
            {
                if( parent.isActive == false )
                {
                    return;
                }                    

                value /= 2f;
                upRT.anchoredPosition3D = new Vector3( 0f, value, 0f );
                downRT.anchoredPosition3D = new Vector3( 0f, -value, 0f );
                leftRT.anchoredPosition3D = new Vector3( -value, 0f, 0f );
                rightRT.anchoredPosition3D = new Vector3( value, 0f, 0f );
            }

            // Set Active
            internal void SetActive( ECrosshair.EView view )
            {
                bool showCross = ( view == ECrosshair.EView.ALL || view == ECrosshair.EView.Cross );
                bool showPoint = ( view == ECrosshair.EView.ALL || view == ECrosshair.EView.Point );

                pointRT.gameObject.SetActive( showPoint );
                upRT.gameObject.SetActive( showCross );
                downRT.gameObject.SetActive( showCross );
                leftRT.gameObject.SetActive( showCross );
                rightRT.gameObject.SetActive( showCross );
            }
        }
                
        [SerializeField]
        private HealthBar m_HealthBar = new HealthBar();
        [SerializeField]
        private Crosshair m_Crosshair = new Crosshair();
        [SerializeField]
        private WeaponInformer m_WeaponInformer = new WeaponInformer();


        public HealthBar healthBar { get { return m_HealthBar; } }
        public Crosshair crosshair { get { return m_Crosshair; } }
        public WeaponInformer weaponInformer { get { return m_WeaponInformer; } }


        [SerializeField]
        private Transform m_RotationElements;
        
        GameObject cameraObj;

        Vector3 nativePosition, nativeRotation;

        CameraHeadBob m_HeadBob;
        PlayerCharacter m_Player;


        // Awake
        internal void AwakeHUD( GameObject owner )
        {
            cameraObj = GetComponentInChildren<Camera>().gameObject;
            
            m_HealthBar.Awake();
            m_Crosshair.Awake( this );
            m_WeaponInformer.Awake();
            
            nativePosition = m_RotationElements.localPosition;
            nativeRotation = m_RotationElements.localEulerAngles;
            
            SetActive( GameSettings.ShowHud );

            m_HeadBob = owner.GetComponent<CameraHeadBob>();
            m_Player = owner.GetComponent<PlayerCharacter>();

            m_Player.SetHud( this );
        }

        // Update
        void Update()
        {
            float smooth = Time.smoothDeltaTime * 15f;
            Vector3 targetValues = Vector3.zero;
            
            targetValues.x = nativeRotation.x - m_HeadBob.xTilt * 1.25f;
            targetValues.y = nativeRotation.y + m_HeadBob.yTilt * 3f;
            targetValues.z = nativeRotation.z;
            m_RotationElements.localRotation = Quaternion.Slerp( m_RotationElements.localRotation, Quaternion.Euler( targetValues ), smooth );
            
            targetValues.x = nativePosition.x - m_HeadBob.xPos * 100f;
            targetValues.y = nativePosition.y - m_HeadBob.yPos * 125f;
            targetValues.z = nativePosition.z;
            m_RotationElements.localPosition = Vector3.Lerp( m_RotationElements.localPosition, targetValues, smooth );
        }
        

        // SetActive
        public void SetActive( bool value )
        {
            isActive = value;

            if( cameraObj != null ) {
                cameraObj.SetActive( value );
            }                           
        }
        // IsActive
        public bool isActive { get; private set; }


        // Show PainScreen
        public void ShowPainScreen()
        {
            if( m_Player.isAlive == false )
            {
                return;
            }                

            StopCoroutine( "ClearPainScreen" );
            m_HealthBar.painScreen.color = Color.clear;
            m_HealthBar.painPointer.color = Color.clear;
            StartCoroutine( "ClearPainScreen" );
        }

        // Clear PainScreen
        private IEnumerator ClearPainScreen()
        {
            Color tmpColor = m_HealthBar.painColor;
            m_HealthBar.painScreen.color = tmpColor;

            if( m_HealthBar.damageTargetPosition != Vector3.zero )
            {
                m_HealthBar.painPointer.color = tmpColor;

                Transform playerTransform = m_Player.getTransform;

                float dx = playerTransform.position.x - m_HealthBar.damageTargetPosition.x;
                float dz = playerTransform.position.z - m_HealthBar.damageTargetPosition.z;

                float delay = Mathf.Atan2( dx, dz ) * Mathf.Rad2Deg - 270f - playerTransform.eulerAngles.y;
                delay *= Mathf.Deg2Rad;

                float pX = m_HealthBar.pointerDistance * Mathf.Cos( delay );
                float pY = m_HealthBar.pointerDistance * Mathf.Sin( delay );

                Vector2 newPointerPosition = new Vector2( pX, -pY );
                float angle = Mathf.Atan2( newPointerPosition.y, newPointerPosition.x ) * Mathf.Rad2Deg - 90f;

                RectTransform pointerRect = m_HealthBar.painPointer.rectTransform;
                pointerRect.anchoredPosition = newPointerPosition;
                pointerRect.rotation = Quaternion.AngleAxis( angle, Vector3.forward );
            }

            for( float el = 0f; el < m_HealthBar.painClearDelay; el += Time.deltaTime ) {
                yield return null;
            }

            for( ; tmpColor.a > 0f; tmpColor.a -= Time.deltaTime )
            {
                m_HealthBar.painScreen.color = tmpColor;
                m_HealthBar.painPointer.color = tmpColor;
                yield return null;
            }
        }
        

        // Show DamegeIndicator
        public void ShowDamegeIndicator()
        {
            if( m_Player.isAlive == false )
            {
                return;
            }                

            StopCoroutine( "ClearDamageIndicator" );
            m_Crosshair.damageIndicator.color = Color.clear;
            StartCoroutine( "ClearDamageIndicator" );
        }

        // Clear DamageIndicator
        private IEnumerator ClearDamageIndicator()
        {
            Color nextColor = m_Crosshair.onDamageColor;
            m_Crosshair.damageIndicator.color = nextColor;

            for( ; nextColor.a > 0f; nextColor.a -= Time.deltaTime )
            {
                m_Crosshair.damageIndicator.color = nextColor;
                yield return null;
            }
        }


        // Player Die
        internal void PlayerDie()
        {
            StopCoroutine( "ClearPainScreen" );
            StopCoroutine( "ClearDamageIndicator" );
            m_HealthBar.painScreen.color = m_HealthBar.painColor;
            m_Crosshair.SetActive( ECrosshair.EView.OFF );
            m_Crosshair.SetColor( ECrosshair.EColor.Normal );
            m_WeaponInformer.SetActive( false );
            m_HealthBar.SetActive( false );
        }
    };
}