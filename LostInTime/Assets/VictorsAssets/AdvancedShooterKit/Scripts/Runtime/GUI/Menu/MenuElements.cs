/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace AdvancedShooterKit
{
    public class MenuElements : MonoBehaviour
    {
        public Slider
            lookSens,
            masterVol, musicVol, SFXVol, voiceVol;

        public Toggle
            showHud,
            invLookX, invLookY;

        public enum EFirstPanel { Gameplay, Audio }
        public EFirstPanel firstPanel = EFirstPanel.Gameplay;

        public GameObject gameplayPanel, audioPanel;

        ASKInputManager m_Input;


        // SetActive
        public void SetActive( bool value )
        {          
            if( value )
                m_Input.UnblockCursor();            
            else
                m_Input.BlockCursor();            

            gameObject.SetActive( value );
        }

        // Awake
        internal void AwakeMENU( ASKInputManager input )
        {
            m_Input = input;
        }

        // Start
        void Start()
        {
            if( firstPanel == EFirstPanel.Audio )
                gameplayPanel.SetActive( false );
            else
                audioPanel.SetActive( false );
        }

        // OnEnable
        void OnEnable()
        {
            if( !Application.isPlaying )
                return;

            showHud.isOn = GameSettings.ShowHud;
            invLookX.isOn = GameSettings.InvertLookX;
            invLookY.isOn = GameSettings.InvertLookY;
            //
            lookSens.value = GameSettings.LookSensitivity;
            //
            masterVol.value = GameSettings.MasterVolume;
            musicVol.value = GameSettings.MusicVolume;
            SFXVol.value = GameSettings.SFXVolume;
            voiceVol.value = GameSettings.VoiceVolume;
        }

        // Set Hud IsOn
        public void SetHudIsOn( bool value )
        {
            GameSettings.ShowHud = value;
        }
        // Set InvLookX IsOn
        public void SetInvLookXIsOn( bool value )
        {
            GameSettings.InvertLookX = value;
        }
        // Set InvLookY IsOn
        public void SetInvLookYIsOn( bool value )
        {
            GameSettings.InvertLookY = value;
        }

        // Set LookSens
        public void SetLookSens( float value )
        {
            GameSettings.LookSensitivity = value;
        }

        // Set MasterVolume
        public void SetMasterVolume( float value )
        {
            GameSettings.MasterVolume = value;
        }
        // Set MusicVolume
        public void SetMusicVolume( float value )
        {
            GameSettings.MusicVolume = value;
        }
        // Set SFXVolume
        public void SetSFXVolume( float value )
        {
            GameSettings.SFXVolume = value;
        }
        // Set VoiceVolume
        public void SetVoiceVolume( float value )
        {
            GameSettings.VoiceVolume = value;
        }


        // UnPause
        public void UnPause()
        {
            m_Input.Pause();
        }


        // Quit Game
        public void QuitGame()
        {
            #if UNITY_EDITOR
		    UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        // Restart Level
        public void StartReloadScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene( "Scene", LoadSceneMode.Single );
        }

        public void LoadScene(string sceneindex)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneindex, LoadSceneMode.Single);
        }
    }
}