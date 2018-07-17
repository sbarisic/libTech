using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FishGfx.Graphics;

namespace libTech.Graphics {
	public static class DefaultShaders {
		static DefaultShaders() {
			Text2D = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/default2d.vert"),
				new ShaderStage(ShaderType.FragmentShader, "content/shaders/default_fullbright_color.frag"));
		}

		public static ShaderProgram Text2D;
	}

	public static class DefaultFonts {
		static DefaultFonts() {
			ConsoleFont = new FreetypeFont("content/fonts/Hack.ttf", 16);

			MainMenuSmall = new FreetypeFont("content/fonts/Hack.ttf", 16);
			MainMenuMedium = new FreetypeFont("content/fonts/Hack.ttf", 22);
			MainMenuBig = new FreetypeFont("content/fonts/Hack.ttf", 32);
		}

		public static FreetypeFont ConsoleFont;
		public static FreetypeFont MainMenuSmall;
		public static FreetypeFont MainMenuMedium;
		public static FreetypeFont MainMenuBig;
	}

	public static class DefaultTextures {
		static DefaultTextures() {
			Panel = Texture.FromFile("content/textures/gui/panel.png");

			Button = Texture.FromFile("content/textures/gui/button.png");
			ButtonHover = Texture.FromFile("content/textures/gui/button_hover.png");
			ButtonClick = Texture.FromFile("content/textures/gui/button_click.png");
		}

		public static Texture Panel;
		public static Texture Button;
		public static Texture ButtonHover;
		public static Texture ButtonClick;
	}
}
