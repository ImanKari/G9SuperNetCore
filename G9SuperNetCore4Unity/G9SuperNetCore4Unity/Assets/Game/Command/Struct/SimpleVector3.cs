﻿using UnityEngine;

namespace G9SuperNetCoreServerSampleApp_GameServer.Commands.Struct
{
    public struct SimpleVector3
    {
        /// <summary>
        ///     Specified position x
        /// </summary>
        public float X;

        /// <summary>
        ///     Specified position y
        /// </summary>
        public float Y;

        /// <summary>
        ///     Specified position z
        /// </summary>
        public float Z;


        public Vector3 ConvertSimpleToMain()
        {
            return new Vector3(X, Y, Z);
        }

    }
}