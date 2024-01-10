/*{
    "CATEGORIES": [
        "Distortion Effect"
    ],
    "CREDIT": "VIDVOX",
    "DESCRIPTION": "Warps an image to fit in a circle by fitting the height of the image to the height of a circle",
    "INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "DEFAULT": 0.1,
            "MAX": 1.0,
            "MIN": 0.0,
            "NAME": "value1",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.1,
            "MAX": 1.0,
            "MIN": 0.0,
            "NAME": "value2",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.1,
            "MAX": 1.0,
            "MIN": 0.0,
            "NAME": "value3",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.1,
            "MAX": 1.0,
            "MIN": 0.0,
            "NAME": "value4",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.1,
            "MAX": 1.0,
            "MIN": 0.0,
            "NAME": "value5",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.1,
            "MAX": 1.0,
            "MIN": 0.0,
            "NAME": "value6",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.1,
            "MAX": 1.0,
            "MIN": 0.0,
            "NAME": "value7",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.1,
            "MAX": 1.0,
            "MIN": 0.0,
            "NAME": "value8",
            "TYPE": "float"
        }
    ],
    "ISFVSN": "2"
}
*/




float sdEllipse( vec2 p, float value)
{
    vec2 e = vec2(0., 0.0);
    float value2;
    vec2 t = vec2(0.70710678118654752, 0.70710678118654752);
   // if (p.x>= 1.0){
         value2 = value;
    //}
    float dist;
    //if (p.x==-2.0){
        if (value2 >= 0.2) {

            e = 1.0/40.0 + vec2(0.001, 0.07);
            vec2 p1 = vec2(p.x - 0.05, p.y);
            
            vec2 p1Abs = abs(p1);
            vec2 ei = 1.0 / e;
            vec2 e2 = e*e;
            vec2 ve = ei * vec2(e2.x - e2.y, e2.y - e2.x);
            
            t = vec2(0.70710678118654752, 0.70710678118654752);
            for (int i = 0; i < 5; i++) {
                vec2 v = ve*t*t*t;
                vec2 u = normalize(p1Abs - v) * length(t * e - v);
                vec2 w = ei * (v + u);
                t = normalize(clamp(w, 0.0, 1.0));
            }
            
            vec2 nearestAbs1 = t*e;
            float dist1 = length(p1Abs - nearestAbs1);
            
            //dist = length(p1Abs - nearestAbs1);
            //dist = dot(p1Abs, p1Abs) < dot(nearestAbs1, nearestAbs1) ? -dist : dist;
            
            vec2 p2 = vec2(p.x + 0.05, p.y);
            vec2 p2Abs = abs(p2);
            ei = 1.0 / e;
            e2 = e*e;
            ve = ei * vec2(e2.x - e2.y, e2.y - e2.x);
            
            t = vec2(0.70710678118654752, 0.70710678118654752);
            for (int i = 0; i < 5; i++) {
                vec2 v = ve*t*t*t;
                vec2 u = normalize(p2Abs - v) * length(t * e - v);
                vec2 w = ei * (v + u);
                t = normalize(clamp(w, 0.0, 1.0));
            }
            
            vec2 nearestAbs2 = t*e;
            float dist2 = length(p2Abs - nearestAbs2);

            if (dot(p1Abs, p1Abs) < dot(nearestAbs1, nearestAbs1))
            {dist = -dist1;}
            else if (dot(p2Abs,p2Abs) < dot(nearestAbs2, nearestAbs2)){
                dist = -dist2;
            }
            else if (dot(p2Abs,p2Abs) > dot(nearestAbs2, nearestAbs2) && dot(p1Abs, p1Abs) > dot(nearestAbs1, nearestAbs1))
            {dist = dist2;}
        }
        else {
            e = 1.0/30.0 + vec2(0.045, 0.06);
            
            vec2 pAbs = abs(p);
            vec2 ei = 1.0 / e;
            vec2 e2 = e*e;
            vec2 ve = ei * vec2(e2.x - e2.y, e2.y - e2.x);
            
            t = vec2(0.70710678118654752, 0.70710678118654752);
            for (int i = 0; i < 5; i++) {
                vec2 v = ve*t*t*t;
                vec2 u = normalize(pAbs - v) * length(t * e - v);
                vec2 w = ei * (v + u);
                t = normalize(clamp(w, 0.0, 1.0));
            }
            
            vec2 nearestAbs = t * e;
            dist = length(pAbs - nearestAbs);
            dist = dot(pAbs, pAbs) < dot(nearestAbs, nearestAbs) ? -dist : dist;
        }
   // }
    

    return dist;
}

vec3 sdBand(float offset, vec2 uv, float value1, float value2, float value3, float value4, float value5, float value6, float value7, float value8)
{
    float scroll = 0.;
    scroll -= mod(TIME*0.05 + offset,2.2*RENDERSIZE.x/RENDERSIZE.y)-1.1*(RENDERSIZE.x/RENDERSIZE.y);
    
    //scroll += offset;
    float d1 = sdEllipse( vec2(scroll,7.0/8.0)+ uv, value1); // top
    float d2 = sdEllipse( vec2(scroll,5.0/8.0)+ uv, value2);
    float d3 = sdEllipse( vec2(scroll,3.0/8.0)+ uv, value3 );
    float d4 = sdEllipse( vec2(scroll,1.0/8.0)+ uv, value4 );
    float d5 = sdEllipse( vec2(scroll,-1.0/8.0)+ uv, value5 );
    float d6 = sdEllipse( vec2(scroll,-3.0/8.0)+ uv, value6 );
    float d7 = sdEllipse( vec2(scroll,-5.0/8.0)+ uv, value7 );
    float d8 = sdEllipse( vec2(scroll,-7.0/8.0)+ uv, value8 );  // bottom
    
    vec3 col1 = vec3(0.0) - sign(d1)*vec3(1.0,1.0,1.0);
    vec3 col2 = vec3(0.0) - sign(d2)*vec3(1.0,1.0,1.0);
    vec3 col3 = vec3(0.0) - sign(d3)*vec3(1.0,1.0,1.0);
    vec3 col4 = vec3(0.0) - sign(d4)*vec3(1.0,1.0,1.0);
    vec3 col5 = vec3(0.0) - sign(d5)*vec3(1.0,1.0,1.0);
    vec3 col6 = vec3(0.0) - sign(d6)*vec3(1.0,1.0,1.0);
    vec3 col7 = vec3(0.0) - sign(d7)*vec3(1.0,1.0,1.0);
    vec3 col8 = vec3(0.0) - sign(d8)*vec3(1.0,1.0,1.0);
    vec3 col = col1*col2*col3*col4*col5*col6*col7*col8;
    
    return col;
    
}

void main()
{
    //float value1 = 1.1; //threshold values
   // float value2 = 0.1; // values < 0.5 --> circle
   // float value3 = 0.2; // values > 0.5 --> elipse
   // float value4 = 0.7;
   // float value5 = 0.1;
   // float value6 = 0.2;
   // float value7 = 0.8;
//    float value8 = 0.3;
   // float value9 = 0.6;
   // float value10 = 0.5;
   // float value11 = 0.3;
   // float value12 = 0.1;
    
    //float value1 = iMouse.x/iResolution.x;
    vec2 e = vec2(0.5, 0.5);
    
	vec2 uv = 2.0 * gl_FragCoord.xy - RENDERSIZE.xy;
	uv = uv/RENDERSIZE.y;

    // scrolling

    vec3 col1;
    vec3 col2;
    vec3 col3;
    vec3 col4;
    vec3 col5;
    vec3 col6;
    vec3 col7;
         
    
    
    col1 = sdBand(0.0, uv, value1, value2, value3, value4, value5, value6, value7, value8);
    col2 = sdBand((3.0/7.0)*2.2*RENDERSIZE.x/RENDERSIZE.y, uv, value1 * 0.9, value2* 0.9, value3* 0.9, value4* 0.9, value5* 0.9, value6* 0.9, value7* 0.9, value8* 0.9);
    col3 = sdBand((5.0/7.0)*2.2*RENDERSIZE.x/RENDERSIZE.y, uv, value1 * 0.8, value2* 0.8, value3* 0.8, value4* 0.8, value5* 0.8, value6* 0.8, value7* 0.8, value8* 0.8);
    col4 = sdBand((2.0/7.0)*2.2*RENDERSIZE.x/RENDERSIZE.y, uv, value1 * 0.7, value2* 0.7, value3* 0.7, value4* 0.7, value5* 0.7, value6* 0.7, value7* 0.7, value8* 0.7);
    col5 = sdBand((1.0/7.0)*2.2*RENDERSIZE.x/RENDERSIZE.y, uv, value1 * 0.6, value2* 0.6, value3* 0.6, value4* 0.6, value5* 0.6, value6* 0.6, value7* 0.6, value8* 0.6);
    col6 = sdBand((6.0/7.0)*2.2*RENDERSIZE.x/RENDERSIZE.y, uv, value1 * 0.5, value2* 0.5, value3* 0.5, value4* 0.5, value5* 0.5, value6* 0.5, value7* 0.5, value8* 0.5);
    col7 = sdBand((4.0/7.0)*2.2*RENDERSIZE.x/RENDERSIZE.y, uv, value1 * 0.4, value2* 0.4, value3* 0.4, value4* 0.4, value5* 0.4, value6* 0.4, value7* 0.4, value8* 0.4);
    	
	gl_FragColor = vec4( col1* col2 * col3 * col4 * col5 * col6 * col7, 1.0 );
}