#version 330 core
layout(location = 0) in vec4 aPosition;

uniform mat4 mat; // Model-View-Projection matrix
out vec2 vel;

void main()
{
    gl_Position = mat * vec4(aPosition.xy, 0.0, 1.0);
    vel = aPosition.zw;
    gl_PointSize = 3 ; // Adjust size as needed
}
