#version 410

uniform vec2 Viewport;
uniform float Thickness;

uniform mat4 Model;
uniform mat4 View;
uniform mat4 Project;

layout (points) in;
layout (triangle_strip, max_vertices = 4) out;

layout (location = 0) in vec4 geom_Clr[];

layout (location = 0) out vec4 frag_Color;
layout (location = 1) out vec2 frag_CenterOffset;
layout (location = 2) out float frag_ScaledThickness;

void main() {
	float ScaledThickness = Thickness;

	vec2 Pos = gl_in[0].gl_Position.xy / gl_in[0].gl_Position.w;
	vec2 Rad = vec2(ScaledThickness) / Viewport;

	gl_Position = vec4(Pos + Rad * (vec2(-1, -1)), gl_in[0].gl_Position.zw);
	frag_Color = geom_Clr[0];
	frag_CenterOffset = vec2(-1, -1);
	frag_ScaledThickness = ScaledThickness;
	EmitVertex();

	gl_Position = vec4(Pos + Rad * (vec2(-1, 1)), gl_in[0].gl_Position.zw);
	frag_Color = geom_Clr[0];
	frag_CenterOffset = vec2(-1, 1);
	frag_ScaledThickness = ScaledThickness;
	EmitVertex();

	gl_Position = vec4(Pos + Rad * (vec2(1, -1)), gl_in[0].gl_Position.zw);
	frag_Color = geom_Clr[0];
	frag_CenterOffset = vec2(1, -1);
	frag_ScaledThickness = ScaledThickness;
	EmitVertex();

	gl_Position = vec4(Pos + Rad * (vec2(1, 1)), gl_in[0].gl_Position.zw);
	frag_Color = geom_Clr[0];
	frag_CenterOffset = vec2(1, 1);
	frag_ScaledThickness = ScaledThickness;
	EmitVertex();

	EndPrimitive();
}