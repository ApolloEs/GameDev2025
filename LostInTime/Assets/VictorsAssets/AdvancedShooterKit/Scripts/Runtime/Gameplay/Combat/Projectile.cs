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
    using Utils;
    

    public class Projectile : MonoBehaviour, IProjectile
    {
        public int damageAmount = 20;
        public DamageInfo damageInfo;
        public enum EType : byte
        {
            Bullet,
            Arrow,
            Throw,
            Rocket
        };

        public enum EResetModes : byte
        {
            UpdateStart,
            UpdateEnd
        };


        /*[System.Serializable] // TODO: Prototype for next updates
        public struct ImpactData
        {
            public float insertValue;

            [Range( 10f, 90f )]
            public float outDispersion;

            [Range( .1f, 1f )]
            public float speedModifier, damageModifier;
        };*/

        [System.Serializable]
        public struct SurfaceData
        {
            public string name;

            [Range(.1f, 3f)]
            public float maxPenetration;
            [Range(10f, 90f)]
            public float outDispersion;

            [Range(0f, 1f)]
            public float ricochetChance;
            [Range(10f, 180f)]
            public float ricochetAngle;
            [Range(10f, 90f)]
            public float ricochetDispersion;

            // IsRicochet
            public bool IsRicochet(float angle)
            {
                return (Random.value < ricochetChance && angle < ricochetAngle);
            }

            // Create New
            public static SurfaceData standard
            {
                get
                {
                    return new SurfaceData
                    {
                        name = string.Empty,
                        maxPenetration = 1f,
                        outDispersion = 45f,
                        ricochetChance = .35f,
                        ricochetAngle = 90f,
                        ricochetDispersion = 75f
                    };
                }
            }
        };


        [SerializeField]
        private EType type = EType.Bullet;

        [SerializeField, Range(.1f, 30f)]
        private float lifetime = 2f;
        [SerializeField, Range(1f, 900f)]
        private float speed = 75f;
        [SerializeField, Range(-3f, 3f)]
        private float gravityPower = 1f;
        [SerializeField, Range(0f, 125f)]
        private float damage = 65f;

        [SerializeField]
        private SurfaceData generic = SurfaceData.standard;
        [SerializeField]
        private SurfaceData[] surfaces = new SurfaceData[0];

        [Range(1, 5)]
        public byte maxImpacts = 2;

        [SerializeField]
        private Explosion explosionObject = null;

        [SerializeField]
        private HitEffect decalObject = null;
        public Shell shellObject = null;
        [SerializeField]
        private AudioClip sound = null;
        [SerializeField]
        private bool impactAfterHit = false;
        [SerializeField]
        private Vector3 noise = Vector3.zero;
        [SerializeField]
        private EResetModes resetAngles = EResetModes.UpdateStart;


        // results
        byte impacts;
        bool movement;
        LayerMask m_HitMask = 1;
        Character m_Owner;
        Transform m_Transform;
        RaycastHit hitInfo;
        string hitSurface;
        Vector3 nextPosition, defaultAnges, currentVelocity;
        AudioSource m_Audio;


        // OnSpawn
        void IProjectile.OnSpawn(LayerMask hitMask, Character owner, float addDamage, float addSpeed)
        {
            m_HitMask = hitMask;
            m_Owner = owner;
            damage += addDamage;
            speed += addSpeed;

            Init();
        }

        // Init
        private void Init()
        {
            m_Transform = transform;
            nextPosition = m_Transform.position;

            movement = true;
            currentVelocity = m_Transform.forward * Random.Range(speed * .85f, speed * 1.15f);

            switch (type)
            {
                case EType.Arrow:
                    Pickup pickup = GetComponent<Pickup>();
                    if (pickup != null) pickup.enabled = false;

                    Collider col = GetComponent<Collider>();
                    if (col != null) col.enabled = false;
                    break;

                case EType.Throw:
                case EType.Rocket:
                    m_Audio = GetComponent<AudioSource>();
                    if (m_Audio == null)
                    {
                        Debug.LogWarning("AudioSource is not found! Warning in " + name);
                        m_Audio = gameObject.AddComponent<AudioSource>();
                    }

                    m_Audio.SetupForSFX();

                    if (type == EType.Rocket)
                    {
                        m_Audio.pitch = Time.timeScale;
                        m_Audio.loop = true;
                        m_Audio.clip = sound;
                        m_Audio.Play();
                        defaultAnges = m_Transform.localEulerAngles;
                    }
                    else if (type == EType.Throw)
                    {
                        Rigidbody myRb = GetComponent<Rigidbody>();
                        if (myRb != null)
                        {
                            myRb.isKinematic = false;
                            myRb.useGravity = true;
                            myRb.linearVelocity = currentVelocity;
                        }
                        else
                        {
                            Debug.LogError("The \"Throw\" projectiles must have Rigidbody. in " + name);
                        }

                        movement = false;
                    }
                    break;
            }

            StartCoroutine(RemoveProjectile());
        }

        // Remove Projectile
        private IEnumerator RemoveProjectile()
        {
            for (float el = 0f; el < lifetime; el += Time.deltaTime)
                yield return null;

            if (IsExplosionObject() == false)
                Destroy(gameObject);
        }


        // Fixed Update
        void FixedUpdate()
        {
            if (movement)
            {
                Move();
            }
        }

        // Move
        private void Move()
        {
            System.Func<Vector3, Vector3> OnCalcDrag = vel =>
            {
                Vector3 gravity = Physics.gravity;
                gravity.y *= gravityPower;
                return -.0024f * vel.sqrMagnitude / .2f * vel.normalized + gravity;
            };

            float deltaTime = Time.fixedDeltaTime;
            Vector3 currentPosition = m_Transform.position;
            Vector3 acceleartionFactorEuler = OnCalcDrag(currentVelocity);
            Vector3 velEuler = currentVelocity + deltaTime * acceleartionFactorEuler;
            nextPosition = currentPosition + deltaTime * .5f * (currentVelocity + velEuler);
            currentVelocity += deltaTime * .5f * (acceleartionFactorEuler + OnCalcDrag(velEuler)); // nextVelocity

            if (type == EType.Rocket)
            {
                if (resetAngles == EResetModes.UpdateStart)
                {
                    m_Transform.localEulerAngles = defaultAnges;
                }

                m_Transform.Rotate((Random.value > .5f ? noise : -noise) * .3f);
                nextPosition += m_Transform.forward * .5f;

                if (resetAngles == EResetModes.UpdateEnd)
                {
                    m_Transform.localEulerAngles = defaultAnges;
                }
            }

            Vector3 direction = nextPosition - currentPosition;
            float distance = direction.magnitude;
            direction.Normalize();

            if (Physics.Raycast(currentPosition, direction, out hitInfo, distance, m_HitMask) && hitInfo.collider.isTrigger == false)
            {
                hitSurface = hitInfo.GetSurface();
                Impact(direction);
                Physics.Raycast(currentPosition, direction, out hitInfo, distance, m_HitMask);
                return;
            }

            //Debug.DrawLine( m_Transform.position, nextPosition, Color.yellow );
            m_Transform.position = nextPosition;
        }




        // Impact
        private void Impact(Vector3 direction)
        {
            if (IsExplosionObject())
                return;

            bool showHitTexture = (type == EType.Bullet);

            DamageHandler handler = hitInfo.collider.GetComponent<DamageHandler>();
            if (handler != null)
            {
                handler.TakeDamage(new DamageInfo(damage, m_Transform, m_Owner, EDamageType.Impact));

                if (handler.isPlayer || handler.isNPC)
                {
                    showHitTexture = false;
                    impacts = maxImpacts;
                    hitSurface = handler.SurfaceType;
                }
            }

            Rigidbody hitRb = hitInfo.collider.attachedRigidbody;

            if (hitRb != null && hitRb.isKinematic == false)
            {
                hitRb.AddForce(m_Transform.forward * ((damage / 10f) * (damage * 20f) / 100f / (hitRb.mass > 1f ? hitRb.mass : 1f)), ForceMode.Impulse);
            }

            switch (type)
            {
                case EType.Arrow:
                    movement = false;
                    HitEffect.SpawnHitEffect(decalObject, hitInfo, hitSurface, false);

                    Collider myCollider = GetComponent<Collider>();
                    if (myCollider == null)
                    {
                        Debug.LogWarning("Collider is not found! Projectile(Arrow) has been destroyed! Warning in " + name);
                        Destroy(gameObject);
                        return;
                    }

                    myCollider.enabled = true;

                    Pickup pickup = GetComponent<Pickup>();
                    if (pickup != null) pickup.enabled = true;

                    m_Transform.position = hitInfo.point;
                    m_Transform.SetParent(hitInfo.transform);
                    break;

                case EType.Bullet:
                    ImpactBullet(direction, showHitTexture);
                    break;

                case EType.Rocket:
                    Destroy(gameObject);
                    break;

                default: break;
            }
        }

        // Impact
        private void ImpactBullet(Vector3 direction, bool showHitTexture)
        {
            if (hitInfo.collider.IsPlayer() || hitInfo.collider.IsNPC())
            {
                Destroy(gameObject);
                return;
            }

            impacts++;
            bool isAlive = (impacts < maxImpacts);
            movement = isAlive;

            if (isAlive)
            {
                // First

                HitEffect.SpawnHitEffect(decalObject, hitInfo, hitSurface, showHitTexture);

                SurfaceData hitData = FindSurface(hitSurface);

                // Ricochet
                Vector3 reflection = Vector3.Reflect(direction, hitInfo.normal);
                float angle = Vector3.Angle(direction, reflection);

                if (hitData.IsRicochet(angle))
                {
                    RotateToNextPoint(hitInfo.point, reflection, hitData.ricochetDispersion);
                    return;
                }

                // Penetration
                Vector3 hitPoint = hitInfo.point;
                Vector3 subOrigin = hitPoint + direction * (hitData.maxPenetration + 1f);
                Vector3 subDirection = hitPoint - subOrigin;
                subDirection.Normalize();

                RaycastHit[] subHits = Physics.RaycastAll(subOrigin, subDirection);

                for (int i = 0; i < subHits.Length; i++)
                {
                    if (subHits[i].collider == hitInfo.collider)
                    {
                        Vector3 subPoint = subHits[i].point;

                        float struckDistance = Vector3.Distance(hitPoint, subPoint);
                        if (struckDistance < hitData.maxPenetration)
                        {
                            HitEffect.SpawnHitEffect(decalObject, subHits[i], subHits[i].GetSurface(), showHitTexture, false);
                            RotateToNextPoint(subPoint, -subDirection, hitData.outDispersion);
                            //Debug.DrawLine( subPoint, hitPoint, Color.red );
                            return;
                        }
                        break;
                    }
                }

                Destroy(gameObject);
            }
            else
            {
                HitEffect.SpawnHitEffect(decalObject, hitInfo, hitSurface, showHitTexture);
                Destroy(gameObject);
            }
        }

        // Rotate ToNextPoint
        private void RotateToNextPoint(Vector3 position, Vector3 direction, float dispersion)
        {
            Vector2 outDispersion = Firearms.CalcDispersion(dispersion);
            Vector3 forward = Quaternion.Euler(outDispersion) * direction;
            forward.Normalize();

            m_Transform.forward = forward;
            m_Transform.position = position + forward * .01f;

            speed *= .75f;
            currentVelocity = m_Transform.forward * speed;

            //Debug.DrawLine( position, position + forward * 2f, Color.cyan );
        }

        // FindSurface ByHit
        private SurfaceData FindSurface(string hitSurName)
        {
            for (int i = 0; i < surfaces.Length; i++)
            {
                if (surfaces[i].name == hitSurName) return surfaces[i];
            }

            return generic;
        }


        // IsExplosion Object
        private bool IsExplosionObject()
        {
            if (explosionObject != null)
            {
                Vector3 spawnPos = m_Transform.position;

                if (type == EType.Throw)
                {
                    spawnPos += m_Transform.up * .25f;
                }
                else
                {
                    spawnPos -= m_Transform.forward * .25f;
                }

                Explosion explosionCopy = Instantiate(explosionObject, spawnPos, Random.rotation);
                explosionCopy.SetHitMask(m_HitMask);
                explosionCopy.StartExplode(m_Owner);

                Destroy(gameObject);

                return true;
            }

            return false;
        }



        // OnCollision Enter
        void OnCollisionEnter(Collision collision)
        {
            Debug.Log("Projectile Hit: " + collision.collider.name);

            DamageHandler damageHandler = collision.collider.GetComponentInParent<DamageHandler>();

            ZombieDamageHandler handler = collision.collider.GetComponentInParent<ZombieDamageHandler>();

            if (handler != null)
            {
                handler.TakeDamage(damageInfo);
            }


            if (damageHandler != null)
            {
                damageHandler.TakeDamage(damageInfo);
                Debug.Log("Damage applied via DamageHandler to: " + collision.collider.name);
            }
            else
            {
                Debug.Log("No DamageHandler found on: " + collision.collider.name);
            }

            Destroy(gameObject);










            if (type != EType.Throw)
            {
                return;
            }

            if (impactAfterHit)
            {
                IsExplosionObject();
            }
            else if (impactAfterHit == false && collision.relativeVelocity.magnitude > .5f)
            {
                m_Audio.pitch = Time.timeScale;
                m_Audio.PlayOneShot(sound);
            }
        }



        // OnDisable
        void OnDisable()
        {
            Destroy(gameObject);
        }
    };
}