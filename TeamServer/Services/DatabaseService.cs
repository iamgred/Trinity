//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;
using TeamServer.Data;
using TeamServer.Utils;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Enums;
using Trinity.Shared.DTOs.Listener;
using Trinity.Shared.Models;
using Trinity.Shared.DTOs.Payload;

namespace TeamServer.Services
{
    public class DatabaseService
    {
        private readonly Context _context;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initialises a new instance of the DatabaseService class.
        /// </summary>
        public DatabaseService(Context context)
        {
            this._context = context;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Inserts an Agent task.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public async Task<int> InsertAgentTask(ICommand command, CommandTypes type, int agentID)
        {
            bool agentExists = await _context.Agents.AnyAsync(a => a.ID.Equals(agentID));

            if (!agentExists)
            {
                throw new InvalidOperationException("Invalid: Operator does not exist!");
            }

            Trinity.Shared.Models.Task task = new Trinity.Shared.Models.Task
            {
                AgentID = agentID,
                CreatedAt = DateTime.UtcNow,
                Status = Trinity.Shared.Enums.TaskStatuses.Queued,
                CommandType = type,
                Command = JsonDocument.Parse(JsonSerializer.Serialize(command, command.GetType())),
            };
            var result = await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            return result.Entity.ID;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Retrieves all the tasks.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Trinity.Shared.Models.Task>> GetTasksAsync()
        {
            List<Trinity.Shared.Models.Task> tasks = await _context.Tasks.ToListAsync();

            return tasks;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<Trinity.Shared.Models.Task?> GetTaskByIDAsync(int taskID)
        {
            Trinity.Shared.Models.Task? tasks = await _context.Tasks.FirstOrDefaultAsync(t => t.ID.Equals(taskID));

            return tasks;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<List<Trinity.Shared.Models.Task>> GetTasksByAgentAsync(int agentID)
        {
            List<Trinity.Shared.Models.Task> tasks = await _context.Tasks
                .Where(t => t.AgentID.Equals(agentID))
                .ToListAsync();

            return tasks;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<bool> ClearQueueAsync(int agentID)
        {
            var result = await _context.Tasks
                .Where(t => t.AgentID.Equals(agentID) && t.Status.Equals(TaskStatuses.Queued))
                .ExecuteDeleteAsync();

            return result > 0;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<int> InsertPayloadAsync(PayloadCreationDTO payloadCreationDTO)
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
                UUID = "test"
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

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<List<Agent>> GetAgentsAsync()
        {
            List<Agent> result = await _context.Agents.ToListAsync();
            return result;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<(Agent, string)> GetAgentAsync(int agentID)
        {
            Agent result = await _context.Agents.FirstOrDefaultAsync(a => a.ID.Equals(agentID)) ?? throw new InvalidOperationException("Agent not found");
            Campaign campaign = await _context.Campaigns.FirstOrDefaultAsync(c => c.ID.Equals(result.CampaignID)) ?? throw new InvalidOperationException("Campaign not found");
            return (result, campaign.Name);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<List<Campaign>> GetCampaignsAsync()
        {
            List<Campaign> result = await _context.Campaigns.ToListAsync();
            return result;
        }
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /**
            * Retrieves a Campaign by its ID.
            *
            * @param campaignID The ID of the Campaign to retrieve.
            * @return The Campaign with the specified ID, or null if not found.
            */
        public async Task<Campaign?> GetCampaignByIdAsync(int campaignID)
        {
            Campaign? result = await _context.Campaigns.FirstOrDefaultAsync(c => c.ID.Equals(campaignID));
            return result;
        }
        /**
            * Inserts a new Campaign into the database.
            *
            * @param campaign The Campaign entity to insert.
            * @return The ID of the newly inserted Campaign.
            */
        public async Task<int> InsertCampaignAsync(Campaign campaign)
        {
            var result = await _context.Campaigns.AddAsync(campaign);
            await _context.SaveChangesAsync();
            return result.Entity.ID;
        }
        /**
            * Updates an existing Campaign in the database.
            *
            * @param campaign The Campaign entity with updated information.
            * @return True if the update was successful, false if the Campaign was not found.
            */
        public async Task<bool> UpdateCampaignAsync(Campaign campaign)
        {
            var existingCampaign = await _context.Campaigns.AnyAsync(c => c.ID.Equals(campaign.ID));

            if (!existingCampaign)
            {
                return false; // Campaign not found
            }

            _context.Campaigns.Update(campaign);

            await _context.SaveChangesAsync();
            return true; // Update successful
        }
        /**
            * Deletes a Campaign from the database by its ID.
            *
            * @param campaignID The ID of the Campaign to delete.
            * @return True if the deletion was successful, false if the Campaign was not found.
            */
        public async Task<bool> DeleteCampaignAsync(int campaignID)
        {
            int result = await _context.Campaigns
                .Where(c => c.ID.Equals(campaignID))
                .ExecuteDeleteAsync();
            return result > 0;
        }
        /**
            * Retrieves an Operator by their ID.
            *
            * @param operatorID The ID of the Operator to retrieve.
            * @return The Operator with the specified ID, or null if not found.
            */
        public async Task<Operator?> GetOperatorByIdAsync(int operatorID)
        {
            Operator? result = await _context.Operators.FirstOrDefaultAsync(o => o.ID.Equals(operatorID));
            return result;
        }
        /**
            * Retrieves all Operators from the database.
            *
            * @return A list of all Operators.
            */
        public async Task<List<Operator>> GetOperatorsAsync()
        {
            List<Operator> result = await _context.Operators.ToListAsync();
            return result;
        }
        /**
            * Retrieves all Operators associated with a specific Campaign ID.
            *
            * @param campaignID The ID of the Campaign to retrieve Operators for.
            * @return A list of Operators associated with the specified Campaign ID.
            */
        public async Task<List<Operator>> GetOperatorsByCampaignIdAsync(int campaignID)
        {
            List<Operator> result = await _context.CampaignBridges
                .Where(c => c.CampaignID.Equals(campaignID))
                .Join(_context.Operators, c => c.OperatorID, o => o.ID, (c, o) => o)
                .ToListAsync();
            return result;
        }
        /**
            * Assigns an Operator to a Campaign by creating a new CampaignBridge entry.
            *
            * @param operatorID The ID of the Operator to assign.
            * @param campaignID The ID of the Campaign to assign the Operator to.
            * @return True if the assignment was successful, false if the Operator or Campaign does not exist or if the assignment already exists.
            */
        public async Task<bool> AssignOperatorToCampaignAsync(int operatorID, int campaignID)
        {
            // Check if the operator exists
            bool operatorExists = await _context.Operators.AnyAsync(o => o.ID.Equals(operatorID));
            if (!operatorExists)
            {
                return false; // Operator does not exist
            }

            // Check if the campaign exists
            bool campaignExists = await _context.Campaigns.AnyAsync(c => c.ID.Equals(campaignID));
            if (!campaignExists)
            {
                return false; // Campaign does not exist
            }

            bool assignmentExists = await _context.CampaignBridges.AnyAsync(cb => cb.OperatorID.Equals(operatorID) && cb.CampaignID.Equals(campaignID));
            if (assignmentExists)
            {
                return false; // Assignment already exists
            }

            // Create a new CampaignBridge entry
            CampaignBridge campaignBridge = new CampaignBridge
            {
                OperatorID = operatorID,
                CampaignID = campaignID,
                AssignedDate = DateTime.UtcNow
            };

            await _context.CampaignBridges.AddAsync(campaignBridge);
            await _context.SaveChangesAsync();

            return true;
        }
        /**
            * Removes an Operator from a Campaign by deleting the corresponding CampaignBridge entry.
            *
            * @param operatorID The ID of the Operator to remove.
            * @param campaignID The ID of the Campaign to remove the Operator from.
            * @return True if the removal was successful, false if the assignment does not exist.
            */
        public async Task<bool> RemoveOperatorFromCampaignAsync(int operatorID, int campaignID)
        {
            int result = await _context.CampaignBridges
                .Where(cb => cb.OperatorID.Equals(operatorID) && cb.CampaignID.Equals(campaignID))
                .ExecuteDeleteAsync();
            return result > 0;
        }
        /**
            * Inserts a new Operator into the database.
            *
            * @param operatorEntity The Operator entity to insert.
            * @return The ID of the newly inserted Operator.
            */
        public async Task<int> InsertOperatorAsync(Operator operatorEntity)
        {
            var result = await _context.Operators.AddAsync(operatorEntity);
            await _context.SaveChangesAsync();
            return result.Entity.ID;
        }
        /**
            * Updates an existing Operator in the database.
            *
            * @param operatorEntity The Operator entity with updated information.
            * @return True if the update was successful, false if the Operator was not found.
            */
        public async Task<bool> UpdateOperatorAsync(Operator operatorEntity)
        {
            var existingOperator = await _context.Operators.AnyAsync(o => o.ID.Equals(operatorEntity.ID));

            if (!existingOperator)
            {
                return false; // Operator not found
            }

            _context.Operators.Update(operatorEntity);

            await _context.SaveChangesAsync();
            return true; // Update successful
        }
        /**
            * Deletes an Operator from the database by their ID.
            *
            * @param operatorID The ID of the Operator to delete.
            * @return True if the deletion was successful, false if the Operator was not found.
            */
        public async Task<bool> DeleteOperatorAsync(int operatorID)
        {
            int result = await _context.Operators
                .Where(o => o.ID.Equals(operatorID))
                .ExecuteDeleteAsync();
            return result > 0;
        }
        /**
            * Retrieves an Admin by their ID.
            *
            * @param adminID The ID of the Admin to retrieve.
            * @return The Admin with the specified ID, or null if not found.
            */
        public async Task<Admin?> GetAdminByIdAsync(int adminID)
        {
            Admin? result = await _context.Admins.FirstOrDefaultAsync(a => a.ID.Equals(adminID));
            return result;
        }
        /**
            * Retrieves all Admins from the database.
            *
            * @return A list of all Admins.
            */
        public async Task<List<Admin>> GetAdminsAsync()
        {
            List<Admin> result = await _context.Admins.ToListAsync();
            return result;
        }
        /**
            * Inserts a new Admin into the database.
            *
            * @param adminEntity The Admin entity to insert.
            * @return The ID of the newly inserted Admin.
            */
        public async Task<int> InsertAdminAsync(Admin adminEntity)
        {
            var result = await _context.Admins.AddAsync(adminEntity);
            await _context.SaveChangesAsync();
            return result.Entity.ID;
        }
        /**
            * Updates an existing Admin in the database.
            *
            * @param adminEntity The Admin entity with updated information.
            * @return True if the update was successful, false if the Admin was not found.
            */
        public async Task<bool> UpdateAdminAsync(Admin adminEntity)
        {
            var existingAdmin = await _context.Admins.AnyAsync(a => a.ID.Equals(adminEntity.ID));

            if (!existingAdmin)
            {
                return false; // Admin not found
            }

            _context.Admins.Update(adminEntity);

            await _context.SaveChangesAsync();
            return true; // Update successful
        }
        /// <summary>
        /// Deletes an Admin from the database by their ID.
        /// </summary>
        /// <param name="adminID"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAdminAsync(int adminID)
        {
            int result = await _context.Admins
                .Where(a => a.ID.Equals(adminID))
                .ExecuteDeleteAsync();
            return result > 0;
        }
    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //