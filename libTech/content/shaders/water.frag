#version 410
#extension GL_ARB_shading_language_420pack: enable

layout (location = 0) in vec4 Clr;
layout (location = 1) in vec2 UV;
layout (location = 2) in vec3 ViewPosition;
layout (location = 3) in vec3 Pos;

layout (location = 0) out vec4 OutClr;

layout (binding = 10) uniform samplerCube SkyboxTexture;
uniform vec3 ViewPos;

uniform mat4 Model;
uniform mat4 View;
uniform mat4 Project;

void main() {
	vec3 ReflectionNormal = normalize(Pos - ViewPos) * vec3(-1, 1, 1);
	vec4 SkyboxSample = texture(SkyboxTexture, ReflectionNormal);

	vec3 X = dFdx(Pos);
	vec3 Y = dFdy(Pos);
	vec3 WaterNormal = normalize(cross(X, Y));

	vec3 ViewNormal = normalize(Pos - ViewPos);


	float ReflectionRatio = clamp((dot(WaterNormal, ReflectionNormal) + 1), 0, 1);

	//if (DotProd > 0)
	//	OutClr = SkyboxSample * vec4(1, 1, 1, DotProd);
	//else
	//	OutClr = vec4(1, 1, 1, 0.2);

	OutClr = mix(SkyboxSample * vec4(vec3(1), 0.05), SkyboxSample * vec4(vec3(1), 0.6), ReflectionRatio);
}