// ================================================================== { START OF FILE } =================================================================== //
#include "CommunicationManager.h"

CommunicationManager::CommunicationManager(std::unique_ptr<CommunicationTransport> transport) : transport(std::move(transport)) {};
void CommunicationManager::Configure(const CommunicationConfig &config)
{
    transport->Configure(config);
}
bool CommunicationManager::Connect()
{
    return transport->Connect();
}
bool CommunicationManager::Send(const std::vector<unsigned char> &data)
{
    return transport->Send(data);
}
std::vector<unsigned char> CommunicationManager::Receive()
{
    return transport->Receive();
}
void CommunicationManager::Disconnect()
{
    transport->Disconnect();
}
// ================================================================== { END OF FILE } =================================================================== //