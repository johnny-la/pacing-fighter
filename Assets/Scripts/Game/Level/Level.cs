using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;

/// <summary>
/// The container and generator for a game level
/// </summary>
/// <remarks>
/// Terminology:
/// Golden path: The main road which leads from the beginning to the end of the level. This is the path the
/// player must take to reach the end of the level, and is randomly generated before branches are randomly
/// added to this path.
/// Branch: A road which branches off the golden path. It leads to nowhere, and serves to add randomness to 
/// the level's geometry.
/// </remarks>

public class Level : MonoBehaviour 
{
	/// <summary>
	/// The seed used by random number generators to generate this level. Given the same
	/// seed and the same grid dimensions, the same level will be reproduced. 
	/// </summary>
	public readonly int seed = 1004922;

	/// <summary>
	/// The center position of the starting cell of the grid.
	/// </summary>
	private Vector2 startCellPosition = Vector2.zero;

	/// <summary>
	/// The position of the grid's starting cell, relative to the center of the grid (is the cell at the top, bottom, left or right of the grid?).
	/// </summary>
	private CellAnchor startCellAnchor;

	/// <summary>
	/// The position of the grid's ending cell, relative to the center of the grid (i.e., is the cell at the top, bottom, left or right of the grid?).
	/// </summary>
	private CellAnchor endCellAnchor;

	/// <summary>
	/// A (row,column) pair denoting the coordinates of the starting cell in the level. Note
	/// that the row and column integers are zero-based
	/// </summary>
	private Cell startCellCoordinates;

	/// <summary>
	/// A(row,column) pair denoting the coordinates of the end cell in the level. Note
	/// that the row and column indices are zero-based
	/// </summary>
	private Cell endCellCoordinates;

	/// <summary>
	/// The amount of rows in the level's grid
	/// </summary>
	public int rows;
	/// <summary>
	/// The number of columns in the level's procedural generation grid
	/// </summary>
	public int columns;

	/// <summary>
	/// The width of a cell in the level's procedurally-generated grid.
	/// </summary>
	public float cellWidth;
	/// <summary>
	/// The height of a cell in the level's procedurally-generated grid.
	/// </summary>
	public float cellHeight;

	/// <summary>
	/// Stores the LevelCells which occupy the level grid.
	/// </summary>
	private LevelCell[,] grid;

	/// <summary>
	/// The GameObject prefabs for cells that are traversable in the level grid 
	/// </summary>
	private GameObject[] traversableCells;

	/// <summary>
	/// The GameObject prefabs for cells that are not traversable by characters in the level grid 
	/// </summary>
	private GameObject[] obstacleCells;

	/// <summary>
	/// Defines the probabilities associated with generating the golden path of the grid
	/// </summary>
	public GridNavigator gridNavigator;

	/// <summary>
	/// The parent for all of the level's GameObjects
	/// </summary>
	private Transform levelHolder;

	/// <summary>
	/// Generates a level given the properties defined by its member variables.
	/// </summary>
	/// <param name="startPosition">The center position of the first cell in the level grid.</param>
	/// <param name="startCellAnchor">The position of the first cell, relative to the grid. .</param>
	/// <param name="endCellAnchor">The position to which the last cell of the grid is anchored (i.e., is the final
	/// cell at the top, bottom, left or right of the grid?) </param>
	public void GenerateLevel(Vector2 startCellPosition, CellAnchor startCellAnchor, CellAnchor endCellAnchor)
	{
		// Store the given arguments in their respective member variables
		this.startCellPosition = startCellPosition;
		this.startCellAnchor = startCellAnchor;
		this.endCellAnchor = endCellAnchor;

		// Create the level grid which stores the GameObjects and cell information for all cells in the level.
		grid = new LevelCell[rows,columns];

		// Set the seed for the random number generator. Ensures that, given the same seed, the same level will be reproduced.
		Random.seed = this.seed;

		// Compute and store the (row,column) coordinates of the starting/ending cells of the level grid.
		startCellCoordinates = ComputeStartCellCoordinates ();
		endCellCoordinates = ComputeEndCellCoordinates();

		// Create the GameObject which will act as a parent to every GameObject in the level.
		levelHolder = new GameObject("Level").transform;

		// Generate the golden path for the level. This is the main road that leads from start to finish.
		GenerateGoldenPath();

	}

	/// <summary>
	/// Generate the golden path for the level. This is the main road that leads the player from the start to the end of the level.
	/// </summary>
	public void GenerateGoldenPath()
	{
		// Start the golden path at the coordinates of the starting cell.
		int row = startCellCoordinates.row;
		int column = startCellCoordinates.column;

		// Create a traversable cell at (row,column) coordinates. This generates a navigatable path for the characters.
		LevelCell cell = CreateLevelCell(row,column,traversableCells,true);

		// Store the LevelCell inside the level grid to easily keep track of each cell in the level
		grid[row,column] = cell;
	}

	/// <summary>
	/// Creates and returns a LevelCell at the given (row,column) coordinates. A random prefab from 'cellPrefabs' is chosen
	/// and duplicated to serve as the physical cell inside the level.
	/// </summary>
	/// <param name="traversable">If true, the cell will be traversable by characters. If false, the cell is blocked off 
	/// by obstacles and cannot be walked through by character </param>
	public LevelCell CreateLevelCell(int row, int column, GameObject[] cellPrefabs, bool traversable)
	{
		// Choose a random prefab to occupy this given coordinates.
		GameObject cellPrefab = cellPrefabs[Random.Range (0,cellPrefabs.Length)];
		
		// Instantiate a copy of the chosen prefab.
		GameObject cellObject = Instantiate (cellPrefab);

		// Create a new LevelCell instance. This acts as a container for the cell in the level grid
		LevelCell levelCell = new LevelCell(row,column,cellObject,traversable);

		// Place the cell at the correct position in the level.
		levelCell.Transform.position = ComputeCellPosition (row,column);

		// Set the parent of each LevelCell to the same GameObject, the 'levelHolder'. Keeps the Hierarchy clean.
		levelCell.Transform.SetParent (levelHolder);

		// Return the created cell to place in the level grid
		return levelCell;
	}

	/// <summary>
	/// Returns the center position of a cell in the given coordinates of the grid.
	/// </summary>
	public Vector2 ComputeCellPosition(int row, int column)
	{
		// Holds the position of the cell at the given coordinates
		Vector2 position = Vector2.zero;

		// Compute the difference in rows and columns from the starting cell to this cell 
		// (Note that startCellCoordinates is a Cell instance holding a (row,column) pair)
		int rowDifference = row - startCellCoordinates.row;
		int columnDifference = column - startCellCoordinates.column;

		// Computes the center position of the cell at the given coordinates. This is calculated by taking the center position of 
		// the starting cell, and adding the column/row difference times the cell's width/height
		position.x = startCellPosition.x + (columnDifference * cellWidth);
		position.y = startCellPosition.y + (rowDifference * cellHeight);

		// Return the position of the cell computed above.
		return position;
	}

	/// <summary>
	/// Returns a (row,column) pair denoting the coordinates of the starting cell in the level. Note
	/// that the row and column integers are zero-based
	/// </summary>
	private Cell ComputeStartCellCoordinates()
	{
		// Stores the (row,column) coordinates of the starting cell
		Cell coordinates;
		// Initialize struct to avoid compiler error
		coordinates.row = coordinates.column = 0;

		// Switch the position to which the starting cell is anchored relative to the grid
		switch(startCellAnchor)
		{
		case CellAnchor.Left:
			// If the cell is anchored to the left of the grid, the starting cell is at coordinate (rows/2,0) [indices are zero-based]
			coordinates.Set (rows/2,0);
			break;
		case CellAnchor.Right:
			// If the cell is anchored to the right of the grid, the starting cell is at coordinate (rows/2,columns-1) [indices are zero-based]
			coordinates.Set (rows/2,columns-1);
			break;
		case CellAnchor.Bottom:
			// If the cell is anchored to the bottom of the grid, the starting cell is at coordinate (0,columns/2) [indices are zero-based]
			coordinates.Set (0,columns/2);
			break;
		case CellAnchor.Top:
			// If the cell is anchored to the top of the grid, the starting cell is at coordinate (rows-1,columns/2) [indices are zero-based]
			coordinates.Set (rows-1,columns/2);
			break;
		}

		// Return the (row,column) coordinates of the starting cell of the grid
		return coordinates;
	}

	/// <summary>
	/// Returns a (row,column) pair denoting the coordinates of the end cell in the level. Note
	/// that the row and column integers are zero-based
	/// </summary>
	private Cell ComputeEndCellCoordinates()
	{
		// Stores the (row,column) coordinates of the starting cell
		Cell coordinates;
		// Initialize struct to avoid compiler error
		coordinates.row = coordinates.column = 0;
		
		// Switch the position to which the end cell is anchored relative to the grid
		switch(endCellAnchor)
		{
		case CellAnchor.Left:
			// If the cell is anchored to the left of the grid, the starting cell is at coordinate (rows/2,0) [indices are zero-based]
			coordinates.Set (rows/2,0);
			break;
		case CellAnchor.Right:
			// If the cell is anchored to the right of the grid, the starting cell is at coordinate (rows/2,columns-1) [indices are zero-based]
			coordinates.Set (rows/2,columns-1);
			break;
		case CellAnchor.Bottom:
			// If the cell is anchored to the bottom of the grid, the starting cell is at coordinate (0,columns/2) [indices are zero-based]
			coordinates.Set (0,columns/2);
			break;
		case CellAnchor.Top:
			// If the cell is anchored to the top of the grid, the starting cell is at coordinate (rows-1,columns/2) [indices are zero-based]
			coordinates.Set (rows-1,columns/2);
			break;
		}
		
		// Return the (row,column) coordinates of the starting cell of the grid
		return coordinates;
	}
}

// Denotes a cell that is part of the level grid. 
[Serializable]
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
	public LevelCell(int row, int column, GameObject gameObject, bool traversable)
	{
		// Stores the given arguments in their respective member variables
		this.row = row;
		this.column = column;
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

/// <summary>
/// The direction (relative to the previous tile) that the next cell is placed in the level grid.
/// </summary>
public enum NavigationDirection
{
	Up,
	Down,
	Forward,
	Backward
}

/// <summary>
/// Defines the probabilities used to generate the golden path (winning road) to the end of the level
/// </summary>
[Serializable]
public class GridNavigator
{
	[Tooltip("The probability that the next tile in the grid is chosen to be above the previous one. All four probabilities must sum to one. ")]
	public float upProbability;
	[Tooltip("The probability that the next tile in the grid is chosen to be below the previous one. All four probabilities must sum to one. ")]
	public float downProbability;
	[Tooltip("The probability that the next tile in the grid is chosen to be to the left of the previous one. All four probabilities must sum to one. ")]
	public float leftProbability;
	[Tooltip("The probability that the next tile in the grid is chosen to be to the right of the previous one. All four probabilities must sum to one. ")]
	public float rightProbability;

	/// <summary>
	/// Choose a random navigation direction based on the probabilities defined with the member variables
	/// </summary>
	/// <returns>The direction.</returns>
	/*public NavigationDirection ChooseDirection()
	{
		// Generate a random float used to return a random direction.
		float randomFloat = Random.Range (0,1);


	}*/
}
