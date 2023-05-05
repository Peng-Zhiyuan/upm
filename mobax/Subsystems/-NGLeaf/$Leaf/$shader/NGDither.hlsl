#ifndef DITHER_CLIP_INCLUDED
#define DITHER_CLIP_INCLUDED

//sampler2D _Dither;

// Returns > 0 if not clipped, < 0 if clipped based
// on the dither
// For use with the "clip" function
// pos is the fragment position in screen space from [0,1]
/*
float Dithered(float2 pos) {
    pos *= _ScreenParams.xy;
    //pos.x -= _ScreenParams.x / 2;
    //pos.y -= _ScreenParams.y / 2;
    // Define a dither threshold matrix which can
    // be used to define how a 4x4 set of pixels
    // will be dithered
    float DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };

    int index = (floor(pos.x) % 4) * 4 + floor(pos.y) % 4;

    return DITHER_THRESHOLDS[index];
}

*/

float Dithered(float2 uv) {
    uv *= _ScreenParams.xy;
    uv %= 8;
    float A4x4[64] =
    {
        0,32,8,40,2,34,10,42,
        48,16,56,24,50,18,58,26,
        12,44,4,36,14,46,6,38,
        60,28,52,20,62,30,54,22,
        3,35,11,43,1,33,9,41,
        51,19,59,27,49,17,57,25,
        15,47,7,39,13,45,5,37,
        63,31,55,23,61,29,53,21
    };
    return A4x4[uv.x * 8 + uv.y] / 64;

}


/*
float Dithered(float2 pos) {

    float4x4 _RowAccess = { 1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1 };
    pos = pos * _ScreenParams.xy;
    //pos *= _ScreenParams.xy;

    // offset so we're centered
    //pos.x -= _ScreenParams.x / 2;
    //pos.y -= _ScreenParams.y / 2;
    return _RowAccess[floor(fmod(pos.x, 4))][floor(fmod(pos.y, 4))];
}

*/
// Helpers that call the above functions and clip if necessary

void ditherClip(float2 pos, float alpha) {
    clip(alpha - Dithered(pos));
}

/*
float isDithered(float2 pos, float alpha) {

  //  pos *= _ScreenParams.xy;

    // offset so we're centered
  //  pos.x -= _ScreenParams.x / 2;
  //  pos.y -= _ScreenParams.y / 2;

    // ensure that we clip if the alpha is zero by
    // subtracting a small value when alpha == 0, because
    // the clip function only clips when < 0
    return alpha - tex2D(_Dither, pos.xy).a - 0.0001 * (1 - ceil(alpha));
}
*/




#endif