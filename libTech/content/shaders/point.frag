#version 450

#define AA 2.0

uniform sampler2D Texture;

layout (location = 0) in vec4 Color;
layout (location = 1) in float Size;

layout (location = 0) out vec4 OutColor;

void main() {
	float D = smoothstep(0.5, 0.5 - (AA / Size), length(gl_PointCoord.xy - vec2(0.5)));
	OutColor = Color * D;
}