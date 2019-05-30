using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Controls;
namespace FaceRecognize_Wpf.Model
{
    public class CameraArgs
    {
        public System.Windows.Controls.Image CamView { get; set; }
        public ImageSource ImageSourceBitmap { get; set; }
    }
}
