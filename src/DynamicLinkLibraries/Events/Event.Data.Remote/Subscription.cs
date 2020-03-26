﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using BaseTypes;

using Event.Data.Remote.Interfaces;



    /// <summary>
    /// This is the class for Subscription Service that is deployed to listen.
    /// </summary>
[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
class Subscription : IRegistration
{


    static Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();

    static Dictionary<string,
        List<IEvent>> events = new Dictionary<string, List<IEvent>>();


    /// <summary>
    /// This is constructor of the class.It is used here to create the instance of the pub/sub data structure

    /// </summary>
    static Subscription()
    {
        dictionary = Event.Data.Remote.ServerNet.Dictionary;
    }

    /// <summary>
    /// This method return the complete subscriber list to publisher service.
    /// </summary>
    /// <param name="eventOperation"></param>
    /// <returns></returns>
    internal static List<IEvent> GetClients(string url)
    {
        lock (typeof(Subscription))
        {
            if (events.ContainsKey(url))
            {
                return events[url];
            }
            return null;
        }
    }

    /// <summary>
    /// This method is called by subscriber to register itself with Server.
    /// It register the client with pub/sub service.
    /// </summary>
    /// <param name="eventOperation"></param>
    public string[] Register()
    {
        lock (typeof(Subscription))
        {
            OperationContext ctx = OperationContext.Current;
          /*  using (System.IO.TextWriter wr = new System.IO.StreamWriter(@"G:\1.txt"))
            {
                PrintProperties("", ctx, wr, new List<object>());
            }*/
            IEvent subscriber = ctx.GetCallbackChannel<IEvent>();
            string url = ctx.EndpointDispatcher.EndpointAddress + "";
            List<IEvent> events;
            if (Subscription.events.ContainsKey(url))
            {
                events = Subscription.events[url];
            }
            else
            {
                events = new List<IEvent>();
                Subscription.events[url] = events;
            }
            if (events.Contains(subscriber))
            {
                return null;
            }
            events.Add(subscriber);
            return dictionary[url];
        }
    }

    /// <summary>
    /// This method is called by subscriber to Unsubscribe itself from  Server.
    /// It   Unsubscribe the client with pub/sub service.
    /// </summary>
    public void UnRegister()
    {
        lock (typeof(Subscription))
        {
            OperationContext ctx = OperationContext.Current;
            string url = ctx.EndpointDispatcher.EndpointAddress + "";
            IEvent subscriber = ctx.GetCallbackChannel<IEvent>();
            List<IEvent> l = events[url];
            if (l.Contains(subscriber))
            {
                l.Remove(subscriber);
            }
            if (l.Count == 0)
            {
                events.Remove(url);
            }
        }
    }
 
}