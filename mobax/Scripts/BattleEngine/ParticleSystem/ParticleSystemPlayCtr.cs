namespace BattleEngine.View
{
    using UnityEngine;
    using System.Threading.Tasks;

    public class ParticleSystemPlayCtr : MonoBehaviour, IUpdatable
    {
        public float duration = 5.0f;
        ParticleSystem particle;
        ParticleSystem[] particles;
        Animator[] animator;

        public string effectPrefabName { get; set; }
        private float currentTime = 0.0f;
        public bool isPlaying = false;
        private float speed = 1.0f;

        private void Awake()
        {
            particle = GetComponent<ParticleSystem>();
            particles = GetComponentsInChildren<ParticleSystem>(true);
            animator = GetComponentsInChildren<Animator>(true);
        }

        public void OnUpdate()
        {
            if (duration <= 0
                || !isPlaying)
            {
                return;
            }
            currentTime += Time.deltaTime * speed;
            if (currentTime >= duration)
            {
                ToRecycle();
                isPlaying = false;
            }
        }

        public bool Playing()
        {
            return isPlaying;
        }

        public void Play()
        {
            Resume();
            if (particle != null)
            {
                particle.Play();
            }
            if (particles != null)
            {
                for (int i = 0; i < particles.Length; i++)
                {
                    particles[i].Play();
                }
            }
            if (animator != null)
            {
                for (int i = 0; i < animator.Length; i++)
                {
                    AnimatorClipInfo[] clips = animator[i].GetCurrentAnimatorClipInfo(0);
                    animator[i].gameObject.SetActive(true);
                    animator[i].speed = 1.0f;
                    if (clips != null
                        && clips.Length > 0)
                    {
                        animator[i].Play(clips[0].clip.name, 0, 0);
                    }
                }
            }
            isPlaying = true;
            currentTime = 0;
            UpdateManager.Instance.Add(this);
        }

        public void SetSpeed(float _speed)
        {
            speed = _speed >= 0 ? _speed : 0;
            if (particle != null)
            {
                var main = particle.main;
                main.simulationSpeed = speed;
            }
            if (particles != null)
            {
                for (int i = 0; i < particles.Length; i++)
                {
                    var main = particles[i].main;
                    main.simulationSpeed = speed;
                }
            }
            if (animator != null)
            {
                for (int i = 0; i < animator.Length; i++)
                {
                    animator[i].speed = _speed;
                }
            }
        }

        public void Pause()
        {
            if (particle != null)
            {
                var main = particle.main;
                main.simulationSpeed = 0.0f;
            }
            else
            {
                return;
            }
            for (int i = 0; i < particles.Length; i++)
            {
                var main = particles[i].main;
                main.simulationSpeed = 0.0f;
            }
            if (animator != null)
            {
                for (int i = 0; i < animator.Length; i++)
                {
                    animator[i].speed = 0.0f;
                }
            }
            isPlaying = false;
        }

        public void Resume()
        {
            if (particle != null)
            {
                var main = particle.main;
                main.simulationSpeed = speed;
            }
            if (particles != null)
            {
                for (int i = 0; i < particles.Length; i++)
                {
                    var main = particles[i].main;
                    main.simulationSpeed = speed;
                }
            }
            if (animator != null)
            {
                for (int i = 0; i < animator.Length; i++)
                {
                    animator[i].speed = speed;
                }
            }
            isPlaying = true;
        }

        public void Stop()
        {
            if (particle != null)
            {
                particle.Stop();
            }
            if (particles != null)
            {
                for (int i = 0; i < particles.Length; i++)
                {
                    particles[i].Stop();
                }
            }
            if (animator != null)
            {
                for (int i = 0; i < animator.Length; i++)
                {
                    animator[i].speed = 0;
                }
            }
            isPlaying = false;
            UpdateManager.Instance.Remove(this);
        }

        public void ToRecycle()
        {
            UpdateManager.Instance.Remove(this);
            isPlaying = false;
            if (!string.IsNullOrEmpty(effectPrefabName)
                && this != null)
            {
                BattleResManager.Instance.RecycleEffect(effectPrefabName, this.gameObject);
            }
        }

        public async void ToRecycle(string effectName, GameObject effectGo, float time)
        {
            if (!string.IsNullOrEmpty(effectName))
            {
                float duration = ParticleSystemLength(effectGo.transform);
                duration = Mathf.Max(duration, time);
                await Task.Delay((int)(duration * 1000));
                BattleResManager.Instance.RecycleEffect(effectName, effectGo);
            }
            isPlaying = false;
        }

        public float ParticleSystemLength(Transform transform)
        {
            ParticleSystem[] particleSystems = transform.GetComponentsInChildren<ParticleSystem>();
            float maxDuration = 0;
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem.MainModule main = particleSystems[i].main;
                ParticleSystem.EmissionModule emission = particleSystems[i].emission;
                if (emission.enabled)
                {
                    if (main.loop)
                    {
                        return -1f;
                    }
                    float dunration = 0f;
                    if (emission.rateOverTimeMultiplier <= 0)
                    {
                        dunration = main.startDelayMultiplier + main.startLifetimeMultiplier;
                    }
                    else
                    {
                        dunration = main.startDelayMultiplier + Mathf.Max(main.duration, main.startLifetimeMultiplier);
                    }
                    if (dunration > maxDuration)
                    {
                        maxDuration = dunration;
                    }
                }
            }
            return maxDuration;
        }
    }
}