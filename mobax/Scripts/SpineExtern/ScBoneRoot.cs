using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
public class ScBoneRoot : MonoBehaviour
{
    public SkeletonRenderer skeletonRenderer;
    public string[] boneNames;
	public bool followXYPosition = true;
	public bool followZPosition = true;
	public bool followBoneRotation = true;

	Dictionary<string, Spine.Bone> boneDic = new Dictionary<string, Spine.Bone>();
	Dictionary<string, Transform> tranDic = new Dictionary<string, Transform>();
	List<string> boneList = new List<string>();


	[Tooltip("Follows the skeleton's flip state by controlling this Transform's local scale.")]
	public bool followSkeletonFlip = true;

	[Tooltip("Follows the target bone's local scale. BoneFollower cannot inherit world/skewed scale because of UnityEngine.Transform property limitations.")]
	public bool followLocalScale = false;

    void Start()
    {
		Init();
    }



    public void Init()
	{
        if (skeletonRenderer == null)
        {
            if (transform.parent == null)
            {
				return;
            }
			var sr=transform.parent.GetComponent<SkeletonRenderer>();
            if (sr == null)
            {
				return;
            }
			skeletonRenderer = sr;

        }




		

		for (int i = 0; i < boneNames.Length; ++i)
        {
            if (string.IsNullOrEmpty(boneNames[i]))
            {
				continue;
            }
			var _name = boneNames[i];
			AddBone(_name);
			//for (int j = 1; j <= 5; ++j)
   //         {
			//	var _fname = _name + "00" + j;

			//}
			
		}
	}

	void AddBone(string _fname)
    {
		var _bone = skeletonRenderer.skeleton.FindBone(_fname);
		if (_bone == null)
		{
			return;
		}
		var _go = new GameObject(_fname);
		_go.transform.parent = this.transform;
		_go.transform.localPosition = Vector3.zero;
		_go.transform.localScale = Vector3.one;
		boneDic[_fname] = _bone;
		tranDic[_fname] = _go.transform;
		boneList.Add(_fname);
	}

    public void LateUpdate()
    {
		for(int i = 0; i < boneList.Count; ++i)
        {
			var _name = boneList[i];
			var bone = boneDic[_name];
			var thisTransform = tranDic[_name];
			// Recommended setup: Use local transform properties if Spine GameObject is the immediate parent
			thisTransform.localPosition = new Vector3(followXYPosition ? bone.worldX : thisTransform.localPosition.x,
													followXYPosition ? bone.worldY : thisTransform.localPosition.y,
													followZPosition ? 0f : thisTransform.localPosition.z);
			if (followBoneRotation)
			{
				float halfRotation = Mathf.Atan2(bone.c, bone.a) * 0.5f;
				if (followLocalScale && bone.scaleX < 0) // Negate rotation from negative scaleX. Don't use negative determinant. local scaleY doesn't factor into used rotation.
					halfRotation += Mathf.PI * 0.5f;

				var q = default(Quaternion);
				q.z = Mathf.Sin(halfRotation);
				q.w = Mathf.Cos(halfRotation);
				thisTransform.localRotation = q;
			}
		}

	}

	public Transform GetTrans(string _name)
    {
        if (!tranDic.ContainsKey(_name)){
			return transform;
        }
		return tranDic[_name];
	}
}
