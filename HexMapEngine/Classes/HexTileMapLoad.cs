using System;
using System.Diagnostics;
using System.Collections;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;


namespace GlobalConquest.HexMapEngine.Classes
    {

    public class HexTileMapLoad
    {

        #region Class Level Declarations

        private int ciActualMapHeightInTiles;
        private int ciActualMapWidthInTiles;


        #endregion


        #region Constructor(s)

        public HexTileMapLoad()
        {

        }

        public HexTileMapLoad(int piActualMapHeightInTiles,
                                int piActualMapWidthInTiles)
        {
            ciActualMapHeightInTiles = piActualMapHeightInTiles;
            ciActualMapWidthInTiles = piActualMapWidthInTiles;
        }

        #endregion


        #region Internal Methods

        public Microsoft.Xna.Framework.Graphics.SpriteFont Load_MyraUIDefaultSpriteFont(Game poThis)
        {
            Microsoft.Xna.Framework.Graphics.SpriteFont loSpriteFont;

            loSpriteFont = poThis.Content.Load<SpriteFont>("MySpriteFont");

            return (loSpriteFont);
        }


        public ArrayList Load_Textured2DArrayList(Game poThis)
        {
            ArrayList loTextured2DArrayListObjects = new ArrayList();

            HexMapEngine.Structures.HexTexture2D loHexTexture2D;

            Microsoft.Xna.Framework.Graphics.Texture2D loTexture2DTile;


            // load content texture images into array    
            loTexture2DTile = poThis.Content.Load<Texture2D>("DarkGrass_72x72");
            loHexTexture2D = new HexMapEngine.Structures.HexTexture2D();
            loHexTexture2D.TEXTURE2D_ID = 1;
            loHexTexture2D.TEXTURE2D_IMAGE_TILE = loTexture2DTile;
            loTextured2DArrayListObjects.Add(loHexTexture2D);


            loTexture2DTile = poThis.Content.Load<Texture2D>("YellowHexagonOutline_72x72");
            loHexTexture2D = new HexMapEngine.Structures.HexTexture2D();
            loHexTexture2D.TEXTURE2D_ID = 2;
            loHexTexture2D.TEXTURE2D_IMAGE_TILE = loTexture2DTile;
            loTextured2DArrayListObjects.Add(loHexTexture2D);

            return (loTextured2DArrayListObjects);
        }

        public HexMapEngine.Structures.HexTile[,] Load_MapHexTileArray()
        {
            return Load_MapHexTileArray(null);
        }

        public HexMapEngine.Structures.HexTile[,] Load_MapHexTileArray(Texture2D[,] textures)
        {
            int  liTileCount = 1;
            bool lboolEvenColumn = false;

            HexMapEngine.Structures.HexTile loHexTile;

            HexMapEngine.Structures.HexTile[,] loMapHexTileArray = new HexMapEngine.Structures.HexTile[ciActualMapHeightInTiles, ciActualMapWidthInTiles];


            for (int liY = 0; liY < ciActualMapHeightInTiles; liY++)
            {
                for (int liX = 0; liX < ciActualMapWidthInTiles; liX++)
                {
                    // 0 & even columns
                    if (liX % 2 == 0)
                    {
                        lboolEvenColumn = true;
                    }
                    else
                    {
                        lboolEvenColumn = false;
                    }

                    loHexTile = new HexMapEngine.Structures.HexTile();

                    loHexTile.TILE_ID = liY.ToString().Trim() + "," + liX.ToString().Trim();
                    loHexTile.BASE_HEX_TEXTURE_ID = 1;    // to be modified after board is designed
                    loHexTile.TILE_COUNT = liTileCount;
                    loHexTile.ROW_ID = liY;
                    loHexTile.COLUMN_ID = liX;
                    loHexTile.EVEN_COLUMN = lboolEvenColumn;
                    if (textures != null)
                    {
                        loHexTile.texture2D = textures[liY, liX];
                    }    

                    loMapHexTileArray[liY, liX] = loHexTile;

                    liTileCount++;
                }
            }

            return (loMapHexTileArray);
        }
        #endregion

    }

    }
