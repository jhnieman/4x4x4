# 4x4x4
## Summary

Solver for 4x4x4 snake

<https://smile.amazon.com/Snake-King-4x4x4-medium-puzzle/dp/B006IGBY6O?sa-no-redirect=1>

This this is a simple C# project I worked on for fun. There are many more optimizations, but this is a good starting point.

## Directory Overview

- WoodPuzzle was original solution
- WoodPuzzleFaster was the better, non-recursive version
- WoodPuzzleFasterThreaded was neat version using some of the Parallels stuff in C# 4.5 for easy parallelization across threads, which added a nice little perf boost.

## Running It

The last solution, *Threaded is the most fun. To run it, open up the VS solution in that folder and hit F5.