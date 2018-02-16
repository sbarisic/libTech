#include "libNative.h"
#include "tiny-gizmo.hpp"

#include <stdexcept>

using namespace tinygizmo;
using namespace minalg;

gizmo_application_state gizmo_app_state;
gizmo_context gizmo_ctx;

typedef struct {
public: float X, Y;
} V2;

typedef struct {
public: float X, Y, Z;
} V3;

typedef struct {
public: float X, Y, Z, W;
} Quat, V4;

typedef void(*RenderFunc)(void* Vertices, uint32_t VerticesCount, void* Indices, uint32_t IndicesCount);
RenderFunc Render;

C_EXPORT void GizmoInit(RenderFunc F) {
	Render = F;

	gizmo_ctx.render = [&](const geometry_mesh& R) {
		Render((void*)&R.vertices[0], (uint32_t)R.vertices.size(), (void*)&R.triangles[0], (uint32_t)R.triangles.size() * 3);
	};
}

C_EXPORT void GizmoBegin(V2 ViewSize, float ClipNear, float ClipFar, float YFov, V3 CamPos, Quat CamRot, float ScreenSpaceScale, V3 Snapping) {
	gizmo_app_state.viewport_size = float2(ViewSize.X, ViewSize.Y);
	gizmo_app_state.cam.near_clip = ClipNear;
	gizmo_app_state.cam.far_clip = ClipFar;
	gizmo_app_state.cam.yfov = YFov;

	gizmo_app_state.cam.position = float3(CamPos.X, CamPos.Y, CamPos.Z);
	gizmo_app_state.cam.orientation = float4(CamRot.X, CamRot.Y, CamRot.Z, CamRot.W);
	gizmo_app_state.screenspace_scale = ScreenSpaceScale;
	gizmo_app_state.snap_translation = Snapping.X;
	gizmo_app_state.snap_rotation = Snapping.Y;
	gizmo_app_state.snap_scale = Snapping.Z;

	gizmo_ctx.update(gizmo_app_state);
}

C_EXPORT void GizmoInput(V2 Cursor, bool Left, bool Translate, bool Rotate, bool Scale, bool Local, bool Ctrl) {
	gizmo_app_state.mouse_left = Left;
	gizmo_app_state.hotkey_translate = Translate;
	gizmo_app_state.hotkey_rotate = Rotate;
	gizmo_app_state.hotkey_scale = Scale;
	gizmo_app_state.hotkey_local = Local;
	gizmo_app_state.hotkey_ctrl = Ctrl;
	gizmo_app_state.cursor.x = Cursor.X;
	gizmo_app_state.cursor.y = Cursor.Y;
}

C_EXPORT bool Gizmo(const char* Name, V3* Position, Quat* Rotation, V3* Scale) {
	rigid_transform T;
	T.position = { Position->X, Position->Y, Position->Z };
	T.orientation = { Rotation->X, Rotation->Y, Rotation->Z, Rotation->W };
	T.scale = { Scale->X, Scale->Y, Scale->Z };
	bool Ret = transform_gizmo(std::string(Name), gizmo_ctx, T);

	*Position = V3{ T.position.x, T.position.y, T.position.z };
	*Rotation = Quat{ T.orientation.x, T.orientation.y, T.orientation.z, T.orientation.w };
	*Scale = V3{ T.scale.x, T.scale.y, T.scale.z };
	return Ret;
}

C_EXPORT void GizmoEnd() {
	gizmo_ctx.draw();
}