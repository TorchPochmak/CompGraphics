#version 330 core

out vec4 FragColor;

in vec3 texCoord;

uniform samplerCube texture0;

void main()
{
    FragColor = texture(texture0, texCoord);
}
