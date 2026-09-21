//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.Mvc;
using System.Text;
using TeamServer.Utils;
using Trinity.Shared.DTOs.Payload;
using Trinity.Shared.Enums;

namespace TeamServer.Services
{
    public class PayloadService
    {
        private DatabaseService _db;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public PayloadService(DatabaseService database)
        {
            this._db = database;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<PayloadCreationDTO> GeneratePayloadAsync(PayloadCreationDTO payloadCreationDTO)
        {
            try
            {
                var result = await _db.InsertPayloadAsync(payloadCreationDTO);
            }
            catch (Exception ex)
            {
                payloadCreationDTO.Error = ex.Message;
            }
            return payloadCreationDTO;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<List<PayloadDTO>> GetPayloadsAsync()
        {
            var result = await _db.GetPayloadsAsync();
            List<PayloadDTO> payloadDTOs = result
                .Select(p => new PayloadDTO
                {
                    Name = AppendFileExtension(p.FileName, p.PayloadType),
                    Size = 200,
                    Type = Util.GetEnumValue<PayloadTypes>(p.PayloadType),
                    CreateTime = p.CreatedAt
                }).ToList();

            return payloadDTOs;
        }

        private string AppendFileExtension(string fileName, PayloadTypes type)
        {
            string name;
            switch (type)
            {
                case PayloadTypes.WinExe:
                    name = fileName + ".exe";
                    break;
                case PayloadTypes.Powershell:
                    name = fileName + ".ps1";
                    break;
                default:
                    name = fileName + ".dll";
                    break;
            }

            return name;
        }

    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
