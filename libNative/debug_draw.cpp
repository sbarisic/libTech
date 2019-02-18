#define DEBUG_DRAW_IMPLEMENTATION
#include "debug_draw.hpp"

#include "libNative.h"

using namespace dd;

typedef void(*DrawLineFunc)(DrawVertex Start, DrawVertex End, bool depthEnabled);
DrawLineFunc DrawLineFnc;

class libTechRenderInterface final : public RenderInterface {
public:
	~libTechRenderInterface() {
	}

	void beginDraw() {

	}

	void endDraw() {

	}

	void drawLineList(const DrawVertex* lines, int count, bool depthEnabled) {
		for (size_t i = 0; i < count; i += 2)
			DrawLineFnc(lines[i], lines[i + 1], depthEnabled);
	}
};

libTechRenderInterface* libRender;

C_EXPORT void debug_draw_init(DrawLineFunc DrawLine) {
	DrawLineFnc = DrawLine;

	libRender = new libTechRenderInterface();
	initialize(libRender);
}

C_EXPORT void debug_draw_flush(int64_t TimeMS) {
	flush(TimeMS);
}

// Drawing functions

C_EXPORT void debug_draw_axis_triad(ddMat4x4_In Mat, float Size, float Length, bool DepthEnabled, int Time) {
	axisTriad(Mat, Size, Length, Time, DepthEnabled);
}

C_EXPORT void debug_draw_aabb(ddVec3_In Mins, ddVec3_In Maxs, ddVec3_In Color, bool DepthEnabled, int Time) {
	aabb(Mins, Maxs, Color, Time, DepthEnabled);
}

C_EXPORT void debug_draw_cross(ddVec3_In Pos, float Length, bool DepthEnabled, int Time) {
	cross(Pos, Length, Time, DepthEnabled);
}

C_EXPORT void debug_draw_sphere(ddVec3_In Pos, ddVec3_In Color, float Radius, bool DepthEnabled, int Time) {
	sphere(Pos, Color, Radius, Time, DepthEnabled);
}

C_EXPORT void debug_draw_arrow(ddVec3_In From, ddVec3_In To, ddVec3_In Color, float Size, bool DepthEnabled, int Time) {
	arrow(From, To, Color, Size, Time, DepthEnabled);
}

C_EXPORT void debug_draw_line(ddVec3_In From, ddVec3_In To, ddVec3_In Color, bool DepthEnabled, int Time) {
	line(From, To, Color, Time, DepthEnabled);
}

C_EXPORT void debug_draw_circle(ddVec3_In Center, ddVec3_In Normal, ddVec3_In Color, float Radius, int Steps, bool DepthEnabled, int Time) {
	circle(Center, Normal, Color, Radius, Steps, Time, DepthEnabled);
}

C_EXPORT void debug_draw_frustum(ddMat4x4_In invClipMatrix, ddVec3_In Color, bool DepthEnabled, int Time) {
	frustum(invClipMatrix, Color, Time, DepthEnabled);
}