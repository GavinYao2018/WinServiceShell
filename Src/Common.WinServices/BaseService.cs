using Common.WinServices.Common;
using System;
using System.ServiceProcess;
using System.Text;

namespace Common.WinServices
{
    public partial class BaseService : ServiceBase
    {
        public BaseService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 调试用
        /// </summary>
        public void Start()
        {
            this.OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                ServiceHelper.OnStartInit();
                ServiceHelper.StartJob();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "OnStart执行异常");
            }
        }

        protected override void OnStop()
        {
            try
            {
                ServiceHelper.StopJob();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "OnStop执行异常");
            }
        }
    }
}
