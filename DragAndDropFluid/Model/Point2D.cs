namespace DragAndDropFluid.Model
{
	public class Point2d(float x, float y)
	{
		public float X { get; set; } = x;
		public float Y { get; set; } = y;

		public override string ToString()
		{
			return $"X: {X}; Y: {Y}";
		}
	}
}
