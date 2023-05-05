using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundLine : MonoBehaviour
{
    private void Update()
    {

        if (pointBornTick > 0.0f)
        {
            pointBornTick -= Time.deltaTime;
            if (pointBornTick <= 0.0f)
            {
                pointBornTick = pointBornTime;
                StartCoroutine(MoveFxPoint());
            }
        }

        if (aniTick>=0.0f)
        {
            aniTick += Time.deltaTime * speed;
            int _cnum = Mathf.CeilToInt((aniTick / aniTime) * num);
            if (_cnum != index)
            {
                index = _cnum;
                UpdateNum(index);
                if (index >= num-2)
                {
                    aniTick = -1.0f;
                    pointPref.transform.position = line.GetPosition(index - 1);
                    pointPref.SetActive(true);
                }
            }
           
        }
        if (showTick > 0.0f)
        {
            showTick -= Time.deltaTime;
            if (showTick <= 0.0f)
            {
                showTick = -1.0f;
               // point.SetActive(false);
               // line.gameObject.SetActive(false);
            }
        }
    }

    public void BeatShow(Vector3 _bpos,Vector3 _epos)
    {
        DrawLine(_bpos,_epos);
        if (!switchLine)
        {
            SwitchDraw(true);
            pointBornTick = pointBornTime;
            StartCoroutine(MoveFxPoint());
        }
    }
    public void SwitchDraw(bool _s)
    {
        switchLine = _s;
        if (switchLine)
        {
            root.SetActive(true);
            pointPref.SetActive(false);
            line.gameObject.SetActive(true);
        }
        else
        {
            root.SetActive(false);
            line.gameObject.SetActive(false);
            StopCoroutine("MoveFxPoint");
            ReleaseAllFxPoint();
        }
    }

    void DrawLine(Vector3 _bpos,Vector3 _epos)
    {


        dir = _bpos-_epos;
        dir.y = 0;
        dir.Normalize();

        dis = Vector3.Distance(_epos, _bpos);

        //
        r = dis / (2.0f * Mathf.Cos(radian));
        y = dis / (2.0f * Mathf.Sin(radian));

        p = _epos + dir * dis * 0.5f;
        p.y -= y;



        bRad = radian;
        float eRad = Mathf.PI - bRad;

        num = Mathf.CeilToInt((eRad - bRad) / radDev);

        index = 0;

        speed = num / aniTime;


        var rnum=UpdateNum(num);


        tailFx.transform.position = line.GetPosition(rnum - 1);
        tailFx.SetActive(true);

        
        return;

        aniTick = 0.0f;
        pointPref.SetActive(false);


        showTick = showTime;
    }

    int UpdateNum(int _num)
    {
        posList.Clear();
        var _half = Mathf.CeilToInt(_num * 0.5f);
        curBRad = 100;
        curERad = 0;
        for (var i = 0; i< _num;++i)
        {
            float cR = bRad + i * radDev;
            Vector3 _cpos = p;
            _cpos.y += Mathf.Sin(cR) * r + y+adjustY;
            _cpos += dir * Mathf.Cos(cR) * r;

            if (i < _half)
            {
                if (_cpos.y < beginYFilter)
                {
                    continue;
                }
            }
            else
            {
                if (_cpos.y < endYFilter)
                {
                    continue;
                }
            }
            curBRad = Mathf.Min(curBRad, cR);
            curERad = Mathf.Max(curERad, cR);

            posList.Add(_cpos);
        }
        line.positionCount = posList.Count;
        for(var i = 0; i < posList.Count; ++i)
        {
            line.SetPosition(i, posList[i]);
        }
        return posList.Count;
    }


    Vector3 GetPos(float _rad)
    {
        Vector3 _cpos = p;
        _cpos.y += Mathf.Sin(_rad) * r + y+adjustY;
        _cpos += dir * Mathf.Cos(_rad) * r;
        return _cpos;

    } 



    GameObject GetFxPoint()
    {
        for(var i = 0; i < fxPointList.Count; ++i)
        {
            if (!fxPointList[i].activeSelf)
            {
                fxPointList[i].SetActive(true);
                return fxPointList[i];
            }
        }

        GameObject _p = GameObject.Instantiate(pointPref,root.transform) as GameObject;
        _p.SetActive(true);
        fxPointList.Add(_p);
        return _p;
    }

    void ReleaseFxPoint(GameObject _fx)
    {
        for (var i = 0; i < fxPointList.Count; ++i)
        {
            if (fxPointList[i] == _fx)
            {
                fxPointList[i].SetActive(false);
                return;
            }
        }
    }

    void ReleaseAllFxPoint()
    {
        for (var i = 0; i < fxPointList.Count; ++i)
        {
            fxPointList[i].SetActive(false);
        }
    }

    IEnumerator MoveFxPoint()
    {
        float moveTick = 0;
        var point = GetFxPoint();
        while (moveTick < pointMoveTime)
        {
            moveTick += Time.deltaTime;
            var crad = Mathf.Lerp(curBRad, curERad, smoothStep(moveTick /pointMoveTime));
            point.transform.position = GetPos(crad);
            yield return null;
        }
        ReleaseFxPoint(point);
    }

    float smoothStep(float x)
    {
        return x * x * (3.0f - 2.0f * x);
    }

    public GameObject root;


    public LineRenderer line;
    public GameObject pointPref;
    float dis = 3;
    public float radian = Mathf.PI / 6;
    public float radDev = Mathf.PI / 10;
    public float aniTime = 1.0f;
    float aniTick = -1.0f;
    float speed = 0.0f;
    

    int num;
    int index = 0;
    float bRad;
    float r;
    Vector3 dir;
    Vector3 p;
    float y;

    public float showTime = 1.0f;
    float showTick = -1.0f;

    public float beginYFilter = 1.0f;
    public float endYFilter = 1.0f;

    List<Vector3> posList = new List<Vector3>();



    public GameObject tailFx;
    List<GameObject> fxPointList = new List<GameObject>();
    public float pointMoveTime = 1.0f;
    float curBRad;
    float curERad;

    public float pointBornTime = 1.0f;
    float pointBornTick = -1.0f;

    private bool switchLine;

    public float adjustY = 1.0f;

}
