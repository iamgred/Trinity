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
        private readonly IPayloadBuilder _payloadBuilder;
        private readonly IWebHostEnvironment _webHostEnvironment;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public PayloadService(PayloadRepository payloadRepository, ListenerRespository listenerRepository, IPayloadBuilder payloadBuilder, IWebHostEnvironment webHostEnvironment)
        {
            this._payloadRepository = payloadRepository;
            this._listenerRepository = listenerRepository;
            this._payloadBuilder = payloadBuilder;
            this._webHostEnvironment = webHostEnvironment;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Generates a new payload based on the provided PayloadCreationDTO. It first checks if the specified listener exists, then uses the IPayloadBuilder to build the payload. If successful, it saves the payload to the database.
        /// </summary>
        /// <param name="payloadCreationDTO"></param>
        /// <returns>A Result indicating the success or failure of the operation.</returns>
        public async Task<Result> GeneratePayloadAsync(PayloadCreationDTO payloadCreationDTO)
        {
            var listener = await _listenerRepository.GetListenerAsync(payloadCreationDTO.ListenerID);

            if (listener == null)
            {
                return ListenerError.NotFound(payloadCreationDTO.ListenerID);
            }

            var payloadUUID = Guid.NewGuid().ToString();

            var buildResult = await _payloadBuilder.BuildAsync(payloadCreationDTO, payloadUUID, listener);

            //if (!buildResult.IsSuccess)
            //{
            //    return PayloadError.GenerationFailed();
            //}

            var payload = new Payload
            {
                PayloadUUID = payloadUUID,
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
        /// <summary>
        /// Retrieves a list of all payloads from the database and constructs a list of PayloadDTOs containing relevant information such as name, size, type, and creation time.
        /// </summary>
        /// <returns>A list of PayloadDTOs.</returns>
        public async Task<Result<List<PayloadDTO>>> GetPayloadsAsync()
        {
            var result = await _payloadRepository.GetPayloadsAsync();

            List<PayloadDTO> payloadDTOs = result
                .Select(p =>
                {
                    string filePath = GetPayloadFilePath(p);
                    int size = File.Exists(filePath) ? (int)new FileInfo(filePath).Length : 0;

                    return new PayloadDTO
                    {
                        Name = AppendFileExtension(p.FileName, p.PayloadType),
                        Size = size,
                        Type = Util.GetEnumValue<PayloadTypes>(p.PayloadType),
                        CreateTime = p.CreatedAt
                    };
                }).ToList();
            return payloadDTOs;
        }
        /// <summary>
        /// Retrieves the file path of a specific payload based on its ID. It fetches the payload from the database and constructs the full file path using the payload's file name and type.
        /// If the payload does not exist or the file is not found, it returns an appropriate error result.
        /// </summary>
        /// <param name="payloadID"></param>
        /// <returns>The file path of the payload, or an error result if the payload is not found.</returns>
        public async Task<Result<String>> GetPayloadFilePathAsync(int payloadID)
        {
            var payload = await _payloadRepository.GetByIdAsync(payloadID);

            if (payload == null)
            {
                return PayloadError.NotFound(payloadID);
            }

            string filePath = GetPayloadFilePath(payload);

            if (!File.Exists(filePath))
            {
                return PayloadError.NotFound(payloadID);
            }

            return filePath;
        }
        /// <summary>
        /// Retrieves the file path of a specific payload based on its ID. It fetches the payload from the database and constructs the full file path using the payload's file name and type.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns>The full file path of the payload.</returns>
        private string GetPayloadFilePath(Payload payload)
        {
            string fileName = AppendFileExtension(payload.FileName, payload.PayloadType);
            return Path.Combine(_webHostEnvironment.ContentRootPath, "Payloads", fileName);
        }
        /// <summary>
        /// Appends the appropriate file extension to the payload file name based on its type.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="type"></param>
        /// <returns>The file name with the appropriate extension.</returns>
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
