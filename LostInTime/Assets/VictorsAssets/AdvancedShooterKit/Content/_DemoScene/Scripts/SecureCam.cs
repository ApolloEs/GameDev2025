using UnityEngine;
using AdvancedShooterKit;

public class SecureCam : MonoBehaviour
{
    Transform m_Body, m_PlayerTransform;
    WeaponBase m_Weapon;
    Animation m_Animation;

    PlayerCharacter m_Player;

    bool alive = true;

    
    // Start
    void Start()
    {        
        m_Body = GetComponentInChildren<Collider>().transform;

        GameObject playerGO = GameObject.FindGameObjectWithTag( TagsManager.Player );
        m_PlayerTransform = playerGO.transform;
        m_Player = playerGO.GetComponent<PlayerCharacter>();

        m_Weapon = GetComponentInChildren<WeaponBase>();

        m_Animation = GetComponentInChildren<Animation>();
        m_Animation.enabled = true;
        m_Animation.playAutomatically = true;
    }


    // Update
    void Update()
    {
        if( alive == false )
            return;

        if( m_Player.isAlive && Vector3.Distance( m_Body.position, m_PlayerTransform.position ) < 10f )
        {
            if( m_Animation.enabled )            
                m_Animation.enabled = false;            

            m_Body.rotation = Quaternion.Lerp( m_Body.rotation, Quaternion.LookRotation( m_PlayerTransform.position - m_Body.position ), 2f * Time.deltaTime );

            if( Vector3.Angle( m_PlayerTransform.position - m_Body.position, m_Body.forward ) < 15f )
                m_Weapon.StartShooting();
        }
        else
        {
            if( m_Animation.enabled == false )
            {
                m_Animation.enabled = true;
                m_Body.localEulerAngles = Vector3.right * 32f;
            }
        }
    }


    // OnRespawn
    void OnRespawn()
    {
        alive = true;
    }

    // ExplodeCam - call after characted die
    public void ExplodeCam()
    {
        if( alive )
        {
            alive = false;
            m_Animation.enabled = false;

            ParticleSystem camParticle = GetComponentInChildren<ParticleSystem>();

            var main = camParticle.main;
            main.playOnAwake = false;

            camParticle.Stop();
            camParticle.Play();
        }
    }
}
