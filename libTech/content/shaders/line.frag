#version 450

#define AA 3.0

uniform sampler2D Texture;

layout (location = 0) in vec4 Color;
layout (location = 1) in float Size;
layout (location = 2) in float EdgeDistance;

layout (location = 0) out vec4 OutColor;

void main() {
	float D = smoothstep(1.0, 1.0 - (AA / Size), abs(EdgeDistance) / Size);
	OutColor = Color * D;
}