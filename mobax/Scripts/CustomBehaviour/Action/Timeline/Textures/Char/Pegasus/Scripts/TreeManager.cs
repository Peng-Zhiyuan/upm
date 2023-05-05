using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Pegasus
{
    /// <summary>
    /// A class to manage tree instances on unity terrain
    /// </summary>
    public class TreeManager
    {
        //What we will store about each tree
        public struct TreeStruct
        {
            public Vector3 position;
            public int prototypeID;
        }

        //Tree storage
        #pragma warning disable 414
        private List<TreePrototype> m_terrainTrees = new List<TreePrototype>();
        private Quadtree<TreeStruct> m_terrainTreeLocations = new Quadtree<TreeStruct>(new Rect(0,0, 10f, 10f));
        #pragma warning restore 414

        /// <summary>
        /// Load the trees in from the terrain
        /// </summary>
        public void LoadTreesFromTerrain()
        {
            //Destroy previous contents
            m_terrainTrees = null;
            m_terrainTreeLocations = null;

            //Work out the bounds of the environment
            float minY = float.NaN;
            float minX = float.NaN;
            float maxX = float.NaN;
            float minZ = float.NaN;
            float maxZ = float.NaN;
            Terrain sampleTerrain = null;
            foreach (Terrain terrain in Terrain.activeTerrains)
            {
                if (float.IsNaN(minY))
                {
                    sampleTerrain = terrain;
                    minY = terrain.transform.position.y;
                    minX = terrain.transform.position.x;
                    minZ = terrain.transform.position.z;
                    maxX = minX + terrain.terrainData.size.x;
                    maxZ = minZ + terrain.terrainData.size.z;
                }
                else
                {
                    if (terrain.transform.position.x < minX)
                    {
                        minX = terrain.transform.position.x;
                    }
                    if (terrain.transform.position.z < minZ)
                    {
                        minZ = terrain.transform.position.z;
                    }
                    if ((terrain.transform.position.x + terrain.terrainData.size.x) > maxX)
                    {
                        maxX = terrain.transform.position.x + terrain.terrainData.size.x;
                    }
                    if ((terrain.transform.position.z + terrain.terrainData.size.z) > maxZ)
                    {
                        maxZ = terrain.transform.position.z + terrain.terrainData.size.z;
                    }
                }
            }

            if (sampleTerrain != null)
            {
                Rect terrainBounds = new Rect(minX, minZ, maxX - minX, maxZ - minZ);

                m_terrainTreeLocations = new Quadtree<TreeStruct>(terrainBounds, 32);
                m_terrainTrees = new List<TreePrototype>(sampleTerrain.terrainData.treePrototypes);

                foreach (Terrain terrain in Terrain.activeTerrains)
                {
                    float terrainOffsetX = terrain.transform.position.x;
                    float terrainOffsetY = terrain.transform.position.y;
                    float terrainOffsetZ = terrain.transform.position.z;
                    float terrainWidth = terrain.terrainData.size.x;
                    float terrainHeight = terrain.terrainData.size.y;
                    float terrainDepth = terrain.terrainData.size.z;
                    TreeInstance[] terrainTreeInstances = terrain.terrainData.treeInstances;
                    for (int treeIdx = 0; treeIdx < terrainTreeInstances.Length; treeIdx++)
                    {
                        TreeInstance treeInstance = terrainTreeInstances[treeIdx];
                        TreeStruct newTree = new TreeStruct();
                        newTree.position = new Vector3(terrainOffsetX + (treeInstance.position.x * terrainWidth), terrainOffsetY + (treeInstance.position.y * terrainHeight), terrainOffsetZ + (treeInstance.position.z * terrainDepth));
                        newTree.prototypeID = terrainTreeInstances[treeIdx].prototypeIndex;
                        m_terrainTreeLocations.Insert(newTree.position.x, newTree.position.z, newTree);
                    }
                }
            }
        }

        /// <summary>
        /// Add a tree instance into storage - must be called after the initial load call
        /// </summary>
        /// <param name="position"></param>
        /// <param name="prototypeIdx"></param>
        public void AddTree(TreeStruct tree)
        {
            if (m_terrainTreeLocations == null)
            {
                return;
            }
            m_terrainTreeLocations.Insert(tree.position.x, tree.position.z, tree);
        }

        /// <summary>
        /// Return the number of trees within range of the location provided
        /// </summary>
        /// <param name="position">Location to check</param>
        /// <param name="range">Range around location to check</param>
        /// <returns>Number of trees within range</returns>
        public int Count(Vector3 position, float range)
        {
            if (m_terrainTreeLocations == null)
            {
                return 0;
            }
            Rect query = new Rect(position.x - range, position.z - range, range * 2f, range * 2f);
            return m_terrainTreeLocations.Find(query).Count();
        }

        /// <summary>
        /// Return the number of trees being managed
        /// </summary>
        /// <returns>Number of trees being managed</returns>
        public int Count()
        {
            if (m_terrainTreeLocations == null)
            {
                return 0;
            }
            return m_terrainTreeLocations.Count;
        }

        /// <summary>
        /// Grab all trees within this range
        /// </summary>
        /// <param name="position">Position to search from</param>
        /// <param name="range">Range to search</param>
        /// <param name="treeList">List of trees found</param>
        /// <returns>Count of trees found</returns>
        public int GetTrees(Vector3 position, float range, ref List<TreeStruct> treeList)
        {
            treeList.Clear();
            if (m_terrainTreeLocations == null)
            {
                return 0;
            }
            Rect query = new Rect(position.x - range, position.z - range, range * 2f, range * 2f);
            treeList.AddRange(m_terrainTreeLocations.Find(query));
            return treeList.Count;
        }
    }
}

