/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using System.Collections;
using UnityEngine;
using AdvancedShooterKit.Utils;

#pragma warning disable 169

namespace AdvancedShooterKit
{
    public sealed class HitEffect : MonoBehaviour
    {      
        [System.Serializable]
        public struct SurfaceData
        {
            public string name;
            public Texture hitTexture;
            public AudioClip hitSound;
            public ParticleSystem hitParticle;

            public SurfaceData( SurfaceData sur )
            {
                name = sur.name;
                hitTexture = sur.hitTexture;
                hitSound = sur.hitSound;
                hitParticle = sur.hitParticle;
            }
        };


        [SerializeField]
        private float lifetime = 3.75f;
        [SerializeField]
        private int framesX = 2, framesY = 2;

        [SerializeField]
        private SurfaceData generic = new SurfaceData();
        [SerializeField]
        private SurfaceData[] surfaces = new SurfaceData[ 0 ];


        // Spawn HitEffect
        public static void SpawnHitEffect( HitEffect original, RaycastHit hitInfo, string hitSurface )
        {
            SpawnHitEffect( original, hitInfo, hitSurface, true, true );
        }
        // Spawn HitEffect
        public static void SpawnHitEffect( HitEffect original, RaycastHit hitInfo, string hitSurface, bool showTexture )
        {
            SpawnHitEffect( original, hitInfo, hitSurface, showTexture, true );
        }
        // Spawn HitEffect
        public static void SpawnHitEffect( HitEffect original, RaycastHit hitInfo, string hitSurface, bool showTexture, bool playSound )
        {
            if( hitInfo.collider == null || hitInfo.collider.isTrigger )
                return;

            if( original == null )
            {
                Debug.LogError( "ERROR: Decal Object is not setup!" );
                return;
            }

            SurfaceData surData = new SurfaceData( original.generic );

            for( int i = 0; i < original.surfaces.Length; i++ )
            {
                if( original.surfaces[ i ].name == hitSurface )
                {
                    surData = new SurfaceData( original.surfaces[ i ] );
                    break;
                }
            }

            HitEffect hitFXCopy = original.SpawnCopy( hitInfo.point + hitInfo.normal * .0003f, Quaternion.FromToRotation( Vector3.forward, hitInfo.normal ) );
            hitFXCopy.Configure( showTexture ? surData.hitTexture : null, playSound ? surData.hitSound : null, surData.hitParticle );
            hitFXCopy.transform.SetParent( hitInfo.transform );
        }


        // Configure
        private void Configure( Texture texture, AudioClip sound, ParticleSystem particle )
        {
            Transform m_Transform = transform;
            m_Transform.Rotate( 0f, 0f, Random.Range( -180f, 180f ), Space.Self ); //Random Rotation

            // Play SFX
            if( sound != null )
            {
                AudioSource fxAudio = gameObject.AddComponent<AudioSource>();
                fxAudio.SetupForSFX();
                fxAudio.PlayOneShot( sound );
            }

            // Spawn FX
            if( particle != null )
            {
                ParticleSystem particleCopy = particle.SpawnCopy( m_Transform );

                var main = particleCopy.main;
                main.playOnAwake = main.loop = false;
                float startLifetime = main.startLifetime.constant;

                particleCopy.Play();
                particleCopy.transform.SetParent( m_Transform );
                Destroy( particleCopy.gameObject, startLifetime );
            }

            // Show Texture
            if( texture != null )
            {
                MeshFilter mFilter = GetComponent<MeshFilter>();
                if( mFilter != null && mFilter.mesh != null )
                {
                    //Random UVs
                    int random = Random.Range( 0, framesX * framesY );
                    int x = Mathf.RoundToInt( random % framesX );
                    int y = Mathf.RoundToInt( random / framesY );

                    Vector2[] quadUVs = { Vector2.zero, Vector2.up, Vector2.right, Vector2.one };
                    Vector2[] meshUVs = new Vector2[ quadUVs.Length ];

                    //Debug.Log( "x: " + x + " y: " + y );
                    for( int i = 0; i < quadUVs.Length; i++ )
                    {
                        meshUVs[ i ].x = ( quadUVs[ i ].x + x ) * ( 1f / framesX );
                        meshUVs[ i ].y = ( quadUVs[ i ].y + y ) * ( 1f / framesY );
                    }

                    mFilter.mesh.uv = meshUVs;
                }
            }
            else
            {
                if( particle != null && sound != null )
                {
                    lifetime = Mathf.Max( sound.length, particle.main.startLifetime.constant );
                }
                else
                {
                    if( particle != null ) {
                        lifetime = particle.main.startLifetime.constant;
                    }

                    if( sound != null ) {
                        lifetime = sound.length;
                    }                        
                }
            }

            StartCoroutine( RemoveDecal( texture ) );
        }

        // Remove Decal
        private IEnumerator RemoveDecal( Texture texture )
        {
            Renderer myRenderer = GetComponent<Renderer>();
            Material mat = ( myRenderer != null ) ? myRenderer.material : null;

            if( mat != null )
            {
                if( texture != null )
                    mat.mainTexture = texture;
                else
                    mat.color = Color.clear;
            }

            for( float el = 0f; el < lifetime; el += Time.deltaTime )
                yield return null;

            // useFade
            if( texture != null && mat != null )
            {
                Color matColor = mat.color;
                while( matColor.a > 0f )
                {
                    matColor.a -= Time.smoothDeltaTime;
                    mat.color = matColor;
                    yield return null;
                }
            }

            Destroy( gameObject );
        }
        

        // OnDisable
        void OnDisable()
        {
            StopCoroutine( "RemoveDecal" );
            Destroy( gameObject );
        }
    };
}