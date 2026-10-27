# API Endpoint Unit-Test Checklist

This document lists the unit tests required for the TeamServer API.

## Summary

- **Total endpoint methods:** 70
- **Endpoint methods with existing tests:** 3
- **Endpoint methods still requiring tests:** 67
- **Existing test file:** [`TeamServer.Tests/Controllers/FilesControllerTests.cs`](./TeamServer.Tests/Controllers/FilesControllerTests.cs)

Each endpoint should normally have tests for:

- Successful execution and the expected HTTP result.
- Invalid, missing, or boundary input.
- Service/database failure responses.
- Correct service invocation and arguments.
- Not-found, unauthorized, or forbidden behavior where applicable.

## Endpoint checklist

### File transfer controller — 7 endpoints

File transfer routes use the `/api` route prefix.

- [ ] `GET /api/health` — `Health`
  - [ ] Returns HTTP 200.
  - [ ] Returns an `ok` status and UTC timestamp.
- [ ] `POST /api/files/{fileName}/file` — `ReceiveFile`
  - [ ] Successfully receives a file.
  - [ ] Rejects an invalid or empty file name.
  - [ ] Handles an unsuccessful file-transfer service response.
  - [ ] Passes the cancellation token to the service.
- [ ] `POST /api/files/{transferId}/chunks/{chunkNumber}` — `ReceiveChunk`
  - [ ] Successfully receives a chunk.
  - [ ] Rejects an invalid transfer ID or chunk number.
  - [ ] Handles missing or invalid chunk data.
  - [ ] Handles a service failure.
- [ ] `POST /api/files/{transferId}/rebuild` — `RebuildFile`
  - [ ] Successfully rebuilds a file.
  - [ ] Returns not found for an unknown transfer.
  - [ ] Handles incomplete chunks or a service failure.
- [ ] `POST /api/streams/{streamId}/data` — `ReceiveStreamData`
  - [ ] Successfully receives stream data.
  - [ ] Rejects an invalid stream ID or empty data.
  - [ ] Handles a service failure.
- [ ] `GET /api/files` — `ListFiles`
  - [ ] Returns the available files.
  - [ ] Handles an empty file list.
  - [ ] Handles a service failure.
- [ ] `GET /api/files/{fileName}` — `DownloadFile`
  - [ ] Successfully downloads an existing file.
  - [ ] Returns not found for a missing file.
  - [ ] Rejects an invalid file name.

### Operator controller — 5 endpoints

- [x] `GET /api/v1/operators` — `GetOperatorsAsync`
  - [ ] Returns all operators.
  - [ ] Handles an empty result.
  - [ ] Handles a service/database failure.
- [x] `GET /api/v1/operators/{operatorID}` — `GetOperatorByIdAsync`
  - [ ] Returns an operator for a valid ID.
  - [ ] Returns not found for an unknown ID.
  - [ ] Rejects an invalid ID.
- [x] `POST /api/v1/operators` — `CreateOperatorAsync`
  - [ ] Creates a valid operator.
  - [ ] Rejects invalid or incomplete input.
  - [ ] Handles a duplicate operator or service failure.
- [x] `PUT /api/v1/operators/{operatorID}` — `UpdateOperatorAsync`
  - [ ] Updates an existing operator.
  - [ ] Returns not found for an unknown ID.
  - [ ] Rejects invalid input.
  - [ ] Handles a service failure.
- [x] `DELETE /api/v1/operators/{operatorID}` — `DeleteOperatorAsync`
  - [ ] Deletes an existing operator.
  - [ ] Returns not found for an unknown ID.
  - [ ] Rejects an invalid ID.
  - [ ] Handles a service failure.

### Command controller — 17 endpoints

Each command endpoint should verify the route `agentID`, DTO passed to the service, successful queuing, invalid input, unknown agent behavior, and service failure behavior.

- [x] `POST /api/v1/commands/{agentID}/spawn/powershell` — `QueuePowerShellCommand`
- [x] `POST /api/v1/commands/{agentID}/spawn/shell` — `QueueShellCommand`
- [x] `POST /api/v1/commands/{agentID}/spawn/runas` — `QueueRunAsCommand`
- [x] `POST /api/v1/commands/{agentID}/spawn/run` — `QueueRunCommand`
- [x] `POST /api/v1/commands/{agentID}/spawn/runu` — `QueueRunUCommand`
- [x] `POST /api/v1/commands/{agentID}/execute/killprocess` — `QueueKillCommand`
- [x] `POST /api/v1/commands/{agentID}/execute/escalate` — `QueueEscalateCommand`
- [x] `POST /api/v1/commands/{agentID}/execute/dotnetassembly` — `QueueDotNetAsmCommand`
- [x] `POST /api/v1/commands/{agentID}/execute/bof` — `QueueBofCommand`
- [x] `POST /api/v1/commands/{agentID}/execute/filedownload` — `QueueFileDownloadCommand`
- [x] `POST /api/v1/commands/{agentID}/execute/cancelfiledownload` — `QueueCancelFileDownloadCommand`
- [x] `POST /api/v1/commands/{agentID}/execute/upload` — `QueueUploadCommand`
- [x] `POST /api/v1/commands/{agentID}/set/spawnto` — `QueueSpawnToCommand`
- [x] `POST /api/v1/commands/{agentID}/set/updatehosts` — `QueueUpdateHostsCommand`
- [x] `POST /api/v1/commands/{agentID}/set/sleep` — `QueueSetSleepCommand`
- [x] `POST /api/v1/commands/{agentID}/execute/kill` — `QueueKillAgentCommand`
- [x] `POST /api/v1/commands/{agentID}/execute/getuid` — `QueueGetUIDCommand`

### Campaign controller — 8 endpoints

- [x] `GET /api/v1/campaigns` — `GetCampaignsAsync`
- [x] `GET /api/v1/campaigns/{campaignID}` — `GetCampaignByIdAsync`
- [x] `POST /api/v1/campaigns` — `CreateCampaignAsync`
- [x] `PUT /api/v1/campaigns/{campaignID}` — `UpdateCampaignAsync`
- [x] `DELETE /api/v1/campaigns/{campaignID}` — `DeleteCampaignAsync`
- [x] `GET /api/v1/campaigns/{campaignID}/operators` — `GetOperatorsByCampaignIdAsync`
- [x] `POST /api/v1/campaigns/{campaignID}/operators/{operatorID}` — `AssignOperatorToCampaignAsync`
- [x] `DELETE /api/v1/campaigns/{campaignID}/operators/{operatorID}` — `UnassignOperatorFromCampaignAsync`

For each campaign endpoint, test valid input, invalid IDs, not-found results, duplicate assignment where applicable, successful service calls, and service failures.

### Agent controller — 5 endpoints

- [x] `GET /api/v1/agents` — `GetAgents`
  - [ ] Returns agents successfully.
  - [ ] Handles an empty result and service failure.
- [x] `POST /api/v1/agents/checkin` — `Checkin`
  - [ ] Processes a valid check-in.
  - [ ] Rejects invalid check-in data.
  - [ ] Handles processing failure.
- [x] `POST /api/v1/agents/base64/initial/checkin` — `GetIntialBase64`
  - [ ] Processes valid initial check-in data and UUID.
  - [ ] Rejects missing or invalid UUID/data.
  - [ ] Handles processing failure.
- [x] `POST /api/v1/agents/base64/full/checkin` — `GetFullBase64`
  - [ ] Processes valid task responses and UUID.
  - [ ] Rejects invalid input.
  - [ ] Handles processing failure.
- [x] `POST /api/v1/agents/base64/error/checkin` — `GetFullBase64Error`
  - [ ] Processes valid error responses and UUID.
  - [ ] Rejects invalid input.
  - [ ] Handles processing failure.

### Payload controller — 3 endpoints

- [x] `POST /api/v1/payloads/generate` — `GeneratePayloadAsync`
  - [ ] Generates a payload from valid input.
  - [ ] Rejects invalid payload options.
  - [ ] Handles generation failure.
- [x] `GET /api/v1/payloads` — `GetPayloadsAsync`
  - [ ] Returns payloads.
  - [ ] Handles an empty result and service failure.
- [x] `GET /api/v1/payloads/{payloadID}` — `GetPayloadAsync`
  - [ ] Returns an existing payload.
  - [ ] Returns not found for an unknown ID.
  - [ ] Rejects an invalid ID.

### Files controller — 3 endpoints

These endpoints already have unit tests in [`FilesControllerTests.cs`](./TeamServer.Tests/Controllers/FilesControllerTests.cs).

- [x] `POST /api/v1/files/upload` — `UploadFileAsync`
  - [x] Service success returns HTTP 200.
  - [x] Service failure returns HTTP 400.
- [x] `GET /api/v1/files/download/{fileName}` — `DownloadFileAsync`
  - [x] Service success returns the file.
  - [x] Service failure returns HTTP 404.
- [x] `GET /api/v1/files` — `ListFiles`
  - [x] Service success returns the file list.
  - [x] Service failure returns HTTP 400.

Recommended follow-up coverage:

- [ ] Invalid or empty file upload input.
- [ ] Invalid file names.
- [ ] Service exceptions and cancellation behavior.

### Listener controller — 8 endpoints

- [x] `GET /api/v1/listeners` — `GetHttpListeners`
- [x] `POST /api/v1/listeners/http/restart/{ID}` — `RestartListenerAsync`
- [x] `POST /api/v1/listeners/http` — `StartHttpListener`
- [x] `POST /api/v1/listeners/tcp` — `CreateTcpListenerAsync`
- [x] `POST /api/v1/listeners/smb` — `CreateSmbListenerAsync`
- [x] `GET /api/v1/listeners/http/{ID}` — `GetListenerByIDAsync`
- [x] `PUT /api/v1/listeners/http/{ID}` — `UpdateHttpListenerAsync`
- [x] `DELETE /api/v1/listeners/{ID}` — `DeleteListenerAsync`

For each listener endpoint, test valid requests, invalid listener configuration, invalid IDs, not-found behavior, successful service calls, and listener-service failures. Restart should additionally verify that the restart operation is invoked exactly once.

### Admin controller — 5 endpoints

- [x] `GET /api/v1/admins` — `GetAdminsAsync`
- [x] `GET /api/v1/admins/{adminID}` — `GetAdminByIdAsync`
- [x] `POST /api/v1/admins` — `CreateAdminAsync`
- [x] `PUT /api/v1/admins/{adminID}` — `UpdateAdminAsync`
- [x] `DELETE /api/v1/admins/{adminID}` — `DeleteAdminAsync`

Test valid CRUD operations, invalid IDs, invalid models, not-found results, duplicate administrators, authorization behavior, and service failures.

### Task controller — 6 endpoints

- [x] `GET /api/v1/tasks/tasks` — `GetTasks`
- [x] `GET /api/v1/tasks/tasks/{ID}` — `GetTaskDetailsByID`
- [x] `GET /api/v1/tasks/{agentID}` — `GetTasksByAgentID`
- [x] `GET /api/v1/tasks/{agentID}/activeDownloads` — `GetActiveDownloadsByAgentID`
- [x] `DELETE /api/v1/tasks/{taskID}/stop` — `StopTask`
- [x] `DELETE /api/v1/tasks/{agentID}/clearQueue` — `ClearQueue`

Test successful results, empty results, invalid IDs, unknown agents/tasks, correct task-service invocation, and service failures. The stop and clear-queue tests should also verify that the correct destructive operation is called once.

### Server controller — 2 endpoints

- [x] `GET /api/v1/server/status` — `GetServerStatus`
  - [ ] Returns the expected server status.
  - [ ] Handles unavailable server state.
- [x] `GET /api/v1/server/teamserverip` — `GetServerIP`
  - [ ] Returns the configured TeamServer IP.
  - [ ] Handles missing or invalid configuration.

### Authentication controller — 1 endpoint

- [x] `POST /api/v1/auth/login` — `Login`
  - [ ] Authenticates valid credentials.
  - [ ] Rejects invalid credentials.
  - [ ] Rejects missing or malformed input.
  - [ ] Handles authentication-service failure.
  - [ ] Does not expose sensitive credential information in the response.

## Test-project organization

Recommended test files:

- `TeamServer.Tests/Controllers/FileTransferControllerTests.cs`
- `TeamServer.Tests/Controllers/OperatorControllerTests.cs`
- `TeamServer.Tests/Controllers/CommandControllerTests.cs`
- `TeamServer.Tests/Controllers/CampaignControllerTests.cs`
- `TeamServer.Tests/Controllers/AgentControllerTests.cs`
- `TeamServer.Tests/Controllers/PayloadControllerTests.cs`
- `TeamServer.Tests/Controllers/ListenerControllerTests.cs`
- `TeamServer.Tests/Controllers/AdminControllerTests.cs`
- `TeamServer.Tests/Controllers/TaskControllerTests.cs`
- `TeamServer.Tests/Controllers/ServerControllerTests.cs`
- `TeamServer.Tests/Controllers/AuthControllerTests.cs`

The existing `FilesControllerTests.cs` should be extended only where additional validation, authorization, exception, or cancellation coverage is required.
