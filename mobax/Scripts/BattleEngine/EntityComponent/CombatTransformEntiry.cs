namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

    public class CombatTransformEntiry : Entity
    {
        public enum Space
        {
            Self,
            World
        }

        public Matrix33 orientation;
        public Vector3 speedVector;

        private Vector3 _localPosition;

        public Vector3 localPosition
        {
            get { return _localPosition; }
            set
            {
                _localPosition = value;
                if (parent != null)
                {
                    _prevPosition = _position;
                    _position = TransformPoint(this._localPosition);
                    UpdateChildPosition();
                }
                else
                {
                    _prevPosition = _position;
                    _position = _localPosition;
                    UpdateChildPosition();
                }
            }
        }

        private Vector3 _prevPosition;
        private Vector3 _position;

        public Vector3 position
        {
            get { return _position; }
            set
            {
                _prevPosition = _position;
                _position = value;
                if (parent != null)
                {
                    _localPosition = parent.InverseTransformPoint(position);
                }
                else
                {
                    _localPosition = new Vector3(_position.x, _position.y, _position.z);
                }
                UpdateChildPosition();
            }
        }

        private Quaternion _localRotation;

        public Quaternion localRotation
        {
            get { return _localRotation; }
            set
            {
                _localRotation = value;
                if (parent != null)
                {
                    _rotation = parent.rotation * this._localRotation;
                }
                else
                {
                    _rotation = new Quaternion(_localRotation.x, _localRotation.y, _localRotation.z, _localRotation.w);
                }
                UpdateChildRotation();
            }
        }

        private Quaternion _rotation = new Quaternion(0, 0, 0, 1);

        public Quaternion rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                if (parent != null)
                {
                    _localRotation = Quaternion.Inverse(parent.rotation) * _rotation;
                }
                else
                {
                    _localRotation = new Quaternion(_rotation.x, _rotation.y, _rotation.z, _rotation.w);
                }
                UpdateChildRotation();
            }
        }

        private Vector3 _localScale;

        public Vector3 localScale
        {
            get { return _localScale; }
            set { _localScale = value; }
        }

        private Vector3 _scale;

        public Vector3 scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        public CombatTransformEntiry parent;
        public List<CombatTransformEntiry> children = new List<CombatTransformEntiry>();

        private bool initialized = false;

        /**
        *  @brief Initializes internal properties based on whether there is a {@link TSCollider} attached.
        **/
        public void Initialize(CombatTransformEntiry parent = null)
        {
            if (initialized) return;
            this._localPosition = Vector3.zero;
            this._localRotation = new Quaternion(0, 0, 0, 1);
            this._localScale = Vector3.one;
            this.parent = parent;
            if (parent == null)
            {
                this._position = Vector3.zero;
                this._rotation = new Quaternion(0, 0, 0, 1);
                this._scale = Vector3.one;
            }
            else
            {
                this._position = TransformPoint(this._localPosition);
                this._rotation = parent.rotation * this._localRotation;
                this._scale = Vector3.one;
            }
            this.speedVector = Vector3.zero;
            initialized = true;
        }

        public void AddChild(CombatTransformEntiry child)
        {
            child.parent = this;
            children.Add(child);
        }

        /**
        *  @brief Rotates game object to point forward vector to a target position. 
        *
        *  @param other TSTrasform used to get target position.
        **/
        public void LookAt(CombatTransformEntiry other)
        {
            LookAt(other.position);
        }

        /**
        *  @brief Rotates game object to point forward vector to a target position. 
        *
        *  @param target Target position.
        **/
        public void LookAt(Vector3 target)
        {
            Vector3 offset = target - position;
            Quaternion q = Quaternion.FromToRotation(Vector3.forward, offset);
            Vector3 newUp = q * Vector3.forward;
            Vector3 worldUp = Vector3.up;
            float dirDot = Vector3.Dot(newUp, worldUp);
            Vector3 vProj = worldUp - newUp * dirDot; //worldUp在xy平面上的投影量
            vProj.Normalize();
            float dotproj = Vector3.Dot(vProj, newUp);
            float theta = Mathf.Acos(dotproj) * Mathf.Rad2Deg;
            Quaternion qNew = Quaternion.AngleAxis(theta, newUp);
            Quaternion qall = qNew * q;
            rotation = Quaternion.Euler(qall.eulerAngles);
        }

        /**
        *  @brief Moves game object based on provided axis values.
        **/
        public void Translate(float x, float y, float z)
        {
            Translate(x, y, z, Space.Self);
        }

        /**
        *  @brief Moves game object based on provided axis values and a relative space.
        *
        *  If relative space is SELF then the game object will move based on its forward vector.
        **/
        public void Translate(float x, float y, float z, Space relativeTo)
        {
            Translate(new Vector3(x, y, z), relativeTo);
        }

        /**
        *  @brief Moves game object based on provided axis values and a relative {@link CombatTransformEntiry}.
        *
        *  The game object will move based on CombatTransformEntiry's forward vector.
        **/
        public void Translate(float x, float y, float z, CombatTransformEntiry relativeTo)
        {
            Translate(new Vector3(x, y, z), relativeTo);
        }

        /**
        *  @brief Moves game object based on provided translation vector.
        **/
        public void Translate(Vector3 translation)
        {
            Translate(translation, Space.Self);
        }

        /**
        *  @brief Moves game object based on provided translation vector and a relative space.
        *
        *  If relative space is SELF then the game object will move based on its forward vector.
        **/
        public void Translate(Vector3 translation, Space relativeTo)
        {
            if (relativeTo == Space.Self)
            {
                Translate(translation, this);
            }
            else
            {
                this.position += translation;
            }
        }

        /**
        *  @brief Moves game object based on provided translation vector and a relative {@link CombatTransformEntiry}.
        *
        *  The game object will move based on CombatTransformEntiry's forward vector.
        **/
        public void Translate(Vector3 translation, CombatTransformEntiry relativeTo)
        {
            Quaternion rotaion = Quaternion.Euler(relativeTo.rotation.eulerAngles);
            var deltaPos = rotaion * translation;
            this.position += deltaPos;
        }

        /**
        *  @brief Rotates game object based on provided axis, point and angle of rotation.
        **/
        public void RotateAround(Vector3 point, Vector3 axis, float angle)
        {
            Vector3 vector = this.position;
            Vector3 vector2 = vector - point;
            Matrix33 matrix = Matrix33.AngleAxis(angle * Mathf.Deg2Rad, axis);
            float num0 = ((position.x * matrix.m00) + (position.y * matrix.m10)) + (position.z * matrix.m20);
            float num1 = ((position.x * matrix.m01) + (position.y * matrix.m11)) + (position.z * matrix.m21);
            float num2 = ((position.x * matrix.m02) + (position.y * matrix.m12)) + (position.z * matrix.m22);
            vector2 = new Vector3(num0, num1, num2);
            vector = point + vector2;
            this.position = vector;
            Rotate(axis, angle);
        }

        /**
        *  @brief Rotates game object based on provided axis and angle of rotation.
        **/
        public void RotateAround(Vector3 axis, float angle)
        {
            Rotate(axis, angle);
        }

        /**
        *  @brief Rotates game object based on provided axis angles of rotation.
        **/
        public void Rotate(float xAngle, float yAngle, float zAngle)
        {
            Rotate(new Vector3(xAngle, yAngle, zAngle), Space.Self);
        }

        /**
        *  @brief Rotates game object based on provided axis angles of rotation and a relative space.
        *
        *  If relative space is SELF then the game object will rotate based on its forward vector.
        **/
        public void Rotate(float xAngle, float yAngle, float zAngle, Space relativeTo)
        {
            Rotate(new Vector3(xAngle, yAngle, zAngle), relativeTo);
        }

        /**
        *  @brief Rotates game object based on provided axis angles of rotation.
        **/
        public void Rotate(Vector3 eulerAngles)
        {
            Rotate(eulerAngles, Space.Self);
        }

        /**
        *  @brief Rotates game object based on provided axis and angle of rotation.
        **/
        public void Rotate(Vector3 axis, float angle)
        {
            Rotate(axis, angle, Space.Self);
        }

        /**
        *  @brief Rotates game object based on provided axis, angle of rotation and relative space.
        *
        *  If relative space is SELF then the game object will rotate based on its forward vector.
        **/
        public void Rotate(Vector3 axis, float angle, Space relativeTo)
        {
            Quaternion result = Quaternion.identity;
            if (relativeTo == Space.Self)
            {
                result = this.rotation * Quaternion.AngleAxis(angle, axis);
            }
            else
            {
                result = Quaternion.AngleAxis(angle, axis) * this.rotation;
            }
            result.Normalize();
            this.rotation = result;
            UpdateChildRotation();
        }

        /**
        *  @brief Rotates game object based on provided axis angles and relative space.
        *
        *  If relative space is SELF then the game object will rotate based on its forward vector.
        **/
        public void Rotate(Vector3 eulerAngles, Space relativeTo)
        {
            Quaternion result = Quaternion.identity;
            if (relativeTo == Space.Self)
            {
                result = this.rotation * Quaternion.Euler(eulerAngles);
            }
            else
            {
                result = Quaternion.Euler(eulerAngles) * this.rotation;
            }
            result.Normalize();
            this.rotation = result;
            UpdateChildRotation();
        }

        /**
        *  @brief Current self forward vector.
        **/
        public Vector3 forward
        {
            get
            {
                Quaternion rotaion = Quaternion.Euler(rotation.eulerAngles);
                return rotaion * Vector3.forward;
            }
            set
            {
                LookAt(position + value.normalized);
                UpdateChildRotation();
            }
        }

        /**
        *  @brief Current self right vector.
        **/
        public Vector3 right
        {
            get
            {
                Quaternion rotaion = Quaternion.Euler(rotation.eulerAngles);
                return rotaion * Vector3.right;
            }
        }

        /**
        *  @brief Current self up vector.
        **/
        public Vector3 up
        {
            get
            {
                Quaternion rotaion = Quaternion.Euler(rotation.eulerAngles);
                return rotaion * Vector3.up;
            }
        }

        /**
        *  @brief Returns Euler angles in degrees.
        **/
        public Vector3 eulerAngles
        {
            get { return rotation.eulerAngles; }
            set
            {
                //rotation = Quaternion.Euler(value);
                //UpdateChildRotation();
            }
        }

        public Matrix4x4 localToWorldMatrix
        {
            get { return Matrix4x4.Inverse(worldToLocalMatrix); }
        }

        public Matrix4x4 worldToLocalMatrix
        {
            get
            {
                Matrix4x4 trans = new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(-position.x, -position.y, -position.z, 1));
                Matrix4x4 rotZ = new Matrix4x4(new Vector4(Mathf.Cos(-rotation.z * Mathf.PI / 180), Mathf.Sin(-rotation.z * Mathf.PI / 180), 0, 0), new Vector4(-Mathf.Sin(-rotation.z * Mathf.PI / 180), Mathf.Cos(-rotation.z * Mathf.PI / 180), 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1));
                Matrix4x4 rotX = new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, Mathf.Cos(-rotation.x * Mathf.PI / 180), Mathf.Sin(-rotation.x * Mathf.PI / 180), 0), new Vector4(0, -Mathf.Sin(-rotation.x * Mathf.PI / 180), Mathf.Cos(-rotation.x * Mathf.PI / 180), 0), new Vector4(0, 0, 0, 1));
                Matrix4x4 rotY = new Matrix4x4(new Vector4(Mathf.Cos(-rotation.y * Mathf.PI / 180), 0, -Mathf.Sin(-rotation.y * Mathf.PI / 180), 0), new Vector4(0, 1, 0, 0), new Vector4(Mathf.Sin(-rotation.y * Mathf.PI / 180), 0, Mathf.Cos(-rotation.y * Mathf.PI / 180), 0), new Vector4(0, 0, 0, 1));
                Matrix4x4 Mview = (new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1))) * rotZ * rotX * rotY * trans;
                return Mview;
            }
        }

        /**
         *  @brief Transform a point from local space to world space.
         **/
        public Vector4 TransformPoint(Vector4 point)
        {
            return localToWorldMatrix * point;
        }

        public Vector3 TransformPoint(Vector3 point)
        {
            return localToWorldMatrix * point;
        }

        /**
         *  @brief Transform a point from world space to local space.
         **/
        public Vector4 InverseTransformPoint(Vector4 point)
        {
            return worldToLocalMatrix * point;
        }

        public Vector3 InverseTransformPoint(Vector3 point)
        {
            return worldToLocalMatrix * point;
        }

        /**
         *  @brief Transform a direction from local space to world space.
         **/
        public Vector4 TransformDirection(Vector4 direction)
        {
            Matrix4x4 matrix = Matrix4x4.Translate(position) * Matrix4x4.Rotate(rotation);
            return matrix * direction;
        }

        public Vector3 TransformDirection(Vector3 direction)
        {
            return TransformDirection(new Vector4(direction.x, direction.y, direction.z, 0));
        }

        /**
         *  @brief Transform a direction from world space to local space.
         **/
        public Vector4 InverseTransformDirection(Vector4 direction)
        {
            Matrix4x4 matrix = Matrix4x4.Translate(position) * Matrix4x4.Rotate(rotation);
            return Matrix4x4.Inverse(matrix) * direction;
        }

        public Vector3 InverseTransformDirection(Vector3 direction)
        {
            return InverseTransformDirection(new Vector4(direction.x, direction.y, direction.z, 0));
        }

        /**
         *  @brief Transform a vector from local space to world space.
         **/
        public Vector4 TransformVector(Vector4 vector)
        {
            return localToWorldMatrix * vector;
        }

        public Vector3 TransformVector(Vector3 vector)
        {
            return TransformVector(new Vector4(vector.x, vector.y, vector.z, 0));
        }

        /**
         *  @brief Transform a vector from world space to local space.
         **/
        public Vector4 InverseTransformVector(Vector4 vector)
        {
            return worldToLocalMatrix * vector;
        }

        public Vector3 InverseTransformVector(Vector3 vector)
        {
            return InverseTransformVector(new Vector4(vector.x, vector.y, vector.z, 0));
        }

        private void UpdateChildPosition()
        {
            for (int i = 0; i < children.Count; ++i)
            {
                children[i].Translate(_position - _prevPosition);
            }
        }

        private void UpdateChildRotation()
        {
            Matrix33 matrix = Matrix33.CreateFromQuaternion(_rotation);
            for (int i = 0; i < children.Count; ++i)
            {
                CombatTransformEntiry child = children[i];
                child.localRotation = Quaternion.Inverse(_rotation);
                child.localPosition = Matrix33.CreateFromQuaternion(child.localRotation) * child.localPosition;
                child.position = TransformPoint(child.localPosition);
            }
        }
    }
}