#version 410

layout (location = 0) in vec2 Pos;
layout (location = 1) in vec4 Clr;
layout (location = 2) in vec2 UV;

layout (location = 0) out vec4 frag_Clr;
layout (location = 1) out vec2 frag_UV;

uniform mat4 Model;
uniform mat4 View;
uniform mat4 Project;

void main() {
	frag_Clr = Clr;
	frag_UV = UV;

	mat4 MVP = Project * View * Model;
	gl_Position = MVP * vec4(Pos, 0.0, 1.0);
}