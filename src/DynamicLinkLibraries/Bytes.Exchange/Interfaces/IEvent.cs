﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using Bytes.Exchange;

/// <summary>
/// This is the service contract of Publish Service
/// </summary>
[ServiceContract]
public interface IEvent
{
    /// <summary>
    /// Event handler
    /// </summary>
    /// <param name="data">Alert data</param>
    [OperationContract(IsOneWay = true)]
    void OnEvent(AlertData data);
}