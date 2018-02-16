#version 450

#define AA 2.0

uniform vec2 Viewport;

layout (lines) in;
layout (triangle_strip, max_vertices = 4) out;

layout (location = 0) in vec4 geom_Color[];
layout (location = 1) in float geom_Size[];

layout (location = 0) out vec4 frag_Color;
layout (location = 1) out float frag_Size;
layout (location = 2) out float frag_EdgeDistance;

void main() {
	vec2 pos0 = gl_in[0].gl_Position.xy / gl_in[0].gl_Position.w;
	vec2 pos1 = gl_in[1].gl_Position.xy / gl_in[1].gl_Position.w;
		
	vec2 dir = pos0 - pos1;
	dir = normalize(vec2(dir.x, dir.y * Viewport.y / Viewport.x)); // correct for aspect ratio
	vec2 tng0 = vec2(-dir.y, dir.x);
	vec2 tng1 = tng0 * geom_Size[1] / Viewport;
	tng0 = tng0 * geom_Size[0] / Viewport;
		
	// line start
	gl_Position = vec4((pos0 - tng0) * gl_in[0].gl_Position.w, gl_in[0].gl_Position.zw); 
	frag_EdgeDistance = -geom_Size[0];
	frag_Size = geom_Size[0];
	frag_Color = geom_Color[0];
	EmitVertex();
		
	gl_Position = vec4((pos0 + tng0) * gl_in[0].gl_Position.w, gl_in[0].gl_Position.zw);
	frag_Color = geom_Color[0];
	frag_EdgeDistance = geom_Size[0];
	frag_Size = geom_Size[0];
	EmitVertex();
		
	// line end
	gl_Position = vec4((pos1 - tng1) * gl_in[1].gl_Position.w, gl_in[1].gl_Position.zw);
	frag_EdgeDistance = -geom_Size[1];
	frag_Size = geom_Size[1];
	frag_Color = geom_Color[1];
	EmitVertex();
		
	gl_Position = vec4((pos1 + tng1) * gl_in[1].gl_Position.w, gl_in[1].gl_Position.zw);
	frag_Color = geom_Color[1];
	frag_Size = geom_Size[1];
	frag_EdgeDistance = geom_Size[1];
	EmitVertex();
}