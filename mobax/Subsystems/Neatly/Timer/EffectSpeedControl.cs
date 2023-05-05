using UnityEngine;

namespace CodeHero {
    public class EffectSpeedControl : MonoBehaviour {
        public float time = 1;//销毁时间
        float mSpeed = 1f;//播放速度

        public void ChangeVelocitySpeed(Transform tr) {
            if (tr.GetComponent<Animation>() != null) {
                foreach (AnimationState an in tr.GetComponent<Animation>()) {
                    an.speed = mSpeed;
                }
            }
            Animator ani = tr.GetComponent<Animator>();
            if (ani != null) {
                ani.speed = mSpeed;
            }
            ParticleSystem ps = tr.GetComponent<ParticleSystem>();
            if (ps != null) {
                ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
                int count = ps.GetParticles(particles);
                for (int i = 0; i < count; i++) {
                    particles[i].velocity = new Vector3(0, mSpeed, 0);
                }
                ps.SetParticles(particles, count);
            }
            for (int i = 0; i < tr.childCount; i++) {
                ChangeVelocitySpeed(tr.GetChild(i));
            }
        }

        public float Speed {
            get {
                return mSpeed;
            }
            set {
                mSpeed = value;
                ChangeVelocitySpeed(transform);
            }
        }
        void Update() {
            time -= Time.deltaTime * mSpeed;
            if (time < 0) {
                Destroy(this.gameObject);
            }
        }
    }
    

}