using System;
using System.Collections.Generic;

namespace woodPuzzle
{
    // This program solves those cool wooden block puzzles where you're trying to take a long string
    // of connected wooden blocks and fold it into a solid 4x4x4 cube.
    // Example: https://www.solve-it-puzzles.com/products/snake-cube-4x4-1
    //
    // To solve, edit the links lengths in BlockOrder for your 4x4x4 puzzle, then run it!
    public class Program
    {
        // BlockOrder describes the link (blocklines) lengths in the puzzle
        // Note: block lines share the last block of their line with the *next* line
        public static readonly int[] BlockOrder =    {3, 2, 3, 2, 2, 4, 2, 3, 2, 3, 2, 3, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 2, 2, 2, 2, 2, 3, 4, 2, 2, 2, 4, 2, 3, 2, 2, 2, 2, 2, 2, 2, 2, 2, 4, 2};
        
        public static void Main(string[] args)
        {
            // Start time
            System.Console.WriteLine("Program starting at {0}.", System.DateTime.Now.ToLongTimeString());
            
            // Set up the puzzle
            BlockPuzzle FourByFourbyFour = new BlockPuzzle(BlockOrder);

            // Solve it!
            BlockPuzzle.Solve(FourByFourbyFour);

            // End time
            System.Console.WriteLine("Program ending at {0}.", System.DateTime.Now.ToLongTimeString());
            System.Console.WriteLine("{0} total positions considered!", FourByFourbyFour.PositionCount);
            System.Console.ReadLine();
        }
    }

    // The state of the puzzle: what points have been set in stone (wood) and which spaces are still open.
    public class BlockPuzzle
    {
        public Point StartingPoint; // In the grid, where does it start?
        public List<BlockLine> BlockLineList = new List<BlockLine>(); // The list of blocklines
        public int BlockLineIndex = 0; // solving index in the blockline list
        public bool [,,] TheGrid = new bool[4,4,4]; // A representation of the filled/not filled state of every unit in the grid
        public UInt64 PositionCount = 0; // For fun, record the number of total positions considered

        public BlockPuzzle(int[] blockLineOrder)
        {
            // Set up the list of blocks
            for(int i = 0; i < blockLineOrder.Length; i++)
            {
                this.BlockLineList.Add(new BlockLine(blockLineOrder[i], i, this)); // Add a new blockline to the list
            }
        }
        
        // Add the next blockline, return true if successful, false if not
        private bool AddBlockLine(int blockLineIndex)
        {
            // For fun, update the number of positions tried.
            this.PositionCount++;

            bool additionSuccess = false;

            // Update block endpoint
            this.BlockLineList[blockLineIndex].EndPoint = this.BlockLineList[blockLineIndex].StartPoint +
                ((this.BlockLineList[blockLineIndex].NumBlocks -1 ) * Point.DeltaPoints[this.BlockLineList[blockLineIndex].Orientation]);

            // Will it fit in the 4x4 grid? And is there space?
            if (this.BoundsCheck(this.BlockLineList[blockLineIndex]) && this.SpaceCheck(this.BlockLineList[blockLineIndex]))
            {
                // If so, set the block!
                this.BlockLineList[blockLineIndex].LockedIn = true;
                additionSuccess = true;
            }

            return additionSuccess;
        }
        
        // Checks to make sure that all the blocks are in bounds (within the 4x4 cube dimensions)
        public bool BoundsCheck(BlockLine block)
        {
            return (block.EndPoint.XPos > -1 && block.EndPoint.XPos <4) &&
				   (block.EndPoint.YPos > -1 && block.EndPoint.YPos <4) &&
				   (block.EndPoint.ZPos > -1 && block.EndPoint.ZPos <4);
        }

        // Print out the puzzle in human-readable notation
        public void PrintPuzzle()
        {
            Console.WriteLine("Solution starting point: {0}, {1}, {2}", this.StartingPoint.XPos, this.StartingPoint.YPos, this.StartingPoint.ZPos);

            foreach (BlockLine blockLine in this.BlockLineList)
            {
                Console.WriteLine("Block {0}, size {1}: {2}", blockLine.BlockIndex, blockLine.NumBlocks, blockLine.Orientation);
            }
        }

        // Is there space to add the blockline (input)? If so, return true, false if not
        public bool SpaceCheck(BlockLine blockLine)
        {
            bool open = true;
            Point checkPoint = blockLine.StartPoint;
						
            // Get a delta point to step in the right direction
            Point unitStep = Point.DeltaPoints[blockLine.Orientation];

            // Skip the starting point, because it's shared by the endpoint of the last block line, meaning that it's obviously not empty
            // start the for loop at 1
            checkPoint = checkPoint + unitStep;

			// Check if it's open
            for (int i = 1; i < blockLine.NumBlocks; i++)
            {
                if (this.TheGrid[checkPoint.XPos, checkPoint.YPos, checkPoint.ZPos])
                {
					open = false;
					break;
				}
                checkPoint = checkPoint + unitStep;				
            }
			
            return open;
        }

        // Start off by enumerating all the starting positions and adding them to the list
        public static void Solve(BlockPuzzle blockPuzzle)
        {
            bool blockAdded = false;
            bool solutionFound = false;
            uint solutionCount = 0;

            // These are the unique starting points
            // all other starting points are equivalent by symmetry in a 4x4x4 cube
            // TODO: Build up a general model for unique starting points for cube size n 
            // NOTE: This will spit out some solutions that are mirrors of each other. Although they're still "unique",
            //       future work would be to prune out the mirrors. 
            //       (Ex: "start in the corner (0,0,0) and move left" is rotationally symmetric to "start in the corner (0,0,0) and move right")
            //       (Ex: "start at 0,1,0, move up, then left" is vertically symmetric to "start at 0,1,0, move up, then right"
            List<Point> startingPoints = new List<Point>()
            {
                new Point(0,0,0),
                new Point(0,1,0),
                new Point(1,1,0),
                new Point(1,1,1),
            };

            // for each starting point, kick off the inner solver
            foreach (Point startingPoint in startingPoints)
            {
                // Start time
                System.Console.WriteLine("Beginning pass with start point {0}, {1}, {2} at {3}. ====================",
                    startingPoint.XPos, startingPoint.YPos, startingPoint.ZPos, System.DateTime.Now.ToLongTimeString());

                // Update the starting point for the puzzle and the first block line
                blockPuzzle.StartingPoint = startingPoint;
                blockPuzzle.BlockLineList[0].StartPoint = startingPoint;

                // Since that's the starting point and we *know* that a block will be there, set that block in the grid
                blockPuzzle.TheGrid[blockPuzzle.StartingPoint.XPos, blockPuzzle.StartingPoint.YPos, blockPuzzle.StartingPoint.ZPos] = true;

                // Reset the starting link and orientation
                blockPuzzle.BlockLineIndex = 0;
                blockPuzzle.BlockLineList[blockPuzzle.BlockLineIndex].ZeroOrientation(); // no prior orientation

                // This is the main loop!
                while (blockPuzzle.BlockLineIndex >= 0)
                {
                    // Try the next link and orientation. Does it fit? If so, add it in!
                    blockAdded = blockPuzzle.AddBlockLine(blockPuzzle.BlockLineIndex);

                    if (blockAdded)
                    {
                        // increment the link index
                        blockPuzzle.BlockLineIndex++;

                        // Are we done? Is that the last block? IS THAT A SOLUTION???
                        solutionFound = blockPuzzle.BlockLineIndex >= blockPuzzle.BlockLineList.Count;
                        if (solutionFound)
                        {
                            Console.WriteLine("Solution #" + (solutionCount++ + 1) + " ================================================================"); // Add one to start with "Solution 1"
                            blockPuzzle.PrintPuzzle();
                            // special last block line case, take the index back down to the last block
                            blockPuzzle.BlockLineIndex--;
                        }
                        // if not, reset the new link's start pos and orientation in light of the last link's orientation
                        else
                        {
                            blockPuzzle.BlockLineList[blockPuzzle.BlockLineIndex].StartPoint = blockPuzzle.BlockLineList[blockPuzzle.BlockLineIndex - 1].EndPoint;
                            blockPuzzle.BlockLineList[blockPuzzle.BlockLineIndex].LastLineOrientation = blockPuzzle.BlockLineList[blockPuzzle.BlockLineIndex - 1].Orientation;
                            blockPuzzle.BlockLineList[blockPuzzle.BlockLineIndex].ZeroOrientation();
                        }
                    }

                    // if the blockline couldn't be added OR we found a solution, rotate the block line in question in prep for next attempt
                    if (!blockAdded || solutionFound)
                    {
                        // rotate the link
                        if (blockPuzzle.BlockLineList[blockPuzzle.BlockLineIndex].RotateBlockLine())
                        {
                            // exit this loop, go back up to the top of the loop, and try again!
                        }
                        // if that was the last rotation, there are no more options for this link. back up one link and rotate 
                        else
                        {
                            do
                            {
                                // back up one
                                --(blockPuzzle.BlockLineIndex);
                            }
                            while ((blockPuzzle.BlockLineIndex >= 0)    // as long as you're not past the beginning
                                && (!blockPuzzle.BlockLineList[blockPuzzle.BlockLineIndex].RotateBlockLine())); // rotate and check, until there are no more rotates
                        }
                    }
                }

                // Clean up the starting point that we added in the beginning
                blockPuzzle.TheGrid[blockPuzzle.StartingPoint.XPos, blockPuzzle.StartingPoint.YPos, blockPuzzle.StartingPoint.ZPos] = false;

                // End time
                System.Console.WriteLine("Ending pass with start point {0}, {1}, {2} at {3}. =======================",
                    startingPoint.XPos, startingPoint.YPos, startingPoint.ZPos, System.DateTime.Now.ToLongTimeString());
            }
        }
    }

    // A collection of individual blocks that form a line
    public class BlockLine
    {
        public int NumBlocks; // How many are in the line
        public int BlockIndex; // This line's order in the overall puzzle
        private BlockPuzzle Puzzle; // keep a ref to the master puzzle

        public BlockLine(int numBlocks, int blockIndex, BlockPuzzle puzzle)
        {
            this.NumBlocks = numBlocks;
            this.BlockIndex = blockIndex;
            this.Puzzle = puzzle;
        }

        private bool lockedIn = false; // Is the line locked into the puzzle, with all blocks accounted for in the grid?
        public bool LockedIn
        {
            get { return this.lockedIn; }
            set
            {
                // If it's being locked and it wasn't already, or if it's being unlocked and it was already, 
                // reserve or unreserve the blocks in the grid
                if (value != this.lockedIn)
                {
                    this.SetUnset(value);
                }

                // save the value
                this.lockedIn = value;
            }
        }

        // Sets the blocks in a reserves the space (requestedState = true) or unsets the blocks and unreserves the space (requestedState = false)
        public void SetUnset(bool requestedState)
        {
            // A block point we're about to reserve/unreserve
            Point lockingPoint = this.StartPoint;

            // Get a delta point to step in the right direction
            Point unitStep = Point.DeltaPoints[this.Orientation];

            // Since block lines share their starting block with the end of the last block line, this starting point should already by set in
            // Shift one block down the line and start there. Make sure to start the for loop at 1 since you did this.
            lockingPoint = lockingPoint + unitStep;

            for (int i = 1; i < this.NumBlocks; i++)
            {
                // mark it as reserved/unreserved
                this.Puzzle.TheGrid[lockingPoint.XPos, lockingPoint.YPos, lockingPoint.ZPos] = requestedState;

                // move to the next one
                lockingPoint = lockingPoint + unitStep;
            }
        }

        private Point startPoint;
        public Point StartPoint
        {
            get { return this.startPoint; }
            set
            {
                // if the starting point is changing and the line was previously locked in, remove all the blocks from the grid
                if (value != this.startPoint)
                {
                    this.LockedIn = false;
                }

                // Update the point
                this.startPoint = value;
            }
        }

        private Point endPoint;
        public Point EndPoint
        {
            get { return this.endPoint; }
            set
            {
                // if the end point is changing and the line was previously locked in, remove all the blocks from the grid
                if (value != this.endPoint)
                {
                    this.LockedIn = false;
                }

                // Update the point
                this.endPoint = value;
            }
        }

        public LineOrientation LastLineOrientation = LineOrientation.NoOrientation; // The previous line's orientation. NoOrientation mean no previous line.
        private LineOrientation orientation = LineOrientation.Left; // This line's orientation. Instantiate with left so it starts not NoOrientation;
        public LineOrientation Orientation
        {
            get
            {
                return this.orientation;
            }
            set
            {
                if (this.orientation != value)
                {
                    // If you're changing rotation, you're no longer locked in
                    this.LockedIn = false;
                }
                this.orientation = value;
            }
        }

        // What direction does the block line point?
        public enum LineOrientation
        {
            Left,
            Right,
            Up,
            Down,
            Front,
            Back,
            NoOrientation
        };

        // Reset orientation to the first valid orientation (which will be left unless it's not valid)
        // Takes in the last blockline's orientation
        public void ZeroOrientation()
        {
            // Start with a Left line orientation, unless the last block was left or right (block lines must be 90deg from the last block line)
            this.Orientation = ((this.LastLineOrientation != LineOrientation.Left) && (this.LastLineOrientation != LineOrientation.Right)) ?
                LineOrientation.Left : LineOrientation.Up;
        }

        // Rotates a block's orientation through the progression of LineOrientations
        // Returns true if rotation succeeds. Returns false if all orientations have been
        // performed and no more remain.
        public bool RotateBlockLine()
        {
            // Rotate one position over!
            this.Orientation = this.Orientation == LineOrientation.Left ? LineOrientation.Right :
                                this.Orientation == LineOrientation.Right ? LineOrientation.Up :
                                this.Orientation == LineOrientation.Up ? LineOrientation.Down :
                                this.Orientation == LineOrientation.Down ? LineOrientation.Front :
                                this.Orientation == LineOrientation.Front ? LineOrientation.Back :
                                LineOrientation.NoOrientation; // If none of those, you're Back or NoOrientation - go to NoOrientation.

            // Remember, all rotations are at 90deg from the last blockline. If you're not 90deg, correct it.
            if ((this.Orientation == LineOrientation.Left || this.Orientation == LineOrientation.Right) &&
                (this.LastLineOrientation == LineOrientation.Left || this.LastLineOrientation == LineOrientation.Right))
                this.Orientation = LineOrientation.Up;

            else if ((this.Orientation == LineOrientation.Up || this.Orientation == LineOrientation.Down) &&
                (this.LastLineOrientation == LineOrientation.Up || this.LastLineOrientation == LineOrientation.Down))
                this.Orientation = LineOrientation.Front;

            else if ((this.Orientation == LineOrientation.Front || this.Orientation == LineOrientation.Back) &&
                (this.LastLineOrientation == LineOrientation.Front || this.LastLineOrientation == LineOrientation.Back))
                this.Orientation = LineOrientation.NoOrientation; // You're done. No more options!

            // Return true if we're not out of orientations (ie, it's not NoOrientation)
            return (this.Orientation != LineOrientation.NoOrientation);
        }
    }

    // This represents the position of a block in 4x4x4 space, x, y, z, 0 to 3
    public class Point
    {
        public int XPos;
        public int YPos;
        public int ZPos;

        public Point(int xPosIn, int yPosIn, int zPosIn)
        {
            this.XPos = xPosIn;
            this.YPos = yPosIn;
            this.ZPos = zPosIn;
        }
        
        // Adds point vectors together and returns the resultant point
        static public Point operator +(Point point1, Point point2)
        {
            return new Point(point1.XPos+point2.XPos, point1.YPos+point2.YPos, point1.ZPos+point2.ZPos);
        }

        // Subtracts point vectors together and returns the resultant point
        static public Point operator -(Point point1, Point point2)
        {
            return new Point(point1.XPos - point2.XPos, point1.YPos - point2.YPos, point1.ZPos - point2.ZPos);
        }

        // Multiplies point vectors together and returns the resultant point
        static public Point operator *(int multiplier, Point point)
        {
            return new Point(point.XPos * multiplier, point.YPos * multiplier, point.ZPos * multiplier);
        }

        // A lookup table of a single unit block based on orientation
        static public Dictionary<BlockLine.LineOrientation, Point> DeltaPoints = new Dictionary<BlockLine.LineOrientation, Point>()
        {
            {BlockLine.LineOrientation.Left, new Point(-1, 0, 0)},
            {BlockLine.LineOrientation.Right, new Point(1, 0, 0)},
            {BlockLine.LineOrientation.Up, new Point(0, 0, 1)},
            {BlockLine.LineOrientation.Down, new Point(0, 0, -1)},
            {BlockLine.LineOrientation.Front, new Point(0, -1, 0)},
            {BlockLine.LineOrientation.Back, new Point(0, 1, 0)},
            {BlockLine.LineOrientation.NoOrientation, new Point(0, 0, 0)}
        };
    }
}
