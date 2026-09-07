-- Схема БД Caimack (music-streaming), PostgreSQL.
-- Снято с EF Core ModelSnapshot: 32 таблицы, snake_case (EFCore.NamingConventions).
-- Ключи — uuid (Guid.CreateVersion7, генерируются приложением, не БД).
-- Время — timestamptz. Enum'ы хранятся как integer, значения перечислены в комментариях.

CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- ============================================================ пользователи

CREATE TABLE users (
    id            uuid PRIMARY KEY,
    username      varchar(100) NOT NULL,
    display_name  varchar(100) NOT NULL,
    password_hash varchar(255) NOT NULL,          -- BCrypt
    is_admin      boolean NOT NULL DEFAULT false,
    is_active     boolean NOT NULL DEFAULT true,
    created_at    timestamptz NOT NULL
);
CREATE UNIQUE INDEX ix_users_username ON users (username);

CREATE TABLE user_settings (
    user_id    uuid PRIMARY KEY REFERENCES users (id) ON DELETE CASCADE,
    quality    integer NOT NULL,                  -- AudioQuality: 0 Low, 1 Normal, 2 High, 3 Original
    data_saver boolean NOT NULL,
    autoplay   boolean NOT NULL,
    time_zone  varchar(64) NOT NULL,              -- IANA; по нему считаются dayparts и «микс дня»
    updated_at timestamptz NOT NULL
);

CREATE TABLE refresh_tokens (
    id         uuid PRIMARY KEY,
    user_id    uuid NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    token_hash varchar(64) NOT NULL,              -- SHA-256 от токена, сам токен не хранится
    created_at timestamptz NOT NULL,
    expires_at timestamptz NOT NULL,
    revoked_at timestamptz
);
CREATE UNIQUE INDEX ix_refresh_tokens_token_hash ON refresh_tokens (token_hash);
CREATE INDEX ix_refresh_tokens_user_id ON refresh_tokens (user_id);

-- ============================================================ каталог
-- normalized_* — результат Domain/Common/Normalize + Translit: по ним уникальность и поиск,
-- отображается всегда оригинальное поле.

CREATE TABLE artists (
    id              uuid PRIMARY KEY,
    name            varchar(300) NOT NULL,
    normalized_name varchar(300) NOT NULL,
    image_path      varchar(400),
    tags_fetched_at timestamptz,                  -- когда последний раз тянули теги из Last.fm
    created_at      timestamptz NOT NULL
);
CREATE UNIQUE INDEX ix_artists_normalized_name ON artists (normalized_name);
CREATE INDEX ix_artists_name ON artists (name);
CREATE INDEX ix_artists_normalized_name_trgm ON artists USING gin (normalized_name gin_trgm_ops);

CREATE TABLE albums (
    id               uuid PRIMARY KEY,
    artist_id        uuid NOT NULL REFERENCES artists (id) ON DELETE RESTRICT,
    title            varchar(300) NOT NULL,
    normalized_title varchar(300) NOT NULL,
    year             integer,
    cover_path       varchar(400),
    created_at       timestamptz NOT NULL
);
CREATE UNIQUE INDEX ix_albums_artist_id_normalized_title ON albums (artist_id, normalized_title);
CREATE INDEX ix_albums_title ON albums (title);
CREATE INDEX ix_albums_created_at ON albums (created_at);
CREATE INDEX ix_albums_normalized_title_trgm ON albums USING gin (normalized_title gin_trgm_ops);

CREATE TABLE genres (
    id              uuid PRIMARY KEY,
    name            varchar(150) NOT NULL,
    normalized_name varchar(150) NOT NULL,
    created_at      timestamptz NOT NULL
);
CREATE UNIQUE INDEX ix_genres_normalized_name ON genres (normalized_name);
CREATE INDEX ix_genres_normalized_name_trgm ON genres USING gin (normalized_name gin_trgm_ops);

CREATE TABLE tracks (
    id                 uuid PRIMARY KEY,
    title              varchar(400) NOT NULL,
    normalized_title   varchar(400) NOT NULL,
    artist_id          uuid NOT NULL REFERENCES artists (id) ON DELETE RESTRICT,  -- основной исполнитель
    album_id           uuid REFERENCES albums (id) ON DELETE SET NULL,
    genre_id           uuid REFERENCES genres (id) ON DELETE SET NULL,
    added_by_user_id   uuid REFERENCES users (id) ON DELETE SET NULL,
    track_number       integer,
    disc_number        integer,
    year               integer,
    duration_seconds   integer NOT NULL,
    file_path          varchar(400) NOT NULL,     -- относительно корня хранилища: music/<xx>/<yy>/<id><ext>
    original_file_name varchar(400) NOT NULL,
    content_hash       varchar(64) NOT NULL,      -- SHA-256 содержимого: защита от дублей
    file_size          bigint NOT NULL,
    mime_type          varchar(100) NOT NULL,
    codec              varchar(16),
    bitrate_kbps       integer,
    sample_rate_hz     integer,
    bits_per_sample    integer,
    ingestion_source   integer NOT NULL,          -- 0 Unknown, 1 WebUpload, 2 DirectoryImport
    tags_fetched_at    timestamptz,
    shuffle_key        double precision NOT NULL DEFAULT random(),  -- дешёвый ORDER BY для случайной выборки
    created_at         timestamptz NOT NULL
);
CREATE UNIQUE INDEX ix_tracks_content_hash ON tracks (content_hash);
CREATE UNIQUE INDEX ix_tracks_file_path ON tracks (file_path);
CREATE INDEX ix_tracks_artist_id ON tracks (artist_id);
CREATE INDEX ix_tracks_album_id ON tracks (album_id);
CREATE INDEX ix_tracks_genre_id ON tracks (genre_id);
CREATE INDEX ix_tracks_title ON tracks (title);
CREATE INDEX ix_tracks_created_at ON tracks (created_at);
CREATE INDEX ix_tracks_shuffle_key ON tracks (shuffle_key);
CREATE INDEX ix_tracks_added_by_user_id_created_at ON tracks (added_by_user_id, created_at);
CREATE INDEX ix_tracks_album_id_disc_number_track_number ON tracks (album_id, disc_number, track_number);
CREATE INDEX ix_tracks_normalized_title_trgm ON tracks USING gin (normalized_title gin_trgm_ops);

-- Полный состав исполнителей трека (feat., ремиксеры). tracks.artist_id — только «главный».
CREATE TABLE track_artists (
    track_id  uuid NOT NULL REFERENCES tracks (id) ON DELETE CASCADE,
    artist_id uuid NOT NULL REFERENCES artists (id) ON DELETE RESTRICT,
    position  integer NOT NULL,
    PRIMARY KEY (track_id, artist_id)
);
CREATE INDEX ix_track_artists_artist_id ON track_artists (artist_id);
CREATE INDEX ix_track_artists_track_id_position ON track_artists (track_id, position);

CREATE TABLE track_lyrics (
    track_id   uuid PRIMARY KEY REFERENCES tracks (id) ON DELETE CASCADE,
    plain      text NOT NULL,
    synced     jsonb NOT NULL,                    -- [{ "At": ms, "Text": "..." }], LRCLIB
    source     integer NOT NULL,                  -- 0 Embedded, 1 Manual, 2 Provider
    updated_at timestamptz NOT NULL
);

-- ============================================================ библиотека пользователя

CREATE TABLE favorites (
    user_id    uuid NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    track_id   uuid NOT NULL REFERENCES tracks (id) ON DELETE CASCADE,
    created_at timestamptz NOT NULL,
    PRIMARY KEY (user_id, track_id)
);
CREATE INDEX ix_favorites_track_id ON favorites (track_id);
CREATE INDEX ix_favorites_user_id_created_at ON favorites (user_id, created_at);

CREATE TABLE playlists (
    id          uuid PRIMARY KEY,
    user_id     uuid NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    name        varchar(200) NOT NULL,
    description varchar(1000),
    cover_path  varchar(400),
    is_public   boolean NOT NULL,
    created_at  timestamptz NOT NULL,
    updated_at  timestamptz NOT NULL
);
CREATE INDEX ix_playlists_user_id_name ON playlists (user_id, name);
CREATE INDEX ix_playlists_is_public_updated_at ON playlists (is_public, updated_at);
CREATE INDEX ix_playlists_created_at ON playlists (created_at);

-- position — плотная нумерация с нуля внутри плейлиста, перестраивается при перетаскивании.
CREATE TABLE playlist_tracks (
    id          uuid PRIMARY KEY,
    playlist_id uuid NOT NULL REFERENCES playlists (id) ON DELETE CASCADE,
    track_id    uuid NOT NULL REFERENCES tracks (id) ON DELETE CASCADE,
    position    integer NOT NULL,
    added_at    timestamptz NOT NULL
);
CREATE UNIQUE INDEX ix_playlist_tracks_playlist_id_track_id ON playlist_tracks (playlist_id, track_id);
CREATE INDEX ix_playlist_tracks_playlist_id_position ON playlist_tracks (playlist_id, position);
CREATE INDEX ix_playlist_tracks_track_id ON playlist_tracks (track_id);

-- Сырая история для экрана «недавнее».
CREATE TABLE listening_history (
    id                uuid PRIMARY KEY,
    user_id           uuid NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    track_id          uuid NOT NULL REFERENCES tracks (id) ON DELETE CASCADE,
    playback_position integer NOT NULL,
    played_at         timestamptz NOT NULL
);
CREATE INDEX ix_listening_history_played_at ON listening_history (played_at);
CREATE INDEX ix_listening_history_track_id ON listening_history (track_id);
CREATE INDEX ix_listening_history_user_id_played_at ON listening_history (user_id, played_at);
CREATE INDEX ix_listening_history_user_id_track_id_played_at ON listening_history (user_id, track_id, played_at);

-- Почасовая агрегация: статистика и вкус по частям суток. hour — начало часа в UTC.
CREATE TABLE listening_stats (
    user_id          uuid NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    hour             timestamptz NOT NULL,
    track_id         uuid NOT NULL REFERENCES tracks (id) ON DELETE CASCADE,
    play_count       integer NOT NULL,
    listened_seconds bigint NOT NULL,
    PRIMARY KEY (user_id, hour, track_id)
);
CREATE INDEX ix_listening_stats_track_id ON listening_stats (track_id);

-- ============================================================ сигналы и профиль вкуса

-- Сырые события плеера, из очереди пишет EventIngestWorker. sequence — монотонный водяной знак,
-- по нему роллап знает, что уже учтено (см. user_taste_profiles.events_watermark).
CREATE TABLE playback_events (
    id               uuid PRIMARY KEY,
    sequence         bigint GENERATED BY DEFAULT AS IDENTITY,
    user_id          uuid NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    track_id         uuid REFERENCES tracks (id) ON DELETE CASCADE,
    session_id       uuid NOT NULL,
    type             integer NOT NULL,  -- 0 Unknown, 1 TrackStarted, 2 TrackPlayed, 3 TrackCompleted,
                                        -- 4 TrackSkipped, 5 TrackPaused, 6 TrackReplayed, 7 TrackLiked,
                                        -- 8 TrackUnliked, 9 TrackAddedToPlaylist, 10 TrackRemovedFromPlaylist,
                                        -- 11 TrackAddedToQueue, 12 ArtistOpened, 13 AlbumOpened,
                                        -- 14 SearchResultClicked, 15 PlaylistOpened
    source           integer NOT NULL,  -- 0 Unknown, 1 Home, 2 Recommendation, 3 Search, 4 Album, 5 Artist,
                                        -- 6 Playlist, 7 Favorites, 8 Genre, 9 History, 10 Queue, 11 Tracks,
                                        -- 12 Radio, 13 Dj, 14 Tag
    source_id        uuid,              -- id раздела-источника (плейлист/альбом/жанр); у тега его нет
    entity_id        uuid,              -- цель события, когда это не трек (артист, альбом, плейлист)
    position_seconds integer NOT NULL,
    listened_seconds integer NOT NULL,
    duration_seconds integer NOT NULL,
    platform         varchar(32) NOT NULL,
    occurred_at      timestamptz NOT NULL
);
CREATE INDEX ix_playback_events_occurred_at ON playback_events (occurred_at);
CREATE INDEX ix_playback_events_track_id ON playback_events (track_id);
CREATE INDEX ix_playback_events_session_id_occurred_at ON playback_events (session_id, occurred_at);
CREATE INDEX ix_playback_events_user_id_occurred_at ON playback_events (user_id, occurred_at);
CREATE INDEX ix_playback_events_user_id_sequence ON playback_events (user_id, sequence);
CREATE INDEX ix_playback_events_user_id_track_id_occurred_at ON playback_events (user_id, track_id, occurred_at);

-- Три таблицы «близости» с одинаковой формой: накопленный вес с экспоненциальным затуханием.
-- decayed_weight хранится на момент decay_anchor, к текущему моменту домножается на exp(-λΔt),
-- поэтому строки не нужно переписывать при каждом тике времени.
CREATE TABLE user_track_affinity (
    user_id                uuid NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    track_id               uuid NOT NULL REFERENCES tracks (id) ON DELETE CASCADE,
    play_count             integer NOT NULL,
    skip_count             integer NOT NULL,
    completed_count        integer NOT NULL,
    replay_count           integer NOT NULL,
    playlist_adds          integer NOT NULL,
    completion_sum         double precision NOT NULL,
    completion_samples     integer NOT NULL,
    total_listened_seconds bigint NOT NULL,
    decayed_weight         double precision NOT NULL,
    decay_anchor           timestamptz NOT NULL,
    score                  double precision NOT NULL,
    first_played_at        timestamptz NOT NULL,
    last_played_at         timestamptz NOT NULL,
    updated_at             timestamptz NOT NULL,
    PRIMARY KEY (user_id, track_id)
);
CREATE INDEX ix_user_track_affinity_track_id ON user_track_affinity (track_id);
CREATE INDEX ix_user_track_affinity_user_id_score ON user_track_affinity (user_id, score);
CREATE INDEX ix_user_track_affinity_user_id_last_played_at ON user_track_affinity (user_id, last_played_at);

CREATE TABLE user_artist_affinity (
    user_id        uuid NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    artist_id      uuid NOT NULL REFERENCES artists (id) ON DELETE CASCADE,
    play_count     integer NOT NULL,
    skip_count     integer NOT NULL,
    decayed_weight double precision NOT NULL,
    decay_anchor   timestamptz NOT NULL,
    score          double precision NOT NULL,
    last_played_at timestamptz NOT NULL,
    updated_at     timestamptz NOT NULL,
    PRIMARY KEY (user_id, artist_id)
);
CREATE INDEX ix_user_artist_affinity_artist_id ON user_artist_affinity (artist_id);
CREATE INDEX ix_user_artist_affinity_user_id_score ON user_artist_affinity (user_id, score);

CREATE TABLE user_genre_affinity (
    user_id        uuid NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    genre_id       uuid NOT NULL REFERENCES genres (id) ON DELETE CASCADE,
    play_count     integer NOT NULL,
    skip_count     integer NOT NULL,
    decayed_weight double precision NOT NULL,
    decay_anchor   timestamptz NOT NULL,
    score          double precision NOT NULL,
    last_played_at timestamptz NOT NULL,
    updated_at     timestamptz NOT NULL,
    PRIMARY KEY (user_id, genre_id)
);
CREATE INDEX ix_user_genre_affinity_genre_id ON user_genre_affinity (genre_id);
CREATE INDEX ix_user_genre_affinity_user_id_score ON user_genre_affinity (user_id, score);

CREATE TABLE user_taste_profiles (
    user_id               uuid PRIMARY KEY REFERENCES users (id) ON DELETE CASCADE,
    top_artists           jsonb NOT NULL,          -- [{ Id, Name, Weight }]
    top_genres            jsonb NOT NULL,
    dayparts              jsonb NOT NULL,          -- [{ Part: 0..3, Share, Energy, TopGenres }]
    year_center           double precision,
    year_spread           double precision NOT NULL,
    average_completion    double precision NOT NULL,
    skip_rate             double precision NOT NULL,
    distinct_tracks       integer NOT NULL,
    total_event_count     integer NOT NULL,
    positive_signal_count integer NOT NULL,
    positive_signal_mass  double precision NOT NULL,  -- то же, но с затуханием от signal_decay_anchor
    signal_decay_anchor   timestamptz NOT NULL,
    maturity              integer NOT NULL,        -- ProfileMaturity: 0 Cold, 1 Warm, 2 Mature
    events_watermark      bigint NOT NULL,         -- последний учтённый playback_events.sequence
    updated_at            timestamptz NOT NULL
);

-- ============================================================ признаки треков и похожесть

CREATE TABLE track_stats (
    track_id         uuid PRIMARY KEY REFERENCES tracks (id) ON DELETE CASCADE,
    play_count       integer NOT NULL,
    skip_count       integer NOT NULL,
    skip_rate        double precision NOT NULL,
    popularity_score double precision NOT NULL,
    last_played_at   timestamptz,
    computed_at      timestamptz NOT NULL
);
CREATE INDEX ix_track_stats_popularity_score ON track_stats (popularity_score);

-- Всё, кроме loudness_db и dynamic_range_db, инвариантно к громкости: тихий мастер той же записи
-- должен попадать в ту же точку. algorithm_version двигают, чтобы воркер переанализировал библиотеку.
CREATE TABLE track_audio_features (
    track_id          uuid PRIMARY KEY REFERENCES tracks (id) ON DELETE CASCADE,
    tempo_bpm         double precision,
    tempo_confidence  double precision NOT NULL,
    energy            double precision NOT NULL,
    brightness        double precision NOT NULL,
    spectral_rolloff  double precision NOT NULL,
    timbre            double precision[] NOT NULL DEFAULT '{}',  -- мел-вектор тембра
    loudness_db       double precision NOT NULL,
    dynamic_range_db  double precision NOT NULL,
    key               integer,                     -- 0..11, C..B
    is_minor          boolean NOT NULL,
    key_strength      double precision NOT NULL,
    analyzed_seconds  double precision NOT NULL,
    algorithm_version integer NOT NULL,
    succeeded         boolean NOT NULL,
    error             varchar(512),
    analyzed_at       timestamptz NOT NULL
);
CREATE INDEX ix_track_audio_features_analyzed_at ON track_audio_features (analyzed_at);
CREATE INDEX ix_track_audio_features_succeeded_algorithm_version
    ON track_audio_features (succeeded, algorithm_version);

-- Пары треков; score — свёртка трёх составляющих, support — сколько наблюдений за коллаборативной.
CREATE TABLE track_similarity (
    track_id         uuid NOT NULL REFERENCES tracks (id) ON DELETE CASCADE,
    similar_track_id uuid NOT NULL REFERENCES tracks (id) ON DELETE CASCADE,
    score            double precision NOT NULL,
    content_score    double precision NOT NULL,
    collab_score     double precision NOT NULL,
    audio_score      double precision,
    support          integer NOT NULL,
    computed_at      timestamptz NOT NULL,
    PRIMARY KEY (track_id, similar_track_id)
);
CREATE INDEX ix_track_similarity_similar_track_id ON track_similarity (similar_track_id);
CREATE INDEX ix_track_similarity_track_id_score ON track_similarity (track_id, score);

-- Отпечаток входов трека: метаданные, состав, аудио, теги, прослушивания, членство в плейлистах.
-- Пересчитываются только изменившиеся треки и их пары. Популярность в отпечаток намеренно не входит.
CREATE TABLE track_similarity_state (
    track_id    uuid PRIMARY KEY REFERENCES tracks (id) ON DELETE CASCADE,
    fingerprint varchar(32) NOT NULL,
    computed_at timestamptz NOT NULL
);
CREATE INDEX ix_track_similarity_state_computed_at ON track_similarity_state (computed_at);

-- Теги (Last.fm): свободные метки с весом, отдельно от жёсткого справочника жанров.
CREATE TABLE track_tags (
    track_id uuid NOT NULL REFERENCES tracks (id) ON DELETE CASCADE,
    name     varchar(100) NOT NULL,
    weight   double precision NOT NULL,
    PRIMARY KEY (track_id, name)
);
CREATE INDEX ix_track_tags_name ON track_tags (name);

CREATE TABLE artist_tags (
    artist_id uuid NOT NULL REFERENCES artists (id) ON DELETE CASCADE,
    name      varchar(100) NOT NULL,
    weight    double precision NOT NULL,
    PRIMARY KEY (artist_id, name)
);
CREATE INDEX ix_artist_tags_name ON artist_tags (name);

-- ============================================================ выдача рекомендаций

-- Готовые полки. payload — сериализованная полка целиком (позиции, объяснения, id).
CREATE TABLE recommendation_cache (
    user_id      uuid NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    shelf_key    varchar(120) NOT NULL,           -- ключ полки, у дневных вариантов включает daypart
    position     integer NOT NULL,                -- порядок полки на главной
    payload      jsonb NOT NULL,
    run_id       uuid NOT NULL,
    generated_at timestamptz NOT NULL,
    expires_at   timestamptz NOT NULL,
    PRIMARY KEY (user_id, shelf_key)
);
CREATE INDEX ix_recommendation_cache_expires_at ON recommendation_cache (expires_at);
CREATE INDEX ix_recommendation_cache_user_id_position ON recommendation_cache (user_id, position);

-- Снимок «микса дня»: первый запрос в локальных сутках фиксирует 60 треков, остальные повторяют его.
CREATE TABLE daily_mixes (
    user_id      uuid NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    local_date   date NOT NULL,                   -- дата в таймзоне слушателя
    track_ids    jsonb NOT NULL,
    generated_at timestamptz NOT NULL,
    PRIMARY KEY (user_id, local_date)
);

CREATE TABLE recommendation_impressions (
    id         uuid PRIMARY KEY,
    user_id    uuid NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    track_id   uuid NOT NULL REFERENCES tracks (id) ON DELETE CASCADE,
    shelf_key  varchar(120) NOT NULL,
    position   integer NOT NULL,
    shown_at   timestamptz NOT NULL,
    clicked_at timestamptz
);
CREATE INDEX ix_recommendation_impressions_shown_at ON recommendation_impressions (shown_at);
CREATE INDEX ix_recommendation_impressions_track_id ON recommendation_impressions (track_id);
CREATE INDEX ix_recommendation_impressions_user_id_shelf_key_shown_at
    ON recommendation_impressions (user_id, shelf_key, shown_at);
CREATE INDEX ix_recommendation_impressions_user_id_track_id_shown_at
    ON recommendation_impressions (user_id, track_id, shown_at);

-- Явное «не интересно»: неявный дизлайк из пропусков всегда спорен, нужен прямой способ сказать.
CREATE TABLE recommendation_suppressions (
    id         uuid PRIMARY KEY,
    user_id    uuid NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    target     integer NOT NULL,                  -- 0 Track, 1 Artist
    target_id  uuid NOT NULL,
    created_at timestamptz NOT NULL,
    expires_at timestamptz                        -- NULL — навсегда
);
CREATE UNIQUE INDEX ix_recommendation_suppressions_user_id_target_target_id
    ON recommendation_suppressions (user_id, target, target_id);
CREATE INDEX ix_recommendation_suppressions_expires_at ON recommendation_suppressions (expires_at);

CREATE TABLE recommendation_runs (
    id              uuid PRIMARY KEY,
    user_id         uuid,                         -- NULL — общий (не пользовательский) прогон
    trigger         integer NOT NULL,             -- 0 Scheduled, 1 Activity, 2 OnDemand
    status          integer NOT NULL,             -- 0 Succeeded, 1 Failed
    candidate_count integer NOT NULL,
    shelf_count     integer NOT NULL,
    duration_ms     integer NOT NULL,
    error           varchar(2000),
    started_at      timestamptz NOT NULL
);
CREATE INDEX ix_recommendation_runs_started_at ON recommendation_runs (started_at);

-- ============================================================ интеграции

CREATE TABLE lastfm_accounts (
    user_id          uuid PRIMARY KEY REFERENCES users (id) ON DELETE CASCADE,
    username         varchar(100) NOT NULL,
    session_key      varchar(2000) NOT NULL,
    enabled          boolean NOT NULL,
    connected_at     timestamptz NOT NULL,
    last_scrobble_at timestamptz
);

-- Исходящая очередь с ретраями (скробблы). dedupe_key уникален: повтор не создаёт вторую задачу.
CREATE TABLE outbound_jobs (
    id              uuid PRIMARY KEY,
    user_id         uuid NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    kind            integer NOT NULL,             -- 1 LastfmNowPlaying, 2 LastfmScrobble
    dedupe_key      varchar(200) NOT NULL,
    state           integer NOT NULL,             -- 0 Pending, 1 Succeeded, 2 Failed
    payload         jsonb NOT NULL,
    attempts        integer NOT NULL,
    next_attempt_at timestamptz NOT NULL,
    last_error      varchar(500),
    created_at      timestamptz NOT NULL
);
CREATE UNIQUE INDEX ix_outbound_jobs_dedupe_key ON outbound_jobs (dedupe_key);
CREATE INDEX ix_outbound_jobs_user_id ON outbound_jobs (user_id);
CREATE INDEX ix_outbound_jobs_state_next_attempt_at ON outbound_jobs (state, next_attempt_at);

-- ============================================================ поиск

-- Ранг совпадения для сортировки поиска: точное → префикс → начало слова → подстрока → мимо.
-- В C# на неё маппится SearchRank.Of через HasDbFunction, чтобы вызывать прямо из LINQ.
CREATE OR REPLACE FUNCTION search_rank(value text, term text) RETURNS integer AS $$
    SELECT CASE
        WHEN value = term                              THEN 0
        WHEN starts_with(value, term)                  THEN 1
        WHEN position(' ' || term in ' ' || value) > 0 THEN 2
        WHEN position(term in value) > 0               THEN 3
        ELSE 4
    END;
$$ LANGUAGE sql IMMUTABLE STRICT PARALLEL SAFE;
