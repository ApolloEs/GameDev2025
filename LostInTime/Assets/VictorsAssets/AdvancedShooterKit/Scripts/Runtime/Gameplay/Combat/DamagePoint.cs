/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;

namespace AdvancedShooterKit
{
    public sealed class DamagePoint : DamageHandler
    {
        [SerializeField, Range( 0f, 10f )]
        private float damageModifier = 1f;


        public override DamageInfo lastDamage { get { return checkHealth ? m_Health.lastDamage : new DamageInfo(); } }

        public override bool isAlive { get { return checkHealth ? m_Health.isAlive : false; } }

        public override bool isPlayer { get { return checkHealth ? m_Health.isPlayer : false; } }
        public override bool isNPC { get { return checkHealth ? m_Health.isNPC : false; } }


        Health m_Health;

        private bool checkHealth
        {
            get
            {
                bool result = ( m_Health != null );

                if( result == false ) {
                    Debug.LogError( "ERR: Health componenet is not setup in: " + name );
                }                    

                return result;
            }
        }


        // Start
        void Start()
        {
            m_Health = GetComponentInParent<Health>();
        }


        // Take Damage
        public override void TakeDamage( DamageInfo damageInfo )
        {
            if( checkHealth ) {
                m_Health.TakeDamage( new DamageInfo( damageInfo.value * damageModifier, damageInfo.source, damageInfo.owner, damageInfo.type ) );
            }                
        }
    };
}