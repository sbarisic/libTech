#include "libNative.h"
//#include "tiny-gizmo.hpp"
#include "im3d.h"

#include <stdexcept>

//using namespace tinygizmo;
//using namespace minalg;

typedef void(*RenderFunc)(int PrimitiveType, void* Vertices, uint32_t VerticesCount);
RenderFunc Render;
Im3d::Vec3 Snap;

Im3d::AppData& Dta = Im3d::GetAppData();
Im3d::Context& Ctx = Im3d::GetContext();

void DoRender(const Im3d::DrawList& R) {
	Render((int)R.m_primType, (void*)R.m_vertexData, R.m_vertexCount);
}

C_EXPORT void GizmoInit(RenderFunc F) {
	Render = F;
	Dta.drawCallback = &DoRender;
	Dta.m_worldUp = Im3d::Vec3(0, 1, 0);
	Dta.m_projOrtho = false;
}

C_EXPORT void GizmoBegin(float Dt, Im3d::Vec2 ViewSize, float ClipNear, float ClipFar, float YFov, Im3d::Vec3 CamPos, Im3d::Vec3 CamDir, float ScreenSpaceScale, Im3d::Vec3 Snapping) {
	Dta.m_deltaTime = Dt;
	Dta.m_viewportSize = ViewSize;
	Dta.m_viewOrigin = CamPos;
	Dta.m_viewDirection = CamDir;
	Dta.m_projScaleY = tanf(YFov * 0.5f) * 2.0f;
	Dta.m_cursorRayOrigin = Dta.m_viewOrigin;
	Snap = Snapping;

	Im3d::NewFrame();
}

C_EXPORT void GizmoInput(Im3d::Vec3 RayDir, bool Left, bool Translate, bool Rotate, bool Scale, bool Local, bool Ctrl) {
	Dta.m_cursorRayDirection = CST(Im3d::Vec3, RayDir);

	Dta.m_keyDown[Im3d::Mouse_Left] = Left;
	Dta.m_keyDown[Im3d::Key_L] = Local;
	Dta.m_keyDown[Im3d::Key_T] = Translate;
	Dta.m_keyDown[Im3d::Key_R] = Rotate;
	Dta.m_keyDown[Im3d::Key_S] = Scale;

	Dta.m_snapTranslation = Ctrl ? Snap.x : 0;
	Dta.m_snapRotation = Ctrl ? Snap.y : 0;
	Dta.m_snapScale = Ctrl ? Snap.z : 0;
}

C_EXPORT bool Gizmo(const char* Name, float* Position, float* Rotation, float* Scale) {
	return Im3d::Gizmo(Name, Position, Rotation, Scale);
}

C_EXPORT void GizmoEnd() {
	//gizmo_ctx.draw();
	Im3d::Draw();
}