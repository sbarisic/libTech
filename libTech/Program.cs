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

using libTech.Reflection;
using libTech.Importer;
using System.Reflection;

namespace libTech {
	public class IntImporter : Importer<int> {
		public override bool CanLoadExt(string Extension) {
			if (Extension == ".int")
				return true;
			return false;
		}

		public override int Load(string FilePath) {
			return 1;
		}
	}

	public class Int2Importer : Importer<int> {
		public override bool CanLoadExt(string Extension) {
			if (Extension == ".int2")
				return true;
			return false;
		}

		public override int Load(string FilePath) {
			return 2;
		}
	}

	public class Program : UltravioletApplication {
		static void Main(string[] args) {
			Console.Title = "libTech";

			using (Program P = new Program()) {
				P.Preinitialize();
				P.Run();
			}
		}

		public Program() : base("Carpmanium", "libTech") {
		}

		public void Preinitialize() {
			foreach (var Type in Reflect.GetAllTypes(Reflect.GetExeAssembly()))
				if (Reflect.Inherits(Type, typeof(Importer.Importer)) && !(Type == typeof(Importer<>)))
					Importers.Register(Type);
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
