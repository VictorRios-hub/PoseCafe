/*
{
  "CATEGORIES" : [
    "Automatically Converted"
  ],
  "INPUTS" : [
	{
			"NAME": "duration",
			"TYPE": "float",
			"DEFAULT": 15.0,
			"MIN": 10.0,
			"MAX": 30.0
	}
  ],
  "DESCRIPTION" : "Automatically converted from http:\/\/glslsandbox.com\/e#34992.0"
}
*/


#ifdef GL_ES
precision mediump float;
#endif

#extension GL_OES_standard_derivatives : enable


vec4 color = vec4(1, 0.65, 0.5, 1);

void render(float a) {
    float alpha = 0.0;
    if (clamp(0.1 - a, 0, 1) != 0.0) {
        alpha = 1.0;
    }
    gl_FragColor = vec4(color.xyz * clamp(1.0 - a, 0.0, 1.0), alpha);
}

#define PI acos(-1.)

// const float duration = 20.0; // define the duration for one loop
float speedOpen = 2*PI/duration;

void main( void ) {

    float temps = duration -0.5 - mod(floor(TIME) /1.0, duration*2);
    if (temps < 0.0){ color = vec4(0, 0, 0, 1);}
    
	vec2 pos = vec2(gl_FragCoord.x, RENDERSIZE.y - gl_FragCoord.y);
	
    vec2 center = vec2(RENDERSIZE.x - 60.0, 60.0);
	
	vec2 dir = center - pos;
	float angle = atan(dir.x, -dir.y) + PI;
	
	if (angle > mod(TIME * speedOpen, PI * 4.) - PI * 2. && angle < mod(TIME * speedOpen, PI * 4.))
	render(length(pos - center) - min(RENDERSIZE.x, RENDERSIZE.y) / 3.2 + 330.);
}