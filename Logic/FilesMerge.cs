using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrlDownloadLib.Model;
namespace UrlDownloadLib.Logic
{
    public class FilesMerge
    {
        UrlDownloadSettings settings;
        public FilesMerge(UrlDownloadSettings _settings)
        {
            settings = _settings;
        }

        public bool mergeParts(string[] parts, string saveFile)
        {

            if (File.Exists(saveFile))
            {
                File.Delete(saveFile);
            }

            if (parts.Length == 1)
            {
                File.Move(parts[0], saveFile);
            }
            else
            {

                Comparison<string> compare = new Comparison<string>(compareString);
                Array.Sort(parts, compare);

                FileStream saveFileStream = new FileStream(saveFile, FileMode.Create, FileAccess.Write);
                for (int i = 0; i < parts.Length; i++)
                {
                    byte[] buffer = new byte[settings.downloadBufferSize];
                    FileStream partFileStream = new FileStream(parts[i], FileMode.Open, FileAccess.Read);
                    int bytesRead = 0;
                    do
                    {
                        bytesRead = partFileStream.Read(buffer, 0, buffer.Length);
                        saveFileStream.Write(buffer, 0, bytesRead);

                    } while (bytesRead != 0);

                    partFileStream.Close();
                    partFileStream.Dispose();
                }

                saveFileStream.Close();
                saveFileStream.Dispose();


            }



            return true;
        }

        public int compareString(string a, string b)
        {
            int aN = int.Parse(Path.GetFileNameWithoutExtension(a).Split('_')[0]);
            int bN = int.Parse(Path.GetFileNameWithoutExtension(b).Split('_')[0]);
            return aN.CompareTo(bN);
          
        }

        //private string getFileNameWithoutPart(string file) {
        //    string ext = Path.GetExtension(file);
        //    string filename = Path.GetFileNameWithoutExtension(file);



        //}
    }


}
