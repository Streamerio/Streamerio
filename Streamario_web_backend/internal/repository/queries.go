package repository

// --- Event Repository Queries ---
const (
	queryCreateEvent = `INSERT INTO events (room_id, viewer_id, triggered_at, metadata, skill1_count, skill2_count, skill3_count, enemy1_count, enemy2_count, enemy3_count) VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9,$10)`

	queryListEventViewerCounts = `
		SELECT 
			'skill1'::text AS event_type,
			e.viewer_id,
			v.name AS viewer_name,
			COALESCE(SUM(e.skill1_count), 0) AS count
		FROM events e
		LEFT JOIN viewers v ON v.id = e.viewer_id
		WHERE e.room_id = $1 AND e.viewer_id IS NOT NULL
		GROUP BY e.viewer_id, v.name
		HAVING COALESCE(SUM(e.skill1_count), 0) > 0
		UNION ALL
		SELECT 
			'skill2'::text AS event_type,
			e.viewer_id,
			v.name AS viewer_name,
			COALESCE(SUM(e.skill2_count), 0) AS count
		FROM events e
		LEFT JOIN viewers v ON v.id = e.viewer_id
		WHERE e.room_id = $1 AND e.viewer_id IS NOT NULL
		GROUP BY e.viewer_id, v.name
		HAVING COALESCE(SUM(e.skill2_count), 0) > 0
		UNION ALL
		SELECT 
			'skill3'::text AS event_type,
			e.viewer_id,
			v.name AS viewer_name,
			COALESCE(SUM(e.skill3_count), 0) AS count
		FROM events e
		LEFT JOIN viewers v ON v.id = e.viewer_id
		WHERE e.room_id = $1 AND e.viewer_id IS NOT NULL
		GROUP BY e.viewer_id, v.name
		HAVING COALESCE(SUM(e.skill3_count), 0) > 0
		UNION ALL
		SELECT 
			'enemy1'::text AS event_type,
			e.viewer_id,
			v.name AS viewer_name,
			COALESCE(SUM(e.enemy1_count), 0) AS count
		FROM events e
		LEFT JOIN viewers v ON v.id = e.viewer_id
		WHERE e.room_id = $1 AND e.viewer_id IS NOT NULL
		GROUP BY e.viewer_id, v.name
		HAVING COALESCE(SUM(e.enemy1_count), 0) > 0
		UNION ALL
		SELECT 
			'enemy2'::text AS event_type,
			e.viewer_id,
			v.name AS viewer_name,
			COALESCE(SUM(e.enemy2_count), 0) AS count
		FROM events e
		LEFT JOIN viewers v ON v.id = e.viewer_id
		WHERE e.room_id = $1 AND e.viewer_id IS NOT NULL
		GROUP BY e.viewer_id, v.name
		HAVING COALESCE(SUM(e.enemy2_count), 0) > 0
		UNION ALL
		SELECT 
			'enemy3'::text AS event_type,
			e.viewer_id,
			v.name AS viewer_name,
			COALESCE(SUM(e.enemy3_count), 0) AS count
		FROM events e
		LEFT JOIN viewers v ON v.id = e.viewer_id
		WHERE e.room_id = $1 AND e.viewer_id IS NOT NULL
		GROUP BY e.viewer_id, v.name
		HAVING COALESCE(SUM(e.enemy3_count), 0) > 0`

	queryListEventTotals = `
		SELECT 'skill1'::text AS event_type, COALESCE(SUM(skill1_count), 0)::int AS count
		FROM events
		WHERE room_id = $1
		UNION ALL
		SELECT 'skill2'::text AS event_type, COALESCE(SUM(skill2_count), 0)::int AS count
		FROM events
		WHERE room_id = $1
		UNION ALL
		SELECT 'skill3'::text AS event_type, COALESCE(SUM(skill3_count), 0)::int AS count
		FROM events
		WHERE room_id = $1
		UNION ALL
		SELECT 'enemy1'::text AS event_type, COALESCE(SUM(enemy1_count), 0)::int AS count
		FROM events
		WHERE room_id = $1
		UNION ALL
		SELECT 'enemy2'::text AS event_type, COALESCE(SUM(enemy2_count), 0)::int AS count
		FROM events
		WHERE room_id = $1
		UNION ALL
		SELECT 'enemy3'::text AS event_type, COALESCE(SUM(enemy3_count), 0)::int AS count
		FROM events
		WHERE room_id = $1`

	queryListViewerTotals = `
		SELECT 
			e.viewer_id,
			v.name AS viewer_name,
			COALESCE(SUM(e.skill1_count + e.skill2_count + e.skill3_count + e.enemy1_count + e.enemy2_count + e.enemy3_count), 0)::int AS count
		FROM events e
		LEFT JOIN viewers v ON v.id = e.viewer_id
		WHERE e.room_id = $1 AND e.viewer_id IS NOT NULL
		GROUP BY e.viewer_id, v.name
		HAVING COALESCE(SUM(e.skill1_count + e.skill2_count + e.skill3_count + e.enemy1_count + e.enemy2_count + e.enemy3_count), 0) > 0
		ORDER BY count DESC, e.viewer_id`

	queryListViewerEventCounts = `
		SELECT 'skill1'::text AS event_type, COALESCE(SUM(skill1_count), 0)::int AS count
		FROM events
		WHERE room_id = $1 AND viewer_id = $2
		UNION ALL
		SELECT 'skill2'::text AS event_type, COALESCE(SUM(skill2_count), 0)::int AS count
		FROM events
		WHERE room_id = $1 AND viewer_id = $2
		UNION ALL
		SELECT 'skill3'::text AS event_type, COALESCE(SUM(skill3_count), 0)::int AS count
		FROM events
		WHERE room_id = $1 AND viewer_id = $2
		UNION ALL
		SELECT 'enemy1'::text AS event_type, COALESCE(SUM(enemy1_count), 0)::int AS count
		FROM events
		WHERE room_id = $1 AND viewer_id = $2
		UNION ALL
		SELECT 'enemy2'::text AS event_type, COALESCE(SUM(enemy2_count), 0)::int AS count
		FROM events
		WHERE room_id = $1 AND viewer_id = $2
		UNION ALL
		SELECT 'enemy3'::text AS event_type, COALESCE(SUM(enemy3_count), 0)::int AS count
		FROM events
		WHERE room_id = $1 AND viewer_id = $2`
)

// --- Room Repository Queries ---
const (
	queryCreateRoom = `INSERT INTO rooms (id, streamer_id, created_at, expires_at, status, settings, ended_at)
		VALUES ($1,$2,$3,$4,$5,$6,$7)`

	queryGetRoom = `SELECT id, streamer_id, created_at, expires_at, status, settings, ended_at FROM rooms WHERE id=$1`

	queryUpdateRoom = `UPDATE rooms SET streamer_id=$1, created_at=$2, expires_at=$3, status=$4, settings=$5, ended_at=$6 WHERE id=$7`

	queryDeleteRoom = `DELETE FROM rooms WHERE id=$1`

	queryMarkEndedRoom = `UPDATE rooms SET status=$1, ended_at=$2 WHERE id=$3`

	queryMarkInGameRoom = `UPDATE rooms SET status=$1 WHERE id=$2`
)

// --- Viewer Repository Queries ---
const (
	queryCreateViewer = `INSERT INTO viewers (id, name, created_at, updated_at) VALUES ($1, $2, $3, $4)
        ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name, updated_at = EXCLUDED.updated_at`

	queryExistsViewer = `SELECT EXISTS(SELECT 1 FROM viewers WHERE id = $1)`

	queryGetViewer = `SELECT id, name, created_at, updated_at FROM viewers WHERE id = $1`
)
