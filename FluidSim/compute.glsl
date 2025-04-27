#version 430 core
#define G 9.81
#define PI 3.141592


layout(local_size_x = 128) in;

layout(std430, binding = 0) buffer PointsBuffer
{
    vec4 points[];
};


uniform mat4 mat;
uniform int pointcount;
uniform float dt;
uniform vec2 bounds; // Mirrors on all axis to get all bounding points
uniform vec2 mousePos;
uniform int isPulling;

float radius = 0.5;
float targetDensity = 0.8;
float pressureMultiplier = 20; // Maybe stolen from Sebastian lague
float viscosityConstant = 0;


void CollideWithBounds(uint index){
    vec2 p = points[index].xy;
    vec2 vel = points[index].zw;

    if( abs(p.x) > bounds.x){
        vel *= vec2(-0.6, 1);
        p.x = clamp(-bounds.x + 0.0001, bounds.x - 0.0001, p.x);
    }
    if( abs(p.y) > bounds.y){
        vel *= vec2(1, -0.6);
        p.y = clamp(-bounds.y + 0.0001, bounds.y - 0.0001, p.y);
    }

    points[index] = vec4(p, vel);
}

float densityKernel(float dist){
    if (dist >= radius){return 0;}
    return 10 / (PI * radius*radius) * pow(-dist / radius + 1, 3);
}

float densityDerivative(float dist){
    if (dist >= radius){return 0;}
    return -10 / (PI * radius*radius*radius) * 3 * pow(-dist / radius + 1, 2);
}

vec2 calculatePressureForce(uint index){
    
    float[100000] densities; // Max is 100_000
    float dist;
    vec2 dir;

    for(int i=0; i<pointcount; i++){
        if (index == i){continue;}
        
        dir = points[index].xy - points[i].xy;
        dist = length(dir);

        if (dist == 0){continue;}
        
        densities[i] = densityKernel(dist);

    }

    float densityerror;

    vec2 pressureForce = vec2(0,0);
    for(int i=0; i<pointcount; i++){
        
        if (index == i){continue;}

        dir = points[index].xy - points[i].xy;
        dist = length(dir);


        if (dist == 0){continue;}


        densityerror = targetDensity - (densities[i] + densities[index]) / 2;

        pressureForce += densityerror * densityDerivative(dist) * normalize(dir); 
        
    }
    return pressureMultiplier * pressureForce;
}

vec2 calculateViscosityDamp(uint index){
    vec2 viscosity = vec2(0,0);
    
    float c;
    float dist;
    vec2 dir;
    for(int i=0; i<pointcount; i++){
        if(i == index){continue;}

        c = dot(points[index].zw, points[i].zw);

        if(abs(c) < 0.00000001){continue;}

        dir = points[index].xy - points[i].xy; 
        dist = length(dir);

        if(dist == 0){continue;}


        viscosity -= (1 - c / length(points[index].zw) / length(points[i].zw)) / 2  * densityKernel(dist) * normalize(dir);
    }
    return viscosityConstant * viscosity;
    //return exp(- viscosityConstant * viscosity / pointcount);
}

void main()
{
    uint index = gl_GlobalInvocationID.x;
    if (index >= pointcount){ return; }



    float itercount = 20;

    vec2 pressureForce;
    vec2 viscosityDamp = vec2(0,0);
    vec2 externalForce = vec2(0,0);


    vec2 mouse = (inverse(mat) * vec4(mousePos, 0, 1)).xy;

    for(int iter=0; iter<itercount; iter++){
        points[index] +=  dt /itercount * vec4(points[index].zw, 0, 0);

        pressureForce = calculatePressureForce(index);
        //viscosityDamp = calculateViscosityDamp(index);
        
        externalForce = vec2(0,0);
        if(isPulling != 0){
            //points[index].y = mouse.y;
            externalForce = (points[index].xy - mouse);
            if (length(externalForce) < 1){externalForce = -isPulling * 200* normalize(externalForce);}
            else{externalForce = vec2(0,0);}
        }
        //externalForce = normalize(externalForce) / dot(externalForce, externalForce);

        points[index] -= dt /itercount * vec4(points[index].zw, 0, 0);

        points[index].zw += dt / itercount * (viscosityDamp + pressureForce + externalForce);
        

        
        points[index].w -=  2 *G * dt / itercount;
        points[index] += dt /itercount * vec4(points[index].zw, 0, 0);
    }




    
    CollideWithBounds(index);

}
