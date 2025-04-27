#version 330 core
out vec4 FragColor;

in vec2 vel;

void main()
{

    vec4 col1 = vec4(0, 0.4, 1, 1);
    vec4 col2 = vec4(0.4, 1, 0.4, 1);
    vec4 col3 = vec4(1, 0.2, 0.2, 1);

    float t = clamp(0, 2, length(vel) / 16.0);


    vec4 inter1 = col1;
    vec4 inter2 = col2;
    if (t > 1) {
        inter1 = col2;
        inter2 = col3;
        t -= 1;
    }



    FragColor = inter1 * (1 - t) + inter2 * t; // white
}
