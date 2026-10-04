// ==================================== { START OF FILE } ==================================== //
#include "FileTransfer.h"

#include <filesystem>
#include <fstream>

bool FileTransfer::CopyFile(
    const std::string &source,
    const std::string &destination)
{
    try
    {
        const std::filesystem::path sourcePath(source);
        const std::filesystem::path destinationPath(destination);

        if (!std::filesystem::is_regular_file(sourcePath))
            return false;

        if (std::filesystem::exists(destinationPath) &&
            std::filesystem::equivalent(sourcePath, destinationPath))
        {
            return false;
        }

        return std::filesystem::copy_file(
            sourcePath,
            destinationPath,
            std::filesystem::copy_options::overwrite_existing);
    }
    catch (const std::filesystem::filesystem_error &)
    {
        return false;
    }
}

bool FileTransfer::GetFileSize(
    const std::string &path,
    std::uintmax_t &size)
{
    size = 0;

    try
    {
        const std::filesystem::path filePath(path);

        if (!std::filesystem::is_regular_file(filePath))
            return false;

        size = std::filesystem::file_size(filePath);
        return true;
    }
    catch (const std::filesystem::filesystem_error &)
    {
        return false;
    }
}

bool FileTransfer::FileExists(
    const std::string &path)
{
    try
    {
        return std::filesystem::is_regular_file(
            std::filesystem::path(path));
    }
    catch (const std::filesystem::filesystem_error &)
    {
        return false;
    }
}

bool FileTransfer::ReadFile(
    const std::string &path,
    std::vector<std::uint8_t> &data)
{
    data.clear();

    std::ifstream file(
        path,
        std::ios::binary | std::ios::ate);

    if (!file.is_open())
        return false;

    const std::streamsize size = file.tellg();

    if (size < 0)
        return false;

    file.seekg(0, std::ios::beg);

    data.resize(static_cast<std::size_t>(size));

    if (size == 0)
        return true;

    file.read(
        reinterpret_cast<char *>(data.data()),
        size);

    if (!file)
    {
        data.clear();
        return false;
    }

    return true;
}
// ==================================== { END OF FILE } ==================================== //