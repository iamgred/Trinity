#include "FileTransfer.h"

#include <curl/curl.h>

#include <algorithm>
#include <array>
#include <cerrno>
#include <cstring>
#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <netinet/in.h>
#include <sstream>
#include <string>
#include <sys/socket.h>
#include <unistd.h>
#include <vector>

namespace fs = std::filesystem;

namespace FileTransfer
{
    namespace
    {

        // All files received by the C++ server are kept below this directory.
        // Keeping the storage root in one constant makes the folder layout easy to change.
        const fs::path StorageDirectory = "storage";

        // Build the folder used to store chunks for one transfer.
        // For example, transfer ID "abc" maps to storage/files/abc.
        std::string FileDirectory(const std::string &transferId)
        {
            return (StorageDirectory / "files" / transferId).string();
        }

        bool PostBinary(const std::string &url, const char *data, std::size_t size,
                        const std::string &fileName = {})
        {
            // Create one libcurl handle for this HTTP request.
            // The handle stores the URL, headers, body, and result for one request.
            CURL *curl = curl_easy_init();
            if (!curl)
                return false;

            // Tell libcurl where the receiving application is listening.
            curl_easy_setopt(curl, CURLOPT_URL, url.c_str());

            // Use HTTP POST because this request sends data to the receiver.
            curl_easy_setopt(curl, CURLOPT_POST, 1L);

            // Point the request body at the supplied bytes.
            curl_easy_setopt(curl, CURLOPT_POSTFIELDS, data);

            // Tell libcurl the exact body size, including binary zero bytes.
            curl_easy_setopt(curl, CURLOPT_POSTFIELDSIZE, static_cast<long>(size));

            // Tell the receiver not to interpret the body as text or a form.
            curl_slist *headers = curl_slist_append(nullptr, "Content-Type: application/octet-stream");
            if (!fileName.empty())
            {
                // The original name is needed when a complete file is sent.
                // Chunks use their transfer ID and chunk number instead.
                headers = curl_slist_append(headers, ("X-File-Name: " + fileName).c_str());
            }

            // Attach all prepared headers to the request.
            curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);

            // Perform the network operation.
            const CURLcode result = curl_easy_perform(curl);

            // Read the HTTP status returned by the receiver.
            long responseCode = 0;
            curl_easy_getinfo(curl, CURLINFO_RESPONSE_CODE, &responseCode);

            // Release libcurl resources after the request completes.
            curl_slist_free_all(headers);
            curl_easy_cleanup(curl);

            // Treat any 2xx response as a successful transfer.
            return result == CURLE_OK && responseCode >= 200 && responseCode < 300;
        }

        std::string HeaderValue(const std::string &request, const std::string &name)
        {
            // This simple server reads headers line by line.
            // A production application should use a complete HTTP parsing library.
            const auto marker = name + ":";
            std::istringstream lines(request);
            std::string line;
            while (std::getline(lines, line))
            {
                // Match only headers that begin with the requested name.
                if (line.rfind(marker, 0) == 0)
                {
                    // Remove the header name and colon.
                    auto value = line.substr(marker.size());

                    // Remove spaces that appear after the colon.
                    while (!value.empty() && (value.front() == ' ' || value.front() == '\t'))
                        value.erase(value.begin());

                    // getline removes '\n', but CRLF input leaves '\r' at the end.
                    if (!value.empty() && value.back() == '\r')
                        value.pop_back();
                    return value;
                }
            }
            return {};
        }

        bool SendResponse(int client, int status, const std::string &body)
        {
            // Build the smallest HTTP response needed by this wireframe server.
            // The client needs a status line, headers, and a response body.
            const std::string statusText = status == 200 ? "OK" : status == 400 ? "Bad Request"
                                                                                : "500 Internal Server Error";
            std::ostringstream response;
            response << "HTTP/1.1 " << status << ' ' << statusText << "\r\n"
                     << "Content-Type: text/plain\r\nContent-Length: " << body.size()
                     << "\r\nConnection: close\r\n\r\n"
                     << body;
            // Convert the string stream to contiguous bytes for send().
            const auto text = response.str();

            // Write the response bytes to the connected TCP socket.
            return send(client, text.data(), text.size(), 0) == static_cast<ssize_t>(text.size());
        }

        void HandleRequest(int client)
        {
            // Store the request until its headers and complete body are available.
            std::string request;
            // Reuse a fixed buffer for each socket read.
            std::array<char, 8192> buffer{};

            // This will become the location of the blank line after HTTP headers.
            std::size_t headerEnd = std::string::npos;

            // Content-Length tells us how many bytes belong to the request body.
            std::size_t contentLength = 0;

            // Read until the blank line separating headers from the body.
            while ((headerEnd = request.find("\r\n\r\n")) == std::string::npos)
            {
                // One recv call may contain only part of an HTTP request.
                const auto count = recv(client, buffer.data(), buffer.size(), 0);
                if (count <= 0 || request.size() > 1024 * 1024)
                {
                    SendResponse(client, 400, "Invalid request.");
                    return;
                }
                // Add this packet to the request accumulated so far.
                request.append(buffer.data(), static_cast<std::size_t>(count));
            }

            // The first line normally looks like: POST /api/files/id/file HTTP/1.1.
            const auto requestLineEnd = request.find("\r\n");
            if (requestLineEnd == std::string::npos)
            {
                SendResponse(client, 400, "Invalid request line.");
                return;
            }
            std::istringstream requestLine(request.substr(0, requestLineEnd));
            std::string method;
            std::string target;
            requestLine >> method >> target;
            // Read the body length from the request headers.
            const auto length = HeaderValue(request, "Content-Length");
            if (!length.empty())
                contentLength = std::stoull(length);

            const auto bodyStart = headerEnd + 4;
            while (request.size() - bodyStart < contentLength)
            {
                const auto count = recv(client, buffer.data(), buffer.size(), 0);
                if (count <= 0)
                {
                    SendResponse(client, 400, "Incomplete request body.");
                    return;
                }
                request.append(buffer.data(), static_cast<std::size_t>(count));
            }
            // Extract exactly the number of bytes declared by Content-Length.
            const std::string body = request.substr(bodyStart, contentLength);

            if (method != "POST")
            {
                SendResponse(client, 400, "Only POST is supported.");
                return;
            }

            if (target.rfind("/api/files/", 0) != 0)
            {
                SendResponse(client, 400, "Unknown endpoint.");
                return;
            }

            const auto relative = target.substr(std::string("/api/files/").size());
            const auto chunksMarker = relative.find("/chunks/");
            if (chunksMarker != std::string::npos)
            {
                // Save a chunk using a zero-padded number for reliable sorting.
                const auto transferId = relative.substr(0, chunksMarker);
                const auto chunkNumber = relative.substr(chunksMarker + 8);
                if (transferId.empty() || chunkNumber.empty())
                {
                    SendResponse(client, 400, "Invalid chunk endpoint.");
                    return;
                }
                // Make sure the transfer folder exists before writing the chunk.
                fs::create_directories(FileDirectory(transferId));

                // Build a zero-padded filename such as chunk_00000003.
                std::ostringstream path;
                path << FileDirectory(transferId) << "/chunk_" << std::setw(8)
                     << std::setfill('0') << std::stoi(chunkNumber);
                // Binary mode preserves the original bytes exactly.
                std::ofstream output(path.str(), std::ios::binary);
                output.write(body.data(), static_cast<std::streamsize>(body.size()));

                // Tell the sender whether the chunk was stored successfully.
                SendResponse(client, output ? 200 : 500, output ? "Chunk received." : "Could not create chunk.");
                return;
            }

            const auto rebuildMarker = relative.find("/rebuild");
            if (rebuildMarker != std::string::npos && rebuildMarker + 8 == relative.size())
            {
                // The rebuild request has no meaningful body.
                const auto transferId = relative.substr(0, rebuildMarker);
                // Place the rebuilt file beside complete received files.
                const auto outputPath = (StorageDirectory / "files" / (transferId + "_rebuilt")).string();
                SendResponse(client, RebuildFile(transferId, outputPath) ? 200 : 500,
                             "File rebuild requested.");
                return;
            }

            const auto fileMarker = relative.find("/file");
            if (fileMarker != std::string::npos && fileMarker + 5 == relative.size())
            {
                // A complete small file is stored directly under storage/files.
                const auto transferId = relative.substr(0, fileMarker);
                (void)transferId;
                // Create the directory used for complete files.
                fs::create_directories(StorageDirectory / "files");
                auto fileName = HeaderValue(request, "X-File-Name");
                if (fileName.empty())
                    fileName = transferId;
                // Keep only the filename portion to avoid path traversal.
                // Keep only the final path component to prevent path traversal.
                fileName = fs::path(fileName).filename().string();

                // Write the request body as one complete binary file.
                std::ofstream output(StorageDirectory / "files" / fileName, std::ios::binary);
                output.write(body.data(), static_cast<std::streamsize>(body.size()));
                SendResponse(client, output ? 200 : 500, output ? "File received." : "Could not create file.");
                return;
            }

            SendResponse(client, 400, "Unknown endpoint.");
        }

    } // namespace

    bool SendFile(const std::string &filePath, const std::string &transferId,
                  const std::string &apiUrl, std::size_t chunkSize)
    {
        // A zero chunk size would make the read loop invalid.
        if (chunkSize == 0)
            return false;

        // Open in binary mode so text conversion cannot change file bytes.
        std::ifstream file(filePath, std::ios::binary);
        if (!file)
            return false;

        // Move to the end to determine the total file size.
        file.seekg(0, std::ios::end);
        const auto fileSize = file.tellg();

        // Return to the beginning before reading file contents.
        file.seekg(0, std::ios::beg);

        // All requests for this transfer share this URL prefix.
        const auto baseUrl = apiUrl + "/api/files/" + transferId;

        if (fileSize < 100LL * 1024 * 1024)
        {
            // Small files are loaded once and sent in one request.
            std::vector<char> data(static_cast<std::size_t>(fileSize));
            file.read(data.data(), fileSize);

            // Include the original filename in the request header.
            return PostBinary(baseUrl + "/file", data.data(), data.size(), fs::path(filePath).filename().string());
        }

        // Reuse one fixed buffer so large files do not fill memory.
        std::vector<char> buffer(chunkSize);

        // Chunk numbering begins at zero.
        int chunkNumber = 0;

        // The condition also processes the final partial chunk.
        while (file.read(buffer.data(), static_cast<std::streamsize>(buffer.size())) || file.gcount() > 0)
        {
            // gcount() reports how many bytes the most recent read produced.
            const auto count = file.gcount();
            // Stop immediately if one chunk request fails.
            if (!PostBinary(baseUrl + "/chunks/" + std::to_string(chunkNumber++),
                            buffer.data(), static_cast<std::size_t>(count)))
                return false;
        }
        // Tell the receiver that it may now rebuild the original file.
        return PostBinary(baseUrl + "/rebuild", nullptr, 0);
    }

    bool RebuildFile(const std::string &transferId, const std::string &outputPath)
    {
        // Find the directory where this server stored the transfer's chunks.
        const fs::path directory = FileDirectory(transferId);
        if (!fs::exists(directory))
            return false;
        // Collect regular files only; directory entries might contain other objects.
        std::vector<fs::path> chunks;
        for (const auto &entry : fs::directory_iterator(directory))
            if (entry.is_regular_file())
                chunks.push_back(entry.path());
        // Zero-padded names sort into the original transfer order.
        std::sort(chunks.begin(), chunks.end());
        if (chunks.empty())
            return false;
        // Open the destination in binary mode.
        std::ofstream output(outputPath, std::ios::binary);
        if (!output)
            return false;
        for (const auto &chunk : chunks)
        {
            // Append each chunk as raw bytes to the destination.
            std::ifstream input(chunk, std::ios::binary);
            if (!input)
                return false;
            output << input.rdbuf();
        }
        return static_cast<bool>(output);
    }

    bool SendStreamData(const std::string &streamId, const std::string &data,
                        const std::string &apiUrl)
    {
        // Stream requests append this data to storage/streams on the C# side.
        return PostBinary(apiUrl + "/api/streams/" + streamId + "/data", data.data(), data.size());
    }

    void StartServer(int port)
    {
        // Create a TCP socket that will accept HTTP connections.
        const int server = socket(AF_INET, SOCK_STREAM, 0);
        if (server < 0)
            throw std::runtime_error("Could not create HTTP server socket.");
        int reuse = 1;
        setsockopt(server, SOL_SOCKET, SO_REUSEADDR, &reuse, sizeof(reuse));
        sockaddr_in address{};
        address.sin_family = AF_INET;
        address.sin_addr.s_addr = INADDR_ANY;
        address.sin_port = htons(static_cast<std::uint16_t>(port));
        if (bind(server, reinterpret_cast<sockaddr *>(&address), sizeof(address)) < 0 ||
            listen(server, 16) < 0)
        {
            close(server);
            throw std::runtime_error("Could not bind or listen on the configured port.");
        }
        std::cout << "C++ HTTP server listening on port " << port << "...\n";
        while (true)
        {
            const int client = accept(server, nullptr, nullptr);
            if (client < 0)
                continue;
            HandleRequest(client);
            close(client);
        }
    }

} // namespace FileTransfer

// Configure a POST request whose body is raw binary data.
// Execute the request and collect its HTTP status code.
// A 2xx response means the receiver accepted the data.
// Read until the blank line separating headers from the body.
// Parse the request method and URL path.
// Content-Length tells us exactly how many bytes belong to the body.
// Continue reading if the body was split across multiple TCP packets.
// This server intentionally supports only POST transfer operations.
// All supported file routes begin with /api/files/.
// Open in binary mode so all file types are preserved.
// Read the size once to decide between complete-file and chunked transfer.
// Large files reuse this fixed buffer for every chunk.
// Tell the C# API that all chunks have been uploaded.
// Collect regular files only; directory entries may include other objects.
// Zero-padded chunk names sort into transfer order.
// Allow quick restart after the process is stopped.
// Bind to every network interface on the requested port.
// Accept connections one at a time for this simple learning project.
