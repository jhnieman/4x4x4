using System;
using System.Collections.Generic;

namespace woodPuzzle
{
    // This program solves those cool wooden block puzzles where you're trying to take a long string
    // of connected wooden blocks and fold it into a solid 4x4 cube.
    // Example: https://www.solve-it-puzzles.com/products/snake-cube-4x4-1
    //
    // To solve, insert the links lengths into BlockOrder, then run it!
    class Program
    {
        // The order of links in the blocks
        // This notation was chosen for ease and could be adjusted
        // The notation describes the first link line at full length (2 blocks long)
        // but every link after that is just the number of additional blocks (since links
        // blocks on the end of the link with the adjacent link).
        public static readonly int[] BlockOrder = {2, 1, 2, 1, 1, 3, 1, 2, 1, 2, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 1, 1, 1, 1, 1, 2, 3, 1, 1, 1, 3, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 1, 0};

        static void Main(string[] args)
        {
            System.Console.WriteLine("Starting at {0}.", System.DateTime.Now.ToLongTimeString());
            BlockPuzzle FourByFour = new BlockPuzzle(BlockOrder);

            recurSolverStarter(FourByFour);
            System.Console.WriteLine("Ending at {0}.", System.DateTime.Now.ToLongTimeString());
        }

        private static void StackSolver(BlockPuzzle blockPuzzle, BlockLine.Orientation nextOrientation)
        {
            if(!blockPuzzle.isFinished)
            {
                if (blockPuzzle.AddBlockLine(nextOrientation))
                {
                    StackSolver(blockPuzzle, BlockLine.Orientation.Left);
                    StackSolver(blockPuzzle, BlockLine.Orientation.Right);
                    StackSolver(blockPuzzle, BlockLine.Orientation.Up);
                    StackSolver(blockPuzzle, BlockLine.Orientation.Down);
                    StackSolver(blockPuzzle, BlockLine.Orientation.Front);
                    StackSolver(blockPuzzle, BlockLine.Orientation.Back);

                    blockPuzzle.RemoveLastBlockLine();
                }
            }
        }

        private static void recurSolverStarter(BlockPuzzle blockPuzzle)
        {
            List<Point> startingPoints = new List<Point>()
            {
                new Point(0,0,0), 
                new Point(0,1,0), 
                new Point(1,0,0),
                new Point(1,1,0),
                //new Point(0,0,1), 
                //new Point(0,1,1), 
                new Point(1,0,1),
                new Point(1,1,1),
            };

            foreach (Point startingPoint in startingPoints)
            {
                BlockLine startingBlock = new BlockLine(BlockOrder[0] + 1, BlockLine.Orientation.Back);
                startingBlock.startPoint = startingPoint;
                startingBlock.endPoint = new Point(startingPoint.xPos, startingPoint.yPos+2, startingPoint.zPos);

                blockPuzzle.AddBlockLineDirectly(startingBlock);

                StackSolver(blockPuzzle, BlockLine.Orientation.Left);
                StackSolver(blockPuzzle, BlockLine.Orientation.Right);
                StackSolver(blockPuzzle, BlockLine.Orientation.Up);
                StackSolver(blockPuzzle, BlockLine.Orientation.Down);
                StackSolver(blockPuzzle, BlockLine.Orientation.Front);
                StackSolver(blockPuzzle, BlockLine.Orientation.Back);

                blockPuzzle.RemoveLastBlockLine();
                Console.WriteLine("Just finished up the pass for starting point {0}, {1}, {2} : {3}",
                    startingPoint.xPos, startingPoint.yPos, startingPoint.zPos, System.DateTime.Now.ToLongTimeString());
            }
        }

    }

    class BlockLine
    {
        public int numBlocks;
        public Point startPoint;
        public Point endPoint;
        public Orientation orientation;

        public BlockLine(int numBlocks, Orientation orientation)
        {
            this.numBlocks = numBlocks;
            this.orientation = orientation;
        }

        public enum Orientation
        {
            Left,
            Right,
            Up,
            Down,
            Front,
            Back
        };
    }

    class BlockPuzzle
    {
        public Point startingPoint;
        public Point endingPoint;
        public List<BlockLine> listOfBlocks = new List<BlockLine>();
        public int[] blockLineOrder;
        public int blockLineIndex=0;
        public bool isFinished = false;
        public int lowestRemoved = 9999;
        public bool [,,] theGrid = new bool[4,4,4];


        public BlockPuzzle(int[] blockLineOrder)
        {
            this.blockLineOrder = blockLineOrder;
            this.startingPoint = new Point(0,0,0);
            this.endingPoint = new Point(0,0,0);
        }

        public bool AddBlockLine(BlockLine.Orientation orientation){
            BlockLine.Orientation lastBlockOrientation;
            bool additionSuccess = false;

            int numBlocks = this.blockLineOrder[this.blockLineIndex];
            BlockLine addedBlock = new BlockLine(numBlocks, orientation);
            
            if(this.listOfBlocks.Count==0){
                // This is the first block, so we need to add one block
                addedBlock.numBlocks++;
                addedBlock.startPoint = new Point(0,0,0);
                addedBlock.endPoint = new Point(0,0,addedBlock.numBlocks-1);
                this.SpaceCheck(addedBlock, new Point(0, 0, 1));
                additionSuccess = true;
            }else{
                lastBlockOrientation = listOfBlocks[listOfBlocks.Count-1].orientation;

                switch(orientation){
                    case BlockLine.Orientation.Back:
                        if(lastBlockOrientation!=BlockLine.Orientation.Back && lastBlockOrientation!=BlockLine.Orientation.Front){
                            addedBlock.startPoint = new Point(this.endingPoint.xPos, this.endingPoint.yPos+1, this.endingPoint.zPos);
                            addedBlock.endPoint = new Point(this.endingPoint.xPos, this.endingPoint.yPos+numBlocks, this.endingPoint.zPos);
                            if(this.BoundsCheck(addedBlock)){
								if(this.SpaceCheck(addedBlock, new Point(0,1,0)))
								{
									additionSuccess = true;
								}
							}
                        }
                        break;
                    case BlockLine.Orientation.Front:
                        if (lastBlockOrientation != BlockLine.Orientation.Back && lastBlockOrientation != BlockLine.Orientation.Front)
                        {
                            addedBlock.startPoint = new Point(this.endingPoint.xPos, this.endingPoint.yPos-1, this.endingPoint.zPos);
                            addedBlock.endPoint = new Point(this.endingPoint.xPos, this.endingPoint.yPos-numBlocks, this.endingPoint.zPos);
                            if (this.BoundsCheck(addedBlock)) 
                            {
                                if(this.SpaceCheck(addedBlock, new Point(0,-1,0)))
								{
									additionSuccess = true;
								}
                            }
                        }
                        break;
                    case BlockLine.Orientation.Left:
                        if (lastBlockOrientation != BlockLine.Orientation.Left && lastBlockOrientation != BlockLine.Orientation.Right)
                        {
                            addedBlock.startPoint = new Point(this.endingPoint.xPos-1, this.endingPoint.yPos, this.endingPoint.zPos);
                            addedBlock.endPoint = new Point(this.endingPoint.xPos-numBlocks, this.endingPoint.yPos, this.endingPoint.zPos);
                            if (this.BoundsCheck(addedBlock))
							{
								if(this.SpaceCheck(addedBlock, new Point(-1,0,0)))
								{
									additionSuccess = true;
								}
							}
                        }
                        break;
                    case BlockLine.Orientation.Right:
                        if (lastBlockOrientation != BlockLine.Orientation.Left && lastBlockOrientation != BlockLine.Orientation.Right)
                        {
                            addedBlock.startPoint = new Point(this.endingPoint.xPos+1, this.endingPoint.yPos, this.endingPoint.zPos);
                            addedBlock.endPoint = new Point(this.endingPoint.xPos+numBlocks, this.endingPoint.yPos, this.endingPoint.zPos);
                            if (this.BoundsCheck(addedBlock)) 
							{
								if(this.SpaceCheck(addedBlock, new Point(1,0,0)))
								{
									additionSuccess = true;
								}
							}
                        }
                        break;
                    case BlockLine.Orientation.Up:
                        if (lastBlockOrientation != BlockLine.Orientation.Up && lastBlockOrientation != BlockLine.Orientation.Down)
                        {
                            addedBlock.startPoint = new Point(this.endingPoint.xPos, this.endingPoint.yPos, this.endingPoint.zPos+1);
                            addedBlock.endPoint = new Point(this.endingPoint.xPos, this.endingPoint.yPos, this.endingPoint.zPos+numBlocks);
                            if (this.BoundsCheck(addedBlock))
							{
								if(this.SpaceCheck(addedBlock, new Point(0,0,1)))
								{
									additionSuccess = true;
								}
							}
                        }
                        break;
                    case BlockLine.Orientation.Down:
                        if (lastBlockOrientation != BlockLine.Orientation.Up && lastBlockOrientation != BlockLine.Orientation.Down)
                        {
                            addedBlock.startPoint = new Point(this.endingPoint.xPos, this.endingPoint.yPos, this.endingPoint.zPos-1);
                            addedBlock.endPoint = new Point(this.endingPoint.xPos, this.endingPoint.yPos, this.endingPoint.zPos-numBlocks);
                            if (this.BoundsCheck(addedBlock))
							{
								if(this.SpaceCheck(addedBlock, new Point(0,0,-1)))
								{
									additionSuccess = true;
								}
							}
                        }
                        break;
                }
            }

            if(additionSuccess){
                this.listOfBlocks.Add(addedBlock);
                this.endingPoint = addedBlock.endPoint;

                //Console.WriteLine("Just added block {0} with orientation {1}.", this.blockLineIndex, addedBlock.orientation);

                this.blockLineIndex++;

                if (this.blockLineOrder[this.blockLineIndex] == 0)
                {
                    this.isFinished = true;
                    Console.WriteLine("==Solution==================================================================================");
                    this.PrintPuzzle();
                }
            }

            return additionSuccess;
        }

        public bool AddBlockLineDirectly(BlockLine addedBlock)
        {
            bool added = false;
            if(this.BoundsCheck(addedBlock)){
                if(this.SpaceCheck(addedBlock, Point.deltaPoints[addedBlock.orientation]))
                {
                    this.listOfBlocks.Add(addedBlock);
                    this.endingPoint = addedBlock.endPoint;
                    this.blockLineIndex++;

                    added = true;

                    //Console.WriteLine("Just added block {0} with orientation {1}.", this.blockLineIndex, addedBlock.orientation);

                    if (this.blockLineOrder[blockLineIndex - 1] == 0) this.isFinished = true;
                }
			}

            return added;
        }

        public bool BoundsCheck(BlockLine block)
        {
            return (block.endPoint.xPos > -1 && block.endPoint.xPos <4) &&
				   (block.endPoint.yPos > -1 && block.endPoint.yPos <4) &&
				   (block.endPoint.zPos > -1 && block.endPoint.zPos <4);
        }

        public void RemoveLastBlockLine()
        {
			int xVal;
			int yVal;
			int zVal;
		
            //if (this.blockLineIndex - 1 < this.lowestRemoved)
            //{
                //Console.WriteLine("Removed {0} at {1}.", this.blockLineIndex - 1, System.DateTime.Now.ToLongTimeString());
                //this.lowestRemoved = blockLineIndex - 1;
            //}
			
			xVal = this.listOfBlocks[this.blockLineIndex-1].startPoint.xPos;
			yVal = this.listOfBlocks[this.blockLineIndex-1].startPoint.yPos;
			zVal = this.listOfBlocks[this.blockLineIndex-1].startPoint.zPos;
           
            Point deltaPoint = Point.deltaPoints[this.listOfBlocks[this.blockLineIndex-1].orientation];
			
			for(int i=0; i<this.listOfBlocks[this.blockLineIndex-1].numBlocks; i++)
            {
                this.theGrid[xVal, yVal, zVal] = false;

                xVal = xVal + deltaPoint.xPos;
                yVal = yVal + deltaPoint.yPos;
                zVal = zVal + deltaPoint.zPos;
            }
			
            this.listOfBlocks.RemoveAt(this.listOfBlocks.Count-1);
			this.blockLineIndex--;
            if (this.blockLineIndex == 0)
            {
                this.endingPoint = new Point(0, 0, 0);
            }
            else
            {
                this.endingPoint = this.listOfBlocks[this.blockLineIndex - 1].endPoint;
            }
            
            this.isFinished = false;
        }

        public bool isValid()
        {
            // Need to check here if each space is full
            // OR if there are any overlaps...
            //
            // For now, call it good
            return true;
        }

        public void PrintPuzzle()
        {
            int counter = 0;
            int printOffset = 0;
            foreach (BlockLine blockLine in this.listOfBlocks)
            {
                if (counter == 0) printOffset = 1;
                Console.WriteLine("Block {0}, size {1}: {2}", counter, blockLine.numBlocks + printOffset, blockLine.orientation);
                counter++;
            }
        }

        public bool SpaceCheck(BlockLine blockLine, Point deltaPoint)
        {
            bool open = true;
			int xVal = blockLine.startPoint.xPos;
            int yVal = blockLine.startPoint.yPos;
            int zVal = blockLine.startPoint.zPos;
						
			// Check if it's open
            for (int i = 0; i < blockLine.numBlocks; i++)
            {
                if (this.theGrid[xVal, yVal, zVal]){
					open=false;
					break;
				}
				xVal = xVal + deltaPoint.xPos;
				yVal = yVal + deltaPoint.yPos;
				zVal = zVal + deltaPoint.zPos;				
            }
			
			// If so, write the ones going back through the blocks
			if(open){
				xVal = blockLine.startPoint.xPos;
				yVal = blockLine.startPoint.yPos;
				zVal = blockLine.startPoint.zPos;
				
				for (int i = 0; i < blockLine.numBlocks; i++)
				{
					this.theGrid[xVal, yVal, zVal] = true;
					
					xVal = xVal + deltaPoint.xPos;
					yVal = yVal + deltaPoint.yPos;
					zVal = zVal + deltaPoint.zPos;	
				}
			}
			
            return open;
        }
    }

    class Point
    {
        public int xPos;
        public int yPos;
        public int zPos;

        public Point(int xPosIn, int yPosIn, int zPosIn)
        {
            this.xPos = xPosIn;
            this.yPos = yPosIn;
            this.zPos = zPosIn;
        }
        
        static public Point PointAdd(Point point1, Point point2)
        {
            return new Point(point1.xPos+point2.xPos, point1.yPos+point2.yPos, point1.zPos+point2.zPos);
        }

        static public Dictionary<BlockLine.Orientation, Point> deltaPoints = new Dictionary<BlockLine.Orientation, Point>()
            {
                {BlockLine.Orientation.Left, new Point(-1, 0, 0)},
                {BlockLine.Orientation.Right, new Point(1, 0, 0)},
                {BlockLine.Orientation.Up, new Point(0, 0, 1)},
                {BlockLine.Orientation.Down, new Point(0, 0, -1)},
                {BlockLine.Orientation.Front, new Point(0, -1, 0)},
                {BlockLine.Orientation.Back, new Point(0, 1, 0)},
            };
    }
}
