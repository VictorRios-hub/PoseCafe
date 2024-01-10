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

            "DEFAULT": 0.5,

            "MAX": 1.0,

            "MIN": 0,

            "NAME": "handY",

            "TYPE": "float"

        },

        {

            "DEFAULT": 0,

            "MAX": 1,

            "MIN": 0,

            "NAME": "handX",

            "TYPE": "float"

        },
        {

            "DEFAULT": 0,

            "MAX": 1,

            "MIN": 0,

            "NAME": "STOP",

            "TYPE": "float"

        }

    ],

	"ISFVSN": "2.0",

    "PASSES": [

    {

        "TARGET": "bufferVariableNameA",

        "PERSISTENT": true,
        "FLOAT": true

    },

    {

        "TARGET": "milkA",

        "PERSISTENT": true,
        "FLOAT": true

    },

    {

        "TARGET":"bufferVariableNameB",

        "PERSISTENT": true,
        "FLOAT": true

    },

    {

        "TARGET": "milkB",

        "PERSISTENT": true,
        "FLOAT": true

    },

    {
        "FLOAT": true
        

    }

    ]

    

}

*/


#define k 25.0
#define l 15.0
#define s 0.25
#define nu 0.5
#define mu 0.5 
#define kappa 0.5


vec2 R = RENDERSIZE.xy;
vec2 Mouse = vec2(1.0 - handX,1.0 - handY)*R;
float t = TIME/4.0;

void main()	{

    if (STOP == 1){
        Mouse = vec2(0,0);
    }
    
    vec2 U = gl_FragCoord.xy*R/RENDERSIZE;
    vec4 Q = vec4(1.0,1.0,1.0,1.0);
    //float milk = 0.0; 

	if (PASSINDEX == 0){

        float radius = length((U/R - vec2(0.5))*vec2(1.0, R.y / R.x));
        float g = 10.0*exp(-t/5.0);
        //g = 10.0;
        float dt = clamp(0.3/(2.0*t),0.00,0.05);
        //dt = 0.0001;

        vec4 a = IMG_NORM_PIXEL(bufferVariableNameB, (U+vec2(1,0))/R);
        vec4 b = IMG_NORM_PIXEL(bufferVariableNameB, (U+vec2(0,1))/R);
        vec4 c = IMG_NORM_PIXEL(bufferVariableNameB, (U+vec2(-1,0))/R);
        vec4 d = IMG_NORM_PIXEL(bufferVariableNameB, (U+vec2(0,-1))/R);
        vec4 e=0.25*(a+b+c+d);
        vec4 dx=a-c;
        vec4 dy=b-d;

        vec2 vu = IMG_NORM_PIXEL(bufferVariableNameB, (U-dt*e.xy*0.5)/R).xy;//advection vector estimate
        Q = IMG_NORM_PIXEL(bufferVariableNameB, (U-dt*vu)/R);//advection

	    float a_milk = IMG_NORM_PIXEL(milkB, (U+vec2(1,0))/R).x;
	    float b_milk = IMG_NORM_PIXEL(milkB, (U+vec2(0,1))/R).x;
	    float c_milk = IMG_NORM_PIXEL(milkB, (U+vec2(-1,0))/R).x;
	    float d_milk = IMG_NORM_PIXEL(milkB, (U+vec2(0,-1))/R).x;
	    float e_milk = 0.25 *(a_milk+b_milk+c_milk+d_milk);
	    float dx_milk = a_milk-c_milk;
	    float dy_milk = b_milk-d_milk;

        

        Q.w = IMG_NORM_PIXEL(milkB, (U-dt*vu)/R).x; // advection for the milk
        e.w = e_milk; // milk gradient
        
     	vec2 gp = vec2(dx.z,dy.z);//pressure gradient
        vec2 gw = vec2(dx_milk,dy_milk);//density gradient
        float div = (dx.x+dy.y);//divergence
        vec2 vdv = vec2(Q.x*dx.x+Q.y*dy.x,Q.x*dx.y+Q.y*dy.y); 

        Q.xy -= dt * (k*gp + Q.w*l*gw + Q.w*vec2(0.0,g)+ s*vdv*Q.w);//forces
        Q.z = e.z - 0.025*div;//pressure
        Q -=  dt * vec4(nu,nu,mu,kappa) * 4.0*(Q-e);//diffusion
    
     	if (radius < 0.39)
            {if (length(U-Mouse.xy) < 10.0) 
                {Q.xy= vec2(0.1,0.1);}
            }

        if (0.39 < radius)
        { 
            if (radius < 0.4){Q.x = max(Q.x - 0.2, 0.0); Q.y = max(Q.y - 0.2, 0.0);}
            if (radius > 0.4) {Q.xy = vec2(0.0);}
            }
            
        //Boundaries
        if (t < 0.01) {Q = vec4(0,0,1.,0);}

        Q.w = 1.0; // need to update the milk amounts in another pass :(
        Q = clamp(Q, vec4(-45.0,-45.0,0.0,0.0), vec4(45.0,45.0,45.0,45.0));
        gl_FragColor = Q;    
	}

	else if (PASSINDEX == 1){

	    float radius = length((U/R - vec2(0.5))*vec2(1.0, R.y / R.x));
	    float dt = clamp(0.3/(2.0*t),0.00,0.05);
	    
	    float a_milk = IMG_NORM_PIXEL(milkB, (U+vec2(1,0))/R).x;
	    float b_milk = IMG_NORM_PIXEL(milkB, (U+vec2(0,1))/R).x;
	    float c_milk = IMG_NORM_PIXEL(milkB, (U+vec2(-1,0))/R).x;
	    float d_milk = IMG_NORM_PIXEL(milkB, (U+vec2(0,-1))/R).x;
	    float e_milk = 0.25 *(a_milk+b_milk+c_milk+d_milk);
	    float dx_milk = a_milk-c_milk;
	    float dy_milk = b_milk-d_milk;
	    
        vec4 a = IMG_NORM_PIXEL(bufferVariableNameB, (U+vec2(1,0))/R);
        vec4 b = IMG_NORM_PIXEL(bufferVariableNameB, (U+vec2(0,1))/R);
        vec4 c = IMG_NORM_PIXEL(bufferVariableNameB, (U+vec2(-1,0))/R);
        vec4 d = IMG_NORM_PIXEL(bufferVariableNameB, (U+vec2(0,-1))/R);
        vec4 e=0.25*(a+b+c+d);
        vec4 dx=a-c;
        vec4 dy=b-d;

        vec2 vu = IMG_NORM_PIXEL(bufferVariableNameB, (U-dt*e.xy*0.5)/R).xy;//advection vector estimate
        float moving_milk = IMG_NORM_PIXEL(milkB, (U-dt*vu)/R).x; // advection for the milk
        moving_milk -= dt * kappa * 4.0*(moving_milk-e_milk);//diffusion

        //Boundaries
        if (t < 0.01) {moving_milk = 0.0;}

     	if (radius < 0.39)
            {if (length(U-Mouse.xy) < 10.0) 
                {moving_milk = 1.0;}
            }
            
        moving_milk = clamp(moving_milk, 0.0, 45.0);
        gl_FragColor = vec4(moving_milk, 1.0, 1.0, 1.0);
	}

	else if (PASSINDEX == 2){

        float radius = length((U/R - vec2(0.5))*vec2(1.0, R.y / R.x));
        float g = 10.0*exp(-t/5.0);
        //g = 10.0;
        float dt = clamp(0.3/(2.0*t),0.00,0.05);
        //dt = 0.0001;

        vec4 a = IMG_NORM_PIXEL(bufferVariableNameA, (U+vec2(1,0))/R);
        vec4 b = IMG_NORM_PIXEL(bufferVariableNameA, (U+vec2(0,1))/R);
        vec4 c = IMG_NORM_PIXEL(bufferVariableNameA, (U+vec2(-1,0))/R);
        vec4 d = IMG_NORM_PIXEL(bufferVariableNameA, (U+vec2(0,-1))/R);
        vec4 e=0.25*(a+b+c+d);
        vec4 dx=a-c;
        vec4 dy=b-d;

        vec2 vu = IMG_NORM_PIXEL(bufferVariableNameA, (U-dt*e.xy*0.5)/R).xy;//advection vector estimate
        Q = IMG_NORM_PIXEL(bufferVariableNameA, (U-dt*vu)/R);//advection

	    float a_milk = IMG_NORM_PIXEL(milkA, (U+vec2(1,0))/R).x;
	    float b_milk = IMG_NORM_PIXEL(milkA, (U+vec2(0,1))/R).x;
	    float c_milk = IMG_NORM_PIXEL(milkA, (U+vec2(-1,0))/R).x;
	    float d_milk = IMG_NORM_PIXEL(milkA, (U+vec2(0,-1))/R).x;
	    float e_milk = 0.25 *(a_milk+b_milk+c_milk+d_milk);
	    float dx_milk = a_milk-c_milk;
	    float dy_milk = b_milk-d_milk;

        

        Q.w = IMG_NORM_PIXEL(milkA, (U-dt*vu)/R).x; // advection for the milk
        e.w = e_milk; // milk gradient
        
     	vec2 gp = vec2(dx.z,dy.z);//pressure gradient
        vec2 gw = vec2(dx_milk,dy_milk);//density gradient
        float div = (dx.x+dy.y);//divergence
        vec2 vdv = vec2(Q.x*dx.x+Q.y*dy.x,Q.x*dx.y+Q.y*dy.y); 

        Q.xy -= dt * (k*gp + Q.w*l*gw + Q.w*vec2(0.0,g)+ s*vdv*Q.w);//forces
        Q.z = e.z - 0.025*div;//pressure
        Q -=  dt * vec4(nu,nu,mu,kappa) * 4.0*(Q-e);//diffusion
    
     	if (radius < 0.39)
            {if (length(U-Mouse.xy) < 10.0) 
                {Q.xy= vec2(0.1,0.1);}
            }

        if (0.39 < radius)
        { 
            if (radius < 0.4){Q.x = max(Q.x - 0.2, 0.0); Q.y = max(Q.y - 0.2, 0.0);}
            if (radius > 0.4) {Q.xy = vec2(0.0);}
            }
            
        //Boundaries
        if (t < 0.01) {Q = vec4(0,0,1.,0);}

        Q.w = 1.0; // need to update the milk amounts in another pass :(
        Q = clamp(Q, vec4(-45.0,-45.0,0.0,0.0), vec4(45.0,45.0,45.0,45.0));
        gl_FragColor = Q;    
	}
	else if (PASSINDEX == 3){

	    float radius = length((U/R - vec2(0.5))*vec2(1.0 , R.y / R.x));
	    float dt = clamp(0.3/(2.0*t),0.0,0.05);
	    
	    float a_milk = IMG_NORM_PIXEL(milkA, (U+vec2(1,0))/R).x;
	    float b_milk = IMG_NORM_PIXEL(milkA, (U+vec2(0,1))/R).x;
	    float c_milk = IMG_NORM_PIXEL(milkA, (U+vec2(-1,0))/R).x;
	    float d_milk = IMG_NORM_PIXEL(milkA, (U+vec2(0,-1))/R).x;
	    float e_milk = 0.25 *(a_milk+b_milk+c_milk+d_milk);
	    float dx_milk = a_milk-c_milk;
	    float dy_milk = b_milk-d_milk;
	    
        vec4 a = IMG_NORM_PIXEL(bufferVariableNameA, (U+vec2(1,0))/R);
        vec4 b = IMG_NORM_PIXEL(bufferVariableNameA, (U+vec2(0,1))/R);
        vec4 c = IMG_NORM_PIXEL(bufferVariableNameA, (U+vec2(-1,0))/R);
        vec4 d = IMG_NORM_PIXEL(bufferVariableNameA, (U+vec2(0,-1))/R);
        vec4 e=0.25*(a+b+c+d);
        vec4 dx=a-c;
        vec4 dy=b-d;

        vec2 vu = IMG_NORM_PIXEL(bufferVariableNameA, (U-dt*e.xy*0.5)/R).xy;//advection vector estimate
        float moving_milk = IMG_NORM_PIXEL(milkA, (U-dt*vu)/R).x; // advection for the milk
        moving_milk -= dt * kappa * 4.0*(moving_milk-e_milk);//diffusion

        //Boundaries
        if (t < 0.01) {moving_milk = 0.0;}

     	if (radius < 0.39)
            {if (length(U-Mouse.xy) < 10.0) 
                {moving_milk = 1.0;}
            }
            
        //moving_milk = clamp(moving_milk, 0.0, 1.0);
        moving_milk = clamp(moving_milk, 0.0, 45.0);
        gl_FragColor = vec4(moving_milk, 1.0, 1.0, 1.0);
	}   

    else if (PASSINDEX == 4){
        float m = 3.0*IMG_NORM_PIXEL(milkB, U/R).x; //masse
        //m = 0.3;
        Q.xyz = m*Q.xyz;
        
        float circ_sdf = smoothstep(0.41, 0.4, length((U/R - vec2(0.5))*vec2(1.0, R.y / R.x)));
        vec3 brown = vec3(0.75, 0.45, 0.2);

        Q.xyz += circ_sdf*brown;
        gl_FragColor = Q;
    }
}
