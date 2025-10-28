-- glogin.sql: SQLcl startup macros
-- Place this file in your SQLcl search path (SQLCL_HOME\bin or a folder on SQLPATH) or keep it here in the workspace and point SQLPATH to it.

PROMPT Loading SQLcl quick connection macros from glogin.sql

-- Define connection templates (no passwords stored)
-- Replace the user and host/service values with your real values.

DEFINE Kansas_city_prod = "connect cfUser@//56.168.201.56:1521/DCSDB"

-- Optional alternate alias using a tnsnames.ora entry (preferred for maintainability)
-- Replace `KANSAS_CITY_TNS` with the entry name from your tnsnames.ora file
DEFINE Kansas_city_tns = "connect cfUser@KANSAS_CITY_TNS"

-- Usage:
-- 1) Start SQLcl (or open an SQLcl terminal in VS Code).
-- 2) Enter at the prompt: &Kansas_city_prod
--    SQLcl will substitute and execute: connect cfUser@//56.168.201.56:1521/DCSDB
--    It will then prompt you for the password.
-- 3) After connecting, run scripts as usual: @mpe_count.sql

-- Security notes:
-- - Do NOT store plaintext passwords in this file. Leave passwords out so SQLcl prompts for them.
-- - For automated, non-interactive runs prefer Oracle Wallet, Windows Credential Manager, or a secrets vault.
-- - If you want to centralize endpoints, use tnsnames.ora and connect with: connect user@tns_alias

-- Optional: Add additional DEFINE entries for other environments or helper shortcuts.

-- Quick usage/help:
--  - To load the macros into an active SQLcl session: @<fullpath>\glogin.sql
--  - To use the network-connect macro: &Kansas_city_prod  (expands to the host/service connect)
--  - To use the tns alias macro:      &Kansas_city_tns   (expands to connect using tnsnames.ora)
--  - Press Enter at the "Enter value for ..." prompt to allow the DEFINE substitution to run (do not type a value).
-- End of glogin.sql
