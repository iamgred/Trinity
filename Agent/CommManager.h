#pragma once
#include <string>

class CommManager {
public:
	CommManager(const std::string& uuid, const std::string& host);

	std::string IntialCheckin(const std::string& json);

private: 
	std::string uuid;
	std::string host;
	std::string callbackUUID;
};