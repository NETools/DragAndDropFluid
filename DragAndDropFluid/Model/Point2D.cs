namespace DragAndDropFluid.Model
{
	public class Point2d(double x, double y)
	{
		public double X { get; set; } = x;
		public double Y { get; set; } = y;

		public override string ToString()
		{
			return $"X: {X}; Y: {Y}";
		}
	}
}
