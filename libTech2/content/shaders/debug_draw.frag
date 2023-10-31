#version 410

layout (location = 0) in vec4 Clr;

layout (location = 0) out vec4 OutColor;

void main() {
	OutColor = Clr;
}