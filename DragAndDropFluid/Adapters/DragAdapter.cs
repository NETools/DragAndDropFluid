using DragAndDropFluid.Misc;
using DragAndDropFluid.Model;
using DragAndDropFluid.Pages.Views;
using System.ComponentModel;

namespace DragAndDropFluid.Adapters
{
	public class DragAdapter(DragContainerView view, PlaceholderView placeholder)
	{
		private DragableItemView? _currentlySelectedItem;
		private int _currentlySelectedIndex = -1;

		public void DragStart(int selectedIndex)
		{
			if (_currentlySelectedItem != null)
				return;

			_currentlySelectedIndex = selectedIndex;
			_currentlySelectedItem = view.DragableItems[_currentlySelectedIndex];

			placeholder.CopyFrom(_currentlySelectedItem);
			placeholder.ToggleVisibility();
		}

		public void DragMove(double mx, double my)
		{
			if (_currentlySelectedItem == null)
				return;

			_currentlySelectedItem.Position.X += mx;
			_currentlySelectedItem.Position.Y += my;

			var (Column, Row) = Toolkit.CalculateIndices(_currentlySelectedItem.Position, _currentlySelectedItem.Size, 0.5);

			var (NewRow, IsAdjusted) = CorrectRow(Column, Row);

			if (!IsAdjusted)
			{
				if (placeholder.Column != Column)
				{
					OnColumnChanged(placeholder.Column, Column);
				}
				if (placeholder.Row != Row)
				{
					OnRowChanged(placeholder.Row, Row);
				}
			}

			Row = NewRow;

			placeholder.Column = Column;
			placeholder.Row = Row;

			CheckCollision();
		}

		private (int NewRow, bool IsAdjusted) CorrectRow(int column, int row)
		{
			var columnElements = view.DragableItems.FindAll(p => p.Column == column && p.Index != _currentlySelectedIndex).OrderBy(p=>p.Row).ToList();

			if (columnElements.Count == 0)
			{
				return (0, false);
			}

			var lastRow = columnElements[columnElements.Count - 1].Row;

			if (lastRow >= row)
			{
				return (row, false);
			}

			return (lastRow + 1, true);
		}

		private void HandleItemsInNewColumn(int newColumn)
		{
			var columnElements = view.DragableItems.FindAll(p => p.Column == newColumn && p.Row >= placeholder.Row).OrderBy(p => p.Row).ToList();

			if (columnElements.Count != 0)
			{
				for (int i = 0; i < columnElements.Count - 1; i++)
				{
					var current = columnElements[i];
					var below = columnElements[i + 1];

					if (current.Index == _currentlySelectedIndex)
						continue;

					current.Position.Y = below.Position.Y;
					current.Row = below.Row;

				}

				var last = columnElements[columnElements.Count - 1];

				if (last.Index != _currentlySelectedIndex)
				{
					last.Position.Y += last.Size.Y;
					last.Row += 1;
				}

			}
		}

		private void HandleItemsInOldColumn(int oldColumn)
		{
			var oldColumnElements = view.DragableItems.FindAll(p => p.Column == oldColumn && p.Row > placeholder.Row).OrderBy(p => p.Row).ToList();

			if (oldColumnElements.Count == 0)
				return;

			for (int i = 1; i < oldColumnElements.Count; i++)
			{
				var above = oldColumnElements[i - 1];
				var current = oldColumnElements[i];

				if (current.Index == _currentlySelectedIndex)
					continue;

				current.Position.Y = above.Position.Y;
				current.Row = above.Row;
			}

			var first = oldColumnElements[0];

			if (first.Index != _currentlySelectedIndex)
			{
				first.Position.Y -= first.Size.Y;
				first.Row -= 1;
			}

		}

		private void OnColumnChanged(int oldColumn, int newColumn)
		{
			HandleItemsInNewColumn(newColumn);
			HandleItemsInOldColumn(oldColumn);
		}

		private void HandleAscendingRow(int oldRow, int newRow)
		{
			var itemAbove = view.DragableItems.Find(p => p.Row == newRow && p.Index != _currentlySelectedItem.Index && p.Column == placeholder.Column);
			if (itemAbove == null)
				return;

			itemAbove.Position.Y = oldRow * itemAbove.Size.Y;
			itemAbove.Row = oldRow;
		}

		private void HandleDescendingRow(int oldRow)
		{
			var itemBelow = view.DragableItems.Find(p => p.Row == (oldRow + 1) && p.Index != _currentlySelectedItem.Index && p.Column == placeholder.Column);
			if (itemBelow == null)
				return;

			itemBelow.Position.Y = oldRow * itemBelow.Size.Y;
			itemBelow.Row = oldRow;
		}

		private void OnRowChanged(int oldRow, int newRow)
		{
			if (newRow < oldRow)
			{
				HandleAscendingRow(oldRow, newRow);
			}
			else
			{
				HandleDescendingRow(oldRow);
			}
		}

		private void CheckCollision()
		{
			var (X, Y) = Toolkit.CalculateCoordinates((placeholder.Column, placeholder.Row), _currentlySelectedItem.Size);

			var clientRect = Toolkit.CreateRectangle(new Point2d(X, Y), placeholder.Size);

			bool hit = false;
			for (int i = 0; i < view.DragableItems.Count; i++)
			{
				var item = view.DragableItems[i];

				if (item.Index == _currentlySelectedIndex)
					continue;

				var itemRect = Toolkit.CreateRectangle(item.Position, item.Size);
				if (itemRect.IntersectsWith(clientRect))
				{
					hit = true;
					break;
				}
			}

			if (!hit)
			{
				placeholder.Position.X = X;
				placeholder.Position.Y = Y;

				placeholder.Update();
			}
		}


		public void Drop()
		{
			if (_currentlySelectedItem == null)
			{
				return;
			}

			placeholder.ToggleVisibility();

			_currentlySelectedItem.Position.X = placeholder.Position.X;
			_currentlySelectedItem.Position.Y = placeholder.Position.Y;

			_currentlySelectedItem.Column = placeholder.Column;
			_currentlySelectedItem.Row = placeholder.Row;

			_currentlySelectedIndex = -1;
			_currentlySelectedItem = null;

		}
	}
}
