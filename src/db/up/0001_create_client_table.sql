CREATE TABLE client
(
    id uuid PRIMARY KEY,
    email_address text NOT NULL,
    email_address_verified bool NOT NULL DEFAULT false
)
