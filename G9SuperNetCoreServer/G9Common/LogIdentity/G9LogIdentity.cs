namespace G9Common.LogIdentity
{
    public struct G9LogIdentity
    {
        /// <summary>
        /// Log Identity => On create new server
        /// </summary>
        public const string CREATE_SERVER = "CREATE_SERVER";
        /// <summary>
        /// Log Identity => On start server
        /// </summary>
        public const string START_SERVER = "START_SERVER";
        /// <summary>
        /// Log Identity => On create socket listener
        /// </summary>
        public const string CREATE_LISTENER = "CREATE_LISTENER";
        /// <summary>
        /// Log Identity => On bind socket listener
        /// </summary>
        public const string BIND_LISTENER = "BIND_LISTENER";
        /// <summary>
        /// Log Identity => On wait socket listener for connection
        /// </summary>
        public const string WAIT_LISTENER = "WAIT_LISTENER";
        /// <summary>
        /// Log Identity => On accepted Connection
        /// </summary>
        public const string ACCEPT_CONNECTION = "ACCEPT_CONNECTION";
        /// <summary>
        /// Log Identity => On send data in server
        /// </summary>
        public const string SERVER_SEND_DATA = "SERVER_SEND_DATA";
        /// <summary>
        /// Log Identity => On receive data in server
        /// </summary>
        public const string SERVER_RECEIVE_DATA = "SERVER_RECEIVE_DATA";
        /// <summary>
        /// Log Identity => On accept call back in server
        /// </summary>
        public const string SERVER_ACCEPT_CALLBACK = "SERVER_ACCEPT_CALLBACK";
        /// <summary>
        /// Log Command => On command running
        /// </summary>
        public const string SERVER_RUNNING_COMMAND = "SERVER_RUNNING_COMMAND";
        /// <summary>
        /// Log Identity => On create new account and session
        /// </summary>
        public const string CREATE_NEW_ACCOUNT = "CREATE_NEW_ACCOUNT";
        /// <summary>
        /// Log Identity => On user log USER_ID_ + 'SessionId'
        /// </summary>
        public const string USER_ID_ = "USER_ID_";
        /// <summary>
        /// Log Identity => On create new client
        /// </summary>
        public const string CREATE_CLIENT = "CREATE_CLIENT";
        /// <summary>
        /// Log Identity => On start client connection
        /// </summary>
        public const string START_CLIENT_CONNECTION = "START_CLIENT_CONNECTION";
        /// <summary>
        /// Log Identity => On client connected to server
        /// </summary>
        public const string CLIENT_CONNECTED = "CLIENT_CONNECTED";
        /// <summary>
        /// Log Identity => On client in step receive data from server
        /// </summary>
        public const string CLIENT_RECEIVE = "CLIENT_RECEIVE";
        /// <summary>
        /// Log Identity => On send data in client
        /// </summary>
        public const string CLIENT_SEND_DATA = "CLIENT_SEND_DATA";
        /// <summary>
        /// Log Identity => Generate Packet
        /// </summary>
        public const string GENERATE_PACKET = "GENERATE_PACKET";

    }
}
