//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.Mvc;
using System.Text;
using TeamServer.Interface;
using TeamServer.Repositories;
using TeamServer.Utils;
using Trinity.Shared.DTOs.Payload;
using Trinity.Shared.Enums;
using Trinity.Shared.Errors;
using Trinity.Shared.Models;
using Trinity.Shared.Results;

namespace TeamServer.Services
{
    public class PayloadService
    {
        private readonly PayloadRepository _payloadRepository;
        private readonly ListenerRespository _listenerRepository;
        private IPayloadBuilder _payloadBuilder;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public PayloadService(PayloadRepository payloadRepository, ListenerRespository listenerRepository, IPayloadBuilder payloadBuilder)
        {
            this._payloadRepository = payloadRepository;
            this._listenerRepository = listenerRepository;
            this._payloadBuilder = payloadBuilder;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<Result> GeneratePayloadAsync(PayloadCreationDTO payloadCreationDTO)
        {
            var listener = await _listenerRepository.GetListenerAsync(payloadCreationDTO.ListenerID);

            if (listener == null)
            {
                return ListenerError.NotFound(payloadCreationDTO.ListenerID);
            }

            var buildResult = await _payloadBuilder.BuildAsync(payloadCreationDTO);

            if (!buildResult.IsSuccess)
            {
                return PayloadError.GenerationFailed();
            }

            var payload = new Payload
            {
                PayloadUUID = Guid.NewGuid().ToString(),
                ListenerID = payloadCreationDTO.ListenerID,
                CampaignID = payloadCreationDTO.CampaignID,
                // TODO: Replace with autheticated operator ID
                CreatedByOperatorID = 1,
                FileName = payloadCreationDTO.Name,
                Architecture = Util.GetEnumString<Architectures>(payloadCreationDTO.Architecture),
                RetryStrategy = payloadCreationDTO.RetryStrategy,
                PayloadType = Util.GetEnumString<PayloadTypes>(payloadCreationDTO.Type),
                AES256KEY = Util.GenerateAES256Key(),
                CreatedAt = DateTime.UtcNow
            };

            await _payloadRepository.InsertPayloadAsync(payload);
            await _payloadRepository.CommitAsync();

            return Result.Success();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<Result<List<PayloadDTO>>> GetPayloadsAsync()
        {
            var result = await _payloadRepository.GetPayloadsAsync();
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
