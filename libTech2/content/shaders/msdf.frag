#version 450

uniform sampler2D Texture;
uniform mat4 Model;
uniform mat4 View;

layout (location = 0) in vec4 Color;
layout (location = 1) in vec2 UV;
layout (location = 2) in vec3 ViewPosition;
layout (location = 3) in mat4 NormalMatrix;

out vec4 OutColor;

float median(float r, float g, float b) {
	return max(min(r, g), min(max(r, g), b));
}

void main() {
	vec2 Pos = UV;

	vec2 msdfUnit = 4.0 / vec2(textureSize(Texture, 0));
	vec3 smpl = texture2D(Texture, Pos).rgb;
	float sigDist = median(smpl.r, smpl.g, smpl.b) - 0.5;
	sigDist *= dot(msdfUnit, 0.5 / fwidth(Pos));
	float opct = clamp(sigDist + 0.5, 0.0, 1.0);
	
	//vec4 TexClr = texture2D(Texture, vec2(UV.x, 1 - UV.y));
	//OutColor = TexClr;
	
	OutColor = mix(vec4(0, 0, 0, 0), vec4(1, 1, 1, 1), opct);
}
