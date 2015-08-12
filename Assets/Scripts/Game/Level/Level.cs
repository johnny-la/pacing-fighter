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
	/// The cells which the player can walk on. Note that dictionary is structured as follows: Each element in the dictionary is a list.
	/// Suppose we take the list stored at 'traversableCells[i]'. This stores all the traversable cells at distance 'i' from the starting
	/// cell. The distance is defined in terms of cell coordinates. For instance, if a character must walk one cell from the starting
	/// to reach his desired cell, then his desired cell is at a distance of '1' away from the starting cell. In this case, this cell
	/// will be inserted into the list 'traversableCells[1]'.
	/// </summary>
	private new Dictionary<int, List<Cell>> traversableCells = new Dictionary<int, List<Cell>>();

	/// <summary>
	/// Stores the minimum and maximum y-values of the level.
	/// </summary>
	private Range verticalBounds = new Range();
	/// <summary>
	/// Stores the minimum and maximum x-values of the level.
	/// </summary>
	private Range horizontalBounds = new Range();

	/// <summary>
	/// The GameObject prefabs for cells that are traversable in the level grid 
	/// </summary>
	public GameObject[] traversableCellPrefabs;

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

		// Update the 'vertical/horizontalBounds' variables to store the positional bounds of the level.
		UpdateVerticalBounds ();
		UpdateHorizontalBounds ();

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
			LevelCell levelCell = CreateLevelCell(currentCell.row,currentCell.column,traversableCellPrefabs,true);

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
				LevelCell branchLevelCell = CreateLevelCell (branchCell.row, branchCell.column, traversableCellPrefabs, true);

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
		LevelCell levelCell = new LevelCell(row,column,cellWidth,cellHeight,cellObject,traversable);
		
		// Place the cell at the correct position in the level.
		levelCell.Transform.position = GetCellPosition (row,column);
		
		// Set the parent of each LevelCell to the same GameObject, the 'levelHolder'. Keeps the Hierarchy clean.
		levelCell.Transform.SetParent (levelHolder);

		// If the created cell is traversable
		if(traversable)
		{
			// Get the distance (in cell units) from the cell to the start of the level. This is the shortest path from the
			// cell to the starting cell of the level.
			int distanceToStart = GetDistanceToStart(row,column);
			// Update the cell's distance to the start of the level.
			levelCell.DistanceToStart = distanceToStart;

			// Populate the dictionary with a list at the index 'distanceToStart' to avoid a NullReferenceException
			if(!traversableCells.ContainsKey(distanceToStart))
				traversableCells.Add (distanceToStart, new List<Cell>());

			// Add the cell to the list of traversable cells in the level. The cell is stored in a dictionary mapping the
			// cell's distance from the start of level, to a list of cells that are at that distance.
			traversableCells[distanceToStart].Add (new Cell(row, column));
		}
			
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
	/// Returns the distance from the given cell to the start of the level. Note: Only works if this cell has at least one neighbouring
	/// cell that is traversable. This distance denotes the shortest path (in cell units) from the start of the level to the given cell. 
	/// Returns zero if the given cell has no traversable neighbouring cells.
	/// </summary>
	public int GetDistanceToStart(int row, int column)
	{
		// Store the traversable LevelCell that is 
		// A) a neighbour of the given cell, and
		// B) closest to the starting cell in the level
		LevelCell closestNeighbourToStart = GetNeighbourClosestToStart (row, column);

		// If the given cell does not have any traversable neighbouring cells
		if(closestNeighbourToStart == null)
		{
			// Return zero, since there does not exist a path from this cell to the start of the level
			return 0;
		}

		// This cell is one unit away (in cell units) from his closest neighbour
		int distanceToStart = closestNeighbourToStart.DistanceToStart + 1;

		// Return the distance (in cell units) from the given cell to the starting cell of the level.
		return distanceToStart;
	}

	/// <summary>
	/// Returns the cell's neighbour that is the closest to the starting point of the level. This method only returns traversable cells,
	/// not obstacle cells. Note: may be null. This method is a helper method used to determine the distance from the cell to the start
	/// of the level.
	/// </summary>
	public LevelCell GetNeighbourClosestToStart(int row, int column)
	{
		// Store the cells which neighbour the current cell
		Cell leftNeighbour = new Cell(row, column-1),
		rightNeighbour = new Cell(row, column+1),
		upperNeighbour = new Cell(row+1, column),
		lowerNeighbour = new Cell(row-1, column);

		// Stores the traversable neighbour with the minimum distance to the start of the level. This distance is calculated in number of cells 
		// needed to travel from the neighbour to the start cell of the level.
		int minDistance = int.MaxValue;

		// Stores the given cell's neighbour that is closest to the start point of the level. Note that this neighbour must be traversable.
		LevelCell closestNeighbour = null;

		// If the left neighbour is traversable, consider it as a candidate for the best neighbour closest to the start of the level
		if(IsTraversable(leftNeighbour))
		{
			// Retrieve the LevelCell instance corresponding to the given cell's left neighbour.
			LevelCell leftNeighbourCell = GetLevelCell (leftNeighbour);

			// If the left neighbour cell is the closest neighbour to the start of the level thus far
			if(leftNeighbourCell.DistanceToStart < minDistance)
			{
				// Update the minimum distance from the given cell's neighbours to the start point of the level
				minDistance = leftNeighbourCell.DistanceToStart;
				closestNeighbour = leftNeighbourCell;
			}
		}

		// If the right neighbour is traversable, consider it as a candidate for the best neighbour closest to the start of the level
		if(IsTraversable(rightNeighbour))
		{
			// Retrieve the LevelCell instance corresponding to the given cell's right neighbour.
			LevelCell rightNeighbourCell = GetLevelCell (rightNeighbour);
			
			// If the right neighbour cell is the closest neighbour to the start of the level thus far
			if(rightNeighbourCell.DistanceToStart < minDistance)
			{
				// Update the minimum distance from the given cell's neighbours to the start point of the level
				minDistance = rightNeighbourCell.DistanceToStart;
				closestNeighbour = rightNeighbourCell;
			}
		}

		// If the top neighbour is traversable, consider it as a candidate for the best neighbour closest to the start of the level
		if(IsTraversable(upperNeighbour))
		{
			// Retrieve the LevelCell instance corresponding to the given cell's top neighbour.
			LevelCell upperNeighbourCell = GetLevelCell (upperNeighbour);
			
			// If the upper neighbour cell is the closest neighbour to the start of the level thus far
			if(upperNeighbourCell.DistanceToStart < minDistance)
			{
				// Update the minimum distance from the given cell's neighbours to the start point of the level
				minDistance = upperNeighbourCell.DistanceToStart;
				closestNeighbour = upperNeighbourCell;
			}
		}

		// If the bottom neighbour is traversable, consider it as a candidate for the best neighbour closest to the start of the level
		if(IsTraversable(lowerNeighbour))
		{
			// Retrieve the LevelCell instance corresponding to the given cell's lower neighbour.
			LevelCell lowerNeighbourCell = GetLevelCell (lowerNeighbour);
			
			// If the lower neighbour cell is the closest neighbour to the start of the level thus far
			if(lowerNeighbourCell.DistanceToStart < minDistance)
			{
				// Update the minimum distance from the given cell's neighbours to the start point of the level
				minDistance = lowerNeighbourCell.DistanceToStart;
				closestNeighbour = lowerNeighbourCell;
			}
		}

		// Return this cell's neighbour that is the closest to the start of the level. 
		return closestNeighbour;

	}

	/// <summary>
	/// Gets the LevelCell instance (the physical cell) corresponding to the given coordinates in the level grid.
	/// </summary>
	public LevelCell GetLevelCell(Cell coordinates)
	{
		// If the given coordinates are out of bounds of the level grid, return null.
		if(IsOutOfBounds (coordinates))
			return null;

		// Return the LevelCell stored at the specified coordinates in the grid.
		return grid[coordinates.row, coordinates.column];
	}

	/// <summary>
	/// Returns the cell which contains this position. The returned cell may be out of bounds of the level. This method
	/// returns the cell coordinates whose bounding box contains this position.
	/// </summary>
	public Cell GetCellFromPosition(Vector2 position)
	{
		// Computes the vector going from the center of the starting cell to the given position
		Vector2 distanceFromStartCell = (position - startCellPosition);

		// Compute the amount of columns which separate the starting cell and the given position. Note that '0.5' is added to make up for
		// the fact that the starting cell's position was defined at the center of the starting cell.
		int columnDifference = (int)(distanceFromStartCell.x / cellWidth + 0.5f);
		// Compute the amount of rows we must travel to go from the start cell to the given position.
		int rowDifference = (int)(distanceFromStartCell.y / cellHeight + 0.5f);

		// Compute the cell coordinates which contains the given position. This is simply the starting cell's coordinates, plus the difference
		// in rows/columns from the starting cell to the given position.
		int row = startCellCoordinates.row + rowDifference;
		int column = startCellCoordinates.column + columnDifference;

		// Return a new Cell instance denoting the cell in the level grid in which the given position can be found
		return new Cell(row, column);
	}

	/// <summary>
	/// Returns a random (traversable) LevelCell which is 'distanceFromStart' cells away from the start of the level.
	/// This is useful for finding a cell which is ahead or behind a character. Note that the greater the given
	/// distance, the further to the end of the level the cell will be.
	/// </summary>
	public LevelCell GetCellAtDistance(int distanceFromStart)
	{
		// If there are no traversable cells that are 'distanceFromStart' cells away from the start of the level
		if(!traversableCells.ContainsKey (distanceFromStart))
			// Return null, since no cell at the desired distance exists
			return null;

		// Find a random cell in the 'traversableCells' dictionary at index 'distanceFromStart'. Note that this index stores a list
		// of LevelCells that are 'distanceFromStart' cell units away from the start of the level.
		Cell randomCell = ArrayUtils.RandomElement (traversableCells[distanceFromStart]);

		// Return the LevelCell which lies at the coordinates found above.
		return GetLevelCell (randomCell);
	}


	/// <summary>
	/// Returns true if the cell at the given coordinates is traversable by characters. If not, the cell is blocked off by obstacles,
	/// and enemies cannot spawn inside it
	/// </summary>
	public bool IsTraversable(Cell cell)
	{
		// If the given cell is out of bounds of the level, or the cell hasn't been created yet, return false, since a traversable cell does not 
		// exist at this spot
		if(IsOutOfBounds (cell) || GetLevelCell(cell) == null)
			return false;

		// If the cell at the given coordinates is traversable, return true
		if(grid[cell.row, cell.column].Traversable)
			return true;

		// If this statement is reached, the given cell is not traversable in the level. Thus, return false.
		return false;
	}

	/// <summary>
	/// Returns true if the cell defines a dead end in the level grid's golden path. This is true if the 
	/// cell's neighbours are traversable cells, or are cells that are off the grid.
	/// </summary>
	public bool IsDeadEnd(Cell cell)
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
	public bool IsOccupied(Cell cell)
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
	/// Returns the center position of a cell at the given coordinates in the level grid.
	/// </summary>
	public Vector2 GetCellPosition(int row, int column)
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

	/// <summary>
	/// Updates the vertical bounds of the level. This is a range which specifies the the upper-most and lower-most y-values 
	/// of the level. The result is stored inside 'verticalBounds:Range'.
	/// </summary>
	private void UpdateVerticalBounds()
	{
		// 'ComputeCellPosition(-1,0).y' returns the center y-position of the bottom-most cell in the level. The half-height of the 
		// cell is subtructed to get the y-value of the bottom of the cell.
		float minY = GetCellPosition (-1, 0).y - (cellHeight * 0.5f);
		// 'ComputeCellPosition(rows,0).y' returns the center y-position of the top-most cell in the level. The half-height of the 
		// cell is added to get the y-value of the top of the cell.
		float maxY = GetCellPosition (rows,0).y + (cellHeight * 0.5f);

		// Set the vertical bounds of the level to the values computed above
		verticalBounds.Set (minY, maxY);
	}

	/// <summary>
	/// Updates the x-values of the left-most and right-most points in the level. The result is stored inside 'horizontalBounds:Range'.
	/// </summary>
	private void UpdateHorizontalBounds()
	{
		// 'ComputeCellPosition(0,-1).x' returns the center x-position of the left-most cell in the level. The half-width of the cell
		// is subtracted to find the x-position of the left edge of this cell.
		float minX = GetCellPosition (0,-1).x - (cellWidth * 0.5f);
		// 'ComputeCellPosition(0,columns).x' returns the center x-position of the right-most cell in the level. The half-width of the cell
		// is added to find the x-position of the right edge of this cell.
		float maxX = GetCellPosition (0,columns).x + (cellWidth * 0.5f);

		// Set the horizontal bounds of the level to the values computed above.
		horizontalBounds.Set (minX, maxX);
	}

	/// <summary>
	/// The cells which the player can walk on. Note that dictionary is structured as follows: Each element in the dictionary is a list.
	/// Suppose we take the list stored at 'traversableCells[i]'. This stores all the traversable cells at distance 'i' from the starting
	/// cell. The distance is defined in terms of cell coordinates. For instance, if a character must walk one cell from the starting
	/// to reach his desired cell, then his desired cell is at a distance of '1' away from the starting cell. In this case, this cell
	/// will be inserted into the list 'traversableCells[1]'.
	/// </summary>
	public Dictionary<int, List<Cell>> TraversableCells 
	{
		get { return traversableCells; }
	}

	/// <summary>
	/// The minimum and maximum y-values of the level's grid.
	/// </summary>
	public Range VerticalBounds
	{
		get { return verticalBounds; }
		set { verticalBounds = value; }
	}
	/// <summary>
	/// The minimum and maximum x-values of the level.
	/// </summary>
	public Range HorizontalBounds
	{
		get { return horizontalBounds; }
		set { horizontalBounds = value; }
	}
}

