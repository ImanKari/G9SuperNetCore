using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace G9SuperNetCoreClient.AbstractClient
{
    public class G9SuperNetCoreClient4UnityHelper : MonoBehaviour
    {
        #region Start Methods

        /// <summary>
        ///     Handler send and receive in frame for client
        /// </summary>

        #region HandleSendReceiveInFrame

        protected void HandleSendReceiveInFrame()
        {
            while (ReceiveAction.Any())
                ReceiveAction.Dequeue()(true);

            while (SendAction.Any())
                SendAction.Dequeue()(true);
        }

        #endregion

        #endregion End Start Methods

        #region Start Fields And Properties

        /// <summary>
        ///     <para>Queue for save receive action</para>
        ///     <para>bool => If true wait action execute finish (Sync)</para>
        /// </summary>
        public static readonly Queue<Action<bool>> ReceiveAction = new Queue<Action<bool>>();

        /// <summary>
        ///     <para>Queue for save send action</para>
        ///     <para>bool => If true wait action execute finish (Sync)</para>
        /// </summary>
        public static readonly Queue<Action<bool>> SendAction = new Queue<Action<bool>>();

        #endregion End Fields And Properties
    }
}