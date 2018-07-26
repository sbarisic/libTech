#version 410

uniform vec2 Viewport;
//uniform float Thickness;

uniform sampler2D Texture;

layout (location = 0) in vec4 Clr;
layout (location = 1) in vec2 CenterOffset;
layout (location = 2) in float Thickness;

#define PixelsAlpha 4
#define MinAlphaSize 8
#define AlphaAbove 1.5

layout (location = 0) out vec4 OutColor;

void main() {
	vec2 CenterOffsetSq = CenterOffset * CenterOffset;
	float Dist = CenterOffsetSq.x + CenterOffsetSq.y;
	float Alpha = mix(mix(1.0, 1.0 - step(1.0, Dist), step(AlphaAbove, Thickness)), 1.0 - smoothstep(1.0 - ((1.0 / Thickness) * PixelsAlpha), 1.0, Dist), step(MinAlphaSize, Thickness));

	OutColor = vec4(Clr.rgb, min(Clr.a, Alpha));
}