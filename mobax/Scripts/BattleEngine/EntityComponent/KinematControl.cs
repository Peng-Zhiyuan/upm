namespace BattleEngine.Logic
{
    using UnityEngine;
    using System;

    public class KinematControl
    {
        protected CombatActorEntity actorEntity;
        Vector3 ForwardVelocity = Vector3.zero;
        //Vector3 Direction = Vector3.forward;
        float AngleSpeed = 0;
        private AnimationCurve CurrentHorizontalDampCurve;
        private AnimationCurve CurrentVerticalDampCurve;
        public AnimationCurve VerticalChargeDamp;
        public AnimationCurve HorizontalChargeDamp;
        private bool enable = true;

        public KinematControl(CombatActorEntity actor)
        {
            actorEntity = actor;
        }

        public virtual void Init()
        {
            enable = true;
            ForwardVelocity = Vector3.zero;
            //Direction = Vector3.zero;
        }

        public virtual void Move(Vector3 _Speed)
        {
            HorizontalChargeVelocity = Vector3.zero;
            ForwardVelocity = _Speed;
        }

        /// <summary>
        /// 人物转向-待废弃
        /// </summary>
        /// <param name="_Forward"></param>
        // public virtual void Turn(Vector3 _Forward, float _AngleSpeed)
        // {
        //     AngleSpeed = _AngleSpeed;
        //     if (_AngleSpeed != 0)
        //         Direction.Set(_Forward.x, 0, _Forward.z);
        // }
        /// <summary>
        /// 人物朝向-待废弃
        /// </summary>
        /// <param name="_Forward"></param>
        // public virtual void Look(Vector3 _Forward)
        // {
        //     if (!enable) return;
        //     if (_Forward != Vector3.zero)
        //     {
        //         LookAt(actorEntity.GetPositionXZ() + _Forward.normalized);
        //     }
        // }
        /// <summary>
        /// 人物朝向-待废弃
        /// </summary>
        // public virtual void Look(Quaternion _Rotation) {
        //     if (!enable) return;
        //     if (actorEntity != null && _Rotation.eulerAngles != Vector3.zero)
        //         actorEntity.SetEulerAngles(_Rotation.eulerAngles);
        // }
        /// <summary>
        /// 人物朝向-待废弃
        /// </summary>
        // public void LookAt(Vector3 _WorldPosition)
        // {
        //     if (!enable) return;
        //     if (actorEntity != null)
        //     {
        //         actorEntity.LookAt(_WorldPosition);
        //     }
        // }
        public float hSpeedValue = 0;
        Vector3 HorizontalChargeVelocity = Vector3.zero;

        public Vector3 mHorizontalChargeVelocity
        {
            get { return HorizontalChargeVelocity; }
        }

        float HorizontalDuration = 0;
        float HorizontalTimer = 0;

        /// <summary>
        /// 水平冲击
        /// </summary>
        public virtual void HorizontalCharge(float _Speed, AnimationCurve _DampCurve, float _Duration)
        {
            hSpeedValue = _Speed;
            Vector3 hSpeed = actorEntity.GetForward() * _Speed;
            Vector3 finalPos = actorEntity.GetPositionXZ() + hSpeed * _Duration;
            if (!BattleUtil.IsInMap(finalPos))
            {
                finalPos.y = BattleUtil.GetWalkablePos(finalPos).y;
            }
            Vector3 finalDistance = finalPos - actorEntity.GetPositionXZ();
            _Duration = finalDistance.magnitude / _Speed;
            HorizontalCharge(hSpeed, _DampCurve, _Duration, 0);
        }

        public virtual void HorizontalCharge(Vector3 _Speed, AnimationCurve _DampCurve, float _Duration, float _curTime)
        {
            if (!enable) return;
            ForwardVelocity = Vector3.zero;
            HorizontalChargeVelocity = _Speed;
            if (_DampCurve != null)
            {
                CurrentHorizontalDampCurve = _DampCurve;
            }
            else
            {
                CurrentHorizontalDampCurve = HorizontalChargeDamp;
            }
            HorizontalTimer = _curTime;
            HorizontalDuration = _Duration;
            Vector3 distance = _Speed * _Duration;
            actorEntity.SetTargetPos(actorEntity.GetPosition() + distance, false);
        }

        /// <summary>
        /// 获取当前水平方向上的冲锋信息
        /// </summary>
        public virtual void GetHorizontalCharge(ref Vector3 speed, ref AnimationCurve _DampCurve, ref float _Duration, ref float _curTime)
        {
            speed = HorizontalChargeVelocity;
            _DampCurve = CurrentHorizontalDampCurve;
            _Duration = HorizontalDuration;
            _curTime = HorizontalTimer;
        }

        Vector3 VerticalChargeHeight = Vector3.zero;
        float VerticalDuration = 0;
        float VerticalTimer = 0;

        public virtual void VerticalCharge(float _Height, AnimationCurve _DampCurve, float _Duration)
        {
            VerticalCharge(_Height, _DampCurve, _Duration, 0);
        }

        /// <summary>
        /// 垂直冲锋
        /// </summary>
        public virtual void VerticalCharge(float _Height, AnimationCurve _DampCurve, float _Duration, float _curTime)
        {
            if (!enable) return;
            VerticalChargeHeight = Vector3.up * _Height;
            if (_DampCurve != null)
            {
                CurrentVerticalDampCurve = _DampCurve;
            }
            else
            {
                CurrentVerticalDampCurve = VerticalChargeDamp;
            }
            VerticalTimer = _curTime;
            VerticalDuration = _Duration;
        }

        /// <summary>
        /// 获取当前垂直方向上的冲锋信息
        /// </summary>
        public virtual void GetVerticalCharge(ref Vector3 height, ref AnimationCurve _DampCurve, ref float _Duration, ref float _curTime)
        {
            height = VerticalChargeHeight;
            _DampCurve = CurrentVerticalDampCurve;
            _Duration = VerticalDuration;
            _curTime = VerticalTimer;
        }

        public float FloatSpeed { get; private set; }

        /// <summary>
        /// 浮空
        /// </summary>
        public virtual void Floating(float speed)
        {
            if (!enable) return;
            FloatSpeed = speed;
        }

        /// <summary>
        /// 强制结束浮空
        /// </summary>
        public virtual void StopFloating()
        {
            FloatSpeed = 0;
            StagnationDuration = 0;
        }

        float StagnationDuration = 0;

        /// <summary>
        /// 滞空
        /// </summary>
        public virtual void Stagnation(float duration)
        {
            if (!enable) return;
            StagnationDuration = duration;
        }

        public virtual void StopMove()
        {
            ForwardVelocity = Vector3.zero;
            //Direction = Vector3.zero;
        }

        public virtual void StopTurn()
        {
            AngleSpeed = 0;
            //Direction = Vector3.zero;
        }

        public virtual void StopCharge()
        {
            HorizontalChargeVelocity = Vector3.zero;
            VerticalChargeHeight = Vector3.zero;
            HorizontalDuration = 0;
            VerticalDuration = 0;
            hSpeedValue = 0;
        }

        public void Enable()
        {
            enable = true;
        }

        public void Disable()
        {
            enable = false;
            StopFloating();
            StopMove();
            StopCharge();
        }

        public virtual void Update(float delta)
        {
            if (!enable) return;
            UpdateVertical(delta);
            UpdateHorizontal(delta);
        }

        private void UpdateVertical(float delta)
        {
            Vector3 VerticalDelta = Vector3.zero;
            if (VerticalDuration > 0
                && VerticalDuration >= VerticalTimer)
            {
                float prePercent = CurrentVerticalDampCurve.Evaluate(VerticalTimer / VerticalDuration);
                VerticalTimer += delta;
                float percent = CurrentVerticalDampCurve.Evaluate(Mathf.Min(1, VerticalTimer / VerticalDuration));
                if (VerticalTimer >= VerticalDuration)
                {
                    VerticalDuration = 0;
                }
                VerticalDelta = VerticalChargeHeight * (percent - prePercent);
                actorEntity.SetPosition(actorEntity.GetPositionXZ() + VerticalDelta);
            }
        }

        private Vector3 UpdateHorizontalForwardDelta;
        private Vector3 UpdateHorizontalDelta;
        private Vector3 UpdateForwardSpeed;
        private Vector3 UpdateHorizontalSpeed;
        private Vector3 currentActorPosition;
        private Vector3 targetPosXZ = Vector3.zero;

        private void UpdateHorizontal(float delta)
        {
            UpdateHorizontalForwardDelta = Vector3.zero;
            UpdateHorizontalDelta = Vector3.zero;
            UpdateForwardSpeed = Vector3.zero;
            UpdateHorizontalSpeed = Vector3.zero;
            currentActorPosition = actorEntity.GetPositionXZ();
            targetPosXZ.x = actorEntity.targetPos.x;
            targetPosXZ.z = actorEntity.targetPos.z;
            bool isReachTarget = false;
            if (ForwardVelocity != Vector3.zero)
            {
                UpdateForwardSpeed = ForwardVelocity;
                UpdateHorizontalForwardDelta = ForwardVelocity * delta;
                float dis = MathHelper.DistanceVec3(currentActorPosition, targetPosXZ);
                if (Math.Round(dis, 2) <= Math.Round(UpdateHorizontalForwardDelta.magnitude, 2))
                {
                    UpdateHorizontalForwardDelta = new Vector3(targetPosXZ.x - currentActorPosition.x, 0, targetPosXZ.z - currentActorPosition.z);
                    isReachTarget = true;
                }
            }
            if (HorizontalDuration > 0
                && HorizontalDuration >= HorizontalTimer)
            {
                float percent = CurrentHorizontalDampCurve.Evaluate(HorizontalTimer / HorizontalDuration);
                HorizontalTimer += delta;
                if (HorizontalTimer >= HorizontalDuration)
                {
                    HorizontalDuration = 0;
                }
                UpdateHorizontalSpeed = HorizontalChargeVelocity * percent;
                UpdateHorizontalDelta = UpdateHorizontalSpeed * delta;
            }
            Vector3 finalDelta = UpdateHorizontalDelta + UpdateHorizontalForwardDelta;
            Vector3 finalPos = actorEntity.GetPosition() + finalDelta;
            if (isReachTarget)
            {
                finalPos = actorEntity.targetPos;
            }
            actorEntity.SetPosition(finalPos);
            if (isReachTarget && finalPos != actorEntity.targetPos)
            {
                actorEntity.SetTargetPos(finalPos);
            }
        }
    }
}