using GomBuild_V3.Model;
using Newtonsoft.Json;
using SharpSvn;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GomBuild_V3.Common
{
    public static class HelperCommon
    {
        public static void CopyFileByExtend(string pathfile, string filename, _TypeFile type, string WorkingCopy, string BuildVersion, bool o = true)
        {
            string FormFolder = Path.Combine(WorkingCopy, BuildVersion, "1.FORM");
            string ReportFolder = Path.Combine(WorkingCopy, BuildVersion, "2.REPORT");
            string TempFolder = Path.Combine(WorkingCopy, BuildVersion, "5.TEMPLATE");
            string ScriptFolder = Path.Combine(WorkingCopy, BuildVersion, "3.SQL", "SCRIPT");
            string StoredFolder = Path.Combine(WorkingCopy, BuildVersion, "3.SQL", "STORED");
            string StoredNonreportFolder = Path.Combine(WorkingCopy, BuildVersion, "3.SQL", "STOREDNonREPORT");
            string OtherFolder = Path.Combine(WorkingCopy, BuildVersion, "3.SQL");

            switch (type)
            {
                case _TypeFile.Report:
                    if (!System.IO.Directory.Exists(ReportFolder)) { System.IO.Directory.CreateDirectory(ReportFolder); }
                    System.IO.File.Copy(pathfile, ReportFolder + @"\" + filename, o);
                    break;
                case _TypeFile.Form:
                    if (!System.IO.Directory.Exists(FormFolder)) { System.IO.Directory.CreateDirectory(FormFolder); }
                    System.IO.File.Copy(pathfile, FormFolder + @"\" + filename, o);
                    break;
                case _TypeFile.Template:
                    if (!System.IO.Directory.Exists(TempFolder)) { System.IO.Directory.CreateDirectory(TempFolder); }
                    System.IO.File.Copy(pathfile, TempFolder + @"\" + filename, o);
                    break;
                case _TypeFile.Script:
                    if (!System.IO.Directory.Exists(ScriptFolder)) { System.IO.Directory.CreateDirectory(ScriptFolder); }
                    System.IO.File.Copy(pathfile, ScriptFolder + @"\" + filename, o);
                    break;
                case _TypeFile.Stored:
                    if (!System.IO.Directory.Exists(StoredFolder)) { System.IO.Directory.CreateDirectory(StoredFolder); }
                    System.IO.File.Copy(pathfile, StoredFolder + @"\" + filename, o);
                    break;
                case _TypeFile.StoredNonReport:
                    if (!System.IO.Directory.Exists(StoredNonreportFolder)) { System.IO.Directory.CreateDirectory(StoredNonreportFolder); }
                    System.IO.File.Copy(pathfile, StoredNonreportFolder + @"\" + filename, o);
                    break;
                case _TypeFile.Other:
                    if (!System.IO.Directory.Exists(OtherFolder)) { System.IO.Directory.CreateDirectory(OtherFolder); }
                    System.IO.File.Copy(pathfile, OtherFolder + @"\" + filename, o);
                    break;
                default:
                    if (!System.IO.Directory.Exists(OtherFolder)) { System.IO.Directory.CreateDirectory(OtherFolder); }
                    System.IO.File.Copy(pathfile, OtherFolder + @"\" + filename, o);
                    break;
            }
        }
        public static DupFile CheckTrungfile(FileInfo oFileInfo, string extend, string project, string WorkingCopy, string BuildVersion)
        {
            DupFile dupf = new DupFile();
            string FormFolder = Path.Combine(WorkingCopy, BuildVersion, "1.FORM");
            string ReportFolder = Path.Combine(WorkingCopy, BuildVersion, "2.REPORT");
            string StoredFolder = Path.Combine(WorkingCopy, BuildVersion, "3.SQL", "STORED");
            string StoredNonreportFolder = Path.Combine(WorkingCopy, BuildVersion, "3.SQL", "STOREDNonREPORT");
            string OtherFolder = Path.Combine(WorkingCopy, BuildVersion, "3.SQL");



            string[] usePaths = new string[3];
            switch (extend)
            {
                case ".gz":
                    usePaths = new string[] { FormFolder };
                    break;
                case ".rdl":
                case ".rdlx":
                    usePaths = new string[] { ReportFolder };
                    break;
                case ".sql":
                    usePaths = new string[] { StoredFolder, StoredNonreportFolder, OtherFolder };
                    break;
            }
            foreach (string usePath in usePaths)
            {
                if (!System.IO.Directory.Exists(usePath))
                    return dupf;
                var dir = new DirectoryInfo(usePath);
                FileInfo[] files = dir.GetFiles();
                if (files.Any(x => x.Name.Equals(oFileInfo.Name)))
                {
                    dupf.NAME = oFileInfo.Name;
                    dupf.EXTEND = extend;
                    dupf.SOURCEPATH = oFileInfo.FullName;
                    dupf.BUILDPATH = Path.Combine(usePath, oFileInfo.Name);
                    break;
                }
            }

            return dupf;
        }
        public static async Task<JsonString> CallApi()
        {
            string apiUrl = ConfigurationManager.AppSettings["API_SQLJSON"].ToString();
            string Master = ConfigurationManager.AppSettings["API_MASTER"].ToString();
            string Access = ConfigurationManager.AppSettings["API_ACCSESS"].ToString();
            string json = await FetchDataFromAPIWithHeaders(apiUrl, Master, Access);
            var _JsonString = JsonConvert.DeserializeObject<JsonString>(json);

            return _JsonString;
        }
        static async Task<string> FetchDataFromAPIWithHeaders(string apiUrl, string Master, string Access)
        {
            string responseData = string.Empty;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("X-Master-Key", Master);
                    client.DefaultRequestHeaders.Add("X-Access-Key", Access);
                    client.DefaultRequestHeaders.Add("X-BIN-META", "false");
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        responseData = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return responseData;
        }
        public static int? GetTypeFile(string extend)
        {
            int? res = null;
            switch (extend)
            {
                case ".rdl":
                case ".rdlx":
                    res = (int)_TypeFile.Report;
                    break;
                case ".gz":
                    res = (int)_TypeFile.Form;
                    break;
                case ".xls":
                case ".xlsx":
                    res = (int)_TypeFile.Template;
                    break;
                case ".json":
                    res = (int)_TypeFile.Style;
                    break;
                case ".png":
                case ".jpg":
                    res = (int)_TypeFile.Image;
                    break;
            }
            return res;
        }
        public static List<Dictionary<string, object>> Decompress(FileInfo fileToDecompress)
        {
            using (FileStream fileStream = fileToDecompress.OpenRead())
            {
                string fullName = fileToDecompress.FullName;
                using (GZipStream gzipStream = new GZipStream((Stream)fileStream, CompressionMode.Decompress))
                {
                    using (StreamReader streamReader = new StreamReader((Stream)gzipStream))
                        return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(streamReader.ReadToEnd());
                }
            }
        }
        public static void WriteJsonToGzipFile(List<Dictionary<string, object>> data, string outputPath)
        {
            string str = System.Text.Json.JsonSerializer.Serialize<List<Dictionary<string, object>>>(data, new JsonSerializerOptions()
            {
                WriteIndented = true
            });
            using (FileStream fileStream = new FileStream(outputPath, FileMode.Create))
            {
                using (GZipStream gzipStream = new GZipStream((Stream)fileStream, CompressionMode.Compress))
                {
                    using (StreamWriter streamWriter = new StreamWriter((Stream)gzipStream, Encoding.UTF8))
                        streamWriter.Write(str);
                }
            }
        }
        public static void GenCodeEvent(Dictionary<string, object> dict, StringBuilder Events, string command)
        {
            if (dict.ContainsKey(command))
            {
                if (dict[command] != null)
                {
                    if (!string.IsNullOrEmpty(dict[command].ToString()))
                    {
                        if (command == "Command_Click")
                        {
                            Events.AppendLine("//[Permission(Name = \"\", Description = \"\")]");
                        }
                        Events.AppendLine(string.Concat("private void ", dict[command].ToString(), " (ICmdParameter p) \n{\n\r}\n"));
                    }
                }
            }
        }
        public static void GenCodeModel(Dictionary<string, object> dict, StringBuilder ModelScript)
        {
            if (dict.ContainsKey("DataBindingName"))
            {
                if (dict["DataBindingName"] != null)
                {
                    if (!string.IsNullOrEmpty(dict["DataBindingName"].ToString()))
                    {
                        string data_type = GetDataType(dict["Template"].ToString());

                        ModelScript.AppendLine(string.Concat("public ", data_type, " ", dict["DataBindingName"].ToString(), " { get; set; } \n"));
                    }
                }
            }
        }
        private static string GetDataType(string template)
        {
            string result = string.Empty;
            switch (template)
            {
                case "CheckBox":
                    result = "bool";
                    break;
                case "DatePicker":
                    result = "DateTime";
                    break;
                case "SearchEntry":
                case "ComboBox":
                case "RadioList":
                    result = "int";
                    break;
                default:
                    result = "string";
                    break;
            }
            return result;
        }
        public static bool CheckIsWorkingCopy(string WorkingCopy)
        {
            LogHelper.Clear();
            try
            {
                using (SvnClient client = new SvnClient())
                {
                    if (client.GetInfo(WorkingCopy, out SvnInfoEventArgs info))
                    {
                        LogHelper.Log("Working copy");
                        LogHelper.Log("URL: " + info.Uri);
                        LogHelper.Log("Revision: " + info.Revision);
                        return true;
                    }
                    else
                    {
                        LogHelper.Log("NOT working copy. Check out SVN trước...");
                        return false;

                    }
                }
            }
            catch
            {
                LogHelper.Log("NOT working copy. Check out SVN trước...");
                return false;
            }

            
        }
        public static bool HasCommitPermission(string workingCopyPath, string username, string password)
        {
            using (var client = new SvnClient())
            {
                client.Authentication.Clear();
                client.Authentication.DefaultCredentials =
                    new System.Net.NetworkCredential(username, password);

                try
                {
                    var args = new SvnStatusArgs
                    {
                        RetrieveRemoteStatus = true,
                        Depth = SvnDepth.Empty
                    };

                    client.GetStatus(workingCopyPath, args, out _);

                    return true; // có quyền
                }
                catch (SvnException ex)
                {
                    if (ex.SvnErrorCode == SvnErrorCode.SVN_ERR_RA_NOT_AUTHORIZED ||
                        ex.SvnErrorCode == SvnErrorCode.SVN_ERR_AUTHN_FAILED)
                    {
                        LogHelper.Log("Không có quyền commit");
                        return false; // không có quyền commit
                    }
                    LogHelper.Log(ex.Message);
                    return false;
                }
            }
        }

        public static void CommitSVN(string WorkingCopy, string messages)
        {
            using (SvnClient client = new SvnClient())
            {
                try
                {
                    // 1. UPDATE FULL WORKING COPY
                    LogHelper.Log("SVN UPDATE...");
                    client.Update(WorkingCopy, new SvnUpdateArgs
                    {
                        Depth = SvnDepth.Infinity,
                        AllowObstructions = true
                    }, out SvnUpdateResult updateResult);
                    LogHelper.Log("Update to revision: " + updateResult.Revision);


                    // 2. Get status toàn bộ working copy
                    Collection<SvnStatusEventArgs> statuses;
                    client.GetStatus(WorkingCopy, new SvnStatusArgs
                    {
                        Depth = SvnDepth.Infinity
                    }, out statuses);

                    // 3. Lấy các node chưa version (bỏ .svn)
                    var notVersioned = statuses
                        .Where(s => s.LocalNodeStatus == SvnStatus.NotVersioned)
                        .Where(s => !s.FullPath.Contains(@"\.svn\"))
                        .OrderBy(s => s.FullPath.Count(c => c == '\\'))
                        .ToList();

                    foreach (var item in notVersioned)
                    {
                        LogHelper.Log("Adding: " + item.FullPath);
                        client.Add(item.FullPath);
                    }

                    // 4. COMMIT WORKING COPY
                    SvnCommitArgs commitArgs = new SvnCommitArgs
                    {
                        LogMessage = messages
                    };

                    if (client.Commit(WorkingCopy, commitArgs, out SvnCommitResult commitResult))
                    {
                        LogHelper.Log("Commit OK!");
                        LogHelper.Log("Revision: " + commitResult.Revision);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log("SVN ERROR: " + ex.Message);
                }
            }

        }
    }
}
