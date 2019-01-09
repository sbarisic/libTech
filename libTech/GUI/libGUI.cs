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

	struct Media {
		public int skin;

		public nk_image menu;
		public nk_image check;
		public nk_image check_cursor;
		public nk_image option;
		public nk_image option_cursor;
		public nk_image header;
		public nk_image window;
		public nk_image scrollbar_inc_button;
		public nk_image scrollbar_inc_button_hover;
		public nk_image scrollbar_dec_button;
		public nk_image scrollbar_dec_button_hover;
		public nk_image button;
		public nk_image button_hover;
		public nk_image button_active;
		public nk_image tab_minimize;
		public nk_image tab_maximize;
		public nk_image slider;
		public nk_image slider_hover;
		public nk_image slider_active;
	}

	public unsafe class libGUI {
		FishGfxDevice Dev;
		RenderWindow RWind;
		List<FMLDocument> Docs;

		int FreeWindowSlot;

		Media media;

		public void Init(RenderWindow RWind, ShaderProgram GUIShader) {
			this.RWind = RWind;
			Docs = new List<FMLDocument>();

			Dev = new FishGfxDevice(RWind.WindowSize, GUIShader);
			Dev.RegisterEvents(RWind);

			NuklearAPI.Init(Dev);

			// TODO: Skinning
			ref nk_style style = ref NuklearAPI.Ctx->style;
			Texture Tex = Texture.FromFile("content/textures/gwen.png");
			Tex.SetFilter(TextureFilter.Linear);

			media = new Media();
			media.skin = Dev.CreateTextureHandle(Tex);
			media.check = Nuklear.nk_subimage_id(media.skin, 512, 512, new NkRect(464, 32, 15, 15));
			media.check_cursor = Nuklear.nk_subimage_id(media.skin, 512, 512, new NkRect(450, 34, 11, 11));
			media.option = Nuklear.nk_subimage_id(media.skin, 512, 512, new NkRect(464, 64, 15, 15));
			media.option_cursor = Nuklear.nk_subimage_id(media.skin, 512, 512, new NkRect(451, 67, 9, 9));
			media.header = Nuklear.nk_subimage_id(media.skin, 512, 512, new NkRect(128, 0, 127, 24));
			media.window = Nuklear.nk_subimage_id(media.skin, 512, 512, new NkRect(128, 23, 127, 104));
			media.scrollbar_inc_button = Nuklear.nk_subimage_id(media.skin, 512, 512, new NkRect(464, 256, 15, 15));
			media.scrollbar_inc_button_hover = Nuklear.nk_subimage_id(media.skin, 512, 512, new NkRect(464, 320, 15, 15));
			media.scrollbar_dec_button = Nuklear.nk_subimage_id(media.skin, 512, 512, new NkRect(464, 224, 15, 15));
			media.scrollbar_dec_button_hover = Nuklear.nk_subimage_id(media.skin, 512, 512, new NkRect(464, 288, 15, 15));
			media.button = Nuklear.nk_subimage_id(media.skin, 512, 512, new NkRect(384, 336, 127, 31));
			media.button_hover = Nuklear.nk_subimage_id(media.skin, 512, 512, new NkRect(384, 368, 127, 31));
			media.button_active = Nuklear.nk_subimage_id(media.skin, 512, 512, new NkRect(384, 400, 127, 31));
			media.tab_minimize = Nuklear.nk_subimage_id(media.skin, 512, 512, new NkRect(451, 99, 9, 9));
			media.tab_maximize = Nuklear.nk_subimage_id(media.skin, 512, 512, new NkRect(467, 99, 9, 9));
			media.slider = Nuklear.nk_subimage_id(media.skin, 512, 512, new NkRect(418, 33, 11, 14));
			media.slider_hover = Nuklear.nk_subimage_id(media.skin, 512, 512, new NkRect(418, 49, 11, 14));
			media.slider_active = Nuklear.nk_subimage_id(media.skin, 512, 512, new NkRect(418, 64, 11, 14));

			style.window.background = new NkColor(204, 204, 204);
			style.window.fixed_background = Nuklear.nk_style_item_image(media.window);
			style.window.border_color = new NkColor(67, 67, 67);
			style.window.combo_border_color = new NkColor(67, 67, 67);
			style.window.contextual_border_color = new NkColor(67, 67, 67);
			style.window.menu_border_color = new NkColor(67, 67, 67);
			style.window.group_border_color = new NkColor(67, 67, 67);
			style.window.tooltip_border_color = new NkColor(67, 67, 67);
			style.window.scrollbar_size = new nk_vec2(16, 16);
			style.window.border_color = new NkColor(0, 0, 0, 0);
			style.window.padding = new nk_vec2(8, 4);
			style.window.border = 3;

			/* window header */
			style.window.header.normal = Nuklear.nk_style_item_image(media.header);
			style.window.header.hover = Nuklear.nk_style_item_image(media.header);
			style.window.header.active = Nuklear.nk_style_item_image(media.header);
			style.window.header.label_normal = new NkColor(95, 95, 95);
			style.window.header.label_hover = new NkColor(95, 95, 95);
			style.window.header.label_active = new NkColor(95, 95, 95);

			/* scrollbar */
			style.scrollv.normal = Nuklear.nk_style_item_color(new NkColor(184, 184, 184));
			style.scrollv.hover = Nuklear.nk_style_item_color(new NkColor(184, 184, 184));
			style.scrollv.active = Nuklear.nk_style_item_color(new NkColor(184, 184, 184));
			style.scrollv.cursor_normal = Nuklear.nk_style_item_color(new NkColor(220, 220, 220));
			style.scrollv.cursor_hover = Nuklear.nk_style_item_color(new NkColor(235, 235, 235));
			style.scrollv.cursor_active = Nuklear.nk_style_item_color(new NkColor(99, 202, 255));
			style.scrollv.dec_symbol = nk_symbol_type.NK_SYMBOL_NONE;
			style.scrollv.inc_symbol = nk_symbol_type.NK_SYMBOL_NONE;
			style.scrollv.show_buttons = 1;
			style.scrollv.border_color = new NkColor(81, 81, 81);
			style.scrollv.cursor_border_color = new NkColor(81, 81, 81);
			style.scrollv.border = 1;
			style.scrollv.rounding = 0;
			style.scrollv.border_cursor = 1;
			style.scrollv.rounding_cursor = 2;

			/* scrollbar buttons */
			style.scrollv.inc_button.normal = Nuklear.nk_style_item_image(media.scrollbar_inc_button);
			style.scrollv.inc_button.hover = Nuklear.nk_style_item_image(media.scrollbar_inc_button_hover);
			style.scrollv.inc_button.active = Nuklear.nk_style_item_image(media.scrollbar_inc_button_hover);
			style.scrollv.inc_button.border_color = new NkColor(0, 0, 0, 0);
			style.scrollv.inc_button.text_background = new NkColor(0, 0, 0, 0);
			style.scrollv.inc_button.text_normal = new NkColor(0, 0, 0, 0);
			style.scrollv.inc_button.text_hover = new NkColor(0, 0, 0, 0);
			style.scrollv.inc_button.text_active = new NkColor(0, 0, 0, 0);
			style.scrollv.inc_button.border = 0.0f;

			style.scrollv.dec_button.normal = Nuklear.nk_style_item_image(media.scrollbar_dec_button);
			style.scrollv.dec_button.hover = Nuklear.nk_style_item_image(media.scrollbar_dec_button_hover);
			style.scrollv.dec_button.active = Nuklear.nk_style_item_image(media.scrollbar_dec_button_hover);
			style.scrollv.dec_button.border_color = new NkColor(0, 0, 0, 0);
			style.scrollv.dec_button.text_background = new NkColor(0, 0, 0, 0);
			style.scrollv.dec_button.text_normal = new NkColor(0, 0, 0, 0);
			style.scrollv.dec_button.text_hover = new NkColor(0, 0, 0, 0);
			style.scrollv.dec_button.text_active = new NkColor(0, 0, 0, 0);
			style.scrollv.dec_button.border = 0.0f;

			/* checkbox toggle */
			{
				ref nk_style_toggle toggle = ref style.checkbox;
				toggle.normal = Nuklear.nk_style_item_image(media.check);
				toggle.hover = Nuklear.nk_style_item_image(media.check);
				toggle.active = Nuklear.nk_style_item_image(media.check);
				toggle.cursor_normal = Nuklear.nk_style_item_image(media.check_cursor);
				toggle.cursor_hover = Nuklear.nk_style_item_image(media.check_cursor);
				toggle.text_normal = new NkColor(95, 95, 95);
				toggle.text_hover = new NkColor(95, 95, 95);
				toggle.text_active = new NkColor(95, 95, 95);
			}

			/* option toggle */
			{
				ref nk_style_toggle toggle = ref style.option;
				toggle.normal = Nuklear.nk_style_item_image(media.option);
				toggle.hover = Nuklear.nk_style_item_image(media.option);
				toggle.active = Nuklear.nk_style_item_image(media.option);
				toggle.cursor_normal = Nuklear.nk_style_item_image(media.option_cursor);
				toggle.cursor_hover = Nuklear.nk_style_item_image(media.option_cursor);
				toggle.text_normal = new NkColor(95, 95, 95);
				toggle.text_hover = new NkColor(95, 95, 95);
				toggle.text_active = new NkColor(95, 95, 95);
			}

			/* default button */
			style.button.normal = Nuklear.nk_style_item_image(media.button);
			style.button.hover = Nuklear.nk_style_item_image(media.button_hover);
			style.button.active = Nuklear.nk_style_item_image(media.button_active);
			style.button.border_color = new NkColor(0, 0, 0, 0);
			style.button.text_background = new NkColor(0, 0, 0, 0);
			style.button.text_normal = new NkColor(95, 95, 95);
			style.button.text_hover = new NkColor(95, 95, 95);
			style.button.text_active = new NkColor(95, 95, 95);

			/* default text */
			style.text.color = new NkColor(95, 95, 95);

			/* contextual button */
			style.contextual_button.normal = Nuklear.nk_style_item_color(new NkColor(206, 206, 206));
			style.contextual_button.hover = Nuklear.nk_style_item_color(new NkColor(229, 229, 229));
			style.contextual_button.active = Nuklear.nk_style_item_color(new NkColor(99, 202, 255));
			style.contextual_button.border_color = new NkColor(0, 0, 0, 0);
			style.contextual_button.text_background = new NkColor(0, 0, 0, 0);
			style.contextual_button.text_normal = new NkColor(95, 95, 95);
			style.contextual_button.text_hover = new NkColor(95, 95, 95);
			style.contextual_button.text_active = new NkColor(95, 95, 95);

			/* menu button */
			style.menu_button.normal = Nuklear.nk_style_item_color(new NkColor(206, 206, 206));
			style.menu_button.hover = Nuklear.nk_style_item_color(new NkColor(229, 229, 229));
			style.menu_button.active = Nuklear.nk_style_item_color(new NkColor(99, 202, 255));
			style.menu_button.border_color = new NkColor(0, 0, 0, 0);
			style.menu_button.text_background = new NkColor(0, 0, 0, 0);
			style.menu_button.text_normal = new NkColor(95, 95, 95);
			style.menu_button.text_hover = new NkColor(95, 95, 95);
			style.menu_button.text_active = new NkColor(95, 95, 95);

			/* tree */
			style.tab.text = new NkColor(95, 95, 95);
			style.tab.tab_minimize_button.normal = Nuklear.nk_style_item_image(media.tab_minimize);
			style.tab.tab_minimize_button.hover = Nuklear.nk_style_item_image(media.tab_minimize);
			style.tab.tab_minimize_button.active = Nuklear.nk_style_item_image(media.tab_minimize);
			style.tab.tab_minimize_button.text_background = new NkColor(0, 0, 0, 0);
			style.tab.tab_minimize_button.text_normal = new NkColor(0, 0, 0, 0);
			style.tab.tab_minimize_button.text_hover = new NkColor(0, 0, 0, 0);
			style.tab.tab_minimize_button.text_active = new NkColor(0, 0, 0, 0);

			style.tab.tab_maximize_button.normal = Nuklear.nk_style_item_image(media.tab_maximize);
			style.tab.tab_maximize_button.hover = Nuklear.nk_style_item_image(media.tab_maximize);
			style.tab.tab_maximize_button.active = Nuklear.nk_style_item_image(media.tab_maximize);
			style.tab.tab_maximize_button.text_background = new NkColor(0, 0, 0, 0);
			style.tab.tab_maximize_button.text_normal = new NkColor(0, 0, 0, 0);
			style.tab.tab_maximize_button.text_hover = new NkColor(0, 0, 0, 0);
			style.tab.tab_maximize_button.text_active = new NkColor(0, 0, 0, 0);

			style.tab.node_minimize_button.normal = Nuklear.nk_style_item_image(media.tab_minimize);
			style.tab.node_minimize_button.hover = Nuklear.nk_style_item_image(media.tab_minimize);
			style.tab.node_minimize_button.active = Nuklear.nk_style_item_image(media.tab_minimize);
			style.tab.node_minimize_button.text_background = new NkColor(0, 0, 0, 0);
			style.tab.node_minimize_button.text_normal = new NkColor(0, 0, 0, 0);
			style.tab.node_minimize_button.text_hover = new NkColor(0, 0, 0, 0);
			style.tab.node_minimize_button.text_active = new NkColor(0, 0, 0, 0);

			style.tab.node_maximize_button.normal = Nuklear.nk_style_item_image(media.tab_maximize);
			style.tab.node_maximize_button.hover = Nuklear.nk_style_item_image(media.tab_maximize);
			style.tab.node_maximize_button.active = Nuklear.nk_style_item_image(media.tab_maximize);
			style.tab.node_maximize_button.text_background = new NkColor(0, 0, 0, 0);
			style.tab.node_maximize_button.text_normal = new NkColor(0, 0, 0, 0);
			style.tab.node_maximize_button.text_hover = new NkColor(0, 0, 0, 0);
			style.tab.node_maximize_button.text_active = new NkColor(0, 0, 0, 0);

			/* selectable */
			style.selectable.normal = Nuklear.nk_style_item_color(new NkColor(206, 206, 206));
			style.selectable.hover = Nuklear.nk_style_item_color(new NkColor(206, 206, 206));
			style.selectable.pressed = Nuklear.nk_style_item_color(new NkColor(206, 206, 206));
			style.selectable.normal_active = Nuklear.nk_style_item_color(new NkColor(185, 205, 248));
			style.selectable.hover_active = Nuklear.nk_style_item_color(new NkColor(185, 205, 248));
			style.selectable.pressed_active = Nuklear.nk_style_item_color(new NkColor(185, 205, 248));
			style.selectable.text_normal = new NkColor(95, 95, 95);
			style.selectable.text_hover = new NkColor(95, 95, 95);
			style.selectable.text_pressed = new NkColor(95, 95, 95);
			style.selectable.text_normal_active = new NkColor(95, 95, 95);
			style.selectable.text_hover_active = new NkColor(95, 95, 95);
			style.selectable.text_pressed_active = new NkColor(95, 95, 95);

			/* slider */
			style.slider.normal = Nuklear.nk_style_item_hide();
			style.slider.hover = Nuklear.nk_style_item_hide();
			style.slider.active = Nuklear.nk_style_item_hide();
			style.slider.bar_normal = new NkColor(156, 156, 156);
			style.slider.bar_hover = new NkColor(156, 156, 156);
			style.slider.bar_active = new NkColor(156, 156, 156);
			style.slider.bar_filled = new NkColor(156, 156, 156);
			style.slider.cursor_normal = Nuklear.nk_style_item_image(media.slider);
			style.slider.cursor_hover = Nuklear.nk_style_item_image(media.slider_hover);
			style.slider.cursor_active = Nuklear.nk_style_item_image(media.slider_active);
			style.slider.cursor_size = new nk_vec2(16.5f, 21);
			style.slider.bar_height = 1;

			/* progressbar */
			style.progress.normal = Nuklear.nk_style_item_color(new NkColor(231, 231, 231));
			style.progress.hover = Nuklear.nk_style_item_color(new NkColor(231, 231, 231));
			style.progress.active = Nuklear.nk_style_item_color(new NkColor(231, 231, 231));
			style.progress.cursor_normal = Nuklear.nk_style_item_color(new NkColor(63, 242, 93));
			style.progress.cursor_hover = Nuklear.nk_style_item_color(new NkColor(63, 242, 93));
			style.progress.cursor_active = Nuklear.nk_style_item_color(new NkColor(63, 242, 93));
			style.progress.border_color = new NkColor(114, 116, 115);
			style.progress.padding = new nk_vec2(0, 0);
			style.progress.border = 2;
			style.progress.rounding = 1;

			/* combo */
			style.combo.normal = Nuklear.nk_style_item_color(new NkColor(216, 216, 216));
			style.combo.hover = Nuklear.nk_style_item_color(new NkColor(216, 216, 216));
			style.combo.active = Nuklear.nk_style_item_color(new NkColor(216, 216, 216));
			style.combo.border_color = new NkColor(95, 95, 95);
			style.combo.label_normal = new NkColor(95, 95, 95);
			style.combo.label_hover = new NkColor(95, 95, 95);
			style.combo.label_active = new NkColor(95, 95, 95);
			style.combo.border = 1;
			style.combo.rounding = 1;

			/* combo button */
			style.combo.button.normal = Nuklear.nk_style_item_color(new NkColor(216, 216, 216));
			style.combo.button.hover = Nuklear.nk_style_item_color(new NkColor(216, 216, 216));
			style.combo.button.active = Nuklear.nk_style_item_color(new NkColor(216, 216, 216));
			style.combo.button.text_background = new NkColor(216, 216, 216);
			style.combo.button.text_normal = new NkColor(95, 95, 95);
			style.combo.button.text_hover = new NkColor(95, 95, 95);
			style.combo.button.text_active = new NkColor(95, 95, 95);

			/* property */
			style.property.normal = Nuklear.nk_style_item_color(new NkColor(216, 216, 216));
			style.property.hover = Nuklear.nk_style_item_color(new NkColor(216, 216, 216));
			style.property.active = Nuklear.nk_style_item_color(new NkColor(216, 216, 216));
			style.property.border_color = new NkColor(81, 81, 81);
			style.property.label_normal = new NkColor(95, 95, 95);
			style.property.label_hover = new NkColor(95, 95, 95);
			style.property.label_active = new NkColor(95, 95, 95);
			style.property.sym_left = nk_symbol_type.NK_SYMBOL_TRIANGLE_LEFT;
			style.property.sym_right = nk_symbol_type.NK_SYMBOL_TRIANGLE_RIGHT;
			style.property.rounding = 10;
			style.property.border = 1;

			/* edit */
			style.edit.normal = Nuklear.nk_style_item_color(new NkColor(240, 240, 240));
			style.edit.hover = Nuklear.nk_style_item_color(new NkColor(240, 240, 240));
			style.edit.active = Nuklear.nk_style_item_color(new NkColor(240, 240, 240));
			style.edit.border_color = new NkColor(62, 62, 62);
			style.edit.cursor_normal = new NkColor(99, 202, 255);
			style.edit.cursor_hover = new NkColor(99, 202, 255);
			style.edit.cursor_text_normal = new NkColor(95, 95, 95);
			style.edit.cursor_text_hover = new NkColor(95, 95, 95);
			style.edit.text_normal = new NkColor(95, 95, 95);
			style.edit.text_hover = new NkColor(95, 95, 95);
			style.edit.text_active = new NkColor(95, 95, 95);
			style.edit.selected_normal = new NkColor(99, 202, 255);
			style.edit.selected_hover = new NkColor(99, 202, 255);
			style.edit.selected_text_normal = new NkColor(95, 95, 95);
			style.edit.selected_text_hover = new NkColor(95, 95, 95);
			style.edit.border = 1;
			style.edit.rounding = 2;

			/* property buttons */
			style.property.dec_button.normal = Nuklear.nk_style_item_color(new NkColor(216, 216, 216));
			style.property.dec_button.hover = Nuklear.nk_style_item_color(new NkColor(216, 216, 216));
			style.property.dec_button.active = Nuklear.nk_style_item_color(new NkColor(216, 216, 216));
			style.property.dec_button.text_background = new NkColor(0, 0, 0, 0);
			style.property.dec_button.text_normal = new NkColor(95, 95, 95);
			style.property.dec_button.text_hover = new NkColor(95, 95, 95);
			style.property.dec_button.text_active = new NkColor(95, 95, 95);
			style.property.inc_button = style.property.dec_button;

			/* property edit */
			style.property.edit.normal = Nuklear.nk_style_item_color(new NkColor(216, 216, 216));
			style.property.edit.hover = Nuklear.nk_style_item_color(new NkColor(216, 216, 216));
			style.property.edit.active = Nuklear.nk_style_item_color(new NkColor(216, 216, 216));
			style.property.edit.border_color = new NkColor(0, 0, 0, 0);
			style.property.edit.cursor_normal = new NkColor(95, 95, 95);
			style.property.edit.cursor_hover = new NkColor(95, 95, 95);
			style.property.edit.cursor_text_normal = new NkColor(216, 216, 216);
			style.property.edit.cursor_text_hover = new NkColor(216, 216, 216);
			style.property.edit.text_normal = new NkColor(95, 95, 95);
			style.property.edit.text_hover = new NkColor(95, 95, 95);
			style.property.edit.text_active = new NkColor(95, 95, 95);
			style.property.edit.selected_normal = new NkColor(95, 95, 95);
			style.property.edit.selected_hover = new NkColor(95, 95, 95);
			style.property.edit.selected_text_normal = new NkColor(216, 216, 216);
			style.property.edit.selected_text_hover = new NkColor(216, 216, 216);

			/* chart */
			style.chart.background = Nuklear.nk_style_item_color(new NkColor(216, 216, 216));
			style.chart.border_color = new NkColor(81, 81, 81);
			style.chart.color = new NkColor(95, 95, 95);
			style.chart.selected_color = new NkColor(255, 0, 0);
			style.chart.border = 1;
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

		ScriptArg[] CreateScriptGlobals() {
			NkRect WindowBounds = new NkRect(0, 0, Engine.Window.WindowWidth, Engine.Window.WindowHeight);

			if (NuklearAPI.Ctx->current != null)
				WindowBounds = NuklearAPI.WindowGetBounds();

			return new ScriptArg[] {
				new ScriptArg("Window", Lua.ConvertToTable(WindowBounds))
			};
		}

		// TODO: Setup variables once and then just copy from global environment for each function before invoking
		T GetScriptedAttrib<T>(FMLAttributes Attrib, string Name, T Default) {
			object Val = Attrib.GetAttribute(Name);

			if (Val is FMLHereDoc ScriptDoc)
				Val = libGUIScriptManager.CacheInvokeFunc<T>(ScriptDoc.Content, CreateScriptGlobals());

			if (Val is T TVal)
				return TVal;

			return Default;
		}

		void PaintTag(FMLTag Tag) {
			FMLAttributes Attrib = Tag.Attributes;

			switch (Tag.TagName) {
				case "root": {
						if (GetScriptedAttrib(Attrib, "disable", false))
							break;

						PaintTags(Tag.Children);
						break;
					}

				case "window": {
						if (GetScriptedAttrib(Attrib, "disable", false))
							break;

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

						float X = GetScriptedAttrib(Attrib, "x", 0.0f);
						float Y = GetScriptedAttrib(Attrib, "y", 0.0f);
						float W = GetScriptedAttrib(Attrib, "width", 50.0f);
						float H = GetScriptedAttrib(Attrib, "height", 50.0f);
						string Title = GetScriptedAttrib(Attrib, "title", GetWindowName());

						bool DoLayout = !Attrib.GetAttribute("nolayout", false);

						NuklearAPI.Window(Title, X, Y, W, H, Flags, () => {
							if (DoLayout)
								NuklearAPI.LayoutRowDynamic();

							PaintTags(Tag.Children);
						});
						break;
					}

				case "panel": {
						if (GetScriptedAttrib(Attrib, "disable", false))
							break;

						NkPanelFlags Flags = NkPanelFlags.NoScrollbar;

						float X = GetScriptedAttrib(Attrib, "x", 0.0f);
						float Y = GetScriptedAttrib(Attrib, "y", 0.0f);
						float W = GetScriptedAttrib(Attrib, "width", 50.0f);
						float H = GetScriptedAttrib(Attrib, "height", 50.0f);

						string Title = GetScriptedAttrib(Attrib, "title", GetWindowName());

						byte R = (byte)Attrib.GetAttribute("r", 0.0f);
						byte G = (byte)Attrib.GetAttribute("g", 0.0f);
						byte B = (byte)Attrib.GetAttribute("b", 0.0f);
						byte A = (byte)Attrib.GetAttribute("a", 0.0f);

						bool DoLayout = !Attrib.GetAttribute("nolayout", false);

						Nuklear.nk_style_push_style_item(NuklearAPI.Ctx, &NuklearAPI.Ctx->style.window.fixed_background, Nuklear.nk_style_item_color(new NkColor(R, G, B, A)));
						NuklearAPI.Window(Title, X, Y, W, H, Flags, () => {
							if (DoLayout)
								NuklearAPI.LayoutRowDynamic();

							PaintTags(Tag.Children);
						});
						Nuklear.nk_style_pop_style_item(NuklearAPI.Ctx);
						break;
					}

				case "button": {
						if (GetScriptedAttrib(Attrib, "disable", false))
							break;

						if (NuklearAPI.ButtonLabel(GetScriptedAttrib(Attrib, "text", "Button"))) {
							if (Attrib.GetAttribute("script") is FMLHereDoc OnClickDoc)
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

						// TODO: Stack these so you can recurse
						Nuklear.nk_layout_row_begin(NuklearAPI.Ctx, Layout, GetScriptedAttrib(Attrib, "height", 0.0f), (int)GetScriptedAttrib(Attrib, "columns", 1.0f));
						Nuklear.nk_layout_row_push(NuklearAPI.Ctx, GetScriptedAttrib(Attrib, "row", 1.0f));

						PaintTags(Tag.Children);

						Nuklear.nk_layout_row_end(NuklearAPI.Ctx);
						break;
					}

				case "row": {
						float Pad = GetScriptedAttrib(Attrib, "pad", 0.0f);
						if (Pad > 0) {
							Nuklear.nk_layout_row_push(NuklearAPI.Ctx, Pad);
							NuklearAPI.Label("");
						}

						Nuklear.nk_layout_row_push(NuklearAPI.Ctx, GetScriptedAttrib(Attrib, "width", 1.0f));
						break;
					}

				case "label": {
						string Txt = GetScriptedAttrib(Attrib, "text", "This label is empty");
						string[] Lines = Txt.Split(new[] { '\n' });

						bool Wrap = Attrib.GetAttribute("wrap", false);
						NkTextAlign HAlign = NkTextAlign.NK_TEXT_ALIGN_LEFT;
						NkTextAlign VAlign = NkTextAlign.NK_TEXT_ALIGN_MIDDLE;

						if (Attrib.GetAttribute("left", false))
							HAlign = NkTextAlign.NK_TEXT_ALIGN_LEFT;
						else if (Attrib.GetAttribute("center", false))
							HAlign = NkTextAlign.NK_TEXT_ALIGN_CENTERED;
						else if (Attrib.GetAttribute("right", false))
							HAlign = NkTextAlign.NK_TEXT_ALIGN_RIGHT;

						if (Attrib.GetAttribute("top", false))
							VAlign = NkTextAlign.NK_TEXT_ALIGN_TOP;
						else if (Attrib.GetAttribute("middle", false))
							VAlign = NkTextAlign.NK_TEXT_ALIGN_MIDDLE;
						else if (Attrib.GetAttribute("bottom", false))
							VAlign = NkTextAlign.NK_TEXT_ALIGN_BOTTOM;

						for (int i = 0; i < Lines.Length; i++) {
							if (Wrap)
								NuklearAPI.LabelWrap(Lines[i]);
							else
								NuklearAPI.Label(Lines[i], VAlign | HAlign);
						}

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
