using System;

/// <summary>
/// A struct for a cell, which is simply a (row,column) pair.
/// </summary>
public struct Cell
{
	/// <summary>
	/// The row of the cell in its parent grid.
	/// </summary>
	public int row;
	/// <summary>
	/// The column of the cell in its parent grid.
	/// </summary>
	public int column;
	
	/// <summary>
	/// Creates a Cell struct at the given (row,column) coordinates
	/// </summary>
	public Cell(int row, int column)
	{
		this.row = row;
		this.column = column;
	}

	/// <summary>
	/// Creates a cell with an identical row and column as the given Cell.
	/// </summary>
	public Cell(Cell other)
	{
		this.row = other.row;
		this.column = other.column;
	}

	/// <summary>
	/// Set the coordinates of the cell
	/// </summary>
	public void Set(int row, int column)
	{
		this.row = row;
		this.column = column;
	}
	
}

