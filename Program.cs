using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using System.Web;
using System.Xml;
using Ionic.Zip;
using System.Threading;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO.Compression;




namespace Item_ImageAll_Backup_Console
{
    class Program
    {

        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string lpFileName);
        #region

        //protected static List<string> GetKeys(string iniFile, string category)
        //{

        //    byte[] buffer = new byte[2048];

        //    GetPrivateProfileSection(category, buffer, 2048, iniFile);
        //    String[] tmp = Encoding.ASCII.GetString(buffer).Trim('\0').Split('\0');

        //    List<string> result = new List<string>();

        //    foreach (String entry in tmp)
        //    {
        //        var value = entry;
        //        result.Add(entry);

        //    }

        //    return result;
        //}


        //static string fle = "";
        //static string k = "";
        //static string[] Dbin = new string[3];
        //static string Dbname = "";
        //static string LocalPath = "";
        //static string foo = "";
        #endregion

        static string localPath = ConfigurationManager.AppSettings["localFilePath"].ToString();
        static string DBoxPath = ConfigurationManager.AppSettings["DropboxPath"].ToString();
        static string key = ConfigurationManager.AppSettings["TokenKey"].ToString();
        static String dtnow = null;
        static string dt = null;
        static MemoryStream ms;
        static void Main(string[] args)
        {
            #region for Dbpath.ini
            //var keyvaluepair = GetKeys(@"C:\backup\Dbpath.ini", "AutoDataBackup-BigSize");

            //string DBName = "";
            //bool IsEnabled = false;
            //int MaxCountInDb = 0;
            //string DBPath = "";
            //string[] DBinstances = new string[3];

            //foreach (string str in keyvaluepair)
            //{
            //    DBName = str.Split('=')[0].ToString();
            //    IsEnabled = (str.Split('=').Last().Split(',').FirstOrDefault().Trim() == "1") ? true : false;
            //    MaxCountInDb = Convert.ToInt32(str.Split('=').Last().Split(',')[1].ToString());
            //   // localPath = str.Split('=').Last().Split(',')[2].ToString().Replace("\"", "").Replace("[", "");
            //    DBPath = str.Split(',')[3].ToString().Replace("\"", "").Replace("]", "");
            //    DBinstances = str.Split('[').Last().Replace("]", "").Split(',');

            //    if (IsEnabled == true)
            //    {

            //        DBoxPath = DBPath;
            //        LocalPath = localPath;
            //        Dbin = DBinstances;
            //        Dbname = DBName;
            //    }
            //}
            // dtnow = DateTime.Now.ToString("ddMMyyyy hh mm").Replace(" ", "").Replace(":", "");
            // dt = DateTime.Now.AddMinutes(-1).ToString("ddMMyyyy hh mm").Replace(" ", "").Replace(":", "");
            #endregion

            ImageZipForSKS(localPath);   // for SKS
            // ImageZip(localPath);      // For Inno
        }


        public static void ImageZipForSKS(string localpath)
        {
            DataTable dtFolder = new DataTable();
            dtFolder.Columns.Add("Image_Name");
            DataTable dtImageCount = SelectImage();
            using (var zip = new Ionic.Zip.ZipFile())
            {
                if (dtImageCount.Rows.Count > 0)
                {
                    zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
                    for (int i = 0; i < dtImageCount.Rows.Count; i++)
                    {
                        string filename = dtImageCount.Rows[i]["Image_Name"].ToString();
                        string s = localPath + filename;
                        if (File.Exists(s))
                        {
                            zip.AddFile(localpath + filename, "Item_Image");

                        }
                        else
                        {
                            DataRow row;
                            row = dtFolder.NewRow();
                            row["Image_Name"] = dtImageCount.Rows[i]["Image_Name"].ToString();
                            dtFolder.Rows.Add(row);
                        }
                    }
                    // dtFolder.AcceptChanges();

                    String date = DateTime.Now.ToString("ddMMyyyy_HHmmss");
                    zip.Save(localpath + "images" + "$" + "_" + date + ".zip");
                }

            }
        }

        public static SqlConnection GetConnection()
        {
            String connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            SqlConnection connection = new SqlConnection(connectionString);
            return connection;
        }
        public static DataTable SelectImage()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            SqlConnection connectionString = GetConnection();
            SqlCommand cmd = new SqlCommand();
            try
            {
                //and  Updated_Date >= '" + today + "'
                cmd.CommandText = "select Image_Name from Item_Image where Image_Name <>''";
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;
                cmd.Connection = connectionString;
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                DataTable dt = new DataTable();
                SqlDataAdapter sd = new SqlDataAdapter(cmd);
                sd.Fill(dt);
                cmd.Connection.Close();
                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public static void WriteLog(string strLog, string filename)
        {
            string logFilePath = "C:\\LogFile\\" + "Item_ImageLog" + System.DateTime.Today.ToString("MM-dd-yyyy") + "." + "txt";
            FileStream fileStream = null;
            FileInfo logFileInfo = new FileInfo(logFilePath);
            DirectoryInfo logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            if (!logFileInfo.Exists)
            {
                fileStream = logFileInfo.Create();
            }
            else
            {
                fileStream = new FileStream(logFilePath, FileMode.Append);
            }
            StreamWriter log = new StreamWriter(fileStream);
            log.WriteLine(strLog + filename + "   " + DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss"));
            log.Close();
        }



        public static void ImageZip(string localPath)
        {

            if (Directory.Exists(localPath))
            {
                string[] Filenames = Directory.GetFiles(localPath);
                using (Ionic.Zip.ZipFile zipfile = new Ionic.Zip.ZipFile())
                {
                    String date = DateTime.Now.ToString("ddMMyyyy_HHmmss");
                    zipfile.UseZip64WhenSaving = Zip64Option.AsNecessary;
                    //zipfile.AlternateEncodingUsage = ZipOption.AsNecessary;   use loh ya tl 
                    //zipfile.UseZip64WhenSaving = Zip64Option.Always;
                    zipfile.AddFiles(Filenames, "Item_Image");
                    zipfile.Save(localPath + "images" + "$" + "_" + date + ".zip");

                }
            }
        }
    }
}





