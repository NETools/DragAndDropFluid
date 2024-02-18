using DragAndDropFluid.Model;
using System;
using System.Drawing;

namespace DragAndDropFluid.Misc
{
	public static class Toolkit
	{
		public static (int Column, int Row) CalculateIndices(Point2d position, Point2d size, double scale)
		{
			double gridWidth = size.X * scale;
			double gridHeight = size.Y * scale;

			var column = (int)((position.X + gridWidth) / size.X);
			var row = (int)((position.Y + gridHeight) / size.Y);

			return (column, row);
		}

		public static (double X, double Y) CalculateCoordinates((int Column, int Row) indices, Point2d size)
		{
			var x = indices.Column * size.X;
			var y = indices.Row * size.Y;

			return (x, y);
		}

		public static RectangleF CreateRectangle(Point2d leftTop, Point2d size)
		{
			return new RectangleF((float)leftTop.X, (float)leftTop.Y, (float)size.X, (float)size.Y);
		}

	}
}
