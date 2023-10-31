#version 410

layout (location = 0) in vec4 Color;
layout (location = 1) in vec2 UV;

out vec4 OutColor;

void main() {
	OutColor = Color;
}