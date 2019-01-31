#version 410

layout (location = 0) in vec4 Clr;
layout (location = 1) in vec2 UV;

layout (location = 0) out vec4 OutClr;

void main() {
	OutClr = vec4(0.25, 0.5, 1, 0.5);
}