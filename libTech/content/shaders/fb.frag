#version 410

layout (location = 0) in vec4 Clr;
layout (location = 1) in vec2 UV;

uniform sampler2D Texture;
uniform sampler2DMS TextureMS;

uniform float MultisampleCount;
uniform float AlphaTest;
uniform vec2 TextureSize;

layout (location = 0) out vec4 OutClr;

vec4 sampleTex(vec2 uv) {
	int SampleCount = int(MultisampleCount);

	if (SampleCount > 0) {
		vec4 clr = vec4(0.0);

		for (int i = 0; i < SampleCount; i++) {
			clr += texelFetch(TextureMS, ivec2(uv.x * TextureSize.x, uv.y * TextureSize.y), i);
		}

		return clr / float(SampleCount);
	} else {
		return texture2D(Texture, uv);
	}
}

void main() {
	vec4 Fragment = sampleTex(UV);
	OutClr = vec4(Fragment.rgb, 1.0f);
}