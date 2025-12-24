begin transaction;

create table messages_log (
    hash varchar(64) primary key,
    message text not null,
    received_at timestamp default current_timestamp
);

commit transaction;