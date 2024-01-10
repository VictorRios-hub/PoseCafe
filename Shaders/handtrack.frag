#ifdef GL_ES
precision mediump float;
#endif

#define EE 2.718281828459025

#define PI 3.14159265359

uniform sampler2D   u_doubleBuffer0;

uniform vec2 u_resolution;
uniform float u_time;
uniform vec2 u_mouse;

uniform vec2 livepose_posecafefilter_0_WRIST;



// code obtained from Glorious Line Algorithm (https://www.shadertoy.com/view/4tc3DX)
float LineDistField(vec2 uv, vec2 pA, vec2 pB){
    // midpoint
    vec2 mid = (pB + pA) * 0.5;
    // vector from point A to B
    vec2 delta = pB - pA;
    // Distance between endpoints
    float lenD = length(delta);
    // unit vector pointing in the line's direction
    vec2 unit = delta / lenD;
    // Check for when line endpoints are the same
    if (lenD < 0.0001) unit = vec2(1.0, 0.0);	// if pA and pB are same
    // Perpendicular vector to unit - also length 1.0
    vec2 perp = unit.yx * vec2(-1.0, 1.0);
    // position along line from midpoint
    float dpx = dot(unit, uv - mid);
    // distance away from line at a right angle
    float dpy = dot(perp, uv - mid);
    // Make a distance function that is 0 at the transition from black to white
    float disty = abs(dpy);
    float distx = abs(dpx) - lenD * 0.5;

    // Too tired to remember what this does. Something like rounded endpoints for distance function.
    float dist = length(vec2(max(0.0, distx), max(0.0,disty)));
    dist = min(dist, max(distx, disty));


    return dist;
}

void main(){

    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    //st.x *= u_resolution.x/u_resolution.y; // normalize coordinates

    vec2 a = (livepose_posecafefilter_0_WRIST);
    a.y = 1.0 - a.y;


    vec4 textureColor = vec4(0.0, 0.0, 0.0, 1.0);

#ifdef DOUBLE_BUFFER_0

    textureColor = texture2D(u_doubleBuffer0, gl_FragCoord.xy/u_resolution.xy);



    float pta = smoothstep(0.02,0.0, length(st - a)) - smoothstep(0.01,0.0, length(st - a));


    vec3 col_a = vec3(0.2,0.1,0.3);





   	vec3  pixel = vec3(vec2(1.0)/u_resolution.xy,0.);
    //float s1 = texture2D(u_doubleBuffer0, st + (-pixel.zy)).a;    //     s1
    //float s2 = texture2D(u_doubleBuffer0, st + (-pixel.xz)).a;    //  s2 s0 s3
    //float s3 = texture2D(u_doubleBuffer0, st + (pixel.xz)).a;     //     s4
    //float s4 = texture2D(u_doubleBuffer0, st + (pixel.zy)).a;
    
    //textureColor.a = 0.25 * (s1 + s2 + s3 + s4);

    textureColor += vec4(col_a,1.0)*pta*2.0;


    textureColor *= 0.99;
    //d *= 0.99;
    //d *= (u_frame <= 1)? 0.0 : 1.0; // Clean buffer at startup


    //textureColor = clamp(textureColor, vec3(0.0),vec3(0.6));

#else

    textureColor = texture2D(u_doubleBuffer0, gl_FragCoord.xy/u_resolution.xy);

#endif

    gl_FragColor = vec4(textureColor.rgb,1.0);
}