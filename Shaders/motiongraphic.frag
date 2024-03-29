// @lsdlive
// CC-BY-NC-SA

// www.moduloprime.com
// Motion Graphics #002

// Checkout this on shadertoy: https://www.shadertoy.com/view/wt3SRl

// With the help of https://thebookofshaders.com/examples/?chapter=motionToolKit
// Inspired by: https://thebookofshaders.com/edit.php?log=160909064609


/*{
    "DESCRIPTION": "Motion Graphics #002",
    "CREDIT": "www.moduloprime.com",
    "CATEGORIES": [
        "Generator"
    ],
    "INPUTS": [
    {
        "NAME": "bpm",
        "LABEL": "BPM",
        "TYPE": "float",
        "DEFAULT": 120,
        "MIN": 0
    },
    {
        "NAME": "speed",
        "LABEL": "Speed",
        "TYPE": "float",
        "DEFAULT": 0.5,
        "MIN": 0
    },
    {
        "NAME": "resync",
        "LABEL": "Resync",
        "TYPE": "float",
        "DEFAULT": 0
    },
    {
        "NAME": "blink_factor",
        "LABEL": "Blink Factor",
        "TYPE": "float",
        "DEFAULT": 0,
        "MIN": 0,
        "MAX": 0.25
    }
    ]
}*/


const float pi = 3.141592654;
const float AA = 3.;

//#define g_time (0.5*(120/60.)*(resync+TIME))
#define g_time TIME - 0.3

// https://lospec.com/palette-list/1bit-monitor-glow
//vec3 col1 = vec3(.133, .137, .137);
//vec3 col2 = vec3(.941, .965, .941);

mat2 r2d(float a) {
    float c = cos(a), s = sin(a);
    return mat2(c, s, -s, c);
}

float fill(float d) {
    return 1. - smoothstep(0., AA / RENDERSIZE.x, d);
}

// inspired by Pixel Spirit Deck: https://patriciogonzalezvivo.github.io/PixelSpiritDeck/
// + https://www.shadertoy.com/view/tsSXRz
float stroke(float d, float width) {
    return 1. - smoothstep(0., AA / RENDERSIZE.x, abs(d) - width * .5);
}

float circle(vec2 p, float radius) {
    return length(p) - radius;
}

float ellipse(vec2 p, vec2 radii) {
    return length(p / radii) - 1.0;
}

float pulse(float begin, float end, float t) {
    return step(begin, t) - step(end, t);
}

float easeInOutExpo(float t) {
    if (t == 0. || t == 1.) {
        return t;
    }
    if ((t *= 2.) < 1.) {
        return .5 * exp2(10. * (t - 1.));
    }
    else {
        return .5 * (-exp2(-10. * (t - 1.)) + 2.);
    }
}

void main() {
    vec2 uv = (gl_FragCoord.xy - .5 * RENDERSIZE.xy) / RENDERSIZE.y;
    uv.x -= 0.01;
    uv.y += 0.013;
    float mask;

    float t1 = fract(g_time); // for blinking rings
    float t2 = easeInOutExpo(fract(g_time));// for easing ring

    // easing ring
    vec2 uv2 = uv * r2d(-pi / 2. * (floor(g_time) + t2));
    if (uv2.x < 0. && uv2.y < 0.)
        mask -= 2. * stroke(circle(uv2, .10), .05);
    mask += stroke(circle(uv, .10), .01);

    // outer rings + central circle
    mask -= fill(circle(uv, .08));
    mask += stroke(ellipse(uv, vec2(.195,.215)), .03);

    mask = clamp(mask, 0., 1.);
    //vec3 col = mix(col1, col2, mask);
    vec3 col = vec3(mask); // black & white mask for VJ tool

    gl_FragColor = vec4(col, mask);
}