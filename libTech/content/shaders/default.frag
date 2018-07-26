#version 410

uniform sampler2D Texture;
uniform mat4 Model;
uniform mat4 View;

layout (location = 0) in vec4 Color;
layout (location = 1) in vec2 UV;
layout (location = 2) in vec3 ViewPosition;
layout (location = 3) in mat4 NormalMatrix;

out vec4 OutColor;

vec3 normals(vec3 pos) {
	return normalize(cross(dFdx(pos), dFdy(pos)));
}

void main() {
	vec3 LightDir = normalize(-vec3(50, -100, -50));
	
	mat4 MV = View * Model;
	mat4 NM = inverse(MV);
	
	//vec3 Norm = normals(ViewPosition);
	vec3 Norm = normalize((NM * vec4(normals(ViewPosition), 0)).xyz);
	vec3 ViewPos = normalize((NM * vec4(ViewPosition, 0)).xyz);
	
	float I = max(dot(Norm, LightDir), 0.2);
	if (I > 0) {
		vec3 H = normalize(LightDir + ViewPos);
		float IntSpec = max(dot(H, Norm), 0.0);
		I = I + (I * pow(IntSpec, 50) * 5);
	}
	
	//I = clamp(I, 0.0, 1.0);

	vec4 TexClr = texture2D(Texture, UV);
	OutColor = (Color * TexClr) * vec4(I, I, I, 1);
	//OutColor = vec4(Norm, 1.0);
}
