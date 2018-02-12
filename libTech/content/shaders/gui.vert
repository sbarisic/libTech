#version 450

layout (location = 0) in vec2 Pos;
layout (location = 1) in vec4 Clr;
layout (location = 2) in vec2 UV;

layout (location = 0) out vec4 frag_Color;
layout (location = 1) out vec2 frag_UV;

uniform mat4 Model;
uniform mat4 View;
uniform mat4 Project;

out gl_PerVertex {
	vec4 gl_Position;
	float gl_PointSize;
	float gl_ClipDistance[];
};

void main() {
	frag_Color = Clr;
	frag_UV = UV;

	mat4 MVP = Project * View * Model;
	gl_Position = MVP * vec4(Pos, 0.0, 1.0);
}