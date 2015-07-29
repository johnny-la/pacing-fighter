using UnityEngine;
using System;
using System.Collections.Generic;
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
	public int seed = 1004922;

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
	/// The number of branches in the level maze. Each branch is a walkable area that branches off the golden
	/// path and leads to nowhere
	/// </summary>
	public float branchCount;

	/// <summary>
	/// The maximum size of a branch in the level grid. This is the max number of cells that can be placed for
	/// a single branch. Terminology: a branch is a divergence in the golden path that leads to nowhere.
	/// </summary>
	public int maxBranchSize;

	/// <summary>
	/// Stores the LevelCells which occupy the level grid.
	/// </summary>
	private LevelCell[,] grid;

	/// <summary>
	/// Holds the cells that form the golden path in the maze. The golden path is the fastest road to the end of the maze.
	/// </summary>
	private List<Cell> goldenPath = new List<Cell>(20);

	/// <summary>
	/// The GameObject prefabs for cells that are traversable in the level grid 
	/// </summary>
	public GameObject[] traversableCells;

	/// <summary>
	/// The GameObject prefabs for cells that are not traversable by characters in the level grid 
	/// </summary>
	public GameObject[] obstacleCells;

	/// <summary>
	/// The prefabs which can be chosen for the top boundaries of the grid.
	/// </summary>
	public GameObject[] topEdgeCells;

	/// <summary>
	/// The prefabs which can be chosen for the side boundaries of the grid.
	/// </summary>
	public GameObject[] sideEdgeCells;

	/// <summary>
	/// The prefabs which can be chosen for the bottom boundaries of the grid.
	/// </summary>
	public GameObject[] bottomEdgeCells;

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

		// Generate the branches in the level maze. These are divergences in the maze that lead to nowhere
		GenerateBranches();

		// Iterate through the level grid and populate all the non-occupied cells with obstacles.
		GenerateObstacleCells();

		// Generates the level cells which close off the level.
		GenerateBoundaries();

	}

	/// <summary>
	/// Generate the golden path for the level. The golden path is the main road that leads the player from the start to the end of the level.
	/// </summary>
	private void GenerateGoldenPath()
	{
		// Start the golden path at the coordinates of the starting cell.
		Cell currentCell = new Cell(startCellCoordinates.row, startCellCoordinates.column);

		// Stores the (row,column) coordinates at which the golden path must end
		Cell endCell = new Cell(endCellCoordinates);
	
		// Keep iterating until the golden path is generated.
		while(true)
		{
			// Create a traversable cell at (row,column) coordinates. This generates a navigatable path for the characters.
			LevelCell levelCell = CreateLevelCell(currentCell.row,currentCell.column,traversableCells,true);

			// Add the cell to the list of cells in the golden path. Useful for keeping track of the fastest way out of the maze
			goldenPath.Add(currentCell);

			// If the current (row,column) cell being cycled through is the ending cell,
			// finish the golden path at this cell
			if(currentCell.row == endCell.row && currentCell.column == endCell.column)
			{

			}

			// Choose the next traversable cell in the golden path
			currentCell = ChooseNextTraversableCell(currentCell);

			// If the next chosen cell is already occupied by a GameObject, or the chosen cell is out of bounds of the level grid,
			// break out of the loop
			if(IsOccupied (currentCell) || IsOutOfBounds(currentCell))
				break;
		}
	}
	/// <summary>
	/// Generate the branches in the level maze. These are divergences in the maze that lead to nowhere
	/// </summary>
	private void GenerateBranches()
	{
		// Calculates the number of cells that separate each branch in the golden path. The branches are evenly distributed accross the golden path
		int branchSeparationDistance = (int)(goldenPath.Count / this.branchCount);

		// Iterate from 0 to the number of branches in the level and create each branch individually.
		for(int branchNumber = 0; branchNumber < this.branchCount; branchNumber++)
		{
			// Calculates the index of the cell at which the branch starts. This is the branchSeparationDistance*(branchNumber+1)
			// (Note: branchNumber incremented by one to avoid starting a branch at the first cell in the golden path.)
			int startCellIndex = branchSeparationDistance * (branchNumber+1);

			// Retrieve the golden path cell at which the branch will start.
			Cell startCell = goldenPath[startCellIndex];

			// Choose a cell in which to start the branch. Note that the function called is smart enough to return an onoccupied cell.
			Cell branchCell = ChooseNextTraversableCell (startCell);

			// Invert the probabilities in the grid navigator so that the branches go in the opposite direction of the golden path.
			gridNavigator.Invert ();

			// Cycles until 'maxBranchSize' cells are placed in the branch. or until a dead end is reached.
			for(int i = 0; i < maxBranchSize; i++)
			{
				// If the chosen cell to form the branching path is already occupied or is out of bounds of the grid, break 
				// this for loop. The branch has reached a dead end.
				if(IsOccupied (branchCell) || IsOutOfBounds (branchCell))
					break;

				// Create a traversable cell at the chosen (row,column) coordinates. This is a cell for the current branch being defined
				CreateLevelCell (branchCell.row, branchCell.column, traversableCells, true);

				// If the cell in the branch which was just created forms a dead end, break the loop, since the branch can't go any further
				if(IsDeadEnd (branchCell))
					break;

				// Choose the next cell to be added to the branching path
				branchCell = ChooseNextTraversableCell (branchCell);
			}
		}
	}

	/// <summary>
	/// Generates the obstacle cells in the level grid. This function loops through the 'grid' matrix and
	/// populates each empty cell with an obstacle LevelCell.
	/// </summary>
	private void GenerateObstacleCells()
	{
		// Stores a Cell struct that will be iterating through each cell in the level grid.
		Cell cell;

		// Iterate through each row in the level grid
		for(cell.row = 0; cell.row < rows; cell.row++)
		{
			// Iterate through each column in the level grid
			for(cell.column = 0; cell.column < columns; cell.column++)
			{
				// If the cell being iterated through is 
				if(!IsOccupied (cell))
				{
					// Create a LevelCell at the current cell coordinates that will act as an obstacle in the level
					CreateLevelCell (cell.row, cell.column, obstacleCells, false);
				}
			}
		}
	}

	/// <summary>
	/// Generates the boundaries of the grid which close off the level and prevent the user from leaving it.
	/// </summary>
	private void GenerateBoundaries()
	{
		// Stores the cell being iterated through
		Cell cell;

		// Iterate on the bottom columns and add the bottom boundaries
		for(cell.row = -1, cell.column = -1; cell.column <= columns; cell.column++)
		{
			// Create a cell at the bottom boundaries of the grid. This closes off the bottom portion of the maze
			CreateLevelCell (cell.row, cell.column, bottomEdgeCells, false, true);
		}

		// Iterate on the top columns and add the top boundaries
		for(cell.row = rows, cell.column = -1; cell.column <= columns; cell.column++)
		{
			// Create a cell at the top boundaries of the grid. This closes off the top portion of the maze
			CreateLevelCell (cell.row, cell.column, topEdgeCells, false, true);
		}

		// Iterate on the left rows and add the left boundaries of the grid
		for(cell.row = -1, cell.column = -1; cell.row <= rows; cell.row++)
		{
			// Create a cell at the left boundaries of the grid. This closes off the left portion of the maze
			CreateLevelCell (cell.row, cell.column, sideEdgeCells, false, true);
		}

		// Iterate on the right rows and add the right boundaries of the grid
		for(cell.row = -1, cell.column = columns; cell.row <= rows; cell.row++)
		{
			// Create a cell at the right boundaries of the grid. This closes off the right portion of the maze
			CreateLevelCell (cell.row, cell.column, sideEdgeCells, false, true);
		}
	}

	/// <summary>
	/// Creates and returns a LevelCell at the given (row,column) coordinates. A random prefab from 'cellPrefabs' is chosen
	/// and duplicated to serve as the physical cell inside the level. 
	/// </summary>
	/// <param name="traversable">If true, the cell will be traversable by characters. If false, the cell is blocked off 
	/// by obstacles and cannot be walked through by character </param>
	private LevelCell CreateLevelCell(int row, int column, GameObject[] cellPrefabs, bool traversable)
	{
		// Call an overloaded version of the method, specifying that the cell is not a boundary cell, but is inside the grid.
		return CreateLevelCell (row,column,cellPrefabs,traversable,false);
	}

	/// <summary>
	/// Creates and returns a LevelCell at the given (row,column) coordinates. A random prefab from 'cellPrefabs' is chosen
	/// and duplicated to serve as the physical cell inside the level.
	/// </summary>
	/// <param name="traversable">If true, the cell will be traversable by characters. If false, the cell is blocked off 
	/// by obstacles and cannot be walked through by character </param>
	/// <param name="boundaryCell">If set to <c>true</c>, the cell is outside the bounds of the grid. It serves to block
	/// of the boundaries of the grid. Default = false</param>
	private LevelCell CreateLevelCell(int row, int column, GameObject[] cellPrefabs, bool traversable, bool boundaryCell)
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

		// If the created cell is inside the maze, and is not made to close off any boundaries, add it to the level grid.
		if(!boundaryCell)
			// Store the created LevelCell inside the level grid for storage purposes
			grid[row,column] = levelCell;
		
		// Return the created cell to place in the level grid
		return levelCell;
	}

	/// <summary>
	/// Chooses the next cell that the characters can traverse on, given the previous cell. This function chooses which
	/// cells are traversable in the level grid.
	/// </summary>
	private Cell ChooseNextTraversableCell(Cell cell)
	{
		// Stores the directions in which the path may NOT go. That is, if this constant is
		// set to 'NavigationDirection.Left', for instance, the next cell CANNOT be to the left
		// of the current one. The illegal directions are set to the direction of each of the 
		// cell's occupied neighbours. That is, if the cell above this one is occupied, the next
		// traversable cell cannot be above the current one, since it is already occupied.
		NavigationDirection illegalDirections = GetOccupiedNeighbours (cell);
		
		// Choose a direction in which to move to define the next cell in the golden path
		NavigationDirection pathDirection = gridNavigator.ChooseDirection (illegalDirections);
		
		// Advance to the next cell in which to continue the current path
		cell = GridNavigator.Advance(cell, pathDirection);

		// Return the next traversable cell to define in the level grid
		return cell;
	}

	/// <summary>
	/// Returns the cell's neighbours that are already occupied by LevelCell instances. If they are occupied, one use-case would
	/// be to avoid generating a branch on these occupied neighbours.
	/// </summary>
	/// <returns>The occupied neighbours. The result is returned as a NavigationDirection enumeration constant, where multiple
	/// neighbours are specified via a bitwise OR operation. To test if the left neighbour is occupied, for instance, run the
	/// boolean conditional if((GetOccupiedNeighbours(cell) & NavigationDirection.Up) == NavigationDirection.Up)</returns>
	private NavigationDirection GetOccupiedNeighbours(Cell cell)
	{
		// Stores the cell's neighbours which are occupied.
		NavigationDirection occupiedNeighbours = NavigationDirection.None;

		// Store the cells which neighbour the current cell
		Cell leftNeighbour = new Cell(cell.row, cell.column-1),
		rightNeighbour = new Cell(cell.row, cell.column+1),
		upperNeighbour = new Cell(cell.row+1, cell.column),
		lowerNeighbour = new Cell(cell.row-1, cell.column);
		
		// If the left neighbour of the given cell is already occupied by a cell or is out of the grid's bounds
		if(IsOccupied (leftNeighbour) || IsOutOfBounds (leftNeighbour))
			// Add the 'Left' direction to the 'illegalDirections' enumerator. This tells this method that the next cell cannot be the one to the left
			occupiedNeighbours |= NavigationDirection.Left;
		// If the right neighbour of the given cell is already occupied by a cell or is out of the grid's bounds
		if(IsOccupied (rightNeighbour) || IsOutOfBounds (rightNeighbour))
			// Add the 'Right' direction to the 'illegalDirections' enumerator. This tells this method that the next cell cannot be the one to the right of the current one
			occupiedNeighbours |= NavigationDirection.Right;
		// If the upper neighbour of the given cell is already occupied by a cell or is out of the grid's bounds
		if(IsOccupied (upperNeighbour) || IsOutOfBounds (upperNeighbour))
			// Add the 'Up' direction to the 'illegalDirections' enumerator. This tells this method that the next cell cannot be the one above the previous one
			occupiedNeighbours |= NavigationDirection.Up;
		// If the lower neighbour of the given cell is already occupied by a cell or is out of the grid's bounds
		if(IsOccupied (lowerNeighbour) || IsOutOfBounds (lowerNeighbour))
			// Add the 'Down' direction to the 'illegalDirections' enumerator. This tells this method that the next cell cannot be the one below the previous
			occupiedNeighbours |= NavigationDirection.Down;

		// Returns the cell's neighbours which are occupied
		return occupiedNeighbours;

	}

	/// <summary>
	/// Returns true if the cell defines a dead end in the level grid's golden path. This is true if the 
	/// cell's neighbours are traversable cells, or are cells that are off the grid.
	/// </summary>
	private bool IsDeadEnd(Cell cell)
	{
		// Define the cells which neighbour the given cell
		Cell leftNeighbour = new Cell(cell.row, cell.column-1),
		     rightNeighbour = new Cell(cell.row, cell.column+1),
		     upperNeighbour = new Cell(cell.row+1, cell.column),
		     lowerNeighbour = new Cell(cell.row-1, cell.column);

		// If the given cell's neighbours are all either
		// A) Out of bounds of the level grid
		// B) Traversable cells
		// then the cell is a dead end for the golden path
		if((IsOutOfBounds(leftNeighbour) ||  (grid[leftNeighbour.row,leftNeighbour.column] != null && grid[leftNeighbour.row,leftNeighbour.column].Traversable))
		   && (IsOutOfBounds(rightNeighbour) || (grid[rightNeighbour.row,rightNeighbour.column] != null && grid[rightNeighbour.row,rightNeighbour.column].Traversable))
		   && (IsOutOfBounds(upperNeighbour) || (grid[upperNeighbour.row,upperNeighbour.column] != null && grid[upperNeighbour.row,upperNeighbour.column].Traversable))
		   && (IsOutOfBounds(lowerNeighbour) || (grid[lowerNeighbour.row,lowerNeighbour.column] != null && grid[lowerNeighbour.row,lowerNeighbour.column].Traversable)))
			return true;

		// If this statement is reached, the given cell is not a dead end in the level grid. Thus, return false.
		return false;
	}

	/// <summary>
	/// Returns true if the given cell is already occupied by a GameObject in the level grid. 
	/// </summary>
	private bool IsOccupied(Cell cell)
	{
		// If the given cell is not out of bounds of the level grid, and the given coordinates are occupied by a LevelCell
		// instance in the 'grid' matrix, return true, since the given cell is occupied
		if(!IsOutOfBounds (cell) && grid[cell.row,cell.column] != null)
			return true;

		// If this statement is reached, the given cell is not occupied in the level grid. Thus, return false
		return false;
	}

	/// <summary>
	/// Returns true if the given cell coordinates are outside the bounds of this level grid.
	/// If so, this means that these cell coordinates cannot legally define a physical cell on
	/// the level grid.
	/// </summary>
	private bool IsOutOfBounds(Cell cell)
	{
		// If the given cell coordinates lie outside the 'rows x columns' sized level grid, return true.
		// Note that rows and columns are zero-based.
		if((cell.row >= rows || cell.row < 0) 
		   || (cell.column >= columns || cell.column < 0))
			return true;

		// If this statement is reached, the cell is within the bounds of the level grid. Thus, return false
		return false;
	}

	/// <summary>
	/// Returns the center position of a cell in the given coordinates of the grid.
	/// </summary>
	private Vector2 ComputeCellPosition(int row, int column)
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
[Flags]
public enum NavigationDirection
{
	None,
	Up = 1, // Powers of 2 used to support bitwise operations
	Down = 2,
	Left = 4,
	Right = 8
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
	/// Advance the cell by one coordinate (either one row or one column), as
	/// defined by the given NavigationDirection.
	/// Returns this next cell which is visited. This serves to define a path
	/// by walking along the level grid grid
	/// </summary>
	public static Cell Advance(Cell cell, NavigationDirection direction)
	{
		// Define the Cell instance representing the next cell in the current path.
		Cell nextCell = new Cell(cell);

		// If the next cell should be created to the right of the cell passed as an argument
		if(direction == NavigationDirection.Left)
		{
			// Decrement the next cell's column by 1 in order to move to the left
			nextCell.column--;
		}
		// If the next cell should be created to the left of the cell passed as an argument
		else if(direction == NavigationDirection.Right)
		{
			// Increment the next cell's column by 1 to move to the right in our path
			nextCell.column++;
		}
		// If the next cell should be created on top of the cell passed as an argument
		else if(direction == NavigationDirection.Up)
		{
			// Increment the next cell's row by 1 in order to move up in our path
			nextCell.row++;
		}
		// If the next cell should be created below the cell passed as an argument
		else if(direction == NavigationDirection.Down)
		{
			// Decrement the next cell's row by 1 to move downwards in our path
			nextCell.row--;
		}

		// Returns the next cell which should be visited in the path denoted by the cell passed as an argument
		return nextCell;

	}

	/// <summary>
	/// Choose a random navigation direction based on the probabilities defined with the member variables.
	/// This method may return any of the four possible directions. Call the overloaded method 
	/// ChooseDirection(illegalDirections:NavigationDirection) to omit certain navigation possibilities
	/// </summary>
	public NavigationDirection ChooseDirection()
	{
		// Call the overloaded method, passing in None to ensure that any of the 4 NavigationDirections can be returned
		return ChooseDirection (NavigationDirection.None);
	}

	/// <summary>
	/// Chooses a random navigation direction in which to walk along a path.
	/// </summary>
	/// <param name="illegalDirections">The directions which cannot be returned.
	/// In order to specify more than one illegal direction, use the enumeration-based
	/// bitwise OR operation. </param>
	public NavigationDirection ChooseDirection(NavigationDirection illegalDirections)
	{
		// Define the probabilities of moving in either of the four directions
		float upProbability = this.upProbability;
		float downProbability = this.downProbability;
		float leftProbability = this.leftProbability;
		float rightProbablity = this.rightProbability;

		// If moving to the left is an illegal turn
		if((illegalDirections & NavigationDirection.Left) == NavigationDirection.Left)
		{
			// Omit the possibility of moving left by setting its respective probability to zero
			leftProbability = 0.0f;
		}
		// If moving to the right is an illegal turn
		if((illegalDirections & NavigationDirection.Right) == NavigationDirection.Right)
		{
			// Omit the possibility of moving right by setting its respective probability to zero
			rightProbability = 0.0f;
		}
		// If navigating up is an illegal turn
		if((illegalDirections & NavigationDirection.Up) == NavigationDirection.Up)
		{
			// Omit the possibility of moving up by setting its respective probability to zero
			upProbability = 0.0f;
		}
		// If moving down is an illegal turn
		if((illegalDirections & NavigationDirection.Down) == NavigationDirection.Down)
		{
			// Omit the possibility of moving down by setting its respective probability to zero
			downProbability = 0.0f;
		}

		// Calculate the sum of all four probabilities. The random number used to choose a direction 
		// will be between zero and this value
		float probabilitySum = leftProbability + rightProbablity + upProbability + downProbability;

		// Generate a random float used to return a random direction.
		float randomFloat = Random.Range (0,probabilitySum);
		
		// Based on the value of the float, choose a NavigationDirection enumeration constant and return it
		if(randomFloat <= upProbability)
			return NavigationDirection.Up;
		else if(randomFloat <= downProbability + upProbability)
			return NavigationDirection.Down;
		else if(randomFloat <= leftProbability + downProbability + upProbability)
			return NavigationDirection.Left;
		else
			return NavigationDirection.Right;

	}

	/// <summary>
	/// Inverts the probabilities for the GridNavigator. Useful for defining branching paths that go against the direction
	/// of the golden path. This works as follows: The probabilities are swapped such that the highest probability is now the 
	/// lowest, and the lowest is now the highest. Essentially reverses the order of the probabilities.
	/// </summary>
	public void Invert()
	{
		// Finds the maximum and minimum probabilities
		float max = Mathf.Max(upProbability,Mathf.Max(downProbability,Mathf.Max(leftProbability,rightProbability)));
		float min = Mathf.Min(upProbability,Mathf.Min(downProbability,Mathf.Min(leftProbability,rightProbability)));

		// Stores the argument for the max and min probabilities
		NavigationDirection maxValue = NavigationDirection.None;
		NavigationDirection minValue = NavigationDirection.None;

		// Replace the max probability with the minimum one
		if(max == upProbability)
		{
			upProbability = min;
			minValue = NavigationDirection.Up;
		}
		else if(max == downProbability)
		{
			downProbability = min;
			minValue = NavigationDirection.Up;
		}
		else if(max == leftProbability)
		{
			leftProbability = min;
			minValue = NavigationDirection.Left;
		}
		else if(max == rightProbability)
		{
			rightProbability = min;
			minValue = NavigationDirection.Right;
		}

		// Replace the min probability with the maximum one
		if(min == upProbability)
		{
			upProbability = max;
			minValue = NavigationDirection.Up;
		}
		else if(min == downProbability)
		{
			downProbability = max;
			minValue = NavigationDirection.Down;
		}
		else if(min == leftProbability)
		{
			leftProbability = max;
			minValue = NavigationDirection.Left;
		}
		else if(min == rightProbability)
		{
			rightProbability = max;
			minValue = NavigationDirection.Right;
		}


		NavigationDirection maxMinValues = minValue | maxValue;

		// Stores the two median probabilities.
		NavigationDirection medianValues = ~maxMinValues;

		float firstMedianProbability = -1.0f;
		float secondMedianProbability = -1.0f;

		if((medianValues & NavigationDirection.Up) == NavigationDirection.Up)
		{
			firstMedianProbability = upProbability;
		}
		if((medianValues & NavigationDirection.Down) == NavigationDirection.Down)
		{
			if(firstMedianProbability < 0)
				firstMedianProbability = downProbability;
			else
				secondMedianProbability = downProbability;
		}
		if((medianValues & NavigationDirection.Left) == NavigationDirection.Left)
		{
			if(firstMedianProbability < 0)
				firstMedianProbability = leftProbability;
			else
				secondMedianProbability = leftProbability;
		}
		if((medianValues & NavigationDirection.Right) == NavigationDirection.Right)
		{
			if(firstMedianProbability < 0)
				firstMedianProbability = rightProbability;
			else
				secondMedianProbability = rightProbability;
		}

		if(firstMedianProbability == upProbability)
		{
			upProbability = secondMedianProbability;
		}
		else if(firstMedianProbability == downProbability)
		{
			downProbability = secondMedianProbability;
		}
		else if(firstMedianProbability == leftProbability)
		{
			leftProbability = secondMedianProbability;
		}
		else if(firstMedianProbability == rightProbability)
		{
			rightProbability = secondMedianProbability;
		}

		if(secondMedianProbability == upProbability)
		{
			upProbability = firstMedianProbability;
		}
		else if(secondMedianProbability == downProbability)
		{
			downProbability = firstMedianProbability;
		}
		else if(secondMedianProbability == leftProbability)
		{
			leftProbability = firstMedianProbability;
		}
		else if(secondMedianProbability == rightProbability)
		{
			rightProbability = firstMedianProbability;
		}
	}


	/// <summary>
	/// Returns the index of the maximum value (zero-based).
	/// </summary>
	private int ArgMax(float a, float b, float c, float d)
	{
		if(a > b)
			return 0;
		
		return 1;
	}

	/// <summary>
	/// Returns the index of the minimum value (zero or one).
	/// </summary>
	private int ArgMin(float a, float b, float c, float d)
	{
		if(a < b)
			return 0;

		return 1;
	}

	/// <summary>
	/// Reverses the probabilities of each axis. That is, the left and right probabilities are swapped, and the up and down
	/// probabilities are also swapped.
	/// </summary>
	public void ReverseAxes()
	{

	}
}
