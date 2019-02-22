#version 410
#extension GL_ARB_shading_language_420pack: enable

layout (location = 0) in vec4 Color;
layout (location = 1) in vec2 UV;
layout (location = 2) in vec3 ViewPosition;
layout (location = 3) in vec3 Pos;

layout (location = 0) out vec4 OutColor;

uniform sampler2D Texture;
uniform vec2 Resolution;

uniform mat4 Model;
uniform mat4 View;
uniform mat4 Project;

uniform float Near;
uniform float Far;

float LinearizeDepth(float depth) 
{
    float z = depth * 2.0 - 1.0; // back to NDC 
    return (2.0 * Near * Far) / (Far + Near - z * (Far - Near));	
}


void main() {
	vec4 DepthTex = texture2D(Texture, gl_FragCoord.xy / Resolution);

	float Z = -abs(DepthTex.x);
	float Fresnel = clamp(DepthTex.y, 0, 1);
	Fresnel = Fresnel / 1.2;

	float DepthFactor = clamp(1 - (Z / -800), 0, 1); // 500
	//OutColor = vec4(mix(vec3(0, 0, 0), vec3(1, 1, 1), DepthFactor * Fresnel), max(max(DepthFactor * Fresnel, 1 - DepthFactor), 0.1));

	OutColor = vec4(vec3(1, 1, 1), 1 - DepthFactor);
}