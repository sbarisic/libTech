#version 450

uniform sampler2D Texture;

layout (location = 0) in vec3 Color;
layout (location = 1) in vec2 UV;

out vec4 OutColor;

void main() {
	vec4 TexClr = texture2D(Texture, UV);
	OutColor = vec4(Color * TexClr.rgb, TexClr.a);
}