using UnityEngine;
public struct CombineDeg
    {
        public int h_deg;
        public int v_deg;
        public CombineDeg(int _h_deg, int _v_deg)
        {
            h_deg = _h_deg;
            v_deg = _v_deg;
        }
        public CombineRad convert_to_rad()
        {
            CombineRad temp_rad;
            temp_rad.h_rad = RadianMath._DEG2RAD * h_deg;
            temp_rad.v_rad = RadianMath._DEG2RAD * v_deg;
            return temp_rad;
        }
        public Vector3 to_vector()
        {
            return convert_to_rad().to_vector();
        }


    }
