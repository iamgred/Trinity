// ================================================================== { START OF FILE } =================================================================== //
#include "TorCommunicationTransport.h"

#include <algorithm>
#include <cstring>
#include <limits>
#include <string>

#ifdef _WIN32

#define NOMINMAX
#include <windows.h>
#include <ws2tcpip.h>

#else

#include <arpa/inet.h>
#include <cerrno>
#include <fcntl.h>
#include <netdb.h>
#include <sys/socket.h>
#include <unistd.h>

#endif

namespace
{
#ifdef _WIN32
    constexpr SocketHandle INVALID_SOCKET_VALUE = INVALID_SOCKET;
#else
    constexpr SocketHandle INVALID_SOCKET_VALUE = -1;
#endif

    constexpr unsigned char SOCKS5_VERSION = 0x05;

    constexpr unsigned char SOCKS5_AUTH_NONE = 0x00;
    constexpr unsigned char SOCKS5_AUTH_USERPASS = 0x02;
    constexpr unsigned char SOCKS5_AUTH_NO_ACCEPTABLE = 0xFF;

    constexpr unsigned char SOCKS5_CMD_CONNECT = 0x01;

    constexpr unsigned char SOCKS5_ATYP_IPV4 = 0x01;
    constexpr unsigned char SOCKS5_ATYP_DOMAIN = 0x03;
    constexpr unsigned char SOCKS5_ATYP_IPV6 = 0x04;

    constexpr unsigned char SOCKS5_REPLY_SUCCEEDED = 0x00;

    constexpr std::size_t DEFAULT_RECEIVE_SIZE = 4096;
    constexpr unsigned short DEFAULT_PROXY_PORT = 9050;

#ifdef _WIN32
    bool InitializeWinsock()
    {
        static bool initialized = false;
        static bool failed = false;

        if (initialized)
        {
            return true;
        }

        if (failed)
        {
            return false;
        }

        WSADATA wsaData{};

        if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0)
        {
            failed = true;
            return false;
        }

        initialized = true;
        return true;
    }
#endif
}

TorCommunicationTransport::TorCommunicationTransport()
    : socket(INVALID_SOCKET_VALUE)
{
#ifdef _WIN32
    InitializeWinsock();
#endif
}

TorCommunicationTransport::~TorCommunicationTransport()
{
    Disconnect();
}

void TorCommunicationTransport::Configure(
    const CommunicationConfig &newConfig)
{
    Disconnect();

    config = newConfig;
    isConnected = false;
}

bool TorCommunicationTransport::ParsePort(
    const std::string &value,
    unsigned short &port)
{
    if (value.empty())
    {
        return false;
    }

    try
    {
        const unsigned long parsed =
            std::stoul(value);

        if (parsed > 65535)
        {
            return false;
        }

        port = static_cast<unsigned short>(parsed);
        return true;
    }
    catch (...)
    {
        return false;
    }
}

bool TorCommunicationTransport::Connect()
{
    Disconnect();

#ifdef _WIN32
    if (!InitializeWinsock())
    {
        return false;
    }
#endif

    if (config.endpoint.empty() || config.port == 0)
    {
        return false;
    }

    if (!ConnectToProxy())
    {
        Disconnect();
        return false;
    }

    if (!PerformSocks5Handshake())
    {
        Disconnect();
        return false;
    }

    isConnected = true;
    return true;
}

bool TorCommunicationTransport::ConnectToProxy()
{
    std::string proxyHost = "127.0.0.1";
    unsigned short proxyPort = DEFAULT_PROXY_PORT;

    const auto hostIt =
        config.options.find("proxy_host");

    if (hostIt != config.options.end() &&
        !hostIt->second.empty())
    {
        proxyHost = hostIt->second;
    }

    const auto portIt =
        config.options.find("proxy_port");

    if (portIt != config.options.end())
    {
        if (!ParsePort(portIt->second, proxyPort))
        {
            return false;
        }
    }

    addrinfo hints{};
    hints.ai_family = AF_UNSPEC;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_protocol = IPPROTO_TCP;

    addrinfo *results = nullptr;

    const std::string proxyPortString =
        std::to_string(proxyPort);

    const int result = getaddrinfo(
        proxyHost.c_str(),
        proxyPortString.c_str(),
        &hints,
        &results);

    if (result != 0 || results == nullptr)
    {
        return false;
    }

    bool connected = false;

    for (addrinfo *current = results;
         current != nullptr;
         current = current->ai_next)
    {
        socket = static_cast<SocketHandle>(
            ::socket(
                current->ai_family,
                current->ai_socktype,
                current->ai_protocol));

        if (socket == INVALID_SOCKET_VALUE)
        {
            continue;
        }

        const int connectResult =
            ::connect(
                socket,
                current->ai_addr,
                static_cast<int>(current->ai_addrlen));

        if (connectResult == 0)
        {
            connected = true;
            break;
        }

        CloseSocket();
    }

    freeaddrinfo(results);

    return connected;
}

bool TorCommunicationTransport::SendAll(
    const unsigned char *data,
    std::size_t size)
{
    if (socket == INVALID_SOCKET_VALUE)
    {
        return false;
    }

    std::size_t totalSent = 0;

    while (totalSent < size)
    {
        const std::size_t remaining =
            size - totalSent;

#ifdef _WIN32
        const int chunk =
            static_cast<int>(
                std::min(
                    remaining,
                    static_cast<std::size_t>(
                        std::numeric_limits<int>::max())));

        const int sent = ::send(
            socket,
            reinterpret_cast<const char *>(data + totalSent),
            chunk,
            0);
#else
        const ssize_t sent = ::send(
            socket,
            data + totalSent,
            remaining,
            0);
#endif

        if (sent <= 0)
        {
            return false;
        }

        totalSent += static_cast<std::size_t>(sent);
    }

    return true;
}

bool TorCommunicationTransport::ReceiveExact(
    unsigned char *data,
    std::size_t size)
{
    if (socket == INVALID_SOCKET_VALUE)
    {
        return false;
    }

    std::size_t totalReceived = 0;

    while (totalReceived < size)
    {
        const std::size_t remaining =
            size - totalReceived;

#ifdef _WIN32
        const int chunk =
            static_cast<int>(
                std::min(
                    remaining,
                    static_cast<std::size_t>(
                        std::numeric_limits<int>::max())));

        const int received = ::recv(
            socket,
            reinterpret_cast<char *>(data + totalReceived),
            chunk,
            0);
#else
        const ssize_t received = ::recv(
            socket,
            data + totalReceived,
            remaining,
            0);
#endif

        if (received <= 0)
        {
            return false;
        }

        totalReceived +=
            static_cast<std::size_t>(received);
    }

    return true;
}

bool TorCommunicationTransport::PerformSocks5Handshake()
{
    const auto usernameIt =
        config.options.find("proxy_username");

    const auto passwordIt =
        config.options.find("proxy_password");

    const bool useAuthentication =
        usernameIt != config.options.end() &&
        passwordIt != config.options.end() &&
        !usernameIt->second.empty();

    // --------------------------------------------------------------
    // SOCKS5 greeting
    // --------------------------------------------------------------

    unsigned char greeting[3];

    greeting[0] = SOCKS5_VERSION;
    greeting[1] = useAuthentication ? 2 : 1;
    greeting[2] = SOCKS5_AUTH_NONE;

    if (useAuthentication)
    {
        greeting[2] = SOCKS5_AUTH_USERPASS;
    }

    if (!SendAll(
            greeting,
            useAuthentication ? 3 : 3))
    {
        return false;
    }

    unsigned char greetingResponse[2]{};

    if (!ReceiveExact(
            greetingResponse,
            sizeof(greetingResponse)))
    {
        return false;
    }

    if (greetingResponse[0] != SOCKS5_VERSION)
    {
        return false;
    }

    if (greetingResponse[1] == SOCKS5_AUTH_NO_ACCEPTABLE)
    {
        return false;
    }

    // --------------------------------------------------------------
    // Optional username/password authentication
    // --------------------------------------------------------------

    if (greetingResponse[1] == SOCKS5_AUTH_USERPASS)
    {
        if (!useAuthentication)
        {
            return false;
        }

        const std::string &username =
            usernameIt->second;

        const std::string &password =
            passwordIt->second;

        if (username.size() > 255 ||
            password.size() > 255)
        {
            return false;
        }

        std::vector<unsigned char> authRequest;

        authRequest.reserve(
            3 +
            username.size() +
            password.size());

        authRequest.push_back(0x01);

        authRequest.push_back(
            static_cast<unsigned char>(
                username.size()));

        authRequest.insert(
            authRequest.end(),
            username.begin(),
            username.end());

        authRequest.push_back(
            static_cast<unsigned char>(
                password.size()));

        authRequest.insert(
            authRequest.end(),
            password.begin(),
            password.end());

        if (!SendAll(
                authRequest.data(),
                authRequest.size()))
        {
            return false;
        }

        unsigned char authResponse[2]{};

        if (!ReceiveExact(
                authResponse,
                sizeof(authResponse)))
        {
            return false;
        }

        if (authResponse[0] != 0x01 ||
            authResponse[1] != 0x00)
        {
            return false;
        }
    }
    else if (greetingResponse[1] != SOCKS5_AUTH_NONE)
    {
        return false;
    }

    // --------------------------------------------------------------
    // SOCKS5 CONNECT request
    //
    // ATYP 0x03 is deliberately used for the endpoint so that Tor
    // performs the DNS resolution itself. This is important for
    // .onion addresses.
    // --------------------------------------------------------------

    if (config.endpoint.size() > 255)
    {
        return false;
    }

    std::vector<unsigned char> request;

    request.reserve(
        7 + config.endpoint.size());

    request.push_back(SOCKS5_VERSION);
    request.push_back(SOCKS5_CMD_CONNECT);
    request.push_back(0x00);
    request.push_back(SOCKS5_ATYP_DOMAIN);

    request.push_back(
        static_cast<unsigned char>(
            config.endpoint.size()));

    request.insert(
        request.end(),
        config.endpoint.begin(),
        config.endpoint.end());

    const unsigned short networkPort =
        htons(config.port);

    const unsigned char *portBytes =
        reinterpret_cast<const unsigned char *>(
            &networkPort);

    request.push_back(portBytes[0]);
    request.push_back(portBytes[1]);

    if (!SendAll(
            request.data(),
            request.size()))
    {
        return false;
    }

    // --------------------------------------------------------------
    // SOCKS5 CONNECT response
    // --------------------------------------------------------------

    unsigned char responseHeader[4]{};

    if (!ReceiveExact(
            responseHeader,
            sizeof(responseHeader)))
    {
        return false;
    }

    if (responseHeader[0] != SOCKS5_VERSION)
    {
        return false;
    }

    if (responseHeader[1] != SOCKS5_REPLY_SUCCEEDED)
    {
        return false;
    }

    // Consume BND.ADDR / BND.PORT.
    switch (responseHeader[3])
    {
    case SOCKS5_ATYP_IPV4:
    {
        unsigned char address[4];

        if (!ReceiveExact(
                address,
                sizeof(address)))
        {
            return false;
        }

        break;
    }

    case SOCKS5_ATYP_IPV6:
    {
        unsigned char address[16];

        if (!ReceiveExact(
                address,
                sizeof(address)))
        {
            return false;
        }

        break;
    }

    case SOCKS5_ATYP_DOMAIN:
    {
        unsigned char length = 0;

        if (!ReceiveExact(
                &length,
                sizeof(length)))
        {
            return false;
        }

        std::vector<unsigned char> address(length);

        if (length > 0 &&
            !ReceiveExact(
                address.data(),
                address.size()))
        {
            return false;
        }

        break;
    }

    default:
        return false;
    }

    unsigned char boundPort[2];

    if (!ReceiveExact(
            boundPort,
            sizeof(boundPort)))
    {
        return false;
    }

    return true;
}

bool TorCommunicationTransport::Send(
    const std::vector<unsigned char> &data)
{
    if (!isConnected ||
        socket == INVALID_SOCKET_VALUE)
    {
        return false;
    }

    if (data.empty())
    {
        return true;
    }

    return SendAll(
        data.data(),
        data.size());
}

std::vector<unsigned char>
TorCommunicationTransport::Receive()
{
    if (!isConnected ||
        socket == INVALID_SOCKET_VALUE)
    {
        return {};
    }

    std::size_t receiveSize =
        DEFAULT_RECEIVE_SIZE;

    const auto sizeIt =
        config.options.find("receive_size");

    if (sizeIt != config.options.end())
    {
        try
        {
            const unsigned long parsed =
                std::stoul(sizeIt->second);

            if (parsed > 0 &&
                parsed <= 1024 * 1024)
            {
                receiveSize =
                    static_cast<std::size_t>(parsed);
            }
        }
        catch (...)
        {
            // Keep the default.
        }
    }

    std::vector<unsigned char> buffer(
        receiveSize);

#ifdef _WIN32
    const int result = ::recv(
        socket,
        reinterpret_cast<char *>(buffer.data()),
        static_cast<int>(buffer.size()),
        0);
#else
    const ssize_t result = ::recv(
        socket,
        buffer.data(),
        buffer.size(),
        0);
#endif

    if (result <= 0)
    {
        if (result == 0)
        {
            Disconnect();
        }

        return {};
    }

    buffer.resize(
        static_cast<std::size_t>(result));

    return buffer;
}

void TorCommunicationTransport::CloseSocket()
{
    if (socket == INVALID_SOCKET_VALUE)
    {
        return;
    }

#ifdef _WIN32
    ::closesocket(socket);
#else
    ::close(socket);
#endif

    socket = INVALID_SOCKET_VALUE;
}

void TorCommunicationTransport::Disconnect()
{
    CloseSocket();
    isConnected = false;
}

// ================================================================== { END OF FILE } =================================================================== //