#version 450

layout(set = 0, binding = 0) uniform ProjectionBuffer
{
    mat4 Projection;
};
layout(set = 0, binding = 1) uniform ViewBuffer
{
    mat4 View;
};
layout(set = 1, binding = 0) uniform WorldBuffer
{
    mat4 World;
};


layout(location = 0) in vec3 Position;

void main()
{
    vec4 worldPos = World * vec4(Position, 1);
    vec4 viewPos = View * worldPos;
    vec4 clipPos = Projection * viewPos;
    gl_Position = clipPos;
}