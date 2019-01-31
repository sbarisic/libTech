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

float rand(float n) {
	return fract(sin(n) * 43758.5453123);
}

float rand(vec2 n) { 
	return fract(sin(dot(n, vec2(12.9898, 4.1414))) * 43758.5453);
}

float noise(float p) {
	float fl = floor(p);
	float fc = fract(p);
	return mix(rand(fl), rand(fl + 1.0), fc);
}
	
float noise(vec2 n) {
	const vec2 d = vec2(0.0, 1.0);
	vec2 b = floor(n), f = smoothstep(vec2(0.0), vec2(1.0), fract(n));
	return mix(mix(rand(b), rand(b + d.yx), f.x), mix(rand(b + d.xy), rand(b + d.yy), f.x), f.y);
}

void main() {
	frag_Clr = Clr;
	frag_UV = UV;
	
	mat4 MV = View * Model;
	//vec3 VertPos = Pos + vec3(0, noise(Pos.xy) * 25, 0);
	vec3 VertPos = Pos;
	
	vec4 ModelPos = MV * vec4(VertPos, 1.0);
	frag_ViewPosition = -ModelPos.xyz;
	
	frag_Pos = VertPos;
	gl_Position = Project * ModelPos;
}