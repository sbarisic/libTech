using FishGfx;
using FishGfx.Graphics;
using FishGfx_Nuklear;
using FishMarkupLanguage;
using libTech.Graphics;
using NuklearDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using DynamicExpression = System.Linq.Dynamic.DynamicExpression;

namespace libTech.GUI {
	struct ParamDef {
		public Type T;
		public string Name;

		public ParamDef(Type T, string Name) {
			this.T = T;
			this.Name = Name;
		}
	}

	static class libGUIScriptManager {
		public class ScriptState {
			public bool Invoke(Action A) {
				A();
				return true;
			}

			public bool Wat() {
				return true;
			}
		}

		static Dictionary<string, Delegate> Scripts = new Dictionary<string, Delegate>();
		static ScriptState ScriptStateInstance = new ScriptState();

		static Delegate CacheCompile(string Script, params ParamDef[] Params) {
			Delegate ScriptDelegate = null;

			List<ParamDef> ParamsDefList = new List<ParamDef>();
			ParamsDefList.Add(new ParamDef(typeof(ScriptState), "Script"));

			if (Params != null)
				ParamsDefList.AddRange(Params);

			if (Scripts.ContainsKey(Script))
				ScriptDelegate = Scripts[Script];
			else {
				ScriptDelegate = DynamicExpression.ParseLambda(ParamsDefList.Select(KV => Expression.Parameter(KV.T, KV.Name)).ToArray(), null, Script).Compile();
				Scripts.Add(Script, ScriptDelegate);
			}
			
			return ScriptDelegate;
		}

		public static object CacheInvoke(string Script, ParamDef[] Params, params object[] Args) {
			List<object> ArgsList = new List<object>();
			ArgsList.Add(ScriptStateInstance);
			ArgsList.AddRange(Args);

			return CacheCompile(Script, Params).DynamicInvoke(ArgsList.ToArray());
		}
	}

	public unsafe class libGUI {
		FishGfxDevice Dev;
		RenderWindow RWind;
		List<FMLDocument> Docs;

		int FreeWindowSlot;

		public void Init(RenderWindow RWind, ShaderProgram GUIShader) {
			this.RWind = RWind;
			Docs = new List<FMLDocument>();

			Dev = new FishGfxDevice(RWind.WindowSize, GUIShader);
			Dev.RegisterEvents(RWind);

			NuklearAPI.Init(Dev);
		}

		public void OnMouseMove(RenderWindow RWind, float X, float Y) {

		}

		public void OnKey(RenderWindow Wnd, Key Key, int Scancode, bool Pressed, bool Repeat, KeyMods Mods) {

		}

		public void OnChar(RenderWindow Wnd, string Char, uint Unicode) {

		}

		public void DrawDocument(FMLDocument Doc) {
			PaintTags(Doc.Tags);
		}

		void PaintTags(IEnumerable<FMLTag> Tags) {
			foreach (var T in Tags)
				PaintTag(T);
		}

		string GetWindowName() {
			return "window_" + FreeWindowSlot++.ToString();
		}

		void PaintTag(FMLTag Tag) {
			FMLAttributes Attrib = Tag.Attributes;

			switch (Tag.TagName) {
				case "root": {
						PaintTags(Tag.Children);
						break;
					}

				case "window": {
						NkPanelFlags Flags = NkPanelFlags.BorderTitle;

						if (Attrib.GetAttribute("minimizable", true))
							Flags |= NkPanelFlags.Minimizable;

						if (Attrib.GetAttribute("movable", false))
							Flags |= NkPanelFlags.Movable;

						if (Attrib.GetAttribute("resizable", false))
							Flags |= NkPanelFlags.Scalable;

						if (Attrib.GetAttribute("closable", false))
							Flags |= NkPanelFlags.Closable;

						if (Attrib.GetAttribute("noscrollbar", false))
							Flags |= NkPanelFlags.NoScrollbar;

						float X = Attrib.GetAttribute("x", 0.0f);
						float Y = Attrib.GetAttribute("y", 0.0f);
						float W = Attrib.GetAttribute("width", 50.0f);
						float H = Attrib.GetAttribute("height", 50.0f);
						string Title = Attrib.GetAttribute("title", GetWindowName());

						bool DoLayout = !Attrib.GetAttribute("nolayout", false);

						NuklearAPI.Window(Title, X, Y, W, H, Flags, () => {
							if (DoLayout)
								NuklearAPI.LayoutRowDynamic();

							PaintTags(Tag.Children);
						});
						break;
					}

				case "panel": {
						NkPanelFlags Flags = NkPanelFlags.NoScrollbar;

						float X = Attrib.GetAttribute("x", 0.0f);
						float Y = Attrib.GetAttribute("y", 0.0f);
						float W = Attrib.GetAttribute("width", 50.0f);
						float H = Attrib.GetAttribute("height", 50.0f);
						string Title = Attrib.GetAttribute("title", GetWindowName());

						byte R = (byte)Attrib.GetAttribute("r", 0.0f);
						byte G = (byte)Attrib.GetAttribute("g", 0.0f);
						byte B = (byte)Attrib.GetAttribute("b", 0.0f);
						byte A = (byte)Attrib.GetAttribute("a", 0.0f);

						Nuklear.nk_style_push_style_item(NuklearAPI.Ctx, &NuklearAPI.Ctx->style.window.fixed_background, Nuklear.nk_style_item_color(new NkColor(R, G, B, A)));
						NuklearAPI.Window(Title, X, Y, W, H, Flags, () => {
							NuklearAPI.LayoutRowDynamic();

							PaintTags(Tag.Children);
						});
						Nuklear.nk_style_pop_style_item(NuklearAPI.Ctx);
						break;
					}

				case "button": {
						if (NuklearAPI.ButtonLabel(Attrib.GetAttribute("text", "Button"))) {
							/*object OnClick = Attrib.GetAttribute("onclick");

							if (OnClick is string OnClickStr)
								libGUIScriptManager.CacheInvoke(OnClickStr, null);*/
						}

						break;
					}

				case "input": {
						NkEditTypes EditType = 0;

						if (Attrib.GetAttribute("editor", false))
							EditType = NkEditTypes.Editor;
						else if (Attrib.GetAttribute("field", false))
							EditType = NkEditTypes.Field;
						else
							throw new Exception("No input box type specified");

						if (Attrib.GetAttribute("readonly", false))
							EditType |= (NkEditTypes)NkEditFlags.ReadOnly;

						NuklearAPI.EditString(EditType | (NkEditTypes)(NkEditFlags.GotoEndOnActivate), Out);
						break;
					}

				case "layout": {
						nk_layout_format Layout = 0;

						if (Attrib.GetAttribute("dynamic", false))
							Layout = nk_layout_format.NK_DYNAMIC;
						else if (Attrib.GetAttribute("static", false))
							Layout = nk_layout_format.NK_STATIC;
						else
							throw new Exception("No layout type specified");

						int Columns = (int)Attrib.GetAttribute("columns", 1.0f);
						object Height = Attrib.GetAttribute("height");

						if (Height is FMLHereDoc HeightScript)
							Height = Convert.ToSingle(libGUIScriptManager.CacheInvoke(HeightScript.Content, new[] { new ParamDef(typeof(NkRect), "Window") }, NuklearAPI.WindowGetBounds()));

						Nuklear.nk_layout_row_begin(NuklearAPI.Ctx, Layout, (Height as float?) ?? 0.0f, Columns);
						Nuklear.nk_layout_row_push(NuklearAPI.Ctx, 1);

						PaintTags(Tag.Children);

						Nuklear.nk_layout_row_end(NuklearAPI.Ctx);
						break;
					}

				case "row": {
						Nuklear.nk_layout_row_push(NuklearAPI.Ctx, Attrib.GetAttribute("width", 1.0f));
						break;
					}

				default:
					throw new InvalidOperationException("Unknown tag name " + Tag.TagName);
			}
		}

		StringBuilder Out = new StringBuilder(1024);
		StringBuilder In = new StringBuilder(1024);

		public void Draw(Action DrawAction) {
			Dev.WindowSize = RWind.WindowSize;
			FreeWindowSlot = 0;

			NuklearAPI.Frame(() => {
				DrawAction();

				/*const NkPanelFlags Flags = NkPanelFlags.ClosableMinimizable | NkPanelFlags.MovableScalable | NkPanelFlags.NoScrollbar;

				NuklearAPI.Window("Consoleeee", 400, 400, 200, 200, Flags, () => {
					NkRect Bounds = NuklearAPI.WindowGetBounds();

					Nuklear.nk_layout_row_begin(NuklearAPI.Ctx, nk_layout_format.NK_DYNAMIC, Bounds.H - 85, 1);
					Nuklear.nk_layout_row_push(NuklearAPI.Ctx, 1);
					NuklearAPI.EditString(NkEditTypes.ReadOnlyEditor | (NkEditTypes)(NkEditFlags.GotoEndOnActivate), Out);
					Nuklear.nk_layout_row_end(NuklearAPI.Ctx);


					NuklearAPI.LayoutRowDynamic();
					NuklearAPI.EditString(NkEditTypes.Field, In);
				});
				//*/
			});

			Docs.Clear();
		}
	}
}
