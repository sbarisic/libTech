#version 450

layout (location = 0) in vec3 Pos;
layout (location = 1) in vec3 Clr;
layout (location = 2) in vec2 UV;

layout (location = 0) out vec3 frag_Color;
layout (location = 1) out vec2 frag_UV;

out gl_PerVertex {
	vec4 gl_Position;
	float gl_PointSize;
	float gl_ClipDistance[];
};

void main() {
	frag_Color = Clr;
	frag_UV = UV;

	gl_Position = vec4(Pos, 1.0);
}