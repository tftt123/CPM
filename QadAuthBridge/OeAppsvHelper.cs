using System;
using System.Data;
using System.Collections.Generic;
using Progress.Open4GL.Proxy;
using Progress.Open4GL.DynamicAPI;

/// <summary>
/// OeAppsvHelper 的摘要说明
/// 调用APPSERVER的P程序 类似于调用SQL存储过程 传入执行程序名和 参数列表
/// 如果要传参数 调用程序要引用Progress.Open4GL.Proxy命名空间 来使用ParamArray parms
/// </summary>
namespace OEAppServer
{
    public class OeAppsvHelper
    {
        //Example: AppServerDC://xxx.xxx.x.x:xxxxx/asqad-qra
        private static string appsvurl = System.Configuration.ConfigurationManager.AppSettings["appsvurl"];
        private static string appsvname = System.Configuration.ConfigurationManager.AppSettings["appsvname"];
        static OeAppsvHelper()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }
        public static void ExecProc(string appsvurl,string appsvname,string procname,ParamArray parms)
        {
            Connection conn = new Connection(appsvurl, "", "", "");
            conn.SessionModel = 1;  //session-free
            OpenAppObject openAO = new OpenAppObject(conn, appsvname);
            openAO.RunProc(procname, parms);
            openAO.Dispose();
            conn.ReleaseConnection();
        }

        public static void ExecProc(string procname, ParamArray parms)
        {
            Connection conn = new Connection(appsvurl, "", "", "");
            conn.SessionModel = 1;  //session-free
            OpenAppObject openAO = new OpenAppObject(conn, appsvname);
            openAO.RunProc(procname, parms);
            openAO.Dispose();
            conn.ReleaseConnection();
        }
		// 默认的调用 APPSV

		public static void ExecProc(string procname)
        {
            Connection conn = new Connection(appsvurl, "", "", "");
            conn.SessionModel = 1;  //session-free
            OpenAppObject openAO = new OpenAppObject(conn, appsvname);
            openAO.RunProc(procname, new ParamArray(0));
            openAO.Dispose();
            conn.ReleaseConnection();
        }
		//默认的调用 APPSV 无传入参数
        //其它重载待补充
    }

}