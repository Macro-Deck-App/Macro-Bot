CREATE SCHEMA IF NOT EXISTS macro_bot;

CREATE SEQUENCE macro_bot.tags_id_seq START WITH 1 INCREMENT BY 1;
ALTER SEQUENCE macro_bot.tags_id_seq OWNER TO macrobotmaster;
CREATE TABLE macro_bot.tags
(
    t_id                INTEGER    DEFAULT nextval('macro_bot.tags_id_seq') PRIMARY KEY,
    t_author            BIGINT     NOT NULL,
    t_guild             BIGINT     NOT NULL,
    t_name              TEXT       NOT NULL,
    t_content           TEXT       NOT NULL,
    t_created_timestamp TIMESTAMP  NOT NULL,
    t_updated_timestamp TIMESTAMP
);
ALTER TABLE macro_bot.tags OWNER TO macrobotmaster;

CREATE INDEX "tag_t_name_idx" ON macro_bot.tags (t_name);
CREATE INDEX "tag_t_id_idx" ON macro_bot.tags (t_id);