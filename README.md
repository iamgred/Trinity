# Trinity

*NB!!*
As per company request
we are not hosting this, as its a containerized system meant to be portable.

# Project Requirement
## Running the project
1. Start the stack (detached mode is recommended so your terminal isn't tied up)
   - run `docker compose up -d`
   - Or, to build and start in one step:
     - run `docker compose up -d --build`
2. Verify everything is running
   - run `docker compose ps` 
3. Access the services
   - http://localhost:5001
   - Or any port that `docker compose ps` returns for the client
## Clean Up
When you're done, stop and remove the containers, network, and (optionally) volumes/images so nothing lingers on your machine.
1. Stop and remove containers + network
  - run `docker compose down`
2. Stop and remove containers + network + named volumes
  - run `docker compose down -v`
3. Also remove the images built for this project
  - `docker compose down --rmi all -v`
