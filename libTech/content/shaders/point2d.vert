#version 410

//#define Thickness 8.0
uniform float Thickness;

layout (location = 0) in vec2 Pos;
layout (location = 1) in vec4 Clr;
layout (location = 2) in vec2 UV;

layout (location = 0) out vec4 geom_Clr;

uniform mat4 Model;
uniform mat4 View;
uniform mat4 Project;

void main() {
	geom_Clr = Clr;

	mat4 MVP = Project * View * Model;
	gl_Position = MVP * vec4(Pos, 0.0, 1.0);
}