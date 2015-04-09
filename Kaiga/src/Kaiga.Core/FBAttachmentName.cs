using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Core
{
	public static class FBAttachmentName
	{
		static readonly public FramebufferAttachment Position = FramebufferAttachment.ColorAttachment0;
		static readonly public FramebufferAttachment Normal = FramebufferAttachment.ColorAttachment1;
		static readonly public FramebufferAttachment Albedo = FramebufferAttachment.ColorAttachment2;
		static readonly public FramebufferAttachment Material = FramebufferAttachment.ColorAttachment3;
		static readonly public FramebufferAttachment Output = FramebufferAttachment.ColorAttachment4;
		static readonly public FramebufferAttachment Post = FramebufferAttachment.ColorAttachment5;
		static readonly public FramebufferAttachment AO = FramebufferAttachment.ColorAttachment6;
	}

	public static class DrawBufferName
	{
		static readonly public DrawBuffersEnum Position = DrawBuffersEnum.ColorAttachment0;
		static readonly public DrawBuffersEnum Normal = DrawBuffersEnum.ColorAttachment1;
		static readonly public DrawBuffersEnum Albedo = DrawBuffersEnum.ColorAttachment2;
		static readonly public DrawBuffersEnum Material = DrawBuffersEnum.ColorAttachment3;
		static readonly public DrawBuffersEnum Output = DrawBuffersEnum.ColorAttachment4;
		static readonly public DrawBuffersEnum Post = DrawBuffersEnum.ColorAttachment5;
		static readonly public DrawBuffersEnum AO = DrawBuffersEnum.ColorAttachment6;
	}
}

