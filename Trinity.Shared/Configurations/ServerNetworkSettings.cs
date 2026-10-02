//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Trinity.Shared.Exceptions;
using Trinity.Shared.Interfaces;

namespace Trinity.Shared.Configurations
{
    public class ServerNetworkSettings : IServerNetworkSettings
    {
        private IPHostEntry _host;
        private string _ipV4;
        private string _ipV6;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initializes a new instance of ServerNetworkSettings and resolves the local host name to a DNS host entry.
        /// </summary>
        /// <exception cref="IpResolutionException">Thrown when DNS resolution of the local host name fails; the exception wraps the underlying SocketException.</exception>
        public ServerNetworkSettings()
        {
            try
            {
                _host = Dns.GetHostEntry(Dns.GetHostName());
                _ipV4 = String.Empty;
                _ipV6 = String.Empty;
            }
            catch (SocketException ex)
            {
                throw new IpResolutionException("DNS resolution falied, unable to resolve teamserver's host name.", ex);
            }
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Gets the IPv4 address.
        /// </summary>
        /// <returns></returns>
        public string GetIpV4Address()
        {
            return _ipV4;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Gets the IPv6 address.
        /// </summary>
        /// <returns>The IPv6 address string.</returns>
        public string GetIpV6Address()
        {
            return _ipV6;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Retrieves the first IPv4 address for the configured host.
        /// </summary>
        /// <remarks>Logs the underlying NullReferenceException and wraps it in an IpResolutionException
        /// when resolution fails.</remarks>
        /// <returns>A dotted-quad IPv4 address string (for example, "192.168.0.1").</returns>
        /// <exception cref="IpResolutionException">Thrown when an IPv4 address cannot be resolved for the host (for example, no InterNetwork addresses or DNS
        /// resolution failure).</exception>
        public ServerNetworkSettings ResolveIpV4Address()
        {
            try
            {
                _ipV4 = _host.AddressList.FirstOrDefault(h => h.AddressFamily.Equals(AddressFamily.InterNetwork))!.ToString();
                return this;
            }
            catch (NullReferenceException ex)
            {
                throw new IpResolutionException("DNS resolution falied, unable to resolve teamserver's IPv4 address.", ex);
            }
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Gets the IPv6 address for the configured host.
        /// </summary>
        /// <remarks>Selects the first address with AddressFamily.InterNetworkV6 from the host's
        /// AddressList; failures are logged.</remarks>
        /// <returns>The IPv6 address as a string.</returns>
        /// <exception cref="IpResolutionException">Thrown when DNS resolution fails or when no IPv6 address can be resolved for the host.</exception>
        public ServerNetworkSettings ResolveIpV6Address()
        {
            try
            {
                _ipV6 = _host.AddressList.FirstOrDefault(h => h.AddressFamily.Equals(AddressFamily.InterNetworkV6))!.ToString();
                return this;
            }
            catch (NullReferenceException ex)
            {
                throw new IpResolutionException("DNS resolution falied, unable to resolve teamserver's IPv6 address.", ex);
            }
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//