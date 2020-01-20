using System;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Xml;
using JetBrains.Annotations;
using S22.Xmpp;
using S22.Xmpp.Client;
using S22.Xmpp.Im;

namespace Heyman
{
    public class XmppHeyman : HeymanBase
    {
        private XmppClient _client;
        private HeymanLocalization _localization;
        private HeymanXmppConfig _xmpp;

        public XmppHeyman(HeymanConfig config):base(config.Commands)
        {
            _xmpp = config.Xmpp;
            _localization = config.Localization;
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            Logger.Error(e.Exception,"Error XMPP client: {0}",e.Reason);
            InternalStart();
        }

        private void OnClientMessage(object sender, MessageEventArgs e)
        {
            using (var wrt = new XmlTextWriter(Console.Out))
            {
                e.Message.Data.WriteContentTo(wrt);    
            }

            Logger.Trace("{0}:{1}", e.Jid, e.Message.Body);
            var user = GetUser(e.Jid);
            if (user == null) return;
            InternalOnMessage(user, GetMessage(e.Message));
        }

        private string GetMessage(Message message)
        {
            return message.Body;
        }

        private UserInfo GetUser(Jid jid)
        {
            var bare = jid.GetBareJid();
            var result = _client.GetRoster().FirstOrDefault(_ => _.Jid == bare);
            if (result == null)
            {
                Logger.Error($"Couldn't find user {jid} in Heyman friends. Try to clear XMPP server cache.");
                return null;
            }
            return new UserInfo
                   {
                       Id = jid.ToString(),
                       Name = result.Name,
                   };
        }

        protected override void InternalStart()
        {
            StopClient();

           _client = new XmppClient(_xmpp.Server, _xmpp.User, _xmpp.Password );
            
            _client.Message += OnClientMessage;
            _client.Error += OnError;
            _client.Connect();
            
        }

        

        private void StopClient()
        {
            if (_client == null) return;
            _client.Message -= OnClientMessage;
            _client.Error -= OnError;
            _client.Close();
        }

        protected override void InternalDisposeOnce()
        {
            StopClient();
        }

       
        protected override void InternalSendMessage(string user, string message)
        {
            if (_client!=null && _client.Connected)
            {
                _client.SendMessage(new Jid(user),message);
            }
        }
     
        protected override void PrintHelp(string user, string unknownMessgae, HeymanCommand[] avalableComamnds)
        {
            var sb = new StringBuilder();
            sb.AppendLine(_localization.HelpHeader);
            foreach (var heymanCommand in avalableComamnds)
            {
                sb.AppendLine(string.Format("'{0}' : {1}", heymanCommand.Title, heymanCommand.Description));
            }

            InternalSendMessage(user, sb.ToString());
        }

      

    }
}