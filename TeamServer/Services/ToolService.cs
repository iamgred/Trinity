//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using TeamServer.Repositories;
using Trinity.Shared.DTOs.Tool;
using Trinity.Shared.Enums;
using Trinity.Shared.Models;
using Trinity.Shared.Results;

namespace TeamServer.Services
{
    public class ToolService
    {
        private readonly ToolRepository _toolRepo;
        private readonly OperatorRepository _operatorRepo;
        private readonly IWebHostEnvironment _env;

        public ToolService(ToolRepository toolRepository, OperatorRepository operatorRepository, IWebHostEnvironment env)
        {
            _toolRepo = toolRepository;
            _operatorRepo = operatorRepository;
            _env = env;
        }

        public async Task<Result> UploadAsync(ToolCreationRequest request)
        {
            // Check operator exists 

            var storagePath = Path.Combine(
                _env.ContentRootPath,
                "Armoury");

            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }

            var fileName = Path.GetFileNameWithoutExtension(request.file.FileName);

            var fullPath = Path.Combine(storagePath, fileName);
            await using var stream = new FileStream(
                fullPath,
                FileMode.Create);
            await request.file.CopyToAsync(stream);

            var tool = new Tool { Type = ToolType.NET_ASM, Arch = Arch.x64, OperatorID = request.OperatorID, Name = request.Name, Path = fullPath, Size = request.file.Length };
            await _toolRepo.AddToolAsync(tool);
            await _toolRepo.CommitAsync();
            return Result.Success();
        }

        // Get Tools 

        // Remove tool 
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//