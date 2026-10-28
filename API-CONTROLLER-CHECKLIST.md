# API Controller Expansion Checklist

This checklist covers the work required to make all file-transfer functionality controllable through the ASP.NET API.

The existing controller is [`project1-aspnet-api/Controllers/FileTransferController.cs`](./project1-aspnet-api/Controllers/FileTransferController.cs). It already exposes receiving, rebuilding, listing, downloading, and stream-upload operations.

## 1. Define the complete API contract

- [ ] Document all API routes.
- [ ] Define request and response formats.
- [ ] Define standard success and error responses.
- [ ] Define how `transferId`, `streamId`, and `chunkNumber` are used.
- [ ] Decide whether files are uploaded as:
  - Multipart form data.
  - A local server-side file path.
  - Raw binary request bodies.
- [ ] Define how the API reports progress and failures.

## 2. Add request and response models

Create strongly typed DTOs for:

- [ ] Sending a file to the C++ server.
- [ ] Sending stream data.
- [ ] Rebuilding a C++ transfer.
- [ ] Starting or stopping the C++ server, if that feature is required.
- [ ] Transfer status.
- [ ] Transfer progress.
- [ ] Transfer errors.
- [ ] Transfer cancellation.

These should be placed in a models or contracts folder rather than passing many primitive parameters directly into controller methods.

## 3. Add an endpoint for C# to send a file to C++

The service already contains `SendFileAsync` in [`project1-aspnet-api/Services/FileTransferService.cs`](./project1-aspnet-api/Services/FileTransferService.cs), but the controller does not expose it.

Add an endpoint such as:

```http
POST /api/transfers/send-to-cpp
```

It should:

- [ ] Accept the source file information.
- [ ] Accept the C++ server URL or use configured settings.
- [ ] Generate or accept a transfer ID.
- [ ] Automatically choose single-request or chunked transfer.
- [ ] Return the transfer ID.
- [ ] Return a useful status response.
- [ ] Handle C++ server connection failures.

## 4. Add a dedicated C++ server client

Create a separate C# client or service for communication with the C++ application.

For example:

```text
CppFileTransferClient
```

It should handle:

- [ ] Sending complete files to C++.
- [ ] Sending chunks to C++.
- [ ] Requesting rebuilds.
- [ ] Sending stream data.
- [ ] Checking whether the C++ server is available.
- [ ] Handling timeouts.
- [ ] Handling non-success HTTP responses.
- [ ] Applying consistent request headers.

The controller should call this service indirectly rather than containing HTTP implementation details.

## 5. Expose stream functionality completely

The current controller supports sending data to:

```http
POST /api/streams/{streamId}/data
```

Add any missing lifecycle operations:

- [ ] Create or initialize a stream.
- [ ] Append data to a stream.
- [ ] Read stream data.
- [ ] List active streams.
- [ ] Close a stream.
- [ ] Delete a stream.
- [ ] Return stream size and last-write time.

Possible endpoints:

```http
POST   /api/streams/{streamId}
POST   /api/streams/{streamId}/data
GET    /api/streams/{streamId}
GET    /api/streams
POST   /api/streams/{streamId}/close
DELETE /api/streams/{streamId}
```

## 6. Expose C++-side rebuild operations

The C# controller currently rebuilds chunks stored by the C# application. It does not rebuild chunks stored by the C++ server.

Add an endpoint such as:

```http
POST /api/cpp/transfers/{transferId}/rebuild
```

This requires:

- [ ] Adding a C++ server endpoint for rebuild requests, if necessary.
- [ ] Adding a C# client method that calls that endpoint.
- [ ] Returning the rebuilt file name or location.
- [ ] Reporting missing or incomplete chunks.
- [ ] Preventing rebuilds while chunks are still being uploaded.

## 7. Add transfer status tracking

The application currently does not maintain a formal transfer record.

Add tracking for:

- [ ] Transfer ID.
- [ ] Original file name.
- [ ] File size.
- [ ] Transfer direction.
- [ ] Source and destination.
- [ ] Expected chunk count.
- [ ] Received chunk numbers.
- [ ] Current status.
- [ ] Start time.
- [ ] Completion time.
- [ ] Error message.
- [ ] Rebuild status.

Add endpoints such as:

```http
GET /api/transfers
GET /api/transfers/{transferId}
GET /api/transfers/{transferId}/progress
```

Possible statuses:

```text
Pending
Uploading
WaitingForChunks
Rebuilding
Completed
Failed
Cancelled
```

## 8. Add transfer management endpoints

Add API control over the transfer lifecycle:

- [ ] Start a transfer.
- [ ] Pause a transfer, if supported.
- [ ] Resume a transfer.
- [ ] Cancel a transfer.
- [ ] Delete an incomplete transfer.
- [ ] Delete a completed file.
- [ ] Retry a failed chunk.
- [ ] Retry a failed transfer.
- [ ] Clean up abandoned chunks.
- [ ] Clean up old stream files.

Possible routes:

```http
POST   /api/transfers
GET    /api/transfers
GET    /api/transfers/{transferId}
POST   /api/transfers/{transferId}/cancel
POST   /api/transfers/{transferId}/retry
DELETE /api/transfers/{transferId}
```

## 9. Add reliable chunk processing

The current design does not yet include all reliability features.

- [ ] Missing-chunk detection.
- [ ] Duplicate-chunk handling.
- [ ] Chunk checksum validation.
- [ ] Complete-file checksum validation.
- [ ] Retry behavior.
- [ ] Transfer timeouts.
- [ ] Resume support after interruption.
- [ ] Idempotent chunk uploads.
- [ ] Verification before rebuilding.
- [ ] Protection against rebuilding incomplete transfers.

## 10. Add C++ server health and lifecycle control

The C++ server is currently launched manually through [`project2-cpp-file-transfer/app/main.cpp`](./project2-cpp-file-transfer/app/main.cpp):

```bash
./build/file_transfer_app server
```

To control its availability through the API:

- [ ] Add a C++ health endpoint.
- [ ] Add an endpoint for checking C++ server connectivity.
- [ ] Add configuration for the C++ server URL and port.
- [ ] Add a C++ server status endpoint.
- [ ] Decide how the C++ process will be started and stopped.
- [ ] Prefer running it as a separate operating-system service.
- [ ] Avoid allowing arbitrary process execution from public API requests.
- [ ] Add restart handling outside the controller if possible.

Possible API routes:

```http
GET /api/cpp/health
GET /api/cpp/status
```

Starting and stopping the C++ process directly from a controller should only be implemented if the deployment environment is tightly controlled.

## 11. Strengthen validation

Validate all values before processing:

- [ ] File names.
- [ ] Transfer IDs.
- [ ] Stream IDs.
- [ ] Chunk numbers.
- [ ] File paths.
- [ ] File sizes.
- [ ] C++ server URLs.
- [ ] Request content types.
- [ ] Maximum upload sizes.
- [ ] Empty request bodies.
- [ ] Invalid or missing chunks.
- [ ] Duplicate transfer IDs.

This should be applied consistently in both the C# and C++ projects.

Relevant files include:

- [`project1-aspnet-api/Controllers/FileTransferController.cs`](./project1-aspnet-api/Controllers/FileTransferController.cs)
- [`project1-aspnet-api/Services/FileTransferService.cs`](./project1-aspnet-api/Services/FileTransferService.cs)
- [`project2-cpp-file-transfer/src/file_transfer.cpp`](./project2-cpp-file-transfer/src/file_transfer.cpp)

## 12. Add security controls

Before exposing all operations through a public API, add:

- [ ] Authentication.
- [ ] Authorization.
- [ ] Role-based access for administrative actions.
- [ ] HTTPS.
- [ ] Upload size limits.
- [ ] Rate limiting.
- [ ] Request timeouts.
- [ ] Safe outbound URL validation.
- [ ] Protection against path traversal.
- [ ] Protection against arbitrary file reads.
- [ ] Protection against unauthorized file deletion.
- [ ] Protection against unauthorized C++ server control.
- [ ] Validation of uploaded file types where appropriate.

## 13. Add API documentation

Add Swagger/OpenAPI documentation for:

- [ ] Health checks.
- [ ] Complete-file uploads.
- [ ] Chunk uploads.
- [ ] Rebuild operations.
- [ ] C# to C++ transfers.
- [ ] Stream operations.
- [ ] File listing.
- [ ] File downloads.
- [ ] Transfer status.
- [ ] Transfer cancellation.
- [ ] Error responses.

The project currently has no explicit Swagger configuration in [`project1-aspnet-api/Program.cs`](./project1-aspnet-api/Program.cs).

## 14. Add logging and monitoring

Add structured logging for:

- [ ] Transfer start.
- [ ] Transfer completion.
- [ ] Chunk received.
- [ ] Chunk failure.
- [ ] Rebuild start and completion.
- [ ] C++ server connection failures.
- [ ] File validation failures.
- [ ] Authentication failures.
- [ ] Cancelled transfers.

Use the transfer ID as a correlation ID so one transfer can be traced through all log entries.

## 15. Add integration tests

Test the full system through the API:

- [ ] Health endpoint works.
- [ ] Small C#-to-C# file transfer works.
- [ ] Large chunked transfer works.
- [ ] C++ to C# transfer works.
- [ ] C# to C++ transfer works.
- [ ] C#-side rebuild works.
- [ ] C++-side rebuild works.
- [ ] Stream data appends correctly.
- [ ] Files can be listed.
- [ ] Files can be downloaded.
- [ ] Binary files preserve their contents.
- [ ] Missing chunks are rejected.
- [ ] Duplicate chunks behave correctly.
- [ ] Failed transfers can be retried.
- [ ] Transfers can be cancelled.
- [ ] Unsafe file names are rejected.
- [ ] An unavailable C++ server returns a clear error.
- [ ] Unauthorized requests are rejected.

## Recommended implementation order

1. Define the API contract.
2. Add request and response DTOs.
3. Add the C++ server client.
4. Expose C#-to-C++ file sending.
5. Complete stream endpoints.
6. Add C++ rebuild support.
7. Add transfer status tracking.
8. Add cancellation, deletion, and retry operations.
9. Add validation and reliability features.
10. Add security.
11. Add Swagger documentation.
12. Add logging and integration tests.

The most important missing feature is the endpoint that calls the existing `SendFileAsync` method. Without that endpoint, the C# service can send files to C++, but users cannot initiate that operation through the API.
