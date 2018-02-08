#version 450

layout (location = 0) in vec3 Pos;
layout (location = 1) in vec3 Clr;

layout (location = 0) out vec3 frag_Color;

out gl_PerVertex {
	vec4 gl_Position;
	float gl_PointSize;
	float gl_ClipDistance[];
};

void main() {
	frag_Color = Clr;

	gl_Position = vec4(Pos, 1.0);
}