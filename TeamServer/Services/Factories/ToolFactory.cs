//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Trinity.Shared.DTOs.Tool;
using Trinity.Shared.Enums;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Models;

namespace TeamServer.Services.Factories
{
    public class ToolFactory : IToolFactory
    {
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates a Tool model.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public Tool CreateTool(ToolCreationRequest request, string fullPath)
        {
            return new Tool
            {
                Type = ToolType.NET_ASM,
                Arch = Arch.x64,
                OperatorID = request.OperatorID,
                Name = request.Name,
                Path = fullPath,
                Size = request.file.Length
            };
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//