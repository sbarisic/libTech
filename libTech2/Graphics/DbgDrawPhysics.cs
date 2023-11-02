using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Graphics {
	class DbgDrawPhysics  {
		/*public DebugDrawModes DebugMode {
			get {
				return DebugDrawModes.DrawWireframe | DebugDrawModes.DrawContactPoints;
			}

			set => throw new NotImplementedException();
		}*/

		public void Draw3dText(ref Vector3 location, string textString) {
			throw new NotImplementedException();
		}

		public void DrawAabb(ref Vector3 from, ref Vector3 to, Color color) {
			throw new NotImplementedException();
		}

		public void DrawArc(ref Vector3 center, ref Vector3 normal, ref Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, Color color, bool drawSect) {
			throw new NotImplementedException();
		}

		public void DrawArc(ref Vector3 center, ref Vector3 normal, ref Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, Color color, bool drawSect, float stepDegrees) {
			throw new NotImplementedException();
		}

		public void DrawBox(ref Vector3 bbMin, ref Vector3 bbMax, ref Matrix4x4 trans, Color color) {
			throw new NotImplementedException();
		}

		public void DrawBox(ref Vector3 bbMin, ref Vector3 bbMax, Color color) {
			throw new NotImplementedException();
		}

		public void DrawCapsule(float radius, float halfHeight, int upAxis, ref Matrix4x4 transform, Color color) {
			DrawCylinder(radius, halfHeight, upAxis, ref transform, color);
		}

		public void DrawCone(float radius, float height, int upAxis, ref Matrix4x4 transform, Color color) {
			throw new NotImplementedException();
		}

		public void DrawContactPoint(ref Vector3 pointOnB, ref Vector3 normalOnB, float distance, int lifeTime, Color color) {
			DbgDraw.DrawCircle(pointOnB, normalOnB, color, 6);

			// throw new NotImplementedException();
		}

		public void DrawCylinder(float radius, float halfHeight, int upAxis, ref Matrix4x4 transform, Color color) {
			Matrix4x4.Decompose(transform, out Vector3 Scale, out Quaternion Rot, out Vector3 Pos);
			Vector3 axis = new Vector3(0, 0, 1);

			if (upAxis == 0)
				axis = new Vector3(1, 0, 0);
			else if (upAxis == 1)
				axis = new Vector3(0, 1, 0);

			//DbgDraw.DrawArrow(Pos, Pos + axis * halfHeight, color, 10);

			DbgDraw.DrawCircle(Pos + axis * halfHeight, axis, color, radius);
			DbgDraw.DrawCircle(Pos - axis * halfHeight, axis, color, radius);

			//throw new NotImplementedException();
		}

		public void DrawLine(ref Vector3 from, ref Vector3 to, Color color) {
			//throw new NotImplementedException();

			DbgDraw.DrawLine(from, to, color);
		}

		public void DrawLine(ref Vector3 from, ref Vector3 to, Color fromColor, Color toColor) {
			throw new NotImplementedException();
		}

		public void DrawPlane(ref Vector3 planeNormal, float planeConst, ref Matrix4x4 transform, Color color) {
			throw new NotImplementedException();
		}

		public void DrawSphere(float radius, ref Matrix4x4 transform, Color color) {
			Matrix4x4.Decompose(transform, out Vector3 Scale, out Quaternion Rot, out Vector3 Pos);
			DbgDraw.DrawSphere(Pos, radius, color);

			//throw new NotImplementedException();
		}

		public void DrawSphere(ref Vector3 p, float radius, Color color) {
			throw new NotImplementedException();
		}

		public void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, Color color) {
			throw new NotImplementedException();
		}

		public void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, Color color, float stepDegrees) {
			throw new NotImplementedException();
		}

		public void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, Color color, float stepDegrees, bool drawCenter) {
			throw new NotImplementedException();
		}

		public void DrawTransform(ref Matrix4x4 transform, float orthoLen) {
			throw new NotImplementedException();
		}

		public void DrawTriangle(ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, Color color, float alpha) {
			throw new NotImplementedException();
		}

		public void DrawTriangle(ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, ref Vector3 __unnamed3, ref Vector3 __unnamed4, ref Vector3 __unnamed5, Color color, float alpha) {
			throw new NotImplementedException();
		}

		public void FlushLines() {
		}

		public void ReportErrorWarning(string warningString) {
			throw new NotImplementedException();
		}
	}
}
