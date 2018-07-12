#version 450

uniform samplerCube Texture;
uniform mat4 Model;
uniform mat4 View;
uniform mat4 Project;

layout (location = 0) in vec4 Color;
layout (location = 1) in vec2 UV;
layout (location = 2) in vec3 ViewPosition;
layout (location = 3) in vec3 Pos;

out vec4 OutColor;

void main() {
	vec3 Fwd = normalize(Pos * vec3(-1, -1, 1));
	vec3 ViewNormal = Fwd;

	vec4 TexClr = texture(Texture, ViewNormal);
	OutColor = Color * TexClr;
}