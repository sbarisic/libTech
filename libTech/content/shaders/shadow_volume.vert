#version 410

layout (location = 0) in vec3 Pos;
layout (location = 0) out vec3 geom_Pos;

void main() {
	geom_Pos = Pos;
}