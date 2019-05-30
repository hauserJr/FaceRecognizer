using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognize_Wpf.Model
{
    public class FaceCollention
    {

        public List<Rectangle> Faces { get; set; }
        public List<Rectangle> Eyes { get; set; }
    }
}
