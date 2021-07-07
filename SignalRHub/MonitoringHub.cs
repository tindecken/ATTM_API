using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ATTM_API.Models;
using Microsoft.AspNetCore.SignalR;


namespace ATTM_API.SignalRHub
{
    public class MonitoringHub : Hub
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        public async Task DevRunningInfo(DevRunRecord devRunRecord)
        {
            await Clients.All.SendAsync("DevRunningInfo", devRunRecord);
        }
        public async Task DevRunningFail(DevRunRecord devRunRecord)
        {
            await Clients.All.SendAsync("DevRunningFail", devRunRecord);
        }
    }
}
