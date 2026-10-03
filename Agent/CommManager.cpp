#include "CommManager.h"
#include "base64.hpp"
#include <iostream>
#include <thread>
#include <chrono>

#ifndef PAYLOAD_UUID
	#define PAYLOAD_UUID "default"
#endif

#ifndef HOST
	#define HOST "127.0.0.1"
#endif


CommManager::CommManager(const std::string& empUUID, const std::string& empHost)
{
	std::string rawUUID = empUUID.empty() ? PAYLOAD_UUID : empUUID;
	uuid = base64::to_base64(rawUUID);
	host = empHost.empty() ? HOST : empHost;
}

void CommManager::IntialCheckin() {
	std::cout << uuid;
	// base64 encode the uuid 
	// Make post request to host
}
