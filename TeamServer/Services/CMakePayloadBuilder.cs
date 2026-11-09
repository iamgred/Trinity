// ============================================================= { START OF FILE } =============================================================== //
using System.Diagnostics;
using System.Text.Json;
using Trinity.Shared.Interfaces;
using Trinity.Shared.DTOs.Listener.Http;
using Trinity.Shared.DTOs.Payload;
using Trinity.Shared.Errors;
using Trinity.Shared.Models;
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
        public async Task<Result<string>> BuildAsync(PayloadCreationDTO payloadCreationDTO, string payloadUUID, Listener listener)
        {
            try
            {
                string agentPath = Path.GetFullPath(
                    Path.Combine(_webHostEnvironment.ContentRootPath, "..", "Agent")
                );

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

                string compiler = GetCompiler(Enum.GetName(payloadCreationDTO.Architecture)!);

                if (string.IsNullOrWhiteSpace(compiler))
                {
                    return PayloadError.GenerationFailed();
                }

                // Deserialize the listener configuration to retrieve necessary parameters for the build process
                var config = JsonSerializer.Deserialize<HttpConfig>(listener.Config);
                // Validate the deserialized configuration to ensure it is not null
                if (config == null)
                {
                    return PayloadError.GenerationFailed();
                }

                // A toolchain file, when configured, decides the compiler and linker pair; otherwise the
                // compiler path above is passed directly, which is the pre-existing behaviour.
                string toolchainName = GetToolchainName(Enum.GetName(payloadCreationDTO.Architecture)!);

                string toolchainArgument = string.IsNullOrWhiteSpace(toolchainName)
                    ? string.Empty
                    : $"-DCMAKE_TOOLCHAIN_FILE=\"{agentPath}/cmake/toolchains/{toolchainName}\" ";

                Console.WriteLine(
                    $"payload build: toolchain={(string.IsNullOrWhiteSpace(toolchainName) ? "none" : toolchainName)} "
                        + $"arch={payloadCreationDTO.Architecture} build={buildRoot}"
                );

                // Configure the build environment and generate build files using CMake
                Result configureResult = await RunProcessAsync(
                    "cmake",
                    $"-S \"{agentPath}\" " +
                    $"-B \"{buildRoot}\" " +
                    "-G Ninja " +
                    $"-DINPUT_HOSTS=\"{string.Join(";", config.Hosts)}\" " +
                    $"-DINPUT_PAYLOAD_UUID=\"{payloadUUID}\" " +
                    $"-DPORT={config.C2Port} " +
                    $"-DINPUT_USER_AGENT=\"{config.UserAgent}\" " +
                    $"-DINPUT_REQUEST_HEADER=\"{config.Header}\" " +
                    toolchainArgument +
                    (string.IsNullOrWhiteSpace(toolchainName) ? $"-DCMAKE_CXX_COMPILER=\"{compiler}\" " : string.Empty)
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
        /// Resolves the CMake toolchain file for a CPU architecture. When set,
        /// <c>PayloadBuilder:Toolchains:&lt;arch&gt;</c> names a file under
        /// <c>&lt;source&gt;/cmake/toolchains/</c> and CMake is passed
        /// <c>-DCMAKE_TOOLCHAIN_FILE</c>; the toolchain, not a compiler path, then decides the
        /// compiler and linker pair. Unset keeps the historical behaviour of passing
        /// <c>-DCMAKE_CXX_COMPILER</c> directly.
        /// </summary>
        private string GetToolchainName(string architecture)
        {
            return architecture.ToLowerInvariant() switch
            {
                "x64" => _configuration["PayloadBuilder:Toolchains:x64"] ?? string.Empty,
                "x86" or "x32" => _configuration["PayloadBuilder:Toolchains:x86"] ?? string.Empty,
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