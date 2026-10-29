#pragma once

#include <cstddef>
#include <string>

#include "Task.h"

namespace FileTransfer
{
    // Sends a local file to the C# API.
    // Files below 100 MB use one request; larger files use 10 MB chunks.
    bool SendFile(const std::string &filePath,
                  const std::string &agentKey,
                  int taskId,
                  const std::string &transferId,
                  const std::string &apiUrl,
                  std::size_t chunkSize = 10 * 1024 * 1024);

    // Joins chunk files stored by the C++ HTTP server into one output file.
    bool RebuildFile(const std::string &transferId,
                     const std::string &outputPath);

    // Sends one piece of continuous text data to the C# API.
    // Returns a task response describing whether the operation succeeded.
    Response SendStreamData(const std::string &streamId,
                            const std::string &data,
                            const std::string &apiUrl);
}