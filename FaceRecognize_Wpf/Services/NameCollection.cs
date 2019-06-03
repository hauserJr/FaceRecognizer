using FaceRecognize_Wpf.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognize_Wpf.Services
{
    
    public static class NameCollection
    {
        private static string FilePath = $"{PathArgs.dataDomain}{PathArgs.facePicturePath}";
        private static List<NameList> _UserTable;
        public static List<NameList> UserTable
        {
            get
            {
                if (_UserTable == null)
                {
                    _UserTable = GenName();
                }
                return _UserTable;
            }
        }

        /// <summary>
        /// 取得Name
        /// </summary>
        /// <returns></returns>
        private static List<NameList> GenName()
        {
            var userData = new List<NameList>();
            foreach (var dirItem in Directory.GetDirectories(FilePath))
            {
                var dirName = dirItem.Replace(FilePath,"");
                var userDetail = dirName.Split('.');


                userData.Add(new NameList()
                {
                    Key = userDetail[0],
                    Name = userDetail[1]
                });
            }
            return userData;
        }

        /// <summary>
        /// 更新Name
        /// </summary>
        public static void Update()
        {
            _UserTable = GenName();
        }
    }
}
