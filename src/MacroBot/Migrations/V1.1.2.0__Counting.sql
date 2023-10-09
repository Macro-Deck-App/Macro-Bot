CREATE SEQUENCE macro_bot.counting_id_seq START WITH 1 INCREMENT BY 1;
ALTER SEQUENCE macro_bot.counting_id_seq OWNER TO macrobotmaster;
CREATE TABLE macro_bot.counting
(
    cnt_id                INTEGER    DEFAULT nextval('macro_bot.counting_id_seq') PRIMARY KEY,
    cnt_author            BIGINT     NOT NULL,
    cnt_count             BIGINT     NOT NULL,
    cnt_high_score        BIGINT     NOT NULL,
    cnt_created_timestamp TIMESTAMP  NOT NULL,
    cnt_updated_timestamp TIMESTAMP
);
ALTER TABLE macro_bot.counting OWNER TO macrobotmaster;

CREATE INDEX "tag_cnt_author_idx" ON macro_bot.counting (cnt_author);