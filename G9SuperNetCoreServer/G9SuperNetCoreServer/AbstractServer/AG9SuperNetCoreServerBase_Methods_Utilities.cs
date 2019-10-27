using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using G9Common.Resource;
using G9LogManagement.Enums;
using G9SuperNetCoreServer.Abstarct;
using G9SuperNetCoreServer.Enums;
using G9SuperNetCoreServer.HelperClass;
using G9SuperSocketNetCoreServer.Class.Struct;

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

        public async Task<G9PropertiesInfo[]> GetPublicPropertiesFromAccountBySessionId(uint sessionId = 0)
        {
            return await Task.Run(() =>
            {
                // Set account
                var account = sessionId == 0
                    ? new TAccount()
                    : _core.GetAccountUtilitiesBySessionId(sessionId)?.Account;

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
            });
        }

        #endregion

        /// <summary>
        ///     Get public properties from session
        ///     Properties can read and write and is value type
        /// </summary>
        /// <param name="sessionId">Specified session id - if set 0 get properties from new instance account</param>
        /// <returns>G9PropertiesInfo specified property name, property type and property value</returns>

        #region GetPublicPropertiesFromSessionBySessionId

        public async Task<G9PropertiesInfo[]> GetPublicPropertiesFromSessionBySessionId(uint sessionId = 0)
        {
            return await Task.Run(() =>
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
            });
        }

        #endregion

        /// <summary>
        ///     Get session with high send and receive
        /// </summary>
        /// <param name="takeCount">Take count for sessions info</param>
        /// <returns>Return report SessionReportInfo</returns>

        #region GetSessionWithHighSendAndReceive

        public async Task<G9SessionReport[]> GetSessionWithHighSendAndReceive(int takeCount)
        {
            return await Task.Run(() =>
            {
                try
                {
                    return _core.SelectAccountUtilities(s =>
                        s.Select(x => x.Account.Session).OrderBy(z => z.SessionTotalSendBytes)
                            .ThenBy(z => z.SessionTotalReceiveBytes).Take(takeCount)).Select(
                        s => new G9SessionReport
                        {
                            SessionId = s.SessionId,
                            TotalReceive = s.SessionTotalReceiveBytes,
                            TotalSend = s.SessionTotalReceiveBytes,
                            StartTime = s.SessionStartDateTime
                        }).ToArray();
                }
                catch (Exception ex)
                {
                    if (_core.Logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                        _core.Logging.LogException(ex);

                    return null;
                }
            });
        }

        #endregion

        /// <summary>
        ///     Get session with high time online
        /// </summary>
        /// <param name="takeCount">Take count for sessions info</param>
        /// <returns>Return report SessionReportInfo</returns>

        #region GetSessionWithHighTimeOnline

        public async Task<G9SessionReport[]> GetSessionWithHighTimeOnline(int takeCount)
        {
            return await Task.Run(() =>
            {
                try
                {
                    return _core.SelectAccountUtilities(s =>
                        s.Select(x => x.Account.Session).OrderBy(z => z.SessionStartDateTime).Take(takeCount)).Select(
                        s => new G9SessionReport
                        {
                            SessionId = s.SessionId,
                            TotalReceive = s.SessionTotalReceiveBytes,
                            TotalSend = s.SessionTotalReceiveBytes,
                            StartTime = s.SessionStartDateTime
                        }).ToArray();
                }
                catch (Exception ex)
                {
                    if (_core.Logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                        _core.Logging.LogException(ex);

                    return null;
                }
            });
        }

        #endregion

        /// <summary>
        /// </summary>
        /// <param name="takeCount">Take count for sessions info</param>
        /// <param name="propertyName">Property name for filter</param>
        /// <param name="value">Property value for filter</param>
        /// <param name="typeOfCompare">Type of compare for filter</param>
        /// <returns>Return report SessionReportInfo</returns>

        #region GetSessionReportInfoByAccountProperty

        public async Task<G9SessionReport[]> GetSessionReportInfoByAccountProperty(int takeCount,
            string propertyName, object value, TypeOfCompare typeOfCompare)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var oTGameAccount = new TAccount();

                    if (oTGameAccount.GetType().GetProperty(propertyName) == null)
                        throw new Exception($"Can't Find Property '{propertyName}' In Account...");

                    var PropertyType = oTGameAccount.GetType().GetProperty(propertyName).PropertyType;

                    return typeOfCompare switch
                    {
                        TypeOfCompare.Contain => _core
                            .SelectAccountUtilities(s => s.Where(g =>
                                g.Account.GetType()
                                    .GetProperty(propertyName)
                                    ?.GetValue(g.Account, null)
                                    ?.ToString()
                                    .Contains(value.ToString()) ?? false))
                            .Take(takeCount)
                            .Select(s => new G9SessionReport
                            {
                                SessionId = s.Account.Session.SessionId,
                                TotalReceive = s.Account.Session.SessionTotalReceiveBytes,
                                TotalSend = s.Account.Session.SessionTotalReceiveBytes,
                                StartTime = s.Account.Session.SessionStartDateTime
                            })
                            .ToArray(),
                        TypeOfCompare.Equal => _core
                            .SelectAccountUtilities(s => s.Where(g =>
                                g.Account.GetType().GetProperty(propertyName)?.GetValue(g.Account, null) == value))
                            .Take(takeCount)
                            .Select(s => new G9SessionReport
                            {
                                SessionId = s.Account.Session.SessionId,
                                TotalReceive = s.Account.Session.SessionTotalReceiveBytes,
                                TotalSend = s.Account.Session.SessionTotalReceiveBytes,
                                StartTime = s.Account.Session.SessionStartDateTime
                            })
                            .ToArray(),
                        TypeOfCompare.NotEqual => _core
                            .SelectAccountUtilities(s => s.Where(g =>
                                g.Account.GetType().GetProperty(propertyName)?.GetValue(g.Account, null) != value))
                            .Take(takeCount)
                            .Select(s => new G9SessionReport
                            {
                                SessionId = s.Account.Session.SessionId,
                                TotalReceive = s.Account.Session.SessionTotalReceiveBytes,
                                TotalSend = s.Account.Session.SessionTotalReceiveBytes,
                                StartTime = s.Account.Session.SessionStartDateTime
                            })
                            .ToArray(),
                        TypeOfCompare.Greater => _core
                            .SelectAccountUtilities(s => s.Where(g =>
                                decimal.TryParse(
                                    g.Account.GetType().GetProperty(propertyName)?.GetValue(g.Account, null).ToString(),
                                    out var data) && decimal.TryParse(value.ToString(), out var valueResult) &&
                                data > valueResult))
                            .Take(takeCount)
                            .Select(s => new G9SessionReport
                            {
                                SessionId = s.Account.Session.SessionId,
                                TotalReceive = s.Account.Session.SessionTotalReceiveBytes,
                                TotalSend = s.Account.Session.SessionTotalReceiveBytes,
                                StartTime = s.Account.Session.SessionStartDateTime
                            })
                            .ToArray(),
                        TypeOfCompare.Less => _core
                            .SelectAccountUtilities(s => s.Where(g =>
                                decimal.TryParse(
                                    g.Account.GetType().GetProperty(propertyName)?.GetValue(g.Account, null).ToString(),
                                    out var data) && decimal.TryParse(value.ToString(), out var valueResult) &&
                                data < valueResult))
                            .Take(takeCount)
                            .Select(s => new G9SessionReport
                            {
                                SessionId = s.Account.Session.SessionId,
                                TotalReceive = s.Account.Session.SessionTotalReceiveBytes,
                                TotalSend = s.Account.Session.SessionTotalReceiveBytes,
                                StartTime = s.Account.Session.SessionStartDateTime
                            })
                            .ToArray(),
                        _ => throw new InvalidEnumArgumentException(nameof(typeOfCompare), (int) typeOfCompare,
                            typeof(TypeOfCompare))
                    };
                }
                catch (Exception ex)
                {
                    if (_core.Logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                        _core.Logging.LogException(ex);

                    return null;
                }
            });
        }

        #endregion

        /// <summary>
        ///     Get session info with session id
        /// </summary>
        /// <param name="sessionId">Specified session id</param>
        /// <returns>Return report SessionReportInfo</returns>

        #region GetSessionInformationBySessionId

        public G9SessionReport? GetSessionInformationBySessionId(uint sessionId)
        {
            try
            {
                var data = _core.GetAccountUtilitiesBySessionId(sessionId);
                return new G9SessionReport
                {
                    SessionId = data.Account.Session.SessionId,
                    TotalReceive = data.Account.Session.SessionTotalReceiveBytes,
                    TotalSend = data.Account.Session.SessionTotalReceiveBytes,
                    StartTime = data.Account.Session.SessionStartDateTime
                };
            }
            catch (Exception ex)
            {
                if (_core.Logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    _core.Logging.LogException(ex);

                return null;
            }
        }

        #endregion

        /// <summary>
        /// Get server information
        /// </summary>
        /// <returns>Get server information like string</returns>
        #region GetServerInfo
        public string GetServerInfo()
        {
            var serverUpTime = DateTime.Now - ServerStartDateTime;
            return
                $"{LogMessage.ServerStartDateTime}: {ServerStartDateTime:yyyy/MM/dd HH:mm:ss}\n{LogMessage.ServerUpTime}: {serverUpTime:G}\n{LogMessage.ServerTotalSendBytes}: {TotalSendBytes:#,##0}\t{LogMessage.ServerTotalReceiveBytes}: {TotalReceiveBytes:#,##0}\n{LogMessage.ServerTotalSendPacket}: {TotalSendPacket:#,##0}\t{LogMessage.ServerTotalReceivePacket}: {TotalReceivePacket:#,##0}\n{LogMessage.TotalSessionFromStartServerCount}: {NumberOfSessionFromStartServer:#,##0}\t{LogMessage.CurrentSessionCount}: {NumberOfCurrentSession:#,##0}";
        }
        #endregion

        #endregion
    }
}