using System;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace GlobalConquest.HexMapEngine.Classes
{

    // ---
    // Notes:
    //
    // Calculations provided by Jeff Modzel in his article from The Code Project entitled:
    //
    //   Hexagonal grid for games and other projects - Part 1
    //
	//  
	//      h = short length (outside)
	//      r = long length (outside)
	//      side = length of a side of the hexagon, all 6 are equal length
	//
	//      h = sin (30 degrees) x side
	//      r = cos (30 degrees) x side
	//
	//		    h
	//	        ---
	//           ____     |r
	//          /    \    |          
	//         /      \   |
	//         \      /
	//          \____/
	//
	//           Flat orientation (scale is off)
	//
	//           /\
	//          /  \
	//         /    \
	//         |    |
	//         |    |
	//         |    |
	//         \    /
	//          \  /
	//           \/
    //
	//           Pointy orientation (scale is off)
    // ---


    public class HexTileMath
    {

        #region Class Level Constants

        #endregion


        #region Class Level Declarations

            private ArrayList  coHexLineXAxisOffsetArrayList = new ArrayList();
            private ArrayList  coHexLineYAxisOffsetArrayList = new ArrayList();
            private ArrayList  coHexLineXAxisStartAndEndPointsArrayList = new ArrayList();
            private ArrayList  coHexLineYAxisStartAndEndPointsArrayList = new ArrayList();
            
            private Microsoft.Xna.Framework.GraphicsDeviceManager  coGraphicsDeviceManager;

        #endregion


        #region Constructor(s)

            public HexTileMath()
            {
                Load_HexLineXAxisOffsetPointsArrayList();
                Load_HexLineYAxisOffsetPointsArrayList();
                Load_HexLineXAxisStartAndEndPointsArrayList();

            }


            public HexTileMath(Microsoft.Xna.Framework.GraphicsDeviceManager poGraphicsDeviceManager)
            {
    
                coGraphicsDeviceManager = poGraphicsDeviceManager;

                Load_HexLineXAxisOffsetPointsArrayList();
                Load_HexLineYAxisOffsetPointsArrayList();
                Load_HexLineXAxisStartAndEndPointsArrayList();
            }

        #endregion


        #region Private Methods - Load Data

            private void Load_HexLineXAxisOffsetPointsArrayList()
            {
                int        liXOffset = HexMapEngine.Structures.Global.MAP_TILE_OFFSET_X;
                int        liActualMapWidthInTiles = HexMapEngine.Structures.Global.ACTUAL_MAP_WIDTH_IN_TILES;

                ArrayList  loHexLineXAxisOffsetArrayList = new ArrayList();


                for (int liCnt = 0; liCnt < liActualMapWidthInTiles; liCnt++)
                {
                    if (liCnt == 0)
                    {
                        loHexLineXAxisOffsetArrayList.Add(0);
                    }
                    else
                    {
                        loHexLineXAxisOffsetArrayList.Add(liCnt * liXOffset);
                    }
                }


                coHexLineXAxisOffsetArrayList = (ArrayList)loHexLineXAxisOffsetArrayList.Clone();
            }


            private void Load_HexLineYAxisOffsetPointsArrayList()
            {
                int        liYOffset = HexMapEngine.Structures.Global.MAP_TILE_OFFSET_Y;
                int        liActualMapHeightInTiles = HexMapEngine.Structures.Global.ACTUAL_MAP_HEIGHT_IN_TILES;

                ArrayList  loHexLineYAxisOffsetArrayList = new ArrayList();


                for (int liCnt = 0; liCnt < liActualMapHeightInTiles; liCnt++)
                {
                    if (liCnt == 0)
                    {
                        loHexLineYAxisOffsetArrayList.Add(0);
                    }
                    else
                    {
                        loHexLineYAxisOffsetArrayList.Add(liCnt * liYOffset);
                    }
                }


                coHexLineYAxisOffsetArrayList = (ArrayList)loHexLineYAxisOffsetArrayList.Clone();
            }


            private void Load_HexLineXAxisStartAndEndPointsArrayList()
            {
                // ---
                // loads each line of a standard hex image with its x position start and end points
                // as well as the y position increment    
                //
                // this array-list represents the entire set of x-start and x-end positions for each
                // pixel line in a standard hex tile image as the image falls within the boundaries
                // of a square of 72 pixels by 72 pixels.
                //
                // if a different sized base-square is to be used, these x-start and x-end positions
                // will have to be recalculated based on the new size
                //
                // the reason for the repetition of entries such as the following is that with the 
                // exception of the top-most and bottom-most pixel lines within the hexagon image,
                // all increments of change within the hexagonal image are done by two lines, which is
                // a result of the actual painting of the graphic image within the standard square
                //
                //    first pixel line in actual hexagon image array-list entries -> 18, 53, 0
                //          
	            //    ____ 18, 53  top-most image pixel line starts at x-pos 18, ends x-pos 53
	            //   /    \             
	            //  /      \  0, 71 middle image pixel line starts at x-pos 0, ends x-pos 71 (width of sqaure)   
	            //  \      /  0, 71 middle image pixel line starts at x-pos 0, ends x-pos 71 (width of sqaure)
	            //   \____/ 
                //         18, 53  bottom-most image pixel line starts at x-pos 18, ends x-pos 53
                //
                //    *** notice that the middle pixel lines require to graphic lines
                //
                // to be used for determining of a mouse click has been made in any displayed   
                // hexagonal tile
                // 
                // each line of data points represents the start and end points on the x axis and 
                // +1 on the y axis
                //
                // all numbers are zero-based
                // ---

                ArrayList loHexLineXAxisStartAndEndPointsArrayList = new ArrayList();


                loHexLineXAxisStartAndEndPointsArrayList.Add("18,53,0");
                loHexLineXAxisStartAndEndPointsArrayList.Add("17,54,1");
                loHexLineXAxisStartAndEndPointsArrayList.Add("17,54,2");
                loHexLineXAxisStartAndEndPointsArrayList.Add("16,55,3");
                loHexLineXAxisStartAndEndPointsArrayList.Add("16,55,4");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("15,56,5");
                loHexLineXAxisStartAndEndPointsArrayList.Add("15,56,6");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("14,57,7");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("14,57,8");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("13,58,9");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("13,58,10");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("12,59,11");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("12,59,12");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("11,60,13");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("11,60,14");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("10,61,15");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("10,61,16");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("9,62,17");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("9,62,18");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("8,63,19");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("8,63,20");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("7,64,21");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("7,64,22");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("6,65,23");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("6,65,24");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("5,66,25");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("5,66,26");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("4,67,27");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("4,67,28");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("3,68,29");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("3,68,30");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("2,69,31");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("2,69,32");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("1,70,33");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("1,70,34");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("0,71,35");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("0,71,36");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("1,70,37");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("1,70,38");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("2,69,39");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("2,69,40");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("3,68,41");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("3,68,42");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("4,67,43");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("4,67,44");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("5,66,45");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("5,66,46");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("6,65,47");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("6,65,48");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("7,64,49");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("7,64,50");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("8,63,51");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("8,63,52");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("9,62,53");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("9,62,54");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("10,61,55");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("10,61,56");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("11,60,57");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("11,60,58");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("12,59,59");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("12,59,60");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("13,58,61");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("13,58,62");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("14,57,63");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("14,57,64");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("15,56,65");
                loHexLineXAxisStartAndEndPointsArrayList.Add("15,56,66");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("16,55,67");
                loHexLineXAxisStartAndEndPointsArrayList.Add("16,55,68");    
                loHexLineXAxisStartAndEndPointsArrayList.Add("17,54,69");
                loHexLineXAxisStartAndEndPointsArrayList.Add("17,54,70");
                loHexLineXAxisStartAndEndPointsArrayList.Add("18,53,71");

                coHexLineXAxisStartAndEndPointsArrayList = (ArrayList)loHexLineXAxisStartAndEndPointsArrayList.Clone();
            }

        #endregion


        #region Internal Methods - Determine if Mouse Position Inside a Hexagon

            public bool Is_PointInHexMapRectangle(int piXPosition, int piYPosition)
		    {
			    // ---
			    // Quick check to see if X,Y coordinate is even in the bounding rectangle of the board.
			    // Can produce a false positive because of the staggerring effect of hexes around the edge
			    // of the board, but can be used to rule out an x,y point.
                // ---

                int liTopLeftXPosition = 0;    // + HexMapEngine.Structures.Global.MAP_TILE_OFFSET_X;
                int liTopLeftYPosition = 0;    // + HexMapEngine.Structures.Global.MAP_TILE_OFFSET_Y;
                int liBottomRightX = liTopLeftXPosition + coGraphicsDeviceManager.GraphicsDevice.Viewport.Width;
                int liBottomRightY = liTopLeftXPosition + coGraphicsDeviceManager.GraphicsDevice.Viewport.Height;

			    if (piXPosition > liTopLeftXPosition && piXPosition < liBottomRightX && piYPosition > liTopLeftYPosition && piYPosition < liBottomRightY)
			    {
				    return true;
			    }
			    else 
			    {
				    return false;
			    }

		    }


            public bool IsPoint_InsideHexagon(ref HexMapEngine.Structures.HexTile poHexTileSelected, int piXMousePosition, int piYMousePosition) 
            {
                // DATA POINTS
                //   [0,0],X Offset: 0,Y Offset: 0,
                //   [0,1],X Offset: 54,Y Offset: 36,
                //   [0,2],X Offset: 108,Y Offset: 0,
                //   [0,3],X Offset: 162,Y Offset: 36,
                //   [1,0],X Offset: 0,Y Offset: 72,
                //   [1,1],X Offset: 54,Y Offset: 108,
                //   [1,2],X Offset: 108,Y Offset: 72,
                //   [1,3],X Offset: 162,Y Offset: 108,


                int       liXIndex = 0;
                int       liYIndex = 0;

                bool      lboolXIndexFound = false;
                bool      lboolYIndexFound = false;

                HexMapEngine.Structures.HexTile loHexTileSelected;
                
                
                // find selected column\index via x-based start and end tile x-points
                for (liXIndex = 0; liXIndex < HexMapEngine.Structures.Global.ACTUAL_MAP_WIDTH_IN_TILES; liXIndex++)
                {
                    if (Is_XMousePositionWithinHexTileBoundaries(HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY[0, liXIndex].MAP_TILE_POSITION_X, piXMousePosition))
                    {
                        lboolXIndexFound = true;
                        break;
                    }
                }


                if (lboolXIndexFound)
                {
                    // find selected row\index via y-based start and end tile y-points
                    for (liYIndex = 0; liYIndex < HexMapEngine.Structures.Global.ACTUAL_MAP_HEIGHT_IN_TILES; liYIndex++)
                    {
                        if (Is_YMousePositionWithinHexTileBoundaries(HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY[liYIndex, liXIndex].MAP_TILE_POSITION_Y, piYMousePosition))
                        {
                            lboolYIndexFound = true;
                            break;
                        }
                    }


                    // find which hex tile has been selected
                    if (lboolYIndexFound)
                    {    
                        loHexTileSelected = HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY[liYIndex, liXIndex];

                        loHexTileSelected.HEX_TILE_SELECTED = true;

                        // update passed hex-tile structure & hex-tile structure in global array
                        poHexTileSelected = loHexTileSelected;
                        HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY[liYIndex, liXIndex] = loHexTileSelected;

                        // update global property for selectyed hex-tile-id
                        HexMapEngine.Structures.Global.SELECTED_HEX_TILE_ID = loHexTileSelected.TILE_ID.Trim();
    
                        return (true);
                    }

                }

                return (false);
            }


            private bool Is_XMousePositionWithinHexTileBoundaries(int piXPositionOffset, int piXMousePosition)
            {
                int       liXStartPosition = 0;
                int       liXEndPosition = 0;

                string[]  loXYPositions;

                foreach (string lsXBoundaries in coHexLineXAxisStartAndEndPointsArrayList)
                {
                    loXYPositions = lsXBoundaries.Split(',');

                    liXStartPosition = Convert.ToInt32(loXYPositions[0]);
                    liXEndPosition = Convert.ToInt32(loXYPositions[1]);

                    if ((piXMousePosition >= (piXPositionOffset + liXStartPosition)) && (piXMousePosition <= (piXPositionOffset + liXEndPosition)))
                    {
                        return (true);
                    }
                }

                return (false);
            }


            private bool Is_YMousePositionWithinHexTileBoundaries(int piYPositionOffset, int piYMousePosition)
            {
                int       liYEndPosition = 71;

 
                if ((piYMousePosition >= piYPositionOffset) && (piYMousePosition <= (piYPositionOffset + liYEndPosition)))
                {
                    return (true);
                }
 
                return (false);
            }

        #endregion

    }

}
