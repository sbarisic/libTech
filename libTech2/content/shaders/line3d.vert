#version 410

#define AA 3.0

layout (location = 0) in vec3 Pos;
layout (location = 1) in vec4 Clr;
layout (location = 2) in vec2 UV;

layout (location = 0) out vec4 geom_Color;
layout (location = 1) out float geom_Size;

uniform mat4 Model;
uniform mat4 View;
uniform mat4 Project;

void main() {
	geom_Color = Clr * vec4(1, 1, 1, smoothstep(0.0, 1.0, UV.x / AA));
	geom_Size = max(UV.x, AA);

	mat4 MVP = Project * View * Model;
	gl_Position = MVP * vec4(Pos, 1.0);
}