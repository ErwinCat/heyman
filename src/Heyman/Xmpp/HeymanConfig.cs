using System.Collections.Generic;

namespace Heyman
{

    public class HeymanLocalization
    {
        public HeymanLocalization()
        {
            HelpHeader = "";
        }
        public string HelpHeader { get; set; }
    }

    public class HeymanConfig
    {
        public HeymanConfig()
        {
            Xmpp = new HeymanXmppConfig
            {
                User = "user",
                Password = "password",
                Server = "server",
            };

            Commands = new[]
                       {
                           new HeymanCommand
                           {
                               FileName = " ",
                               Description = " ",
                               Regex = " ",
                               Title = " ",
                               EndLine = "|",
                               WorkingDirectory = "./"
                           }
                       };

            Localization = new HeymanLocalization();
        }
        public HeymanXmppConfig Xmpp { get; set; }

        public HeymanCommand[] Commands { get; set; }
        public HeymanLocalization Localization { get; set; }
    }

    

    public class HeymanXmppConfig
    {
        public HeymanXmppConfig()
        {
            User = "user";
            Password = "password";
            Server = "server";
        }

        public string Server { get; set; }

        public string Password { get; set; }

        public string User { get; set; }
    }
}