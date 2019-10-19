using System.Linq;
using System.Reflection;
using G9SuperNetCoreClient.Helper;
using G9SuperNetCoreServer.Abstarct;

namespace G9SuperNetCoreServer.AbstractServer
{
    public abstract partial class AG9SuperNetCoreServerBase<TAccount, TSession>
        where TAccount : AServerAccount<TSession>, new()
        where TSession : AServerSession, new()
    {
        #region Methods

        /// <summary>
        ///     Get public properties from account
        ///     Properties can read and write and is value type
        /// </summary>
        /// <param name="sessionId">Specified session id - if set 0 get properties from new instance account</param>
        /// <returns>G9PropertiesInfo specified property name, property type and property value</returns>

        #region GetPublicPropertiesFromAccountBySessionId

        public G9PropertiesInfo[] GetPublicPropertiesFromAccountBySessionId(uint sessionId = 0)
        {
            // Set account
            var account = sessionId == 0 ? new TAccount() : _core.GetAccountUtilitiesBySessionId(sessionId)?.Account;

            // Get public properties and values
            return account?.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p =>
                    p.CanRead
                    &&
                    p.CanWrite
                    &&
                    (p.PropertyType.IsValueType || p.PropertyType == typeof(string))
                )
                .Select(s => new G9PropertiesInfo
                {
                    PropertyName = s.Name,
                    PropertyType = s.PropertyType,
                    PropertyValue = s.GetValue(account).ToString()
                }).ToArray();
        }

        #endregion

        /// <summary>
        ///     Get public properties from session
        ///     Properties can read and write and is value type
        /// </summary>
        /// <param name="sessionId">Specified session id - if set 0 get properties from new instance account</param>
        /// <returns>G9PropertiesInfo specified property name, property type and property value</returns>

        #region GetPublicPropertiesFromSessionBySessionId

        public G9PropertiesInfo[] GetPublicPropertiesFromSessionBySessionId(uint sessionId = 0)
        {
            // Set session
            var session = sessionId == 0
                ? new TSession()
                : _core.GetAccountUtilitiesBySessionId(sessionId)?.Account?.Session;

            // Get public properties and values
            return session?.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p =>
                    p.CanRead
                    &&
                    p.CanWrite
                    &&
                    (p.PropertyType.IsValueType || p.PropertyType == typeof(string))
                )
                .Select(s => new G9PropertiesInfo
                {
                    PropertyName = s.Name,
                    PropertyType = s.PropertyType,
                    PropertyValue = s.GetValue(session).ToString()
                })
                .ToArray();
        }

        #endregion

        #endregion
    }
}