using System;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace GlobalConquest.HexMapEngine.Classes
{

    public static class Common
    {

        #region Internal Methods - IIF Functionality

            public static object IIf(bool pboolExpression, object poTruePart, object poFalsePart)
            {

              object loReturnValue = pboolExpression == true ? poTruePart : poFalsePart;


              return loReturnValue;
            }


            public static string IIf(bool pboolExpression, string poTruePart, string poFalsePart)
            {

              string loReturnValue = pboolExpression == true ? poTruePart : poFalsePart;


              return loReturnValue;
            }


            public static bool IIf(bool pboolExpression, bool poTruePart, bool poFalsePart)
            {
 
              bool loReturnValue = pboolExpression == true ? poTruePart : poFalsePart;


              return loReturnValue;
            }


            public static int IIf(bool pboolExpression, int poTruePart, int poFalsePart)
            {

              int loReturnValue = pboolExpression == true ? poTruePart : poFalsePart;


              return loReturnValue;
            }

        #endregion

    }

}
