using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognize_Wpf.Model.Configure
{
    public class ConfigureModel
    {
        private string _faceBaseScore = "2000.0";

        /// <summary>
        /// 臉部基數
        /// </summary>
        public string faceBaseScore
        {
            get
            {
                return string.Format("{0:N2}", double.Parse(_faceBaseScore));
            }
            set
            {
                _faceBaseScore = value;
            }
        }

        private string _faceMLMinScore = "10";
        /// <summary>
        /// 以百分比計算最小值(預設10%)
        /// </summary>
        public string faceMLMinScore
        {
            get
            {
                return string.Format("{0:N2}", double.Parse(_faceMLMinScore));
            }
            set
            {
                _faceMLMinScore = value;
            }
        }

        private string _faceMLMaxScore = "10";
        /// <summary>
        /// 以百分比計算最大值(預設10%)
        /// </summary>
        public string faceMLMaxScore
        {
            get
            {
                return string.Format("{0:N2}", double.Parse(_faceMLMaxScore));
            }
            set
            {
                _faceMLMaxScore = value;
            }
        }

        private string _samplePicutreMax = "-1";
        public string samplePicutreMax
        {
            get
            {
                return _samplePicutreMax;
            }
            set
            {
                _samplePicutreMax = value;
            }
        }
    }
}
