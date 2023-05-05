#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif

using UnityEngine;
using System.Collections.Generic;
using Spine.Unity;


#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
	[RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
	public class RoleOutline : MonoBehaviour
	{
		public MeshRenderer referenceRenderer;

		bool updateViaSkeletonCallback = false;
		MeshFilter referenceMeshFilter;
		MeshRenderer ownRenderer;
		MeshFilter ownMeshFilter;

		public Material[] sharedMaterials = new Material[0];

		void Awake()
		{
			Init();
		}

		public void Init()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			transform.localScale = Vector3.one;
			transform.localPosition = new Vector3(0, 0, 0.3f);
			if (referenceRenderer == null)
			{
				referenceRenderer = this.transform.parent.GetComponentInParent<MeshRenderer>();
			}

			// subscribe to OnMeshAndMaterialsUpdated
			var skeletonRenderer = referenceRenderer.GetComponent<SkeletonAnimation>();
			if (skeletonRenderer)
			{
				skeletonRenderer.OnMeshAndMaterialsUpdated -= UpdateOnCallback;
				skeletonRenderer.OnMeshAndMaterialsUpdated += UpdateOnCallback;
				updateViaSkeletonCallback = true;
			}
			referenceMeshFilter = referenceRenderer.GetComponent<MeshFilter>();
			ownRenderer = this.GetComponent<MeshRenderer>();
			ownMeshFilter = this.GetComponent<MeshFilter>();
		}




	public void setColor(Color _c)
    {
		if(ownRenderer == null)
        {
			Init();
        }
		MaterialPropertyBlock _mpb = new MaterialPropertyBlock();
		_mpb.SetColor("_OutlineColor", _c);
		ownRenderer.SetPropertyBlock(_mpb);
	}


    void LateUpdate()
		{

			if (updateViaSkeletonCallback)
				return;
			UpdateMaterials();
		}

		void UpdateOnCallback(SkeletonRenderer r)
		{
			UpdateMaterials();
		}

		void UpdateMaterials()
		{
        if (!referenceMeshFilter)
        {
			return;
        }
			ownMeshFilter.sharedMesh = referenceMeshFilter.sharedMesh;
			ownRenderer.sharedMaterials = sharedMaterials;
		}


	}



