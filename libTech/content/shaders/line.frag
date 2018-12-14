#version 410

//#define Thickness 8.0
uniform float Thickness;

uniform sampler2D Texture;

layout (location = 0) in vec4 Clr;

#define EdgeDistance 0.0
#define Size 1.0

layout (location = 0) out vec4 OutColor;

void main() {
	//float D = smoothstep(1.0, 1.0 - (Thickness / Size), abs(EdgeDistance) / Size);

	//OutColor = Clr * D;
	//OutColor = vec4(1.0, 1.0, 1.0, 1.0);
	OutColor = Clr;
	//OutColor = vec4(1, 1, 1, 1);
}