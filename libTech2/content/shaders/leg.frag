#version 410

layout (location = 0) in vec4 Color;
layout (location = 1) in vec2 UV;
layout (location = 3) in vec3 Pos;
layout (location = 4) in float Dist;

out vec4 OutColor;

void main() {
	OutColor = Color;
}