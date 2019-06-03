using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognize_Wpf
{
    public static class PathArgs
    {
        public static string dataDomain
        {
            get
            {
                return @"./Configure";
            }
        }
        public static string facePicturePath
        {
            get
            {
                return @"/camera_data/";
            }
        }
        public static string faceDataPath
        {
            get
            {
                return @"/face_data/";
            }
        }

        public static string faceDataBackupPath
        {
            get
            {
                return @"/face_data/backup/";
            }
        }
        public static string faceDateName
        {
            get
            {
                return @"faces.dat";
            }
        }

        public static string configureFile
        {
            get
            {
                return @"application.json";
            }
        }

        public static string haarcascade_Face
        {
            get
            {
                return "./haarcascade_frontalface_default.xml";
            }
        }
        public static string haarcascade_Eye
        {
            get
            {
                return "./haarcascade_eye.xml";
            }
        }
    }
}
