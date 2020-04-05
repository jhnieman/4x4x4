using System;
using System.Collections.Generic;

namespace woodPuzzle
{
    // This program solves those cool wooden block puzzles where you're trying to take a long string
    // of connected wooden blocks and fold it into a solid 4x4 cube.
    // Example: https://www.solve-it-puzzles.com/products/snake-cube-4x4-1
    //
    // To solve, insert the links lengths into BlockOrder, then run it!
    public class Program
    {
        // The order of lines/links in the blocks
        // The notation describes the first block/link at full length (2 blocks long)
        // but every link after that is just the number of additional blocks (since lines
        // share blocks on the end of the link with the adjacent link).
        // 0 terminated.
        public static readonly int[] BlockOrder = {2, 1, 2, 1, 1, 3, 1, 2, 1, 2, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 1, 1, 1, 1, 1, 2, 3, 1, 1, 1, 3, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 1, 0};

        public static void Main(string[] args)
        {
            // Start time
            System.Console.WriteLine("Starting at {0}.", System.DateTime.Now.ToLongTimeString());
            
            // Set up the puzzle
            BlockPuzzle FourByFour = new BlockPuzzle(BlockOrder);

            // Solve it!
            PuzzleSolver(FourByFour);

            // End time
            System.Console.WriteLine("Ending at {0}.", System.DateTime.Now.ToLongTimeString());
            System.Console.WriteLine("{0} total positions considered!", FourByFour.PositionCount);
            System.Console.ReadKey();
        }

        // Start off by enumerating all the starting positions and adding them to the list
        private static void PuzzleSolver(BlockPuzzle blockPuzzle)
        {
            // These are the unique starting points, based on the first block size
            List<Point> startingPoints = new List<Point>()
            {
                new Point(0,0,0), 
                new Point(0,1,0), 
                new Point(1,0,0),
                new Point(1,1,0),
                new Point(1,0,1),
                new Point(1,1,1),
            };

            // for each starting point, kick off the inner solver
            foreach (Point startingPoint in startingPoints)
            {
                // Start by going any direction, back chosen at random
                BlockLine startingBlock = new BlockLine(BlockOrder[0] + 1, BlockLine.BlockLineOrientation.Back); 
                startingBlock.StartPoint = startingPoint;
                startingBlock.EndPoint = new Point(startingPoint.XPos, startingPoint.YPos + BlockOrder[0], startingPoint.ZPos);

                // Add the first block to the stack
                blockPuzzle.AddBlockLineDirectly(startingBlock);

                // Throw each next direction onto the depth-first queue
                PuzzleSolverInnerLoop(blockPuzzle, BlockLine.BlockLineOrientation.Left);
                PuzzleSolverInnerLoop(blockPuzzle, BlockLine.BlockLineOrientation.Right);
                PuzzleSolverInnerLoop(blockPuzzle, BlockLine.BlockLineOrientation.Up);
                PuzzleSolverInnerLoop(blockPuzzle, BlockLine.BlockLineOrientation.Down);
                PuzzleSolverInnerLoop(blockPuzzle, BlockLine.BlockLineOrientation.Front);
                PuzzleSolverInnerLoop(blockPuzzle, BlockLine.BlockLineOrientation.Back);

                // Remove the first block to trigger the end of the routine
                blockPuzzle.RemovePreviousBlockLine();
                Console.WriteLine("Just finished up the pass for starting point {0}, {1}, {2} : {3}",
                    startingPoint.XPos, startingPoint.YPos, startingPoint.ZPos, System.DateTime.Now.ToLongTimeString());
            }
        }

        // This part does the work and is called link by link 
        private static void PuzzleSolverInnerLoop(BlockPuzzle blockPuzzle, BlockLine.BlockLineOrientation nextOrientation)
        {
            // If you're not done, keep working!
            if(!blockPuzzle.IsFinished)
            {
                // Try to add the requested orientation
                if (blockPuzzle.AddBlockLine(nextOrientation))
                {
                    // If adding the requested block succeeds, try to add another block in each direction!
                    PuzzleSolverInnerLoop(blockPuzzle, BlockLine.BlockLineOrientation.Left);
                    PuzzleSolverInnerLoop(blockPuzzle, BlockLine.BlockLineOrientation.Right);
                    PuzzleSolverInnerLoop(blockPuzzle, BlockLine.BlockLineOrientation.Up);
                    PuzzleSolverInnerLoop(blockPuzzle, BlockLine.BlockLineOrientation.Down);
                    PuzzleSolverInnerLoop(blockPuzzle, BlockLine.BlockLineOrientation.Front);
                    PuzzleSolverInnerLoop(blockPuzzle, BlockLine.BlockLineOrientation.Back);

                    // Once all directions have been tried, unroll the block you just placed from the master list
                    blockPuzzle.RemovePreviousBlockLine();
                }
            }
        }
    }

    // A collection of individual blocks in a line
    public class BlockLine
    {
        public int NumBlocks; // How many are in the line
        public Point StartPoint; 
        public Point EndPoint;
        public BlockLineOrientation Orientation;

        public BlockLine(int numBlocks, BlockLineOrientation orientation)
        {
            this.NumBlocks = numBlocks;
            this.Orientation = orientation;
        }

        // What direction does the block line point?
        public enum BlockLineOrientation
        {
            Left,
            Right,
            Up,
            Down,
            Front,
            Back
        };
    }

    // This class represents the state of the puzzle: what points have
    // been set in stone (wood) and which spaces are still open.
    public class BlockPuzzle
    {
        public Point StartingPoint; // In the grid, where does it start?
        public Point EndingPoint; // In the grid, where does it end?
        public List<BlockLine> ListOfBlocks = new List<BlockLine>(); // The list of blocklines
        public int[] BlockLineOrder; // Keep an internal list of these, just for completeness
        public int BlockLineIndex = 0; // index in the blockline
        public bool IsFinished = false; // Not true til it's done 
        public bool [,,] TheGrid = new bool[4,4,4]; // A representation of the filled/not filled state of every unit in the grid
        public UInt64 PositionCount = 0; // For fun, record the number of total positions considered


        public BlockPuzzle(int[] blockLineOrder)
        {
            this.BlockLineOrder = blockLineOrder; 
            this.StartingPoint = new Point(0,0,0);
            this.EndingPoint = new Point(0,0,0);
        }

        // Add the next blockline in the direction provided
        public bool AddBlockLine(BlockLine.BlockLineOrientation orientation)
        {
            // For fun, increment the number of total positions considered
            this.PositionCount++;

            BlockLine.BlockLineOrientation lastBlockOrientation;
            bool additionSuccess = false;

            int numBlocks = this.BlockLineOrder[this.BlockLineIndex];
            BlockLine addedBlock = new BlockLine(numBlocks, orientation);
            
            if(this.ListOfBlocks.Count==0)
            {
                // This is the first block, so we need to add one block
                addedBlock.NumBlocks++;
                addedBlock.StartPoint = new Point(0,0,0);
                addedBlock.EndPoint = new Point(0,0,addedBlock.NumBlocks-1);
                this.SpaceCheckAndReserve(addedBlock, new Point(0, 0, 1));
                additionSuccess = true;
            }
            else
            {
                // Find out what direction the last block line went in order to determine where the next one can go
                // Block lines are always at 90 degress to the last line
                lastBlockOrientation = ListOfBlocks[ListOfBlocks.Count-1].Orientation;
                switch(orientation)
                {
                    case BlockLine.BlockLineOrientation.Back:
                        if(lastBlockOrientation!=BlockLine.BlockLineOrientation.Back && lastBlockOrientation!=BlockLine.BlockLineOrientation.Front)
                        {
                            addedBlock.StartPoint = new Point(this.EndingPoint.XPos, this.EndingPoint.YPos+1, this.EndingPoint.ZPos);
                            addedBlock.EndPoint = new Point(this.EndingPoint.XPos, this.EndingPoint.YPos+numBlocks, this.EndingPoint.ZPos);
                            
                            // Will it fit in the 4x4 grid? And is there space?
                            if(this.BoundsCheck(addedBlock))
                            {
								if(this.SpaceCheckAndReserve(addedBlock, new Point(0,1,0)))
								{
									additionSuccess = true;
								}
							}
                        }
                        break;

                    case BlockLine.BlockLineOrientation.Front:
                        if (lastBlockOrientation != BlockLine.BlockLineOrientation.Back && lastBlockOrientation != BlockLine.BlockLineOrientation.Front)
                        {
                            addedBlock.StartPoint = new Point(this.EndingPoint.XPos, this.EndingPoint.YPos-1, this.EndingPoint.ZPos);
                            addedBlock.EndPoint = new Point(this.EndingPoint.XPos, this.EndingPoint.YPos-numBlocks, this.EndingPoint.ZPos);

                            // Will it fit in the 4x4 grid? And is there space?
                            if (this.BoundsCheck(addedBlock)) 
                            {
                                if(this.SpaceCheckAndReserve(addedBlock, new Point(0,-1,0)))
								{
									additionSuccess = true;
								}
                            }
                        }
                        break;

                    case BlockLine.BlockLineOrientation.Left:
                        if (lastBlockOrientation != BlockLine.BlockLineOrientation.Left && lastBlockOrientation != BlockLine.BlockLineOrientation.Right)
                        {
                            addedBlock.StartPoint = new Point(this.EndingPoint.XPos-1, this.EndingPoint.YPos, this.EndingPoint.ZPos);
                            addedBlock.EndPoint = new Point(this.EndingPoint.XPos-numBlocks, this.EndingPoint.YPos, this.EndingPoint.ZPos);

                            // Will it fit in the 4x4 grid? And is there space?
                            if (this.BoundsCheck(addedBlock))
							{
								if(this.SpaceCheckAndReserve(addedBlock, new Point(-1,0,0)))
								{
									additionSuccess = true;
								}
							}
                        }
                        break;

                    case BlockLine.BlockLineOrientation.Right:
                        if (lastBlockOrientation != BlockLine.BlockLineOrientation.Left && lastBlockOrientation != BlockLine.BlockLineOrientation.Right)
                        {
                            addedBlock.StartPoint = new Point(this.EndingPoint.XPos+1, this.EndingPoint.YPos, this.EndingPoint.ZPos);
                            addedBlock.EndPoint = new Point(this.EndingPoint.XPos+numBlocks, this.EndingPoint.YPos, this.EndingPoint.ZPos);

                            // Will it fit in the 4x4 grid? And is there space?
                            if (this.BoundsCheck(addedBlock)) 
							{
								if(this.SpaceCheckAndReserve(addedBlock, new Point(1,0,0)))
								{
									additionSuccess = true;
								}
							}
                        }
                        break;
                    
                    case BlockLine.BlockLineOrientation.Up:
                        if (lastBlockOrientation != BlockLine.BlockLineOrientation.Up && lastBlockOrientation != BlockLine.BlockLineOrientation.Down)
                        {
                            addedBlock.StartPoint = new Point(this.EndingPoint.XPos, this.EndingPoint.YPos, this.EndingPoint.ZPos+1);
                            addedBlock.EndPoint = new Point(this.EndingPoint.XPos, this.EndingPoint.YPos, this.EndingPoint.ZPos+numBlocks);

                            // Will it fit in the 4x4 grid? And is there space?
                            if (this.BoundsCheck(addedBlock))
							{
								if(this.SpaceCheckAndReserve(addedBlock, new Point(0,0,1)))
								{
									additionSuccess = true;
								}
							}
                        }
                        break;

                    case BlockLine.BlockLineOrientation.Down:
                        if (lastBlockOrientation != BlockLine.BlockLineOrientation.Up && lastBlockOrientation != BlockLine.BlockLineOrientation.Down)
                        {
                            addedBlock.StartPoint = new Point(this.EndingPoint.XPos, this.EndingPoint.YPos, this.EndingPoint.ZPos-1);
                            addedBlock.EndPoint = new Point(this.EndingPoint.XPos, this.EndingPoint.YPos, this.EndingPoint.ZPos-numBlocks);

                            // Will it fit in the 4x4 grid? And is there space?
                            if (this.BoundsCheck(addedBlock))
							{
								if(this.SpaceCheckAndReserve(addedBlock, new Point(0,0,-1)))
								{
									additionSuccess = true;
								}
							}
                        }
                        break;
                }
            }

            // Add the block to the list
            if(additionSuccess)
            {
                this.ListOfBlocks.Add(addedBlock);
                this.EndingPoint = addedBlock.EndPoint;

                this.BlockLineIndex++;

                // If you find the 0 termination at the end of the list, you're done!
                if (this.BlockLineOrder[this.BlockLineIndex] == 0)
                {
                    this.IsFinished = true;
                    Console.WriteLine("==Solution==================================================================================");
                    this.PrintPuzzle();
                }
            }

            return additionSuccess;
        }

        // Add a block line by taking in a BlockLine, not the next orientation
        // Used primarily for the first block in the puzzle.
        // Returns true if the line is added, false otherwise
        public bool AddBlockLineDirectly(BlockLine addedBlock)
        {
            // For fun, increment the number of total positions considered
            this.PositionCount++;

            bool added = false;
            if(this.BoundsCheck(addedBlock))
            {
                if(this.SpaceCheckAndReserve(addedBlock, Point.DeltaPoints[addedBlock.Orientation]))
                {
                    this.ListOfBlocks.Add(addedBlock);
                    this.EndingPoint = addedBlock.EndPoint;
                    this.BlockLineIndex++;

                    added = true;

                    // Just in case, check if this is the last block
                    if (this.BlockLineOrder[BlockLineIndex - 1] == 0) this.IsFinished = true;
                }
			}

            return added;
        }

        // Checks to make sure that all the blocks are in bounds (within the 4x4 cube dimensions)
        public bool BoundsCheck(BlockLine block)
        {
            return (block.EndPoint.XPos > -1 && block.EndPoint.XPos <4) &&
				   (block.EndPoint.YPos > -1 && block.EndPoint.YPos <4) &&
				   (block.EndPoint.ZPos > -1 && block.EndPoint.ZPos <4);
        }

        // Pull a block off the list
        public void RemovePreviousBlockLine()
        {
			int xVal;
			int yVal;
			int zVal;
			
			xVal = this.ListOfBlocks[this.BlockLineIndex-1].StartPoint.XPos;
			yVal = this.ListOfBlocks[this.BlockLineIndex-1].StartPoint.YPos;
			zVal = this.ListOfBlocks[this.BlockLineIndex-1].StartPoint.ZPos;
           
            // Get a size-one point delta in the direction of the blockline
            Point deltaPoint = Point.DeltaPoints[this.ListOfBlocks[this.BlockLineIndex-1].Orientation];
			
            // clear out the blocks
			for(int i=0; i<this.ListOfBlocks[this.BlockLineIndex-1].NumBlocks; i++)
            {
                this.TheGrid[xVal, yVal, zVal] = false;

                xVal = xVal + deltaPoint.XPos;
                yVal = yVal + deltaPoint.YPos;
                zVal = zVal + deltaPoint.ZPos;
            }
			
            // Remove the blocks from the list
            this.ListOfBlocks.RemoveAt(this.ListOfBlocks.Count-1);
			this.BlockLineIndex--;

            // Set the EndingPoint (special case if it's the first block)
            if (this.BlockLineIndex == 0)
            {
                this.EndingPoint = new Point(0, 0, 0);
            }
            else
            {
                this.EndingPoint = this.ListOfBlocks[this.BlockLineIndex - 1].EndPoint;
            }
            
            this.IsFinished = false;
        }

        // Print out the puzzle in human-readable notation
        public void PrintPuzzle()
        {
            int counter = 0;
            int printOffset = 0;
            foreach (BlockLine blockLine in this.ListOfBlocks)
            {
                if (counter == 0) printOffset = 1;
                Console.WriteLine("Block {0}, size {1}: {2}", counter, blockLine.NumBlocks + printOffset, blockLine.Orientation);
                counter++;
            }
        }

        // Is there space to add the blockline?
        // Takes in a blockline and a direction (delta point direction)
        // If successful, returns true and blocks off the used spaces in the grid
        public bool SpaceCheckAndReserve(BlockLine blockLine, Point deltaPoint)
        {
            bool open = true;
			int xVal = blockLine.StartPoint.XPos;
            int yVal = blockLine.StartPoint.YPos;
            int zVal = blockLine.StartPoint.ZPos;
						
			// Check if it's open
            for (int i = 0; i < blockLine.NumBlocks; i++)
            {
                if (this.TheGrid[xVal, yVal, zVal])
                {
					open = false;
					break;
				}
				xVal = xVal + deltaPoint.XPos;
				yVal = yVal + deltaPoint.YPos;
				zVal = zVal + deltaPoint.ZPos;				
            }
			
			// If so, write the ones going back through the blocks
			if(open)
            {
				xVal = blockLine.StartPoint.XPos;
				yVal = blockLine.StartPoint.YPos;
				zVal = blockLine.StartPoint.ZPos;
				
				for (int i = 0; i < blockLine.NumBlocks; i++)
				{
					this.TheGrid[xVal, yVal, zVal] = true;
					
					xVal = xVal + deltaPoint.XPos;
					yVal = yVal + deltaPoint.YPos;
					zVal = zVal + deltaPoint.ZPos;	
				}
			}
			
            return open;
        }
    }

    // This represents the position of a block in 4x4 space.
    // x, y, z, 0 to 3
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
        static public Point PointAdd(Point point1, Point point2)
        {
            return new Point(point1.XPos+point2.XPos, point1.YPos+point2.YPos, point1.ZPos+point2.ZPos);
        }

        // A lookup table of a single unit block based on orientation
        static public Dictionary<BlockLine.BlockLineOrientation, Point> DeltaPoints = new Dictionary<BlockLine.BlockLineOrientation, Point>()
            {
                {BlockLine.BlockLineOrientation.Left, new Point(-1, 0, 0)},
                {BlockLine.BlockLineOrientation.Right, new Point(1, 0, 0)},
                {BlockLine.BlockLineOrientation.Up, new Point(0, 0, 1)},
                {BlockLine.BlockLineOrientation.Down, new Point(0, 0, -1)},
                {BlockLine.BlockLineOrientation.Front, new Point(0, -1, 0)},
                {BlockLine.BlockLineOrientation.Back, new Point(0, 1, 0)},
            };
    }
}
