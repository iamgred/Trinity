Quick issue list
🔴 Definite integration issues
1. Upload controller queues the wrong task
   - Uses TaskType.DOWNLOAD
   - Uses TaskCode.CANCEL_DOWNLOAD
   - Should represent an upload operation.
2. C# ↔ C++ TaskCode mismatch
   - C# 8 = DOWNLOAD
   - Agent 8 = DOWNLOAD_CHUNK
   - Same numeric value, different meaning.
3. Download chunk preparation is disconnected
   - FileTransferService.PrepareDownloadChunkAsync() exists.
   - Nothing currently calls it.
4. Upload chunk handling is disconnected
   - ReceiveCheckinUploadChunkAsync() exists.
   - No current response strategy/handler path reaches it.
5. File-transfer response statuses are missing
   - Handler references:
     - UPLOAD_CHUNK
     - UPLOAD_FINAL
     - DOWNLOAD_CHUNK
     - DOWNLOAD_FINAL
     - KEYLOGGER
   - Current Status enum does not contain them.
6. Handler references classes that aren't present
   - UploadStrategy
   - DownloadStrategy
   - KeyloggerStrategy
7. Current Agent has no active upload/download execution
   - Agent.cpp handles PowerShell, WinTask and .NET ASM.
   - Download/upload fall through to the generic success response.
🟠 Payload/build issues
8. CMake variable names don't match the payload builder
   - CMake expects INPUT_HOSTS / INPUT_PAYLOAD_UUID
   - CMakePayloadBuilder passes HOST / PAYLOAD_UUID.
9. Configured host isn't actually used by CommManager
   - CMake provides a host list.
   - CommManager hardcodes localhost.
10. Port configuration isn't passed through
    - CMake has PORT.
    - Builder/`CommManager` don't properly use the configured value.
11. PayloadService ignores build failure
- Payload can potentially be stored/reported as successful even if compilation failed.
🟡 Agent/code-quality/integration concerns
12. Agent constructor self-assigns parameters
    - commManager = commManager;
    - taskManager = taskManager;
    - Members aren't actually initialized from the arguments.
13. Unsupported task types silently return success
    - This can make an unimplemented feature look like it worked.
14. TaskRequestFactory has download-related override functionality that's currently unused
    - Create(task, commandOverride) exists.
    - Current AgentService doesn't use it.
15. Multiple status/task-status systems exist
    - Tasks.Status
    - TaskStatuses
    - Models.TaskStatus
    - Some older code appears to use the wrong one.