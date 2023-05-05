using System.Collections;
using UnityEngine;

public class UIFxSimple : MonoBehaviour
{
    public bool IsInitDisFx = true;

    private GameObject cachobj = null;
    public GameObject CachObj
    {
        get
        {
            if (cachobj == null)
                cachobj = this.gameObject;
            return cachobj;
        }
    }

    public System.Action delegatePlayFinish;
    public float finshTime;
    private ParticleSystem[] _componentParticles = null;
    private ParticleSystem[] componentParticles
    {
        get
        {
            if (_componentParticles == null)
            {
                if (this.gameObject != null)
                    _componentParticles = CachObj.GetComponentsInChildren<ParticleSystem>();
            }
            return _componentParticles;
        }
    }

    public void Play(bool play = true)
    {
        CachObj.SetActive(play);
        if (play)
        {
            Resume();
            for (int i = 0; i < componentParticles.Length; i++)
            {
                componentParticles[i].Play();
            }
            if (finshTime > 0)
                StartCoroutine(waitFinish());
        }
    }

    public bool isPlaying
    {
        get
        {
            if (!CachObj.activeSelf)
                return false;
            int length = componentParticles.Length;
            for (int i = 0; i < length; i++)
            {
                if (componentParticles[i].isPlaying)
                    return true;
            }
            return false;
        }
    }

    void Awake()
    {
        if (IsInitDisFx)
            CachObj.SetActive(false);
    }

    public IEnumerator waitFinish()
    {
        yield return new WaitForSeconds(finshTime);
        if (delegatePlayFinish != null)
        {
            delegatePlayFinish();
            delegatePlayFinish = null;
        }
    }

    public void Resume()
    {
        if (componentParticles != null)
        {
            for (int i = 0; i < componentParticles.Length; i++)
            {
                var main = componentParticles[i].main;
                componentParticles[i].Stop();
            }
        }
    }
}