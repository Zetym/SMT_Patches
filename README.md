# SMT_Patches
BepInEx mod for Supermarket Together with various patches and bug fixes

## Current Patches
- **StatisticsDesk**
  - Fixes the issue of product statistics (products sold, products acquired, revenue per product, cost per product) only being recorded for the first completed day each session
  - Fixes the issue of product revenue and cost aggregating incorrectly by truncating all values *before* aggregating, which could sometimes lead to some products having a reported revenue and/or cost of 0, despite several being sold/bought that day
  - Fixes the issue of all daily stats being recorded as the day after, i.e. stats for day 1 were recorded as day 2, which also had the knock-on effect that you could only view stats from 2 days ago and earlier, not the day you just completed

### Important Note
The fixes for the StatisticsDesk change how the game reads and writes the stats file, and adding or removing this mod to a save with an existing stats file **will** cause issues of stats being misread, and can cause stats for certain days to become or appear lost.

 ## Installation
 - Install [BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html) (Tested with [v5.4.23.3](https://github.com/BepInEx/BepInEx/releases#release-v5.4.23.3))
 - Copy the .dll files into the game's `BepInEx/Plugins` directory
