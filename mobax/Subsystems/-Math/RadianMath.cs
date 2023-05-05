//ybzuo-ro
using UnityEngine;
    public class RadianMath
    {
        public const float _DEG2RAD = 0.0174532925199433f;
        public const float _RAD2DEG = 57.2957795130823f;

        public const float DA5 = 5.0f * _DEG2RAD;
        public const float DA10 = 10.0f * _DEG2RAD;
        public const float DA15 = 15.0f * _DEG2RAD;
        public const float DA20 = 20.0f * _DEG2RAD;
        public const float DA25 = 25.0f * _DEG2RAD;
        public const float DA30 = 30.0f * _DEG2RAD;
        public const float DA35 = 35.0f * _DEG2RAD;
        public const float DA40 = 40.0f * _DEG2RAD;
        public const float DA45 = 45.0f * _DEG2RAD;
        public const float DA50 = 50.0f * _DEG2RAD;
        public const float DA55 = 55.0f * _DEG2RAD;
        public const float DA60 = 60.0f * _DEG2RAD;
        public const float DA65 = 65.0f * _DEG2RAD;
        public const float DA70 = 70.0f * _DEG2RAD;
        public const float DA75 = 75.0f * _DEG2RAD;
        public const float DA80 = 80.0f * _DEG2RAD;
        public const float DA85 = 85.0f * _DEG2RAD;
        public const float DA90 = 90.0f * _DEG2RAD;
        public const float DA120 = 120.0f * _DEG2RAD;
        public const float DA180 = 180.0f * _DEG2RAD;
        public const float DA270 = 270.0f * _DEG2RAD;
        public const float DA360 = 360.0f * _DEG2RAD;



        /*
        public static float vector_to_radian(Vector3 _forward)
        {
            float result_value = Mathf.Asin(_forward.y);
            result_value = limit_radian(result_value);
            if (_forward.x < 0.0f)
            {
                if (_forward.y >= 0.0f)
                {
                    result_value = DA180 - result_value;
                }
                else
                {
                    result_value = DA180 + DA360 - result_value;
                }
            }
            return result_value;
        }
         */
    }
