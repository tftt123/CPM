using Progress.Open4GL.DynamicAPI;
using Progress.Open4GL.Proxy;

namespace CpmServer.Services;

/// <summary>
/// OeAppsvHelper — 调用 QAD AppServer 的 4GL 过程
/// </summary>
public static class OeAppsvHelper
{
    private static string? appsvurl;
    private static string? appsvname;

    public static void Initialize(string url, string name)
    {
        appsvurl = url;
        appsvname = name;
    }

    public static void ExecProc(string appsvurl, string appsvname, string procname, ParamArray parms)
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

    public static void ExecProc(string procname)
    {
        Connection conn = new Connection(appsvurl, "", "", "");
        conn.SessionModel = 1;  //session-free
        OpenAppObject openAO = new OpenAppObject(conn, appsvname);
        openAO.RunProc(procname, new ParamArray(0));
        openAO.Dispose();
        conn.ReleaseConnection();
    }
}
