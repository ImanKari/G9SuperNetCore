namespace G9Common.LogIdentity
{
    public struct G9LogIdentity
    {
        /* ########################################## Server Identity ########################################## */

        #region Server Identity

        /// <summary>
        ///     Log Identity => On create new server
        /// </summary>
        public const string CREATE_SERVER = "CREATE_SERVER";

        /// <summary>
        ///     Log Identity => On start server
        /// </summary>
        public const string START_SERVER = "START_SERVER";

        /// <summary>
        ///     Log Identity => On stop server
        /// </summary>
        public const string STOP_SERVER = "STOP_SERVER";

        /// <summary>
        ///     Log Identity => On create socket listener
        /// </summary>
        public const string CREATE_LISTENER = "CREATE_LISTENER";

        /// <summary>
        ///     Log Identity => On bind socket listener
        /// </summary>
        public const string BIND_LISTENER = "BIND_LISTENER";

        /// <summary>
        ///     Log Identity => On wait socket listener for connection
        /// </summary>
        public const string WAIT_LISTENER = "WAIT_LISTENER";

        /// <summary>
        ///     Log Identity => On accepted Connection
        /// </summary>
        public const string ACCEPT_CONNECTION = "ACCEPT_CONNECTION";

        /// <summary>
        ///     Log Identity => On send data in server
        /// </summary>
        public const string SERVER_SEND_DATA = "SERVER_SEND_DATA";

        /// <summary>
        ///     Log Identity => On send data in server for all clients
        /// </summary>
        public const string SERVER_SEND_DATA_ALL_CLIENTS = "SERVER_SEND_DATA_ALL_CLIENTS";

        /// <summary>
        ///     Log Identity => On receive data in server
        /// </summary>
        public const string SERVER_RECEIVE_DATA = "SERVER_RECEIVE_DATA";

        /// <summary>
        ///     Log Identity => On accept call back in server
        /// </summary>
        public const string SERVER_ACCEPT_CALLBACK = "SERVER_ACCEPT_CALLBACK";

        /// <summary>
        ///     Log Identity => On enable test mode command for all client
        /// </summary>
        public const string ENABLE_TEST_MODE_ALL_CLIENT = "ENABLE_TEST_MODE_ALL_CLIENT";

        /// <summary>
        ///     Log Identity => On disable test mode command for all client
        /// </summary>
        public const string DISABLE_TEST_MODE_ALL_CLIENT = "DISABLE_TEST_MODE_ALL_CLIENT";

        /// <summary>
        ///     Log Identity => On enable test mode command for single client
        /// </summary>
        public const string ENABLE_TEST_MODE_SINGLE_CLIENT = "ENABLE_TEST_MODE_SINGLE_CLIENT";

        /// <summary>
        ///     Log Identity => On disable test mode command for single client
        /// </summary>
        public const string DISABLE_TEST_MODE_SINGLE_CLIENT = "DISABLE_TEST_MODE_SINGLE_CLIENT";

        /// <summary>
        ///     Log Identity => On session receive request over the limit in second
        /// </summary>
        public const string RECEIVE_REQUEST_OVER_THE_LIMIT = "RECEIVE_REQUEST_OVER_THE_LIMIT";

        #endregion

        /* ########################################## Command Identity ########################################## */

        #region Command Identity

        /// <summary>
        ///     Log Identity => On command added
        /// </summary>
        public const string ADD_COMMAND = "ADD_COMMAND";

        /// <summary>
        ///     Log Identity => On command running
        /// </summary>
        public const string RUNNING_COMMAND = "RUNNING_COMMAND";

        /// <summary>
        ///     Log Identity => On receive unhandled command
        /// </summary>
        public const string RECEIVE_UNHANDLED_COMMAND = "RECEIVE_UNHANDLED_COMMAND";

        #endregion

        /* ########################################## Common Identity ########################################## */

        #region Common Identity

        /// <summary>
        ///     Log Identity => On create new account and session
        /// </summary>
        public const string CREATE_NEW_ACCOUNT = "CREATE_NEW_ACCOUNT";

        /// <summary>
        ///     Log Identity => On user log USER_ID_ + 'SessionId'
        /// </summary>
        public const string USER_ID_ = "USER_ID_";

        /// <summary>
        ///     Log Identity => Generate Packet
        /// </summary>
        public const string GENERATE_PACKET = "GENERATE_PACKET";

        /// <summary>
        ///     Log Identity => Client Ping
        /// </summary>
        public const string CLIENT_PING = "CLIENT_PING";

        /// <summary>
        ///     Log Identity => On test send and receive enable
        /// </summary>
        public const string TEST_SEND_RECEIVE = "TEST_SEND_RECEIVE";

        /// <summary>
        ///     Log Identity => On test send and receive enable
        /// </summary>
        public const string ECHO_COMMAND = "ECHO_COMMAND";

        /// <summary>
        ///     Log Identity => On test authorization fail
        /// </summary>
        public const string AUTHORIZATION_FAIL = "AUTHORIZATION_FAIL";

        /// <summary>
        ///     Log Identity => On test authorization success
        /// </summary>
        public const string AUTHORIZATION_SUCCESS = "AUTHORIZATION_SUCCESS";

        #endregion

        /* ########################################## Client Identity ########################################## */

        #region Client Identity

        /// <summary>
        ///     Log Identity => On create new client
        /// </summary>
        public const string CREATE_CLIENT = "CREATE_CLIENT";

        /// <summary>
        ///     Log Identity => On start client connection
        /// </summary>
        public const string START_CLIENT_CONNECTION = "START_CLIENT_CONNECTION";

        /// <summary>
        ///     Log Identity => On client connected to server
        /// </summary>
        public const string CLIENT_CONNECTED = "CLIENT_CONNECTED";

        /// <summary>
        ///     Log Identity => On client in step receive data from server
        /// </summary>
        public const string CLIENT_RECEIVE = "CLIENT_RECEIVE";

        /// <summary>
        ///     Log Identity => On send data in client
        /// </summary>
        public const string CLIENT_SEND_DATA = "CLIENT_SEND_DATA";

        #endregion
    }
}