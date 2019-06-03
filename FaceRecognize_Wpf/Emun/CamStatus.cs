using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognize_Wpf.Emun
{
    public enum CamStatus : int
    {
        Work = 0,
        Stop = 1,
        NotFound = 2,


        #region
        Exception = -1,
        #endregion
    }
}
