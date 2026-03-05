
using GomBuild_V3.Common;
using GomBuild_V3.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GomBuild_V3
{
    public partial class FormMain : Form
    {
        public List<FileInfo> lstCurrent = new List<FileInfo>();
        public List<DupFile> lstFileTrung = new List<DupFile>();

        private List<string> lstProject = new List<string>();
        private List<BUILDS> lstVer = new List<BUILDS>();

        StringBuilder mess = new StringBuilder();
        public FormMain()
        {
            InitializeComponent();

            List<TypeFile> typeFiles = new List<TypeFile>()
            {
                new TypeFile { ID = (int)_TypeFile.Stored, NAME = "Stored" }, new TypeFile { ID = (int)_TypeFile.StoredNonReport, NAME = "StoredNonReport" },
                new TypeFile { ID = (int)_TypeFile.Report, NAME = "Report" }, new TypeFile { ID = (int)_TypeFile.Form, NAME = "Form" },
                new TypeFile { ID = (int)_TypeFile.Script, NAME = "Script" }, new TypeFile { ID = (int)_TypeFile.Template, NAME = "Template" }, new TypeFile { ID = (int)_TypeFile.Other, NAME = "Other" },
                new TypeFile { ID = (int)_TypeFile.Style, NAME = "Style" },new TypeFile { ID = (int)_TypeFile.Image, NAME = "Image" }
            };
            List<TypeFile> List = typeFiles;

            (dataGridView1.Columns["Type"] as DataGridViewComboBoxColumn).DataSource = List;
            (dataGridView1.Columns["Type"] as DataGridViewComboBoxColumn).DisplayMember = "NAME";
            (dataGridView1.Columns["Type"] as DataGridViewComboBoxColumn).ValueMember = "ID";
        }
        public void AppendLog(string message)
        {
            if (richTextLog.InvokeRequired)
            {
                richTextLog.Invoke(new Action(() =>
                {
                    richTextLog.AppendText(message + Environment.NewLine);
                }));
            }
            else
            {
                richTextLog.AppendText(message + Environment.NewLine);
            }
        }
        private void BrowserFile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CboBuildVersion.Text))
            {
                MessageBox.Show("Chọn build version");
                return;
            }
            var SelectedBuild = (CboBuildVersion.SelectedItem as BUILDS);
            string project = SelectedBuild.PROJECT;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "files (*.gz;*.rdl;*.rdlx;*.sql)|*.gz;*.rdl;*.rdlc;*.rdlx;*.sql|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (String file in openFileDialog.FileNames)
                    {
                        System.IO.FileInfo oFileInfo = new System.IO.FileInfo(file);

                        // kiểm tra đã tồn tại trong list hiện tại chưa
                        if (lstCurrent.Any(x => x.Name == oFileInfo.Name))
                            continue;

                        // Kiểm tra có trùng trong folder build chưa

                        var duplicate = HelperCommon.CheckTrungfile(oFileInfo, oFileInfo.Extension, project, textBox1.Text, CboBuildVersion.Text);
                        string status = "OK";
                        bool ISDUP = false;
                        bool Override = false;
                        if (!string.IsNullOrEmpty(duplicate.NAME))
                        {
                            status = "Duplicate";
                            ISDUP = true;
                            Override = true;
                        }
                        lstCurrent.Add(oFileInfo);
                        this.dataGridView1.Rows.Add(oFileInfo.FullName, oFileInfo.Extension, HelperCommon.GetTypeFile(oFileInfo.Extension), oFileInfo.Name, status, duplicate.BUILDPATH, ISDUP, Override, false);
                    }
                }

            }
        }

        private async void FormMain_Load(object sender, EventArgs e)
        {
            LogHelper.Init(richTextLog);


            JsonString _JsonString = await HelperCommon.CallApi();
            string PathCommit = ConfigurationManager.AppSettings["WokingCopy"];
            lstVer = _JsonString.BUILDS;
            foreach (var item in _JsonString.DEV.ToString().Split(','))
            {
                CboDEV.Items.Add(item);
            }
            //foreach (var item in _JsonString.SITE.ToString().Split(','))
            //{
            //    comboBox3.Items.Add(item);
            //}
            CboBuildVersion.DataSource = _JsonString.BUILDS;
            CboBuildVersion.DisplayMember = "VERSION";
            CboBuildVersion.ValueMember = "STT";
            CboBuildVersion.SelectedIndex = -1;

            string DEV = ConfigurationManager.AppSettings["DEV"];
            textBox1.Text = PathCommit;
            CboDEV.SelectedIndex = CboDEV.Items.IndexOf(DEV);

        }

        private void CommitFile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaJira.Text))
            {
                MessageBox.Show("Nhập mã jira vô");
                return;
            }
            if (string.IsNullOrEmpty(CboBuildVersion.Text))
            {
                MessageBox.Show("Chọn build ver");
                return;
            }
             if (string.IsNullOrEmpty(CboDEV.Text))
            {
                MessageBox.Show("Tên DEV không được để trống");
                return;
            }
            var SelectedBuild = (CboBuildVersion.SelectedItem as BUILDS);
            if (SelectedBuild != null)
            {
                if (!SelectedBuild.ISCURRENT && checkBox1.Checked)
                {
                    MessageBox.Show("Chỉ hotfix trên build hiện tại");
                    return;
                }
            }
            bool check = false;
            foreach (DataGridViewRow item in dataGridView1.Rows)
            {
                if (string.IsNullOrEmpty(item.Cells[2].FormattedValue.ToString()))
                {
                    check = true;
                    break;
                }
            }
            if (check)
            {
                MessageBox.Show("Chọn loại file kìa....");
                return;
            }

            mess.Clear();


            var dev = CboDEV.Text;
            //var project = SelectedBuild.PROJECT;
            string pathBackup = Path.Combine(textBox1.Text, /*project,*/ CboBuildVersion.Text, "4.FOLDER_BACKUP");

            bool exists = System.IO.Directory.Exists(Path.Combine(pathBackup, dev, txtMaJira.Text));
            if (!exists) { System.IO.Directory.CreateDirectory(Path.Combine(pathBackup, dev, txtMaJira.Text)); }

            if (checkBox1.Checked)
                mess.Append("*HOTFIX ");

            mess.Append("- JIRA: " + txtMaJira.Text + "\n");
            mess.Append("- BUILD: " + CboBuildVersion.Text + "\n");

            if (!string.IsNullOrEmpty(textBox8.Text))
                mess.Append("- CODE: " + textBox8.Text + "\n");

            if (!string.IsNullOrEmpty(textBox3.Text))
                mess.Append("- NỘI DUNG: " + textBox3.Text + "\n");

            try
            {
                if (!string.IsNullOrEmpty(textBox4.Text))
                {
                    mess.Append("- THAM SỐ: " + "\n");
                    for (int i = 0; i < textBox4.Lines.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(textBox4.Lines[i]))
                        {
                            mess.Append("    + " + textBox4.Lines[i] + "\n");

                        }
                    }
                }
                mess.Append("- FILE: " + "\n");
                foreach (DataGridViewRow item in dataGridView1.Rows)
                {
                    string pathfile = item.Cells[0].Value.ToString();
                    var Filename = item.Cells[3].Value.ToString();

                    string type = item.Cells[2].FormattedValue.ToString();
                    string Extend = item.Cells[1].FormattedValue.ToString();

                    bool isDuplicate = (bool)(item.Cells[6] as DataGridViewCheckBoxCell).Value;
                    bool isOverride = (bool)(item.Cells[7] as DataGridViewCheckBoxCell).Value && isDuplicate;
                    bool isAddNew = false; //(bool)(item.Cells[8] as DataGridViewCheckBoxCell).Value && isDuplicate;

                    var ssss = (item.Cells[2].Value is null) ? _TypeFile.Other : (_TypeFile)item.Cells[2].Value;

                    mess.Append("    + " + Filename + "\n");

                    //gen new file name
                    if (isDuplicate && isAddNew)
                    {
                        Filename = string.Format("{0}_{1}_{2}{3}", System.IO.Path.GetFileNameWithoutExtension(Filename), CboDEV.Text, DateTime.Now.ToString("ddMMyy"), Extend);
                    }

                    //Move file to detail folder
                    string pathdes = Path.Combine(pathBackup, dev, txtMaJira.Text) + @"\" + Filename;
                    HelperCommon.CopyFileByExtend(pathfile, Filename, ssss,/* project,*/textBox1.Text,CboBuildVersion.Text);

                    //Move file to folder backup
                    System.IO.File.Copy(pathfile, pathdes, true);

                    //Hotfix
                    if (checkBox1.Checked)
                    {
                        string pathHothix = Path.Combine(textBox1.Text, /*project,*/ CboBuildVersion.Text, "6.HOTFIX");
                        if (!System.IO.Directory.Exists(pathHothix)) { System.IO.Directory.CreateDirectory(pathHothix); }
                        System.IO.File.Copy(pathfile, pathHothix + @"\" + Filename, true);

                        //Tạo folder theo jira để quản lý hotfix
                        string pathHf_backup = Path.Combine(pathHothix, "LIST_JIRA");

                        bool exists_v = System.IO.Directory.Exists(Path.Combine(pathHf_backup, txtMaJira.Text));
                        if (!exists) { System.IO.Directory.CreateDirectory(Path.Combine(pathHf_backup, txtMaJira.Text)); }

                        System.IO.File.Copy(pathfile, pathHf_backup + @"\" + txtMaJira.Text + @"\" + Filename, true);
                        if (string.IsNullOrEmpty(txtMaJira.Text))
                        {
                            using (StreamWriter writer = new StreamWriter(Path.Combine(pathHf_backup, txtMaJira.Text, "02 - JIRA.url")))
                            {
                                writer.WriteLine("[{000214A0-0000-0000-C000-000000000046}]");
                                writer.WriteLine("Prop3=19,11");
                                writer.WriteLine("[InternetShortcut]");
                                writer.WriteLine("IDList=");
                                writer.WriteLine("URL=" + string.Format("https://jira.fis.com.vn/browse/{0}", txtMaJira.Text));
                                writer.Flush();
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(mess.ToString()))
                {
                    System.IO.File.WriteAllText(Path.Combine(pathBackup, dev, txtMaJira.Text, "01 - README.txt"), mess.ToString(), Encoding.UTF8);
                }
                if (!checkBox2.Checked)
                {
                    using (StreamWriter writer = new StreamWriter(Path.Combine(pathBackup, dev, txtMaJira.Text, "02 - JIRA.url")))
                    {
                        writer.WriteLine("[{000214A0-0000-0000-C000-000000000046}]");
                        writer.WriteLine("Prop3=19,11");
                        writer.WriteLine("[InternetShortcut]");
                        writer.WriteLine("IDList=");
                        writer.WriteLine("URL=" + string.Format("https://jira.fis.com.vn/browse/{0}", txtMaJira.Text));
                        writer.Flush();
                    }
                }

                // Commit SVN
                HelperCommon.CommitSVN(textBox1.Text, mess.ToString());


                var formRes = MessageBox.Show(mess.ToString(), "Xong rồi đó (OK = copy text)");
                if (formRes == DialogResult.OK)
                {
                    System.Windows.Forms.Clipboard.SetText(mess.ToString());
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }


        private void ClearForm()
        {
            lstCurrent.Clear();
            dataGridView1.Rows.Clear();
            lstFileTrung.Clear();
            textBox3.Text = string.Empty;
            textBox4.Text = string.Empty;
            mess.Clear();
        }

        private void CboBuildVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CboBuildVersion.SelectedIndex < 0 || string.IsNullOrEmpty(textBox1.Text))
            {
                return;
            }
            else
            {
                var SelectedBuild = (CboBuildVersion.SelectedItem as BUILDS);
                string WorkingCopy = Path.Combine(ConfigurationManager.AppSettings["WokingCopy"].ToString(), SelectedBuild.PROJECT);

                label8.Text = SelectedBuild.NOTE;
                label11.Text = SelectedBuild.PROJECT;
                textBox1.Text = WorkingCopy;

                HelperCommon.CheckIsWorkingCopy(WorkingCopy);
                //HelperCommon.HasCommitPermission(WorkingCopy,,);
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                txtMaJira.Enabled = false;
                txtMaJira.Text = "NO_JIRA";
            }
            else
            {
                txtMaJira.Enabled = true;
                txtMaJira.Text = string.Empty;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ClearForm();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "files (*.gz)|*.gz";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string directoryName = Path.GetDirectoryName(openFileDialog.FileName);
                    string withoutExtension = Path.GetFileNameWithoutExtension(openFileDialog.FileName);

                    textBox5.Text = openFileDialog.FileName;
                    textBox6.Text = Path.Combine(directoryName, withoutExtension);
                }

            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox5.Text))
                return;
            textBox7.Text = string.Empty;
            StringBuilder Command_Click = new StringBuilder();
            StringBuilder Command_DoubleClick = new StringBuilder();
            StringBuilder Command_TextChanged = new StringBuilder();
            StringBuilder Command_SelChanged = new StringBuilder();
            StringBuilder Command_Final = new StringBuilder();

            StringBuilder ModelScript = new StringBuilder();

            // Tạo thư mục nếu chưa có
            string outputPath = textBox6.Text;
            Directory.CreateDirectory(outputPath);

            FileInfo fileToDecompress = new FileInfo(textBox5.Text);
            try
            {
                string directoryName = Path.GetDirectoryName(textBox5.Text);
                string withoutExtension = Path.GetFileNameWithoutExtension(textBox5.Text);

                List<Dictionary<string, object>> data = HelperCommon.Decompress(fileToDecompress);

                var nameCount = new Dictionary<string, int>();
                var usedNames = new HashSet<string>();

                int StartTabIndex = 0;
                foreach (var dict in data)
                {
                    if (!dict.ContainsKey("Name") || dict["Name"] == null)
                        continue;

                    if (checkBox4.Checked)
                    {
                        HelperCommon.GenCodeModel(dict, ModelScript);
                    }

                    if (checkBox5.Checked)
                    {
                        HelperCommon.GenCodeEvent(dict, Command_Click, "Command_Click");
                        HelperCommon.GenCodeEvent(dict, Command_DoubleClick, "Command_DoubleClick");
                        HelperCommon.GenCodeEvent(dict, Command_TextChanged, "Command_TextChanged");
                        HelperCommon.GenCodeEvent(dict, Command_SelChanged, "Command_SelChanged");
                    }

                    //if (checkBox6.Checked)
                    //{
                    //    string[] TemplateIgnore = new string[] { "ControlContainer", "Panel", "GridView", "Label", "RadioList", "TabButton", "Flowbuttons", "GroupBox" };
                    //    string originalTemplate = dict["Template"].ToString();
                    //    if (!TemplateIgnore.Contains(originalTemplate))
                    //    {
                    //        dict["TabIndex"] = StartTabIndex;
                    //        StartTabIndex++;
                    //    }
                    //    else
                    //    {
                    //        dict["TabIndex"] = 0;
                    //    }
                    //}

                    string originalName = dict["Name"].ToString();
                    string newName = originalName;

                    if (!usedNames.Contains(newName))
                    {
                        usedNames.Add(newName);
                        nameCount[originalName] = 1;
                    }
                    else
                    {
                        int suffix = nameCount[originalName];
                        do
                        {
                            newName = $"{originalName}_{suffix}";
                            suffix++;
                        } while (usedNames.Contains(newName));

                        dict["Name"] = newName;
                        usedNames.Add(newName);
                        nameCount[originalName] = suffix;
                    }
                }


                HelperCommon.WriteJsonToGzipFile(data, Path.Combine(outputPath, withoutExtension + "_NEW.gz"));

                Command_Final.AppendLine("#region Command_Click");
                Command_Final.Append(Command_Click);
                Command_Final.AppendLine("#endregion");

                Command_Final.AppendLine("#region Command_DoubleClick");
                Command_Final.Append(Command_DoubleClick);
                Command_Final.AppendLine("#endregion");

                Command_Final.AppendLine("#region Command_TextChanged");
                Command_Final.Append(Command_TextChanged);
                Command_Final.AppendLine("#endregion");

                Command_Final.AppendLine("#region Command_SelChanged");
                Command_Final.Append(Command_SelChanged);
                Command_Final.AppendLine("#endregion");

                File.WriteAllText(Path.Combine(outputPath, "Event.cs"), Command_Final.ToString());
                File.WriteAllText(Path.Combine(outputPath, "Model.cs"), ModelScript.ToString());



                textBox7.Text = "Xong rồi đó";

                if (checkBox3.Checked)
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = outputPath,
                        UseShellExecute = true
                    };

                    Process.Start(psi);
                }

            }
            catch (Exception ex)
            {
                textBox7.Text = ex.Message;
                throw;
            }

        }
    }
}
