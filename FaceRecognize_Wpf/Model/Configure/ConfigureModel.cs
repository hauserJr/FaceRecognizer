using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognize_Wpf.Model.Configure
{
    public class ConfigureModel
    {
        private double _faceBaseScore = 2000.0;
        public double faceBaseScore
        {
            get
            {
                return _faceBaseScore;
            }
            set
            {
                _faceBaseScore = value;
            }
        }
    }
}
