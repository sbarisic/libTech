using FishGfx;
using FishGfx.Graphics;
using FishGfx_Nuklear;
using FishMarkupLanguage;
using libTech.Graphics;
using libTech.Scripting;
using NuklearDotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using LuaFuncRef = LuaNET.LuaFuncRef;
using LuaReference = LuaNET.LuaReference;

namespace libTech.GUI {
	public class ScriptArg {
		public string Name;
		public object Value;

		public ScriptArg(string Name, object Value) {
			this.Name = Name;
			this.Value = Value;
		}
	}

	static class libGUIScriptManager {
		static Dictionary<string, LuaFuncRef> Scripts = new Dictionary<string, LuaFuncRef>();

		static LuaFuncRef CacheCompile(string Script) {
			LuaFuncRef ScriptDelegate = null;
			Script = Script.Trim();

			if (Scripts.ContainsKey(Script))
				ScriptDelegate = Scripts[Script];
			else {
				ScriptDelegate = Lua.Compile(Script);
				Scripts.Add(Script, ScriptDelegate);
			}

			return ScriptDelegate;
		}

		public static T CacheInvokeFunc<T>(string Script, params ScriptArg[] Args) {
			LuaFuncRef Func = CacheCompile(Script);
			// TODO: Single environment

			using (LuaReference Env = Lua.CreateNewEnvironment(Lua.GUIEnvironment)) {
				Lua.SetEnvironment(Func, Env);

				foreach (var Arg in Args)
					Lua.Set(Env, Arg.Name, Arg.Value);

				return Lua.Run<T>(Func);
			}
		}

		public static void CacheInvokeAction(string Script, params ScriptArg[] Args) {
			object Result = CacheInvokeFunc<object>(Script, Args);

			if (Result is LuaReference Ref)
				Ref.Dispose();
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

		public void DrawDocument(GUIDocument Doc) {
			lock (Doc)
				PaintTags(Doc.Tags);
		}

		void PaintTags(IEnumerable<FMLTag> Tags) {
			foreach (var T in Tags)
				PaintTag(T);
		}

		string GetWindowName() {
			return "window_" + FreeWindowSlot++.ToString();
		}

		Stopwatch SWatch = Stopwatch.StartNew();

		ScriptArg[] CreateScriptGlobals() {
			return new ScriptArg[] {
				new ScriptArg("Window", Lua.ConvertToTable(NuklearAPI.WindowGetBounds())),
				new ScriptArg("Time", SWatch.ElapsedMilliseconds / 1000.0f)
			};
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
							object OnClick = Attrib.GetAttribute("script");

							if (OnClick is FMLHereDoc OnClickDoc)
								libGUIScriptManager.CacheInvokeAction(OnClickDoc.Content, CreateScriptGlobals());
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
							Height = libGUIScriptManager.CacheInvokeFunc<float>(HeightScript.Content, CreateScriptGlobals());

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
