using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VietphraseMixHTML
{
    public class WorkingThread
    {
        public event EventHandler<WorkingEventArg> DoWork;
        public event EventHandler<WorkingEventArg> WorkingProgress;
        public event EventHandler<WorkingEventArg> WorkCompleted;
        private Thread _thread = null;
        public object Extra { get; set; }
        public bool Background
        {
            get
            {
               return  _thread.IsBackground;
            }
            set
            {
                _thread.IsBackground = value; 
            }
        }

        public delegate void WorkDelegate(object sender, WorkingEventArg eventArg);
        public WorkingThread()
        {

            _thread = new Thread(new ThreadStart(DoInnerWork));
        }

        public WorkingThread(object extra)
        {
            this.Extra = extra;
            _thread = new Thread(new ThreadStart(DoInnerWork));
        }

        public void RunWorkAsync()
        {
            _thread.Start();
        }
        private void DoInnerWork()
        {
            if(DoWork!= null)
            {
                DoWork(this,new WorkingEventArg());
                //ProcessDelegate(DoWork, this, new WorkingEventArg());
            }
            if(WorkCompleted!= null)
            {
                ProcessDelegate(WorkCompleted, this, new WorkingEventArg());
            }
        }
        public void Stop()
        {
            if (_thread != null && _thread.IsAlive)
            {
                _thread.Abort();
            }
        }

        public void ReportProgress(int percentage,object state =null)
        {
            if(WorkingProgress!=null)
            {
                WorkingEventArg eventArg = new WorkingEventArg();
                eventArg.ProgressPercentage = percentage;
                eventArg.UserState = state;
                ProcessDelegate(WorkingProgress, this, eventArg);
            }
        }
        

        /*public static void fireAsyncEvent<T>(EventHandler<T> eventHandler, object sender, T eventArgs, AsyncCallback callback) where T : BaseEventArgs
        {
            eventHandler.BeginInvoke(sender, eventArgs, callback, eventArgs);
        }
        public static void fireEvent<T>(EventHandler<T> eventHandler, object sender, T eventArgs) where T : BaseEventArgs
        {
            ProcessDelegate(eventHandler, sender, eventArgs);
        }*/

        private static void ProcessDelegate(Delegate del, params object[] args)
        {
            Delegate temp = del;
            if (temp == null)
            {
                return;
            }

            Delegate[] delegates = temp.GetInvocationList();
            foreach (Delegate handler in delegates)
            {
                InvokeDelegate(handler, args);
            }
        }
        private static void InvokeDelegate(Delegate del, object[] args)
        {
            System.ComponentModel.ISynchronizeInvoke synchronizer;
            synchronizer = del.Target as System.ComponentModel.ISynchronizeInvoke;
            try
            {
                if (synchronizer != null) //A Windows Forms object
                {
                    if (synchronizer.InvokeRequired == false)
                    {
                        del.DynamicInvoke(args);
                        return;
                    }
                    try
                    {
                        synchronizer.Invoke(del, args);
                    }
                    catch
                    {

                    }
                }

                else //Not a Windows Forms object
                {
                    del.DynamicInvoke(args);
                }
            }
            catch (Exception exp)
            {
                    throw exp;
            }
        }

    }
    
    public class WorkingEventArg : EventArgs
    {
        public int ProgressPercentage { get; set; }
        public object UserState { get; set; }
    }
}
