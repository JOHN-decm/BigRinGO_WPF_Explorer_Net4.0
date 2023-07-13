using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using JHR_GetIcon;
using Microsoft.VisualBasic.FileIO;
using Shell32;

namespace BigRinGO_WPF_Explorer
{
    public partial class RinGO_Form1 : Form
    {
        ArrayList accesspaths = new ArrayList(); //访问过的路径数组
        GetIcon geticon = new GetIcon();
        bool flag_pre_next = true;
        ArrayList copyobjs = new ArrayList(); //复制或剪切的对象数组
        bool iscut = false; //剪切标记，true表示剪切，false表示复制
        private bool isFirstAccess = true;//是否第一次访问的标志

        private ColumnHeader sortedColumn; // 保存当前排序的列，为排序做准备
        private SortOrder sortOrder = SortOrder.Ascending; // 默认升序排序，为 排序做准备


        public RinGO_Form1()
        {
            InitializeComponent();
        }

        private void RinGO_Form1_Load(object sender, EventArgs e)
        {

            Icon[] myIcon;
            sortedColumn = listView1.Columns[0];// 初始化sortedColumn为第一列,为 排序做准备
            int[] myindexs = { 15, 34, 43, 8, 11, 7, 101, 4, 2, 0, 16, 17 };
            string[] mykeys = { "computer", "desktop", "favorites", "localdriver", "cdrom", "movabledriver", "recycle", "defaultfolder", "defaultexeicon", "unknowicon", "printer", "network" };
            for (int i = 0; i < myindexs.Length; i++)
            {
                myIcon = geticon.GetIconByIndex(myindexs[i]);
                imageList1.Images.Add(mykeys[i], myIcon[0]);
                imageList2.Images.Add(mykeys[i], myIcon[1]);
            }
            treeView1.ImageList = imageList1;
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            //搜索框显示文字及设置
            //设置默认显示文字
            text_search.Text = "你所热爱的就是你的生活";
            text_search.ForeColor = SystemColors.GrayText;

            //桌面节点
            string mypath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            TreeNode desk = new TreeNode("桌面");
            desk.ImageKey = desk.SelectedImageKey = "desktop";
            desk.Tag = mypath;
            treeView1.Nodes.Add(desk);

            //此电脑节点
            mypath = "mycomputer";
            TreeNode root = new TreeNode("此电脑");
            root.SelectedImageKey = root.ImageKey = "computer";
            root.Tag = mypath;
            treeView1.Nodes.Add(root);
            RinGO_GetFogerTree(root);//在此电脑节点下增加驱动器节点
            root.Expand();//展开此电脑节点

            //主文件夹
            mypath = "favorites";
            TreeNode tnf = new TreeNode("主文件夹");
            tnf.SelectedImageKey = tnf.ImageKey = "favorites";
            tnf.Tag = mypath;
            treeView1.Nodes.Add(tnf);

            //在主文件夹节点下添加：文档，图片，音乐，视频
            mypath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            TreeNode tn1 = new TreeNode("文档");
            myIcon = geticon.GetIconByFileName(mypath, FileAttributes.Directory);
            imageList1.Images.Add("mydocument", myIcon[0]);//小图标
            imageList2.Images.Add("mydocument", myIcon[1]);//大图标
            tn1.SelectedImageKey = tn1.ImageKey = "mydocument";
            tn1.Tag = mypath;
            tnf.Nodes.Add(tn1);

            mypath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            tn1 = new TreeNode("音乐");
            myIcon = geticon.GetIconByFileName(mypath, FileAttributes.Directory);
            if (myIcon != null)
            {
                imageList1.Images.Add("mymusic", myIcon[0]);//小图标
                imageList2.Images.Add("mymusic", myIcon[1]);//大图标
                tn1.SelectedImageKey = tn1.ImageKey = "mymusic";
                tn1.Tag = mypath;
                tnf.Nodes.Add(tn1);
            }

            mypath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            tn1 = new TreeNode("图片");
            myIcon = geticon.GetIconByFileName(mypath, FileAttributes.Directory);
            if (myIcon != null)
            {
                imageList1.Images.Add("mypictures", myIcon[0]);//小图标
                imageList2.Images.Add("mypictures", myIcon[1]);//大图标
                tn1.SelectedImageKey = tn1.ImageKey = "mypictures";
                tn1.Tag = mypath;
                tnf.Nodes.Add(tn1);
            }

            mypath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            tn1 = new TreeNode("视频");
            myIcon = geticon.GetIconByFileName(mypath, FileAttributes.Directory);
            if (myIcon != null)
            {
                imageList1.Images.Add("myvideos", myIcon[0]);//小图标
                imageList2.Images.Add("myvideos", myIcon[1]);//大图标
                tn1.SelectedImageKey = tn1.ImageKey = "myvideos";
                tn1.Tag = mypath;
                tnf.Nodes.Add(tn1);
            }

            //回收站
            mypath = "recycle";
            TreeNode tnr = new TreeNode("回收站");
            tnr.SelectedImageKey = tnr.ImageKey = "recycle";
            tnr.Tag = mypath;
            treeView1.Nodes.Add(tnr);
            treeView1.EndUpdate();
            //初始化listviewl，显示驱动器信息
            RinGO_GetDriveListview();
            //处理地址栏和访问历史数组
            accesspaths.Add("此电脑");
            combo_url.DataSource = accesspaths;
            combo_url.SelectedIndex = 0;

            //安装相关事件
            this.treeView1.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.RinGO_treeView1_BeforeExpand);
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.RinGO_treeView1_AfterSelect);
            combo_url.SelectedIndexChanged += new EventHandler(RinGO_combo_url_SelectedIndexChanged);
        }
        private void RinGO_combo_url_TextChanged(object sender, EventArgs e)
        {
            if (isFirstAccess && combo_url.Text == ("C:\\"))//这里要修改一下，判定逻辑，提示显示一次就好
            {
                isFirstAccess = false;

                DialogResult result = MessageBox.Show("欢迎来到C盘，这里储存着系统运行所需的文件。\n出于对系统平稳运行的安全考虑，我们隐藏了本驱动器的内容。\n请您决定是否仍要继续访问C盘？",
                    "警告!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    combo_url.Text = "C:\\";
                }
                if (result == DialogResult.No)
                {
                    combo_url.SelectedIndex = 1;
                }
            }
        }

        private void RinGO_treeView1_AfterSelect(object sender, TreeViewEventArgs e)//展开被选中的树形目录
        {
            TreeNode tn = e.Node;
            int flag = 0;
            switch (tn.Text)
            {
                case "桌面":
                    {
                        if (accesspaths.IndexOf("桌面") > -1) accesspaths.Remove("桌面");
                        accesspaths.Insert(0, "桌面");
                        RinGO_GetDesktopListview();
                        break;
                    }
                case "此电脑":
                    {
                        if (accesspaths.IndexOf("此电脑") > -1) accesspaths.Remove("此电脑");
                        accesspaths.Insert(0, "此电脑");
                        RinGO_GetDriveListview();
                        break;
                    }
                case "回收站":
                    {
                        if (accesspaths.IndexOf("回收站") > -1) accesspaths.Remove("回收站");
                        accesspaths.Insert(0, "回收站");
                        RinGO_GetRecyleListView();
                        break;
                    }
                case "主文件夹":
                    {
                        if (accesspaths.IndexOf("主文件夹") > -1) accesspaths.Remove("主文件夹");
                        accesspaths.Insert(0, "主文件夹");
                        RinGO_GetfavoritesListView();
                        break;
                    }
                default:
                    {

                        flag = RinGO_GetFolderListview(tn.Tag.ToString());
                        if (flag == 0)
                        {
                            if (accesspaths.IndexOf(tn.Tag.ToString()) > -1) accesspaths.Remove(tn.Tag.ToString());
                            accesspaths.Insert(0, tn.Tag.ToString());
                        }
                        break;
                    }
            }
            if (flag == 0)
            {
                combo_url.SelectedIndexChanged -= new EventHandler(RinGO_combo_url_SelectedIndexChanged);
                combo_url.DataSource = null;
                combo_url.DataSource = accesspaths;//******前面有代码
                combo_url.SelectedIndex = 0;//*********后面有代码
                combo_url.SelectedIndexChanged += new EventHandler(RinGO_combo_url_SelectedIndexChanged);

            }
            else MessageBox.Show("访问失败.缺少权限货设备未就绪", "错误");
        }

        /// <summary>
        /// 获取指定路径下的文件和目录对象,显示在listview1
        /// </summary>
        private int RinGO_GetFolderListview(string p)
        {
            listView1.Items.Clear();
            RinGO_CreateCol_F();
            ListViewItem lv;
            string[] dirs;
            string[] files;
            try
            {
                //获取路径p的子目录
                dirs = Directory.GetDirectories(p);
                //获取路径p下的文件
                files = Directory.GetFiles(p);
            }
            catch
            {
                return 1;
            }
            //处理目录对象
            foreach (string dir in dirs)
            {
                try
                {
                    DirectoryInfo dinfo = new DirectoryInfo(dir);
                    lv = new ListViewItem(dinfo.Name);
                    lv.Tag = dinfo.FullName;
                    lv.ImageKey = "defaultfolder";
                    //类型
                    lv.SubItems.Add("文件夹");
                    //修改时间
                    lv.SubItems.Add(dinfo.LastWriteTime.ToString());
                    //大小
                    lv.SubItems.Add("");
                    //创建时间
                    lv.SubItems.Add(dinfo.CreationTime.ToString());
                    listView1.Items.Add(lv);

                }
                catch { }
            }
            //处理文件对象
            foreach (string f in files)
            {
                try
                {
                    FileInfo finfo = new FileInfo(f);
                    //名称
                    lv = new ListViewItem(finfo.Name);
                    lv.Tag = finfo.FullName;
                    //根据扩展名提取图标
                    lv.ImageKey = RinGO_GetFileIconKey(finfo.Extension, finfo.FullName);
                    //获取文件类型名称
                    string typename = geticon.GetTypeName(finfo.FullName);
                    //类型
                    lv.SubItems.Add(typename);
                    //修改时间
                    lv.SubItems.Add(finfo.LastWriteTime.ToString());
                    long size = finfo.Length;
                    string sizestring = "";
                    if (size < 1024)
                    {
                        sizestring = size.ToString() + " Byte";
                    }
                    else if (size >= 1024 && size < 1024 * 1024)
                    {
                        sizestring = (size / 1024).ToString() + " KB";
                    }
                    else if (size >= 1024 * 1024 && size < 1024 * 1024 * 1024)
                    {
                        sizestring = ((size / 1024) / 1024).ToString() + " MB";
                    }
                    else
                    {
                        sizestring = (((size / 1024) / 1024) / 1024).ToString() + " GB";
                    }
                    //大小
                    lv.SubItems.Add(sizestring);
                    //创建时间
                    lv.SubItems.Add(finfo.CreationTime.ToString());
                    listView1.Items.Add(lv);
                }
                catch { }

            }
            lb_ojbnum.Text = listView1.Items.Count.ToString();
            return 0;

        }

        /// <summary>
        /// 读取主文件夹的对象,显示在listview1
        /// </summary>
        private void RinGO_GetfavoritesListView()
        {
            listView1.Items.Clear();
            RinGO_CreateCol_F();
            string mypath = "";
            mypath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            ListViewItem lv = new ListViewItem("文档");// 名称
            lv.Tag = mypath;
            lv.ImageKey = "mydocument";
            DirectoryInfo dinfo = new DirectoryInfo(mypath);
            lv.SubItems.Add("文件夹");//类型
            lv.SubItems.Add(dinfo.LastWriteTime.ToString());//修改时间lv.SubItems.Add(“文件夹“)://类型
            lv.SubItems.Add("");//大小
            lv.SubItems.Add(dinfo.CreationTime.ToString());//创建时间
            listView1.Items.Add(lv);

            mypath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            if (mypath != null && !mypath.Equals(""))
            {
                lv = new ListViewItem("音乐");//名称
                lv.Tag = mypath;
                lv.ImageKey = "mymusic";
                dinfo = new DirectoryInfo(mypath);
                lv.SubItems.Add("文件夹");//类型
                lv.SubItems.Add(dinfo.LastWriteTime.ToString());//修改时间
                lv.SubItems.Add("");//l大小
                lv.SubItems.Add(dinfo.CreationTime.ToString());//创建时间
                listView1.Items.Add(lv);
            }

            mypath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if (mypath != null && !mypath.Equals(""))
            {
                lv = new ListViewItem("图片");//名称
                lv.Tag = mypath;
                lv.ImageKey = "mypictures";

                dinfo = new DirectoryInfo(mypath);
                lv.SubItems.Add("文件夹");//类型
                lv.SubItems.Add(dinfo.LastWriteTime.ToString());//修改时间

                lv.SubItems.Add("");//大小
                lv.SubItems.Add(dinfo.CreationTime.ToString());//创建时间
                listView1.Items.Add(lv);
            }

            mypath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            if (mypath != null && !mypath.Equals(""))
            {
                lv = new ListViewItem("视频");// 名称
                lv.Tag = mypath;
                lv.ImageKey = "myvideos";
                dinfo = new DirectoryInfo(mypath);
                lv.SubItems.Add("文件夹");//11类型
                lv.SubItems.Add(dinfo.LastWriteTime.ToString());//修改时间

                lv.SubItems.Add("");//大小
                lv.SubItems.Add(dinfo.CreationTime.ToString());// 创建时间
                listView1.Items.Add(lv);
            }

            //修改状态栏的对象教目
            toolStripStatusLabel1.Text = listView1.Items.Count.ToString();
        }

        /// <summary>
        /// 读取回收站的对象,显示在listview1
        /// </summary>
        private void RinGO_GetRecyleListView()
        {
            listView1.Items.Clear();
            RinGO_CreateCol_R();
            Shell shell = new Shell();
            Folder recycleBin = shell.NameSpace(10);

            foreach (FolderItem f in recycleBin.Items())
            {
                ListViewItem lv = new ListViewItem(f.Name);
                lv.Tag = f.Path;//路径
                //lv.Tag = selectedItemsDirectory;
                lv.IndentCount = 1;
                //文件夹
                if (f.IsFolder)
                {
                    lv.ImageKey = "defaultfolder";
                }
                //文件
                else
                {
                    lv.ImageKey = RinGO_GetFileIconKey(f.Path.Substring(f.Path.LastIndexOf('.')), f.Path);
                }
                lv.SubItems.Add(f.Type);
                lv.SubItems.Add(f.Path);
                lv.SubItems.Add(f.ModifyDate.ToString());
                listView1.Items.Add(lv);
                lb_ojbnum.Text = listView1.Items.Count.ToString();
            }
        }

        /// <summary>
        /// 读取桌面的对象,显示在listview1
        /// </summary>
        private void RinGO_GetDesktopListview()
        {
            //特殊对象的读取
            listView1.Items.Clear();
            RinGO_CreateCol_F();
            ListViewItem lv = new ListViewItem("此电脑");
            lv.Tag = "mycomputer";
            lv.ImageKey = "computer";
            lv.SubItems.Add("");
            lv.SubItems.Add("");
            lv.SubItems.Add("");
            lv.SubItems.Add("");
            listView1.Items.Add(lv);

            lv = new ListViewItem("linqin");
            lv.Tag = ("C:\\Users\\linqin\\");
            lv.ImageKey = "defaultfolder";
            lv.SubItems.Add("");
            lv.SubItems.Add("");
            lv.SubItems.Add("");
            lv.SubItems.Add("");
            listView1.Items.Add(lv);

            lv = new ListViewItem("网络");
            lv.Tag = "network";
            lv.ImageKey = "network";
            lv.SubItems.Add("");
            lv.SubItems.Add("");
            lv.SubItems.Add("");
            lv.SubItems.Add("");
            listView1.Items.Add(lv);
            //桌面一级目录下的读取
            //string desktopPath = GetPhysicalDesktopPath()

            //普通文件和文件夹对象的读取
            //*****获取桌面目录和文件
            string[] dirs;
            string[] files;
            string p = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            try
            {
                dirs = Directory.GetDirectories(p);//获取路径p的子目录
                files = Directory.GetFiles(p);//获取路径p下的文件

                foreach (string dir in dirs)//处理目录
                {
                    try
                    {
                        DirectoryInfo dinfo = new DirectoryInfo(dir);
                        lv = new ListViewItem(dinfo.Name);
                        lv.Tag = dinfo.FullName;
                        lv.ImageKey = "defaultfolder";
                        lv.SubItems.Add("文件夹");//类型
                        lv.SubItems.Add(dinfo.LastWriteTime.ToString());//修改时间
                        lv.SubItems.Add("");//大小
                        lv.SubItems.Add(dinfo.CreationTime.ToString());//创建时间
                        listView1.Items.Add(lv);
                    }
                    catch { }

                }
                foreach (string f in files)//读取文件
                {
                    try
                    {
                        FileInfo finfo = new FileInfo(f);
                        //名称
                        lv = new ListViewItem(finfo.Name);
                        lv.Tag = finfo.FullName;
                        //根据扩展名提取图标
                        lv.ImageKey = RinGO_GetFileIconKey(finfo.Extension, finfo.FullName);
                        //获取文件类型名称
                        string typename = geticon.GetTypeName(finfo.FullName);
                        //类型
                        lv.SubItems.Add(typename);
                        //修改时间
                        lv.SubItems.Add(finfo.LastWriteTime.ToString());
                        long size = finfo.Length;
                        string sizestring = "";
                        if (size < 1024)
                        {
                            sizestring = size.ToString() + " Byte";
                        }
                        else if (size >= 1024 && size < 1024 * 1024)
                        {
                            sizestring = (size / 1024).ToString() + " KB";
                        }
                        else if (size >= 1024 * 1024 && size < 1024 * 1024 * 1024)
                        {
                            sizestring = ((size / 1024) / 1024).ToString() + " MB";
                        }
                        else
                        {
                            sizestring = (((size / 1024) / 1024) / 1024).ToString() + " GB";
                        }
                        //大小
                        lv.SubItems.Add(sizestring);
                        //创建时间
                        lv.SubItems.Add(finfo.CreationTime.ToString());
                        listView1.Items.Add(lv);
                    }
                    catch { }

                }
            }
            catch { }
            lb_ojbnum.Text = listView1.Items.Count.ToString();
        }

        private string RinGO_GetFileIconKey(string exten, string fullname)
        {
            string imgkey = "";
            Icon[] myIcon;
            //提取可执行文件/快捷方式的专用图标，如果失败则使用默认可执行文件图标/未知文件图标
            if (exten.ToUpper().Equals(".EXE") || exten.ToUpper().Equals(".LNK"))
            {
                myIcon = geticon.GetIconByFileName(fullname, FileAttributes.Normal);
                if (myIcon != null)
                {
                    if (myIcon[0] != null && myIcon[1] != null)
                    {
                        //更新该类型文件的图标
                        if (imageList1.Images.ContainsKey(fullname))
                        {
                            imageList1.Images.RemoveByKey(fullname);
                        }
                        if (imageList2.Images.ContainsKey(fullname))
                        {
                            imageList2.Images.RemoveByKey(fullname);
                        }
                        imageList1.Images.Add(fullname, myIcon[0]);
                        imageList2.Images.Add(fullname, myIcon[1]);
                        imgkey = fullname;
                    }
                }
                //如果获取图标失败，则设置默认图标
                if (imgkey == "")
                {
                    if (exten.ToUpper().Equals(".EXE"))
                    {
                        imgkey = "defaultexeicon";
                    }
                    else
                    {
                        imgkey = "unknowicon";
                    }
                }
            }
            //根据扩展名提取图标，如果失败，则使用默认的未知文件图标
            else
            {
                myIcon = geticon.GetIconByFileType(exten);
                if (myIcon != null)
                {
                    if (myIcon[0] != null && myIcon[1] != null)
                    {//更新该类型文件的图标
                        if (imageList1.Images.ContainsKey(exten))
                        {
                            imageList1.Images.RemoveByKey(exten);
                        }
                        if (imageList2.Images.ContainsKey(exten))
                        {
                            imageList2.Images.RemoveByKey(exten);
                        }
                        imageList1.Images.Add(exten, myIcon[0]);
                        imageList2.Images.Add(exten, myIcon[1]);
                        imgkey = exten;
                    }
                    else
                    {
                        imgkey = "unknowicon";
                    }
                }
                else
                {
                    imgkey = "unknowicon";
                }
            }


            return imgkey;

        }

        private void RinGO_treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            //单击"+",展开其下一级子目录
            TreeNode tn = e.Node;
            //如果是此电脑节点,则读取硬盘驱动器
            if (tn.Tag.Equals("mycomputer"))
            {
                tn.Nodes.Clear();
                RinGO_GetFogerTree(tn);
            }
            else //否则,如果不是主文件夹,则读取子目录,并显示在treeview中
                 if (!tn.Tag.Equals("favorites"))
            {
                tn.Nodes.Clear();
                RinGO_GetFolderTree(tn);
            }
        }

        /// <summary>
        /// 根据节点代表的文件夹路径,获得子目录并显示
        /// </summary>
        /// <param name="tn">即将展开的节点</param>
        private void RinGO_GetFolderTree(TreeNode tn)
        {
            string folderpath = tn.Tag.ToString();
            string[] f_names = Directory.GetDirectories(folderpath);
            foreach (string fn in f_names)
            {
                DirectoryInfo dinfo = new DirectoryInfo(fn);
                TreeNode newtn = new TreeNode(dinfo.Name);
                newtn.Tag = dinfo.FullName;//完整路径
                newtn.SelectedImageKey = newtn.ImageKey = "defaultfolder";
                try
                {
                    //判断是否有子目录,有则添加临时子节点.以便显示"+"号
                    string[] temps = Directory.GetDirectories(fn);
                    if (temps.Length > 0) newtn.Nodes.Add("temp");
                }
                catch { }
                tn.Nodes.Add(newtn);
            }
        }

        private void RinGO_GetDriveListview()
        {
            listView1.Items.Clear();
            RinGO_CreateCol_D();//创建列
            DriveInfo[] drivers = DriveInfo.GetDrives();//提取计算机的驱动器集合
            string lvname1, lvname2, lvtype, keyname, lvtotal = "", lvfree = "";
            foreach (DriveInfo driver in drivers)
            {
                ListViewItem newitem = new ListViewItem();
                newitem.IndentCount = 1;
                if (driver.IsReady) lvname1 = driver.VolumeLabel;//卷标，没有则为空串
                else lvname1 = "";
                lvname2 = driver.Name;//驱动器的名称，如C:\
                switch (driver.DriveType)
                {
                    case DriveType.Fixed:
                        {
                            keyname = "localdriver";//本地磁盘
                            lvtype = "本地磁盘";
                            if (lvname1.Equals("")) lvname1 = "本地磁盘";
                            newitem.Group = listView1.Groups["lvGroup1"]; break;
                        }
                    case DriveType.Removable:
                        {
                            keyname = "movabledriver";//移动磁盘
                            lvtype = "移动存储";
                            if (lvname1.Equals("")) lvname1 = "移动存储";
                            newitem.Group = listView1.Groups["lvGroup2"]; break;
                        }
                    case DriveType.CDRom:
                        {
                            keyname = "cdrom";//光驱
                            lvtype = "光盘驱动器";
                            if (lvname1.Equals("")) lvname1 = "光盘驱动器";
                            newitem.Group = listView1.Groups["lvGroup2"]; break;
                        }
                    default:
                        {
                            keyname = "movabledriver";
                            lvtype = "未知设备";
                            if (lvname1.Equals("")) lvname1 = "未知设备";
                            newitem.Group = listView1.Groups["lvGroup3"]; break;
                        }
                }
                newitem.SubItems[0].Text = (lvname1 + "(" + lvname2.Substring(0, 2) + ")");//名称
                newitem.SubItems.Add(lvtype);//类型
                if (driver.IsReady)
                {
                    lvtotal = Math.Round(driver.TotalSize / (1024 * 1024 * 1024 * 1.0), 1).ToString() + " GB";
                    lvfree = Math.Round(driver.TotalFreeSpace / (1024 * 1024 * 1024 * 1.0), 1).ToString() + " GB";
                }
                newitem.SubItems.Add(lvtotal);//总大小
                newitem.SubItems.Add(lvfree);//可用空间大小
                newitem.ImageKey = keyname;//图标的key
                newitem.Tag = lvname2;
                listView1.Items.Add(newitem);
            }
            //统计list view1中显示的对象数目，并记录在状态栏中的标签内
            lb_ojbnum.Text = listView1.Items.Count.ToString();
        }

        private static void RinGO_GetFogerTree(TreeNode root)
        {
            DriveInfo[] drivers = DriveInfo.GetDrives();
            string keyname = "";
            string drivername = "";
            string drivertag = "";
            foreach (DriveInfo driver in drivers)
            {
                if (driver.IsReady) drivername = driver.VolumeLabel;
                else drivername = "";
                switch (driver.DriveType)
                {
                    case DriveType.Fixed:
                        keyname = "localdriver"; if (drivername.Equals("")) drivername = "本地磁盘";
                        break;//本地磁盘
                    case DriveType.Removable:
                        keyname = "movabledriver"; if (drivername.Equals("")) drivername = "移动存储";
                        break;//移动磁盘
                    case DriveType.CDRom:
                        keyname = "cdrom"; if (drivername.Equals("")) drivername = "光盘驱动器";
                        break;//光驱
                    default:
                        keyname = "movabledriver"; if (drivername.Equals("")) drivername = "未知设备";
                        break;//未知驱动器

                }
                drivername = drivername + "(" + driver.Name.Substring(0, 2) + ")";
                drivertag = driver.Name;
                TreeNode tn = new TreeNode(drivername);
                tn.SelectedImageKey = tn.ImageKey = keyname;
                tn.Tag = drivertag;
                if (driver.IsReady)
                {
                    try
                    {
                        DirectoryInfo driver_info = new DirectoryInfo(driver.Name);
                        DirectoryInfo[] dirs = driver_info.GetDirectories();
                        if (dirs.Length > 0) tn.Nodes.Add("temp");
                    }
                    catch { }
                }
                root.Nodes.Add(tn);
            }//foreach结束

        }

        //定义RinGO_listView1_ColumnClick()并绑定给listview1中的ColumnClick事件，实现单击后排序
        private void RinGO_listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == sortedColumn.Index)
            {
                // 如果点击的是当前排序列，改变排序顺序
                if (sortOrder == SortOrder.Ascending)
                    sortOrder = SortOrder.Descending;
                else
                    sortOrder = SortOrder.Ascending;
            }
            else
            {
                // 如果点击的是新列头，设置当前排序列为点击的列
                sortedColumn = listView1.Columns[e.Column];
                sortOrder = SortOrder.Ascending;
            }

            // 调用ListView的Sort方法进行排序
            listView1.ListViewItemSorter = new ListViewItemComparer(e.Column, sortOrder);

            // 更新列头显示的排序箭头图标
            listView1.Sort();
        }

        public class ListViewItemComparer : IComparer
        {
            private int column;
            private SortOrder sortOrder;
            //private int dateColumnIndex;

            public ListViewItemComparer(int column, SortOrder sortOrder)
            {
                this.column = column;
                this.sortOrder = sortOrder;
                //this.dateColumnIndex = dateColumnIndex;
            }

            public int Compare(object x, object y)
            {
                ListViewItem listViewItemX = (ListViewItem)x;
                ListViewItem listViewItemY = (ListViewItem)y;

                string xValue = listViewItemX.SubItems[column].Text;
                string yValue = listViewItemY.SubItems[column].Text;

                // 检查是否为特定格式的值，并将空值或不存在的情况处理为"0 Byte"
                xValue = NormalizeValue(xValue);
                yValue = NormalizeValue(yValue);
                // 检查是否为日期格式的值
                DateTime xDateTime;
                DateTime yDateTime;
                bool isXDateTime = DateTime.TryParse(xValue, out xDateTime);
                bool isYDateTime = DateTime.TryParse(yValue, out yDateTime);

                if (isXDateTime && isYDateTime)
                {
                    // 如果是日期型数据，则按照时间顺序进行排序
                    int result = DateTime.Compare(xDateTime, yDateTime);
                    if (sortOrder == SortOrder.Descending)
                        result = -result;

                    return result;
                }
                // 根据不同的值进行排序
                if (IsGBValue(xValue) && IsGBValue(yValue))
                {
                    double xSize = GetNumericValue(xValue);
                    double ySize = GetNumericValue(yValue);
                    return CompareNumericValues(xSize, ySize);
                }
                else if ((IsKBValue(xValue) || IsByteValue(xValue) || IsMBValue(xValue) || IsGBValue(xValue)) &&
                    (IsKBValue(yValue) || IsByteValue(yValue) || IsMBValue(yValue) || IsGBValue(yValue)))
                {
                    double xSize = ConvertToByteValue(xValue);
                    double ySize = ConvertToByteValue(yValue);
                    return CompareNumericValues(xSize, ySize);
                }
                else
                {
                    // 默认按照字符串比较
                    int result = string.Compare(xValue, yValue);
                    if (sortOrder == SortOrder.Descending)
                        result = -result;

                    return result;
                }
            }

            private string NormalizeValue(string value)
            {
                // 将空值或不存在的情况处理为"-1 Byte"(文件夹类型或者特殊位置，因为其没有大小，故这样设置，方便排序)
                if (string.IsNullOrWhiteSpace(value))
                {
                    // 将空值或不存在的情况处理为"-1 Byte"
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return "-1 Byte";
                    }
                }

                return value;
            }

            private bool IsMBValue(string value)
            {
                // 判断是否为 MB 值
                //throw new NotImplementedException();
                return value.EndsWith(" MB");
            }
            private bool IsGBValue(string value)
            {
                // 判断是否为 GB 值
                return value.EndsWith(" GB");
            }
            private bool IsKBValue(string value)
            {
                // 判断是否为 KB 值
                return value.EndsWith(" KB");
            }

            private bool IsByteValue(string value)
            {
                // 判断是否为 Byte 值
                return value.EndsWith(" Byte");
            }

            private double GetNumericValue(string value)
            {
                // 提取数字部分并转换为 double 类型
                string numericPart = value.Split(' ')[0];
                return double.Parse(numericPart);
            }

            private double ConvertToByteValue(string value)
            {
                // 将GB MB KB 或 Byte 值转换为对应的 Byte 值
                string numericPart = value.Split(' ')[0];
                double numericValue = double.Parse(numericPart);
                if (value.EndsWith(" MB"))
                {
                    numericValue *= 1024 * 1024;
                }
                if (value.EndsWith(" GB"))
                {
                    numericValue *= 1024 * 1024 * 1024;
                }
                if (value.EndsWith(" KB"))
                {
                    numericValue *= 1024;
                }
                else if (value.EndsWith(" Byte"))
                {
                    numericValue /= 1024;
                }

                return numericValue;
            }

            private int CompareNumericValues(double x, double y)
            {
                // 按照数字大小进行比较
                if (x < y)
                {
                    return sortOrder == SortOrder.Ascending ? -1 : 1;
                }
                else if (x > y)
                {
                    return sortOrder == SortOrder.Ascending ? 1 : -1;
                }
                else
                {
                    return 0;
                }
            }
        }

        private void RinGO_splitContainer1_Panel1_SizeChanged(object sender, EventArgs e)
        {
            combo_url.Width = splitContainer1.Panel1.Width - btn_next.Width - btn_pre.Width - 12;
        }

        private void RinGO_splitContainer1_Panel2_SizeChanged(object sender, EventArgs e)
        {
            text_search.Width = splitContainer1.Panel2.Width - btn_search.Width - 6;
        }

        private void RinGO_CreateCol_D()
        {
            listView1.Columns.Clear();

            ColumnHeader columnHeader1 = new ColumnHeader();
            columnHeader1.Text = "名称";
            columnHeader1.TextAlign = HorizontalAlignment.Left;
            columnHeader1.Width = 200;
            columnHeader1.Name = "chname";
            listView1.Columns.Add(columnHeader1);

            columnHeader1 = new ColumnHeader();
            columnHeader1.Text = "类型";
            columnHeader1.TextAlign = HorizontalAlignment.Left;
            columnHeader1.Width = 100;
            columnHeader1.Name = "chtype";
            listView1.Columns.Add(columnHeader1);

            columnHeader1 = new ColumnHeader();
            columnHeader1.Text = "总大小";
            columnHeader1.TextAlign = HorizontalAlignment.Right;
            columnHeader1.Width = 120;
            columnHeader1.Name = "chtotal";
            listView1.Columns.Add(columnHeader1);

            columnHeader1 = new ColumnHeader();
            columnHeader1.Text = "可用大小";
            columnHeader1.TextAlign = HorizontalAlignment.Right;
            columnHeader1.Width = 120;
            columnHeader1.Name = "chfree";
            listView1.Columns.Add(columnHeader1);
        }

        private void RinGO_CreateCol_F()
        {
            listView1.Columns.Clear();

            ColumnHeader columnHeader1 = new ColumnHeader();
            columnHeader1.Text = "名称";
            columnHeader1.TextAlign = HorizontalAlignment.Left;
            columnHeader1.Width = 200;
            columnHeader1.Name = "chname";
            listView1.Columns.Add(columnHeader1);

            columnHeader1 = new ColumnHeader();
            columnHeader1.Text = "类型";
            columnHeader1.TextAlign = HorizontalAlignment.Left;
            columnHeader1.Width = 100;
            columnHeader1.Name = "chtype";
            listView1.Columns.Add(columnHeader1);

            columnHeader1 = new ColumnHeader();
            columnHeader1.Text = "修改时间";
            columnHeader1.TextAlign = HorizontalAlignment.Left;
            columnHeader1.Width = 120;
            columnHeader1.Name = "chmodify";
            listView1.Columns.Add(columnHeader1);

            columnHeader1 = new ColumnHeader();
            columnHeader1.Text = "大小";
            columnHeader1.TextAlign = HorizontalAlignment.Right;
            columnHeader1.Width = 120;
            columnHeader1.Name = "chtotal";
            listView1.Columns.Add(columnHeader1);

            columnHeader1 = new ColumnHeader();
            columnHeader1.Text = "创建时间";
            columnHeader1.TextAlign = HorizontalAlignment.Left;
            columnHeader1.Width = 120;
            columnHeader1.Name = "chcreate";
            listView1.Columns.Add(columnHeader1);
        }

        private void RinGO_CreateCol_R()
        {
            listView1.Columns.Clear();
            ColumnHeader columnHeader1 = new ColumnHeader();
            columnHeader1.Text = "名称";
            columnHeader1.TextAlign = HorizontalAlignment.Left;
            columnHeader1.Width = 200;
            columnHeader1.Name = "chname";
            listView1.Columns.Add(columnHeader1);
            columnHeader1 = new ColumnHeader();
            columnHeader1.Text = "类型";
            columnHeader1.TextAlign = HorizontalAlignment.Left;
            columnHeader1.Width = 100;
            columnHeader1.Name = "chtype";
            listView1.Columns.Add(columnHeader1);
            columnHeader1 = new ColumnHeader(); columnHeader1.Text = "位置";
            columnHeader1.TextAlign = HorizontalAlignment.Left;
            columnHeader1.Width = 200;
            columnHeader1.Name = "chpath";
            listView1.Columns.Add(columnHeader1);
            columnHeader1 = new ColumnHeader();
            columnHeader1.Text = "修改日期";
            columnHeader1.TextAlign = HorizontalAlignment.Left;
            columnHeader1.Width = 120;
            columnHeader1.Name = "chdel";
            listView1.Columns.Add(columnHeader1);
        }

        private void RinGO_btn_pre_Click(object sender, EventArgs e)
        {
            if (combo_url.SelectedIndex == combo_url.Items.Count - 1)
            {
                MessageBox.Show("哦呀，您已经来到了后退的最后一个目录！", "提示！", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            else
            {
                flag_pre_next = true;
                combo_url.SelectedIndex += 1;
            }
        }

        private void RinGO_btn_next_Click(object sender, EventArgs e)
        {
            if (combo_url.SelectedIndex == combo_url.Items.Count - 1)
            {
                MessageBox.Show("哈哈，这里已经是最新的目录了！", "提示！", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            else
            {
                flag_pre_next = true;
                combo_url.SelectedIndex -= 1;
            }
        }

        private void RinGO_combo_url_SelectedIndexChanged(object sender, EventArgs e)
        {
            int flag = 0;
            string newpath = combo_url.Text.Trim();
            switch (newpath)
            {
                case "桌面":
                    {
                        if (flag_pre_next == false)
                        {
                            accesspaths.Remove("桌面");
                            accesspaths.Insert(0, "桌面");
                        }
                        RinGO_GetDesktopListview();
                        break;
                    }
                case "此电脑":
                    {
                        if (flag_pre_next == false)
                        {
                            accesspaths.Remove("此电脑");
                            accesspaths.Insert(0, "此电脑");
                        }
                        RinGO_GetDriveListview();
                        break;
                    }
                case "回收站":
                    {
                        if (flag_pre_next == false)
                        {
                            accesspaths.Remove("回收站");
                            accesspaths.Insert(0, "回收站");
                        }
                        RinGO_GetRecyleListView();
                        break;
                    }
                case "主文件夹":
                    {
                        if (flag_pre_next == false)
                        {
                            accesspaths.Remove("主文件夹");
                            accesspaths.Insert(0, "主文件夹");
                        }
                        RinGO_GetfavoritesListView();
                        break;
                    }
                default:
                    {
                        flag = RinGO_GetFolderListview(newpath);
                        if (flag_pre_next == false)
                        {
                            accesspaths.Remove(newpath);
                            accesspaths.Insert(0, newpath);
                        }
                        break;
                    }
            }

            if (flag_pre_next == false)
            {
                // 重新绑定combo_url
                combo_url.SelectedIndexChanged -= new EventHandler(RinGO_combo_url_SelectedIndexChanged);
                combo_url.DataSource = null;
                combo_url.DataSource = accesspaths;
                combo_url.SelectedIndex = 0;
                combo_url.SelectedIndexChanged += new EventHandler(RinGO_combo_url_SelectedIndexChanged);
                if (flag == 1)
                {
                    listView1.Items.Clear();
                    MessageBox.Show("访问失败，缺少权限或设备未就绪", "错误");
                }
                flag_pre_next = false;
            }
        }

        private void RinGO_listView1_ItemActivate(object sender, EventArgs e)
        {
            ListViewItem fitem = listView1.FocusedItem;//获取被双击的对象
            string fullname = fitem.Tag.ToString();
            string urltext = combo_url.Text;//双击的对象的父目录路径
            string mytype = fitem.SubItems[1].Text;//记得listview1的第二列统一为类型列

            if (urltext.Equals("此电脑"))//父目录路径是此电脑
            {
                DriveInfo dinfo = new DriveInfo(fullname);
                if (dinfo.IsReady)
                {
                    mytype = "文件夹";//如果驱动器就绪﹐则作为文件夹处理
                }
                else
                {
                    MessageBox.Show("设备未就绪，无法读取", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
            }
            if (urltext.Equals("回收站"))
            {
                //父目录路径是回收站，则对象还有待处理，目前信息不足。
                MessageBox.Show("回收站的对象不能直接访问!", "警告!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                return;
            }
            if (urltext.Equals("桌面") && fitem.SubItems[0].Text.Equals("网络"))
            {
                MessageBox.Show("网上邻居功能还未实现∶");
                return;
            }
            if (urltext.Equals("桌面") && fitem.SubItems[0].Text.Equals("文档")) mytype = "文件夹";
            switch (mytype)
            {
                case "文件夹":
                    {
                        if (accesspaths.IndexOf(fullname) > -1)
                        {
                            accesspaths.Remove(fullname);
                        }
                        accesspaths.Insert(0, fullname);
                        RinGO_GetFolderListview(fullname);
                        combo_url.SelectedIndexChanged -= new EventHandler(RinGO_combo_url_SelectedIndexChanged);
                        combo_url.DataSource = null;
                        combo_url.DataSource = accesspaths;
                        combo_url.SelectedIndex = 0;
                        combo_url.SelectedIndexChanged += new EventHandler(RinGO_combo_url_SelectedIndexChanged);
                        break;
                    }

                case "":
                    {
                        if (fitem.SubItems[0].Text.Equals("此电脑"))
                        {
                            if (accesspaths.IndexOf("此电脑") > -1)
                            {
                                accesspaths.Remove("此电脑");
                            }
                            accesspaths.Insert(0, "此电脑");
                            RinGO_GetDriveListview();
                            combo_url.SelectedIndexChanged -= new EventHandler(RinGO_combo_url_SelectedIndexChanged);
                            combo_url.DataSource = null;
                            combo_url.DataSource = accesspaths;
                            combo_url.SelectedIndex = 0;
                            combo_url.SelectedIndexChanged += new EventHandler(RinGO_combo_url_SelectedIndexChanged);
                        }
                        if (fitem.SubItems[0].Text.Equals("回收站"))
                        {
                            if (accesspaths.IndexOf("回收站") > -1)
                            {
                                accesspaths.Remove("回收站");
                            }
                            accesspaths.Insert(0, "回收站");
                            RinGO_GetRecyleListView();
                            combo_url.SelectedIndexChanged -= new EventHandler(RinGO_combo_url_SelectedIndexChanged);
                            combo_url.DataSource = null;
                            combo_url.DataSource = accesspaths;
                            combo_url.SelectedIndex = 0;
                            combo_url.SelectedIndexChanged += new EventHandler(RinGO_combo_url_SelectedIndexChanged);
                        }
                        break;
                    }
                default:
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(fullname);
                        }
                        catch
                        {
                            MessageBox.Show("无法打开或者运行该文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }
                        break;
                    }
            }
        }

        private void RinGO_toolStripButton2_Click(object sender, EventArgs e)
        {
            //刷新，重新安装事件
            RinGO_GetFolderListview(combo_url.Items[0].ToString());
            combo_url.SelectedIndexChanged -= new EventHandler(RinGO_combo_url_SelectedIndexChanged);
            combo_url.DataSource = null;
            combo_url.DataSource = accesspaths;
            combo_url.SelectedIndex = 0;
            combo_url.SelectedIndexChanged += new EventHandler(RinGO_combo_url_SelectedIndexChanged);
        }

        private void RinGO_toolStripButton3_Click(object sender, EventArgs e)
        {//返回上一级
            string currpath = combo_url.Text;
            //简单暴力的处理一下警告
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string A = @"A:\"; //如果是软盘对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string B = @"B:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string C = @"C:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string D = @"D:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string E = @"E:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string F = @"F:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string G = @"G:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string H = @"H:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string I = @"I:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string J = @"J:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string K = @"K:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string L = @"L:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string M = @"M:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string N = @"N:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string O = @"O:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string P = @"P:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string Q = @"Q:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string R = @"R:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string S = @"S:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string T = @"T:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string U = @"U:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string V = @"V:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string W = @"W:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string X = @"X:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string Y = @"Y:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
            string Z = @"Z:\"; //驱动器对象
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值

            switch (currpath)
            {
                case "桌面": return;
                case "回收站": combo_url.Text = "桌面"; break;
                case "此电脑": combo_url.Text = "桌面"; break;
                case "主文件夹": combo_url.Text = "桌面"; break;

                case @"A:\":
                    combo_url.Text = "此电脑"; break;
                case @"B:\":
                    combo_url.Text = "此电脑"; break;
                case @"C:\":
                    combo_url.Text = "此电脑"; break;
                case @"D:\":
                    combo_url.Text = "此电脑"; break;
                case @"E:\":
                    combo_url.Text = "此电脑"; break;
                case @"F:\":
                    combo_url.Text = "此电脑"; break;
                case @"G:\":
                    combo_url.Text = "此电脑"; break;
                case @"H:\":
                    combo_url.Text = "此电脑"; break;
                case @"I:\":
                    combo_url.Text = "此电脑"; break;
                case @"J:\":
                    combo_url.Text = "此电脑"; break;
                case @"K:\":
                    combo_url.Text = "此电脑"; break;
                case @"L:\":
                    combo_url.Text = "此电脑"; break;
                case @"M:\":
                    combo_url.Text = "此电脑"; break;
                case @"N:\":
                    combo_url.Text = "此电脑"; break;
                case @"O:\":
                    combo_url.Text = "此电脑"; break;
                case @"P:\":
                    combo_url.Text = "此电脑"; break;
                case @"Q:\":
                    combo_url.Text = "此电脑"; break;
                case @"R:\":
                    combo_url.Text = "此电脑"; break;
                case @"S:\":
                    combo_url.Text = "此电脑"; break;
                case @"T:\":
                    combo_url.Text = "此电脑"; break;
                case @"U:\":
                    combo_url.Text = "此电脑"; break;
                case @"V:\":
                    combo_url.Text = "此电脑"; break;
                case @"W:\":
                    combo_url.Text = "此电脑"; break;
                case @"X:\":
                    combo_url.Text = "此电脑"; break;
                case @"Y:\":
                    combo_url.Text = "此电脑"; break;
                case @"Z:\":
                    combo_url.Text = "此电脑"; break;
                default:
                    {
                        try { combo_url.Text = Directory.GetParent(currpath).FullName; }
                        catch { combo_url.Text = "此电脑"; }
                        break;
                    }
            }
            RinGO_combo_url_SelectedIndexChanged(null, null);
        }

        private void RinGO_toolStripSplitButton2_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripSplitButton tsb = (ToolStripSplitButton)sender;
            for (int i = 0; i < tsb.DropDownItems.Count; i++)
            {
                if (tsb.DropDownItems[i] != e.ClickedItem)
                {
                    ((ToolStripMenuItem)tsb.DropDownItems[i]).Checked = false;
                }
                else
                {
                    ((ToolStripMenuItem)tsb.DropDownItems[i]).Checked = true;
                }
                switch (e.ClickedItem.Text)
                {
                    case "大图标":
                        listView1.View = View.LargeIcon; break;
                    case "小图标":
                        listView1.View = View.SmallIcon; break;
                    case "列表":
                        listView1.View = View.List; break;
                    case "详细列表":
                        listView1.View = View.Details; break;
                    default:
                        listView1.View = View.Tile; break;
                }
            }
        }

        private void RinGO_contextMenu_lv_Opening(object sender, CancelEventArgs e)
        {
            // 复制，剪切,粘贴﹐删除，重命名，新建，刷新,属性
            if (listView1.SelectedItems.Count == 0)
            {
                contextMenu_lv.Items["item_copy"].Enabled = false;
                contextMenu_lv.Items["item_cut"].Enabled = false;
                if (copyobjs.Count == 0)
                {
                    contextMenu_lv.Items["item_paste"].Enabled = false;
                }
                else
                {
                    contextMenu_lv.Items["item_paste"].Enabled = true;
                }
                contextMenu_lv.Items["item_delete"].Enabled = false;
                contextMenu_lv.Items["item_rename"].Enabled = false;
                contextMenu_lv.Items["item_new"].Enabled = true;
                contextMenu_lv.Items["item_refresh"].Enabled = true;
                contextMenu_lv.Items["item_attr"].Enabled = false;
            }
            else
            {
                contextMenu_lv.Items["item_copy"].Enabled = true;
                contextMenu_lv.Items["item_cut"].Enabled = true;
                contextMenu_lv.Items["item_paste"].Enabled = false;
                contextMenu_lv.Items["item_delete"].Enabled = true;
                contextMenu_lv.Items["item_rename"].Enabled = true;
                contextMenu_lv.Items["item_new"].Enabled = false;
                contextMenu_lv.Items["item_refresh"].Enabled = false;
                contextMenu_lv.Items["item_attr"].Enabled = true;
            }
        }

        private void RinGO_contextMenu_lv2_Opening(object sender, CancelEventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)//右键位置是listview1的空白处
            {
                contextMenu_lv2.Items["item_refresh2"].Enabled = true;
                contextMenu_lv2.Items["item_open"].Enabled = false;
                contextMenu_lv2.Items["item_revert"].Enabled = false;
                contextMenu_lv2.Items["item_del"].Enabled = false;
                if (combo_url.Text.Equals("回收站"))
                    contextMenu_lv2.Items["item_empty"].Enabled = true;
                else contextMenu_lv2.Items["item_empty"].Enabled = false;
            }
            else
            {
                contextMenu_lv2.Items["item_empty"].Enabled = false;
                contextMenu_lv2.Items["item_refresh2"].Enabled = false;
                if (combo_url.Text.Equals("回收站"))
                {
                    contextMenu_lv2.Items["item_open"].Enabled = false;
                    contextMenu_lv2.Items["item_revert"].Enabled = true;
                    contextMenu_lv2.Items["item_del"].Enabled = true;
                }
                else
                {
                    contextMenu_lv2.Items["item_open"].Enabled = true;
                    contextMenu_lv2.Items["item_revert"].Enabled = false;
                    contextMenu_lv2.Items["item_del"].Enabled = false;
                }
            }
        }

        private void RinGO_contextMenu_item_Click(object sender, EventArgs e)
        {
            ToolStripItem tsi = (ToolStripItem)sender;
            switch (tsi.Name)
            {
                case "item_copy": RinGO_docopy(); break;
                case "item_cut": RinGO_docut(); break;
                case "item_paste": RinGO_dopaste(); break;
                case "item_delete": RinGO_dodelete(); break;
                case "item_rename": RinGO_dorename(); break;
                case "item_refresh": RinGO_combo_url_SelectedIndexChanged(null, null); break;
                case "item_newfolder": RinGO_donew("folder"); break;
                case "item_word": RinGO_donew("word"); break;
                case "item_newtxt": RinGO_donew("txt"); break;
                case "item_newexcel": RinGO_donew("excel"); break;
                case "item_ppt": RinGO_donew("ppt"); break;
                case "item_open": RinGO_OpenObj(listView1.SelectedItems[0]); break;
                case "item_del": RinGO_doRecycleDel(); break;
                case "item_revert": RinGO_doRevert(); break;
                case "item_empty": RinGO_doEmpty(); break;
                case "item_refresh2": RinGO_combo_url_SelectedIndexChanged(null, null); break;
                case "文件夹ToolStripMenuItem": RinGO_donew("folder"); break;
                case "文本文档ToolStripMenuItem": RinGO_donew("txt"); break;
                case "word文档ToolStripMenuItem": RinGO_donew("word"); break;
                case "execel电子表格ToolStripMenuItem": RinGO_donew("excel"); break;
                case "pPT演示文稿ToolStripMenuItem": RinGO_donew("ppt"); break;
                case "item_attr": RinGO_showattr(listView1.SelectedItems[0].Tag.ToString()); break;
            }
        }

        private void RinGO_doEmpty()
        {
            try
            {
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    string fullname = listView1.Items[i].Tag.ToString();
                    if (File.Exists(fullname))
                    {
                        //FileSystem.DeleteFile 引用Microsoft.VisualBasic.DLLousing Microsoft.VisualBasic.FileIo;
                        FileSystem.DeleteFile(fullname, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
                    }
                    else
                    {
                        if (Directory.Exists(fullname))
                        {
                            FileSystem.DeleteDirectory(fullname, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
                        }
                        else
                        {
                            MessageBox.Show(fullname + ", 删除失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
            //刷新listview1
            RinGO_combo_url_SelectedIndexChanged(null, null);
        }

        private void RinGO_doRevert()
        {
            try
            {
                Shell shell = new Shell();
                Folder recycleBin = shell.NameSpace(10);
                if (listView1.SelectedItems.Count == 0) return;
                for (int i = 0; i < listView1.SelectedItems.Count; i++)
                {
                    string fullname = listView1.SelectedItems[i].Tag.ToString();
                    // 检查文件是否存在于回收站中
                    if (File.Exists(fullname) || Directory.Exists(fullname))
                    {
                        foreach (FolderItem2 recfile in recycleBin.Items())//遍历回收站中每一项文件
                        {
                            Console.WriteLine(recfile.Path);
                            if (recfile.Path == fullname)
                            {
                                recfile.Verbs().Item(0).DoIt();
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("文件不存在于回收站中！");
                    }
                    Console.WriteLine(fullname);
                }
            }
            catch (Exception ee) { MessageBox.Show(ee.Message); }
            RinGO_combo_url_SelectedIndexChanged(null, null);
        }

        private void RinGO_doRecycleDel()
        {
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }
            try
            {
                for (int i = 0; i < listView1.SelectedItems.Count; i++)
                {
                    string fullname = listView1.SelectedItems[i].Tag.ToString();
                    if (File.Exists(fullname))
                    {
                        //FileSystem.DeleteFile 引用Microsoft.VisualBasic.DLL，using Microsoft.isualBasic.FileIO;

                        FileSystem.DeleteFile(fullname, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
                    }
                    else
                    {
                        if (Directory.Exists(fullname))
                        {
                            FileSystem.DeleteDirectory(fullname, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
                        }
                        else
                        {
                            MessageBox.Show(fullname + ",删除失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); ;
                        }
                    }
                }
            }
            catch (Exception ee) { MessageBox.Show(ee.Message); }
            RinGO_combo_url_SelectedIndexChanged(null, null);
        }

        private void RinGO_OpenObj(ListViewItem listViewItem)
        {
            ListViewItem fitem = listView1.FocusedItem;//获取被双击的对象
            string fullname = fitem.Tag.ToString();
            string urltext = combo_url.Text;//被双击的对象的父目录路径
            string mytype = fitem.SubItems[1].Text;//记得listview1的第二列统一的类型列
            if (urltext.Equals("此电脑"))//父目录路径是此电脑
            {
                DriveInfo dinfo = new DriveInfo(fullname);
                if (dinfo.IsReady) mytype = "文件夹";//如果驱动器就绪，则作为文件夹处理
                else
                {
                    MessageBox.Show("设备未就绪，无法读取", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
            }
            if (urltext.Equals("回收站"))//父目录路径是回收站，则对象有待处理，目前信息不足。
            {
                MessageBox.Show("回收站的对象不能直接访问！");
                return;
            }
            if (urltext.Equals("桌面") && fitem.SubItems[0].Text.Equals("网络"))
            {
                MessageBox.Show("网上邻居功能未实现");
                return;
            }
            if (urltext.Equals("桌面") && fitem.SubItems[0].Text.Equals("文档")) mytype = "文件夹";
            switch (mytype)
            {
                case "文件夹"://如果是文件夹，则打开该文件夹，并更新combo_url地址栏控件的数据
                    {
                        if (accesspaths.IndexOf(fullname) > -1) accesspaths.Remove(fullname);
                        accesspaths.Insert(0, fullname);
                        RinGO_GetFolderListview(fullname);
                        combo_url.SelectedIndexChanged -= new EventHandler(RinGO_combo_url_SelectedIndexChanged);
                        combo_url.DataSource = null;
                        combo_url.DataSource = accesspaths;
                        combo_url.SelectedIndex = 0;
                        combo_url.SelectedIndexChanged += new EventHandler(RinGO_combo_url_SelectedIndexChanged);
                        break;
                    }
                case ""://如果没有类型，则处理此电脑，回收站。并更新combo_url地址栏控件的数据
                    {
                        if (fitem.SubItems[0].Text.Equals("此电脑"))
                        {
                            if (accesspaths.IndexOf("此电脑") > -1) accesspaths.Remove("此电脑");
                            accesspaths.Insert(0, "此电脑");
                            RinGO_GetDesktopListview();
                            combo_url.SelectedIndexChanged -= new EventHandler(RinGO_combo_url_SelectedIndexChanged);
                            combo_url.DataSource = null;
                            combo_url.DataSource = accesspaths;
                            combo_url.SelectedIndex = 0;
                            combo_url.SelectedIndexChanged += new EventHandler(RinGO_combo_url_SelectedIndexChanged);
                        }
                        break;
                    }
                default://尝试打开或运行文件
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(fullname);
                        }
                        catch { MessageBox.Show("无法打开或运行文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); }
                        break;
                    }
            }
        }

        private void RinGO_donew(string v)
        {
            string newname = "";
            string newext = "";
            switch (v)
            {
                case "folder": newname = "新建文件夹"; break;
                case "word": newname = "新建word文档"; newext = ".doc"; break;
                case "txt": newname = "新建文本文档"; newext = ".txt"; break;
                case "excel": newname = "新建excel文档"; newext = ".xls"; break;
                case "ppt": newname = "新建演示文稿"; newext = ".ppt"; break;
            }
            try
            {
                string _path = "";
                if (combo_url.Text == "桌面")
                {
                    _path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                }
                else
                {
                    _path = combo_url.Text;
                }
                if (v.Equals("folder"))
                {
                    int i = 1;
                    string temp = newname;
                    while (Directory.Exists(Path.Combine(_path, newname))) newname = temp + i++.ToString();
                    Directory.CreateDirectory(Path.Combine(_path, newname));
                    ListViewItem lv = new ListViewItem(newname);
                    lv.Tag = Path.Combine(_path, newext);
                    lv.ImageKey = "defaultfolder";
                    lv.IndentCount = 1;
                    lv.SubItems.Add("文件夹");
                    lv.SubItems.Add(DateTime.Now.ToString());
                    lv.SubItems.Add("");
                    lv.SubItems.Add(DateTime.Now.ToString());
                    listView1.Items.Add(lv);
                    listView1.Items[listView1.Items.Count - 1].Selected = true;
                }
                else
                {
                    int i = 1;
                    string temp = newname;
                    newname += newext;//加上扩展名
                    while (File.Exists(Path.Combine(_path, newname))) newname = temp + i++.ToString() + newext;
                    File.Create(Path.Combine(_path, newname));
                    ListViewItem lv = new ListViewItem(newname);
                    lv.Tag = Path.Combine(combo_url.Text, newname);
                    lv.ImageKey = RinGO_GetFileIconKey(newext, Path.Combine(combo_url.Text, newname));
                    lv.IndentCount = 1;
                    string typename = geticon.GetTypeName(Path.Combine(combo_url.Text, newname));
                    lv.SubItems.Add(typename);
                    lv.SubItems.Add(DateTime.Now.ToString());
                    lv.SubItems.Add("");
                    lv.SubItems.Add(DateTime.Now.ToString());
                    listView1.Items.Add(lv);
                    listView1.Items[listView1.Items.Count - 1].Selected = true;
                }
            }
            catch (Exception ee) { MessageBox.Show(ee.Message); }

        }

        private void RinGO_showattr(string fullname)
        {
            FileInfo finfo = new FileInfo(fullname);
            try
            {
                long size = finfo.Length;
                string sizestring = "";
                if (size < 1024)
                {
                    sizestring = size.ToString() + " Byte";
                }
                else if (size >= 1024 && size < 1024 * 1024)
                {
                    sizestring = (size / 1024).ToString() + " KB";
                }
                else if (size >= 1024 * 1024 && size < 1024 * 1024 * 1024)
                {
                    sizestring = ((size / 1024) / 1024).ToString() + " MB";
                }
                else
                {
                    sizestring = (((size / 1024) / 1024) / 1024).ToString() + " GB";
                }
                MessageBox.Show("名称:" + finfo.Name + "\n" + "修改时间:" + finfo.LastWriteTime.ToString() + "\n" +
                                "类型:" + geticon.GetTypeName(finfo.FullName) + "\n" + "文件大小:" + sizestring, "属性", MessageBoxButtons.YesNo);
            }
            catch
            {
                MessageBox.Show("名称:" + fullname + "\n" + "类型:" + geticon.GetTypeName(finfo.FullName) + "\n", "属性", MessageBoxButtons.YesNo);
                return;
            }
        }

        private void RinGO_dorename()
        {
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }
            string oldname = listView1.SelectedItems[0].SubItems[0].Text;
            if (oldname.Equals("此电脑") || oldname.Equals("网络") || oldname.Equals("回收站") || oldname.Equals("文档"))
            {
                return;
            }
            listView1.LabelEdit = true;
            listView1.SelectedItems[0].BeginEdit();

        }

        private void RinGO_dodelete()
        {
            try
            {
                if (listView1.SelectedItems.Count == 0)
                {
                    return;
                }
                for (int i = 0; i < listView1.SelectedItems.Count; i++)
                {
                    string fullname = listView1.SelectedItems[i].Tag.ToString();

                    if (File.Exists(fullname))
                    {
                        //FileSystem. DeleteFile 引用Microsoft.VisualBasic.DLL using Microsoft.VisualBasic.FileIo;
                        FileSystem.DeleteFile(fullname, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    }
                    else
                    {
                        if (Directory.Exists(fullname))
                        {
                            FileSystem.DeleteDirectory(fullname, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                        }
                        else
                        {
                            MessageBox.Show(fullname + ", 删除失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }
                    }
                }
            }
            catch (Exception ee) { MessageBox.Show(ee.Message); }
            RinGO_combo_url_SelectedIndexChanged(null, null);
        }

        private void RinGO_dopaste()
        {
            string currpath = combo_url.Text;
            if (currpath.Equals("桌面"))
            {
                currpath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            //如果复制对象数为0。或自录不存在，或源目录和目的目录相同，则运回
            if (copyobjs.Count == 0 || !Directory.Exists(currpath) || currpath.Equals(Directory.GetParent(copyobjs[0].ToString()).Name))
            {
                return;
            }
            for (int i = 0; i < copyobjs.Count; i++)
            {
                if (File.Exists(copyobjs[i].ToString()))
                {
                    //文件
                    RinGO_copycut_file(copyobjs[i].ToString(), currpath);
                }
                else if (Directory.Exists(copyobjs[i].ToString()))
                {
                    //目录
                    RinGO_copycut_directory(copyobjs[i].ToString(), currpath);
                }
            }
            if (iscut == true)
            {
                copyobjs.Clear();

            }
            RinGO_combo_url_SelectedIndexChanged(null, null);

        }

        private void RinGO_copycut_directory(string v, string currpath)
        {
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(v);
                string dname = directoryInfo.Name;
                string destfullpath = Path.Combine(currpath, dname);
                DialogResult result = DialogResult.Yes;
                if (Directory.Exists(destfullpath))
                {
                    result = MessageBox.Show("目录“" + dname + "”已经存在，是否覆盖。", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        Directory.Delete(destfullpath, true);
                    }
                    else
                    {
                        return;
                    }
                }
                DirectoryInfo dinfo = new DirectoryInfo(destfullpath);
                dinfo.Create();
                FileInfo[] files = directoryInfo.GetFiles();
                foreach (FileInfo file in files)
                {
                    file.CopyTo(Path.Combine(destfullpath, file.Name), true);
                }
                DirectoryInfo[] dirs = directoryInfo.GetDirectories();
                foreach (DirectoryInfo dir in dirs)
                {
                    RinGO_copycut_directory(dir.FullName, destfullpath);
                }
                if (iscut == true)
                {
                    directoryInfo.Delete(true);
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void RinGO_docut()
        {

            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }
            iscut = true;
            copyobjs.Clear();
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                copyobjs.Add(listView1.SelectedItems[i].Tag.ToString());
            }
        }

        private void RinGO_docopy()
        {
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }
            iscut = false;
            copyobjs.Clear();
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                copyobjs.Add(listView1.SelectedItems[i].Tag.ToString());

            }
        }

        private void RinGO_copycut_file(string fullname, string destpath)//复制或剪切文件
        {
            try
            {
                FileInfo finfo = new FileInfo(fullname);
                string filename = finfo.Name;
                string currpath = destpath;
                string destfullname = Path.Combine(currpath, filename);
                DialogResult result = DialogResult.Yes;
                if (File.Exists(destfullname))
                    result = MessageBox.Show("文件“" + filename + "”已经存在，是否覆盖。", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    finfo.CopyTo(destfullname, true);
                    if (iscut == true) File.Delete(fullname);
                }
            }
            catch (Exception ee) { MessageBox.Show(ee.Message); }
        }

        private void RinGO_docutToolStrip_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                RinGO_docut();
            }
            else
            {
                MessageBox.Show("您应该先选中一项之后再进行剪切操作。", "提示！",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RinGO_showattrToolStrip_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                RinGO_showattr(listView1.SelectedItems[0].Tag.ToString());
            }
            else
            {
                MessageBox.Show("您应该先选中一项之后再选择查看属性。", "提示！",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RinGO_docopyToolStrip_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                RinGO_docopy();
            }
            else
            {
                MessageBox.Show("您应该先选中一项之后再进行复制操作。", "提示！",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RinGO_dopasteToolStrip_Click(object sender, EventArgs e)
        {
            RinGO_dopaste();
            //这个不用选中，直接目录里粘贴就好，当然有些默认目录不生效
        }

        private void RinGO_dodeleteToolStrip_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                RinGO_dodelete();
            }
            else
            {
                MessageBox.Show("您应该先选中一项之后再进行删除操作。", "提示！",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RinGO_dorenameToolStrip_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                RinGO_dorename();
            }
            else
            {
                MessageBox.Show("您应该先选中一项之后再尝试重命名。", "提示！",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RinGO_checkverToolStrip_Click(object sender, EventArgs e)
        {
            MessageBox.Show("大林檎的文件资源管理器 v1.0.0 alpha", "关于", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RinGO_purchaseToolStrip_Click(object sender, EventArgs e)
        {
            string multiLineText = "本产品授权给：\n用户名：林檎_大林檎\n激活码：1145141919810\n激活类型：开发版本，永久许可！";
            MessageBox.Show(multiLineText, "注册信息！", MessageBoxButtons.OK, MessageBoxIcon.Question);
        }

        private void RinGO_licToolStrip_Click(object sender, EventArgs e)
        {
            string multiLineText = "本软件为早期开发版本，实际显示效果可能与正式版本不同！\n版权所限，翻录必究！\nCopyright ©  2023 大林檎.All rights reserved.";
            MessageBox.Show(multiLineText, "作者声明！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void RinGO_toolStripSplitButton1_Click(object sender, EventArgs e)
        {
            RinGO_donew("folder");
        }

        private void RinGO_listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {//选中listview1，添加事件AfterLabelEdit，并编写代码，参考如下： 到31页
            if (e.Label == null || e.Label.Trim() == "" || e.Label.Trim().Equals(listView1.Items[e.Item].SubItems[0].Text.Trim()))
                e.CancelEdit = true;
            else
            {
                string _path = "";
                if (combo_url.Text == "桌面")
                {
                    _path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                }
                else
                {
                    _path = combo_url.Text;
                }

                string newname = e.Label.Trim();
                try
                {
                    //根据名称判断是文件还是文件夹
                    var ext = Path.GetExtension(newname).Trim();
                    if (!string.IsNullOrEmpty(ext))
                    {
                        if (File.Exists(Path.Combine(_path, newname)))
                        {
                            MessageBox.Show("文件名已经存在，请重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.CancelEdit = true;
                        }
                        else
                        {
                            File.Move(listView1.Items[e.Item].Tag.ToString(), Path.Combine(_path, newname));
                            listView1.Items[e.Item].Tag = Path.Combine(_path, newname);
                        }
                    }
                    else
                    {


                        if (Directory.Exists(Path.Combine(_path, newname)))
                        {
                            MessageBox.Show("文件夹已经存在，请重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.CancelEdit = true;
                        }
                        else
                        {
                            Directory.Move(listView1.Items[e.Item].Tag.ToString(), Path.Combine(_path, newname));
                            listView1.Items[e.Item].Tag = Path.Combine(_path, newname);
                        }
                    }
                }
                catch { }
            }
            listView1.LabelEdit = false;
        }

        private void RinGO_listView1_MouseEnter(object sender, EventArgs e)
        {
            string folderpath = combo_url.Text;
            if (folderpath.Equals("回收站") || folderpath.Equals("主文件夹") || folderpath.Equals("此电脑"))
            {
                listView1.ContextMenuStrip = contextMenu_lv2;
            }
            else
            {
                listView1.ContextMenuStrip = contextMenu_lv;
            }
        }

        private void RinGO_btn_search_Click(object sender, EventArgs e)
        {
            if (text_search.Text.Trim().Equals(""))
            {
                RinGO_combo_url_SelectedIndexChanged(null, null);
                return;
            }
            if (combo_url.Text.Equals("回收站"))
            {
                for (int i = listView1.Items.Count - 1; i >= 0; i--)
                {
                    string temp = listView1.Items[i].SubItems[0].Text.ToUpper();
                    if (temp.IndexOf(text_search.Text.Trim().ToUpper()) == -1) listView1.Items.RemoveAt(i);
                }
            }
            else
            {
                //text_search.Visible = true;
                lb_ojbnum.Text = "0";
                statusStrip1.Refresh();
                RinGO_CreateCol_R();
                string topfolder = combo_url.Text;
                if (topfolder.Equals("此电脑"))
                {
                    //这里有省略号.......
                    //DriveInfo[] drives = DriveInfo.GetDrives();
                    //foreach (DriveInfo drive in drives)
                    //   if (drive.IsReady) RinGO_doSearchFile(drive.Name, text_search.Text.Trim());
                    DialogResult result = MessageBox.Show("当前位置为此电脑，当前页面仅有驱动器。\n需要搜索请进入各驱动器。恕本页面无法提供文件查看方式。\n点击'是'返回上一级页面，点击'否'进入本页面搜索结果", "提示！", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        //无操作即可
                    }
                    else if (result == DialogResult.No)
                    {
                        DriveInfo[] drives = DriveInfo.GetDrives();
                        foreach (DriveInfo drive in drives)
                            if (drive.IsReady) RinGO_doSearchFile(drive.Name, text_search.Text.Trim());
                    }

                }
                else
                    if (topfolder.Equals("主文件夹"))
                {
                    RinGO_doSearchFile(Environment.GetFolderPath(Environment.SpecialFolder.MyComputer), text_search.Text.Trim());
                    RinGO_doSearchFile(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), text_search.Text.Trim());
                    RinGO_doSearchFile(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), text_search.Text.Trim());
                    RinGO_doSearchFile(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), text_search.Text.Trim());
                }
                else
                    listView1.Items.Clear();
                RinGO_doSearchFile(topfolder, text_search.Text.Trim());
            }
            lb_ojbnum.Text = listView1.Items.Count.ToString();
        }

        private void RinGO_doSearchFile(string topfolder, string content)
        {
            if (Directory.Exists(topfolder))
            {
                try
                {
                    string[] files = Directory.GetFiles(topfolder);
                    foreach (string f in files)
                    {
                        try
                        {
                            FileInfo finfo = new FileInfo(f);
                            if (finfo.Name.ToUpper().IndexOf(content.ToUpper()) != -1)
                            {
                                //名称
                                ListViewItem lv = new ListViewItem(finfo.Name);
                                lv.Tag = finfo.FullName;
                                lv.IndentCount = 1;
                                //根据扩展名提取图标
                                lv.ImageKey = RinGO_GetFileIconKey(finfo.Extension, finfo.FullName);
                                //获取文件类型名称
                                string typename = geticon.GetTypeName(finfo.FullName);
                                //类型
                                lv.SubItems.Add(typename);
                                //位置
                                lv.SubItems.Add(finfo.FullName);
                                //更改时间
                                lv.SubItems.Add(finfo.LastWriteTime.ToString());
                                listView1.Items.Add(lv);
                            }
                        }
                        catch
                        {
                            string[] dirs = Directory.GetDirectories(topfolder);
                            foreach (string d in dirs)
                            {
                                RinGO_doSearchFile(d, content);
                            }
                        }
                    }

                }
                catch
                {

                }
            }

        }

        private void Btn_largeIcon_Click(object sender, EventArgs e)
        {
            listView1.View = View.LargeIcon;
        }

        private void Btn_Details_Click(object sender, EventArgs e)
        {
            listView1.View = View.Details;
        }

        private void text_search_Enter(object sender, EventArgs e)
        {
            // 点击搜索框时，清空默认显示文字
            if (text_search.Text == "你所热爱的就是你的生活")
            {
                text_search.Text = "";
                text_search.ForeColor = SystemColors.WindowText;
            }
        }

        private void text_search_Leave(object sender, EventArgs e)
        {
            // 离开搜索框时，如果没有输入内容，则重新显示默认显示文字
            if (string.IsNullOrWhiteSpace(text_search.Text))
            {
                text_search.Text = "你所热爱的就是你的生活";
                text_search.ForeColor = SystemColors.GrayText;
            }
        }
    }
}