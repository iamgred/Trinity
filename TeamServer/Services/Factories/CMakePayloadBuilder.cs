// ============================================================= { START OF FILE } =============================================================== //
using System.Diagnostics;
using System.Text.Json;
using TeamServer.Interface;
using Trinity.Shared.DTOs.Payload;
using Trinity.Shared.Errors;
using Trinity.Shared.Results;

/// <summary>
/// CMakePayloadBuilder is responsible for building payloads using CMake. It implements the IPayloadBuilder interface and provides methods to configure the build environment, compile the source code, and copy the resulting executable to the output directory.
/// https://github.com/HavocFramework/Havoc/blob/main/payloads/Demon/CMakeLists.txt
/// </summary>
namespace TeamServer.Services
{
    public class CMakePayloadBuilder : IPayloadBuilder
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public CMakePayloadBuilder(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        /// <summary>
        /// Builds a payload using CMake based on the provided PayloadCreationDTO. It configures the build environment, compiles the source code, and copies the resulting executable to the output directory.
        /// </summary>
        /// <param name="payloadCreationDTO"></param>
        /// <returns></returns>
        public async Task<Result<string>> BuildAsync(PayloadCreationDTO payloadCreationDTO, JsonDocument listenerConfig)
        {
            try
            {
                string agentPath = Path.Combine(_webHostEnvironment.ContentRootPath, "..", "Agent");

                agentPath = Path.GetFullPath(agentPath);

                if (!Directory.Exists(agentPath))
                {
                    return PayloadError.GenerationFailed();
                }

                string buildRoot = Path.Combine(
                    _webHostEnvironment.ContentRootPath,
                    "PayloadBuilds",
                    Guid.NewGuid().ToString()
                );

                Directory.CreateDirectory(buildRoot);

                string outputPath = Path.Combine(
                    _webHostEnvironment.ContentRootPath,
                    "Payloads"
                );

                Directory.CreateDirectory(outputPath);

                string compiler = GetCompiler(payloadCreationDTO.Architecture);

                if (string.IsNullOrWhiteSpace(compiler))
                {
                    return PayloadError.GenerationFailed();
                }

                // Configure the build environment and generate build files using CMake
                Result configureResult = await RunProcessAsync(
                    "cmake",
                    $"-S \"{agentPath}\" " +
                    $"-B \"{buildRoot}\" " +
                    "-G Ninja " +
                    $"-DCMAKE_CXX_COMPILER=\"{compiler}\" "
                );

                if (!configureResult.IsSuccess)
                {
                    return PayloadError.GenerationFailed();
                }

                Result buildResult = await RunProcessAsync(
                    "cmake",
                    $"--build \"{buildRoot}\" --config Release"
                );

                if (!buildResult.IsSuccess)
                {
                    return PayloadError.GenerationFailed();
                }

                string executablePath = FindExecutable(buildRoot);

                if (string.IsNullOrWhiteSpace(executablePath))
                {
                    return PayloadError.GenerationFailed();
                }

                string outputFileName = Path.GetFileNameWithoutExtension(payloadCreationDTO.Name) + ".exe";

                string finalOutputPath = Path.Combine(outputPath, outputFileName);

                File.Copy(executablePath, finalOutputPath, true);

                return finalOutputPath;
            }
            catch
            {
                return PayloadError.GenerationFailed();
            }
        }

        /// <summary>
        /// Determines the appropriate compiler based on the specified architecture. It retrieves the compiler path from the configuration settings and returns it. If the architecture is not recognized, an empty string is returned.
        /// </summary>
        /// <param name="architecture"></param>
        /// <returns></returns>
        private string GetCompiler(string architecture)
        {
            return architecture.ToLowerInvariant() switch
            {
                "x64" => _configuration["PayloadBuilder:Compilers:x64"] ?? "x86_64-w64-mingw32-g++",
                "x86" => _configuration["PayloadBuilder:Compilers:x86"] ?? "i686-w64-mingw32-g++",
                "x32" => _configuration["PayloadBuilder:Compilers:x86"] ?? "i686-w64-mingw32-g++",
                _ => string.Empty,
            };
        }

        /// <summary>
        /// Runs a process asynchronously with the specified file name and arguments. It captures the standard output and error streams, waits for the process to exit, and returns a Result indicating success or failure based on the exit code.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<Result> RunProcessAsync(string fileName, string arguments)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = _webHostEnvironment.ContentRootPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();

                await process.StandardOutput.ReadToEndAsync();
                await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                return process.ExitCode == 0 ? Result.Success() : PayloadError.GenerationFailed();
            }
        }

        /// <summary>
        /// Searches for the first executable file (.exe) in the specified build directory and its subdirectories. If found, it returns the full path to the executable; otherwise, it returns an empty string.
        /// </summary>
        /// <param name="buildDirectory"></param>
        /// <returns></returns>
        private string FindExecutable(string buildDirectory)
        {
            string[] executables = Directory.GetFiles(buildDirectory, "*.exe", SearchOption.AllDirectories);

            return executables.FirstOrDefault() ?? string.Empty;
        }
    }
}
// ============================================================= { END OF FILE } =============================================================== //