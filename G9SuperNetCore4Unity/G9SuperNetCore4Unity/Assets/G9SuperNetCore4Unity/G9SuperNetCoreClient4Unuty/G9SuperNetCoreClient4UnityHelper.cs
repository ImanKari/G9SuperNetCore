using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace G9SuperNetCoreClient.AbstractClient
{
    [Serializable]
    public class G9SuperNetCoreClient4UnityHelper : MonoBehaviour
    {
        #region Start Methods

        /// <summary>
        ///     Handler send, receive and events in frame for client
        /// </summary>

        #region HandleSendReceiveAndEventsInFrame

        protected void HandleSendReceiveAndEventsInFrame()
        {
            while (ReceiveAction.Any())
                ReceiveAction.Dequeue()(true);

            while (EventsQueue.Any())
                EventsQueue.Dequeue()();
        }

        #endregion

        #endregion End Start Methods

        #region Fields And Properties

        /// <summary>
        ///     <para>Queue for save receive action</para>
        ///     <para>bool => If true wait action execute finish (Sync)</para>
        /// </summary>
        public static readonly Queue<Action<bool>> ReceiveAction = new Queue<Action<bool>>();

        /// <summary>
        ///     <para>Queue for save events action</para>
        /// </summary>
        public static readonly Queue<Action> EventsQueue = new Queue<Action>();

        #endregion End Fields And Properties
    }
}