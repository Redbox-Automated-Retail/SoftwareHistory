using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace x2de5aa54b5fc5109;

internal class x7d8d2ed69d304345
{
	private struct xb8c0e9aa6e5461e0
	{
		public short xccda1407ab22cc33;

		public short xf3035cfdeddb8039;

		public ushort x9948f71afde4bb2c;

		public void x06b0e25aa6ad68a9(BinaryReader xb55b340ae3a3e4e0)
		{
			xccda1407ab22cc33 = xb55b340ae3a3e4e0.ReadInt16();
			xf3035cfdeddb8039 = xb55b340ae3a3e4e0.ReadInt16();
			x9948f71afde4bb2c = xb55b340ae3a3e4e0.ReadUInt16();
		}

		public void x8d81a984b7f0332e(BinaryWriter x9d007d79b26e5331)
		{
			x9d007d79b26e5331.Write(xccda1407ab22cc33);
			x9d007d79b26e5331.Write(xf3035cfdeddb8039);
			x9d007d79b26e5331.Write(x9948f71afde4bb2c);
		}
	}

	private struct x09e194c3ff44676a
	{
		public byte xfa7dbc0eccccbeb4;

		public byte x6217af5b397ce969;

		public byte x7a6944b5e303f3ed;

		public byte x15b3f8048b984758;

		public short xaa162b45459eeb26;

		public short x93d93a2918659f9f;

		public int x0999f16d62fab164;

		public int x5cd99d26db5ad682;

		public void x06b0e25aa6ad68a9(BinaryReader xb55b340ae3a3e4e0)
		{
			xfa7dbc0eccccbeb4 = xb55b340ae3a3e4e0.ReadByte();
			x6217af5b397ce969 = xb55b340ae3a3e4e0.ReadByte();
			x7a6944b5e303f3ed = xb55b340ae3a3e4e0.ReadByte();
			x15b3f8048b984758 = xb55b340ae3a3e4e0.ReadByte();
			xaa162b45459eeb26 = xb55b340ae3a3e4e0.ReadInt16();
			x93d93a2918659f9f = xb55b340ae3a3e4e0.ReadInt16();
			x0999f16d62fab164 = xb55b340ae3a3e4e0.ReadInt32();
			x5cd99d26db5ad682 = xb55b340ae3a3e4e0.ReadInt32();
		}

		public void x8d81a984b7f0332e(BinaryWriter x9d007d79b26e5331)
		{
			x9d007d79b26e5331.Write(xfa7dbc0eccccbeb4);
			x9d007d79b26e5331.Write(x6217af5b397ce969);
			x9d007d79b26e5331.Write(x7a6944b5e303f3ed);
			x9d007d79b26e5331.Write(x15b3f8048b984758);
			x9d007d79b26e5331.Write(xaa162b45459eeb26);
			x9d007d79b26e5331.Write(x93d93a2918659f9f);
			x9d007d79b26e5331.Write(x0999f16d62fab164);
			x9d007d79b26e5331.Write(x5cd99d26db5ad682);
		}
	}

	private struct x427414780a515181
	{
		public bool x1c962f245a4cc107;

		public int x2949b6f2ca095d22;

		public int x1ee4efc1758e7a60;

		public IntPtr x497cac8750df2865;

		public IntPtr xff73e9350ab314ad;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	private struct x794c1ff18db268b8
	{
		public int x11973ad34d8be202;

		public int xf3c006205b66ef7b;

		public int xfa6ea744155689ad;

		public int x77b193b0c5b926ef;

		public short xdf0cf91496cdbd62;

		public short x1c04732f63de1934;

		public IntPtr x71d244b6916f28a3;
	}

	private struct x3c6db2a2a39d4e14
	{
		public uint x440c29b38a78cb31;

		public int xb7f80632423c1af0;

		public int xf9a5116b5085d07c;

		public ushort x6d3a982953818629;

		public ushort xc2300ec77e85d258;

		public uint x855558f59cd84b5c;

		public uint xf8f23a12d493170a;

		public int x5339f9cd1f6b1b8b;

		public int x3cfbfef6f917c767;

		public uint xf5358965bce830a8;

		public uint x991bafaf8cee1b0d;
	}

	private Hashtable xbb30c52680b7a16a;

	private Hashtable x2edae83a7c3180bb;

	private xdc8e11111d3c00f8[] x0e7883a5ad2e094a;

	public x7d8d2ed69d304345(Stream s)
	{
		using BinaryReader xb55b340ae3a3e4e = new BinaryReader(s);
		xb8c0e9aa6e5461e0 xb8c0e9aa6e5461e1 = default(xb8c0e9aa6e5461e0);
		xb8c0e9aa6e5461e1.x06b0e25aa6ad68a9(xb55b340ae3a3e4e);
		if (xb8c0e9aa6e5461e1.xccda1407ab22cc33 != 0 || xb8c0e9aa6e5461e1.xf3035cfdeddb8039 != 1)
		{
			throw new Exception("The specified stream did not contain a valid icon.");
		}
		x09e194c3ff44676a x09e194c3ff44676a2 = default(x09e194c3ff44676a);
		checked
		{
			x0e7883a5ad2e094a = new xdc8e11111d3c00f8[(uint)unchecked((int)xb8c0e9aa6e5461e1.x9948f71afde4bb2c)];
			xbb30c52680b7a16a = new Hashtable();
			x2edae83a7c3180bb = new Hashtable();
		}
		for (int i = 0; i < xb8c0e9aa6e5461e1.x9948f71afde4bb2c; i++)
		{
			x09e194c3ff44676a2.x06b0e25aa6ad68a9(xb55b340ae3a3e4e);
			ref xdc8e11111d3c00f8 reference = ref x0e7883a5ad2e094a[i];
			reference = new xdc8e11111d3c00f8(new Size(x09e194c3ff44676a2.xfa7dbc0eccccbeb4, x09e194c3ff44676a2.x6217af5b397ce969), x1eb5908432d6e3bd(x09e194c3ff44676a2.x93d93a2918659f9f * x09e194c3ff44676a2.xaa162b45459eeb26));
			long position = s.Position;
			s.Position = x09e194c3ff44676a2.x5cd99d26db5ad682;
			byte[] array = new byte[checked((uint)x09e194c3ff44676a2.x0999f16d62fab164)];
			s.Read(array, 0, x09e194c3ff44676a2.x0999f16d62fab164);
			xbb30c52680b7a16a[x0e7883a5ad2e094a[i]] = array;
			x2edae83a7c3180bb[x0e7883a5ad2e094a[i]] = x09e194c3ff44676a2;
			s.Position = position;
		}
	}

	private ColorDepth x1eb5908432d6e3bd(int x4b8b317913377903)
	{
		return x4b8b317913377903 switch
		{
			4 => ColorDepth.Depth4Bit, 
			8 => ColorDepth.Depth8Bit, 
			16 => ColorDepth.Depth16Bit, 
			24 => ColorDepth.Depth24Bit, 
			32 => ColorDepth.Depth32Bit, 
			_ => ColorDepth.Depth4Bit, 
		};
	}

	public Icon xc35a870ec8a128b3(xdc8e11111d3c00f8 x29e1d2ee8eb66cb9)
	{
		byte[] array = (byte[])xbb30c52680b7a16a[x29e1d2ee8eb66cb9];
		Icon result = null;
		if (array != null)
		{
			Stream stream = new MemoryStream();
			BinaryWriter x9d007d79b26e = new BinaryWriter(stream);
			xb8c0e9aa6e5461e0 xb8c0e9aa6e5461e1 = default(xb8c0e9aa6e5461e0);
			xb8c0e9aa6e5461e1.x9948f71afde4bb2c = 1;
			xb8c0e9aa6e5461e1.xf3035cfdeddb8039 = 1;
			xb8c0e9aa6e5461e1.x8d81a984b7f0332e(x9d007d79b26e);
			x09e194c3ff44676a x09e194c3ff44676a2 = (x09e194c3ff44676a)x2edae83a7c3180bb[x29e1d2ee8eb66cb9];
			x09e194c3ff44676a2.x5cd99d26db5ad682 = Marshal.SizeOf(typeof(xb8c0e9aa6e5461e0)) + Marshal.SizeOf(typeof(x09e194c3ff44676a));
			x09e194c3ff44676a2.x8d81a984b7f0332e(x9d007d79b26e);
			stream.Write(array, 0, array.Length);
			stream.Position = 0L;
			result = new Icon(stream, x09e194c3ff44676a2.xfa7dbc0eccccbeb4, x09e194c3ff44676a2.x6217af5b397ce969);
		}
		return result;
	}

	public Icon xc35a870ec8a128b3(Size x0ceec69a97f73617)
	{
		xdc8e11111d3c00f8 x29e1d2ee8eb66cb = x21dc1ae9e92ff3a5(x0ceec69a97f73617);
		if (!(x29e1d2ee8eb66cb.x437e3b626c0fdd43 != Size.Empty))
		{
			return null;
		}
		return xc35a870ec8a128b3(x29e1d2ee8eb66cb);
	}

	public Bitmap xd2b1a4678f6c35f4(xdc8e11111d3c00f8 x29e1d2ee8eb66cb9)
	{
		return xd2b1a4678f6c35f4(xc35a870ec8a128b3(x29e1d2ee8eb66cb9), x29e1d2ee8eb66cb9);
	}

	public static Bitmap xd2b1a4678f6c35f4(Icon x8546e7d3d8c4f973, xdc8e11111d3c00f8 x29e1d2ee8eb66cb9)
	{
		Bitmap bitmap = null;
		if (x8546e7d3d8c4f973 != null)
		{
			if (x29e1d2ee8eb66cb9.x94af564f5d0fedbf == ColorDepth.Depth32Bit)
			{
				GetIconInfo(x8546e7d3d8c4f973.Handle, out var piconinfo);
				x794c1ff18db268b8 lpObject = default(x794c1ff18db268b8);
				GetObjectBitmap(piconinfo.xff73e9350ab314ad, Marshal.SizeOf(typeof(x794c1ff18db268b8)), ref lpObject);
				int num = lpObject.x77b193b0c5b926ef * lpObject.xfa6ea744155689ad;
				checked
				{
					int[] array = new int[(uint)unchecked(num / 4)];
					GetBitmapBits(piconinfo.xff73e9350ab314ad, num, array);
					GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
					Bitmap bitmap2 = new Bitmap(lpObject.xf3c006205b66ef7b, lpObject.xfa6ea744155689ad, lpObject.x77b193b0c5b926ef, PixelFormat.Format32bppArgb, Marshal.UnsafeAddrOfPinnedArrayElement(array, 0));
					bitmap = new Bitmap(bitmap2.Width, bitmap2.Height);
					using (Graphics graphics = Graphics.FromImage(bitmap))
					{
						graphics.DrawImage(bitmap2, 0, 0, bitmap2.Width, bitmap2.Height);
					}
					gCHandle.Free();
					DeleteObject(piconinfo.x497cac8750df2865);
					DeleteObject(piconinfo.xff73e9350ab314ad);
				}
			}
			else
			{
				bitmap = x8546e7d3d8c4f973.ToBitmap();
			}
		}
		return bitmap;
	}

	public Bitmap xd2b1a4678f6c35f4(Size x0ceec69a97f73617)
	{
		xdc8e11111d3c00f8 x29e1d2ee8eb66cb = x21dc1ae9e92ff3a5(x0ceec69a97f73617);
		if (!(x29e1d2ee8eb66cb.x437e3b626c0fdd43 != Size.Empty))
		{
			return null;
		}
		return xd2b1a4678f6c35f4(x29e1d2ee8eb66cb);
	}

	private xdc8e11111d3c00f8 x21dc1ae9e92ff3a5(Size x0ceec69a97f73617)
	{
		xdc8e11111d3c00f8 result = new xdc8e11111d3c00f8(Size.Empty, ColorDepth.Depth4Bit);
		xdc8e11111d3c00f8[] array = x0e7883a5ad2e094a;
		for (int i = 0; i < array.Length; i++)
		{
			xdc8e11111d3c00f8 xdc8e11111d3c00f9 = array[i];
			if (xdc8e11111d3c00f9.x437e3b626c0fdd43 == x0ceec69a97f73617 && xdc8e11111d3c00f9.x94af564f5d0fedbf >= result.x94af564f5d0fedbf)
			{
				result = xdc8e11111d3c00f9;
			}
		}
		return result;
	}

	public xdc8e11111d3c00f8[] xdbe5f635faa12ead()
	{
		return (xdc8e11111d3c00f8[])x0e7883a5ad2e094a.Clone();
	}

	[DllImport("gdi32", CharSet = CharSet.Auto, EntryPoint = "GetObject")]
	private static extern int GetObjectBitmap(IntPtr hObject, int nCount, ref x794c1ff18db268b8 lpObject);

	[DllImport("gdi32")]
	private static extern int GetBitmapBits(IntPtr hBitmap, int dwCount, int[] lpBits);

	[DllImport("user32")]
	private static extern int GetIconInfo(IntPtr hIcon, out x427414780a515181 piconinfo);

	[DllImport("gdi32")]
	private static extern int DeleteObject(IntPtr hObject);
}
