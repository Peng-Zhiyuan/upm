
using UnityEngine;  
using System.Collections;  
using System.Collections.Generic;  
public class Obstacle2Transparente : MonoBehaviour {  
  
    // 所有障碍物的Renderer数组  
    private List<Renderer> _ObstacleCollider;  
  
    // 人物主角（之后通过名字识别？还是tag？目前手动拖过来）  
    public GameObject _target;  
  
    // 临时接收，用于存储  
    private Renderer[] _tempRenderer;
    private Vector3 cacheOffset;
    void Start()  
    {  
        _ObstacleCollider = new List<Renderer>();
        cacheOffset = transform.position - _target.transform.position;
    }  
    void LateUpdate()  
    {  
        // 调试使用：红色射线，仅Scene场景可见     
        #if UNITY_EDITOR  
        Debug.DrawLine(_target.transform.position, transform.position, Color.red);
#endif
        RaycastHit[] hits = Physics.RaycastAll(_target.transform.position, transform.position - _target.transform.position, 100f, LayerMask.GetMask("Default"));
        //RaycastHit[] hit = Physics.RaycastAll(new Ray(_target.transform.position, (transform.position - _target.transform.position).normalized));
        //RaycastHit[] hit = Physics.RaycastAll(_target.transform.position, transform.position,100);  
        Debug.LogError("hit.Length:"+hits.Length);
        //  如果碰撞信息数量大于0条  
        if (hits.Length > 0)  
        {   // 设置障碍物透明度为0.5  
            for (int i = 0; i < hits.Length; i++)  
            {  
                _tempRenderer = hits[i].collider.gameObject.GetComponentsInChildren<Renderer>();
                if (_tempRenderer.Length > 0)
                {
                    for (int j = 0; j < _tempRenderer.Length; j++)
                    {
                        _ObstacleCollider.Add(_tempRenderer[j]);
                        float ratio = Vector3.SqrMagnitude(transform.position - _target.transform.position) / Vector3.SqrMagnitude(cacheOffset);
                        float alpha = Mathf.Pow(ratio, 8) * 0.02f;
                        SetMaterialsAlpha(_tempRenderer[j], alpha);
                        Debug.Log(hits[i].collider.name);
                    }
                }
               
            }  
        }// 恢复障碍物透明度为1  
        else  
        {  
            for (int i = 0; i < _ObstacleCollider.Count; i++)  
            {  
                SetMaterialsAlpha(_ObstacleCollider[i], 1f);  
            }  
        }  
  
    }  
     
  
    // 修改障碍物的透明度  
    private void SetMaterialsAlpha(Renderer _renderer, float Transpa)  
    {  
        // 一个游戏物体的某个部分都可以有多个材质球  
        int materialsCount = _renderer.materials.Length;  
              Debug.LogError("materialsCount:"+materialsCount);
        for (int i = 0; i < materialsCount; i++)  
        {  
  
            // 获取当前材质球颜色  
            Color color = _renderer.materials[i].color;  
  
            // 设置透明度（0--1）  
            color.a = Transpa;  
      
            // 设置当前材质球颜色（游戏物体上右键SelectShader可以看见属性名字为_Color）  
            _renderer.materials[i].SetColor("_BaseColor", color);  
        }  
  
    }  
} 