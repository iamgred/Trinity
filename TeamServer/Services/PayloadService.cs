//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using TeamServer.Repositories;
using TeamServer.Utils;
using Trinity.Shared.DTOs.Payload;
using Trinity.Shared.Enums;
using Trinity.Shared.Errors;
using Trinity.Shared.Results;

namespace TeamServer.Services
{
    public class PayloadService
    {
        private readonly PayloadRepository _payloadRepository;
        private readonly ListenerRepository _listenerRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public PayloadService(PayloadRepository payloadRepository, ListenerRepository listenerRepository, IWebHostEnvironment webHostEnvironment)
        {
            _payloadRepository = payloadRepository;
            _listenerRepository = listenerRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Generates a payload based on the provided PayloadCreationDTO. It checks if the specified listener exists, creates a payload file with placeholder content, and inserts the payload information into the database. Returns a Result indicating success or failure.
        /// </summary>
        /// <param name="payloadCreationDTO"></param>
        /// <returns></returns>
        public async Task<Result> GeneratePayloadAsync(PayloadCreationDTO payloadCreationDTO)
        {
            var listener = await _listenerRepository.GetListenerAsync(payloadCreationDTO.ListenerID);

            if (listener == null)
            {
                return PayloadError.ListenerNotFound(payloadCreationDTO.ListenerID);
            }

            try
            {
                string payloadUUID = Guid.NewGuid().ToString();

                string payloadDirectory = Path.Combine(_webHostEnvironment.ContentRootPath, "Payloads");

                Directory.CreateDirectory(payloadDirectory);

                string fileName = payloadCreationDTO.Name;

                string filePath = Path.Combine(payloadDirectory, fileName);

                string payloadContent =
                $"This is a placeholder for the generated payload.\r\n" +
                $"UUID: {payloadUUID}\r\n" +
                $"Listener ID: {listener.ID}\r\n" +
                $"ARchitecture: {payloadCreationDTO.Architecture}\r\n" +
                $"Type: {payloadCreationDTO.Type}\r\n";

                await File.WriteAllTextAsync(filePath, payloadContent);

                await _payloadRepository.InsertPayloadAsync(payloadCreationDTO, payloadUUID);

                return Result.Success();
            }
            catch
            {
                return PayloadError.GenerationFailed();
            }
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Retrieves a list of payloads from the database and maps them to PayloadDTO objects. Each PayloadDTO includes the name (with appropriate file extension), size, type, and creation time. Returns a Result containing the list of PayloadDTOs.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Appends the appropriate file extension to the given file name based on the specified payload type. For example, if the payload type is WinExe, it appends ".exe"; if it's Powershell, it appends ".ps1"; otherwise, it appends ".dll".
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
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
