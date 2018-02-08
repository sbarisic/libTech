﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;
using System.IO;

namespace libTech.Graphics {
	public class ShaderProgram : GraphicsObject {
		List<ShaderStage> ShaderStages;

		public ShaderProgram() {
			ID = Gl.CreateProgram();

			ShaderStages = new List<ShaderStage>();
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

			ErrorString = "";
			return true;
		}

		public void Link() {
			if (!Link(out string ErrorString))
				throw new Exception("Failed to link program\n" + ErrorString);
		}

		public override void Bind() {
			Gl.UseProgram(ID);
		}

		public override void Unbind() {
			Gl.UseProgram(0);
		}

		public int GetAttribLocation(string Name) {
			return Gl.GetAttribLocation(ID, Name);
		}

		public override void GraphicsDispose() {
			Gl.DeleteProgram(ID);
		}

		public override string ToString() {
			return string.Join(";", ShaderStages);
		}
	}

	public class ShaderStage : GraphicsObject {
		string Source;
		string SrcFile;
		ShaderType ShaderType;

		public ShaderStage(ShaderType T) {
			ID = Gl.CreateShader(T);
			ShaderType = T;
		}

		public void SetSourceCode(string Code) {
			Source = Code;
			SrcFile = null;
		}

		public void SetSourceFile(string FilePath) {
			Source = File.ReadAllText(FilePath);
			SrcFile = Path.GetFullPath(FilePath);
		}

		public bool Compile(out string ErrorString) {
#if DEBUG
			SetLabel(ObjectIdentifier.Shader, ToString());
#endif

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

		public void Compile() {
			if (!Compile(out string ErrorString))
				throw new Exception("Failed to compile shader\n" + ErrorString);
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
