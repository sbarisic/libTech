#version 410
#extension GL_ARB_shading_language_420pack: enable

layout (location = 0) in vec4 Color;
layout (location = 1) in vec2 UV;
layout (location = 2) in vec3 ViewPosition;
layout (location = 3) in vec3 Pos;

layout (location = 0) out vec4 OutColor;

uniform vec2 Resolution;
layout (binding = 1) uniform sampler2D InColor;

uniform mat4 Model;
uniform mat4 View;
uniform mat4 Project;

uniform float Scale;

uniform float Near;
uniform float Far;

float LinearizeDepth(float Depth)  {
    return (2.0 * Near * Far) / (Far + Near - (Depth * 2.0 - 1.0) * (Far - Near));	
}

void main() {
	vec3 N = normalize(cross(dFdx(ViewPosition), dFdy(ViewPosition)));
	float Fresnel = pow(1.0 - abs(dot(normalize(ViewPosition), N)), 4.0);

	OutColor = vec4(LinearizeDepth(gl_FragCoord.z), Fresnel, 0, 0) * Scale;
}