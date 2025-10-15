using System;
using System.IO;
using System.Text;
using System.Collections;


namespace GlobalConquest.HexMapEngine.Classes
{

    public class TextFileIO
    {

        #region Constructor

            public TextFileIO()
            {

            }

        #endregion


        #region Read Methods

            public string Read_TextIntoString(string psFilePath)
            {

                // reads a text file into a concatenated string

                string        lsTextLine = "";
                string        lsTextString = "";

                StreamReader  loStreamReader = null;


                if (!File.Exists(psFilePath))
                {
                    throw new Exception("file does not exist");
                }


                loStreamReader = new StreamReader(psFilePath);

                try
                {
                    while ((lsTextLine = loStreamReader.ReadLine()) != null)
                    {
                        lsTextString = lsTextString + lsTextLine;
                    }
                }
                catch (Exception loException)
                {
                    throw loException;
                }


                loStreamReader.Close();
                loStreamReader = null;

                return (lsTextString);
            }


            public ArrayList Read_TextIntoArrayList(string psFilePath)
            {

                // reads a text file into an array-list

                string        lsTextLine = "";

                ArrayList     loArrayList = new ArrayList();
                StreamReader  loStreamReader = null;


                if (!File.Exists(psFilePath))
                {
                    throw new Exception("file does not exist");
                }


                loStreamReader = new StreamReader(psFilePath);

                try
                {
                    while ((lsTextLine = loStreamReader.ReadLine()) != null)
                    {
                        loArrayList.Add(lsTextLine);
                    }
                }
                catch (Exception loException)
                {
                    throw loException;
                }


                loStreamReader.Close();
                loStreamReader = null;

                return (loArrayList);
            }

        #endregion


        #region Internal Methods - Generic Write Methods

            public void Write_StringToTextFile(string psFilePath, string psTextData)
            {

                // writes a string to an existing text-file

                StreamWriter loStreamWriter;


                if (File.Exists(psFilePath))
                {
                    File.Delete(psFilePath);
                }


                loStreamWriter = new StreamWriter(psFilePath);
                loStreamWriter.WriteLine(psTextData);
                loStreamWriter.Close();
            }


            public void Write_ArrayListToTextFile(string psFilePath, ArrayList poArrayList)
            {

                // writes an array-list to an existing text-file

                StreamWriter loStreamWriter;


                if (File.Exists(psFilePath))
                {
                    File.Delete(psFilePath);
                }


                loStreamWriter = new StreamWriter(psFilePath);

                for (int liCnt = 0; liCnt < poArrayList.Count; liCnt++)
                {
                    loStreamWriter.WriteLine(poArrayList[liCnt].ToString());
                }


                loStreamWriter.Close();
            }

        #endregion


        #region Internal Methods - Specific Write Methods

            public void Write_HexTileXYOffsetsArrayListToTextFile(string psFilePath, ArrayList poArrayList)
            {

                // writes an array-list to an existing text-file

                StringBuilder  loStringBuilder = new StringBuilder();
                StreamWriter   loStreamWriter;


                if (File.Exists(psFilePath))
                {
                    File.Delete(psFilePath);
                }


                loStreamWriter = new StreamWriter(psFilePath);

                foreach (HexMapEngine.Structures.HexTileXYOffsets loHexTileXYOffsets in poArrayList)
                {
                    //build position information string and add to array - list
                    loStringBuilder.Length = 0;

                    loStringBuilder.Append("[" + loHexTileXYOffsets.TILE_ID + "],");
                    loStringBuilder.Append("X Offset: " + loHexTileXYOffsets.HEX_TILE_X_OFFSET.ToString().Trim() + ",");
                    loStringBuilder.Append("Y Offset: " + loHexTileXYOffsets.HEX_TILE_Y_OFFSET.ToString().Trim());

                    loStreamWriter.WriteLine(loStringBuilder.ToString().Trim());

                }

                loStreamWriter.Close();
            }

        #endregion

    }

}
