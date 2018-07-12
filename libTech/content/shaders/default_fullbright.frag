#version 450

uniform sampler2D Texture;
uniform mat4 Model;
uniform mat4 View;

layout (location = 0) in vec4 Color;
layout (location = 1) in vec2 UV;
layout (location = 2) in vec3 ViewPosition;
layout (location = 3) in mat4 NormalMatrix;

out vec4 OutColor;

void main() {
	vec4 TexClr = texture2D(Texture, UV);
	OutColor = Color * TexClr;
}