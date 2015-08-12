using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Denotes a cell that is part of the level grid. 
/// </summary> 
[System.Serializable]
public class LevelCell
{
	/// <summary>
	/// The row where the cell is placed in the level grid.
	/// </summary>
	private int row;
	/// <summary>
	/// The column where the cell is placed in the level grid.
	/// </summary>
	private int column;
	
	/// <summary>
	/// The width of the level cell in meters.
	/// </summary>
	private float width;
	/// <summary>
	/// The height of the level cell in meters.
	/// </summary>
	private float height;

	/// <summary>
	/// The distance from this cell to the start of the level it belongs to. This is measured in cell units.
	/// </summary>
	private int distanceToStart;
	
	/** The game object used to physically represent the level cell. */
	private GameObject gameObject;
	
	/** Caches the Transform component of the cell's GameObject. */
	private Transform transform;
	
	/** True if the cell can be traversed by characters. If false, the cell is blocked off by some sort of obstacles */
	private bool traversable;
	
	/// <summary>
	/// Initializes a cell for a level
	/// </summary>
	/// <param name="gameObject">The gameObject used to visually depict the cell in-game.</param>
	/// <param name="traversable">If true, the cell can be walked in by characters. If false, the cell is blocked
	/// off by obstacles and the characters cannot access it </param>
	public LevelCell(int row, int column, float width, float height, GameObject gameObject, bool traversable)
	{
		// Stores the given arguments in their respective member variables
		this.row = row;
		this.column = column;
		this.width = width;
		this.height = height;
		this.GameObject = gameObject;
		this.traversable = traversable;
	}
	
	/// <summary>
	/// The game object used to physically represent the cell in the level. 
	/// </summary>
	public GameObject GameObject
	{
		get { return gameObject; }
		set { gameObject = value; transform = gameObject.transform; }
	}
	
	/// <summary>
	/// True if the cell can be traversed by characters. If false, the cell is blocked off by some sort of obstacles	
	/// </summary>
	public bool Traversable 
	{
		get { return traversable; }
		set { traversable = value; }
	}
	
	/// <summary>
	/// A cached version of the Transform component attached to the cell's GameObject
	/// </summary>
	public Transform Transform
	{
		get { return transform; }
		set { this.transform = transform; }
	}
	
	/// <summary>
	/// Returns the y-position of the top of the cell.
	/// </summary>
	public float Top
	{
		get { return Transform.position.y + (height * 0.5f); }
	}
	
	/// <summary>
	/// Returns the y-position of the bottom of the cell.
	/// </summary>
	public float Bottom
	{
		get { return Transform.position.y - (height * 0.5f); }
	}
	
	/// <summary>
	/// Returns the x-position of the left of the cell.
	/// </summary>
	public float Left
	{
		get { return Transform.position.x - (width * 0.5f); }
	}
	
	/// <summary>
	/// Returns the x-position of the right of the cell.
	/// </summary>
	public float Right
	{
		get { return Transform.position.x + (width * 0.5f); }
	}

	/// <summary>
	/// The distance from this cell to the start of the level it belongs to. This is measured in cell units. For instance,
	/// if this is set to '3', characters must traverse at least three cells to reach this cell, assuming they start from
	/// the beginning cell of the level.
	/// </summary>
	public int DistanceToStart
	{
		get { return distanceToStart; }
		set { distanceToStart = value; }
	}
}



/// <summary>
/// Denotes the position of a cell relative to the grid it belongs to. /// </summary>
public enum CellAnchor
{
	Top,
	Bottom,
	Left,
	Right
}