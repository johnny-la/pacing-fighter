using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

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
		float upProb = this.upProbability;
		float downProb = this.downProbability;
		float leftProb = this.leftProbability;
		float rightProb = this.rightProbability;
		
		// If moving to the left is an illegal turn
		if((illegalDirections & NavigationDirection.Left) == NavigationDirection.Left)
		{
			// Omit the possibility of moving left by setting its respective probability to zero
			leftProb = 0.0f;
		}
		// If moving to the right is an illegal turn
		if((illegalDirections & NavigationDirection.Right) == NavigationDirection.Right)
		{
			// Omit the possibility of moving right by setting its respective probability to zero
			rightProb = 0.0f;
		}
		// If navigating up is an illegal turn
		if((illegalDirections & NavigationDirection.Up) == NavigationDirection.Up)
		{
			// Omit the possibility of moving up by setting its respective probability to zero
			upProb = 0.0f;
		}
		// If moving down is an illegal turn
		if((illegalDirections & NavigationDirection.Down) == NavigationDirection.Down)
		{
			// Omit the possibility of moving down by setting its respective probability to zero
			downProb = 0.0f;
		}
		
		// Calculate the sum of all four probabilities. The random number used to choose a direction 
		// will be between zero and this value
		float probabilitySum = leftProb + rightProb + upProb + downProb;
		
		// Generate a random float used to return a random direction.
		float randomFloat = Random.Range (0,probabilitySum);
		
		// Based on the value of the float, choose a NavigationDirection enumeration constant and return it
		if(randomFloat <= upProb)
			return NavigationDirection.Up;
		else if(randomFloat <= downProb + upProb)
			return NavigationDirection.Down;
		else if(randomFloat <= leftProb + downProb + upProb)
			return NavigationDirection.Left;
		else
			return NavigationDirection.Right;
		
	}
	
	/// <summary>
	/// Inverts the probabilities for the GridNavigator. Useful for defining branching paths that go against the direction
	/// of the golden path. This works as follows: The probabilities are swapped such that the highest probability is now the 
	/// lowest, and the lowest is now the highest. Essentially reverses the order of the probabilities.
	/// </summary>
	/*public void Invert()
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
	}*/
	
	/// <summary>
	/// Reverses the probabilities of each axis. That is, the left and right probabilities are swapped, and the up and down
	/// probabilities are also swapped.
	/// </summary>
	public void ReverseAxes()
	{
		
	}

	public string ToString()
	{
		return "Up: " + upProbability + ", Down: " + downProbability + ", Left: " + leftProbability + ", Right: " + rightProbability;
	}
}
