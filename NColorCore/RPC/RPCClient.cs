﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using NBitcoin.RPC;
using NBitcoin.DataEncoders;
using NBitcoin;
using System.Collections.Specialized;
using System.Web.NBitcoin;
using System.Web;
using System.Net.Http;

namespace NColorCore.RPC
{
    public partial class RPCClient
    {
        const int DEFAULTPORT = 8081;
        private readonly string _Authentication;
        private readonly Uri _address;
        public Uri Address
        {
            get
            {
                return _address;
            }
        }

#if !NOFILEIO
        /// <summary>
        /// Use default bitcoin parameters to configure a RPCClient.
        /// </summary>
        /// <param name="network">The network used by the node. Must not be null.</param>
        public RPCClient() : this(null as string, BuildUri(null, DEFAULTPORT))
        {
        }
#endif
        public RPCClient(NetworkCredential credentials, string host)
            : this(credentials, BuildUri(host, DEFAULTPORT))
        {
        }

        public RPCClient(NetworkCredential credentials, Uri address)
            : this(credentials == null ? null : (credentials.UserName + ":" + credentials.Password), address)
        {
        }

        public RPCClient(string authenticationString, Uri address, int port = DEFAULTPORT)
        {
            if (string.IsNullOrEmpty(authenticationString))
            {
                throw new ArgumentException("The authentication string to RPC is not provided.");
            }

            if (address == null)
            {
                address = new Uri("http://127.0.0.1:" + port + "/");
            }

            _Authentication = authenticationString;
            _address = address;
        }

        private static Uri BuildUri(string hostOrUri, int port)
        {
            if (hostOrUri != null)
            {
                hostOrUri = hostOrUri.Trim();
                try
                {
                    if (hostOrUri.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                       hostOrUri.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                        return new Uri(hostOrUri, UriKind.Absolute);
                }
                catch { }
            }
            hostOrUri = hostOrUri ?? "127.0.0.1";
            UriBuilder builder = new UriBuilder();
            builder.Host = hostOrUri;
            builder.Scheme = "http";
            builder.Port = port;
            return builder.Uri;
        }

        public RPCResponse SendCommand(RPCOperations commandName, string[] paramNames, object[] parameters)
        {
            return SendCommand(commandName.ToString(), paramNames, parameters);
        }

        public uint256 IssueAsset(BitcoinAddress sourceAddress, BitcoinColoredAddress toAddress, int amount)
        {
            uint256 txid = null;
            try
            {
                txid = IssueAssetAsync(sourceAddress, toAddress, amount).Result;
            }
            catch (AggregateException aex)
            {
                ExceptionDispatchInfo.Capture(aex.InnerException).Throw();
            }
            return txid;
        }

        public async Task<uint256> IssueAssetAsync(BitcoinAddress sourceAddress, BitcoinColoredAddress toAddress, long amount)
        {
            List<object> parameters = new List<object>();
            parameters.Add(sourceAddress.ToString());
            parameters.Add(amount.ToString());

            List<string> paramNames = new List<string>();
            paramNames.Add("address");
            paramNames.Add("amount");
            
            if(toAddress != null)
            {
                parameters.Add(toAddress.ToString());
                paramNames.Add("to");
            }
            
            var resp = await SendCommandAsync(RPCOperations.issueasset, paramNames.ToArray(), parameters.ToArray()).ConfigureAwait(false);
            return uint256.Parse(resp.Result.ToString().Replace("\"", string.Empty));
        }


        public Task<RPCResponse> SendCommandAsync(RPCOperations commandName, string[] paramNames, object[] parameters)
        {
            return SendCommandAsync(commandName.ToString(), paramNames, parameters);
        }

        /// <summary>
        /// Send a command
        /// </summary>
        /// <param name="commandName">https://en.bitcoin.it/wiki/Original_Bitcoin_client/API_calls_list</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public RPCResponse SendCommand(string commandName, string[] paramNames, object[] parameters)
        {
            return SendCommand(new RPCRequest(commandName, paramNames, parameters));
        }

        public Task<RPCResponse> SendCommandAsync(string commandName, string[] paramNames, object[] parameters)
        {
            return SendCommandAsync(new RPCRequest(commandName, paramNames, parameters));
        }

        public RPCResponse SendCommand(RPCRequest request, bool throwIfRPCError = true)
        {
            try
            {
                return SendCommandAsync(request, throwIfRPCError).Result;
            }
            catch (AggregateException aex)
            {
                ExceptionDispatchInfo.Capture(aex.InnerException).Throw();
                return null; //Can't happen
            }
        }

        public async Task<RPCResponse> SendCommandAsync(RPCRequest request, bool throwIfRPCError = true)
        {
            RPCResponse response;
            var url = string.Format("{0}{1}", Address, request.Method);

            NameValueCollection outgoingQueryString = System.Web.HttpUtility.ParseQueryString(String.Empty);
            IList<KeyValuePair<string, string>> keyValuepair = new List<KeyValuePair<string, string>>();

            for (int i = 0; i < request.ParamNames.Count(); i++)
            {
                outgoingQueryString.Add(request.ParamNames[i], request.Params[i].ToString());

                keyValuepair.Add(new KeyValuePair<string, string>(request.ParamNames[i], request.Params[i].ToString()));
            }

            try
            {
                HttpClient client = new HttpClient();
                var result = await client.PostAsync(url, new FormUrlEncodedContent(keyValuepair));

                if (!result.IsSuccessStatusCode)
                {
                    RPCErrorCode errorCode;
                    switch (result.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                            errorCode = RPCErrorCode.RPC_INVALID_REQUEST;
                            break;
                        case HttpStatusCode.Forbidden:
                            errorCode = RPCErrorCode.RPC_FORBIDDEN_BY_SAFE_MODE;
                            break;
                        case HttpStatusCode.InternalServerError:
                            errorCode = RPCErrorCode.RPC_INTERNAL_ERROR;
                            break;
                        default:
                            errorCode = RPCErrorCode.RPC_MISC_ERROR;
                            break;
                    }

                    response = new RPCResponse(null, new RPCError { Code = errorCode, Message = result.ReasonPhrase });
                }
                else
                {
                    response = new RPCResponse(await result.Content.ReadAsStringAsync(), null);
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }

            return response;
        }

        private async Task<Stream> ToMemoryStreamAsync(Stream stream)
        {
            MemoryStream ms = new MemoryStream();
            await stream.CopyToAsync(ms).ConfigureAwait(false);
            ms.Position = 0;
            return ms;
        }
    }
}