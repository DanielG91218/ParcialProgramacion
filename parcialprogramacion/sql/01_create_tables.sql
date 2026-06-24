CREATE TABLE IF NOT EXISTS tb_master_control (
    id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fecha_sistema timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    terreno_n integer NOT NULL CHECK (terreno_n >= 1),
    despegue_x integer NOT NULL CHECK (despegue_x >= 0),
    despegue_y integer NOT NULL CHECK (despegue_y >= 0)
);

CREATE TABLE IF NOT EXISTS tb_det_log (
    id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    master_control_id integer NOT NULL,
    paso_ofuscado integer NOT NULL,
    posicion_x integer NOT NULL CHECK (posicion_x >= 0),
    posicion_y integer NOT NULL CHECK (posicion_y >= 0),
    CONSTRAINT fk_tb_det_log_master
        FOREIGN KEY (master_control_id)
        REFERENCES tb_master_control (id)
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_tb_det_log_master_control_id
    ON tb_det_log (master_control_id);
