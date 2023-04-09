using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System;

namespace WindowsFormsApp1
{
	public enum BitmapCopyType
	{
		EditDirectlyScreenBitmap_Fastest = 1,
		EditCopyScreenBitmap = 2,
		EditEmptyArray = 3,
		EditCopyScreenBitmapWithAdditionalArray = 4
	}

	/// <summary>
	/// Обертка над Bitmap для быстрого чтения и изменения пикселов.
	/// Также, класс контролирует выход за пределы изображения: при чтении за границей изображения - возвращает DefaultColor, при записи за границей изображения - игнорирует присвоение.
	/// </summary>
	public class ImageWrapper : IDisposable, IEnumerable<System.Drawing.Point>
	{
		public int _width;
		public int _height;
		//public Color DefaultColor;

		private byte[] _editArray;   //массив для редактирования
		private byte[] _mainScreenBufferWithoutLens;// промежуточный экран

		private int _stride;
		private BitmapData _bmpData;
		private Bitmap _bmp;
		private int _length;
		private bool _useEditArray;
		private bool _editing;
		BitmapCopyType _type;


		/// <summary>
		/// Создание обертки поверх bitmap.
		/// </summary>
		/// <param name="copySourceToOutput">Копирует исходное изображение в выходной буфер</param>
		public ImageWrapper(Bitmap bmp, BitmapCopyType type)
		{
			_type = type;
			_width = bmp.Width;
			_height = bmp.Height;
			_bmp = bmp;
			_length = _width * _height * 4;
			_stride = _width * 4;
			_editing = false;

			_useEditArray =
				_type == BitmapCopyType.EditCopyScreenBitmap ||
				_type == BitmapCopyType.EditEmptyArray ||
				_type == BitmapCopyType.EditCopyScreenBitmapWithAdditionalArray;

			_editArray = new byte[_length];
			_mainScreenBufferWithoutLens = new byte[_length];
			//if (_useEditArray)
			//{
			//	editArray = new byte[_length];
			//}

			//if (_type == BitmapCopyType.EditCopyScreenBitmapWithAdditionalArray)
			//{
			//	mainScreenBufferWithoutLens = new byte[_length];
			//}
		}

		public void ChangeMode(BitmapCopyType type)
		{
			if (_editing) throw new Exception("public void ChangeMode(BitmapCopyType type)");

			if (type == BitmapCopyType.EditCopyScreenBitmapWithAdditionalArray)
			{
				//скопировать экран без дополнительной графики в массив _mainScreenBufferWithoutLens
				_bmpData = _bmp.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
				System.Runtime.InteropServices.Marshal.Copy(_bmpData.Scan0, _mainScreenBufferWithoutLens, 0, _length);
				_bmp.UnlockBits(_bmpData);
			}
			_type = type;
		}

		public void StartEditing()
		{
			_editing = true;

			if (_type == BitmapCopyType.EditDirectlyScreenBitmap_Fastest)
			{
				_bmpData = _bmp.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
				//_stride = _bmpData.Stride;
			}

			if (_type == BitmapCopyType.EditCopyScreenBitmap)
			{
				_bmpData = _bmp.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
				// скопировать экранный битмап в массив для редактирования
				System.Runtime.InteropServices.Marshal.Copy(_bmpData.Scan0, _editArray, 0, _length);
			}

			if (_type == BitmapCopyType.EditEmptyArray)
			{
				// заново бсолютно всё рисуем на пустом массиве для редактирования
				_editArray = new byte[_stride * _height];
			}

			if (_type == BitmapCopyType.EditCopyScreenBitmapWithAdditionalArray)
			{
				// скопировать из массива экрана без дополнительных рисунков в массив для редактирования
				Buffer.BlockCopy(_mainScreenBufferWithoutLens, 0, _editArray, 0, _length);
			}
		}

		// После отрисовки всех ботов сохранить образец и продолжить рисовать на буфере редиктирования дополнительную графику
		public void IntervalEditing()
		{
			if (_type == BitmapCopyType.EditCopyScreenBitmapWithAdditionalArray)
			{
				// скопировать из массива экрана без дополнительных рисунков в буфер редактирования
				Buffer.BlockCopy(_editArray, 0, _mainScreenBufferWithoutLens, 0, _length);
			}
		}

		public void EndEditing()
		{
			if (_type == BitmapCopyType.EditDirectlyScreenBitmap_Fastest)
			{
				_bmp.UnlockBits(_bmpData);
			}

			if (_type == BitmapCopyType.EditCopyScreenBitmap)
			{
				// скопировать массив в экранный битмап
				System.Runtime.InteropServices.Marshal.Copy(_editArray, 0, _bmpData.Scan0, _editArray.Length);
				_bmp.UnlockBits(_bmpData);
			}

			if (_type == BitmapCopyType.EditEmptyArray || _type == BitmapCopyType.EditCopyScreenBitmapWithAdditionalArray)
			{
				_bmpData = _bmp.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
				// скопировать массив в экранный битмап
				System.Runtime.InteropServices.Marshal.Copy(_editArray, 0, _bmpData.Scan0, _editArray.Length);
				_bmp.UnlockBits(_bmpData);
			}
			_editing = false;
		}

		/// <summary>
		/// Заносит квадрат
		/// </summary>
		public unsafe void FillSquare(int x, int y, int size, Color color)
		{
			byte* curpos;

			if (x >= 0 && x < _width - size && y >= 0 && y < _height - size)
			{
				var ind = x * 4 + y * _stride;

				if (_useEditArray)
				{
					for (var j = 0; j < size; j++)
					{
						for (var i = 0; i < size; i++)
						{
							_editArray[ind++] = color.B;
							_editArray[ind++] = color.G;
							_editArray[ind++] = color.R;
							_editArray[ind++] = 255;
						}
						ind += _stride - size * 4;
					}
				}
				else
				{
					curpos = ((byte*)_bmpData.Scan0) + ind;

					for (var j = 0; j < size; j++)
					{
						for (var i = 0; i < size; i++)
						{
							*(curpos++) = color.B;
							*(curpos++) = color.G;
							*(curpos++) = color.R;
							*(curpos++) = 255;
						}
						curpos += _stride - size * 4;
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
			_bmp.UnlockBits(_bmpData);
		}


		//int GetIndex(int x, int y)
		//{
		//	return (x < 0 || x >= _width || y < 0 || y >= _height) ? -1 : x * 4 + y * _stride;
		//}


		///// <summary>
		///// Возвращает пиксел из исходнго изображения.
		///// Либо заносит пиксел в выходной буфер.
		///// </summary>
		//public Color this[int x, int y]
		//{
		//	get
		//	{
		//		var i = GetIndex(x, y);
		//		return i < 0 ? DefaultColor : Color.FromArgb(data[i + 3], data[i + 2], data[i + 1], data[i]);
		//	}

		//	set
		//	{
		//		var i = GetIndex(x, y);
		//		if (i >= 0)
		//		{
		//			outData[i] = value.B;
		//			outData[i + 1] = value.G;
		//			outData[i + 2] = value.R;
		//			outData[i + 3] = value.A;
		//		};
		//	}
		//}


		///// <summary>
		///// Возвращает пиксел из исходнго изображения.
		///// Либо заносит пиксел в выходной буфер.
		///// </summary>
		//public Color this[System.Drawing.Point p]
		//{
		//	get { return this[p.X, p.Y]; }
		//	set { this[p.X, p.Y] = value; }
		//}

		///// <summary>
		///// Заносит в выходной буфер значение цвета, заданные в double.
		///// Допускает выход double за пределы 0-255.
		///// </summary>
		//public void SetPixel(System.Drawing.Point p, double r, double g, double b)
		//{
		//	if (r < 0) r = 0;
		//	if (r >= 256) r = 255;
		//	if (g < 0) g = 0;
		//	if (g >= 256) g = 255;
		//	if (b < 0) b = 0;
		//	if (b >= 256) b = 255;

		//	this[p.X, p.Y] = Color.FromArgb((int)r, (int)g, (int)b);
		//}


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

		///// <summary>
		///// Меняет местами входной и выходной буферы
		///// </summary>
		//public void SwapBuffers()
		//{
		//	//var temp = data;
		//	//data = outData;
		//	//outData = temp;
		//}
	}
}
