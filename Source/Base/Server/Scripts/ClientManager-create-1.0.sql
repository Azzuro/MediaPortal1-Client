-- This script creates the ClientManager schema. DO NOT MODIFY!
-- Albert, 2009-11-26

-- Contains entries for all attached clients.
CREATE TABLE ATTACHED_CLIENTS (
  FRONTEND_SERVER_UUID %STRING(50)% NOT NULL PRIMARY KEY
);
