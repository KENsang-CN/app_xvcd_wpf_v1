using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class ConsoleLog : StreamWriter
    {
        public ConsoleLog(string file) : base(file, true)
        {
        }

        public override Encoding Encoding => Encoding.UTF8;

        public string GetCallerInfo()
        {
#if true
            return $"[{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff}] ";
#else

            var callinfo = ClassHelper.GetMethodInfo(4);

            string str = $"[{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff}] ";
            if (callinfo != null)
            {
                str += "(";

                if (!string.IsNullOrEmpty(callinfo.FileName))
                {
                    str += $"{callinfo.FileName}, ";
                }
                else
                {
                    str += "unknown, ";
                }

                if (!string.IsNullOrEmpty(callinfo.MethodName))
                {
                    str += $"{callinfo.MethodName}, ";
                }
                else
                {
                    str += "unknown, ";
                }

                str += $"L.{callinfo.LineNumber})";
            }
            else
            {
                str += "(unknown, unknown, L.0)";
            }

            return str;
#endif
        }

        public override void Write(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            base.Write(GetCallerInfo() + value);
            base.Flush();
        }

        public override void WriteLine(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            base.WriteLine(GetCallerInfo() + value);
            base.Flush();
        }
    }

    public class ClassHelper
    {
        public class MethodInfo
        {
            public MethodBase Method { get; set; }
            public string ModuleName { get; set; }
            public string Namespace { get; set; }
            public string ClassName { get; set; }
            public string FullClassName { get; set; }
            public string MethodName { get; set; }
            public string CallChain { get; set; }
            public int LineNumber { get; set; }
            public string FileName { get; set; }
        }

        public static MethodInfo GetMethodInfo(int index)
        {
            try
            {
                index++;
                var stack = new StackTrace(true);
                var currentFrame = stack.GetFrame(index);
                var method = currentFrame.GetMethod();
                var module = method.Module;
                var declaringType = method.DeclaringType;
                var stackFrames = stack.GetFrames();
                string callChain = string.Join(" -> ", stackFrames.Select((r, i) =>
                {
                    if (i == 0)
                    {
                        return null;
                    }

                    var m = r.GetMethod();
                    return $"{m.DeclaringType.FullName}.{m.Name}";
                }).Where(r => !string.IsNullOrWhiteSpace(r)).Reverse());

                return new MethodInfo()
                {
                    Method = method,
                    ModuleName = module.Name,
                    Namespace = declaringType.Namespace,
                    ClassName = declaringType.Name,
                    FullClassName = declaringType.FullName,
                    MethodName = method.Name,
                    CallChain = callChain,
                    LineNumber = currentFrame.GetFileLineNumber(),
                    FileName = currentFrame.GetFileName(),
                };
            }
            catch
            {
                return null;
            }
        }
    }

}
