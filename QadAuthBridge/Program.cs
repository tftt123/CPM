using System;
using System.Collections.Generic;
using System.IO;
using Progress.Open4GL.Proxy;
using OEAppServer;

namespace QadAuthBridge
{
    class Program
    {
        /// <summary>
        /// QAD 登录验证桥接程序
        /// 参数通过 stdin 逐行传入（避免命令行转义问题）：
        ///   line1: user
        ///   line2: pwd
        ///   line3: domain
        ///   line4: url
        ///   line5: name
        ///   line6: proc
        /// 输出 JSON: { "success": true/false, "message": "..." }
        /// </summary>
        static int Main(string[] args)
        {
            string user, pwd, domain, url, name, proc;

            // 如果传入 --stdin 参数，从标准输入读取参数
            if (args.Length > 0 && args[0].Equals("--stdin", StringComparison.OrdinalIgnoreCase))
            {
                var lines = new List<string>();
                for (int i = 0; i < 6; i++)
                {
                    string line = Console.ReadLine() ?? string.Empty;
                    lines.Add(line);
                }
                user = lines[0];
                pwd = lines[1];
                domain = lines[2];
                url = lines[3];
                name = lines[4];
                proc = lines[5];
            }
            else
            {
                // 兼容旧命令行方式
                user = GetArg(args, "--user");
                pwd = GetArg(args, "--pwd");
                domain = GetArg(args, "--domain");
                url = GetArg(args, "--url");
                name = GetArg(args, "--name");
                proc = GetArg(args, "--proc");
            }

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pwd) ||
                string.IsNullOrEmpty(url) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(proc))
            {
                Console.WriteLine(@"{ ""success"": false, ""message"": ""参数不全"" }");
                return 1;
            }

            try
            {
                var parms = new ParamArray(5);
                parms.AddCharacter(0, user, ParamArrayMode.INPUT);
                parms.AddCharacter(1, pwd, ParamArrayMode.INPUT);
                parms.AddCharacter(2, domain ?? string.Empty, ParamArrayMode.INPUT);
                parms.AddCharacter(3, string.Empty, ParamArrayMode.OUTPUT);
                parms.AddLogical(4, false, ParamArrayMode.OUTPUT);

                // 直接传入 URL 和 Name，不依赖 ConfigurationManager
                OeAppsvHelper.ExecProc(url, name, proc, parms);

                string outmsg = (string)parms.GetOutputParameter(3);
                bool ologin = (bool)parms.GetOutputParameter(4);
                parms.Clear();

                string jsonMsg = EscapeJson(outmsg);
                Console.WriteLine($"{{ \"success\": {ologin.ToString().ToLower()}, \"message\": \"{jsonMsg}\" }}");
                return ologin ? 0 : 2;
            }
            catch (Exception ex)
            {
                string jsonMsg = EscapeJson(ex.Message);
                Console.WriteLine($"{{ \"success\": false, \"message\": \"{jsonMsg}\" }}");
                return 3;
            }
        }

        static string GetArg(string[] args, string key)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals(key, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }
            return null;
        }

        static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\r", "\\r")
                    .Replace("\n", "\\n")
                    .Replace("\t", "\\t");
        }
    }
}
