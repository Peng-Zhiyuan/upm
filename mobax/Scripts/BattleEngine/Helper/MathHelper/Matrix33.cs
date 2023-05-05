namespace BattleEngine.Logic
{
    using UnityEngine;

    public struct Matrix33
    {
        public static readonly Matrix33 zero = new Matrix33(Vector3.zero, Vector3.zero, Vector3.zero);

        public static readonly Matrix33 identity = new Matrix33(new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1));

        // mRowCol  列优先存储
        public float m00;
        public float m10;
        public float m20;
        public float m01;
        public float m11;
        public float m21;
        public float m02;
        public float m12;
        public float m22;

        public Matrix33(Vector3 column0, Vector3 column1, Vector3 column2)
        {
            this.m00 = column0.x;
            this.m01 = column1.x;
            this.m02 = column2.x;
            this.m10 = column0.y;
            this.m11 = column1.y;
            this.m12 = column2.y;
            this.m20 = column0.z;
            this.m21 = column1.z;
            this.m22 = column2.z;
        }

        public float this[int row, int column]
        {
            get { return this[row + column * 3]; }
            set { this[row + column * 3] = value; }
        }
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.m00;
                    case 1:
                        return this.m10;
                    case 2:
                        return this.m20;
                    case 3:
                        return this.m01;
                    case 4:
                        return this.m11;
                    case 5:
                        return this.m21;
                    case 6:
                        return this.m02;
                    case 7:
                        return this.m12;
                    case 8:
                        return this.m22;
                    default:
                        return 0;
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this.m00 = value;
                        break;
                    case 1:
                        this.m10 = value;
                        break;
                    case 2:
                        this.m20 = value;
                        break;
                    case 3:
                        this.m01 = value;
                        break;
                    case 4:
                        this.m11 = value;
                        break;
                    case 5:
                        this.m21 = value;
                        break;
                    case 6:
                        this.m02 = value;
                        break;
                    case 7:
                        this.m12 = value;
                        break;
                    case 8:
                        this.m22 = value;
                        break;
                }
            }
        }

        public override int GetHashCode()
        {
            return this.GetColumn(0).GetHashCode() ^ this.GetColumn(1).GetHashCode() << 2 ^ this.GetColumn(2).GetHashCode() >> 2;
        }

        public override bool Equals(object other)
        {
            if (!(other is Matrix33))
                return false;
            return this.Equals((Matrix33)other);
        }

        public bool Equals(Matrix33 other)
        {
            return this.GetColumn(0).Equals(other.GetColumn(0)) && this.GetColumn(1).Equals(other.GetColumn(1)) && this.GetColumn(2).Equals(other.GetColumn(2));
        }

        /// <summary>
        /// Multiplies two matrices.
        /// </summary>
        /// <param name="value1">The first matrix.</param>
        /// <param name="value2">The second matrix.</param>
        /// <returns>The product of both values.</returns>
#region public static JMatrix operator *(JMatrix value1,JMatrix value2)
        public static Matrix33 operator *(Matrix33 value1, Matrix33 value2)
        {
            Matrix33 result;
            Matrix33.Multiply(ref value1, ref value2, out result);
            return result;
        }
#endregion

        public static Vector3 operator *(Matrix33 lhs, Vector3 vector3)
        {
            Vector3 vec = new Vector3();
            vec.x = lhs.m00 * vector3.x + lhs.m01 * vector3.y + lhs.m02 * vector3.z;
            vec.y = lhs.m10 * vector3.x + lhs.m11 * vector3.y + lhs.m12 * vector3.z;
            vec.z = lhs.m20 * vector3.x + lhs.m21 * vector3.y + lhs.m22 * vector3.z;
            return vec;
        }

        public static bool operator ==(Matrix33 lhs, Matrix33 rhs)
        {
            return lhs.GetColumn(0) == rhs.GetColumn(0) && lhs.GetColumn(1) == rhs.GetColumn(1) && lhs.GetColumn(2) == rhs.GetColumn(2);
        }

        public static bool operator !=(Matrix33 lhs, Matrix33 rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        ///   <para>Get a column of the matrix.</para>
        /// </summary>
        /// <param name="index"></param>
        public Vector3 GetColumn(int index)
        {
            switch (index)
            {
                case 0:
                    return new Vector3(this.m00, this.m10, this.m20);
                case 1:
                    return new Vector3(this.m01, this.m11, this.m21);
                case 2:
                    return new Vector3(this.m02, this.m12, this.m22);
                default:
                    return Vector3.zero;
            }
        }

        /// <summary>
        ///   <para>Returns a row of the matrix.</para>
        /// </summary>
        /// <param name="index"></param>
        public Vector3 GetRow(int index)
        {
            switch (index)
            {
                case 0:
                    return new Vector3(this.m00, this.m01, this.m02);
                case 1:
                    return new Vector3(this.m10, this.m11, this.m12);
                case 2:
                    return new Vector3(this.m20, this.m21, this.m22);
                default:
                    return Vector3.zero;
            }
        }

        /// <summary>
        ///   <para>Sets a column of the matrix.</para>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="column"></param>
        public void SetColumn(int index, Vector3 column)
        {
            this[0, index] = column.x;
            this[1, index] = column.y;
            this[2, index] = column.z;
        }

        /// <summary>
        ///   <para>Sets a row of the matrix.</para>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="row"></param>
        public void SetRow(int index, Vector3 row)
        {
            this[index, 0] = row.x;
            this[index, 1] = row.y;
            this[index, 2] = row.z;
        }

        /// <summary>
        /// Multiply two matrices. Notice: matrix multiplication is not commutative.
        /// </summary>
        /// <param name="matrix1">The first matrix.</param>
        /// <param name="matrix2">The second matrix.</param>
        /// <returns>The product of both matrices.</returns>
#region public static JMatrix Multiply(JMatrix matrix1, JMatrix matrix2)
        public static Matrix33 Multiply(Matrix33 matrix1, Matrix33 matrix2)
        {
            Matrix33 result;
            Matrix33.Multiply(ref matrix1, ref matrix2, out result);
            return result;
        }

        /// <summary>
        /// Multiply two matrices. Notice: matrix multiplication is not commutative.
        /// </summary>
        /// <param name="matrix1">The first matrix.</param>
        /// <param name="matrix2">The second matrix.</param>
        /// <param name="result">The product of both matrices.</param>
        public static void Multiply(ref Matrix33 matrix1, ref Matrix33 matrix2, out Matrix33 result)
        {
            float num0 = ((matrix1.m00 * matrix2.m00) + (matrix1.m01 * matrix2.m10)) + (matrix1.m02 * matrix2.m20);
            float num1 = ((matrix1.m00 * matrix2.m01) + (matrix1.m01 * matrix2.m11)) + (matrix1.m02 * matrix2.m21);
            float num2 = ((matrix1.m00 * matrix2.m02) + (matrix1.m01 * matrix2.m12)) + (matrix1.m02 * matrix2.m22);
            float num3 = ((matrix1.m10 * matrix2.m00) + (matrix1.m11 * matrix2.m10)) + (matrix1.m12 * matrix2.m20);
            float num4 = ((matrix1.m10 * matrix2.m01) + (matrix1.m11 * matrix2.m11)) + (matrix1.m12 * matrix2.m21);
            float num5 = ((matrix1.m10 * matrix2.m02) + (matrix1.m11 * matrix2.m12)) + (matrix1.m12 * matrix2.m22);
            float num6 = ((matrix1.m20 * matrix2.m00) + (matrix1.m21 * matrix2.m10)) + (matrix1.m22 * matrix2.m20);
            float num7 = ((matrix1.m20 * matrix2.m01) + (matrix1.m21 * matrix2.m11)) + (matrix1.m22 * matrix2.m21);
            float num8 = ((matrix1.m20 * matrix2.m02) + (matrix1.m21 * matrix2.m12)) + (matrix1.m22 * matrix2.m22);
            result.m00 = num0;
            result.m01 = num1;
            result.m02 = num2;
            result.m10 = num3;
            result.m11 = num4;
            result.m12 = num5;
            result.m20 = num6;
            result.m21 = num7;
            result.m22 = num8;
        }
#endregion

        /// <summary>
        /// Multiply a matrix by a scalefactor.
        /// </summary>
        /// <param name="matrix1">The matrix.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <returns>A JMatrix multiplied by the scale factor.</returns>
#region public static JMatrix Multiply(JMatrix matrix1, float scaleFactor)
        public static Matrix33 Multiply(Matrix33 matrix1, float scaleFactor)
        {
            Matrix33 result;
            Matrix33.Multiply(ref matrix1, scaleFactor, out result);
            return result;
        }

        /// <summary>
        /// Multiply a matrix by a scalefactor.
        /// </summary>
        /// <param name="matrix1">The matrix.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <param name="result">A JMatrix multiplied by the scale factor.</param>
        public static void Multiply(ref Matrix33 matrix1, float scaleFactor, out Matrix33 result)
        {
            float num = scaleFactor;
            result.m00 = matrix1.m00 * num;
            result.m01 = matrix1.m01 * num;
            result.m02 = matrix1.m02 * num;
            result.m10 = matrix1.m10 * num;
            result.m11 = matrix1.m11 * num;
            result.m12 = matrix1.m12 * num;
            result.m20 = matrix1.m20 * num;
            result.m21 = matrix1.m21 * num;
            result.m22 = matrix1.m22 * num;
        }
#endregion

        /// <summary>
        /// Creates a JMatrix representing an orientation from a quaternion.
        /// </summary>
        /// <param name="quaternion">The quaternion the matrix should be created from.</param>
        /// <returns>JMatrix representing an orientation.</returns>
#region public static JMatrix CreateFromQuaternion(JQuaternion quaternion)
        public static Matrix33 CreateFromLookAt(Vector3 position, Vector3 target)
        {
            Matrix33 result;
            LookAt(target - position, Vector3.up, out result);
            return result;
        }

        public static Matrix33 LookAt(Vector3 forward, Vector3 upwards)
        {
            Matrix33 result;
            LookAt(forward, upwards, out result);
            return result;
        }

        public static void LookAt(Vector3 forward, Vector3 upwards, out Matrix33 result)
        {
            Vector3 zaxis = forward;
            zaxis.Normalize();
            Vector3 xaxis = Vector3.Cross(upwards, zaxis);
            xaxis.Normalize();
            Vector3 yaxis = Vector3.Cross(zaxis, xaxis);
            result.m00 = xaxis.x;
            result.m10 = yaxis.x;
            result.m20 = zaxis.x;
            result.m01 = xaxis.y;
            result.m11 = yaxis.y;
            result.m21 = zaxis.y;
            result.m02 = xaxis.z;
            result.m12 = yaxis.z;
            result.m22 = zaxis.z;
        }

        public static Matrix33 CreateFromQuaternion(Quaternion quaternion)
        {
            Matrix33 result;
            Matrix33.CreateFromQuaternion(ref quaternion, out result);
            return result;
        }

        /// <summary>
        /// Creates a JMatrix representing an orientation from a quaternion.
        /// </summary>
        /// <param name="quaternion">The quaternion the matrix should be created from.</param>
        /// <param name="result">JMatrix representing an orientation.</param>
        public static void CreateFromQuaternion(ref Quaternion quaternion, out Matrix33 result)
        {
            float num9 = quaternion.x * quaternion.x;
            float num8 = quaternion.y * quaternion.y;
            float num7 = quaternion.z * quaternion.z;
            float num6 = quaternion.x * quaternion.y;
            float num5 = quaternion.z * quaternion.w;
            float num4 = quaternion.z * quaternion.x;
            float num3 = quaternion.y * quaternion.w;
            float num2 = quaternion.y * quaternion.z;
            float num1 = quaternion.x * quaternion.w;
            result.m00 = 1 - (2 * (num8 + num7));
            result.m01 = 2 * (num6 + num5);
            result.m02 = 2 * (num4 - num3);
            result.m10 = 2 * (num6 - num5);
            result.m11 = 1 - (2 * (num7 + num9));
            result.m12 = 2 * (num2 + num1);
            result.m20 = 2 * (num4 + num3);
            result.m21 = 2 * (num2 - num1);
            result.m22 = 1 - (2 * (num8 + num9));
        }
#endregion

        /// <summary>
        /// Creates a matrix which rotates around the given axis by the given angle.
        /// </summary>
        /// <param name="axis">The axis.</param>
        /// <param name="angle">The angle.</param>
        /// <param name="result">The resulting rotation matrix</param>
#region public static void CreateFromAxisAngle(ref JVector axis, float angle, out JMatrix result)
        public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Matrix33 result)
        {
            float x = axis.x;
            float y = axis.y;
            float z = axis.z;
            float num2 = Mathf.Sin(angle);
            float num1 = Mathf.Cos(angle);
            float num11 = x * x;
            float num10 = y * y;
            float num9 = z * z;
            float num8 = x * y;
            float num7 = x * z;
            float num6 = y * z;
            result.m00 = num11 + (num1 * (1 - num11));
            result.m01 = (num8 - (num1 * num8)) + (num2 * z);
            result.m02 = (num7 - (num1 * num7)) - (num2 * y);
            result.m10 = (num8 - (num1 * num8)) - (num2 * z);
            result.m11 = num10 + (num1 * (1 - num10));
            result.m12 = (num6 - (num1 * num6)) + (num2 * x);
            result.m20 = (num7 - (num1 * num7)) + (num2 * y);
            result.m21 = (num6 - (num1 * num6)) - (num2 * x);
            result.m22 = num9 + (num1 * (1 - num9));
        }

        /// <summary>
        /// Creates a matrix which rotates around the given axis by the given angle.
        /// </summary>
        /// <param name="axis">The axis.</param>
        /// <param name="angle">The angle.</param>
        /// <returns>The resulting rotation matrix</returns>
        public static Matrix33 AngleAxis(float angle, Vector3 axis)
        {
            Matrix33 result;
            CreateFromAxisAngle(ref axis, angle, out result);
            return result;
        }
#endregion

        /// <summary>
        /// Calculates the inverse of a give matrix.
        /// </summary>
        /// <param name="matrix">The matrix to invert.</param>
        /// <returns>The inverted JMatrix.</returns>
#region public static JMatrix Inverse(JMatrix matrix)
        public static Matrix33 Inverse(Matrix33 matrix)
        {
            Matrix33 result;
            Matrix33.Inverse(ref matrix, out result);
            return result;
        }

        public float Determinant()
        {
            return m00 * m11 * m22 + m01 * m12 * m20 + m02 * m10 * m21 - m20 * m11 * m02 - m21 * m12 * m00 - m22 * m10 * m01;
        }

        public static void Invert(ref Matrix33 matrix, out Matrix33 result)
        {
            float determinantInverse = 1 / matrix.Determinant();
            float m11 = (matrix.m11 * matrix.m22 - matrix.m12 * matrix.m21) * determinantInverse;
            float m12 = (matrix.m02 * matrix.m21 - matrix.m22 * matrix.m01) * determinantInverse;
            float m13 = (matrix.m01 * matrix.m12 - matrix.m11 * matrix.m02) * determinantInverse;
            float m21 = (matrix.m12 * matrix.m20 - matrix.m10 * matrix.m22) * determinantInverse;
            float m22 = (matrix.m00 * matrix.m22 - matrix.m02 * matrix.m20) * determinantInverse;
            float m23 = (matrix.m02 * matrix.m10 - matrix.m00 * matrix.m12) * determinantInverse;
            float m31 = (matrix.m10 * matrix.m21 - matrix.m11 * matrix.m20) * determinantInverse;
            float m32 = (matrix.m01 * matrix.m20 - matrix.m00 * matrix.m21) * determinantInverse;
            float m33 = (matrix.m00 * matrix.m11 - matrix.m01 * matrix.m10) * determinantInverse;
            result.m00 = m11;
            result.m01 = m12;
            result.m02 = m13;
            result.m10 = m21;
            result.m11 = m22;
            result.m12 = m23;
            result.m20 = m31;
            result.m21 = m32;
            result.m22 = m33;
        }

        /// <summary>
        /// Calculates the inverse of a give matrix.
        /// </summary>
        /// <param name="matrix">The matrix to invert.</param>
        /// <param name="result">The inverted JMatrix.</param>
        public static void Inverse(ref Matrix33 matrix, out Matrix33 result)
        {
            float det = 1024 * matrix.m00 * matrix.m11 * matrix.m22 - 1024 * matrix.m00 * matrix.m12 * matrix.m21 - 1024 * matrix.m01 * matrix.m10 * matrix.m22 + 1024 * matrix.m01 * matrix.m12 * matrix.m20 + 1024 * matrix.m02 * matrix.m10 * matrix.m21 - 1024 * matrix.m02 * matrix.m11 * matrix.m20;
            float num11 = 1024 * matrix.m11 * matrix.m22 - 1024 * matrix.m12 * matrix.m21;
            float num12 = 1024 * matrix.m02 * matrix.m21 - 1024 * matrix.m01 * matrix.m22;
            float num13 = 1024 * matrix.m01 * matrix.m12 - 1024 * matrix.m11 * matrix.m02;
            float num21 = 1024 * matrix.m12 * matrix.m20 - 1024 * matrix.m22 * matrix.m10;
            float num22 = 1024 * matrix.m00 * matrix.m22 - 1024 * matrix.m20 * matrix.m02;
            float num23 = 1024 * matrix.m02 * matrix.m10 - 1024 * matrix.m12 * matrix.m00;
            float num31 = 1024 * matrix.m10 * matrix.m21 - 1024 * matrix.m20 * matrix.m11;
            float num32 = 1024 * matrix.m01 * matrix.m20 - 1024 * matrix.m21 * matrix.m00;
            float num33 = 1024 * matrix.m00 * matrix.m11 - 1024 * matrix.m10 * matrix.m01;
            if (det == 0)
            {
                result.m00 = float.PositiveInfinity;
                result.m01 = float.PositiveInfinity;
                result.m02 = float.PositiveInfinity;
                result.m10 = float.PositiveInfinity;
                result.m11 = float.PositiveInfinity;
                result.m12 = float.PositiveInfinity;
                result.m20 = float.PositiveInfinity;
                result.m21 = float.PositiveInfinity;
                result.m22 = float.PositiveInfinity;
            }
            else
            {
                result.m00 = num11 / det;
                result.m01 = num12 / det;
                result.m02 = num13 / det;
                result.m10 = num21 / det;
                result.m11 = num22 / det;
                result.m12 = num23 / det;
                result.m20 = num31 / det;
                result.m21 = num32 / det;
                result.m22 = num33 / det;
            }
        }
#endregion

        public override string ToString()
        {
            return $"{m00},{m10},{m20},{m01},{m11},{m21},{m02},{m12},{m22}";
        }
    }
}