using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;
using System.Numerics;
using System.Runtime.InteropServices;

namespace libTech.Graphics {
	public unsafe class Framebuffer : GraphicsObject {
		Dictionary<FramebufferAttachment, Texture> Textures;
		FramebufferTarget Target;

		public bool Multisampled { get; private set; }

		public Framebuffer() {
			ID = Gl.CreateFramebuffer();
			Textures = new Dictionary<FramebufferAttachment, Texture>();
		}

		void AddTexture(FramebufferAttachment Attachment, Texture Tex) {
			if (Textures.Count == 0)
				Multisampled = Tex.Multisampled;
			else if (Textures.Count > 0 && Tex.Multisampled != Multisampled)
				throw new InvalidOperationException("Every attached texture has to have the same multisampling");

			if (Textures.ContainsKey(Attachment))
				Textures.Remove(Attachment);

			Textures.Add(Attachment, Tex);

			Gl.NamedFramebufferTexture(ID, Attachment, Tex.ID, 0);
		}

		Texture GetTexture(FramebufferAttachment Attachment) {
			if (Textures.ContainsKey(Attachment))
				return Textures[Attachment];

			return null;
		}

		public Texture GetColorTexture(int Color = 0) {
			return GetTexture(FramebufferAttachment.ColorAttachment0 + Color);
		}

		public Texture GetDepthTexture() {
			return GetTexture(FramebufferAttachment.DepthAttachment);
		}

		public void AttachColorTexture(Texture Tex, int Color = 0) {
			AddTexture(FramebufferAttachment.ColorAttachment0 + Color, Tex);
		}

		public void AttachDepthTexture(Texture Tex) {
			AddTexture(FramebufferAttachment.DepthAttachment, Tex);
		}

		public void Clear(Vector4? Color = null, int ColorAttachment = 0, float? Depth = null, int? Stencil = null) {
			if (Color != null)
				Gl.ClearNamedFramebuffer(ID, OpenGL.Buffer.Color, ColorAttachment, new float[] { Color.Value.X, Color.Value.Y, Color.Value.Z, Color.Value.W });

			if (Depth != null)
				Gl.ClearNamedFramebuffer(ID, OpenGL.Buffer.Depth, 0, new float[] { Depth.Value });

			if (Stencil != null)
				Gl.ClearNamedFramebuffer(ID, OpenGL.Buffer.Stencil, 0, new int[] { Stencil.Value });
		}

		void BindFramebuffer(FramebufferTarget Target) {
#if DEBUG
			FramebufferStatus S = Gl.CheckNamedFramebufferStatus(ID, Target);
			if (S != FramebufferStatus.FramebufferComplete)
				throw new InvalidOperationException("Incomplete framebuffer");
#endif

			this.Target = Target;
			Gl.BindFramebuffer(Target, ID);
		}

		public override void Bind() {
			BindFramebuffer(FramebufferTarget.Framebuffer);
		}

		public void BindRead() {
			BindFramebuffer(FramebufferTarget.ReadFramebuffer);
		}

		public void BindDraw() {
			BindFramebuffer(FramebufferTarget.DrawFramebuffer);
		}

		public override void Unbind() {
			Gl.BindFramebuffer(Target, 0);
		}

		public override void GraphicsDispose() {
			Gl.DeleteFramebuffers(new uint[] { ID });
		}
	}
}
