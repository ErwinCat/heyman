using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NLog;

namespace Heyman
{
    public class UserInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class HeymanTalk
    {
        private readonly UserInfo _user;
        private readonly IHeymanMouth _mouth;

        readonly Process _proc = new Process();
        private string _endLine;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();


        public HeymanTalk(UserInfo user, IHeymanMouth mouth)
        {
            _user = user;
            _mouth = mouth;

        }

        public UserInfo User { get { return _user; } }

        public void Run(HeymanCommand command,string message)
        {
            _endLine = command.EndLine;
            _proc.StartInfo.FileName = command.FileName;
            _proc.StartInfo.UseShellExecute = false;
            _proc.StartInfo.CreateNoWindow = true;
            if (!string.IsNullOrWhiteSpace(command.UserName)) _proc.StartInfo.UserName = command.UserName;

            _proc.StartInfo.Arguments = FillArgs(command,message);
            _proc.StartInfo.RedirectStandardOutput = true;
            _proc.StartInfo.RedirectStandardInput = true;
            _proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _proc.StartInfo.WorkingDirectory = !string.IsNullOrWhiteSpace(command.WorkingDirectory) ? command.WorkingDirectory : AppDomain.CurrentDomain.BaseDirectory;
            
            _proc.OutputDataReceived += OnOutputStreamRecived;

            _proc.Start();

            _proc.BeginOutputReadLine();
            _proc.StandardInput.AutoFlush = true;
        }

        private string FillArgs(HeymanCommand command,string message)
        {
            return command.Arguments.
                Replace("$MESSAGE", message).
                Replace("$USER_ID", _user.Id).
                Replace("$USER_NAME", _user.Name);
        }

        public void WaitExit()
        {
            _proc.WaitForExit();
            _proc.Close();
            _proc.Dispose();
        }

        private void OnOutputStreamRecived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Data)) return;
            _mouth.Say(_user.Id, FromatFromStdout(e.Data));
        }

        private string FromatFromStdout(string data)
        {
            return !string.IsNullOrWhiteSpace(_endLine) ? data.Replace(_endLine, "\n") : data;
        }

        private string FromatFromStdin(string data)
        {
            return !string.IsNullOrWhiteSpace(_endLine) ? data.Replace("\n", _endLine) : data;
        }

        public void OnMessage(string body)
        {
            _proc.StandardInput.WriteLine(FromatFromStdin(body));
        }

        
    }
}