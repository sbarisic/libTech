#version 410

layout (triangles) in;
layout (triangle_strip, max_vertices = 24) out;
layout (location = 0) in vec3 Pos[];

uniform vec3 LightPosition;
uniform float LightRadius;

uniform mat4 Model;
uniform mat4 View;
uniform mat4 Project;

const float EPSILON = 0.1;
const float ExtendOvershoot = 1.1f;

vec4 Transform(vec3 V) {
	mat4 MVP = Project * View * Model;
	return (MVP * vec4(V, 1.0));
}

void Emit(vec4 P) {	
	gl_Position = P;
	EmitVertex();
}

void Emit2(vec3 P) {
	Emit(Project * vec4(P, 1));
}

void main() {
	mat4 Mat = Project * View * Model;

	vec3 A = Pos[0];
	vec3 B = Pos[1];
	vec3 C = Pos[2];
	vec3 Center = (A + B + C) / 3;

	mat4 MV = View * Model;
	mat4 MVP = Project * MV;

	vec3 WA = (MV * vec4(A, 1)).xyz;
	vec3 WB = (MV * vec4(B, 1)).xyz;
	vec3 WC = (MV * vec4(C, 1)).xyz;
	vec3 WCenter = (MV * vec4(Center, 1)).xyz;
	vec3 LightPos = (View * vec4(LightPosition, 1)).xyz;

	vec3 LightDir = normalize(LightPos - WCenter);
	vec3 ALightDir = normalize(LightPos - WA);
	vec3 BLightDir = normalize(LightPos - WB);
	vec3 CLightDir = normalize(LightPos - WC);

	vec3 Normal = normalize(cross(WB - WA, WC - WA));
	float L = LightRadius * ExtendOvershoot;

	float AtoL = length(WA - LightPos);
	float BtoL = length(WB - LightPos);
	float CtoL = length(WC - LightPos);

	WA = WA - (ALightDir * EPSILON);
	WB = WB - (BLightDir * EPSILON);
	WC = WC - (CLightDir * EPSILON);

	vec3 WA_ = WA - (ALightDir * (L - AtoL));
	vec3 WB_ = WB - (BLightDir * (L - BtoL));
	vec3 WC_ = WC - (CLightDir * (L - CtoL));

	if (dot(Normal, LightDir) < 0) {
		// Front face
		Emit2(WA);
		Emit2(WB);
		Emit2(WC);
		EndPrimitive();

		Emit2(WC);
		Emit2(WB);
		Emit2(WC_);
		EndPrimitive();

		Emit2(WB);
		Emit2(WB_);
		Emit2(WC_);
		EndPrimitive();

		Emit2(WC);
		Emit2(WC_);
		Emit2(WA_);
		EndPrimitive();

		Emit2(WA);
		Emit2(WC);
		Emit2(WA_);
		EndPrimitive();

		Emit2(WB);
		Emit2(WA);
		Emit2(WB_);
		EndPrimitive();

		Emit2(WB_);
		Emit2(WA);
		Emit2(WA_);
		EndPrimitive();

		// Back face
		Emit2(WA_);
		Emit2(WC_);
		Emit2(WB_);
		EndPrimitive();
	}
}