
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Http;
using Microsoft.AspNetCore.SignalR;
using NotifyUser.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Security.Claims;
using System.Text;

namespace NotifyUser.Hubs
{
    public class NotifyHub : Hub
    {

        public override Task OnConnectedAsync()
        { 
            Clients.Caller.SendAsync("Connected");
            ConnectionUser.conn = Context.ConnectionId;
            return base.OnConnectedAsync();
        }
      
    }
}
