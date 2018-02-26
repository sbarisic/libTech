using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;
using System.IO;
using Vector2 = System.Numerics.Vector2;
using Matrix4 = System.Numerics.Matrix4x4;

namespace libTech.Graphics {
	public class ShaderProgram : GraphicsObject {
		List<ShaderStage> ShaderStages;
		Dictionary<string, int> UniformLocations;

		public ShaderProgram() {
			ID = Gl.CreateProgram();

			ShaderStages = new List<ShaderStage>();
			UniformLocations = new Dictionary<string, int>();
		}

		public ShaderProgram(params ShaderStage[] Stages) : this() {
			foreach (var S in Stages)
				AttachShader(S);
			Link();
		}

		public void AttachShader(ShaderStage S) {
			ShaderStages.Add(S);

			Gl.AttachShader(ID, S.ID);
		}

		public bool Link(out string ErrorString) {
#if DEBUG
			SetLabel(ObjectIdentifier.Program, ToString());
#endif

			Gl.LinkProgram(ID);

			Gl.GetProgram(ID, ProgramProperty.LinkStatus, out int Linked);
			if (Linked != Gl.TRUE) {
				StringBuilder Log = new StringBuilder(4096);
				Gl.GetProgramInfoLog(ID, Log.Capacity, out int Len, Log);
				ErrorString = Log.ToString();
				return false;
			}

			string[] UniformKeys = UniformLocations.Keys.ToArray();
			UniformLocations.Clear();

			for (int i = 0; i < UniformKeys.Length; i++)
				GetUniformLocation(UniformKeys[i]);

			// Get some defaults
			GetUniformLocation("Model");
			GetUniformLocation("View");
			GetUniformLocation("Project");

			ErrorString = "";
			return true;
		}

		public void Link() {
			if (!Link(out string ErrorString))
				throw new Exception("Failed to link program\n" + ErrorString);
		}

		/*void UpdateCamera(Camera C) {
			UniformMatrix4f("View", C.View);
			UniformMatrix4f("Project", C.Projection);
		}*/

		public override void Bind() {
			if (Camera.ActiveCamera == null)
				throw new Exception("No active camera");

			bool Dirty = false;
			bool Errors = false;

			foreach (var SS in ShaderStages) {
				if (SS.WatchHandle) {
					SS.WatchHandle.Reset();
					Dirty = true;

					if (!SS.Compile(out string Err)) {
						Errors = true;
						GConsole.WriteLine(Err);
					}
				}
			}

			if (Dirty && !Errors)
				Link();

			SetModelMatrix(Matrix4.Identity);
			Uniform2f("Viewport", Camera.ActiveCamera.ViewportSize);
			UniformMatrix4f("View", Camera.ActiveCamera.View);
			UniformMatrix4f("Project", Camera.ActiveCamera.Projection);

			Gl.UseProgram(ID);
		}

		public override void Unbind() {
			Gl.UseProgram(0);
		}

		public int GetAttribLocation(string Name) {
			return Gl.GetAttribLocation(ID, Name);
		}

		public int GetUniformLocation(string Name) {
			if (UniformLocations.ContainsKey(Name))
				return UniformLocations[Name];

			int Loc = Gl.GetUniformLocation(ID, Name);
			if (Loc != -1)
				UniformLocations.Add(Name, Loc);

			return Loc;
		}

		public void UniformMatrix4f(string Uniform, Matrix4 M, bool Transpose = false) {
			Gl.ProgramUniformMatrix4f(ID, GetUniformLocation(Uniform), 1, Transpose, ref M);
		}

		public void Uniform2f<T>(string Uniform, T Val) where T : struct {
			Gl.ProgramUniform2f(ID, GetUniformLocation(Uniform), 1, ref Val);
		}

		public void SetModelMatrix(Matrix4 M) {
			UniformMatrix4f("Model", M);
		}

		public override void GraphicsDispose() {
			Gl.DeleteProgram(ID);
		}

		public override string ToString() {
			return string.Join(";", ShaderStages);
		}
	}

	public class ShaderStage : GraphicsObject {
		public FileWatchHandle WatchHandle;

		string Source;
		string SrcFile;
		ShaderType ShaderType;


		public ShaderStage(ShaderType T) {
			ID = Gl.CreateShader(T);
			ShaderType = T;
		}

		public ShaderStage(ShaderType T, string SourceFile) : this(T) {
			SetSourceFile(SourceFile);
			Compile();
		}

		public ShaderStage SetSourceCode(string Code) {
			Source = Code;
			SrcFile = null;
			WatchHandle = null;
			return this;
		}

		public ShaderStage SetSourceFile(string FilePath) {
			Source = null;
			SrcFile = Path.GetFullPath(FilePath);
			WatchHandle = FileWatcher.Watch(FilePath);
			return this;
		}

		public bool Compile(out string ErrorString) {
#if DEBUG
			SetLabel(ObjectIdentifier.Shader, ToString());
#endif

			// TODO: Find something better
			if (SrcFile != null) {
				bool Succeeded = false;

				while (!Succeeded) {
					try {
						Source = File.ReadAllText(SrcFile);
						Succeeded = true;
					} catch (Exception) {
					}
				}
			}

			Gl.ShaderSource(ID, new string[] { Source });
			Gl.CompileShader(ID);

			Gl.GetShader(ID, ShaderParameterName.CompileStatus, out int Status);
			if (Status != Gl.TRUE) {
				StringBuilder Log = new StringBuilder(4096);
				Gl.GetShaderInfoLog(ID, Log.Capacity, out int Len, Log);

				if (SrcFile != null)
					ErrorString = SrcFile + "\n" + Log.ToString();
				else
					ErrorString = Log.ToString();
				return false;
			}

			ErrorString = "";
			return true;
		}

		public ShaderStage Compile() {
			if (!Compile(out string ErrorString))
				throw new Exception("Failed to compile shader\n" + ErrorString);

			return this;
		}

		public override void GraphicsDispose() {
			Gl.DeleteShader(ID);
		}

		public override string ToString() {
			if (SrcFile != null)
				return SrcFile;

			return ShaderType.ToString();
		}
	}
}
