using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ultraviolet;
using Ultraviolet.Content;
using Ultraviolet.Core;
using Ultraviolet.Core.Text;
using Ultraviolet.Graphics.Graphics2D;
using Ultraviolet.Graphics.Graphics2D.Text;
using Ultraviolet.OpenGL;
using Ultraviolet.Platform;

namespace libTech {
	public class Program : UltravioletApplication {
		static void Main(string[] args) {
			Console.Title = "libTech";

			using (Program P = new Program()) {
				P.Run();
			}
		}

		public Program() : base("Carpmanium", "libTech") {

		}

		protected override UltravioletContext OnCreatingUltravioletContext() {
			OpenGLUltravioletConfiguration Cfg = new OpenGLUltravioletConfiguration();

#if DEBUG
			Cfg.Debug = true;
			Cfg.DebugLevels = DebugLevels.All;
			Cfg.DebugCallback = (UV, Lvl, Msg) => {
				Console.WriteLine(Msg);
			};
#endif


			return new OpenGLUltravioletContext(this, Cfg);
		}

		protected override void OnInitialized() {
			base.OnInitialized();

			IUltravioletPlatform Platform = Ultraviolet.GetPlatform();
			IUltravioletWindow Window = Platform.Windows.FirstOrDefault();

			if (Window != null) 
				Window.Caption = "libTech";	
		}

		protected override void OnDrawing(UltravioletTime time) {
			base.OnDrawing(time);

			IUltravioletGraphics Gfx = Ultraviolet.GetGraphics();
			Gfx.Clear(Color.DarkSlateGray);
		}
	}
}
