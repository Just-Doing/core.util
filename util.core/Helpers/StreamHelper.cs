using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Util.Core.Helpers
{
    class StreamHelper
    {
        //stream 转为byte[] 
        public byte[] Stream2byte(Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            return buffer;
        }
        //byte[] 转stream
        public Stream  Byte2stream(byte[] buffer)
        {
            Stream stream = new MemoryStream(buffer);
            stream.Seek(0, SeekOrigin.Begin);
            //设置stream的position为流的开始
            return stream;
        }

        //stream写到文件
        public void Stream2File(Stream stream, string filename)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            FileStream fs = new FileStream(filename, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fs);
            writer.Write(buffer);
            writer.Close();
            fs.Close();
        }
        //读取文件到stream
        public Stream File2Stream(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                Stream stream = new MemoryStream(buffer);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
        }
    }
}
