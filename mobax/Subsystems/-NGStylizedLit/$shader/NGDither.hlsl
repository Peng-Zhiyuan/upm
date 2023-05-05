#ifndef DITHER_CLIP_INCLUDED
#define DITHER_CLIP_INCLUDED

//sampler2D _Dither;

// Returns > 0 if not clipped, < 0 if clipped based
// on the dither
// For use with the "clip" function
// pos is the fragment position in screen space from [0,1]

float isDithered(float2 pos, float alpha) {
    pos *= _ScreenParams.xy;
  //    pos.x -= _ScreenParams.x / 2;
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

    //int index = (int(pos.x) % 4) * 4 + int(pos.y) % 4;
    int index = (floor(pos.x) % 4) * 4 + floor(pos.y) % 4;
    //int index = floor(fmod(pos.x, 4)) * 4 + floor(fmod(pos.y, 4))
    return  alpha - DITHER_THRESHOLDS[index];
    //DITHER_THRESHOLDS[index];
}

// Helpers that call the above functions and clip if necessary
void ditherClip(float2 pos, float alpha) {
    clip(isDithered(pos, alpha));
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

void ditherClip(float2 pos, float alpha) {
    clip(isDithered(pos, alpha));
}
*/
/*
float isDithered(float2 pos, float alpha) {

    float4x4 _RowAccess = { 1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1 };
    pos = pos * _ScreenParams.xy;
    //pos *= _ScreenParams.xy;

    // offset so we're centered
    //pos.x -= _ScreenParams.x / 2;
    //pos.y -= _ScreenParams.y / 2;
    return alpha - _RowAccess[floor(fmod(pos.x, 4))][floor(fmod(pos.y, 4))];
}

void ditherClip(float2 pos, float alpha) {
    clip(isDithered(pos, alpha));
}

*/
#endif