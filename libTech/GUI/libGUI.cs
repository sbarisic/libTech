using FishGfx;
using FishGfx.Formats;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
using libTech.Graphics;
using libTech.GUI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace libTech.GUI {
	public class libGUI {
		Dictionary<Key, Control> MouseHeldControls;
		List<Control> Controls;

		Vector2 MousePos;

		BMFont AnonPro16;
		BMFont AnonPro32;

		public GfxFont DefaultFont;
		public GfxFont DebugFont;

		public Texture WindowSkin;

		public Texture ButtonSkin;
		public Texture ButtonHoverSkin;
		public Texture ButtonDisabledSkin;
		public Texture ButtonClickSkin;

		public Texture ButtonCloseSkin;
		public Texture ButtonCloseHoverSkin;
		public Texture ButtonCloseClickSkin;

		public Texture CheckBoxSkin;
		public Texture CheckBoxCheckedSkin;
		public Texture CheckBoxHoverOverlaySkin;

		public void LoadSkin(string SkinFolder) {
			Texture.CreateOrUpdateFromFile(ref WindowSkin, Path.Combine(SkinFolder, "window.png"));

			Texture.CreateOrUpdateFromFile(ref ButtonSkin, Path.Combine(SkinFolder, "button.png"));
			Texture.CreateOrUpdateFromFile(ref ButtonHoverSkin, Path.Combine(SkinFolder, "button_hover.png"));
			Texture.CreateOrUpdateFromFile(ref ButtonDisabledSkin, Path.Combine(SkinFolder, "button_disabled.png"));
			Texture.CreateOrUpdateFromFile(ref ButtonClickSkin, Path.Combine(SkinFolder, "button_click.png"));

			Texture.CreateOrUpdateFromFile(ref ButtonCloseSkin, Path.Combine(SkinFolder, "button_close.png"));
			Texture.CreateOrUpdateFromFile(ref ButtonCloseHoverSkin, Path.Combine(SkinFolder, "button_close_hover.png"));
			Texture.CreateOrUpdateFromFile(ref ButtonCloseClickSkin, Path.Combine(SkinFolder, "button_close_click.png"));

			Texture.CreateOrUpdateFromFile(ref CheckBoxSkin, Path.Combine(SkinFolder, "checkbox.png"));
			Texture.CreateOrUpdateFromFile(ref CheckBoxCheckedSkin, Path.Combine(SkinFolder, "checkbox_checked.png"));
			Texture.CreateOrUpdateFromFile(ref CheckBoxHoverOverlaySkin, Path.Combine(SkinFolder, "checkbox_hover_overlay.png"));
		}

		public libGUI(RenderWindow RWind) {
			Controls = new List<Control>();
			MouseHeldControls = new Dictionary<Key, Control>();
			LoadSkin("content/textures/gui_elements/standard");

			RWind.OnMouseMove += (W, X, Y) => {
				MousePos = new Vector2(X, W.WindowHeight - Y);
				OnMouseMoveEventArgs Evt = new OnMouseMoveEventArgs(this, MousePos);

				for (int i = Controls.Count - 1; i >= 0; i--) {
					// for (int i = 0; i < Controls.Count; i++) {
					Control C = Controls[i];
					C.OnMouseMove(Evt);

					if (Evt.Consumed)
						break;
				}
			};

			RWind.OnKey += (W, Key, Scancode, Pressed, Repeat, Mods) => {
				bool IsMouse = Key >= Key.MouseButton1 && Key <= Key.MouseButton8;
				OnKeyEventArgs Evt = new OnKeyEventArgs(this, MousePos, Pressed, Key);

				if (IsMouse) {
					bool ClickedOnAny = false;
					Control ClickedRootControl = null;

					for (int i = 0; i < Controls.Count; i++) {
						// for (int i = Controls.Count - 1; i >= 0; i--) {
						Control C = Controls[i];

						if (C.IsInside(MousePos)) {
							Control Ctrl = C.GetRecursiveClientAreaChildAt(MousePos) ?? C;
							ClickControl(Ctrl, Key, Pressed);

							ClickedOnAny = true;
							ClickedRootControl = C;
						}
					}

					if (ClickedRootControl != null)
						BringToFront(ClickedRootControl);

					if (!ClickedOnAny)
						ClickControl(null, Key, Pressed);
				}

				for (int i = Controls.Count - 1; i >= 0; i--) {
					//for (int i = 0; i < Controls.Count; i++) {
					Control C = Controls[i];
					C.OnKey(Evt);

					if (Evt.Consumed)
						break;
				}
			};

			DefaultFont = new BMFont("content/fonts/proggy_clean_16.fnt");
			((BMFont)DefaultFont).LoadTextures("content/textures", TextureFilter.Linear);

			DebugFont = new BMFont("content/fonts/proggy_small_16.fnt");
			((BMFont)DebugFont).LoadTextures("content/textures", TextureFilter.Linear);

			AnonPro16 = new BMFont("content/fonts/anonymous_pro_16.fnt");
			AnonPro16.LoadTextures("content/textures", TextureFilter.Linear);

			AnonPro32 = new BMFont("content/fonts/anonymous_pro_32.fnt");
			AnonPro32.LoadTextures("content/textures", TextureFilter.Linear);
		}

		void ClickControl(Control C, Key MouseKey, bool Pressed) {
			if (!MouseHeldControls.ContainsKey(MouseKey))
				MouseHeldControls.Add(MouseKey, null);

			if (Pressed) {
				Control PressedOn = null;
				if ((PressedOn = MouseHeldControls[MouseKey]) == null) {
					if (C != null) {
						MouseHeldControls[MouseKey] = C;
						C.OnBeginHold(new OnKeyEventArgs(this, MousePos, Pressed, MouseKey));
					}
				} else {
					ClickControl(PressedOn, MouseKey, false);
					ClickControl(C, MouseKey, true);
				}
			} else {
				Control ReleasedOn = null;
				if ((ReleasedOn = MouseHeldControls[MouseKey]) != null) {
					MouseHeldControls[MouseKey] = null;
					ReleasedOn.OnEndHold(new OnKeyEventArgs(this, MousePos, Pressed, MouseKey));
				}
			}
		}

		public Control GetHeldControl(Key MouseKey) {
			if (MouseHeldControls.ContainsKey(MouseKey))
				return MouseHeldControls[MouseKey];

			return null;
		}

		public void Update(float Dt) {
			foreach (var C in Controls)
				C.Update(Dt);
		}

		public void AddControl(Control C, bool PerformSanityCheck = true) {
			if (PerformSanityCheck) {
				if (Controls.Contains(C))
					throw new Exception("Control already added to UI");

				foreach (var Ctrl in Controls)
					if (Ctrl.ContainsControl(C))
						throw new Exception("Control already present in one of the existing ones");
			}

			Controls.Add(C);
		}

		public void RemoveControl(Control C) {
			Controls.Remove(C);
		}

		public void BringToFront(Control C) {
			if (Controls.IndexOf(C) < Controls.Count - 1) {
				RemoveControl(C);
				AddControl(C);
			}
		}

		public void Draw() {
			float FT = Engine.FrameTime.Average();
			float FPS = 1.0f / FT;

			RenderState RS = Gfx.PeekRenderState();
			RS.EnableScissorTest = true;
			RS.ScissorRegion = new AABB(new Vector2(Engine.WindowWidth, Engine.WindowHeight));
			Gfx.PushRenderState(RS);
			
			/*Color[] Colors = new Color[] { Color.White, Color.Black, Color.Yellow, Color.Cyan, Color.Red };

			for (int i = 0; i < Colors.Length; i++) {
				Gfx.DrawText(AnonPro32, new Vector2(10, Engine.WindowHeight - 50 - AnonPro32.ScaledLineHeight * i), "Hello Font World!", Colors[i]);
			}*/

			foreach (var C in Controls)
				C.Draw();

			Gfx.PopRenderState();
		}
	}

	public static class libGUIExtensions {
		public static Button AddButton(this Window Wnd, Vector2 Pos, Vector2 Size, string Text, OnButtonClickFunc OnClick = null) {
			Button Btn = new Button(Wnd.GUI);
			Btn.Position = Pos;
			Btn.Size = Size;
			Btn.Text = Text;

			if (OnClick != null)
				Btn.OnClick += OnClick;

			Wnd.AddChild(Btn);
			return Btn;
		}
	}
}
