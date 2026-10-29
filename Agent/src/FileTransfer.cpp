#include "FileTransfer.h"

#include <curl/curl.h>

#include <algorithm>
#include <filesystem>
#include <fstream>
#include <string>
#include <vector>

namespace fs = std::filesystem;

namespace FileTransfer
{
    namespace
    {
        const fs::path StorageDirectory = "storage";

        std::string FileDirectory(const std::string& transferId)
        {
            return (StorageDirectory / "files" / transferId).string();
        }

        bool PostBinary(
            const std::string& url,
            const char* data,
            std::size_t size,
            const std::string& fileName = {})
        {
            CURL* curl = curl_easy_init();

            if (!curl)
                return false;

            curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
            curl_easy_setopt(curl, CURLOPT_POST, 1L);
            curl_easy_setopt(curl, CURLOPT_POSTFIELDS, data);
            curl_easy_setopt(
                curl,
                CURLOPT_POSTFIELDSIZE,
                static_cast<long>(size));

            curl_slist* headers =
                curl_slist_append(
                    nullptr,
                    "Content-Type: application/octet-stream");

            if (!fileName.empty())
            {
                const std::string header =
                    "X-File-Name: " + fileName;

                headers = curl_slist_append(
                    headers,
                    header.c_str());
            }

            curl_easy_setopt(
                curl,
                CURLOPT_HTTPHEADER,
                headers);

            const CURLcode result =
                curl_easy_perform(curl);

            long responseCode = 0;

            curl_easy_getinfo(
                curl,
                CURLINFO_RESPONSE_CODE,
                &responseCode);

            curl_slist_free_all(headers);
            curl_easy_cleanup(curl);

            return result == CURLE_OK &&
                   responseCode >= 200 &&
                   responseCode < 300;
        }
    }

    bool SendFile(
        const std::string& filePath,
        const std::string& transferId,
        const std::string& apiUrl,
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
            apiUrl + "/api/files/" + transferId;

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

            return PostBinary(
                baseUrl + "/" + fileName + "/file",
                data.empty() ? nullptr : data.data(),
                data.size(),
                fileName);
        }

        // Large files are sent in chunks.
        std::vector<char> buffer(chunkSize);

        int chunkNumber = 0;

        while (
            file.read(
                buffer.data(),
                static_cast<std::streamsize>(buffer.size()))
            || file.gcount() > 0)
        {
            const auto count = file.gcount();

            if (count <= 0)
                break;

            if (!PostBinary(
                    baseUrl +
                        "/chunks/" +
                        std::to_string(chunkNumber++),
                    buffer.data(),
                    static_cast<std::size_t>(count)))
            {
                return false;
            }
        }

        // Tell the server to rebuild the uploaded chunks.
        return PostBinary(
            baseUrl + "/rebuild",
            nullptr,
            0);
    }

    bool RebuildFile(
        const std::string& transferId,
        const std::string& outputPath)
    {
        const fs::path directory =
            FileDirectory(transferId);

        if (!fs::exists(directory))
            return false;

        std::vector<fs::path> chunks;

        for (const auto& entry :
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

        for (const auto& chunk : chunks)
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

    bool SendStreamData(
        const std::string& streamId,
        const std::string& data,
        const std::string& apiUrl)
    {
        return PostBinary(
            apiUrl +
                "/api/streams/" +
                streamId +
                "/data",
            data.data(),
            data.size());
    }
}