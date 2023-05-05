using System.Collections.Generic;
using UnityEngine;

namespace Pegasus
{
    /// <summary>
    /// This will cause the targeted object to be followed by this object and has local avoidance as well.
    /// </summary>
    public class PegasusFollow : MonoBehaviour
    {
        //The thing we are chasing
        public Transform m_target;

        //Attachment & rotation to terrain
        [Header("Local Overrides")]
        [Tooltip("Always show gizmo's, even when not selected")]
        public bool m_alwaysShowGizmos = false;
        [Tooltip("Ground character to terrain")]
        public bool m_groundToTerrain = true;
        [Tooltip("Rotate character to terrain. Best for quadrapeds.")]
        public bool m_rotateToTerrain = false;
        [Tooltip("Avoid collisions with terrain trees.")]
        public bool m_avoidTreeCollisions = false;
        [Tooltip("Avoid collisions with objects on the object collision layer.")]
        public bool m_avoidObjectCollisions = false;
        [Tooltip("Distance to check for collisions. Bigger values will slow your system down.")]
        [Range(1f, 10f)]
        public float m_collisionRange = 2f;
        [Tooltip("Layers to check for collisions on. Ensure that your terrain is NOT included in these layers.")]
        public LayerMask m_objectCollisionLayer;

        private int m_currentCollisionCount = 0;
        private RaycastHit[] m_currentCollisionHitArray;
        private int m_currentTreeCollisionCount = 0;
        private List<TreeManager.TreeStruct> m_currentTreeCollisionHitArray = new List<TreeManager.TreeStruct>();
        private TreeManager m_terrainTreeManager;
        private Dictionary<int, Collider> m_myColliders = new Dictionary<int, Collider>();

        //Target walk & run speeds
        [Header("Character Speeds")]
        [Tooltip("Walk speed")]
        public float m_walkSpeed = 2f;
        [Tooltip("Run speed")]
        public float m_runSpeed = 7f;

        //Determine when we can stop
        [Header("Distance Thresholds")]
        [Tooltip("Minimum distance from target that character can stop.")]
        public float m_stopDistanceMin = 0.05f;
        [Tooltip("Maximum distance from target that character can stop.")]
        public float m_stopDistanceMax = 0.05f;
        private float m_currentStopDistance = 0.05f;
        private bool m_updateStopDistance = false;

        //Speed beypond which we should run
        [Tooltip("Character will run if further away from target than this distance, otherwise will walk or is stopped.")]
        public float m_runIfFurtherThan = 15f;

        [Header("Change Rates")]
        [Range(0.001f, 3f), Tooltip("Turn rate. Lower values produce smoother turns, larger values produce more accurate turns.")]
        public float m_turnChange = 1.5f;
        [Range(0.001f, 3f), Tooltip("Movement rate. Lower values produce smoother movement, larger values produce more accurate movement.")]
        public float m_movementChange = 1.5f;

        [Header("Path Randomisation")]
        [Range(0.0f, 2f), Tooltip("Deviation rate. Smaller values produce slower more subtle deviations, larger values produce more obvious and rapid deviations.")]
        public float m_deviationRate = 0f;
        [Tooltip("Deviation range in X plane")]
        public float m_maxDeviationX = 0f;
        [Tooltip("Deviation range in Y plane")]
        public float m_maxDeviationY = 0f;
        [Tooltip("Deviation range in Z plane")]
        public float m_maxDeviationZ = 0f;

        //Internal
        private Vector3 m_targetPosition = Vector3.zero;
        private float m_distanceToTarget = 0f;
        private float m_currentMovementDistance = 0f;
        private Vector3 m_currentVelocity = Vector3.zero;

        // Use this for initialization
        void Start()
        {
            if (m_target == null)
            {
                return;
            }
            m_updateStopDistance = false;
            m_targetPosition = m_target.position;
            m_distanceToTarget = (m_targetPosition - transform.position).magnitude;
            m_currentStopDistance = UnityEngine.Random.Range(m_stopDistanceMin, m_stopDistanceMax);
            m_currentCollisionCount = 0;
            m_currentCollisionHitArray = new RaycastHit[20]; //Change the size here if you want more collisions to be considered

            //Load up terrain trees if we are avoiding collisions
            if (m_avoidTreeCollisions)
            {
                m_terrainTreeManager = new TreeManager();
                m_terrainTreeManager.LoadTreesFromTerrain();
            }

            //Load up our own colliders - we will filter them out when checking for hits
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                m_myColliders.Add(collider.GetInstanceID(), collider);
            }
        }

        // Update is called once per frame
        void Update()
        {
            //No target then exit
            if (m_target == null)
            {
                return;
            }

            //Get target position plus noise influence
            Vector3 targetPosition = GetTargetPositionWithNoise(m_target.position);

            //Get target position plus collision influence
            targetPosition = GetTargetPositionWithCollisions(targetPosition);

            //If nothing to do then exit
            if (m_target.position == m_targetPosition && m_distanceToTarget <= m_currentStopDistance)
            {
                return;
            }

            //And save it
            m_targetPosition = targetPosition;

            //Determine direction of target
            Vector3 targetOffset = m_targetPosition - transform.position;
            m_distanceToTarget = targetOffset.magnitude;

            //Move to target
            if (m_distanceToTarget > m_currentStopDistance)
            {
                //Signal that we need a new stop distance then next time we stop
                m_updateStopDistance = true;

                //Rotate to target
                Vector3 targetRotation = new Vector3(targetOffset.x, 0f, targetOffset.z);
                if (m_deviationRate > 0f && m_maxDeviationY > 0f)
                {
                    targetRotation = new Vector3(targetOffset.x, targetOffset.y, targetOffset.z);
                }
                if (targetRotation == Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, m_turnChange * Time.deltaTime);
                }
                else
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetRotation), m_turnChange * Time.deltaTime);
                }

                //Move
                float distanceToMove = Time.deltaTime * m_walkSpeed;
                if (m_distanceToTarget > m_runIfFurtherThan)
                {
                    distanceToMove = Time.deltaTime * m_runSpeed;
                }
                m_currentMovementDistance = Mathf.Lerp(m_currentMovementDistance, distanceToMove, m_movementChange * Time.deltaTime);
                m_currentVelocity = transform.forward * m_currentMovementDistance;
                transform.position += m_currentVelocity;
                m_currentVelocity *= (1f / Time.deltaTime); //Determine velocity per second
            }
            else
            {
                m_currentMovementDistance = 0f;
                m_currentVelocity = Vector3.zero;
                if (m_updateStopDistance)
                {
                    m_updateStopDistance = false;
                    m_currentStopDistance = UnityEngine.Random.Range(m_stopDistanceMin, m_stopDistanceMax);
                }
            }
        }


        /// <summary>
        /// Fix up grounding and ground based rotation
        /// </summary>
        void LateUpdate()
        {
            if (m_target == null)
            {
                return;
            }

            if (m_groundToTerrain || m_rotateToTerrain)
            {
                Vector3 position = transform.position;
                Terrain terrain = GetTerrain(position);
                if (terrain == null)
                {
                    return;
                }

                position.y = terrain.SampleHeight(position);

                if (m_groundToTerrain)
                {
                    transform.position = position;
                }

                if (m_rotateToTerrain)
                {
                    Vector3 terrainLocalPos = terrain.transform.InverseTransformPoint(position);
                    Vector3 normalizedPos = new Vector3(Mathf.InverseLerp(0.0f, terrain.terrainData.size.x, terrainLocalPos.x),
                        Mathf.InverseLerp(0.0f, terrain.terrainData.size.y, terrainLocalPos.y),
                        Mathf.InverseLerp(0.0f, terrain.terrainData.size.z, terrainLocalPos.z));
                    Vector3 normal = terrain.terrainData.GetInterpolatedNormal(normalizedPos.x, normalizedPos.z);
                    //float steepness = terrain.terrainData.GetSteepness(normalizedPos.x, normalizedPos.z);
                    transform.rotation = Quaternion.FromToRotation(transform.up, normal) * transform.rotation;
                }
            }

            //Update collisions if seleted
            if (m_avoidObjectCollisions)
            {
                //Do sphere cast
                m_currentCollisionCount = Physics.SphereCastNonAlloc(transform.position, m_collisionRange, transform.forward, m_currentCollisionHitArray, 0f, m_objectCollisionLayer, QueryTriggerInteraction.UseGlobal);
            }

            if (m_avoidTreeCollisions)
            {
                //Load trees if not loaded
                if (m_terrainTreeManager == null)
                {
                    m_terrainTreeManager = new TreeManager();
                    m_terrainTreeManager.LoadTreesFromTerrain();
                }
                //Do tree check
                m_currentTreeCollisionCount = m_terrainTreeManager.GetTrees(transform.position, m_collisionRange + (m_collisionRange / 2f), ref m_currentTreeCollisionHitArray);
            }
        }

        /// <summary>
        /// Get target position influenced by noise
        /// </summary>
        /// <returns>Target position as influenced by noise</returns>
        Vector3 GetTargetPositionWithNoise(Vector3 targetPosition)
        {
            //Offset target position by noise
            if (m_deviationRate > 0f)
            {
                float noise = (-0.5f + Mathf.PerlinNoise(targetPosition.x * m_deviationRate, targetPosition.z * m_deviationRate)) * 2f;
                Vector3 offset = new Vector3(noise * m_maxDeviationX, noise * m_maxDeviationY, noise * m_maxDeviationZ);
                targetPosition += offset;
            }
            //And retrun
            return targetPosition;
        }

        /// <summary>
        /// Get target position inflenced by collisions
        /// </summary>
        /// <returns>Traget position influenced by avoiding collisions</returns>
        Vector3 GetTargetPositionWithCollisions(Vector3 targetPosition)
        {
            //Collisions are updated in late update - exit quick if we can
            if (m_currentCollisionCount == 0 && m_currentTreeCollisionCount == 0)
            {
                return targetPosition;
            }

            //Now process the collisions and update the target accordingly
            RaycastHit hit;
            float repulsion = 0f;
            float distanceSqr = 0f;
            float scale = 0f;
            float sqrCollisionRange = m_collisionRange * m_collisionRange;
            Vector3 desiredVelocity;
            Vector3 targetOffset = Vector3.zero;
            Vector3 position = transform.position;

            //Project our position 0.5 second into future based on current velocity
            //Debug.DrawRay(position, m_currentVelocity * m_predictFuture, Color.magenta);
            position += (m_currentVelocity * 0.5f);

            //Process general hits
            for (int i = 0; i < m_currentCollisionCount; i++)
            {
                //Only check the collider if its not us
                if (!m_myColliders.ContainsKey(m_currentCollisionHitArray[i].collider.GetInstanceID()))
                {
                    Vector3 checkVector = m_currentCollisionHitArray[i].transform.position - position;
                    if (Physics.Raycast(position, checkVector, out hit, m_collisionRange, m_objectCollisionLayer))
                    {
                        desiredVelocity = position - hit.point;
                        distanceSqr = desiredVelocity.sqrMagnitude;
                        repulsion = Mathf.Abs(1f - (distanceSqr / sqrCollisionRange));
                        if (repulsion > 0f)
                        {
                            scale = repulsion * (sqrCollisionRange / distanceSqr);
                            desiredVelocity *= scale;
                            //Debug.DrawRay(position, desiredVelocity, Color.cyan);
                            targetOffset += desiredVelocity;
                        }
                    }
                }
            }

            //Process tree hits
            for (int i = 0; i < m_currentTreeCollisionCount; i++)
            {
                desiredVelocity = position - m_currentTreeCollisionHitArray[i].position;
                distanceSqr = desiredVelocity.sqrMagnitude - 0.0625f; //Allow for trunk width
                repulsion = Mathf.Abs(1f - (distanceSqr / sqrCollisionRange));
                if (repulsion > 0f)
                {
                    scale = repulsion * (sqrCollisionRange / distanceSqr); 
                    desiredVelocity *= scale;
                    //Debug.DrawRay(position, desiredVelocity, Color.cyan);
                    targetOffset += desiredVelocity;
                }
            }

            //Make it an average of overall hits
            //targetOffset /= (float)(m_currentCollisionCount + m_currentTreeCollisionCount);
            //Debug.DrawRay(position, targetOffset, Color.magenta);

            return targetPosition + targetOffset;
        }

        /// <summary>
        /// Get the terrain that matches this location, otherwise return null
        /// </summary>
        /// <param name="locationWU">Location to check in world units</param>
        /// <returns>Terrain here or null</returns>
        private Terrain GetTerrain(Vector3 locationWU)
        {
            Terrain terrain;
            Vector3 terrainMin = new Vector3();
            Vector3 terrainMax = new Vector3();

            //First check active terrain - most likely already selected
            terrain = Terrain.activeTerrain;
            if (terrain != null)
            {
                terrainMin = terrain.GetPosition();
                terrainMax = terrainMin + terrain.terrainData.size;
                if (locationWU.x >= terrainMin.x && locationWU.x <= terrainMax.x)
                {
                    if (locationWU.z >= terrainMin.z && locationWU.z <= terrainMax.z)
                    {
                        return terrain;
                    }
                }
            }

            //Then check rest of terrains
            for (int idx = 0; idx < Terrain.activeTerrains.Length; idx++)
            {
                terrain = Terrain.activeTerrains[idx];
                terrainMin = terrain.GetPosition();
                terrainMax = terrainMin + terrain.terrainData.size;
                if (locationWU.x >= terrainMin.x && locationWU.x <= terrainMax.x)
                {
                    if (locationWU.z >= terrainMin.z && locationWU.z <= terrainMax.z)
                    {
                        return terrain;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Draw gizmos when not selected
        /// </summary>
        private void OnDrawGizmos()
        {
            DrawGizmos(false);
        }

        /// <summary>
        /// Draw gizmos when selected
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            DrawGizmos(true);
        }

        /// <summary>
        /// Draw the gizmos
        /// </summary>
        /// <param name="isSelected"></param>
        private void DrawGizmos(bool isSelected)
        {
            //Determine whether to drop out
            if (!isSelected && !m_alwaysShowGizmos)
            {
                return;
            }

            Color gCol = Gizmos.color;

            //Draw what we are following
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, m_targetPosition);

            //Draw collisions
            if (m_avoidObjectCollisions || m_avoidTreeCollisions)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, m_collisionRange);

                Gizmos.color = Color.red;
                for (int i = 0; i < m_currentCollisionCount; i++)
                {
                    Gizmos.DrawLine(transform.position, m_currentCollisionHitArray[i].transform.position);
                }
                for (int i = 0; i < m_currentTreeCollisionCount; i++)
                {
                    Gizmos.DrawLine(transform.position, m_currentTreeCollisionHitArray[i].position);
                }
            }
            Gizmos.color = gCol;
        }
    }
}

