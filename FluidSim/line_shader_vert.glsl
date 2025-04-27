#version 330 core
layout(location = 0) in vec2 aPosition;

uniform mat4 mat; // Model-View-Projection matrix


void main()
{
    gl_Position = mat * vec4(aPosition, 0, 1.0);

    //gl_LineSize = 1 ; // Adjust size as needed
}
