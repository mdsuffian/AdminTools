using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Data.Sql;
using System.Data.SqlClient;

namespace AdminTools
{
    public partial class Form1 : Form
    {

        public DataSet dsIIS = new DataSet();
        public DataTable dtIIS = new DataTable();
        public Form1()
        {
            InitializeComponent();
        }

        private void btnstart_Click(object sender, EventArgs e)
        {
            PrepareDataColumn();

            string serverlst = ConfigurationManager.AppSettings["ServerList"];
            List<string> lstserver = serverlst.Split(',').ToList<string>();
            foreach (string servervar in lstserver)
            {
                
                using (var mgr = ServerManager.OpenRemote(servervar))
                {
                    foreach (Site site in mgr.Sites.ToList())
                    {
                        
                        int siteport = site.Bindings.Where(x => x.Protocol == "http").Select(y => y.EndPoint.Port).FirstOrDefault();

                        var logfile = site.LogFile;
                        string sitename = site.Name;

                        foreach (Microsoft.Web.Administration.Application app in site.Applications.ToList())
                        {
                            string AppPath = app.Path.ToString();
                            string ApppoolName = app.ApplicationPoolName;
                            string RuntimeVer = mgr.ApplicationPools[ApppoolName].ManagedRuntimeVersion;
                            bool enable32bit = mgr.ApplicationPools[ApppoolName].Enable32BitAppOnWin64;
                            string managedpipeline = mgr.ApplicationPools[ApppoolName].ManagedPipelineMode.ToString();
                            string IdentityType = mgr.ApplicationPools[ApppoolName].ProcessModel.IdentityType.ToString();
                            string IdentityUser = mgr.ApplicationPools[ApppoolName].ProcessModel.UserName.ToString();

                            DataRow drIIS = dtIIS.NewRow();
                            drIIS["servername"] = servervar;
                            drIIS["sitename"] = sitename;
                            drIIS["port"] = siteport;
                            drIIS["apppath"] = AppPath;
                            drIIS["apppoolname"] = ApppoolName;
                            drIIS["RuntimeVer"] = RuntimeVer;
                            drIIS["enable32bit"] = enable32bit;
                            drIIS["managedpipelinemode"] = managedpipeline;
                            drIIS["IdentityType"] = IdentityType;
                            drIIS["IdentityUser"] = IdentityUser;
                            drIIS["PhysicalPath"] = app.VirtualDirectories[0].PhysicalPath;
                            drIIS["URLPath"] = @"http://" + servervar + @":" + siteport + AppPath;

                            
                            if (app.VirtualDirectories[0].PhysicalPath.Contains(':'))
                            {
                                string configpath = @"\\" + servervar + @"\" + app.VirtualDirectories[0].PhysicalPath.Split(':')[0] + @"$" + app.VirtualDirectories[0].PhysicalPath.Split(':')[1] + @"\web.config";
                                ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
                                configMap.ExeConfigFilename = configpath;
                                System.Configuration.Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

                                try
                                {


                                    if (config.ConnectionStrings.ConnectionStrings.Count > 0)
                                    {
                                        List<string> datas = new List<string>();

                                        foreach (ConnectionStringSettings con in config.ConnectionStrings.ConnectionStrings)
                                        {
                                            if (!(con.ConnectionString.Contains("Microsoft.Jet.OLEDB")))
                                            {


                                                var csb = new SqlConnectionStringBuilder(con.ConnectionString);


                                                if (!(csb.DataSource == @".\SQLEXPRESS" || csb.DataSource == ""))
                                                {
                                                    if (!(datas.Contains(csb.DataSource.ToLower())))
                                                    {



                                                        datas.Add(csb.DataSource.ToLower().Replace("data source=", ""));
                                                    }

                                                }
                                            }


                                        }


                                        drIIS["DataConnection"] = string.Join(",", datas.ToArray());

                                        dtIIS.Rows.Add(drIIS);
                                    }
                                }
                                catch (ConfigurationException ce)
                                {

                                }
                                catch (Exception ex)
                                {


                                }

                            }

                        }
                    }
                }

            }


            //dataGridView1.DataSource = dtIIS;

        }

        private void PrepareDataColumn()
        {
            DataColumn dcservername = new DataColumn("servername");
            DataColumn dcsitename = new DataColumn("sitename");
            DataColumn dcport = new DataColumn("port");
            DataColumn dcapppath = new DataColumn("apppath");
            DataColumn dcapppoolname = new DataColumn("apppoolname");
            DataColumn dcRuntimeVer = new DataColumn("RuntimeVer");
            DataColumn dcenable32bit = new DataColumn("enable32bit");
            DataColumn dcmanagedpipelinemode = new DataColumn("managedpipelinemode");
            DataColumn dcIdentityType = new DataColumn("IdentityType");
            DataColumn dcIdentityUser = new DataColumn("IdentityUser");
            DataColumn dcPhysicalPath = new DataColumn("PhysicalPath");
            DataColumn dcURLPath = new DataColumn("URLPath");
            DataColumn dcDataConnection = new DataColumn("DataConnection");

            dtIIS.Columns.Add(dcservername);
            dtIIS.Columns.Add(dcsitename);
            dtIIS.Columns.Add(dcport);
            dtIIS.Columns.Add(dcapppath);
            dtIIS.Columns.Add(dcapppoolname);
            dtIIS.Columns.Add(dcRuntimeVer);
            dtIIS.Columns.Add(dcenable32bit);
            dtIIS.Columns.Add(dcmanagedpipelinemode);
            dtIIS.Columns.Add(dcIdentityType);
            dtIIS.Columns.Add(dcIdentityUser);
            dtIIS.Columns.Add(dcPhysicalPath);
            dtIIS.Columns.Add(dcURLPath);
            dtIIS.Columns.Add(dcDataConnection);
        }
    }
}

