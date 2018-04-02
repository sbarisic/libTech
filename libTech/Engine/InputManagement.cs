using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using libTech.Reflection;
using libTech.Importer;
using System.Reflection;
using System.Numerics;
using CARP;
using System.IO;
using System.Drawing;

using NuklearDotNet;
using System.Runtime.InteropServices;
using libTech.UI;

using Glfw3;
using OpenGL;
using System.Threading;
using System.Diagnostics;
using Khronos;

using libTech.Graphics;

using Matrix4 = System.Numerics.Matrix4x4;

namespace libTech {
	public unsafe static partial class Engine {
		static Glfw.CursorMode LastCursorMode;
		public static void CaptureMouse(bool Capture) {
			Glfw.CursorMode M = Capture ? Glfw.CursorMode.Disabled : Glfw.CursorMode.Normal;

			if (LastCursorMode != M) {
				LastCursorMode = M;
				Glfw.SetInputMode(Window, Glfw.InputMode.Cursor, M);
			}
		}

		public static bool GetMouseButton(Glfw.MouseButton Btn) {
			return Glfw.GetMouseButton(Window, Btn);
		}

		public static bool GetKey(Glfw.KeyCode Key) {
			return Glfw.GetKey(Window, (int)Key);
		}

		static NkKeys ConvertToNkKey(Glfw.KeyCode KCode, Glfw.KeyMods Mods) {
			switch (KCode) {
				case Glfw.KeyCode.RightShift:
				case Glfw.KeyCode.LeftShift:
					return NkKeys.Shift;

				case Glfw.KeyCode.LeftControl:
				case Glfw.KeyCode.RightControl:
					return NkKeys.Ctrl;

				case Glfw.KeyCode.Delete:
					return NkKeys.Del;

				case Glfw.KeyCode.Enter:
				case Glfw.KeyCode.NumpadEnter:
					return NkKeys.Enter;

				case Glfw.KeyCode.Tab:
					return NkKeys.Tab;

				case Glfw.KeyCode.Backspace:
					return NkKeys.Backspace;

				case Glfw.KeyCode.Up:
					return NkKeys.Up;

				case Glfw.KeyCode.Down:
					return NkKeys.Down;

				case Glfw.KeyCode.Left:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.TextWordLeft;
					return NkKeys.Left;

				case Glfw.KeyCode.Right:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.TextWordRight;
					return NkKeys.Right;

				case Glfw.KeyCode.Home:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.TextStart;
					return NkKeys.LineStart;

				case Glfw.KeyCode.End:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.TextEnd;
					return NkKeys.LineEnd;

				case Glfw.KeyCode.PageUp:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.ScrollStart;
					return NkKeys.ScrollUp;

				case Glfw.KeyCode.PageDown:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.ScrollEnd;
					return NkKeys.ScrollDown;

				case Glfw.KeyCode.A:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.TextSelectAll;
					return NkKeys.None;

				case Glfw.KeyCode.Z:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.TextUndo;
					return NkKeys.None;

				case Glfw.KeyCode.Y:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.TextRedo;
					return NkKeys.None;

				case Glfw.KeyCode.C:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.Copy;
					return NkKeys.None;

				case Glfw.KeyCode.V:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.Paste;
					return NkKeys.None;

				case Glfw.KeyCode.X:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.Cut;
					return NkKeys.None;

				default:
					return NkKeys.None;
			}
		}
	}
}