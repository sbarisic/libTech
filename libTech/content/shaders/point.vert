#version 450

#define AA 2.0

layout (location = 0) in vec3 Pos;
layout (location = 1) in vec4 Clr;
layout (location = 2) in vec2 UV;

layout (location = 0) out vec4 frag_Color;
layout (location = 1) out float frag_Size;

uniform mat4 Model;
uniform mat4 View;
uniform mat4 Project;

out gl_PerVertex {
	vec4 gl_Position;
	float gl_PointSize;
	float gl_ClipDistance[];
};

void main() {
	frag_Color = Clr * vec4(1, 1, 1, smoothstep(0.0, 1.0, UV.x / AA));
	frag_Size = max(UV.x, AA);
	gl_PointSize = max(UV.x, AA);

	mat4 MVP = Project * View * Model;
	gl_Position = MVP * vec4(Pos, 1.0);
}