#version 450

uniform sampler2DMS Texture;
uniform mat4 Model;
uniform mat4 View;
uniform vec2 TexSize;
uniform float Exposure;

layout (location = 0) in vec4 Color;
layout (location = 1) in vec2 UV;
layout (location = 2) in vec3 ViewPosition;
layout (location = 3) in mat4 NormalMatrix;

out vec4 OutColor;

void main() {
	vec4 TexClr = texelFetch(Texture, ivec2(int(UV.x * TexSize.x), int(UV.y * TexSize.y)), gl_SampleID);

	TexClr.rgb = vec3(1.0) - exp(-TexClr.rgb * Exposure);
	
	OutColor = Color * TexClr;
}