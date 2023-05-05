
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class OcclusionToTransparent : MonoBehaviour
{ 
    // 所有障碍物的Renderer数组  
    private List<Renderer> obstacleList = new List<Renderer>();
    private HashSet<Renderer> obstacleMap = new HashSet<Renderer>();
    // 人物主角（之后通过名字识别？还是tag？目前手动拖过来）  
    private Transform transformOfTarget;
    private Transform transformOfCamera;

    // 临时接收，用于存储  
    public Renderer[] tempRenderers;
    //private Vector3 offset;
    private float sqrDistance = 1;
    private CustomCameraController cameraControlA;
    void Start()
    {
        cameraControlA = this.GetComponent<CustomCameraController>();
//        Debug.LogError("cameraControlA.m_Distance:"+ cameraControlA.m_Distance);

    }

    /*
    public void SetTarget(Transform targetTransform, Transform cameraTransform, float distance)
    {
        transformOfTarget = targetTransform;
        transformOfCamera = cameraTransform;
        sqrDistance = Mathf.Pow(distance, 2);
        //offset = transformOfCamera.position - transformOfTarget.position;
    }
    */
    
    public void Update()
    {
        if (cameraControlA == null || !CameraManager.IsAccessable) return;
        transformOfTarget = CameraManager.Instance.GetTarget();
        if (transformOfTarget == null) return;
        transformOfCamera = this.transform;
        sqrDistance = Mathf.Pow(cameraControlA.m_Distance, 2);
        // 调试使用：红色射线，仅Scene场景可见     
#if UNITY_EDITOR
        Debug.DrawLine(transformOfTarget.transform.position, (transformOfCamera.position - transformOfTarget.position).normalized * 20, Color.red);
#endif
        RaycastHit[] hits = Physics.RaycastAll(transformOfTarget.transform.position, transformOfCamera.position - transformOfTarget.transform.position, 10f, LayerMask.GetMask("CameraOcc"));
        //RaycastHit[] hit = Physics.RaycastAll(new Ray(_target.transform.position, (transform.position - _target.transform.position).normalized));
        //RaycastHit[] hit = Physics.RaycastAll(_target.transform.position, transform.position,100);  
        //Debug.LogError("hit.Length:" + hits.Length);
        //  如果碰撞信息数量大于0条  
        if (hits.Length > 0)
        {
            // 设置障碍物透明度为0.5  
            for (int i = 0; i < hits.Length; i++)
            {

                tempRenderers = hits[i].collider.gameObject.GetComponentsInChildren<Renderer>();
                if (tempRenderers.Length > 0)
                {
                    float distance = Vector3.SqrMagnitude(transformOfCamera.position - hits[i].collider.gameObject.transform.position);
                    float ratio = Mathf.Lerp(0, 0.5f, distance / sqrDistance);//Vector3.SqrMagnitude(transformOfCamera.position - hits[i].collider.gameObject.transform.position) / sqrDistance;
                    //float alpha = Mathf.Pow(ratio, 8) * 0.02f;
                    float alpha = Mathf.Pow(ratio, 8) * 0.02f;
                    for (int j = 0; j < tempRenderers.Length; j++)
                    {
                        SetMaterialAlpha(tempRenderers[j], alpha);
                        obstacleMap.Add(tempRenderers[j]);
                    }
                }
              

              
            }
        }

        if(obstacleList.Count > 0)
        {
            for (int i = obstacleList.Count - 1; i >= 0; i--)
            {
                if (!obstacleMap.Contains(obstacleList[i]))
                {
                    SetMaterialAlpha(obstacleList[i], 1f);
                    //obstacleList.RemoveAt(i);
                }
            }
            obstacleList.Clear();
        }

        if (obstacleMap.Count > 0)
        {
            foreach (Renderer tempRenderer in obstacleMap)
            {
                obstacleList.Add(tempRenderer);
            }
            obstacleMap.Clear();
        }

        
    }

    void OnDisable()
    {
        if (obstacleList.Count > 0)
        {
            //obstacleMap
            for (int i = obstacleList.Count - 1; i >= 0; i--)
            {
                SetMaterialAlpha(obstacleList[i], 1f);
            }
            obstacleList.Clear();
        }
    }
    /*
    private float GetMaterialAlpha(Renderer _renderer)
    {
        // 一个游戏物体的某个部分都可以有多个材质球  
        int materialsCount = _renderer.materials.Length;
        if (_renderer.materials.Length > 0)
        {
            return _renderer.materials[0].color.a;
        }
        return 1;
    }
    */
    // 修改障碍物的透明度  
    private void SetMaterialAlpha(Renderer _renderer, float alpha)
    {
        // 一个游戏物体的某个部分都可以有多个材质球  
        int materialsCount = _renderer.materials.Length;
        //Debug.LogError("materialsCount:" + materialsCount);
        for (int i = 0; i < materialsCount; i++)
        {

            // 获取当前材质球颜色  
            Color color = _renderer.materials[i].color;

            // 设置透明度（0--1）  
            color.a = alpha;
            _renderer.materials[i].EnableKeyword("_USE_DITHER");
            // 设置当前材质球颜色（游戏物体上右键SelectShader可以看见属性名字为_Color）  
            _renderer.materials[i].SetColor("_BaseColor", color);
        }

    }
}