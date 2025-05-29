/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


/** Uncomment this 'define' for 'TouchControlsKit' integration. 
    OR Add to 'PlayerSettings->OtherSettings->Configuration->ScriptingDefineSymbols'.*/

//#define TOUCH_CONTROLS_KIT


#if TOUCH_CONTROLS_KIT
using TouchControlsKit;
using TCKAxisType = TouchControlsKit.EAxisType;
using TCKActionEvent = TouchControlsKit.EActionEvent;
#endif

using UnityEngine;
using UnityEngine.EventSystems;
using AdvancedShooterKit.Events;

namespace AdvancedShooterKit
{
    public class ASKInputManager : MonoBehaviour
    {
        public enum EUpdateType : byte
        {
            Update,
            LateUpdate,
            FixedUpdate,
            OFF
        };


        [System.Serializable]
        public struct UIPrefabs
        {
            public HudElements hudElements;
            public MenuElements menuElements;

#if TOUCH_CONTROLS_KIT
            public TCKInput touchUIElements;
#endif
        };

        [System.Serializable]
        public sealed class Axes
        {
            public string moveX = "Move Horizontal", moveY = "Move Vertical", lookX = "Look Horizontal", lookY = "Look Vertical";

#if TOUCH_CONTROLS_KIT
            public string moveJoystick = "Move Joystick", lookTouchpad = "Look Touchpad";
#endif
        }

        [System.Serializable]
        public sealed class Actions
        {
            public string
                fire = "Fire", zoom = "Zoom", run = "Run", jump = "Jump", crouch = "Crouch", use = "Use",
                reloadWeapon = "Reload Weapon",
                nextFiremode = "Next Firemode", nextAmmotype = "Next Ammotype", toSubweapon = "To Subweapon",
                dropWeapon = "Drop Weapon", prevWeapon = "Prev Weapon", nextWeapon = "Next Weapon",
                pause = "Pause", blockCursor = "Block Cursor", unblockCursor = "Unblock Cursor";
        }


        [SerializeField]
        private EUpdateType updateType = EUpdateType.Update;

#if TOUCH_CONTROLS_KIT
        public enum EInputType { Standalone = 0, TouchControlsKit = 1 }
        [SerializeField]
        private EInputType inputType = EInputType.Standalone;
#endif

        [SerializeField]
        private UIPrefabs m_UIPrefabs;

        [SerializeField]
        private Axes axes = null;

        [SerializeField]
        private Actions actions = null;


        public bool gameIsPaused { get; private set; }
        bool cursorIsBlocked = true;


        PlayerCharacter m_Player;        
        WeaponsManager m_WeaponsManager { get { return m_Player.weaponsManager; } }
        FirstPersonController m_Controller { get { return m_Player.fpController; } }

        HudElements m_Hud;
        MenuElements m_Menu;


        // Spawn UIElements 
        private void SpawnUIElements()
        {
            m_Hud = SpawnSingleUIElement( m_UIPrefabs.hudElements );
            m_Hud.AwakeHUD( gameObject );

            m_Menu = SpawnSingleUIElement( m_UIPrefabs.menuElements );
            m_Menu.AwakeMENU( this );

#if TOUCH_CONTROLS_KIT
            SpawnSingleUIElement<TCKInput>( m_UIPrefabs.touchUIElements );
#endif

            if( FindObjectOfType<EventSystem>() == null )
            {
                new GameObject( "EventSystem", typeof( EventSystem ), typeof( StandaloneInputModule ) );
            }                          
        }

        // Spawn SingleUIElement
        private static T SpawnSingleUIElement<T>( T prefab ) where T : MonoBehaviour
        {
            T[] lostPrafabs = FindObjectsOfType<T>();
            int lostSize = lostPrafabs.Length;

            for( int i = 1; i < lostSize; i++ )
            {
                Destroy( lostPrafabs[ i ].gameObject );
            }

            T curretElement = ( lostSize > 0 ) ? lostPrafabs[ 0 ] : null;
            if( curretElement == null )
            {
                if( prefab != null )
                {
                    curretElement = Instantiate( prefab );
                }                    
                else
                {
                    Debug.LogError( "Error: UI Prefab is not setup." );
                }                   
            }

            return curretElement;
        }

        
        // Awake
        void Awake()
        {
            gameIsPaused = false;
            SpawnUIElements();

            m_Player = GetComponent<PlayerCharacter>();            

            Time.timeScale = 1f;
        }

        // Start
        void Start()
        {
            GameSettings.UpdateMixerVolumes();
            m_Menu.SetActive( false );

            BindAllActions();
        }

        // OnDisable
        void OnDisable()
        {
            moveHorizontal = moveVertical = 0f;
            lookHorizontal = lookVertical = 0f;
            zoomAction = false;            
        }


        // Update
        void Update()
        {
            if( updateType == EUpdateType.Update )
                InputsUpdate();
        }
        // Late Update
        void LateUpdate()
        {
            if( updateType == EUpdateType.LateUpdate )
                InputsUpdate();
        }
        // Fixed Update
        void FixedUpdate()
        {
            if( updateType == EUpdateType.FixedUpdate )
                InputsUpdate();
        }


        // Bind AllActions
        private void BindAllActions()
        {
            InputSettings.BindAction( actions.jump, EActionEvent.Down, m_Controller.Jump );
            InputSettings.BindAction( actions.crouch, EActionEvent.Down, m_Controller.Crouch );

            InputSettings.BindAction( actions.use, EActionEvent.Down, m_Player.getCamera.UseItem );

            InputSettings.BindAction( actions.reloadWeapon, EActionEvent.Down, m_WeaponsManager.ReloadWeapon );
            InputSettings.BindAction( actions.nextFiremode, EActionEvent.Down, m_WeaponsManager.SwitchFiremode );
            InputSettings.BindAction( actions.nextAmmotype, EActionEvent.Down, m_WeaponsManager.SwitchAmmotype );
            InputSettings.BindAction( actions.toSubweapon, EActionEvent.Down, m_WeaponsManager.SwitchToSubWeapon );
            InputSettings.BindAction( actions.dropWeapon, EActionEvent.Down, m_WeaponsManager.DropCurrentWeapon );
            InputSettings.BindAction( actions.prevWeapon, EActionEvent.Down, m_WeaponsManager.SelectPreviousWeapon );
            InputSettings.BindAction( actions.nextWeapon, EActionEvent.Down, m_WeaponsManager.SelectNextWeapon );

            InputSettings.BindAction( actions.blockCursor, EActionEvent.Down, BlockCursor );
            InputSettings.BindAction( actions.unblockCursor, EActionEvent.Down, UnblockCursor );
        }


        // Inputs Update
        private void InputsUpdate()
        {            

#if TOUCH_CONTROLS_KIT
            if( inputType == EInputType.TouchControlsKit )
                TouchKitInput();
            else
#endif
                StandaloneInput();
        }

        // Standalone Input
        private void StandaloneInput()
        {
            if( InputSettings.GetAction( actions.pause, EActionEvent.Down ) )
                Pause();

            if( gameIsPaused )
                return;

            InputSettings.RunActions();
            InputSettings.RunActionAxis();
            InputSettings.RunAxis();            


            // Cursor lock
            if( cursorIsBlocked && Time.timeSinceLevelLoad > .1f )
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }


            moveHorizontal = InputSettings.GetAxis( axes.moveX );
            moveVertical = InputSettings.GetAxis( axes.moveY );
            lookHorizontal = InputSettings.GetAxis( axes.lookX ) * GameSettings.GetLookSensitivityByInvert_X;
            lookVertical = InputSettings.GetAxis( axes.lookY ) * GameSettings.GetLookSensitivityByInvert_Y;

            runAction = InputSettings.GetAction( actions.run, EActionEvent.Press );

            zoomAction = InputSettings.GetAction( actions.zoom, EActionEvent.Press );
            zoomActionDown = InputSettings.GetAction( actions.zoom, EActionEvent.Down );
            zoomActionUp = InputSettings.GetAction( actions.zoom, EActionEvent.Up );

            bool fireAction = InputSettings.GetAction( actions.fire, EActionEvent.Press );
            // Fire and Reset Weapon
            if( fireAction && m_Controller.isRunning == false )
                m_WeaponsManager.WeaponFire();
            else
                m_WeaponsManager.WeaponReset();

            
            // Select Weapon ByIndex
            if( Input.GetKeyDown( KeyCode.Alpha1 ) )
                m_WeaponsManager.SelectWeaponByIndex( 0 );
            else if( Input.GetKeyDown( KeyCode.Alpha2 ) )
                m_WeaponsManager.SelectWeaponByIndex( 1 );
            else if( Input.GetKeyDown( KeyCode.Alpha3 ) )
                m_WeaponsManager.SelectWeaponByIndex( 2 );
            else if( Input.GetKeyDown( KeyCode.Alpha4 ) )
                m_WeaponsManager.SelectWeaponByIndex( 3 );
            else if( Input.GetKeyDown( KeyCode.Alpha5 ) )
                m_WeaponsManager.SelectWeaponByIndex( 4 );
            else if( Input.GetKeyDown( KeyCode.Alpha6 ) )
                m_WeaponsManager.SelectWeaponByIndex( 5 );
            else if( Input.GetKeyDown( KeyCode.Alpha7 ) )
                m_WeaponsManager.SelectWeaponByIndex( 6 );
            else if( Input.GetKeyDown( KeyCode.Alpha8 ) )
                m_WeaponsManager.SelectWeaponByIndex( 7 );
            else if( Input.GetKeyDown( KeyCode.Alpha9 ) )
                m_WeaponsManager.SelectWeaponByIndex( 8 );
        }

#if TOUCH_CONTROLS_KIT
        // TouchKit Input
        private void TouchKitInput()
        {
            if( TCKInput.CheckController( actions.pause ) && TCKInput.GetAction( actions.pause, TCKActionEvent.Down ) )
                Pause();

            if( gameIsPaused )
                return;

            runAction = ( TCKInput.CheckController( actions.run ) && TCKInput.GetAction( actions.run, TCKActionEvent.Press ) );

            if( TCKInput.CheckController( actions.jump ) && TCKInput.GetAction( actions.jump, TCKActionEvent.Down ) )
                m_Controller.Jump();
            if( TCKInput.CheckController( actions.crouch ) && TCKInput.GetAction( actions.crouch, TCKActionEvent.Down ) )
                m_Controller.Crouch();
            
            if( TCKInput.CheckController( actions.use ) && TCKInput.GetAction( actions.use, TCKActionEvent.Down ) )
                m_Player.getCamera.UseItem();    
                    
            if( TCKInput.CheckController( actions.reloadWeapon ) && TCKInput.GetAction( actions.reloadWeapon, TCKActionEvent.Down ) )
                m_WeaponsManager.ReloadWeapon();
            if( TCKInput.CheckController( actions.nextFiremode ) && TCKInput.GetAction( actions.nextFiremode, TCKActionEvent.Down ) )
                m_WeaponsManager.SwitchFiremode();
            if( TCKInput.CheckController( actions.nextAmmotype ) && TCKInput.GetAction( actions.nextAmmotype, TCKActionEvent.Down ) )
                m_WeaponsManager.SwitchAmmotype();
            if( TCKInput.CheckController( actions.toSubweapon ) && TCKInput.GetAction( actions.toSubweapon, TCKActionEvent.Down ) )
                m_WeaponsManager.SwitchToSubWeapon();
            if( TCKInput.CheckController( actions.dropWeapon ) && TCKInput.GetAction( actions.dropWeapon, TCKActionEvent.Down ) )
                m_WeaponsManager.DropCurrentWeapon();
            if( TCKInput.CheckController( actions.prevWeapon ) && TCKInput.GetAction( actions.prevWeapon, TCKActionEvent.Down ) )
                m_WeaponsManager.SelectPreviousWeapon();
            if( TCKInput.CheckController( actions.nextWeapon ) && TCKInput.GetAction( actions.nextWeapon, TCKActionEvent.Down ) )
                m_WeaponsManager.SelectNextWeapon();


            if( TCKInput.CheckController( axes.moveJoystick ) )
            {
                moveHorizontal = Mathf.Clamp( TCKInput.GetAxis( axes.moveJoystick, TCKAxisType.Horizontal ), -1f, 1f );
                moveVertical = runAction ? 1f : Mathf.Clamp( TCKInput.GetAxis( axes.moveJoystick, TCKAxisType.Vertical ), -1f, 1f );
            }

            if( TCKInput.CheckController( axes.lookTouchpad ) )
            {
                lookHorizontal = TCKInput.GetAxis( axes.lookTouchpad, TCKAxisType.Horizontal ) * GameSettings.GetLookSensitivityByInvert_X;
                lookVertical = TCKInput.GetAxis( axes.lookTouchpad, TCKAxisType.Vertical ) * GameSettings.GetLookSensitivityByInvert_Y;
            }

            if( TCKInput.CheckController( actions.zoom ) )
            {
                zoomAction = TCKInput.GetAction( actions.zoom, TCKActionEvent.Press );
                zoomActionDown = TCKInput.GetAction( actions.zoom, TCKActionEvent.Down );
                zoomActionUp = TCKInput.GetAction( actions.zoom, TCKActionEvent.Up );
            }

            if( TCKInput.CheckController( actions.fire ) )
            {
                bool fireAction = TCKInput.GetAction( actions.fire, TCKActionEvent.Press );
                // Fire and Reset Weapon
                if( fireAction && m_Controller.isRunning == false )
                    m_WeaponsManager.WeaponFire();
                else
                    m_WeaponsManager.WeaponReset();
            }
        }
#endif

        // Bind Action
        public static void BindAction( string m_Name, EActionEvent m_Event, ActionHandler m_Handler )
        {
            InputSettings.BindAction( m_Name, m_Event, m_Handler );
        }
        // Unbind Action
        public static void UnbindAction( string m_Name, EActionEvent m_Event, ActionHandler m_Handler )
        {
            InputSettings.UnbindAction( m_Name, m_Event, m_Handler );
        }

        // Bind ActionAxis
        public static void BindActionAxis( string m_Name, EAxisState m_State, ActionHandler m_Handler )
        {
            InputSettings.BindActionAxis( m_Name, m_State, m_Handler );
        }
        // Unbind ActionAxis
        public static void UnbindActionAxis( string m_Name, EAxisState m_State, ActionHandler m_Handler )
        {
            InputSettings.UnbindActionAxis( m_Name, m_State, m_Handler );
        }

        // Bind Axis
        public static void BindAxis( string m_Name, AxisHandler m_Handler )
        {
            InputSettings.BindAxis( m_Name, m_Handler );
        }
        // Unbind Axis
        public static void UnbindAxis( string m_Name, AxisHandler m_Handler )
        {
            InputSettings.UnbindAxis( m_Name, m_Handler );
        }        
        

        // Get Action
        public static bool GetAction( string m_Name, EActionEvent m_Event )
        {
            return InputSettings.GetAction( m_Name, m_Event );
        }

        // Get ActionAxis
        public static bool GetActionAxis( string m_Name, EAxisState m_State )
        {
            return InputSettings.GetActionAxis( m_Name, m_State );
        }

        // Get Axis
        public static float GetAxis( string m_Name )
        {
            return InputSettings.GetAxis( m_Name );
        }


        // Block Cursor
        public void BlockCursor()
        {
            cursorIsBlocked = true;
        }
        // Unblock Cursor
        public void UnblockCursor()
        {
            cursorIsBlocked = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Pause
        public void Pause()
        {
            if( m_Player.isAlive == false )
            {
                return;
            }                

            gameIsPaused = !gameIsPaused;
            Time.timeScale = gameIsPaused ? 0f : 1f;
            m_Menu.SetActive( gameIsPaused );
            
            if( GameSettings.ShowHud ) {
                m_Hud.SetActive( !gameIsPaused );
            }                

#if TOUCH_CONTROLS_KIT
            TCKInput.SetActive( !gameIsPaused );
#endif
        }

        // PlayerDie
        internal void PlayerDie()
        {
            m_Menu.SetActive( true );

#if TOUCH_CONTROLS_KIT
            TCKInput.SetActive( false );
#endif
        }


        // move Horizontal 
        internal float moveHorizontal { get; private set; }
        // move Vertical 
        internal float moveVertical { get; private set; }

        // look Horizontal 
        internal float lookHorizontal { get; private set; }
        // look Vertical 
        internal float lookVertical { get; private set; }

        // run Action 
        internal bool runAction { get; private set; }

        // zoom Action 
        internal bool zoomAction { get; private set; }
        // zoom ActionDown
        internal bool zoomActionDown { get; private set; }
        // zoom ActionUp
        internal bool zoomActionUp { get; private set; }
    }
}