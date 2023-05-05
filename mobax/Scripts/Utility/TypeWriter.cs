using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading;

public class TypeWriter : BaseMeshEffect
{
    [SerializeField] private int textIndex;

    private UIVertex vertex = new UIVertex();

    //记录刷新时间
    private float m_TimeCount = 0;

    private float m_charTime = 0;

    //是否开始显示打字
    private int state = 0;

    //每个字的出现间隔
    private float m_TimeSpaceScale = 0;
    private float m_TimeSpace = 0;

    //要显示的Text组件
    private Text m_Text;

    private int alphaIndex = 0;

    private float[] arrVertex;

    private int aniComplete = 0;
    private int m_charLen = 0;
    private int hasInit = 0;

    private Action _completedAction;
    private bool _isPlaying = false;

    public bool IsPlaying => this._isPlaying;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
        {
            return;
        }

        m_charLen = vh.currentVertCount / 4;
        int charLen = m_charLen;
        int vCount = 4;
        int vertexIndex = 0;

        if (state == 0)
        {
            return;
        }

        //文本长度
        if (state == 1)
        {
            for (var i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                Color c = vertex.color;
                c.a = 0;
                vertex.color = c;
                vh.SetUIVertex(vertex, i);
            }

            state = 2;
            return;
        }


        for (int i = 0; i < charLen; i++)
        {
            float alpha = 1;
            if (arrVertex != null && arrVertex.Length > 0 && i < arrVertex.Length)
            {
                alpha = arrVertex[i];
            }

            for (int j = 0; j < vCount; j++)
            {
                if (vertexIndex < vh.currentVertCount)
                {
                    vh.PopulateUIVertex(ref vertex, vertexIndex);
                    Color c = vertex.color;
                    c.a = alpha;
                    vertex.color = c;
                    vh.SetUIVertex(vertex, vertexIndex);
                }

                vertexIndex++;
            }
        }

        this._isPlaying = true;
        if (alphaIndex >= charLen && state > 0)
        {
            state = 0;
            aniComplete = 1;
            this._isPlaying = false;
        }
    }

    protected override void Awake()
    {
        m_Text = gameObject.GetComponent<Text>();
        textIndex = 0;
        m_Text.text = "";
    }

    protected override void Start()
    {
    }

    void Update()
    {
        if (state == 2)
        {
            if (hasInit == 0)
            {
                hasInit = 1;
                arrVertex = new float[m_charLen];
                for (int i = 0; i < m_charLen; i++)
                {
                    arrVertex[i] = 0;
                }
            }

            doText();
            doAlphaText();
        }

        if (state == 0)
        {
            arrVertex = new float[m_charLen];
            textIndex = m_charLen;
            this.doAlphaText();
        }

        if (aniComplete == 1)
        {
            aniComplete = 0;
            this._completedAction?.Invoke();
        }
    }

    private void doText()
    {
        int charLen = m_charLen;
        if (textIndex >= charLen)
        {
            return;
        }

        if (Time.time - m_charTime > m_TimeSpace / m_TimeSpaceScale)
        {
            m_charTime = Time.time;
            textIndex++;
        }
    }

    private void doAlphaText()
    {
        if (Time.time - m_TimeCount <= 0.02f)
        {
            return;
        }

        m_TimeCount = Time.time;
        int charLen = m_charLen;
        bool needModify = false;
        for (int i = alphaIndex; i < textIndex && i < charLen; i++)
        {
            float alpha = arrVertex[i];
            alpha += 0.1f;
            needModify = true;
            if (alpha >= 1)
            {
                alphaIndex++;
                alpha = 1;
            }

            arrVertex[i] = alpha;
        }

        if (needModify)
        {
            graphic.SetVerticesDirty();
        }
    }

    public void PlayAni(string contents, float timeSpace = 0.1f, int timeScale = 1)
    {
        if (state > 0)
        {
            return;
        }

        m_Text.text = contents;
        m_TimeSpaceScale = timeScale;
        m_TimeSpace = timeSpace;
        hasInit = 0;
        alphaIndex = 0;
        state = 1;
        textIndex = 0;
        graphic.SetVerticesDirty();
    }

    public void ClearText()
    {
        m_Text.text = "";
    }
    
    public void SetSpeed(int timeScale)
    {
        m_TimeSpaceScale = timeScale;
    }

    public void StopAni()
    {
        state = 0;
        this._completedAction = null;
        this._isPlaying = false;
    }

    public void CutAction()
    {
        this._completedAction = null;
    }

    public void showText()
    {
        if (hasInit == 0)
        {
            return;
        }

        alphaIndex = m_charLen;
        textIndex = m_charLen;
        for (int i = 0; i < m_charLen; i++)
        {
            arrVertex[i] = 1.0f;
        }

        graphic.SetVerticesDirty();
    }

    public void OnTextAniComplete(Action completedAction)
    {
        this._completedAction = completedAction;
    }
}