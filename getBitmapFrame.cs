using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 
using System.Windows.Media.Imaging;

namespace netHTA
{
    class getBitmapFrame
    {
        public BitmapFrame fromString(string path)
        {
            if (System.IO.File.Exists(path))
            {
                // 元から絶対Pathだったとしても無視して絶対Pathへの変換を実行する。
                path = System.IO.Path.GetFullPath(path);
            }

            Uri bmpURI = new Uri(path, UriKind.RelativeOrAbsolute);
            return BitmapFrame.Create(bmpURI);
        }
    }
}
