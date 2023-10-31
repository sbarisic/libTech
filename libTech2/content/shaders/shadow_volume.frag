#version 410

//layout (location = 0) in vec4 Clr;
//layout (location = 1) in vec2 UV;
//layout (location = 2) in vec3 ViewPosition;
//layout (location = 3) in vec3 Pos;

uniform sampler2D Texture;
uniform float AlphaTest;

layout (location = 0) out vec4 OutClr;
//layout (location = 1) out vec4 OutPos;
//layout (location = 2) out vec4 OutNormal;

void main() {
	//vec4 Fragment = texture(Texture, UV) * Clr;
	//OutClr = Fragment;

	//OutPos = vec4(Pos, 1);

	//vec3 X = dFdx(Pos);
	//vec3 Y = dFdy(Pos);
	//OutNormal = vec4(normalize(cross(X, Y)), 0);

	OutClr = vec4(1, 1, 1, 1);
	//OutPos = vec4(Pos, 1);

	//vec3 X = dFdx(Pos);
	//vec3 Y = dFdy(Pos);
	//OutNormal = vec4(normalize(cross(X, Y)), 0);
}