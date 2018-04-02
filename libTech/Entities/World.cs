using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libTech.Graphics;
using libTech.Importer;
using System.IO;
using System.Numerics;

namespace libTech.Entities {
	public class World : PhysicsEntity {
		public Model WorldModel;
		public List<Vector3> RelativeSpawns;

		public World(string DefFile) {
			Load(DefFile);

			List<Vector3> Verts = new List<Vector3>();
			foreach (var Msh in WorldModel.Meshes)
				if (Msh.Solid)
					Verts.AddRange(Msh.Vertices);

			CreateStaticMesh(Verts);
		}

		void Load(string DefFile) {
			string[] Lines = File.ReadAllLines(DefFile);
			RelativeSpawns = new List<Vector3>();

			foreach (var L in Lines) {
				string[] Args = L.Trim().Split(' ');

				switch (Args[0].ToLower()) {
					case "model":
						WorldModel = Importers.Load<Model>(Args[1]);
						break;

					case "spawn":
						RelativeSpawns.Add(new Vector3(Args[1].ParseToFloat(), Args[2].ParseToFloat(), Args[3].ParseToFloat()));
						break;

					default:
						GConsole.WriteLine("Invalid world argument: {0}", L);
						break;
				}
			}

			if (WorldModel == null)
				throw new Exception("World model was not loaded");
		}

		public override void Update(float Dt) {
			WorldModel.Position = Position;
		}

		public override void Draw(float Dt) {
			WorldModel.Draw();
		}

		public override void DrawTransparent(float Dt) {
			WorldModel.DrawTransparent();
		}
	}
}
