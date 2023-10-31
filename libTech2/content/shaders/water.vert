#version 410

layout (location = 0) in vec3 Pos;
layout (location = 1) in vec4 Clr;
layout (location = 2) in vec2 UV;

layout (location = 0) out vec4 frag_Clr;
layout (location = 1) out vec2 frag_UV;
layout (location = 2) out vec3 frag_ViewPosition;
layout (location = 3) out vec3 frag_Pos;

uniform mat4 Model;
uniform mat4 View;
uniform mat4 Project;

void main() {
	frag_Clr = Clr;
	frag_UV = UV;
	
	mat4 MV = View * Model;
	mat4 MVP = Project * View * Model;
	vec4 Pos4 = vec4(Pos, 1.0);
	
	vec4 ModelPos = MV * Pos4;
	frag_ViewPosition = -ModelPos.xyz;
	
	frag_Pos = (Model * Pos4).xyz;
	gl_Position = Project * ModelPos;
}