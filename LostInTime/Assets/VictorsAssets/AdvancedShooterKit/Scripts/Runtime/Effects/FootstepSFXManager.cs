/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;
using AdvancedShooterKit.Utils;


namespace AdvancedShooterKit
{
    public class FootstepSFXManager : MonoBehaviour
    {
        [System.Serializable]
        public struct SurfaceData
        {
            public string name;
            public AudioClip jumpingSFX, landingSFX;
            public AudioClip[] footstepSounds;
        };

        [SerializeField]
        private SurfaceData generic = new SurfaceData();
        [SerializeField]
        private SurfaceData[] surfaces = new SurfaceData[ 0 ];


        private AudioSource m_Audio = null;


        // Use this for initialization
        void Awake()
        {
            m_Audio = GetComponent<AudioSource>();
            m_Audio.SetupForSFX();
        }


        // Play JumpingSound
        public void PlayJumpingSound( RaycastHit hit )
        {
            m_Audio.PlayOneShot( GetSurfaceByHit( hit ).jumpingSFX );
        }

        // Play LandingSound
        public void PlayLandingSound( RaycastHit hit )
        {
            m_Audio.PlayOneShot( GetSurfaceByHit( hit ).landingSFX );
        }

        // Play FootStepAudio
        public void PlayFootStepSound( RaycastHit hit )
        {
            AudioClip[] stepSounds = GetSurfaceByHit( hit ).footstepSounds;

            //Play RandomStepSound
            int index = Random.Range( 1, stepSounds.Length );
            m_Audio.clip = stepSounds[ index ];
            m_Audio.PlayOneShot( m_Audio.clip );
            stepSounds[ index ] = stepSounds[ 0 ];
            stepSounds[ 0 ] = m_Audio.clip;
        }


        // GetSurface ByHit
        private SurfaceData GetSurfaceByHit( RaycastHit hit )
        {
            m_Audio.outputAudioMixerGroup = GameSettings.SFXOutput;
            m_Audio.pitch = Time.timeScale;

            string surName = hit.GetSurface();

            for( int i = 0; i < surfaces.Length; i++ )
            {
                if( surfaces[ i ].name == surName )
                    return surfaces[ i ];
            }                

            return generic;
        }
    };
}