// ================================================================= { START OF FILE } ================================================================== //
using Microsoft.EntityFrameworkCore;
using TeamServer.Data;
using TeamServer.Utils;
using Trinity.Shared.DTOs.Payload;
using Trinity.Shared.Enums;
using Trinity.Shared.Models;

namespace TeamServer.Repositories
{
    public class PayloadRepository : RepositoryBase<Payload>
    {
        public PayloadRepository(Context context) : base(context)
        {
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<int> InsertPayloadAsync(PayloadCreationDTO payloadCreationDTO, string payloadUUID)
        {
            Payload payload = new Payload
            {
                ListenerID = payloadCreationDTO.ListenerID,
                Architecture = Util.GetEnumString<Architectures>(payloadCreationDTO.Architecture),
                FileName = payloadCreationDTO.Name,
                CreatedAt = DateTime.UtcNow,
                PayloadType = Util.GetEnumString<PayloadTypes>(payloadCreationDTO.Type),
                Platform = Platforms.Windows,
                ProfileID = 1,
                UUID = payloadUUID
            };

            var result = await _context.AddAsync(payload);
            await _context.SaveChangesAsync();
            return result.Entity.ID;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<List<Payload>> GetPayloadsAsync()
        {
            List<Payload> result = await _context.Payloads.ToListAsync();
            return result;
        }
    }
}
// ================================================================= { START OF FILE } ================================================================== //