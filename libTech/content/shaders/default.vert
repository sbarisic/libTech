#version 450

layout (location = 0) in vec3 Pos;
layout (location = 1) in vec4 Clr;
layout (location = 2) in vec2 UV;

layout (location = 0) out vec4 frag_Color;
layout (location = 1) out vec2 frag_UV;
layout (location = 2) out vec3 frag_ViewPosition;

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
	
	mat4 MV = View * Model;
	
	vec4 ModelPos = MV * vec4(Pos, 1.0);
	frag_ViewPosition = -ModelPos.xyz;
	
	gl_Position = Project * ModelPos;
}