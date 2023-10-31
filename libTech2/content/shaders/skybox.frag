#version 410
#extension GL_ARB_shading_language_420pack: enable

layout (location = 0) in vec4 Color;
layout (location = 1) in vec2 UV;
layout (location = 2) in vec3 ViewPosition;
layout (location = 3) in vec3 Pos;

layout (location = 0) out vec4 OutColor;

layout (binding = 10) uniform samplerCube Texture;

uniform mat4 Model;
uniform mat4 View;
uniform mat4 Project;

void main() {
	vec3 Fwd = normalize(Pos * vec3(-1, -1, 1));
	vec3 ViewNormal = Fwd;

	OutColor = vec4(texture(Texture, ViewNormal).rgb, 1);
}