namespace BattleEngine.Logic
{
    using UnityEngine;

    public abstract class CombatUnitEntity : Entity
    {
#region Var
        private ACTOR_LIFE_STATE _lifeState = ACTOR_LIFE_STATE.None;

        public void SetLifeState(ACTOR_LIFE_STATE state)
        {
            _lifeState = state;
        }

        public ACTOR_LIFE_STATE CurrentLifeState
        {
            get { return _lifeState; }
        }

        private Vector3 _transPostion = Vector3.zero;
        private Vector3 _transPostionXY = Vector3.zero;

        public Vector3 GetPositionXZ()
        {
            _transPostionXY.x = _transPostion.x;
            _transPostionXY.z = _transPostion.z;
            return _transPostionXY;
        }

        public Vector3 GetPosition()
        {
            return _transPostion;
        }

        public void SetPosition(Vector3 position)
        {
            _transPostion.x = position.x;
            _transPostion.y = position.y;
            _transPostion.z = position.z;
        }

        private Vector3 _transRotation = Vector3.zero;

        public Vector3 GetEulerAngles()
        {
            return new Vector3(0, _transRotation.y, 0);
        }

        public void SetEulerAngles(Vector3 rot)
        {
            SetEulerAnglesY(rot.y);
        }

        public void SetEulerAnglesY(float OffsetY)
        {
            _transRotation.y = OffsetY;
        }

        private float scale = 1.0f;

        public float GetSize()
        {
            return scale;
        }

        public void SetSize(float size)
        {
            scale = size;
        }

        public Vector3 GetForward()
        {
            Quaternion rotaion = Quaternion.Euler(_transRotation);
            return rotaion * Vector3.forward;
        }

        public Vector3 GetRight()
        {
            Quaternion rotaion = Quaternion.Euler(_transRotation);
            return rotaion * Vector3.right;
        }

        public Vector3 GetUp()
        {
            Quaternion rotaion = Quaternion.Euler(_transRotation);
            return rotaion * Vector3.up;
        }

        private Vector3 _centers = new Vector3(0, 2, 0);

        public Vector3 GetCenter()
        {
            return _centers;
        }

        private float _touchHeight = 2.0f;

        public float GetTouchHight()
        {
            return _touchHeight;
        }

        private float _touchRadius = 2.0f;

        public float GetTouchRadiu()
        {
            return _touchRadius * scale;
        }

        private float _hitRadius = 2.0f;

        public float GetHitRadiu()
        {
            return _hitRadius * scale;
        }

        private float _aiLocRadius = 2.0f;

        public float GetAiLocRadiu()
        {
            return _aiLocRadius * scale;
        }
#endregion

#region Method
        public virtual void Born(Vector3 pos, Vector3 rot, float _size = 1.0f)
        {
            SetPosition(pos);
            SetEulerAngles(rot);
            SetSize(_size);
        }

        public virtual void BornCharacters(Vector3 pos, Vector3 rot, float _size, HeroRow row)
        {
            SetPosition(pos);
            SetEulerAngles(rot);
            SetSize(_size);
            this._touchHeight = row.touchHeight;
            this._centers = new Vector3(0, row.touchHeight * 0.5f, 0);
            this._touchRadius = row.touchRadius;
            this._hitRadius = row.hitRadius;
            this._aiLocRadius = row.AILocRadius;
        }

        public void SetForward(Vector3 forward)
        {
            if (forward == Vector3.zero)
                return;
            Quaternion q = Quaternion.LookRotation(forward);
            _transRotation.y = q.eulerAngles.y;
        }

        /// <summary>
        /// 把位置转到本地坐标位置
        /// </summary>
        /// <param name="_worldPosition"></param>
        /// <returns></returns>
        public Vector3 World2Local(Vector3 _worldPosition)
        {
            Matrix4x4 trans = new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(-_transPostion.x, -_transPostion.y, -_transPostion.z, 1));
            Matrix4x4 rotZ = new Matrix4x4(new Vector4(Mathf.Cos(-_transRotation.z * Mathf.PI / 180), Mathf.Sin(-_transRotation.z * Mathf.PI / 180), 0, 0), new Vector4(-Mathf.Sin(-_transRotation.z * Mathf.PI / 180), Mathf.Cos(-_transRotation.z * Mathf.PI / 180), 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1));
            Matrix4x4 rotX = new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, Mathf.Cos(-_transRotation.x * Mathf.PI / 180), Mathf.Sin(-_transRotation.x * Mathf.PI / 180), 0), new Vector4(0, -Mathf.Sin(-_transRotation.x * Mathf.PI / 180), Mathf.Cos(-_transRotation.x * Mathf.PI / 180), 0), new Vector4(0, 0, 0, 1));
            Matrix4x4 rotY = new Matrix4x4(new Vector4(Mathf.Cos(-_transRotation.y * Mathf.PI / 180), 0, -Mathf.Sin(-_transRotation.y * Mathf.PI / 180), 0), new Vector4(0, 1, 0, 0), new Vector4(Mathf.Sin(-_transRotation.y * Mathf.PI / 180), 0, Mathf.Cos(-_transRotation.y * Mathf.PI / 180), 0), new Vector4(0, 0, 0, 1));
            Matrix4x4 Mview = (new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1))) * rotZ * rotX * rotY;
            Vector4 Pworld = new Vector4(_worldPosition.x, _worldPosition.y, _worldPosition.z, 1);
            Vector3 PLocal = Mview * Pworld;
            return PLocal;
        }

        public virtual void Die(CombatUnitEntity other)
        {
            _lifeState = ACTOR_LIFE_STATE.Dead;
        }

        public override void OnUpdate(int currentFrame) { }
#endregion

#region 生命周期
        public bool IsEntering()
        {
            return _lifeState == ACTOR_LIFE_STATE.Entering;
        }

        public bool IsSubstitut()
        {
            return _lifeState == ACTOR_LIFE_STATE.Substitut;
        }

        public bool IsFriendAssist()
        {
            return _lifeState == ACTOR_LIFE_STATE.Assist;
        }

        public bool Alive()
        {
            return _lifeState == ACTOR_LIFE_STATE.Alive || _lifeState == ACTOR_LIFE_STATE.God || _lifeState == ACTOR_LIFE_STATE.Guard || _lifeState == ACTOR_LIFE_STATE.StopLogic || _lifeState == ACTOR_LIFE_STATE.Born || _lifeState == ACTOR_LIFE_STATE.Substitut;
        }

        public bool IsDied()
        {
            return _lifeState == ACTOR_LIFE_STATE.Dead;
        }
#endregion
    }
}