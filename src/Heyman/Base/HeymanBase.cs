using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Heyman
{

    public abstract class HeymanBase : IHeymanMouth, IDisposable
    {
        protected readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly HeymanCommand[] _commands;
        private int _disposeCount;
        private readonly object _sync = new object();
        private readonly Dictionary<string, HeymanTalk> _talks = new Dictionary<string, HeymanTalk>();


        protected HeymanBase(HeymanCommand[] commands)
        {
            if (commands == null) throw new ArgumentNullException("commands");
            _commands = commands;
        }

        public void Start()
        {
            Logger.Trace("Start heyman");
            try
            {
                ThrowIfDisposed();
                InternalStart();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex,"Error to start app");
                throw ex;
            }
        }

        protected void InternalOnMessage(UserInfo user, string message)
        {
            if (IsDisposed) return;
            Logger.Trace("Message '{0}' -> Heyman:{1}",user,message);
            lock (_sync)
            {
                HeymanTalk talk;
                // try find talk with this user
                if (_talks.TryGetValue(user.Id, out talk))
                {
                    //already talks with this user -> send message ot talk
                    talk.OnMessage(message);
                }
                else
                {
                    // new talk -> try find command by message
                    var cmd = FindCommand(message);
                    if (cmd == null)
                    {
                        Logger.Warn("'{0}' -> Heyman: Unkwnon command '{1}'", user, message);
                        PrintHelp(user.Id, message,_commands);
                        return;
                    }
                    
                    talk = new HeymanTalk(user, this);
                    
                    _talks.Add(user.Id, talk);

                    try
                    {
                        Logger.Info("Begin talk '{0}' with user {1}", cmd.Title, user);
                        //run on current thread
                        talk.Run(cmd,message);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Error to begin talk {0} with", cmd.Title, user);
                        Say(talk.User.Id, ex.Message);
                    }
                    
                    //wait for exit on new thread
                    Task.Factory.StartNew(()=>RemoveOnExit(talk), TaskCreationOptions.LongRunning);
                }
            }
        }

        
       
        private void RemoveOnExit(HeymanTalk talk)
        {
            try
            {
                talk.WaitExit();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error to wait for exit talk with user {0}", talk.User);
                Say(talk.User.Id,ex.Message);
            }
            
            lock (_sync)
            {
                Logger.Info("COMPLETE {0}", talk);
                _talks.Remove(talk.User.Id);
            }
        }


       

        private HeymanCommand FindCommand(string message)
        {
            return _commands.FirstOrDefault(_ => Regex.IsMatch(message, _.Regex, RegexOptions.IgnoreCase));
        }

        public bool IsDisposed
        {
            get { return Thread.VolatileRead(ref _disposeCount) > 0; }
        }

        protected void ThrowIfDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
        }

       

     

        public void Say(string user, string message)
        {
            Logger.Trace("Message Heyman -> '{0}':{1}", user, message);
            try
            {
                InternalSendMessage(user, message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex,"Error to say user '{0}' message: {1}",user,message);
            }
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposeCount, 1, 0) == 0)
            {
                InternalDisposeOnce();
                GC.SuppressFinalize(this);
            }
        }

        protected abstract void InternalStart();

        protected abstract void InternalDisposeOnce();

        protected abstract void InternalSendMessage(string user, string message);

        protected abstract void PrintHelp(string user, string unknownMessgae, HeymanCommand[] avalableComamnds);

    }
}