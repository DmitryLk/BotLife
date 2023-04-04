using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System;

namespace WindowsFormsApp1
{
	/// <summary>
	/// Обертка над Bitmap для быстрого чтения и изменения пикселов.
	/// Также, класс контролирует выход за пределы изображения: при чтении за границей изображения - возвращает DefaultColor, при записи за границей изображения - игнорирует присвоение.
	/// </summary>
	public class ImageWrapper : IDisposable, IEnumerable<System.Drawing.Point>
	{
		/// <summary>
		/// Ширина изображения
		/// </summary>
		public int _width { get; private set; }
		/// <summary>
		/// Высота изображения
		/// </summary>
		public int _height { get; private set; }
		/// <summary>
		/// Цвет по-умолачнию (используется при выходе координат за пределы изображения)
		/// </summary>
		public Color DefaultColor { get; set; }

		private byte[] data;//буфер исходного изображения
		private byte[] outData;//выходной буфер
		private int stride;
		private bool _useCopy;
		private bool _copySourceToOutput;
		private BitmapData bmpData;
		private Bitmap _bmp;

		/// <summary>
		/// Создание обертки поверх bitmap.
		/// </summary>
		/// <param name="copySourceToOutput">Копирует исходное изображение в выходной буфер</param>
		public ImageWrapper(Bitmap bmp, bool useCopy, bool copySourceToOutput = false)
		{
			_useCopy = useCopy;
			_copySourceToOutput = copySourceToOutput;
			_width = bmp.Width;
			_height = bmp.Height;
			_bmp = bmp;
		}

		public void StartEditing()
		{
			bmpData = _bmp.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			stride = bmpData.Stride;

			//data = new byte[stride * Height];
			//System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, data, 0, data.Length);

			if (_useCopy)
			{
				outData = _copySourceToOutput ? (byte[])data.Clone() : new byte[stride * _height];
			}
		}

		public void EndEditing()
		{
			if (_useCopy)
			{
				System.Runtime.InteropServices.Marshal.Copy(outData, 0, bmpData.Scan0, outData.Length);
			}
			_bmp.UnlockBits(bmpData);
		}

		/// <summary>
		/// Заносит квадрат
		/// </summary>
		public unsafe void FillSquare(int x, int y, int size, Color color)
		{
			byte* curpos;

			if (x >= 0 && x < _width - size && y >= 0 && y < _height - size)
			{
				var ind = x * 4 + y * stride;

				if (_useCopy)
				{
					for (var j = 0; j < size; j++)
					{
						for (var i = 0; i < size; i++)
						{
							outData[ind++] = color.B;
							outData[ind++] = color.G;
							outData[ind++] = color.R;
							outData[ind++] = 255;
						}
						ind += stride - size * 4;
					}
				}
				else
				{
					curpos = ((byte*)bmpData.Scan0) + ind;

					for (var j = 0; j < size; j++)
					{
						for (var i = 0; i < size; i++)
						{
							*(curpos++) = color.B;
							*(curpos++) = color.G;
							*(curpos++) = color.R;
							*(curpos++) = 255;
						}
						curpos += stride - size * 4;
					}
				}
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


		/// <summary>
		/// Заносит в bitmap выходной буфер и снимает лок.
		/// Этот метод обязателен к исполнению (либо явно, лмбо через using)
		/// </summary>
		public void Dispose()
		{
			_bmp.UnlockBits(bmpData);
		}


		int GetIndex(int x, int y)
		{
			return (x < 0 || x >= _width || y < 0 || y >= _height) ? -1 : x * 4 + y * stride;
		}


		/// <summary>
		/// Возвращает пиксел из исходнго изображения.
		/// Либо заносит пиксел в выходной буфер.
		/// </summary>
		public Color this[int x, int y]
		{
			get
			{
				var i = GetIndex(x, y);
				return i < 0 ? DefaultColor : Color.FromArgb(data[i + 3], data[i + 2], data[i + 1], data[i]);
			}

			set
			{
				var i = GetIndex(x, y);
				if (i >= 0)
				{
					outData[i] = value.B;
					outData[i + 1] = value.G;
					outData[i + 2] = value.R;
					outData[i + 3] = value.A;
				};
			}
		}


		/// <summary>
		/// Возвращает пиксел из исходнго изображения.
		/// Либо заносит пиксел в выходной буфер.
		/// </summary>
		public Color this[System.Drawing.Point p]
		{
			get { return this[p.X, p.Y]; }
			set { this[p.X, p.Y] = value; }
		}

		/// <summary>
		/// Заносит в выходной буфер значение цвета, заданные в double.
		/// Допускает выход double за пределы 0-255.
		/// </summary>
		public void SetPixel(System.Drawing.Point p, double r, double g, double b)
		{
			if (r < 0) r = 0;
			if (r >= 256) r = 255;
			if (g < 0) g = 0;
			if (g >= 256) g = 255;
			if (b < 0) b = 0;
			if (b >= 256) b = 255;

			this[p.X, p.Y] = Color.FromArgb((int)r, (int)g, (int)b);
		}


		/// <summary>
		/// Перечисление всех точек изображения
		/// </summary>
		public IEnumerator<System.Drawing.Point> GetEnumerator()
		{
			for (int y = 0; y < _height; y++)
				for (int x = 0; x < _width; x++)
					yield return new System.Drawing.Point(x, y);
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Меняет местами входной и выходной буферы
		/// </summary>
		public void SwapBuffers()
		{
			//var temp = data;
			//data = outData;
			//outData = temp;
		}
	}
}
