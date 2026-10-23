# Trinity

*NB!!*
As per company request
we are not hosting this, as its a containerized system meant to be portable.

# Project Requirement
## Running the project
The stack uses a default local development database password when no override is provided. To set a private password, copy `.env.example` to `.env` and change `POSTGRES_PASSWORD`. Shell environment variables are prioritised over `.env` entries. Postgres applies this password only when initializing an empty data volume. Changing it later does not rotate the password in an existing database; update the database role explicitly, or reset the volume if its data can be discarded.

`bash` Example:
```bash
cp .env.example .env
sed -i 's/please-replace-this-postgres-password/your-own-secure-local-postgres-password/g' .env
```

### Docker
1. Start the stack:  
    `docker compose up -d`
2. Rebuild and start:  
    `docker compose up -d --build`
3. Check service health:  
    `docker compose ps`
4. Open the client at http://localhost:5001.

### Debugging Containerised TeamServer (VS Code)
The debug-only container runs idle until VS Code launches the API under the debugger. It leaves the regular TeamServer available on port 5000 and serves the debug instance on port 5002.

Start the debug container with:  
`docker compose -f docker-compose.yml -f docker-compose.debug.yml up -d --build teamserver-debug`

After the stack and debug container have been started once, rebuild only the debug service after a TeamServer code edit:  
`docker compose -f docker-compose.yml -f docker-compose.debug.yml up -d --no-deps --build teamserver-debug`

The `--no-deps` form assumes Postgres is already running and migrations have already completed. The Dockerfile keeps package restore, runtime system packages, and `vsdbg` in cached layers, and excludes migration-bundle generation from a debug-only build. Source changes still need to be compiled into the image, but should not reinstall those dependencies. In VS Code, use Restart after rebuilding to launch the updated API.

In VS Code, start `Launch TeamServer in debug container`, then open Swagger at http://localhost:5002. The debug override adds `vsdbg` and `SYS_PTRACE`; this should be used only for local development. 

The debug image's pinned Linux x64 `vsdbg` archive was validated with C# Dev Kit (`ms-dotnettools.csdevkit`) 3.40.215 and the C# extension (`ms-dotnettools.csharp`) 2.160.4. The archive URL and SHA-256 are pinned in `TeamServer/Dockerfile` to match that C# extension. When upgrading the extension, update and verify the debugger archive and checksum together.

## Clean Up
When done, stop and remove the containers and network. To remove the local database too, include `-v`; this deletes the database volume.

For the **normal** stack run:  
`docker compose down`  
If the **debug** override was used, to remove both TeamServer containers run:  
`docker compose -f docker-compose.yml -f docker-compose.debug.yml down`  
Add `-v` to either command to delete the database volume; add `--rmi all` to also remove project images.

# Agent C++ VS Code Development Setup 
1. Install VS Code extensions
    - `ms-vscode.cmake-tools` 
    - `llvm-vs-code-extensions.vscode-clangd`
    - Possibly Microsoft's C/C++ Extension Pack (which should include `ms-vscode.cmake-tools`)
2. Compile Agent to generate the `Agent/out/build/mingw-x86_64/compile_commands.json` (required by clangd):
    ```bash
    (cd Agent; [ ! -d out ] || rm -rfv out && cmake --preset mingw-x86_64 && cmake --build out/build/mingw-x86_64)
    ```


# Video Links Used In Presentation for Code Walkthrough
- Presentation: https://www.youtube.com/watch?v=M2PHoHGGW1Y
- System Demo: https://www.youtube.com/watch?v=vw6UiIC5VX4
- James Marchall code walkthrough: https://www.youtube.com/watch?v=nw_U1A0IO_c
- Lyle Richardson code walkthrough: https://www.youtube.com/watch?v=DpOx4p39GXM&feature=youtu.be
- Connaire Myall code walkthrough: https://www.youtube.com/watch?v=krvivaSBQH0
- Simon Slingerland code walkthrough: https://www.youtube.com/watch?v=UXqz9NEM7fI
